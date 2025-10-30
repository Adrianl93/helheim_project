using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Spawneo")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnRate = 2f;
    [SerializeField] private float spawnDuration = 10f;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    [Header("Animación")]
    [SerializeField] private Animator tombAnimator;
    [SerializeField] private float openAnimationDuration = 2f;

    [Header("Identificadores")]
    [SerializeField] private int spawnerID = 0;
    [SerializeField] private int spawnGroupID = 0;
    [SerializeField] private float spawnDelayPerID = 1.5f;

    [Header("Tiempo Personalizado")]
    [SerializeField] private bool useIndividualSpawnDuration = false;
    [SerializeField] private float individualSpawnDuration = 8f;

    private Coroutine spawnRoutine;

    public static event Action<int> OnSpawnerGroupTriggered;
    public static event Action OnSpawningFinished;

    private void Awake()
    {
       
        Debug.Log($"[EnemySpawner #{spawnerID}] Awake() - Registrando grupo {spawnGroupID}");
        OnSpawnerGroupTriggered += HandleGroupTriggered;
   
    }

    private void OnDisable()
    {
        Debug.Log($"[EnemySpawner #{spawnerID}] Desuscribiendo evento de grupo...");
        OnSpawnerGroupTriggered -= HandleGroupTriggered;
    }

    private void HandleGroupTriggered(int groupID)
    {
        if (groupID != spawnGroupID) return;

        Debug.Log($"[EnemySpawner #{spawnerID}] Activado por grupo {groupID}.");

        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(StartWithDelay());
    }

    private IEnumerator StartWithDelay()
    {
        float delay = spawnerID * spawnDelayPerID;
        if (delay > 0)
        {
            Debug.Log($"[EnemySpawner #{spawnerID}] Esperando {delay:F1}s antes de iniciar spawneo.");
            yield return new WaitForSeconds(delay);
        }

        yield return StartCoroutine(SpawnEnemiesWithAnimation());
    }

    private IEnumerator SpawnEnemies()
    {
        float elapsed = 0f;
        float duration = useIndividualSpawnDuration ? individualSpawnDuration : spawnDuration;
        Debug.Log($"[EnemySpawner #{spawnerID}] Iniciando spawneo durante {duration}s.");

        while (elapsed < duration)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnRate);
            elapsed += spawnRate;
        }

        Debug.Log($"[EnemySpawner #{spawnerID}] Spawneo finalizado.");
        spawnRoutine = null;

        // Notificamos cuando todos los spawners del grupo terminen
        OnSpawningFinished?.Invoke();
    }

    private IEnumerator SpawnEnemiesWithAnimation()
    {
        if (tombAnimator != null)
        {
            tombAnimator.SetTrigger("Open");
            yield return new WaitForSeconds(openAnimationDuration);
        }

        yield return StartCoroutine(SpawnEnemies());
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0 || spawnPoint == null)
        {
            Debug.LogWarning($"[EnemySpawner #{spawnerID}] Falta prefab o spawnPoint.");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, enemyPrefabs.Count);
        GameObject enemyPrefab = enemyPrefabs[randomIndex];

        Vector3 spawnPosition = spawnPoint.position + spawnOffset;
        spawnPosition.z = 0f;

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"[EnemySpawner #{spawnerID}] Enemigo instanciado.");
    }

    public static void TriggerSpawnerGroup(int groupID)
    {
        Debug.Log($"[EnemySpawner] Evento global: activando grupo {groupID}...");
        OnSpawnerGroupTriggered?.Invoke(groupID);
    }
}
