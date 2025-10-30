using UnityEngine;
using System;
using System.Collections;

public class SpawnerTrigger : MonoBehaviour
{
    [Header("Configuración de Activación")]
    [SerializeField] private float activationDelay = 1f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool destroyAfterActivation = true;

    [Header("Grupo a activar (solo uno por trigger)")]
    [SerializeField] private int groupIDToTrigger = 1; // cada trigger activa exactamente este grupo

    [Header("Configuración de Puerta")]
    [SerializeField] private bool sendDoorEvent = true; // si quieres que se active MagicDoor o no

    public static event Action OnSpawnerTriggered;

    private bool hasActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasActivated) return;
        if (other.CompareTag(playerTag))
        {
            hasActivated = true;
            Debug.Log($"[SpawnerTrigger] Player entró al trigger. Activando grupo {groupIDToTrigger} en {activationDelay} s...");
            StartCoroutine(ActivateAfterDelay());
        }
    }

    private IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        Debug.Log($"[SpawnerTrigger] Activando grupo {groupIDToTrigger}");
        EnemySpawner.TriggerSpawnerGroup(groupIDToTrigger);

        if (sendDoorEvent)
        {
            Debug.Log("[SpawnerTrigger] Enviando evento de puerta (OnSpawnerTriggered)");
            OnSpawnerTriggered?.Invoke();
        }

        if (destroyAfterActivation)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
