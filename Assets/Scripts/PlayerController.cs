using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed;

    [Header("Ataque Melee")]
    [SerializeField] private float meleeAttackRadius = 1.5f;
    [SerializeField] private int meleeAttackDamage = 15;
    [SerializeField] private float meleeAttackCooldown = 1f;

    [Header("Ataque Ranged")]
    [SerializeField] private int rangedAttackDamage = 10;
    [SerializeField] private float rangedAttackCooldown = 1.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private Vector3 movement;
    private Vector2 lastMoveDir = Vector2.right;
    private float lastMeleeAttackTime = 0f;
    private float lastRangedAttackTime = 0f;

    private void Update()
    {
        // Movimiento
        movement = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0).normalized;

        if (movement != Vector3.zero)
        {
            lastMoveDir = movement;
        }

        // Ataque melee con Ctrl
        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            && Time.time >= lastMeleeAttackTime + meleeAttackCooldown)
        {
            lastMeleeAttackTime = Time.time;
            MeleeAttack();
        }

        // Ataque ranged con Alt
        if ((Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            && Time.time >= lastRangedAttackTime + rangedAttackCooldown)
        {
            lastRangedAttackTime = Time.time;
            RangedAttack();
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + (movement * speed * Time.fixedDeltaTime));
    }

    private void MeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, meleeAttackRadius, enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyController enemy = enemyCollider.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(meleeAttackDamage);
                Debug.Log("Player atacó al enemigo con " + meleeAttackDamage + " de daño (melee)");
            }
        }
    }

    private void RangedAttack()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
        if (prb != null)
        {
            prb.linearVelocity = lastMoveDir.normalized * 10f;
        }

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetDamage(rangedAttackDamage);
        }

        Debug.Log("Player lanzó un proyectil en dirección " + lastMoveDir + " (ranged)");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRadius);
    }
}
