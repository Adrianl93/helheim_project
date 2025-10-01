using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;

    [Header("Ataques")]
    [SerializeField] private float meleeAttackRadius = 1.5f;
    [SerializeField] private int meleeAttackDamage = 15;
    [SerializeField] private float meleeAttackCooldown = 1f;

    [SerializeField] private int rangedAttackDamage = 10;
    [SerializeField] private float rangedAttackCooldown = 1.5f;
    [SerializeField] private float projectileSpeed = 10f;

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Mana y monedas")]
    [SerializeField] private int coins = 0;
    [SerializeField] private int maxMana = 50;
    [SerializeField] private int currentMana = 25;
    [SerializeField] private int rangedManaCost = 5;

    private PlayerInput playerInput;
    private Vector2 input;
    private Vector2 lastMoveDir = Vector2.right;

    private float lastMeleeAttackTime = 0f;
    private float lastRangedAttackTime = 0f;

    public int Coins => coins;
    public int CurrentMana => currentMana;
    public int MaxMana => maxMana;
    public int MeleeDamage => meleeAttackDamage;
    public int RangedDamage => rangedAttackDamage;
    public float MeleeAttackRadius => meleeAttackRadius;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        // Movimiento
        input = playerInput.actions["Move"].ReadValue<Vector2>();
        if (input != Vector2.zero)
            lastMoveDir = input;

        // Ataque melee con botón izquierdo del mouse
        if (playerInput.actions["Melee"].triggered && Time.time >= lastMeleeAttackTime + meleeAttackCooldown)
        {
            lastMeleeAttackTime = Time.time;
            MeleeAttack();
        }

        // Ataque ranged con botón derecho del mouse
        if (playerInput.actions["Ranged"].triggered && Time.time >= lastRangedAttackTime + rangedAttackCooldown)
        {
            lastRangedAttackTime = Time.time;
            RangedAttack();
        }

        // Interactuar
        if (playerInput.actions["Interact"].triggered)
        {
            Interact();
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
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
        if (!TryConsumeMana(rangedManaCost))
        {
            Debug.Log($"No hay suficiente mana para el ataque ranged. Mana actual: {currentMana}/{maxMana}");
            return;
        }

        // Dirección hacia el mouse
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mouseWorldPos - firePoint.position).normalized;

        Debug.Log(direction);

        // Crear proyectil
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
        if (prb != null)
            prb.linearVelocity = direction.normalized * projectileSpeed;

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetDamage(rangedAttackDamage);
            projScript.SetOwner(gameObject);
        }

        Debug.Log("Player lanzó un proyectil hacia " + direction + " (ranged). Mana restante: " + currentMana);
    }

    private void Interact()
    {
        Debug.Log("Jugador intentó interactuar");
        
    }

    #region Stats & Mana
    public int AddAttack(int amount)
    {
        meleeAttackDamage += amount;
        rangedAttackDamage += amount;
        return Mathf.Max(meleeAttackDamage, rangedAttackDamage);
    }

    public void AddAttackBoost(int amount)
    {
        meleeAttackDamage += amount;
        rangedAttackDamage += amount;
        Debug.Log($"Player recogió un Attack Boost! Nuevo daño melee: {meleeAttackDamage}, daño ranged: {rangedAttackDamage}");
    }

    public void SetStats(int melee, int ranged)
    {
        meleeAttackDamage = melee;
        rangedAttackDamage = ranged;
    }

    public void SetCoins(int amount) => coins = amount;

    public void AddCoins(int amount)
    {
        coins += amount;
        GameManager.Instance.AddScore(amount*10); 
        Debug.Log($"Player recogió {amount} monedas. Total: {coins}");
    }

    public void SetMana(int value)
    {
        currentMana = Mathf.Clamp(value, 0, maxMana);
    }

    public void AddMana(int amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        Debug.Log($"Mana aumentado en {amount}. Mana actual: {currentMana}/{maxMana}");
    }

    public bool TryConsumeMana(int cost)
    {
        if (cost <= 0) return true;
        if (currentMana >= cost)
        {
            currentMana -= cost;
            return true;
        }
        return false;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRadius);
    }
}
