using UnityEngine;

public class DropItemManager : MonoBehaviour
{
    public static DropItemManager Instance { get; private set; }

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerController playerController;

    [SerializeField] private GameObject[] easyItems = new GameObject[10];
    [SerializeField] private GameObject[] mediumItems = new GameObject[10];
    [SerializeField] private GameObject[] hardItems = new GameObject[10];

    [SerializeField] private float dropOffsetRadius = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void DropItem(Vector3 position)
    {
        (GameObject prefabToDrop, string poolName) = GetItemBasedOnStats();

        if (prefabToDrop != null)
        {
            Vector2 offset = Random.insideUnitCircle * dropOffsetRadius;
            Vector3 spawnPos = position + new Vector3(offset.x, offset.y, 0);

            Instantiate(prefabToDrop, spawnPos, Quaternion.identity);

            Debug.Log($"Se dropeó el item {prefabToDrop.name} del pool: {poolName}");
        }
        else
        {
            Debug.LogWarning("[DropItemManager] No hay items configurados en el pool elegido.");
        }
    }

    private (GameObject, string) GetItemBasedOnStats()
    {
        int armor = playerHealth.Armor;
        int meleeAttack = playerController.MeleeDamage;

        GameObject[] chosenPool;
        string poolName;

        if (armor <= 7 && meleeAttack <= 19)
        {
            chosenPool = easyItems;
            poolName = "Easy";
        }
        else if (armor <= 11 && meleeAttack <= 25)
        {
            chosenPool = mediumItems;
            poolName = "Medium";
        }
        else
        {
            chosenPool = hardItems;
            poolName = "Hard";
        }

        if (chosenPool == null || chosenPool.Length == 0) return (null, poolName);

        int index = Random.Range(0, chosenPool.Length);
        return (chosenPool[index], poolName);
    }
}
