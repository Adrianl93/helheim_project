using UnityEngine;
using UnityEngine.SceneManagement; 

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

    private void OnEnable()
    {
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Solo ejecutar si la escena cargada es "Scene1"
        if (scene.name == "Scene1")
        {
            StartCoroutine(AssignPlayerWithDelay());
        }
    }

    private System.Collections.IEnumerator AssignPlayerWithDelay()
    {
        // Esperar 2 frames antes de asignar (para evitar errores al respawnear y y mover al player)
        yield return null;
        yield return null;

        AssignPlayerReferences();
    }

    private void Update()
    {

        if (SceneManager.GetActiveScene().name != "Scene1") return;
        // Si en algún momento el Player se pierde
        // volvemos a asignarlo automáticamente.
        if (playerHealth == null || playerController == null)
        {
            AssignPlayerReferences();
        }
    }

    private void AssignPlayerReferences()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.Player != null)
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
    }


    public void DropItem(Vector3 position)
    {
        (GameObject prefabToDrop, string poolName) = GetItemBasedOnStats();

        if (prefabToDrop != null)
        {
            Vector2 offset = Random.insideUnitCircle * dropOffsetRadius;
            Vector3 spawnPos = position + new Vector3(offset.x, offset.y, 0);

           
            Transform pickupsParent = GameObject.Find("Pickups")?.transform;

            if (pickupsParent != null)
            {
                
                Instantiate(prefabToDrop, spawnPos, Quaternion.identity, pickupsParent);
            }
            else
            {
                Debug.LogWarning("[DropItemManager] No se encontró el objeto 'Pickups' en la escena. Se instancia sin padre.");
                Instantiate(prefabToDrop, spawnPos, Quaternion.identity);
            }

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

        GameObject[] chosenPool = easyItems;
        string poolName = "Easy";

        if (armor > 20 || meleeAttack > 35)
        {
            chosenPool = hardItems;
            poolName = "Hard";
        }
        else if (armor >= 16 || meleeAttack >= 30)
        {
            chosenPool = mediumItems;
            poolName = "Medium";
        }
        else if (armor <= 11 && meleeAttack <= 26)
        {
            chosenPool = easyItems;
            poolName = "Easy";
        }
        else if (coins >= 150)
        {
            chosenPool = killItems;
            poolName = "Kill";
        }

        int index = Random.Range(0, chosenPool.Length);
        return (chosenPool[index], poolName);
    }
}
