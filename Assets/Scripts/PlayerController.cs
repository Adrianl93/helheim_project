using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private Animator animator;
    private NavMeshAgent agent;
    private PlayerInput playerInput;
    private Vector2 input;
    private Vector2 lastMoveDir = Vector2.right;

    [Header("Ataque Melee")]
    [SerializeField] private GameObject meleeHitboxPrefab;
    [SerializeField] private float meleeDistance = 0.8f;
    [SerializeField] private int meleeAttackDamage = 15;
    [SerializeField] private float meleeAttackCooldown = 1f;
    [SerializeField] private float meleeOffsetY = 0.5f;
    [SerializeField] private float meleeOffsetDiagonal = 0.2f;
    [SerializeField] private float meleeHitboxDuration = 0.2f; 
    private float lastMeleeAttackTime = 0f;

    [Header("Ataque a Distancia")]
    [SerializeField] private int rangedAttackDamage = 10;
    [SerializeField] private float rangedAttackCooldown = 1.5f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    private float lastRangedAttackTime = 0f;
    private bool rangedUnlocked = false;

    [Header("Mana y Recursos")]
    [SerializeField] private int coins = 0;
    [SerializeField] private int maxMana = 50;
    [SerializeField] private int currentMana = 25;
    [SerializeField] private int rangedManaCost = 5;

    [Header("Stats (Lectura)")]
    public int Coins => coins;
    public int CurrentMana => currentMana;
    public int MaxMana => maxMana;
    public int MeleeDamage => meleeAttackDamage;
    public int RangedDamage => rangedAttackDamage;
    public float MeleeAttackRadius => meleeDistance;
    public bool RangedUnlocked => rangedUnlocked;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        playerInput = GetComponent<PlayerInput>();

        // configuracion del navmesh
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // le paso la speed del player al agent del navmesh
        agent.speed = speed;
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
        //input de botones para controlar al player
        input = playerInput.actions["Move"].ReadValue<Vector2>();

        if (input != Vector2.zero)
        {
            lastMoveDir = input;
            animator.SetFloat("MoveX", input.x);
            animator.SetFloat("MoveY", input.y);
            animator.SetBool("IsMoving", true);

            // movimiento con navmesh X Y
            Vector2 moveDir = input.normalized;
            Vector3 move = new Vector3(moveDir.x, moveDir.y, 0f);
            agent.Move(move * speed * Time.deltaTime);
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

    private void MeleeAttack()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
        if (meleeAudioSource != null && meleeAttackSound != null)
            meleeAudioSource.PlayOneShot(meleeAttackSound);
        // obtenemos direccion del mouse
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 attackDir = (mouseWorldPos - transform.position).normalized;

        // normalizamos la direccion del ataque para evitar puntos intermedios ( solo 8 direcciones)
        Vector2[] directions = new Vector2[]
        {
        Vector2.up,                        // Norte
        new Vector2(1, 1).normalized,      // Noreste
        Vector2.right,                     // Este
        new Vector2(1, -1).normalized,     // Sudeste
        Vector2.down,                      // Sur
        new Vector2(-1, -1).normalized,    // Suroeste
        Vector2.left,                      // Oeste
        new Vector2(-1, 1).normalized      // Noroeste
        };

        // Elegimos la dirección más cercana (de las 8)
        float maxDot = -Mathf.Infinity;
        Vector2 snappedDir = Vector2.zero;

        foreach (Vector2 dir in directions)
        {
            float dot = Vector2.Dot(attackDir, dir);
            if (dot > maxDot)
            {
                maxDot = dot;
                snappedDir = dir;
            }
        }

        attackDir = snappedDir;

        // aplicamos un offset especifico segun cada una de las 8 direcciones
        Vector3 offset = Vector3.zero;

        if (attackDir == Vector2.up)
            offset = new Vector3(0f, meleeDistance + meleeOffsetY, 0f);
        else if (attackDir == Vector2.down)
            offset = new Vector3(0f, -meleeDistance - meleeOffsetY, 0f);
        else if (attackDir == Vector2.left)
            offset = new Vector3(-meleeDistance - meleeOffsetDiagonal, 0f, 0f);
        else if (attackDir == Vector2.right)
            offset = new Vector3(meleeDistance + meleeOffsetDiagonal, 0f, 0f);
        else if (attackDir == new Vector2(1, 1).normalized)
            offset = new Vector3(meleeDistance + meleeOffsetDiagonal, meleeDistance + meleeOffsetDiagonal, 0f);
        else if (attackDir == new Vector2(-1, 1).normalized)
            offset = new Vector3(-meleeDistance - meleeOffsetDiagonal, meleeDistance + meleeOffsetDiagonal, 0f);
        else if (attackDir == new Vector2(1, -1).normalized)
            offset = new Vector3(meleeDistance + meleeOffsetDiagonal, -meleeDistance - meleeOffsetDiagonal, 0f);
        else if (attackDir == new Vector2(-1, -1).normalized)
            offset = new Vector3(-meleeDistance - meleeOffsetDiagonal, -meleeDistance - meleeOffsetDiagonal, 0f);

        // creamos el hitbox
        Vector3 spawnPos = transform.position + offset;
        GameObject hitbox = Instantiate(meleeHitboxPrefab, spawnPos, Quaternion.identity, transform);

        hitbox.transform.right = attackDir;

        // la animacion se modifica segun el lugar al que disparamos
        animator.SetFloat("MoveX", attackDir.x);
        animator.SetFloat("MoveY", attackDir.y);

       
        AttackHitbox hitboxScript = hitbox.GetComponent<AttackHitbox>();
        if (hitboxScript != null)
        {
            hitboxScript.Initialize(meleeAttackDamage, enemyLayer, transform.position);
        }

        // destruimos el hitbox despues del delay
        Destroy(hitbox, meleeHitboxDuration);

        Debug.Log($"Ataque melee en dirección {attackDir}");
    }


    private void RangedAttack()
    {
        if (!TryConsumeMana(rangedManaCost))
        {
            Debug.Log($"No hay suficiente mana para el ataque ranged. Mana actual: {currentMana}/{maxMana}");
            return;
        }
        if (rangedAudioSource != null && rangedAttackSound != null)
            rangedAudioSource.PlayOneShot(rangedAttackSound);
        // El ataque se apunta con el mouse
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mouseWorldPos - firePoint.position).normalized;

        // Calculamos el ángulo para rotar el proyectil
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Instanciamos el proyectil rotado en la dirección del disparo
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0f, 0f, angle - 90f));
        Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
        if (prb != null)
            prb.linearVelocity = direction * projectileSpeed;

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetDamage(rangedAttackDamage);
            projScript.SetOwner(gameObject);
            // PASAMOS la posición de origen del ataque (la posición del player)
            projScript.SetAttackOrigin(transform.position);
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
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        Debug.Log($"Mana actual: {currentMana}");
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
    [Header("Audio")]
    [SerializeField] private AudioSource meleeAudioSource;
    [SerializeField] private AudioSource rangedAudioSource;
    [SerializeField] private AudioClip meleeAttackSound;
    [SerializeField] private AudioClip rangedAttackSound;
}
