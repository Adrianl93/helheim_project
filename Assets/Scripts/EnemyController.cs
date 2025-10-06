using UnityEngine;
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
    [SerializeField] private int rewardScore = 1000;

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
    [SerializeField] private float burstPauseDuration = 1f;

    private Coroutine engagementCheckRoutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
                    movement = (player.position - transform.position).normalized;
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
                TryMeleeAttack();
            else if (distanceToPlayer <= detectionRadius)
                movement = (player.position - transform.position).normalized;
        }
        else if (enemyType == EnemyType.Ranged)
        {
            if (distanceToPlayer <= detectionRadius)
            {
                if (distanceToPlayer < minRangedDistance)
                    movement = (transform.position - player.position).normalized;
                else if (distanceToPlayer > rangedAttackRange)
                    movement = (player.position - transform.position).normalized;

                if (distanceToPlayer <= rangedAttackRange)
                    TryRangedAttack();
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
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

        //recibe el ccomponente playerhealth del player y le aplica daño 
        if (player == null) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && Time.time >= lastMeleeAttackTime + meleeCooldown)
        {
            lastMeleeAttackTime = Time.time;
            playerHealth.TakeDamage(meleeDamage);

            if (meleeAttackSound != null)
                AudioSource.PlayClipAtPoint(meleeAttackSound, transform.position, meleeSoundVolume);
        }
    }

    private void TryRangedAttack()
    //ataca al player con proyectiles, consume mana al hacerlo
    //el ataque tiene un cooldown
    //se dispara en direccion al player
    {
        if (projectilePrefab == null || player == null) return;
        if (Time.time < lastRangedAttackTime + rangedCooldown) return;

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
        {
            projScript.SetDamage(rangedDamage);
            projScript.SetOwner(gameObject);
        }
    }

    private IEnumerator FireRangedBurstAndPause()
    {
        isBursting = true;
        FireRangedBurst();
        yield return new WaitForSeconds(burstPauseDuration);
        isBursting = false;
    }

    private void FireRangedBurst()
    {

        //ataque del boss que lanza varios proyectiles en todas direcciones cada cierto tiempo
        if (projectilePrefab == null || firePoint == null) return;

        float angleStep = 360f / burstProjectileCount;

        for (int i = 0; i < burstProjectileCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
            if (prb != null)
                prb.linearVelocity = dir * projectileSpeed;

            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.SetDamage(rangedDamage);
                projScript.SetOwner(gameObject);
            }
        }

        Debug.Log($"[Boss] lanzó un burst de {burstProjectileCount} proyectiles en todas direcciones.");
    }

    public void TakeDamage(int incomingDamage)
    //el enemigo recibe daño (se calcula daño del player - armadura del enemigo)
    //al ser atacado se activa el enraged state
    {
        int finalDamage = Mathf.Max(incomingDamage - armor, 0);
        health -= finalDamage;

        TriggerEngagement();

        if (health <= 0)
            Die();
    }

    private void Die()
    {
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
        Gizmos.color = Color.orangeRed;
        Gizmos.DrawWireSphere(transform.position, rangedAttackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minRangedDistance);
    }
}
