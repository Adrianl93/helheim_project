using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Trap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damagePerSecond = 10;

    [Header("Enemy Damage Modifier")]
    [SerializeField] private float enemyDamageMultiplier = 0.5f;

    [Header("Self Destruction Settings")]
    [SerializeField] private bool selfDestruct = false;
    [SerializeField] private float destroyDelay = 5f;

    [Header("Fade Out")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Animación")]
    [SerializeField] private Animator animator;

    private SpriteRenderer spriteRenderer;

    // almacenamos las corrutinas por separado para que no se solapen
    private Dictionary<Collider2D, Coroutine> damageCoroutines = new Dictionary<Collider2D, Coroutine>();

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (selfDestruct)
        {
            //si selfDestruct es true, se inicia el timer de autodestruccion
            StartCoroutine(DestroyAfterDelay());
        }

        if (animator != null)
            animator.SetBool("Idle", true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // crear y almacenar coroutina de player
                Coroutine dmgCo = StartCoroutine(ApplyDamageOverTimePlayer(playerHealth));
                damageCoroutines[other] = dmgCo;
            }
        }

        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // crear y almacenar coroutina de enemigo (por separado)
                Coroutine dmgCo = StartCoroutine(ApplyDamageOverTimeEnemy(enemy));
                damageCoroutines[other] = dmgCo;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //si el objeto que entra al collider es el player, se inicia el daño por segundo
        //si el player sale del collider, se detiene el daño por segundo
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            //detener solo la coroutina de este objeto
            if (damageCoroutines.ContainsKey(other))
            {
                StopCoroutine(damageCoroutines[other]);
                damageCoroutines.Remove(other);
            }
        }
    }

    private IEnumerator ApplyDamageOverTimePlayer(PlayerHealth playerHealth)
    {
        //se aplica daño por segundo en el player
        while (true)
        {
            playerHealth.TakeDamage(damagePerSecond);
            yield return new WaitForSeconds(1f);
        }
    }

    //aplica daño por segundo al enemigo, pero con un reductor porcentual de cantidad de daño
    private IEnumerator ApplyDamageOverTimeEnemy(EnemyController enemy)
    {
        int enemyDamage = Mathf.RoundToInt(damagePerSecond * enemyDamageMultiplier);
        while (true)
        {
            enemy.TakeDamage(enemyDamage);
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        // se destruye la trampa despues de un delay
        yield return new WaitForSeconds(destroyDelay);

        // Detenemos TODAS las corrutinas antes del fade
        StopAllDamageCoroutines();

        //fade de desaparicion
        yield return StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float elapsed = 0f;
        Color initialColor = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // detenemos todas las corrutinas antes de destruir la trampa
        StopAllDamageCoroutines();

        Destroy(gameObject);
    }

    private void StopAllDamageCoroutines()
    {
        foreach (var co in damageCoroutines.Values)
        {
            if (co != null) StopCoroutine(co);
        }
        damageCoroutines.Clear();
    }
}
