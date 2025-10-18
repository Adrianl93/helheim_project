using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType { Melee, Ranged, Boss }

    [SerializeField] private EnemyType enemyType = EnemyType.Melee;
    private Transform player;

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
    [SerializeField] private float minRangedDistance = 4f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float burstMultiplier = 2f;
    [SerializeField] private int rewardScore = 1000;

    [SerializeField] private float enragedDuration = 5f;

    private NavMeshAgent agent; 
    private Vector2 movement;
    private float lastMeleeAttackTime = -1f;
    private float lastRangedAttackTime = 0f;

    private float originalDetectionRadius;
    private float enragedTimer = 0f;

    public int Armor => armor;
    public int CurrentHealth => health;
    private EnemyType currentAttackMode;

    [SerializeField] private AudioClip meleeAttackSound;
    [SerializeField] private float meleeSoundVolume = 1f;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float deathSoundVolume = 1f;

    [SerializeField] private int manaReward = 5;

    [SerializeField] private float rangedBurstInterval = 15f;
    [SerializeField] private int burstProjectileCount = 8;
    private float rangedBurstTimer = 0f;
    private bool playerDetected = false;

    private bool hasTriggeredEngagement = false;
    private bool isBursting = false;
    [SerializeField] private float burstPauseDuration = 8f;

    private Coroutine engagementCheckRoutine;

    [Header("Animaciones")]
    [SerializeField] private Animator animator;
    private Vector2 lastMoveDir = Vector2.down;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // configuracion del navmesh
        agent.updateRotation = false;
        agent.updateUpAxis = false;
       
        // le paso la speed del enemy al agent del navmesh
        agent.speed = speed;

        originalDetectionRadius = detectionRadius;
        maxHealth = health;

        currentAttackMode = (enemyType == EnemyType.Boss) ? EnemyType.Melee : enemyType;
        rangedBurstTimer = rangedBurstInterval;


        AssignPlayerReference();
    }

    void Update()
    {
        if (player == null)
        {
            AssignPlayerReference();
            return;
        }

        HandleEnragedState();
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        movement = Vector2.zero;

        // al detectar al player activa el enraged state
        if (!hasTriggeredEngagement && distanceToPlayer <= originalDetectionRadius)
        {
            hasTriggeredEngagement = true;
            TriggerEngagement();
        }

        // lógica variable segun el tipo de enemigo boss/valkirye/werewolf
        if (enemyType == EnemyType.Boss)
        {
            if (!playerDetected && distanceToPlayer <= detectionRadius)
            {
                playerDetected = true;
                rangedBurstTimer = rangedBurstInterval;
            }

            if (!isBursting)
            {
                if (distanceToPlayer <= meleeRadius)
                    TryMeleeAttack();
                else if (distanceToPlayer <= detectionRadius)
                    MoveTowardsPlayer();
                else
                    agent.ResetPath();
            }

            if (playerDetected)
            {
                rangedBurstTimer -= Time.deltaTime;
                if (rangedBurstTimer <= 0f)
                {
                    StartCoroutine(FireRangedBurstAndPause());
                    rangedBurstTimer = rangedBurstInterval;
                }
            }
        }
        else if (enemyType == EnemyType.Melee)
        {
            if (distanceToPlayer <= meleeRadius)
            {
                TryMeleeAttack();
                agent.ResetPath();
            }
            else if (distanceToPlayer <= detectionRadius)
                MoveTowardsPlayer();
            else
                agent.ResetPath();
        }
        else if (enemyType == EnemyType.Ranged)
        {
            if (distanceToPlayer <= detectionRadius)
            {
                // Si el player se acerca demasiado, busca alejarse para seguir atacando a distancia
                if (distanceToPlayer < minRangedDistance)
                    MoveAwayFromPlayer();
                else if (distanceToPlayer > rangedAttackRange)
                    MoveTowardsPlayer();
                else
                    agent.ResetPath();

                if (distanceToPlayer <= rangedAttackRange)
                    TryRangedAttack();
            }
            else
            {
                agent.ResetPath();
            }
        }

        
        UpdateAnimator();
    }

    private void MoveTowardsPlayer()
    {
        if (agent != null && player != null)
            agent.SetDestination(player.position);
    }

    private void MoveAwayFromPlayer()
    {
        if (agent == null || player == null) return;

        // dirección contraria al jugador
        Vector2 dir = (transform.position - player.position).normalized;
        Vector2 retreatPos = (Vector2)transform.position + dir * 2f;

        agent.SetDestination(retreatPos);
    }

    private void UpdateAnimator()
    {
        if (animator != null)
        {
            Vector2 velocity = new Vector2(agent.velocity.x, agent.velocity.y);

            if (velocity.sqrMagnitude > 0.01f)
                lastMoveDir = velocity.normalized;

            animator.SetFloat("MoveX", lastMoveDir.x);
            animator.SetFloat("MoveY", lastMoveDir.y);
            animator.SetBool("IsMoving", velocity.sqrMagnitude > 0.01f);
        }
    }

    private void AssignPlayerReference()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
            player = GameManager.Instance.Player.transform;
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

    // si se activa el rango de deteccion aumenta su tamaño para perseguir al player
    private void TriggerEngagement()
    {
        detectionRadius = originalDetectionRadius * 2f;
        enragedTimer = enragedDuration;

        if (engagementCheckRoutine != null)
            StopCoroutine(engagementCheckRoutine);

        engagementCheckRoutine = StartCoroutine(CheckPlayerPresence());
    }

    private IEnumerator CheckPlayerPresence()
    {
        //se checkea cada 3 segundos si el player continua en el rango de deteccion original, si huye se vuelve al estado normal
        while (true)
        {
            yield return new WaitForSeconds(3f);

            if (player == null) yield break;

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= originalDetectionRadius)
            {
                enragedTimer = enragedDuration;
            }
            else
            {
                hasTriggeredEngagement = false;
                StopCoroutine(engagementCheckRoutine);
                engagementCheckRoutine = null;
                yield break;
            }
        }
    }

    private void TryMeleeAttack()
    {
        if (player == null) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && Time.time >= lastMeleeAttackTime + meleeCooldown)
        {
            lastMeleeAttackTime = Time.time;

            if (animator != null)
                animator.SetTrigger("AttackMelee");

            playerHealth.TakeDamage(meleeDamage);

            if (meleeAttackSound != null)
                AudioSource.PlayClipAtPoint(meleeAttackSound, transform.position, meleeSoundVolume);
        }
    }

    private void TryRangedAttack()
    {
        if (projectilePrefab == null || player == null) return;
        if (Time.time < lastRangedAttackTime + rangedCooldown) return;

        lastRangedAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("AttackRanged");

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
        if (prb != null)
        {
            Vector2 dir = (player.position - firePoint.position).normalized;
            prb.linearVelocity = dir * projectileSpeed;
        }

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetDamage(rangedDamage);
            projScript.SetOwner(gameObject);
        }
    }

    private IEnumerator FireRangedBurstAndPause()
    {
        isBursting = true;

        int burstCount = 3;     //cantidad de rafagas       
        float delayBetweenBursts = 1f; //tiempo entre rafagas de ataques

        for (int i = 0; i < burstCount; i++)
        {
            FireRangedBurst();
            yield return new WaitForSeconds(delayBetweenBursts);
        }

        // Pausa final después de la última ráfaga
        yield return new WaitForSeconds(burstPauseDuration);
        isBursting = false;
    }

    private void FireRangedBurst()
    {
        if (projectilePrefab == null || firePoint == null) return;

        float angleStep = 360f / burstProjectileCount;

        for (int i = 0; i < burstProjectileCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            SpiralProjectile spiralProj = projectile.GetComponent<SpiralProjectile>();
            if (spiralProj != null)
            {
                float spiralRotation = 90f;
                spiralProj.Initialize(dir, projectileSpeed * burstMultiplier, spiralRotation, rangedDamage, gameObject);
            }
        }

        Debug.Log($"[Boss] lanzó un burst de {burstProjectileCount} proyectiles en espiral.");
    }

    public void TakeDamage(int incomingDamage)
    {
        int finalDamage = Mathf.Max(incomingDamage - armor, 0);
        health -= finalDamage;

        TriggerEngagement();

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        if (animator != null)
            animator.SetTrigger("Die");

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathSoundVolume);

        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                int manaRewardRandom = Random.Range(0, manaReward + 1);
                pc.AddMana(manaRewardRandom);
            }

            Destroy(gameObject);
            GameManager.Instance.AddScore(rewardScore);
            DropItemManager.Instance.DropItem(transform.position);
        }
    }

    private void OnDrawGizmos()
    {
        //amarillo = rango de deteccion 
        //rojo = rango melee 
        //naranja = rango ataque ranged 
        //verde = distancia minima para huir
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f); // naranja
        Gizmos.DrawWireSphere(transform.position, rangedAttackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minRangedDistance);
    }
}
