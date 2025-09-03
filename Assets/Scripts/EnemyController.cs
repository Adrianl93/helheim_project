using UnityEngine;

public class EnemyController : MonoBehaviour
{
  
    [SerializeField] private Transform player;

   
    [SerializeField] private int health = 50;
    [SerializeField] private int armor = 2;
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 2f;

    
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private float attackCooldown = 1.5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float lastAttackTime = 0f;

    public int Armor => armor;
    public int CurrentHealth => health;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRadius)
        {
            movement = Vector2.zero;
            TryAttack();
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            movement = (player.position - transform.position).normalized;
      
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    private void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                
            }
        }
    }

    
    public void TakeDamage(int incomingDamage)
    {
        int finalDamage = Mathf.Max(incomingDamage - armor, 0);
        health -= finalDamage;

        Debug.Log($"Enemigo recibio {finalDamage} de daño (HP restante: {health})");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemigo murio");
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
