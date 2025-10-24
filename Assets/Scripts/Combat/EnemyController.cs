using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

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

    [SerializeField] private int minManaReward = 3;
    [SerializeField] private int maxManaReward = 5;


    [SerializeField] private float rangedBurstInterval = 8f;
    [SerializeField] private int burstProjectileCount = 6; 
    private float rangedBurstTimer = 0f;
    private bool playerDetected = false;

    private bool hasTriggeredEnragement = false;
    private bool isBursting = false;
    [SerializeField] private float burstPauseDuration = 8f;

    private Coroutine enragementCheckRoutine;
    private Coroutine burstRoutine; // tiempo entre rafagas (burst)

    [Header("Animaciones")]
    [SerializeField] private Animator animator;
    private Vector2 lastMoveDir = Vector2.down;

    [Header("UI de vida de enemigos")]
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private GameObject manaPopupPrefab;
    [SerializeField] private GameObject healthBarPrefab;
    private EnemyHealthBar healthBar;
    private GameObject healthBarInstance;
    private UnityEngine.UI.Image healthBarFill;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private Vector3 damagePopupOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector3 manaPopupOffset = new Vector3(0f, 2f, 0f);

    void Start()
    {
        // Crear la barra de vida
        if (healthBarPrefab != null)
        {
            // Se instancia el prefab dela healthBar
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity);

            if (healthBarInstance == null)
            {
                Debug.LogError($"[EnemyController] Falló la instancia del prefab de barra de vida para {name}.");
            }
            else
            {
                // asignamos el componente EnemyHealthBar
                healthBar = healthBarInstance.GetComponent<EnemyHealthBar>();
                if (healthBar == null)
                {
                    Debug.LogError($"[EnemyController] El prefab de barra de vida no tiene componente EnemyHealthBar.");
                }
                else
                {
                    healthBar.SetTarget(transform);
                }

                // Asignamos cámara principal al Canvas del prefab
                var canvas = healthBarInstance.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.worldCamera = Camera.main;
                }

                // Buscar el fill de la barra
                Transform fillTransform = healthBarInstance.transform.Find("HealthBar Foreground");
                if (fillTransform == null)
                {
                    Debug.LogWarning($"[EnemyController] No se encontró el objeto hijo 'HealthBar Foreground' en el prefab de barra de vida.");
                    healthBarFill = healthBarInstance.GetComponentInChildren<UnityEngine.UI.Image>();
                }
                else
                {
                    healthBarFill = fillTransform.GetComponent<UnityEngine.UI.Image>();
                }
            }
        }
        else
        {
            Debug.LogError($"[EnemyController] healthBarPrefab no asignado en el inspector para {name}.");
        }

        // configuración del NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError($"[EnemyController] No se encontró NavMeshAgent en {name}");
        else
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = speed;
        }

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
        if (!hasTriggeredEnragement && distanceToPlayer <= originalDetectionRadius)
        {
            hasTriggeredEnragement = true;
            TriggerEnragement();
        }

        // lógica variable segun el tipo de enemigo boss/valkirye/werewolf
        if (enemyType == EnemyType.Boss)
        {
            // Detectamos entrada y salida del rango
            if (distanceToPlayer <= detectionRadius)
            {
                if (!playerDetected)
                {
                    playerDetected = true;
                    rangedBurstTimer = rangedBurstInterval;
                }
            }
            else
            {
                // Si el jugador se aleja por determinado tiempo, el burst se detiene
                if (playerDetected)
                {
                    playerDetected = false;
                    if (burstRoutine != null)
                    {
                        StopCoroutine(burstRoutine);
                        burstRoutine = null;
                    }
                    isBursting = false;
                    Debug.Log($"[Boss] Jugador fuera de rango, burst detenido.");
                }
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
                    if (burstRoutine != null)
                        StopCoroutine(burstRoutine);

                    burstRoutine = StartCoroutine(FireRangedBurstAndPause());
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

        if (healthBarInstance == null && healthBarPrefab != null)
        {
            Debug.LogWarning($"[EnemyController] {name}: healthBarInstance desapareció o nunca se creó.");
        }

        if (healthBarInstance != null)
        {
            // hacemos que la barra siga al enemigo
            healthBarInstance.transform.position = transform.position + healthBarOffset;
            Debug.DrawLine(transform.position, healthBarInstance.transform.position, Color.magenta);
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
    private void TriggerEnragement()
    {
        detectionRadius = originalDetectionRadius * 2f;
        enragedTimer = enragedDuration;

        if (enragementCheckRoutine != null)
            StopCoroutine(enragementCheckRoutine);

        enragementCheckRoutine = StartCoroutine(CheckPlayerPresence());
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
                hasTriggeredEnragement = false;
                StopCoroutine(enragementCheckRoutine);
                enragementCheckRoutine = null;
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

        Vector2 dir = (player.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0, 0, angle - 90f));
        Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
        if (prb != null)
        {
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

        int projectileCount = burstProjectileCount; // ahora sí tomamos la cantidad de proyectiles por burst desde el inspector
        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

            // Instanciamos el proyectil con la rotación inicial correcta
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
            SpiralProjectile spiralProj = projectile.GetComponent<SpiralProjectile>();
            if (spiralProj != null)
            {
                float spiralRotation = 90f; // velocidad de giro de la espiral
                spiralProj.Initialize(dir, projectileSpeed * burstMultiplier, spiralRotation, rangedDamage, gameObject, angle);

            }
        }

        Debug.Log($"[Boss] lanzó un burst de {projectileCount} proyectiles en espiral.");
    }


   

    public void TakeDamage(int incomingDamage)
    {
        int finalDamage = Mathf.Max(incomingDamage - armor, 0);
        health -= finalDamage;

        // Pop up de daño
        if (damagePopupPrefab != null)
        {
            Vector3 spawnPos = transform.position + damagePopupOffset;
            GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
            var popupScript = popup.GetComponentInChildren<PopupUI>();

            if (popupScript != null)
                popupScript.Setup("-" + finalDamage);
        }

        // Actualizamos la barra de vida
        if (healthBarFill != null)
            healthBarFill.fillAmount = Mathf.Clamp01((float)health / maxHealth);

        if (healthBar != null)
            healthBar.UpdateHealth(health, maxHealth);

        // al llegar a 0 activamos el metodo Die
        if (health <= 0)
        {
            health = 0;
            Die();
            return;
        }

        // Activa el estado de enraged al recibir daño
        TriggerEnragement();
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
                int manaRewardRandom = Random.Range(minManaReward, maxManaReward + 1);

                pc.AddMana(manaRewardRandom);

                // Pop up de maná
                if (manaPopupPrefab != null)
                {
                    Vector3 spawnPos = transform.position + manaPopupOffset;
                    GameObject popup = Instantiate(manaPopupPrefab, spawnPos, Quaternion.identity);
                    var popupScript = popup.GetComponentInChildren<PopupUI>();
                    if (popupScript != null)
                        popupScript.Setup("+" + manaRewardRandom);
                }
            }
        }

        // destuimos la barra de vida al quedarnos sin vida
        if (healthBarInstance != null)
            Destroy(healthBarInstance);

        // aumentamos el score y dropeamos un item al morir
        GameManager.Instance.AddScore(rewardScore);
        DropItemManager.Instance.DropItem(transform.position);

        // Destruimos al enemigo
        Destroy(gameObject);
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
