using UnityEngine;
using System.Collections;

public class PassiveRegen : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Regeneración pasiva")]
    [SerializeField] private float regenDelay = 5f;
    [SerializeField] private float regenRate = 1f;
    [SerializeField] private float regenTick = 0.5f;

    // time of last real damage
    private float lastDamageTime = -Mathf.Infinity;

    
    private Coroutine regenCoroutine = null;
    private bool isRegenerating = false;

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        lastDamageTime = Time.time;
    }

    private void Update()
    {
        if (playerHealth == null || playerHealth.IsDead) return;

        float sinceLast = Time.time - lastDamageTime;

        // si recibió daño hace poco corto la regeneración
        if (sinceLast < regenDelay)
        {
            StopRegen();
            return;
        }

        // si ya pasó el delay y no está regenerando inicio la regeneración
        if (!isRegenerating && playerHealth.CurrentHealth < playerHealth.MaxHealth)
        {
            StartRegen();
        }
    }

    // notificacion recibida desde PlayerHealth
    public void NotifyDamageTaken()
    {
        // actualizamos el tiempo de último daño y detenemos la regeneración
        lastDamageTime = Time.time;
        StopRegen();
    }

    private void StartRegen()
    {
        if (isRegenerating) return;

        // eliminamos corrutinas previas
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }

        regenCoroutine = StartCoroutine(RegenCoroutine());
        isRegenerating = true;
    }

    private void StopRegen()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }

        isRegenerating = false;
    }

    private IEnumerator RegenCoroutine()
    {
        // separador de un frame por seguridad
        yield return null;

        while (playerHealth.CurrentHealth < playerHealth.MaxHealth)
        {
            // snapshot del lastDamageTime 
            float snapshotDamageTime = lastDamageTime;

            // chequeo del snapshot
            if (Time.time - snapshotDamageTime < regenDelay)
            {
                // si hubo daño reciente, abortamos sin curar
                regenCoroutine = null;
                isRegenerating = false;
                yield break;
            }

            //segunda comprobacion del snapshot antes de curar
            if (!Mathf.Approximately(snapshotDamageTime, lastDamageTime))
            {
                regenCoroutine = null;
                isRegenerating = false;
                yield break;
            }

            // aplicar curación
            int healAmount = Mathf.RoundToInt(regenRate * regenTick);
            playerHealth.Heal(healAmount);

            // esperar el tick pero comprobando constantemente si llega daño
            float t = 0f;
            while (t < regenTick)
            {
                // si llega daño se cancela la regeneración
                if (Time.time - lastDamageTime < regenDelay)
                {
                    regenCoroutine = null;
                    isRegenerating = false;
                    yield break;
                }

                t += Time.deltaTime;
                yield return null;
            }
        }

        //si la vida esta llena detengo la regeneración
        regenCoroutine = null;
        isRegenerating = false;
    }
}
