using UnityEngine;

public class SpiralProjectile : MonoBehaviour
{
    [Header("Velocidades de espiral")]
    [SerializeField] private float speed = 2f;           // Velocidad de alejamiento 
    [SerializeField] private float rotationSpeed = 180f; // Velocidad de giro de la espiral sobre el origen (grados/segundo)
    [SerializeField] private float selfRotationSpeed = 90f; // Rotaci�n sobre su propio eje

    [SerializeField] private float lifetime = 5f;        // Tiempo de vida 
    [SerializeField] private float fadeDuration = 0.8f;  // Tiempo de fadeout antes de destruir, modificable en inspector
    [SerializeField] private float popScale = 2f;       // Escala m�xima para el "pop" al impactar
    [SerializeField] private float fadeExponent = 3f;   // Exponente para hacer el fade m�s pronunciado

    private float currentAngle;   // �ngulo actual 
    private float radius;         // Radio actual 
    private int damage;           // Da�o 
    private GameObject owner;     // Referencia al lanzador para evitar autoda�o y da�o a aliados
    private Vector2 origin;       // Posici�n inicial del proyectil (punto de origen de la espiral)

    private SpriteRenderer spriteRenderer; // Para controlar transparencia

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Clonamos el material para que cada proyectil tenga color y fade independiente
        spriteRenderer.material = new Material(spriteRenderer.material);

        // Iniciamos el fadeout autom�tico despu�s de lifetime
        StartCoroutine(FadeOutAndDestroy(lifetime));

        // evitamos da�o al owner (lanzador)
        Collider2D projCol = GetComponent<Collider2D>();
        if (projCol != null && owner != null)
        {
            foreach (var c in owner.GetComponentsInChildren<Collider2D>())
                if (c != null)
                    Physics2D.IgnoreCollision(projCol, c);
        }

        radius = 0f;
    }

    void Update()
    {
        // Incrementamos el �ngulo y el radio usando las velocidades definidas en el inspector
        currentAngle += rotationSpeed * Time.deltaTime;
        radius += speed * Time.deltaTime;

        // Calculamos la nueva posici�n usando coordenadas polares
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        transform.position = origin + offset;

        // Rotamos sobre su propio eje
        transform.Rotate(0f, 0f, selfRotationSpeed * Time.deltaTime, Space.Self);
    }

    // M�todo de inicializaci�n llamado desde EnemyController
    public void Initialize(Vector2 direction, int dmg, GameObject projOwner, Vector2 startPosition, float initialAngle)
    {
        origin = startPosition;   // Punto de origen
        currentAngle = initialAngle; // �ngulo inicial
        damage = dmg;             // Da�o 
        owner = projOwner;        // Referencia al lanzador
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Evitamos da�o al lanzador
        if (collision.gameObject == owner) return;

        // Evitamos fuego amigo
        if (owner != null && collision.gameObject.layer == owner.layer) return;

        // Aplicar da�o al jugador
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            StartCoroutine(PopAndFade());
            return;
        }

        // Aplicar da�o a un enemigo
        EnemyController enemy = collision.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, origin);
            StartCoroutine(PopAndFade());
            return;
        }

        // Destruir proyectil al chocar con cualquier otro objeto s�lido
        if (!collision.isTrigger)
            StartCoroutine(PopAndFade());
    }

    // Coroutine para fadeout cuando se acabe el lifetime
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

        // Hacemos que el fade sea m�s pronunciado usando un exponente
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(elapsed / fadeDuration, fadeExponent));
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    // Pop visible al impactar, sin interferir con el fade del lifetime
    private System.Collections.IEnumerator PopAndFade()
    {
        // Cancelamos el fade anterior para este proyectil �nicamente
        StopCoroutine(FadeOutAndDestroy(lifetime));

        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;
        float duration = 0.1f; // Duraci�n del pop, muy r�pido

        // Escalado hacia afuera para pop
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * popScale, elapsed / duration);
            yield return null;
        }

        // Mini fade r�pido despu�s del pop
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
