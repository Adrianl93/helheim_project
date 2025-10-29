using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class MagicDoor : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 1.0f;

    [Header("Opciones")]
    [SerializeField] private bool disableColliderOnFadeOutStart = true;
    [Range(0f, 1f)][SerializeField] private float collisionEnableThreshold = 1.0f;

    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;
    private Rigidbody2D rb2d;
    [SerializeField] private Animator animator;

    private bool isActive = false;
    private bool isFading = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
            rb2d = gameObject.AddComponent<Rigidbody2D>();

        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.gravityScale = 0f;
        rb2d.constraints = RigidbodyConstraints2D.FreezeAll;

        // Empieza invisible y sin colisión
        doorCollider.enabled = false;
        rb2d.simulated = false;
        SetAlpha(0f);

        if (animator != null)
            animator.enabled = false;
    }

    private void OnEnable()
    {
        SpawnerTrigger.OnSpawnerTriggered += OnSpawnerTriggered;
        EnemySpawner.OnSpawningFinished += OnSpawningFinished;
    }

    private void OnDisable()
    {
        SpawnerTrigger.OnSpawnerTriggered -= OnSpawnerTriggered;
        EnemySpawner.OnSpawningFinished -= OnSpawningFinished;
    }

    private void OnSpawnerTriggered()
    {
        if (isActive || isFading) return;
        StartCoroutine(FadeInAndEnableCollision());
    }

    private void OnSpawningFinished()
    {
        if (!isActive || isFading) return;
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeInAndEnableCollision()
    {
        isFading = true;

        // Activamos colisiones antes de empezar el fade-in
        EnableCollision();
        isActive = true;

        float elapsed = 0f;
        //al tocar el trigger se activa la puerta con fade in
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Clamp01(elapsed / fadeInDuration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(1f);

        // Activamos animación idle cuando está totalmente activa
        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("Idle", 0, 0f);
        }

        isFading = false;
    }

    private void EnableCollision()
    {
        doorCollider.enabled = true;
        rb2d.simulated = true;
    }

    private IEnumerator FadeOutAndDestroy()
    {
        isFading = true;

        if (disableColliderOnFadeOutStart)
        {
            doorCollider.enabled = false;
            rb2d.simulated = false;
        }

        // Desactivamos animación para evitar interferencia visual durante fade-out
        if (animator != null)
            animator.enabled = false;

        float elapsed = 0f;
        float startAlpha = spriteRenderer.color.a;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(0f);
        Destroy(gameObject);
    }

    private void SetAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = Mathf.Clamp01(alpha);
        spriteRenderer.color = c;
    }
}
