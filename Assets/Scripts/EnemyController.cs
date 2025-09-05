using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType
    {
        Melee,
        Ranged,
        Boss
    }

    [SerializeField] private EnemyType enemyType = EnemyType.Melee;

    [SerializeField] private Transform player;
    [SerializeField] private int health = 50;
    private int maxHealth;
    [SerializeField] private int armor = 2;
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private int rangedDamage = 8;
    [SerializeField] private float speed = 2f;

    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float meleeRadius = 1.5f;
    [SerializeField] private float meleeCooldown = 1.5f;
    [SerializeField] private float rangedCooldown = 2f;
    [SerializeField] private float rangedAttackRange = 8f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;

    [SerializeField] private float enragedDuration = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float lastMeleeAttackTime = -1f;
    private float lastRangedAttackTime = 0f;

    private float originalDetectionRadius;
    private float enragedTimer = 0f;

    public int Armor => armor;
    public int CurrentHealth => health;
    private EnemyType currentAttackMode;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalDetectionRadius = detectionRadius;
        maxHealth = health;

        if (enemyType == EnemyType.Boss)
            currentAttackMode = EnemyType.Melee;
        else
            currentAttackMode = enemyType;
    }

    void Update()
    {
        if (player == null) return;

        HandleEnragedState();
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (enemyType == EnemyType.Melee || (enemyType == EnemyType.Boss && currentAttackMode == EnemyType.Melee))
        {
            if (distanceToPlayer <= meleeRadius)
            {
                movement = Vector2.zero;
                TryMeleeAttack();
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
        else if (enemyType == EnemyType.Ranged || (enemyType == EnemyType.Boss && currentAttackMode == EnemyType.Ranged))
        {
            if (distanceToPlayer < rangedAttackRange * 0.9f)
                movement = (transform.position - player.position).normalized;
            else if (distanceToPlayer > rangedAttackRange)
                movement = (player.position - transform.position).normalized;
            else
                movement = Vector2.zero;

            if (distanceToPlayer <= rangedAttackRange)
                TryRangedAttack();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    private void HandleEnragedState()
    {
        if (enragedTimer > 0)
        {
            enragedTimer -= Time.deltaTime;
            if (enragedTimer <= 0f)
                detectionRadius = originalDetectionRadius;
        }
    }

    private void TryMeleeAttack()
    {
        if (player == null) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && Time.time >= lastMeleeAttackTime + meleeCooldown)
        {
            lastMeleeAttackTime = Time.time;
            playerHealth.TakeDamage(meleeDamage);
            Debug.Log($"{name} atacó al Player con {meleeDamage} de daño (melee). Vida restante: {playerHealth.CurrentHealth}");
        }
    }


    private void TryRangedAttack()
    {
        if (projectilePrefab == null) return;

        if (Time.time >= lastRangedAttackTime + rangedCooldown)
        {
            lastRangedAttackTime = Time.time;

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
            if (prb != null)
            {
                Vector2 dir = (player.position - firePoint.position).normalized;
                prb.linearVelocity = dir * projectileSpeed;
            }

            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
                projScript.SetDamage(rangedDamage);

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                Debug.Log($"{name} disparó un proyectil al Player con {rangedDamage} de daño. Vida restante del Player: {playerHealth.CurrentHealth}");
        }
    }

    public void TakeDamage(int incomingDamage)
    {
        int finalDamage = Mathf.Max(incomingDamage - armor, 0);
        health -= finalDamage;

        detectionRadius = originalDetectionRadius * 4f;
        enragedTimer = enragedDuration;

        if (enemyType == EnemyType.Boss)
        {
            float healthPercent = (float)health / maxHealth;
            if (healthPercent <= 0.75f && healthPercent > 0.5f && currentAttackMode != EnemyType.Ranged)
                currentAttackMode = EnemyType.Ranged;
            else if (healthPercent <= 0.5f && healthPercent > 0.25f && currentAttackMode != EnemyType.Melee)
                currentAttackMode = EnemyType.Melee;
            else if (healthPercent <= 0.25f && currentAttackMode != EnemyType.Ranged)
                currentAttackMode = EnemyType.Ranged;
        }

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangedAttackRange);
    }
}
