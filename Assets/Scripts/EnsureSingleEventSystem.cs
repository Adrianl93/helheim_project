using UnityEngine;
using UnityEngine.EventSystems;

public class EnsureSingleEventSystem : MonoBehaviour
{
    //CODIGO PARA EVITAR DUPLICAR EVENT SYSTEMS 
    //si ya existe un event system, destruye el nuevo
    void Awake()
    {
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        if (eventSystems.Length > 1)
        {
            Destroy(gameObject);
        }
    }
}

