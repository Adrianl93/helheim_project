using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    public string sceneName; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Algo entró al trigger: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Es el jugador, cargando escena: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }
}