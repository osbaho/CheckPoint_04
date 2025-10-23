using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* 
 * ThirdPersonController - Versión Actualizada y Optimizada
 * Compatible con Cinemachine 3.1.4 e Input System 1.15.0
 * 
 * CAMBIOS PRINCIPALES:
 * - Compatible con Cinemachine 3.x (namespace Unity.Cinemachine)
 * - Optimización del manejo de Input System
 * - Mejor organización del código de animaciones
 * - Comentarios en español para facilitar modificaciones
 * - Sistema de animación más modular y extensible
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        #region Variables Públicas - Movimiento
        
        [Header("Player Movement")]
        [Tooltip("Velocidad de movimiento del personaje en m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Velocidad al correr en m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("Qué tan rápido el personaje gira hacia la dirección de movimiento")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Aceleración y desaceleración del movimiento")]
        public float SpeedChangeRate = 10.0f;

        #endregion

        #region Variables Públicas - Audio
        
        [Header("Audio")]
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        #endregion

        #region Variables Públicas - Salto y Gravedad
        
        [Header("Jump & Gravity")]
        [Tooltip("Altura del salto")]
        public float JumpHeight = 1.2f;

        [Tooltip("Valor de gravedad personalizado. El default de Unity es -9.81f")]
        public float Gravity = -15.0f;

        [Tooltip("Tiempo de espera antes de poder saltar de nuevo")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Tiempo antes de entrar en estado de caída (útil para escaleras)")]
        public float FallTimeout = 0.15f;

        #endregion

        #region Variables Públicas - Grounded Check
        
        [Header("Player Grounded")]
        [Tooltip("Si el personaje está en el suelo o no")]
        public bool Grounded = true;

        [Tooltip("Offset para terreno irregular")]
        public float GroundedOffset = -0.14f;

        [Tooltip("Radio del chequeo de suelo (debe coincidir con el CharacterController)")]
        public float GroundedRadius = 0.28f;

        [Tooltip("Capas que el personaje considera como suelo")]
        public LayerMask GroundLayers;

        #endregion

        #region Variables Públicas - Cámara (Cinemachine 3.x Compatible)
        
        [Header("Cinemachine Camera Settings")]
        [Tooltip("Transform que la cámara Cinemachine seguirá (Tracking Target)")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("Límite superior de rotación de cámara en grados")]
        public float TopClamp = 70.0f;

        [Tooltip("Límite inferior de rotación de cámara en grados")]
        public float BottomClamp = -30.0f;

        [Tooltip("Grados adicionales para ajustar la posición de la cámara")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("Bloquear la posición de la cámara en todos los ejes")]
        public bool LockCameraPosition = false;

        #endregion

        #region Variables Privadas - Estado de Cámara
        
        // Estado de rotación de cámara Cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        #endregion

        #region Variables Privadas - Estado del Jugador
        
        // Estado de movimiento del jugador
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        #endregion

        #region Variables Privadas - Timeouts
        
        // Deltas de tiempo para saltos y caídas
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        #endregion

        #region Variables Privadas - IDs de Animación
        
        // IMPORTANTE: IDs de parámetros del Animator
        // Para añadir nuevas animaciones, declara aquí los IDs y asígnalos en AssignAnimationIDs()
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        // EJEMPLO: Añadir nuevos parámetros de animación
        // private int _animIDAttack;
        // private int _animIDRoll;
        // private int _animIDDeath;

        #endregion

        #region Variables Privadas - Componentes
        
#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        #endregion

        #region Constantes y Flags
        
        private const float _threshold = 0.01f;
        private bool _hasAnimator;

        #endregion

        #region Propiedades
        
        /// <summary>
        /// Verifica si el dispositivo actual es mouse (para ajustar sensibilidad)
        /// </summary>
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        #endregion

        #region Métodos Unity - Ciclo de Vida

        private void Awake()
        {
            // Obtener referencia a la cámara principal
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            // Inicializar rotación de cámara
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            // Cachear componentes
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets requiere Input System. Por favor instala las dependencias.");
#endif

            // Asignar IDs de parámetros del Animator
            AssignAnimationIDs();

            // Resetear timeouts
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            // Verificar si tenemos Animator (puede cambiar en runtime)
            _hasAnimator = TryGetComponent(out _animator);

            // Actualizar sistemas principales
            JumpAndGravity();
            GroundedCheck();
            Move();
            
            // NOTA: Aquí puedes añadir llamadas a nuevos sistemas de gameplay
            // Ejemplo: HandleCombat(); HandleInteractions(); etc.
        }

        private void LateUpdate()
        {
            // La rotación de cámara se hace en LateUpdate para evitar jittering
            CameraRotation();
        }

        #endregion

        #region Sistema de Animación
        
        /// <summary>
        /// Asigna los IDs de los parámetros del Animator usando StringToHash para optimización
        /// MODIFICAR AQUÍ: Para añadir nuevas animaciones, añade nuevos IDs
        /// </summary>
        private void AssignAnimationIDs()
        {
            // Parámetros base del sistema
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

            // EJEMPLO: Añadir nuevos parámetros personalizados
            // _animIDAttack = Animator.StringToHash("Attack");
            // _animIDRoll = Animator.StringToHash("Roll");
            // _animIDDeath = Animator.StringToHash("Death");
        }

        /// <summary>
        /// Actualiza el parámetro Speed del Animator
        /// </summary>
        private void UpdateAnimatorSpeed(float speed)
        {
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, speed);
            }
        }

        /// <summary>
        /// Actualiza el parámetro MotionSpeed del Animator
        /// </summary>
        private void UpdateAnimatorMotionSpeed(float motionSpeed)
        {
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDMotionSpeed, motionSpeed);
            }
        }

        /// <summary>
        /// Actualiza el parámetro Grounded del Animator
        /// </summary>
        private void UpdateAnimatorGrounded(bool grounded)
        {
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, grounded);
            }
        }

        /// <summary>
        /// Activa/desactiva el parámetro Jump del Animator
        /// </summary>
        private void SetAnimatorJump(bool jump)
        {
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, jump);
            }
        }

        /// <summary>
        /// Activa/desactiva el parámetro FreeFall del Animator
        /// </summary>
        private void SetAnimatorFreeFall(bool freeFall)
        {
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDFreeFall, freeFall);
            }
        }

        // EJEMPLO: Añadir métodos para nuevas animaciones
        /*
        private void TriggerAttackAnimation()
        {
            if (_hasAnimator)
            {
                _animator.SetTrigger(_animIDAttack);
            }
        }

        private void TriggerRollAnimation()
        {
            if (_hasAnimator)
            {
                _animator.SetTrigger(_animIDRoll);
            }
        }
        */

        #endregion

        #region Sistema de Detección de Suelo
        
        /// <summary>
        /// Verifica si el personaje está en el suelo usando un CheckSphere
        /// </summary>
        private void GroundedCheck()
        {
            // Calcular posición de la esfera con offset
            Vector3 spherePosition = new Vector3(
                transform.position.x, 
                transform.position.y - GroundedOffset,
                transform.position.z
            );
            
            // Realizar el check de colisión con el suelo
            Grounded = Physics.CheckSphere(
                spherePosition, 
                GroundedRadius, 
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            // Actualizar animación
            UpdateAnimatorGrounded(Grounded);
        }

        #endregion

        #region Sistema de Rotación de Cámara (Cinemachine 3.x Compatible)
        
        /// <summary>
        /// Maneja la rotación de la cámara basada en input del jugador
        /// Compatible con Cinemachine 3.x
        /// </summary>
        private void CameraRotation()
        {
            // Solo rotar si hay input y la cámara no está bloqueada
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                // No multiplicar input de mouse por Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // Limitar rotaciones a 360 grados
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Aplicar rotación al target que Cinemachine seguirá
            // NOTA: Cinemachine 3.x usa el mismo sistema de tracking
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 
                0.0f
            );
        }

        /// <summary>
        /// Clampea un ángulo entre un mínimo y máximo
        /// </summary>
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        #endregion

        #region Sistema de Movimiento
        
        /// <summary>
        /// Maneja el movimiento del personaje
        /// </summary>
        private void Move()
        {
            // Determinar velocidad objetivo (normal o sprint)
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // Si no hay input, velocidad objetivo es 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // Velocidad horizontal actual
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // Acelerar o decelerar suavemente hacia la velocidad objetivo
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // Interpolación suave usando Lerp
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed, 
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate
                );

                // Redondear a 3 decimales
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // Actualizar blend de animación suavemente
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // Normalizar dirección de input
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // Rotar el personaje hacia la dirección de movimiento
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                                  
                float rotation = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y, 
                    _targetRotation, 
                    ref _rotationVelocity,
                    RotationSmoothTime
                );

                // Aplicar rotación
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            // Calcular dirección objetivo
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // Mover el personaje
            _controller.Move(
                targetDirection.normalized * (_speed * Time.deltaTime) +
                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
            );

            // Actualizar animaciones
            UpdateAnimatorSpeed(_animationBlend);
            UpdateAnimatorMotionSpeed(inputMagnitude);
        }

        #endregion

        #region Sistema de Salto y Gravedad
        
        /// <summary>
        /// Maneja el salto y la aplicación de gravedad
        /// </summary>
        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // Resetear timeout de caída
                _fallTimeoutDelta = FallTimeout;

                // Resetear animaciones de aire
                SetAnimatorJump(false);
                SetAnimatorFreeFall(false);

                // Evitar que la velocidad vertical siga bajando indefinidamente
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Manejar salto
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // Calcular velocidad necesaria: sqrt(H * -2 * G)
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // Activar animación de salto
                    SetAnimatorJump(true);
                }

                // Manejar timeout de salto
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // Resetear timeout de salto
                _jumpTimeoutDelta = JumpTimeout;

                // Manejar timeout de caída
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // Activar animación de caída libre
                    SetAnimatorFreeFall(true);
                }

                // No permitir saltar en el aire
                _input.jump = false;
            }

            // Aplicar gravedad
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        #endregion

        #region Sistema de Audio (Animation Events)
        
        /// <summary>
        /// Llamado por Animation Event para reproducir sonidos de pasos
        /// </summary>
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(
                        FootstepAudioClips[index], 
                        transform.TransformPoint(_controller.center), 
                        FootstepAudioVolume
                    );
                }
            }
        }

        /// <summary>
        /// Llamado por Animation Event para reproducir sonido de aterrizaje
        /// </summary>
        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(
                    LandingAudioClip, 
                    transform.TransformPoint(_controller.center), 
                    FootstepAudioVolume
                );
            }
        }

        #endregion

        #region Gizmos para Debug
        
        /// <summary>
        /// Dibuja gizmos en la escena para visualizar el GroundedCheck
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;

            // Dibujar esfera del grounded check
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius
            );
        }

        #endregion
    }
}
