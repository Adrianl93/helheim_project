using UnityEngine;
using System;
using System.Collections;

public class SpawnerTrigger : MonoBehaviour
{
    [Header("Configuración de Activación")]
    [SerializeField] private float activationDelay = 1f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool destroyAfterActivation = true;

    public static event Action OnSpawnerTriggered;

    private bool hasActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasActivated) return;

        if (other.CompareTag(playerTag))
        {
            hasActivated = true;
            Debug.Log($"[SpawnerTrigger] Player entró al trigger. Activando en {activationDelay} segundos...");
            StartCoroutine(ActivateAfterDelay());
        }
    }

    private IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        Debug.Log("[SpawnerTrigger] Invocando evento OnSpawnerTriggered...");
        OnSpawnerTriggered?.Invoke();

        if (destroyAfterActivation)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
