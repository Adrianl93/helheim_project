using UnityEngine;

public class SpiralProjectile : MonoBehaviour
{
    private float currentAngle;        // �ngulo de la espiral
    private float radius;              // Radio de la espiral
    [SerializeField] private float speed = 2f;          // Velocidad de alejamiento (crecimiento del radio)
    [SerializeField] private float rotationSpeed = 180f; // Velocidad de giro de la espiral en grados por segundo
    [SerializeField] private float selfRotationSpeed = 90f; // Rotaci�n del sprite sobre su propio eje
    private int damage;
    private GameObject owner;
    private Vector2 origin;            // Punto de origen del disparo
    [SerializeField] private float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);

        // Evitamos da�o al lanzador
        Collider2D projCol = GetComponent<Collider2D>();
        if (projCol != null && owner != null)
        {
            foreach (var c in owner.GetComponentsInChildren<Collider2D>())
                if (c != null)
                    Physics2D.IgnoreCollision(projCol, c);
        }

        radius = 0f; // Radio inicial
    }

    void Update()
    {
        // Incrementamos el �ngulo y el radio
        currentAngle += rotationSpeed * Time.deltaTime;
        radius += speed * Time.deltaTime;

        // Calculamos posici�n usando coordenadas polares
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        transform.position = origin + offset;

        // Rotaci�n sobre su propio eje
        transform.Rotate(0f, 0f, selfRotationSpeed * Time.deltaTime, Space.Self);
    }

    // Inicializaci�n desde EnemyController
    public void Initialize(Vector2 direction, float projectileSpeed, float spiralRotation, int dmg, GameObject projOwner, float initialAngle)
    {
        origin = transform.position;
        currentAngle = initialAngle;
        speed = projectileSpeed;
        rotationSpeed = spiralRotation;
        damage = dmg;
        owner = projOwner;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Da�o solo al jugador
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Destruye al impactar con otros objetos s�lidos
        if (!collision.isTrigger)
            Destroy(gameObject);
    }
}
