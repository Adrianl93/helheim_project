using UnityEngine;
using System.Collections;

public class RangedUnlockTrigger : MonoBehaviour
{
    // --- Configuración de Mensaje UI ---
    [Header("Configuración de Mensaje")]
    [SerializeField] private MostrarMensaje gestorMensaje;
    [SerializeField] private string mensajeAdvertencia = "¡Has obtenido el don del fuego a distancia!";
    [SerializeField] private float duracionMensaje = 5.0f; // Tiempo que dura el mensaje en pantalla

    // --- Configuración del Desbloqueo ---
    [Header("Configuración del Desbloqueo")]
    [SerializeField] private float delaySeconds = 1.0f; // Tiempo de espera EXTRA después del mensaje
    [SerializeField] private bool destroyAfterUnlock = true;

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            activated = true;

            // Desactivamos el collider para evitar re-activaciones
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;

            Debug.Log($"[RangedUnlockTrigger] Jugador entró. Mensaje y desbloqueo iniciado.");
            StartCoroutine(UnlockAfterDelay());
        }
    }

    private IEnumerator UnlockAfterDelay()
    {
        // 1. MOSTRAR EL MENSAJE
        if (gestorMensaje != null)
        {
            // Llama a la corrutina de tu script de mensaje
            gestorMensaje.Mostrar(mensajeAdvertencia, duracionMensaje);

            // Espera el tiempo completo del mensaje antes de continuar
            yield return new WaitForSeconds(duracionMensaje);
        }
        else
        {
            Debug.LogWarning("[RangedUnlockTrigger] GestorMensaje no asignado. Saltando mensaje UI.");
        }

        // 2. DELAY ADICIONAL (Opcional)
        // Puedes usar esto para un pequeño efecto o pausa después de que el mensaje desaparece.
        yield return new WaitForSeconds(delaySeconds);

        // 3. DESBLOQUEO DE LA HABILIDAD
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnlockRangedAttack();
            Debug.Log("[RangedUnlockTrigger] Ataque a distancia desbloqueado.");
        }
        else
        {
            Debug.LogWarning("[RangedUnlockTrigger] No se encontró instancia de GameManager.");
        }

        // 4. LIMPIEZA
        if (destroyAfterUnlock)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}