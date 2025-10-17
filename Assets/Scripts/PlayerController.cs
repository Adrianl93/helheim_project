using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] private Animator animator;


    
    [SerializeField] private GameObject meleeHitboxPrefab;
    [SerializeField] private float meleeDistance = 0.8f;
    [SerializeField] private int meleeAttackDamage = 15;
    [SerializeField] private float meleeAttackCooldown = 1f;

    [SerializeField] private int rangedAttackDamage = 10;
    [SerializeField] private float rangedAttackCooldown = 1.5f;
    [SerializeField] private float projectileSpeed = 10f;

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

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
    public float MeleeAttackRadius => meleeDistance;

    private bool rangedUnlocked = false;
    public bool RangedUnlocked => rangedUnlocked;
    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        GameManager.OnRangedUnlocked += EnableRangedAttack;
    }

    private void OnDisable()
    {
        GameManager.OnRangedUnlocked -= EnableRangedAttack;
    }

    private void EnableRangedAttack()
    {
        rangedUnlocked = true;
        Debug.Log("[PlayerController] Ataque a distancia activado!");
    }

    private void Update()
    {
       
        input = playerInput.actions["Move"].ReadValue<Vector2>();

        if (input != Vector2.zero)
        {
            lastMoveDir = input;
            animator.SetFloat("MoveX", input.x);
            animator.SetFloat("MoveY", input.y);
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

        
        if (playerInput.actions["Melee"].triggered && Time.time >= lastMeleeAttackTime + meleeAttackCooldown)
        {
            lastMeleeAttackTime = Time.time;
            StartCoroutine(MeleeAnimationRoutine());
        }

        
        if (rangedUnlocked && playerInput.actions["Ranged"].triggered && Time.time >= lastRangedAttackTime + rangedAttackCooldown)
        {
            lastRangedAttackTime = Time.time;
            StartCoroutine(RangedAnimationRoutine());
        }

       
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
        if (animator != null)
            animator.SetTrigger("Attack");

        // se calcula un offset segun la direccion del ataque
        Vector2 normalizedDir = lastMoveDir.normalized;
        float xOffset = normalizedDir.x * meleeDistance;
        float yOffset = normalizedDir.y * meleeDistance;

        // se agrega este offset para evitar que se superponga al personaje
        if (Mathf.Abs(normalizedDir.y) > 0.1f)
        {
            yOffset += normalizedDir.y * 0.3f; 
        }

        // Se genera el hitbox
        Vector3 spawnPos = transform.position + new Vector3(xOffset, yOffset, 0f);
        GameObject hitbox = Instantiate(meleeHitboxPrefab, spawnPos, Quaternion.identity, transform);
        hitbox.transform.right = lastMoveDir;

        AttackHitbox hitboxScript = hitbox.GetComponent<AttackHitbox>();
        if (hitboxScript != null)
        {
            hitboxScript.Initialize(meleeAttackDamage, enemyLayer);
        }

        Debug.Log("Player realizó un ataque melee hacia " + lastMoveDir);
    }

    private void RangedAttack()
    {
        if (!TryConsumeMana(rangedManaCost))
        {
            Debug.Log($"No hay suficiente mana para el ataque ranged. Mana actual: {currentMana}/{maxMana}");
            return;
        }

        // El ataque se apunta con el mouse
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mouseWorldPos - firePoint.position).normalized;

        // creamos el proyectil
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
        GameManager.Instance.AddScore(amount * 10);
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

    public void SetRangedUnlocked(bool value)
    {
        rangedUnlocked = value;
        if (rangedUnlocked)
        {
            Debug.Log("[PlayerController] Ataque a distancia restaurado desde checkpoint");
            GameManager.Instance.TriggerRangedUnlocked(); 
        }
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


    private IEnumerator MeleeAnimationRoutine()
    {
        animator.SetBool("IsAttackingMelee", true);
        MeleeAttack(); 
        yield return new WaitForSeconds(0.5f); 
        animator.SetBool("IsAttackingMelee", false);
    }

    private IEnumerator RangedAnimationRoutine()
    {
        animator.SetBool("IsAttackingRanged", true);
        RangedAttack(); 
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("IsAttackingRanged", false);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)lastMoveDir * meleeDistance);
    }
}
