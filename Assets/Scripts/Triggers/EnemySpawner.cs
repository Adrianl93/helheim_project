using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Spawneo")]
    [SerializeField] private List<GameObject> enemyPrefabs; // Lista de prefabs de enemigos
    [SerializeField] private Transform spawnPoint; // Punto desde donde se spawnean
    [SerializeField] private float spawnRate = 2f; // Tiempo entre cada spawn
    [SerializeField] private float spawnDuration = 10f; // Duración total del spawneo
    [SerializeField] private Vector3 spawnOffset = Vector3.zero; // Offset desde el spawnPoint

    [Header("Animation")]
    [SerializeField] private Animator tombAnimator; // Animator de la tumba
    [SerializeField] private float openAnimationDuration = 2f; // Tiempo de la animación


    private Coroutine spawnRoutine;

    // Evento que se invocará al finalizar el spawneo (para desbloquear una puerta)
    public static event Action OnSpawningFinished;

    private void Awake()
    {
        // Suscribimos temprano para evitar perder el evento si el trigger se activa rápido
        Debug.Log("[EnemySpawner] (Awake) Suscribiendo a OnSpawnerTriggered...");
        SpawnerTrigger.OnSpawnerTriggered += StartSpawning;
    }

    private void OnEnable()
    {
        Debug.Log("[EnemySpawner] (OnEnable) Componente activo.");
    }

    private void OnDisable()
    {
        Debug.Log("[EnemySpawner] (OnDisable) Desuscribiendo evento.");
        SpawnerTrigger.OnSpawnerTriggered -= StartSpawning;
    }

    private void StartSpawning()
    {
        Debug.Log("[EnemySpawner] Evento recibido: comenzando animación del sarcófago.");

        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnEnemiesWithAnimation());
    }

    private IEnumerator SpawnEnemies()
    {
        float elapsed = 0f;
        Debug.Log("[EnemySpawner] Iniciando rutina de spawneo.");

        while (elapsed < spawnDuration)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnRate);
            elapsed += spawnRate;
        }

        Debug.Log("[EnemySpawner] Spawneo finalizado. Invocando OnSpawningFinished...");
        OnSpawningFinished?.Invoke(); // FUTURO: se puede usar para abrir una puerta

        spawnRoutine = null;
    }

    private IEnumerator SpawnEnemiesWithAnimation()
    {
        // Activamos trigger para abrir el sarcófago
        if (tombAnimator != null)
        {
            tombAnimator.SetTrigger("Open");
            Debug.Log("[EnemySpawner] Trigger 'Open' activado en Animator.");

            // Esperamos la duración de la animación
            yield return new WaitForSeconds(openAnimationDuration);
            Debug.Log("[EnemySpawner] Animación finalizada. Comenzando spawneo.");
        }

        // Ahora sí empezamos el spawn
        yield return StartCoroutine(SpawnEnemies());
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0 || spawnPoint == null)
        {
            Debug.LogWarning("[EnemySpawner] Falta asignar prefabs o spawnPoint.");
            return;
        }

        // Selección aleatoria de prefab
        int randomIndex = UnityEngine.Random.Range(0, enemyPrefabs.Count);
        GameObject enemyPrefab = enemyPrefabs[randomIndex];

        // Calculamos la posición final con offset
        Vector3 spawnPosition = spawnPoint.position + spawnOffset;

        // Aseguramos que Z = 0 para 2D (visible en cámara ortográfica)
        spawnPosition.z = 0f;

        // Instanciamos el enemigo
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Debug para confirmar que el enemigo se creó
        Debug.Log($"[EnemySpawner] Spawned enemigo #{randomIndex} ({enemy.name}) en posición {spawnPosition}");

        // Opcional: aseguramos que el sprite sea visible
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning($"[EnemySpawner] El prefab '{enemyPrefab.name}' no tiene SpriteRenderer.");
        }
        else
        {
            sr.sortingLayerName = "Default"; 
            sr.sortingOrder = 0;
        }
    }
}
