
using UnityEngine;

public class ObjectOrbit : MonoBehaviour
{
    [Tooltip("El objeto alrededor del cual se orbitará. Asigna tu personaje aquí.")]
    public Transform target;

    [Tooltip("El radio (distancia) de la órbita.")]
    public float orbitRadius = 2f;

    [Tooltip("La velocidad de la órbita en grados por segundo.")]
    public float orbitSpeed = 40f;

    [Tooltip("La altura de la órbita relativa al pivote del objetivo.")]
    public float orbitHeight = 1.5f;

    private float angle = 0f;

    void LateUpdate()
    {
        // Si no hay un objetivo asignado, no hacer nada para evitar errores.
        if (target == null)
        {
            return;
        }

        // Incrementar el ángulo basado en la velocidad y el tiempo.
        angle += orbitSpeed * Time.deltaTime;

        // Calcular la nueva posición en un círculo horizontal (plano XZ).
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * orbitRadius;
        float z = Mathf.Sin(angle * Mathf.Deg2Rad) * orbitRadius;

        // Construir la posición final.
        // Empezamos desde la posición del objetivo.
        Vector3 targetPosition = target.position;
        // Le sumamos la altura deseada.
        targetPosition.y += orbitHeight;
        // Le sumamos el desplazamiento orbital.
        transform.position = targetPosition + new Vector3(x, 0, z);
    }
}
