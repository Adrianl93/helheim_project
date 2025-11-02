using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    public string sceneName;
    public EnemyController boss; // ← Arrastra aquí al boss desde el Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (boss == null || boss.IsDead)
            {
                Debug.Log("El jefe ha muerto o fue destruido, cargando escena...");
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.Log("El jefe aún vive, no puedes salir.");
            }
        }
    }
}
