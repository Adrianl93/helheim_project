using UnityEngine;
using System.Collections;

public class RangedUnlockTrigger : MonoBehaviour
{
    [SerializeField] private float delaySeconds = 10f;
    [SerializeField] private bool destroyAfterUnlock = true;
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            activated = true;
            Debug.Log($"[RangedUnlockTrigger] Jugador entró al trigger. Desbloqueo en {delaySeconds} segundos...");
            StartCoroutine(UnlockAfterDelay());
        }
    }

    private IEnumerator UnlockAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnlockRangedAttack();
            Debug.Log("[RangedUnlockTrigger] Ataque a distancia desbloqueado.");
        }
        else
        {
            Debug.LogWarning("[RangedUnlockTrigger] No se encontró instancia de GameManager.");
        }

        if (destroyAfterUnlock)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
