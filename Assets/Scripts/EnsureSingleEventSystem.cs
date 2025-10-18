using UnityEngine;
using UnityEngine.EventSystems;

public class EnsureSingleEventSystem : MonoBehaviour
{
    void Awake()
    {
        // Si ya existe otro EventSystem, destruye este
        if (FindObjectsOfType<EventSystem>().Length > 1)
        {
            Destroy(gameObject);
        }
    }
}
