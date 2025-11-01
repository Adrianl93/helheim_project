#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject triggersContainer;

    [Header("Valores de Cheats")]
    [SerializeField] private int cheatDamageValue = 50;
    [SerializeField] private int cheatArmorValue = 50;
    [SerializeField] private float cheatSpeedValue = 25f;

    [Header("Input")]
    [SerializeField] private InputActionAsset cheatInputAsset;
    [SerializeField] private string cheatActionMapName = "Cheats";

    private InputActionMap cheatMap;
    private InputAction cheatDamage;
    private InputAction cheatArmor;
    private InputAction cheatSpeed;
    private InputAction cheatDisableTriggers;

    private void Awake()
    {
        if (cheatInputAsset == null)
        {
            Debug.LogWarning("[CheatManager] No se asigno un InputActionAsset. Los cheats no estaran activos.");
            return;
        }

        // Buscar el action map dentro del asset
        cheatMap = cheatInputAsset.FindActionMap(cheatActionMapName, true);
        if (cheatMap == null)
        {
            Debug.LogWarning($"[CheatManager] No se encontro el Action Map '{cheatActionMapName}' en el asset.");
            return;
        }

        // buscamos los respectivos eventos
        cheatDamage = cheatMap.FindAction("CheatDamage", true);
        cheatArmor = cheatMap.FindAction("CheatArmor", true);
        cheatSpeed = cheatMap.FindAction("CheatSpeed", true);
        cheatDisableTriggers = cheatMap.FindAction("CheatDisableTriggers", true);

        // nos subscribimos a los eventos
        cheatDamage.performed += _ => SetPlayerDamage(cheatDamageValue);
        cheatArmor.performed += _ => SetPlayerArmor(cheatArmorValue);
        cheatSpeed.performed += _ => SetPlayerSpeed(cheatSpeedValue);
        cheatDisableTriggers.performed += _ => DisableTriggers();

       // se activan los cheats
        cheatMap.Enable();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        Debug.Log("[CheatManager] Cheats activados (Editor/Dev Build).");
    }

    private void OnDestroy()
    {
        if (cheatMap == null) return;

        cheatDamage.performed -= _ => SetPlayerDamage(cheatDamageValue);
        cheatArmor.performed -= _ => SetPlayerArmor(cheatArmorValue);
        cheatSpeed.performed -= _ => SetPlayerSpeed(cheatSpeedValue);
        cheatDisableTriggers.performed -= _ => DisableTriggers();

        cheatMap.Disable();
    }

    private void SetPlayerDamage(int value)
    {
        //aumentamos damage
        if (gameManager == null || gameManager.Player == null) return;
        var controller = gameManager.Player.GetComponent<PlayerController>();
        controller.SetStats(value, value);
        Debug.Log($"[Cheat] Daño aumentado a {value}");
    }

    private void SetPlayerArmor(int value)
    {
        //aumentamos armor
        if (gameManager == null || gameManager.Player == null) return;
        var health = gameManager.Player.GetComponent<PlayerHealth>();
        health.SetArmor(value);
        Debug.Log($"[Cheat] Armadura aumentada a {value}");
    }

    private void SetPlayerSpeed(float value)
    {
        if (gameManager == null || gameManager.Player == null) return;
        var controller = gameManager.Player.GetComponent<PlayerController>();
        var agent = gameManager.Player.GetComponent<UnityEngine.AI.NavMeshAgent>();

        // aumentamos speed
        var speedField = typeof(PlayerController).GetField("speed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (speedField != null)
            speedField.SetValue(controller, value);

        if (agent != null)
            agent.speed = value;

        Debug.Log($"[Cheat] Velocidad aumentada a {value}");
    }

    private void DisableTriggers()
    {
        if (triggersContainer == null)
        {
            Debug.LogWarning("[Cheat] No se asigno el contenedor de triggers");
            return;
        }

        triggersContainer.SetActive(false);
        Debug.Log("[Cheat] Triggers desactivados");
    }
}
#endif
