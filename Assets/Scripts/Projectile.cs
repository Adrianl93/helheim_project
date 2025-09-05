using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int damage;
    private Vector2 startPos;
    private Rigidbody2D rb;

    [SerializeField] private float maxDistance = 8f; 
    [SerializeField] private LayerMask enemyLayer;

    private void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Proyectil impactó al enemigo. Daño: " + damage + " | Vida restante: " + enemy.CurrentHealth);
            }

            Destroy(gameObject);
        }
    }
}
