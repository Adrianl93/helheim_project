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

    [Header("Delays de Ataque")]
    [SerializeField] private float meleeAttackDelay = 0.2f;  
    [SerializeField] private float rangedAttackDelay = 0.25f; 


    [Header("Mana y Recursos")]
    [SerializeField] private int coins = 0;
    [SerializeField] private int maxMana = 50;
    [SerializeField] private int currentMana = 25;
    [SerializeField] private int rangedManaCost = 5;

    [Header("Audio")]
    [SerializeField] private AudioSource meleeAudioSource;
    [SerializeField] private AudioSource rangedAudioSource;
    [SerializeField] private AudioClip meleeAttackSFX;
    [SerializeField] private AudioClip rangedAttackSFX;

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

        agent.updateRotation = false;
        agent.updateUpAxis = false;
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
        input = playerInput.actions["Move"].ReadValue<Vector2>();

        if (input != Vector2.zero)
        {
            lastMoveDir = input;
            animator.SetFloat("MoveX", input.x);
            animator.SetFloat("MoveY", input.y);
            animator.SetBool("IsMoving", true);

            Vector2 moveDir = input.normalized;
            Vector3 move = new Vector3(moveDir.x, moveDir.y, 0f);
            Vector3 proposedPosition = transform.position + move * speed * Time.deltaTime;

            // verificamos que la posicion este en el navmesh
            if (NavMesh.SamplePosition(proposedPosition, out NavMeshHit hit, 0.1f, NavMesh.AllAreas))
            {
                // solo se mueve si la posicion es valida
                agent.Move(hit.position - transform.position);
            }
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
        if (meleeAudioSource != null && meleeAttackSFX != null)
        {
            meleeAudioSource.pitch = Random.Range(0.95f, 1.05f);
            meleeAudioSource.PlayOneShot(meleeAttackSFX);
        }

        

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 attackDir = (mouseWorldPos - transform.position).normalized;

        Vector2[] directions = new Vector2[]
        {
            Vector2.up,
            new Vector2(1, 1).normalized,
            Vector2.right,
            new Vector2(1, -1).normalized,
            Vector2.down,
            new Vector2(-1, -1).normalized,
            Vector2.left,
            new Vector2(-1, 1).normalized
        };

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

        Vector3 offset = Vector3.zero;

        if (attackDir == Vector2.up)
            offset = new Vector3(0f, meleeDistance + meleeOffsetY, 0f);
        else if (attackDir == Vector2.down)
            offset = new Vector3(0f, -meleeDistance - meleeOffsetY * 1.2f, 0f);
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

        Vector3 spawnPos = transform.position + offset;
        GameObject hitbox = Instantiate(meleeHitboxPrefab, spawnPos, Quaternion.identity, transform);

        hitbox.transform.right = attackDir;

        animator.SetFloat("MoveX", attackDir.x);
        animator.SetFloat("MoveY", attackDir.y);

        AttackHitbox hitboxScript = hitbox.GetComponent<AttackHitbox>();
        if (hitboxScript != null)
        {
            hitboxScript.Initialize(meleeAttackDamage, enemyLayer, transform.position);
        }

        Destroy(hitbox, meleeHitboxDuration);

        Debug.Log($"Ataque melee en dirección {attackDir}");
    }


    private void RangedAttack()
    {
        if (!TryConsumeMana(rangedManaCost))
        {
            Debug.Log($"No hay suficiente mana para ranged. Mana: {currentMana}/{maxMana}");
            return;
        }

        if (rangedAudioSource != null && rangedAttackSFX != null)
        {
            rangedAudioSource.pitch = Random.Range(0.98f, 1.02f);
            rangedAudioSource.PlayOneShot(rangedAttackSFX);
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mouseWorldPos - firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveY", direction.y);
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0f, 0f, angle));

        Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
        if (prb != null)
            prb.linearVelocity = direction * projectileSpeed;

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetDamage(rangedAttackDamage);
            projScript.SetOwner(gameObject);
            projScript.SetAttackOrigin(transform.position);
        }

        Debug.Log("Player lanzó un proyectil hacia " + direction);
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
        Debug.Log($"Attack Boost! Nuevo daño melee: {meleeAttackDamage}, ranged: {rangedAttackDamage}");
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
        animator.SetTrigger("AttackingMelee");
        

        //delay antes de crear el hitbox para sincronizar con la animación
        yield return new WaitForSeconds(meleeAttackDelay);
        MeleeAttack();

        yield return new WaitForSeconds(0.5f);
      
    }

    private IEnumerator RangedAnimationRoutine()
    {
        animator.SetTrigger("AttackingRanged");
        
        //delay antes de instanciar el ataque para sincronizar con la animación
        yield return new WaitForSeconds(rangedAttackDelay);
        RangedAttack();

        yield return new WaitForSeconds(0.5f);
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)lastMoveDir * meleeDistance);
    }
}
