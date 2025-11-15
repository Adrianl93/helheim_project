using UnityEngine;
using System.Collections; // Necesario para la corrutina

// Este script se coloca en el GameObject que tiene el Collider 2D (el trigger)
public class TriggerMensaje : MonoBehaviour
{
    [Tooltip("Arrastra aquí el GameObject que tiene el script MostrarMensaje.")]
    public MostrarMensaje gestorMensaje;

    [Header("Configuración del Mensaje")]
    public string mensajeAMostrar = "Bienvenido a esta área especial.";
    public float duracionDelMensaje = 3.0f; // Duración en segundos

    [Header("Control de Activación")]
    [Tooltip("Si es verdadero, el trigger se destruirá después de activarse.")]
    public bool destruirAlActivar = true;

    private bool hasActivated = false; // Bandera para asegurar que solo se activa una vez

    // --- Usamos OnTriggerEnter2D para juegos 2D ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Evitar activaciones repetidas
        if (hasActivated) return;

        // 2. Verificar si el objeto que entró es el jugador
        if (other.CompareTag("Player"))
        {
            hasActivated = true; // Marcamos como activado

            // Desactivamos el collider inmediatamente para evitar más llamadas
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Debug.Log("¡El jugador entró al trigger! Llamando a MostrarMensaje.");

            // 3. Comprobar que el gestor esté asignado y llamar
            if (gestorMensaje != null)
            {
                gestorMensaje.Mostrar(mensajeAMostrar, duracionDelMensaje);

                // 4. Iniciar Corrutina para la Autodestrucción
                // Esperamos la duración del mensaje antes de destruir el trigger
                StartCoroutine(CleanupAfterDelay(duracionDelMensaje));
            }
            else
            {
                Debug.LogError("El Gestor de Mensajes no está asignado en el Inspector. El trigger no puede funcionar.");
                // Si el gestor no está, aún podemos limpiarnos para no molestar
                if (destruirAlActivar)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    // --- Corrutina de Limpieza y Destrucción ---
    IEnumerator CleanupAfterDelay(float delay)
    {
        // Esperamos exactamente la duración del mensaje.
        yield return new WaitForSeconds(delay);

        if (destruirAlActivar)
        {
            Debug.Log($"Trigger destruido después de {delay} segundos.");
            Destroy(gameObject);
        }
        else
        {
            // Si no se debe destruir, solo se desactiva
            Debug.Log("Trigger desactivado.");
            gameObject.SetActive(false);
        }
    }
}