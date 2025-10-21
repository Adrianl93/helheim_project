using UnityEngine;
using System.Collections;

public class Trap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damagePerSecond = 10;

    [Header("Self Destruction Settings")]
    [SerializeField] private bool selfDestruct = false;
    [SerializeField] private float destroyDelay = 5f;

    private Coroutine damageCoroutine;

    private void Start()
    {
        if (selfDestruct)
        {
            //si selfDestruct es true, se inicia el timer de autodestruccion
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                damageCoroutine = StartCoroutine(ApplyDamageOverTime(playerHealth));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //si el objeto que entra al collider es el player, se inicia el daño por segundo
        //si el player sale del collider, se detiene el daño por segundo
        if (other.CompareTag("Player"))
        {
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    private IEnumerator ApplyDamageOverTime(PlayerHealth playerHealth)
    {
        //se aplica daño por segundo en el player
        while (true)
        {
            playerHealth.TakeDamage(damagePerSecond);
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator DestroyAfterDelay()
    {

        // se destruye la trampa despues de un delay
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
