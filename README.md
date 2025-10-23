# 🎮 Guía Actualización - Cinemachine 3.1.4 + Input System 1.15.0

## ⚡ Instalación Rápida (5 minutos)

### 1. Actualizar Paquetes
```
Window → Package Manager
- Cinemachine → 3.1.4+
- Input System → 1.15.0+
```

### 2. Reemplazar Scripts
```
Renombrar viejos:
ThirdPersonController.cs → ThirdPersonController_OLD.cs
StarterAssetsInputs.cs → StarterAssetsInputs_OLD.cs

Renombrar nuevos:
ThirdPersonController_Updated.cs → ThirdPersonController.cs
StarterAssetsInputs_Updated.cs → StarterAssetsInputs.cs
```

### 3. Actualizar Cámara Virtual
Si tienes **CinemachineVirtualCamera**:
```
1. Selecciona Virtual Camera
2. Clic "Upgrade" (si aparece)
3. O Manual:
   - Delete CinemachineVirtualCamera
   - Add → CinemachineCamera
   - Add → CinemachineFollow (Mode: Third Person)
   - Add → CinemachineRotationComposer
   - Asignar Tracking + Look At: PlayerCameraRoot
```

### 4. Verificar Main Camera
```
Main Camera debe tener: CinemachineBrain
```

### 5. Testear
```
✓ WASD = moverse
✓ Mouse = cámara
✓ Space = saltar
✓ Shift = sprint
```

---

## 🎬 Configuración Cinemachine 3

### CinemachineCamera
```
Lens Field of View: 40-60
Tracking Target: PlayerCameraRoot
Look At Target: PlayerCameraRoot
```

### CinemachineFollow
```
Mode: Third Person
Distance: 4.0
Side: 1.0
Height: 2.0
Damping: 0.1, 0.5, 0.1
```

### CinemachineRotationComposer
```
Screen: 0.5, 0.5
Damping: 0.5, 0.5
```

---

## 🎨 Modificar Animaciones

### Parámetros del Animator

| Parámetro | Tipo | Uso |
|-----------|------|-----|
| Speed | Float | 0=Idle, 2=Walk, 5.335=Run |
| Grounded | Bool | true=suelo |
| Jump | Bool | Trigger salto |
| FreeFall | Bool | Trigger caída |
| MotionSpeed | Float | 0-1 blend |

### Método Rápido: Reemplazar Clip
```
1. Animator Controller → Estado (Idle/Walk/Run)
2. Motion → Arrastra nuevo clip
3. Done!
```

### Añadir Animación (Ejemplo: Ataque)

**Animator:**
```
1. Parameter → "Attack" (Trigger)
2. State → "Attack"
3. Transition: Any State → Attack
```

**Script:**
```csharp
// Declarar
private int _animIDAttack;

// Asignar en AssignAnimationIDs()
_animIDAttack = Animator.StringToHash("Attack");

// Usar
if (Input.GetKeyDown(KeyCode.Mouse0)) {
    _animator.SetTrigger(_animIDAttack);
}
```

**Usar ejemplo completo:** `ThirdPersonAnimationExtension.cs`

### Ajustar Velocidades
```csharp
MoveSpeed = 2.0f;        // Caminar
SprintSpeed = 5.335f;    // Correr
SpeedChangeRate = 10.0f; // Suavidad
```

---

## 🐛 Solución de Problemas

### Error: Cinemachine namespace
```
Actualizar Package Manager → Cinemachine 3.1.4+
```

### Cámara no sigue
```
1. CinemachineCamera → Tracking Target asignado
2. Main Camera → CinemachineBrain existe
3. ThirdPersonController → CinemachineCameraTarget asignado
```

### Animaciones no funcionan
```
1. Animator Controller asignado
2. Parámetros con nombres exactos
3. _hasAnimator = true en runtime
```

### Input no responde
```
1. PlayerInput component presente
2. StarterAssets.inputactions asignado
3. Input System activo en Project Settings
```

### Movimiento no suave
```csharp
RotationSmoothTime = 0.12f;  // ↓=más rápido
SpeedChangeRate = 10.0f;     // ↑=más rápido
```

---

## 📝 Archivos Incluidos

| Archivo | Descripción |
|---------|-------------|
| **ThirdPersonController_Updated.cs** | Controller actualizado |
| **StarterAssetsInputs_Updated.cs** | Input optimizado |
| **ThirdPersonAnimationExtension.cs** | Ejemplo animaciones extras |
| **GUIA_ACTUALIZACION_CINEMACHINE_3.md** | Esta guía |

---

## 💡 Tips

**Ajustes rápidos:**
- Más rápido: `MoveSpeed = 3.0f, SprintSpeed = 6.0f`
- Rotación suave: `RotationSmoothTime = 0.2f`
- Saltos altos: `JumpHeight = 2.0f, Gravity = -12.0f`

**Teclas Unity:**
- `F` = Enfocar objeto
- `Ctrl+D` = Duplicar
- `Ctrl+Shift+F` = Alinear con vista

---

**¡Listo para usar!** 🎉
