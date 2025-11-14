using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;


public class EnemyController : MonoBehaviour, IBossState
{
    public enum EnemyType { Melee, Ranged, Boss }

    [Header("Tipo y stats base")]
    [SerializeField] private EnemyType enemyType = EnemyType.Melee;
    private Transform player;
    [SerializeField] private int health = 50;
    private int maxHealth;
    [SerializeField] private int armor = 2;
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private int rangedDamage = 8;
    [SerializeField] private float speed = 2f;
    [SerializeField] private int rewardScore = 1000;
    [SerializeField] private float knockbackForce;

    [Header("Detección y rangos")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float meleeRadius = 1.5f;
    [SerializeField] private float meleeCooldown = 1.5f;
    [SerializeField] private float rangedCooldown = 2f;
    [SerializeField] private float rangedAttackRange = 8f;
    [SerializeField] private float minRangedDistance = 4f;

    [Header("Proyectiles y ataque a distancia")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float rangedAttackDelay = 0.5f;
    [SerializeField] private float rangedBurstInterval = 8f;
    [SerializeField] private int burstProjectileCount = 6;
    [SerializeField] private float burstPauseDuration = 8f;

    [Header("Ataque cuerpo a cuerpo")]
    // Nota: se conserva el campo meleeHitboxPrefab para compatibilidad, pero ya no se usa.
    [SerializeField] private GameObject meleeHitboxPrefab;
    [SerializeField] private float meleeHitboxDuration = 0.3f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float meleeAttackDelay = 0.5f;
    [SerializeField] private float meleeDistance = 0.8f;
    [SerializeField] private float meleeOffsetY = 0.5f;
    [SerializeField] private float meleeOffsetX = 0.4f;
    [SerializeField] private float meleeOffsetDiagonal = 0.2f;

    [Header("Estados y temporizadores")]
    [SerializeField] private float enragedDuration = 5f;
    private bool isDead = false;
    private float lastMeleeAttackTime = -1f;
    private float lastRangedAttackTime = 0f;
    private float originalDetectionRadius;
    private float enragedTimer = 0f;
    private float rangedBurstTimer = 0f;
    private bool playerDetected = false;
    private bool hasTriggeredEnragement = false;
    private bool isBursting = false;
    private Coroutine enragementCheckRoutine;
    private Coroutine burstRoutine;

    [Header("Referencias")]
    private NavMeshAgent agent;
    private Vector2 movement;
    private EnemyType currentAttackMode;
    private Vector2 lastMoveDir = Vector2.down;

    [Header("Audio")]
    [SerializeField] private AudioClip meleeAttackSound;
    [SerializeField] private float meleeSoundVolume = 1f;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float deathSoundVolume = 1f;

    [Header("Recompensas")]
    [SerializeField] private int minManaReward = 3;
    [SerializeField] private int maxManaReward = 5;

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

    [Header("Animaciones")]
    [SerializeField] private Animator animator;

    [Header("Propiedades públicas")]
    public int Armor => armor;
    public int CurrentHealth => health;
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
                    healthBar.SetOffset(healthBarOffset);
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
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (isDead) return;
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
        // se aleja del player para tratar de evitar ser atacado, manteniendo una distancia minima
        if (agent == null || player == null) return;

        Vector2 dir = (transform.position - player.position).normalized;
        Vector2 basePos = transform.position;
        float desiredDistance = 2.5f; // distancia a la que intentará reposicionarse

        Vector3 bestPosition = transform.position;
        bool foundValid = false;

        // Probar varios ángulos alternativos si el camino directo no es navegable
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f; // pruebo en todas las direcciones cardinales
            Vector2 rotatedDir = Quaternion.Euler(0, 0, angle) * dir;
            Vector2 testPos = basePos + rotatedDir * desiredDistance;

            // Verificamos si el punto está dentro del NavMesh
            if (NavMesh.SamplePosition(testPos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                bestPosition = hit.position;
                foundValid = true;
                break;
            }
        }

        if (foundValid)
        {
            // Si el agente está fuera del NavMesh, lo “recolocamos” antes de moverlo
            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit fixHit, 1f, NavMesh.AllAreas))
                {
                    agent.Warp(fixHit.position);
                }
            }

            // Asignamos destino dentro del NavMesh
            agent.SetDestination(bestPosition);
        }
        else
        {
            // No hay posición válida, reseteamos movimiento
            agent.ResetPath();
        }

        // Prevención extra: si el agente se sale del NavMesh, lo recoloca en el siguiente frame
        if (!agent.isOnNavMesh)
        {
            StartCoroutine(RepositionOnNavMesh());
        }
    }

    private IEnumerator RepositionOnNavMesh()
    {
        // esperamos un frame para evitar warps dobles en el mismo update
        yield return null;

        if (agent != null && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                Debug.Log($"[EnemyController] {name} recolocado en NavMesh tras detección de salida.");
            }
        }
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

    // si se activa, el rango de deteccion aumenta su tamaño para perseguir al player
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
        if (Time.time < lastMeleeAttackTime + meleeCooldown) return;

        lastMeleeAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("AttackMelee");

        // Delay para animacion
        StartCoroutine(PerformMeleeAttackDelayed(meleeAttackDelay));
    }

    private void TryRangedAttack()
    {
        if (projectilePrefab == null || player == null) return;
        if (Time.time < lastRangedAttackTime + rangedCooldown) return;

        lastRangedAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("AttackRanged");

        // Esperamos que termine la animación antes de instanciar el proyectil
        StartCoroutine(DelayedRangedAttack(rangedAttackDelay)); // duración del clip (0.6s)
    }

    private IEnumerator DelayedRangedAttack(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (projectilePrefab == null || player == null) yield break;

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


        yield return new WaitForSeconds(rangedAttackDelay);

        int burstCount = 3;     //cantidad de rafagas       
        float delayBetweenBursts = 1f; //tiempo entre rafagas de ataques

        for (int i = 0; i < burstCount; i++)
        {
            if (animator != null)
                animator.SetTrigger("AttackBurst");

            // Espera el delay propio de la animación
            yield return new WaitForSeconds(rangedAttackDelay);
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

        int projectileCount = burstProjectileCount; //tomamos la cantidad de disparos desde el inspector
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
                spiralProj.Initialize(
                    dir,               // dirección del disparo 
                    rangedDamage,      // daño
                    gameObject,        // owner
                    firePoint.position,// posición inicial
                    angle              // ángulo inicial
                );
            }


            Debug.Log($"[Boss] lanzó un burst de {projectileCount} proyectiles en espiral.");
        }
    }




    public void TakeDamage(int incomingDamage, Vector2? attackOrigin = null)
    {
        if (isDead) return; //si esta muerto no se aplica daño
        int finalDamage = Mathf.Max(incomingDamage - armor, 0);
        health -= finalDamage;

        // animación de daño
        if (animator != null && health > 0)
            animator.SetTrigger("TakeDamage");

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && attackOrigin.HasValue)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - attackOrigin.Value).normalized;

            // iniciamos una corrutina para manejar el knockback sin romper el navmesh
            StartCoroutine(ApplyKnockback(rb, knockbackDir));
        }

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



    // knockback (retroceso) al recibir daño
    private IEnumerator ApplyKnockback(Rigidbody2D rb, Vector2 dir)
    {
        float knockbackDuration = 0.1f;   // duración total del empuje
        float knockbackDistance = 0.5f;   // distancia del retroceso
        float elapsed = 0f;

        // micro stun(aturdimiento temporal)
        bool wasStopped = false;
        if (agent != null)
        {
            wasStopped = agent.isStopped;
            agent.isStopped = true;
        }

        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + dir.normalized * knockbackDistance;


        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / knockbackDuration;
            Vector2 newPos = Vector2.Lerp(startPos, targetPos, t);
            Vector3 validPos;
            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
                validPos = hit.position;
            else
                validPos = transform.position; // no se mueve si no hay posición válida (navmesh)

            agent.Warp(validPos);

            yield return null;
        }


        // micro stun (aturdimiento)
        yield return new WaitForSeconds(0.05f);

        // reanudar movimiento SOLO si el enemigo sigue vivo y el agente está activo
        if (!isDead && agent != null && agent.enabled)
            agent.isStopped = wasStopped;

    }


    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Detener movimiento del navmesh
        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Detener movimiento físico
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Animación de muerte
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Die");
            animator.SetBool("IsDead", true);
        }

        // Sonido
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathSoundVolume);

        // Desactivamos colisiones para no chocarlo
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1.2f);
        GameManager.Instance.AddScore(rewardScore);
        DropItemManager.Instance.DropItem(transform.position);

        //Mana que ganara el player
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            //se asigna un valor aleatorio de mana entre el minimo y maximo definido
            int manaReward = Random.Range(minManaReward, maxManaReward + 1);
            playerController.AddMana(manaReward);

            // Pop-up de mana ganado
            if (manaPopupPrefab != null)
            {
                Vector3 spawnPos = transform.position + manaPopupOffset;
                GameObject popup = Instantiate(manaPopupPrefab, spawnPos, Quaternion.identity);
                var popupScript = popup.GetComponentInChildren<PopupUI>();
                if (popupScript != null)
                    popupScript.Setup("+" + manaReward);
            }
        }

        if (healthBarInstance != null)
            Destroy(healthBarInstance);

        Destroy(gameObject);
    }



    // ---- Reemplazado: ataque melee tipo SLASH (cono 60°) ----
    private IEnumerator PerformMeleeAttackDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (player == null) yield break;

        // Dirección hacia donde está el jugador (y hacia donde "mira" el enemy)
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        lastMoveDir = dir;

        float coneAngle = 60f; // apertura total del cono (opción A)
        float coneHalf = coneAngle * 0.5f;
        float coneDistance = meleeRadius; // usamos meleeRadius como alcance del cono

        // Detectamos todos los colliders en el radio y filtramos por ángulo
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, coneDistance, (int)playerLayer);


        foreach (var hit in hits)
        {
            if (hit == null) continue;

            Vector2 toTarget = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            float angleToTarget = Vector2.Angle(dir, toTarget);

            if (angleToTarget <= coneHalf)
            {
                PlayerHealth hp = hit.GetComponent<PlayerHealth>();
                if (hp != null)
                {
                    hp.TakeDamage(meleeDamage);
                }
            }
        }

        if (meleeAttackSound != null)
            AudioSource.PlayClipAtPoint(meleeAttackSound, transform.position, meleeSoundVolume);

        Debug.Log($"[EnemyController] {name} realizó un SLASH frontal (cono {coneAngle}°).");
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

        // Dibujo opcional del cono frontal (solo en editor, si hay player asignado)
#if UNITY_EDITOR
        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            float coneAngle = 60f;
            float coneDistance = meleeRadius;

            Vector3 leftDir = Quaternion.Euler(0, 0, coneAngle / 2f) * (Vector3)dir * coneDistance;
            Vector3 rightDir = Quaternion.Euler(0, 0, -coneAngle / 2f) * (Vector3)dir * coneDistance;

            Gizmos.DrawLine(transform.position, transform.position + leftDir);
            Gizmos.DrawLine(transform.position, transform.position + rightDir);

            // Opcional: dibujar varios segmentos para visualizar el borde curvo
            int segments = 12;
            Vector3 prev = transform.position + rightDir;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float a = -coneAngle / 2f + t * coneAngle;
                Vector3 p = transform.position + (Quaternion.Euler(0, 0, a) * (Vector3)dir * coneDistance);
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }
#endif
    }
    public bool IsDead => isDead;
    public bool IsChasing => playerDetected && !isDead;
}
