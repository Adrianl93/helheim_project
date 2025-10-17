using UnityEngine;

public class SpiralProjectile : MonoBehaviour
{
    private float currentAngle;      
    private float radius;            
    private float speed = 5f;       
    private float rotationSpeed = 10f; 
    private int damage;
    private GameObject owner;
    private Vector2 origin;          // Punto de origen del disparo
    [SerializeField] private float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);

        // evito daño al lanzador
        Collider2D projCol = GetComponent<Collider2D>();
        if (projCol != null && owner != null)
        {
            foreach (var c in owner.GetComponentsInChildren<Collider2D>())
                if (c != null)
                    Physics2D.IgnoreCollision(projCol, c);
        }

       //radio en 0 al iniciar
        radius = 0f;
    }

    void Update()
    {
        // Incremento ángulo para giro
        currentAngle += rotationSpeed * Time.deltaTime;

        // Incremento radio para alejamiento
        radius += speed * Time.deltaTime;

        // Calculo posición usando coordenadas polares
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        transform.position = origin + offset;
    }

    //se crea el prefab en enemycontroller
    public void Initialize(Vector2 direction, float projectileSpeed, float spiralRotation, int dmg, GameObject projOwner)
    {
        origin = transform.position; //guardo el punto de origen
        currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        speed = projectileSpeed;
        rotationSpeed = spiralRotation;
        damage = dmg;
        owner = projOwner;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == owner) return;

        if (owner != null && owner.GetComponent<PlayerController>() != null)
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Destroy(gameObject);
            }
        }

        if (!collision.isTrigger)
            Destroy(gameObject);
    }
}
