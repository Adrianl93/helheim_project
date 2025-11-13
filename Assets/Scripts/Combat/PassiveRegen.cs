using UnityEngine;
using System.Collections;

public class PassiveRegen : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Regeneración pasiva")]
    [SerializeField] private float regenDelay = 5f;   // tiempo en el que se considera que dejó de combatir
    [SerializeField] private float regenRate = 1f;    // vida recuperada por segundo
    [SerializeField] private float regenTick = 0.5f;  // intervalo entre curaciones

    private float lastDamageTime;
    private bool isRegenerating = false;
    private Coroutine regenCoroutine; // referencia a la corrutina activa

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (playerHealth == null || playerHealth.IsDead) return;

        // al recibir daño se frena la curacion y se espera el delay para volver a curar
        if (Time.time - lastDamageTime < regenDelay)
        {
            if (isRegenerating)
            {
                if (regenCoroutine != null)
                {
                    StopCoroutine(regenCoroutine);
                    regenCoroutine = null;
                }
                isRegenerating = false;
            }
            return;
        }

        //al pasar el delay, iniciar la curacion 
        if (!isRegenerating && playerHealth.CurrentHealth < playerHealth.MaxHealth)
        {
            regenCoroutine = StartCoroutine(RegenCoroutine());
        }
    }

    public void NotifyDamageTaken()
    {
        //actualizamos el tiempo desde el ultimo daño
        lastDamageTime = Time.time;
    }

    //corrutina para el delay entre curaciones
    private IEnumerator RegenCoroutine()
    {
        isRegenerating = true;

        while (playerHealth.CurrentHealth < playerHealth.MaxHealth)
        {
            playerHealth.Heal(Mathf.RoundToInt(regenRate * regenTick));
            yield return new WaitForSeconds(regenTick);

            // detener si se recibe daño durante la regeneración
            if (Time.time - lastDamageTime < regenDelay)
            {
                isRegenerating = false;
                regenCoroutine = null;
                yield break;
            }
        }

        isRegenerating = false;
        regenCoroutine = null;
    }
}
