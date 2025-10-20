using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseUI;

    private PlayerInput playerInput;
    private bool isPaused = false;

    public static event Action OnGamePaused;
    public static event Action OnGameResumed;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            // Nos suscribimos a la acción "Pause" directamente
            var pauseAction = playerInput.actions["Pause"];
            if (pauseAction != null)
                pauseAction.performed += ctx => TogglePause();
            else
                Debug.LogError("[PauseManager] No se encontró la acción 'Pause' en PlayerInput.");
        }
        else
        {
            Debug.LogWarning("[PauseManager] Falta PlayerInput en este objeto.");
        }

        Time.timeScale = 1f; // aseguramos tiempo normal al iniciar
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            var pauseAction = playerInput.actions["Pause"];
            if (pauseAction != null)
                pauseAction.performed -= ctx => TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseUI?.SetActive(true);
        OnGamePaused?.Invoke();
        Debug.Log("[PauseManager] Juego en pausa.");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseUI?.SetActive(false);
        OnGameResumed?.Invoke();
        Debug.Log("[PauseManager] Juego reanudado.");
    }

    public void ExitToMenu()
    {
        Debug.Log("[PauseManager] Intentando ir al menú...");
        bool sceneExists = false;

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (sceneName == "Menu 1") sceneExists = true;
        }

        if (!sceneExists)
        {
            Debug.LogError("[ExitToMenu] La escena 'Menu 1' NO se encuentra en Build Settings!");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu 1");
    }

    public void ResumeFromButton() => ResumeGame();
}
