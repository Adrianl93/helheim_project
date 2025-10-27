using UnityEngine;

public class Projectile : MonoBehaviour
{
    int damage;
    GameObject owner;
    Vector2 attackOrigin;
    [SerializeField] float lifetime = 1f;

    [Header("FX de Sonido")]
    [SerializeField] private AudioClip shootSFX;
    [SerializeField] private AudioClip impactSFX;
    private AudioSource audioSource;

    [Header("FX Visuales")]
    [SerializeField] private float fadeDuration = 0.8f;  // Duración del fade al final del lifetime
    [SerializeField] private float popScale = 1.8f;      // Escala para pop al impactar
    [SerializeField] private float fadeExponent = 3f;    // Exponente para fade

    private SpriteRenderer spriteRenderer; // Para controlar transparencia
    private Coroutine lifetimeCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Clonamos el material para que cada proyectil tenga fade independiente
        spriteRenderer.material = new Material(spriteRenderer.material);

        // Iniciamos la corutina de fade y destrucción por lifetime
        lifetimeCoroutine = StartCoroutine(FadeOutAndDestroy(lifetime));

        // Ignorar colisión con el dueño
        Collider2D projCol = GetComponent<Collider2D>();
        if (projCol != null && owner != null)
        {
            foreach (var c in owner.GetComponentsInChildren<Collider2D>())
                if (c != null)
                    Physics2D.IgnoreCollision(projCol, c);
        }

        PlaySFX(shootSFX);
    }

    public void SetDamage(int dmg) => damage = dmg;
    public void SetOwner(GameObject o) => owner = o;
    public void SetAttackOrigin(Vector2 origin) => attackOrigin = origin;

    private bool OwnerIsPlayer() => owner != null && owner.GetComponent<PlayerController>() != null;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == owner) return;

        // Evitamos daño al lanzador y a aliados
        bool sameLayer = owner != null && collision.gameObject.layer == owner.layer;

        if (OwnerIsPlayer())
        {
            if (!sameLayer)
            {
                EnemyController enemy = collision.GetComponent<EnemyController>();
                if (enemy != null)
                    enemy.TakeDamage(damage, attackOrigin);
            }
        }
        else
        {
            if (!sameLayer)
            {
                PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(damage);
            }
        }

        // Destruir el proyectil con pop y fade al impactar
        if (!collision.isTrigger)
        {
            PlaySFX(impactSFX);
            // Cancelamos solo el fade por lifetime de este proyectil
            if (lifetimeCoroutine != null)
                StopCoroutine(lifetimeCoroutine);

            StartCoroutine(PopAndFadeOnImpact());
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    // Coroutine para fadeout al final del lifetime
    private System.Collections.IEnumerator FadeOutAndDestroy(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (spriteRenderer == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(elapsed / fadeDuration, fadeExponent));
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    // Coroutine para pop y fade al impactar
    private System.Collections.IEnumerator PopAndFadeOnImpact()
    {
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;
        float popDuration = 0.1f;

        // Pop (escala rápida)
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * popScale, elapsed / popDuration);
            yield return null;
        }

        // Mini fade rápido perceptible
        elapsed = 0f;
        Color originalColor = spriteRenderer.color;
        float fadeTime = 0.05f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsed / fadeTime);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
