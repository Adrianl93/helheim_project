using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player")]
    [SerializeField] private GameObject player; // referencia interna
    public GameObject Player => player;        // propiedad pública

    private GameObject startPoint;
    private GameObject[] checkpoints;
    private Vector3 lastCheckpointPos; // posición real del último checkpoint
    private bool firstLoad = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Recuperar última posición guardada
            if (PlayerPrefs.HasKey("cpX"))
            {
                float x = PlayerPrefs.GetFloat("cpX");
                float y = PlayerPrefs.GetFloat("cpY");
                float z = PlayerPrefs.GetFloat("cpZ");
                lastCheckpointPos = new Vector3(x, y, z);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PositionPlayerImmediately();
    }

    private void PositionPlayerImmediately()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("[GameManager] No se encontró Player en la escena.");
            return;
        }

        // Detectar StartPoint
        if (startPoint == null)
            startPoint = GameObject.FindGameObjectWithTag("StartPoint");

        // Detectar checkpoints
        checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");

        // Si no hay checkpoint guardado, usar StartPoint
        if (lastCheckpointPos == Vector3.zero)
        {
            if (startPoint != null)
                lastCheckpointPos = startPoint.transform.position;
        }

        // Colocar player
        player.transform.position = lastCheckpointPos;

        // Solo mostrar log la primera vez
        if (firstLoad)
        {
            Debug.Log($"[GameManager] Player colocado en posición inicial: {lastCheckpointPos}");
            firstLoad = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !Input.GetKey(KeyCode.LeftControl))
            RestartScene();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            RestartGame();
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
        lastCheckpointPos = startPoint != null ? startPoint.transform.position : Vector3.zero;

        PlayerPrefs.DeleteKey("cpX");
        PlayerPrefs.DeleteKey("cpY");
        PlayerPrefs.DeleteKey("cpZ");
        PlayerPrefs.Save();

        SceneManager.LoadScene("Scene1");
        Debug.Log("[GameManager] Juego reiniciado desde StartPoint");
    }

    public void SaveCheckpoint(GameObject checkpoint)
    {
        if (checkpoint.CompareTag("Checkpoint"))
        {
            lastCheckpointPos = checkpoint.transform.position;

            PlayerPrefs.SetFloat("cpX", lastCheckpointPos.x);
            PlayerPrefs.SetFloat("cpY", lastCheckpointPos.y);
            PlayerPrefs.SetFloat("cpZ", lastCheckpointPos.z);
            PlayerPrefs.Save();

            Debug.Log($"[GameManager] Checkpoint guardado en {lastCheckpointPos} -> {checkpoint.name}");
        }
    }
}
