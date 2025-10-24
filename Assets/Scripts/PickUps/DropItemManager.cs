using UnityEngine;

public class DropItemManager : MonoBehaviour
{
    public static DropItemManager Instance { get; private set; }

    private PlayerHealth playerHealth;
    private PlayerController playerController;

    [SerializeField] private GameObject[] easyItems = new GameObject[10];
    [SerializeField] private GameObject[] mediumItems = new GameObject[10];
    [SerializeField] private GameObject[] hardItems = new GameObject[10];
    [SerializeField] private GameObject[] killItems = new GameObject[10];

    [SerializeField] private float dropOffsetRadius = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        AssignPlayerReferences();
    }

    private void Update()
    {
        // Si en algún momento el Player se pierde (ej: al reiniciar escena),
        // volvemos a asignarlo automáticamente.
        if (playerHealth == null || playerController == null)
        {
            AssignPlayerReferences();
        }
    }

    private void AssignPlayerReferences()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            playerHealth = GameManager.Instance.Player.GetComponent<PlayerHealth>();
            playerController = GameManager.Instance.Player.GetComponent<PlayerController>();

            if (playerHealth == null || playerController == null)
            {
                Debug.LogWarning("[DropItemManager] El Player no tiene PlayerHealth o PlayerController.");
            }
            else
            {
                Debug.Log("[DropItemManager] Player asignado correctamente.");
            }
        }
        else
        {
            Debug.LogWarning("[DropItemManager] No se encontró GameManager o Player.");
        }
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
        if (playerHealth == null || playerController == null)
        {
            Debug.LogWarning("[DropItemManager] Player no asignado, devolviendo null.");
            return (null, "None");
        }

        int armor = playerHealth.Armor;
        int meleeAttack = playerController.MeleeDamage;
        int coins = playerController.Coins;

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
        else if ((armor < 11 && meleeAttack < 25) && coins >= 200 )
        {
            chosenPool = hardItems;
            poolName = "Hard";
           
        }
        else
        {
            chosenPool = killItems;
            poolName = "Kill";
        }

        if (chosenPool == null || chosenPool.Length == 0) return (null, poolName);

        int index = Random.Range(0, chosenPool.Length);
        return (chosenPool[index], poolName);
    }
}
