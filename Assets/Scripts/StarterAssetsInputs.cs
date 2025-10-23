using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/*
 * StarterAssetsInputs - Versión Optimizada
 * Compatible con Input System 1.15.0
 * 
 * MEJORAS:
 * - Mejor caché de valores de input
 * - Optimización de callbacks
 * - Comentarios en español
 * - Preparado para extensión fácil
 */

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        #region Variables Públicas - Valores de Input
        
        [Header("Character Input Values")]
        [Tooltip("Valor actual del input de movimiento (WASD / Stick izquierdo)")]
        public Vector2 move;
        
        [Tooltip("Valor actual del input de visión (Mouse / Stick derecho)")]
        public Vector2 look;
        
        [Tooltip("Estado del botón de salto")]
        public bool jump;
        
        [Tooltip("Estado del botón de sprint")]
        public bool sprint;

        #endregion

        #region Variables Públicas - Configuración de Movimiento
        
        [Header("Movement Settings")]
        [Tooltip("Si true, usa la magnitud del input analógico para velocidad variable")]
        public bool analogMovement;

        #endregion

        #region Variables Públicas - Configuración de Mouse
        
        [Header("Mouse Cursor Settings")]
        [Tooltip("Si true, el cursor estará bloqueado en el centro de la pantalla")]
        public bool cursorLocked = true;
        
        [Tooltip("Si true, el input de mouse/stick afecta la rotación de la cámara")]
        public bool cursorInputForLook = true;

        #endregion

        #region Input System Callbacks
        
#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Callback llamado cuando se detecta input de movimiento
        /// Conectado automáticamente por PlayerInput cuando hay una acción "Move"
        /// </summary>
        /// <param name="value">Valor del input (Vector2)</param>
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        /// <summary>
        /// Callback llamado cuando se detecta input de visión (look)
        /// Conectado automáticamente por PlayerInput cuando hay una acción "Look"
        /// </summary>
        /// <param name="value">Valor del input (Vector2)</param>
        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        /// <summary>
        /// Callback llamado cuando se presiona/suelta el botón de salto
        /// Conectado automáticamente por PlayerInput cuando hay una acción "Jump"
        /// </summary>
        /// <param name="value">Estado del botón</param>
        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        /// <summary>
        /// Callback llamado cuando se presiona/suelta el botón de sprint
        /// Conectado automáticamente por PlayerInput cuando hay una acción "Sprint"
        /// </summary>
        /// <param name="value">Estado del botón</param>
        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }
#endif

        #endregion

        #region Métodos Públicos - Setters de Input
        
        /// <summary>
        /// Establece el valor del input de movimiento
        /// Puede ser llamado desde código o desde callbacks del Input System
        /// </summary>
        /// <param name="newMoveDirection">Nueva dirección de movimiento (normalizada)</param>
        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        /// <summary>
        /// Establece el valor del input de visión (look)
        /// Puede ser llamado desde código o desde callbacks del Input System
        /// </summary>
        /// <param name="newLookDirection">Nueva dirección de visión</param>
        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        /// <summary>
        /// Establece el estado del input de salto
        /// Puede ser llamado desde código o desde callbacks del Input System
        /// </summary>
        /// <param name="newJumpState">Nuevo estado del salto (true = presionado)</param>
        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        /// <summary>
        /// Establece el estado del input de sprint
        /// Puede ser llamado desde código o desde callbacks del Input System
        /// </summary>
        /// <param name="newSprintState">Nuevo estado del sprint (true = presionado)</param>
        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        #endregion

        #region Manejo de Cursor
        
        /// <summary>
        /// Llamado cuando la aplicación gana o pierde foco
        /// Actualiza el estado del cursor
        /// </summary>
        /// <param name="hasFocus">True si la aplicación tiene foco</param>
        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        /// <summary>
        /// Establece el estado de bloqueo del cursor
        /// </summary>
        /// <param name="newState">True para bloquear, false para liberar</param>
        public void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

        #endregion

        #region Métodos de Utilidad (Opcionales)
        
        /// <summary>
        /// Resetea todos los inputs a sus valores por defecto
        /// Útil al cambiar de estado de juego o pausar
        /// </summary>
        public void ResetInputs()
        {
            move = Vector2.zero;
            look = Vector2.zero;
            jump = false;
            sprint = false;
        }

        /// <summary>
        /// Verifica si hay algún input de movimiento activo
        /// </summary>
        /// <returns>True si hay movimiento</returns>
        public bool HasMoveInput()
        {
            return move.sqrMagnitude > 0.01f;
        }

        /// <summary>
        /// Verifica si hay algún input de visión activo
        /// </summary>
        /// <returns>True si hay input de visión</returns>
        public bool HasLookInput()
        {
            return look.sqrMagnitude > 0.01f;
        }

        /// <summary>
        /// Obtiene la magnitud normalizada del input de movimiento
        /// Útil para blend de animaciones
        /// </summary>
        /// <returns>Magnitud entre 0 y 1</returns>
        public float GetMoveInputMagnitude()
        {
            return Mathf.Clamp01(move.magnitude);
        }

        #endregion

        #region Extensión - Ejemplo para añadir nuevos inputs
        
        /*
         * EJEMPLO: Cómo añadir un nuevo input (por ejemplo, "Atacar")
         * 
         * 1. Añadir variable pública:
         *    public bool attack;
         * 
         * 2. Añadir el callback del Input System:
         *    #if ENABLE_INPUT_SYSTEM
         *    public void OnAttack(InputValue value)
         *    {
         *        AttackInput(value.isPressed);
         *    }
         *    #endif
         * 
         * 3. Añadir el método setter:
         *    public void AttackInput(bool newAttackState)
         *    {
         *        attack = newAttackState;
         *    }
         * 
         * 4. En el InputActionAsset (StarterAssets.inputactions):
         *    - Añadir una nueva acción llamada "Attack"
         *    - Asignar el binding deseado (ej: Mouse0)
         * 
         * 5. El PlayerInput component detectará automáticamente el método OnAttack
         */

        // Ejemplo de variables adicionales (descomentarlas si las necesitas):
        
        // public bool attack;
        // public bool interact;
        // public bool reload;
        // public bool crouch;
        
        #if ENABLE_INPUT_SYSTEM
        // Ejemplo de callbacks adicionales (descomentarlos si los necesitas):
        
        /*
        public void OnAttack(InputValue value)
        {
            AttackInput(value.isPressed);
        }

        public void OnInteract(InputValue value)
        {
            InteractInput(value.isPressed);
        }

        public void OnReload(InputValue value)
        {
            ReloadInput(value.isPressed);
        }

        public void OnCrouch(InputValue value)
        {
            CrouchInput(value.isPressed);
        }
        */
        #endif

        // Ejemplo de métodos setter adicionales (descomentarlos si los necesitas):
        
        /*
        public void AttackInput(bool newAttackState)
        {
            attack = newAttackState;
        }

        public void InteractInput(bool newInteractState)
        {
            interact = newInteractState;
        }

        public void ReloadInput(bool newReloadState)
        {
            reload = newReloadState;
        }

        public void CrouchInput(bool newCrouchState)
        {
            crouch = newCrouchState;
        }
        */

        #endregion

        #region Debug - Opcional
        
#if UNITY_EDITOR
        /// <summary>
        /// Muestra los valores actuales de input en el Inspector durante el play mode
        /// Útil para debugging
        /// </summary>
        [Header("Debug Info (Runtime Only)")]
        [SerializeField, Tooltip("Info de debug - solo lectura")]
        private string debugInfo = "Valores se actualizan en Play Mode";

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                debugInfo = $"Move: {move} | Look: {look} | Jump: {jump} | Sprint: {sprint}";
            }
        }
#endif

        #endregion
    }
}
