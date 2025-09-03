using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed;

    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private int meleeAttackDamage = 15;
    [SerializeField] private int distanceAttackDamage = 10;
    [SerializeField] private float meleeAttackCooldown = 1f;
    [SerializeField] private float distanceAttackCooldown = 1.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private Vector3 movement;
    private Vector2 lastMoveDir = Vector2.right; 
    private float lastMeleeAttackTime = 0f;
    private float lastDistanceAttackTime = 0f;

    private void Update()
    {
        
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

        // Ataque a distancia con Alt
        if ((Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            && Time.time >= lastDistanceAttackTime + distanceAttackCooldown)
        {
            lastDistanceAttackTime = Time.time;
            DistanceAttack();
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + (movement * speed * Time.fixedDeltaTime));
    }

    private void MeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyController enemy = enemyCollider.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(meleeAttackDamage);
                Debug.Log("Player ataco al enemigo con " + meleeAttackDamage + " de daño");
            }
        }
    }

    private void DistanceAttack()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
        if (prb != null)
        {
            prb.linearVelocity = lastMoveDir.normalized * 10f;
        }

        // Pasar el daño del Player al proyectil
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetDamage(distanceAttackDamage);
        }

        Debug.Log("Player lanzó un proyectil en dirección " + lastMoveDir);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
