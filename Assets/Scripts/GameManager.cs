using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event Action OnTimeout;

    [Header("Timer")]
    [SerializeField] private float gameDuration = 1200f; // 20 minutos = 1200s
    private float timer;
    private bool timeoutTriggered = false;
    private bool isPaused = false;

    [Header("Audio")]
    [SerializeField] private AudioClip ambientMusic;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.5f;
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

    public float RemainingTime => timer;

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

            // Inicializar timer
            timer = gameDuration;
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
            playerController.SetMana(lastCheckpointState.mana);

            // Restaurar tiempo
            timer = lastCheckpointState.remainingTime;
            timeoutTriggered = false; 
        }
        else
        {
            // Si no hay checkpoint, reiniciar al tiempo original
            timer = gameDuration;
            timeoutTriggered = false;
        }


        if (firstLoad)
        {
            Debug.Log($"[GameManager] Player colocado en posición inicial: {lastCheckpointPos}");
            firstLoad = false;
        }
    }

    private void Update()
    {
        // Control del timer
        if (!isPaused && !timeoutTriggered)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0f;
                timeoutTriggered = true;
                OnTimeout?.Invoke();
                Debug.Log("[GameManager] Tiempo agotado: OnTimeout lanzado.");
            }
        }

        // Teclas de reinicio
        if (Input.GetKeyDown(KeyCode.R) && !Input.GetKey(KeyCode.LeftControl))
            RestartScene();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            RestartGame();
    }

   
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
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
                playerController.Coins,
                playerController.CurrentMana,
                timer
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
