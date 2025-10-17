using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event Action OnTimeout;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;
    public static event Action<int> OnScoreChanged;

    [Header("Timer")]
    [SerializeField] private float gameDuration = 1200f; // 20 minutos
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

    [Header("Score")]
    [SerializeField] private int totalScore = 0;
    public int TotalScore => totalScore;
    




    public float RemainingTime => timer;


    private PlayerInput playerInput;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Config audio
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = ambientMusic;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = musicVolume;

            if (ambientMusic != null)
                audioSource.Play();

            SceneManager.sceneLoaded += OnSceneLoaded;

            // Timer inicial
            timer = gameDuration;

           
            playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.actions["Reboot"].performed += ctx => RestartScene();
                playerInput.actions["Restart"].performed += ctx => RestartGame();
                playerInput.actions["Pause"].performed += ctx => TogglePause();
            }
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

    public static event Action OnRangedUnlocked;

    public void UnlockRangedAttack()
    {
        OnRangedUnlocked?.Invoke();
        Debug.Log("[GameManager] Ataque a distancia desbloqueado!");
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
            playerController.SetRangedUnlocked(lastCheckpointState.rangedUnlocked);
            Debug.Log($"[LoadCheckpoint] rangedUnlocked = {lastCheckpointState.rangedUnlocked}");

            timer = lastCheckpointState.remainingTime;
            totalScore = lastCheckpointState.score; 
            GameManager.OnScoreChanged?.Invoke(totalScore); 

            timeoutTriggered = false;
        }
        else
        {
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
        // Timer
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
    }

    #region Pausa
    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        OnGameResumed?.Invoke(); ;
    }
    #endregion

    #region Reinicio
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
        lastCheckpointPos = startPoint != null ? startPoint.transform.position : Vector3.zero;
        lastCheckpointState = null;

        totalScore = 0; 
        OnScoreChanged?.Invoke(totalScore); 

        SceneManager.LoadScene("Scene1");
        Debug.Log("[GameManager] Juego reiniciado desde StartPoint con Score en 0");

    }
    #endregion

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
                timer,
                totalScore,
                playerController.RangedUnlocked

            );
            Debug.Log($"[SaveCheckpoint] rangedUnlocked = {playerController.RangedUnlocked}");
            Debug.Log($"[GameManager] Checkpoint guardado en {lastCheckpointPos} -> {checkpoint.name}");
        }
    }


    public void AddScore(int amount)
    {
        totalScore += amount;
        OnScoreChanged?.Invoke(totalScore);
        Debug.Log($"[GameManager] Score aumentado en {amount}. Total: {totalScore}");
    }

    public void TriggerRangedUnlocked()
    {
        OnRangedUnlocked?.Invoke();
        Debug.Log("[GameManager] Evento OnRangedUnlocked disparado desde RestoreCheckpoint");
    }


    #region Audio
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
            audioSource.volume = musicVolume;
    }

    public void IncreaseVolume(float increment = 0.05f) => SetMusicVolume(musicVolume + increment);
    public void DecreaseVolume(float decrement = 0.05f) => SetMusicVolume(musicVolume - decrement);
    #endregion
}
