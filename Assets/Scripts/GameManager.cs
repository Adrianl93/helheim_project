using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event Action OnTimeout;
    public static event Action<int> OnScoreChanged;

    [Header("Timer")]
    [SerializeField] private float gameDuration = 1200f; // 20 minutos
    private float timer;
    private bool timeoutTriggered = false;




    [Header("Player")]
    [SerializeField] private GameObject player;
    public GameObject Player => player;
    private PlayerController playerController;
    private PlayerHealth playerHealth;

    private GameObject startPoint;
    private GameObject[] checkpoints;

    private Vector3 lastCheckpointPos;
    public Vector3 LastCheckpointPos => lastCheckpointPos;
    private PlayerState lastCheckpointState;
    private bool firstLoad = true;

    [Header("Score")]
    [SerializeField] private int totalScore = 0;
    public int TotalScore => totalScore;

    public float RemainingTime => timer;

    private PlayerInput playerInput;

   
    private InputAction rebootAction;
    private InputAction restartAction;
    private InputAction exitAction;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            timer = gameDuration;

            playerInput = GetComponent<PlayerInput>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // SOLO en editor o build de desarrollo
            if (playerInput != null)
            {
                rebootAction = playerInput.actions["Reboot"];
                restartAction = playerInput.actions["Restart"];
                exitAction = playerInput.actions["Exit"];

                if (rebootAction != null) rebootAction.performed += OnRebootPerformed;
                if (restartAction != null) restartAction.performed += OnRestartPerformed;
                if (exitAction != null) exitAction.performed += OnExitPerformed;
            }
#endif
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (rebootAction != null) rebootAction.performed -= OnRebootPerformed;
        if (restartAction != null) restartAction.performed -= OnRestartPerformed;
        if (exitAction != null) exitAction.performed -= OnExitPerformed;
#endif

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnRebootPerformed(InputAction.CallbackContext ctx)
    {
        RestartScene();
    }

    private void OnRestartPerformed(InputAction.CallbackContext ctx)
    {
        RestartGame();
    }

    private void OnExitPerformed(InputAction.CallbackContext ctx)
    {
        ExitGame();
    }
#endif

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedPositionPlayer());
    }

    private IEnumerator DelayedPositionPlayer()
    {
        yield return null; 
        yield return null; 
        PositionPlayerImmediately();
        Debug.Log("[GameManager] Player reposicionado tras esperar 1 frame");
    }

    public static event Action OnRangedUnlocked;

    public void UnlockRangedAttack()
    {
        OnRangedUnlocked?.Invoke();
        Debug.Log("[GameManager] Ataque a distancia desbloqueado!");
    }

    private void PositionPlayerImmediately()
    {
        Debug.Log($"[PositionPlayerImmediately] lastCheckpointPos = {lastCheckpointPos}");

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

        if ((lastCheckpointPos == Vector3.zero || float.IsNaN(lastCheckpointPos.x)) && startPoint != null)
        {
            lastCheckpointPos = startPoint.transform.position;
            Debug.Log("[GameManager] Usando StartPoint por no haber checkpoint previo.");
        }
        else
        {
            Debug.Log($"[GameManager] Usando último checkpoint: {lastCheckpointPos}");
        }

        // desactivar física y collider antes de mover
        var rb = player.GetComponent<Rigidbody2D>();
        var col = player.GetComponent<Collider2D>();
        bool rbState = true;
        bool colState = true;

        if (rb != null)
        {
            rbState = rb.simulated;
            rb.simulated = false;
        }

        if (col != null)
        {
            colState = col.enabled;
            col.enabled = false;
        }

        // Validar que la posición esté sobre el NavMesh (NavMesh Plus 2D compatible)
        if (UnityEngine.AI.NavMesh.SamplePosition(lastCheckpointPos, out var hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
        {
            lastCheckpointPos = hit.position;
            Debug.Log("[NavMeshPlus] Posición válida sobre el NavMesh.");
        }
        else
        {
            Debug.LogWarning("[NavMeshPlus] No se encontró NavMesh cerca del checkpoint. Se usa posición original.");
        }

        // Intentar mover al jugador usando NavMeshAgent (NavMesh Plus usa este componente en 2D)
        var agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.enabled)
        {
            bool warped = agent.Warp(lastCheckpointPos);
            if (warped)
            {
                Debug.Log("[NavMeshPlus] Player reposicionado correctamente con NavMeshAgent.Warp().");
            }
            else
            {
                Debug.LogWarning("[NavMeshPlus] Warp falló, usando Transform.position como respaldo.");
                player.transform.position = lastCheckpointPos;
            }
        }
        else
        {
            player.transform.position = lastCheckpointPos;
            Debug.Log("[NavMeshPlus] Player reposicionado directamente (sin NavMeshAgent activo).");
        }

        Debug.Log($"[Reposicionado FINAL] Player a {player.transform.position}");
        Debug.Log($"[DEBUG] Pos antes del NavMeshAgent: {player.transform.position}");
 
        Debug.Log($"[DEBUG] Pos después del frame (NavMeshAgent): {player.transform.position}");

        // Reactivamos fisica y collider
        if (rb != null) rb.simulated = rbState;
        if (col != null) col.enabled = colState;

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
        if (!timeoutTriggered)
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

    #region Reinicio
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
    #endif
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

    //IMPORTANTE NO TOCAR: este IF raro funciona para que lo detecte unity editor y no tire error al ejecutar Application.Quit en el editor ya que esta funcion no se permite en el editor
    public void ExitGame()
    {
        Debug.Log("[GameManager] Cerrando el juego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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


    //#region Audio
    //public void SetMusicVolume(float volume)
    //{
    //    musicVolume = Mathf.Clamp01(volume);
    //    if (audioSource != null)
    //        audioSource.volume = musicVolume;
    //}

    //public void IncreaseVolume(float increment = 0.05f) => SetMusicVolume(musicVolume + increment);
    //public void DecreaseVolume(float decrement = 0.05f) => SetMusicVolume(musicVolume - decrement);
    //#endregion
}
