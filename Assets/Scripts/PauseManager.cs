using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI asignada en Inspector (solo Scene1)")]
    [SerializeField] private GameObject pauseUI;

    private PlayerInput playerInput;
    private InputAction pauseAction;
    private bool isPaused = false;

    public static event Action OnGamePaused;
    public static event Action OnGameResumed;

    private void Awake()
    {
        // No usar singleton global ni DontDestroyOnLoad
        // El PauseManager solo vive dentro de la escena jugable.

        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("[PauseManager] Falta componente PlayerInput.");
            return;
        }

        pauseAction = playerInput.actions["Pause"];
        if (pauseAction != null)
            pauseAction.performed += OnPausePerformed;
        else
            Debug.LogError("[PauseManager] No se encontró la acción 'Pause'.");

        // En caso de entrar desde el menú con el juego pausado
        ResumeGame();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (pauseAction != null)
            pauseAction.performed -= OnPausePerformed;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si cargamos una escena de menú, este objeto se destruye
        if (scene.name.ToLower().Contains("menu"))
        {
            Destroy(gameObject);
            return;
        }

        // Reset por seguridad
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseUI != null)
            pauseUI.SetActive(false);
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        Debug.Log($"[PauseManager] TogglePause presionado. isPaused = {isPaused}");
        // Soltar el foco de la UI antes de cambiar el estado
        EventSystem.current?.SetSelectedGameObject(null);

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (pauseUI == null)
        {
            Debug.LogWarning("[PauseManager] No se asignó el Pause UI en el inspector.");
            return;
        }

        isPaused = true;
        Time.timeScale = 0f;
        pauseUI.SetActive(true);
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseUI?.SetActive(false);
        OnGameResumed?.Invoke();
    }

    public void ResumeFromButton() => ResumeGame();

    public void ExitToMenu()
    {
        // Siempre asegurarse de restaurar TimeScale antes de salir
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log("[PauseManager] Volviendo al menú principal...");
        SceneManager.LoadScene("Menu 1");
        Destroy(gameObject);
    }
}
