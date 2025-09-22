using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Audio")]
    [SerializeField] private AudioClip ambientMusic;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.5f; // Medidor de volumen
    private AudioSource audioSource;

    [Header("Player")]
    [SerializeField] private GameObject player;
    public GameObject Player => player;
    private PlayerController playerController;
    private PlayerHealth playerHealth;

    private GameObject startPoint;
    private GameObject[] checkpoints;

    private Vector3 lastCheckpointPos;
    private PlayerState lastCheckpointState;
    private bool firstLoad = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Configurar audio
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = ambientMusic;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = musicVolume;

            if (ambientMusic != null)
                audioSource.Play();

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

        if (playerController == null)
            playerController = player.GetComponent<PlayerController>();
        if (playerHealth == null)
            playerHealth = player.GetComponent<PlayerHealth>();

        if (startPoint == null)
            startPoint = GameObject.FindGameObjectWithTag("StartPoint");

        checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");

        if (lastCheckpointPos == Vector3.zero && startPoint != null)
            lastCheckpointPos = startPoint.transform.position;

        player.transform.position = lastCheckpointPos;

        if (lastCheckpointState != null)
        {
            playerController.SetStats(
                lastCheckpointState.meleeAttack,
                lastCheckpointState.rangedAttack
            );
            playerHealth.SetArmor(lastCheckpointState.armor);
            playerController.SetCoins(lastCheckpointState.coins);
        }

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
        lastCheckpointState = null;
        SceneManager.LoadScene("Scene1");
        Debug.Log("[GameManager] Juego reiniciado desde StartPoint");
    }

    public void SaveCheckpoint(GameObject checkpoint)
    {
        if (checkpoint.CompareTag("Checkpoint"))
        {
            lastCheckpointPos = checkpoint.transform.position;

            lastCheckpointState = new PlayerState(
                checkpoint.transform.position,
                playerController.MeleeDamage,
                playerController.RangedDamage,
                playerHealth.Armor,
                playerController.Coins
            );

            Debug.Log($"[GameManager] Checkpoint guardado en {lastCheckpointPos} -> {checkpoint.name}");
        }
    }


    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
            audioSource.volume = musicVolume;
    }

    public void IncreaseVolume(float increment = 0.05f)
    {
        SetMusicVolume(musicVolume + increment);
    }

    public void DecreaseVolume(float decrement = 0.05f)
    {
        SetMusicVolume(musicVolume - decrement);
    }
}
