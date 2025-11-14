#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System;

public class CheatManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject triggersContainer;

    [Header("Valores Cheat (Incrementos)")]
    [SerializeField] private int bonusDamage = 50;
    [SerializeField] private int bonusArmor = 50;
    [SerializeField] private float bonusSpeed = 25f;
    [SerializeField] private int bonusMana = 50;

    [Header("Input")]
    [SerializeField] private InputActionAsset cheatInputAsset;
    [SerializeField] private string cheatActionMapName = "Cheats";

    private InputActionMap cheatMap;
    private InputAction cheatDamageAction;
    private InputAction cheatArmorAction;
    private InputAction cheatSpeedAction;
    private InputAction cheatTriggerAction;
    private InputAction cheatRangedAction;
    private InputAction cheatManaAction;

    // Estados de cheats toggle
    private bool damageCheatActive = false;
    private bool armorCheatActive = false;
    private bool speedCheatActive = false;
    private bool triggersDisabled = false;

    // Para restaurar valores originales
    private int baseMelee;
    private int baseRanged;
    private int baseArmor;
    private float baseSpeed;
    private float baseAgentSpeed;

    // Guardamos handlers para desuscribir correctamente
    private Action<InputAction.CallbackContext> damageHandler;
    private Action<InputAction.CallbackContext> armorHandler;
    private Action<InputAction.CallbackContext> speedHandler;
    private Action<InputAction.CallbackContext> triggerHandler;
    private Action<InputAction.CallbackContext> rangedHandler;
    private Action<InputAction.CallbackContext> manaHandler;

    private void Awake()
    {
        // validaciones básicas
        if (cheatInputAsset == null)
        {
            Debug.LogWarning("[CheatManager] No se asignó InputActionAsset en el inspector. Asignalo para que los cheats funcionen.");
            return;
        }

        // encontrá el map
        cheatMap = cheatInputAsset.FindActionMap(cheatActionMapName, true);
        if (cheatMap == null)
        {
            Debug.LogWarning($"[CheatManager] No se encontró ActionMap '{cheatActionMapName}' en el asset. Verificá el nombre exacto.");
            return;
        }

     
        cheatDamageAction = cheatMap.FindAction("CheatDamage", false);
        cheatArmorAction = cheatMap.FindAction("CheatArmor", false);
        cheatSpeedAction = cheatMap.FindAction("CheatSpeed", false);
        cheatTriggerAction = cheatMap.FindAction("CheatDisableTriggers", false);
        cheatRangedAction = cheatMap.FindAction("CheatRanged", false);
        cheatManaAction = cheatMap.FindAction("CheatMana", false);

        bool anyMissing = false;
        if (cheatDamageAction == null) { Debug.LogWarning("[CheatManager] Falta action 'CheatDamage' en el ActionMap."); anyMissing = true; }
        if (cheatArmorAction == null) { Debug.LogWarning("[CheatManager] Falta action 'CheatArmor' en el ActionMap."); anyMissing = true; }
        if (cheatSpeedAction == null) { Debug.LogWarning("[CheatManager] Falta action 'CheatSpeed' en el ActionMap."); anyMissing = true; }
        if (cheatTriggerAction == null) { Debug.LogWarning("[CheatManager] Falta action 'CheatDisableTriggers' en el ActionMap."); anyMissing = true; }
        if (cheatRangedAction == null) { Debug.LogWarning("[CheatManager] Falta action 'CheatUnlockRanged' en el ActionMap."); anyMissing = true; }
        if (cheatManaAction == null) { Debug.LogWarning("[CheatManager] Falta action 'CheatAddMana' en el ActionMap."); anyMissing = true; }

        if (anyMissing)
        {
            Debug.LogWarning("[CheatManager] Algunas actions faltan. Revisá el Input Actions asset. Se seguirán registrando las que existan.");
        }

        // guardamos gameManager si no está seteado
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        

        // registramos handlers sólo si no son null
        if (cheatDamageAction != null)
        {
            damageHandler = ctx => ToggleDamageCheat();
            cheatDamageAction.performed += damageHandler;
        }
        if (cheatArmorAction != null)
        {
            armorHandler = ctx => ToggleArmorCheat();
            cheatArmorAction.performed += armorHandler;
        }
        if (cheatSpeedAction != null)
        {
            speedHandler = ctx => ToggleSpeedCheat();
            cheatSpeedAction.performed += speedHandler;
        }
        if (cheatTriggerAction != null)
        {
            triggerHandler = ctx => ToggleTriggers();
            cheatTriggerAction.performed += triggerHandler;
        }
        if (cheatRangedAction != null)
        {
            rangedHandler = ctx => UnlockRanged();
            cheatRangedAction.performed += rangedHandler;
        }
        if (cheatManaAction != null)
        {
            manaHandler = ctx => AddMana();
            cheatManaAction.performed += manaHandler;
        }

        // habilitamos el map
        cheatMap.Enable();
        Debug.Log("[CheatManager] Cheats cargados y action map habilitado.");
    }
    private void Start()
    {
        CacheBaseStats();
    }

    private void OnDestroy()
    {
        // desuscribimos con seguridad
        if (cheatDamageAction != null && damageHandler != null) cheatDamageAction.performed -= damageHandler;
        if (cheatArmorAction != null && armorHandler != null) cheatArmorAction.performed -= armorHandler;
        if (cheatSpeedAction != null && speedHandler != null) cheatSpeedAction.performed -= speedHandler;
        if (cheatTriggerAction != null && triggerHandler != null) cheatTriggerAction.performed -= triggerHandler;
        if (cheatRangedAction != null && rangedHandler != null) cheatRangedAction.performed -= rangedHandler;
        if (cheatManaAction != null && manaHandler != null) cheatManaAction.performed -= manaHandler;

        if (cheatMap != null) cheatMap.Disable();
    }

    
    // Guardamos los valores base del Player

    private void CacheBaseStats()
    {
        if (gameManager == null || gameManager.Player == null) return;

        var pc = gameManager.Player.GetComponent<PlayerController>();
        var ph = gameManager.Player.GetComponent<PlayerHealth>();
        var agent = gameManager.Player.GetComponent<NavMeshAgent>();

        if (pc != null)
        {
            baseMelee = pc.MeleeDamage;
            baseRanged = pc.RangedDamage;
            var f = typeof(PlayerController).GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null) baseSpeed = (float)f.GetValue(pc);
        }

        if (ph != null)
            baseArmor = ph.Armor;

        if (agent != null)
            baseAgentSpeed = agent.speed;
    }

    
    // CHEAT DAMAGE (toggle)
    
    private void ToggleDamageCheat()
    {
        if (gameManager == null || gameManager.Player == null) { Debug.LogWarning("[Cheat] No hay Player asignado en GameManager."); return; }

        var pc = gameManager.Player.GetComponent<PlayerController>();
        if (pc == null) { Debug.LogWarning("[Cheat] PlayerController no encontrado."); return; }

        damageCheatActive = !damageCheatActive;

        if (damageCheatActive)
        {
            pc.AddAttack(bonusDamage);
            Debug.Log($"[Cheat] +{bonusDamage} Damage ACTIVADO");
        }
        else
        {
            pc.AddAttack(-bonusDamage);
            Debug.Log($"[Cheat] -{bonusDamage} Damage DESACTIVADO");
        }
    }

    
    // CHEAT ARMOR (toggle)
    
    private void ToggleArmorCheat()
    {
        if (gameManager == null || gameManager.Player == null) { Debug.LogWarning("[Cheat] No hay Player asignado en GameManager."); return; }

        var ph = gameManager.Player.GetComponent<PlayerHealth>();
        if (ph == null) { Debug.LogWarning("[Cheat] PlayerHealth no encontrado."); return; }

        armorCheatActive = !armorCheatActive;

        if (armorCheatActive)
        {
            ph.AddArmor(bonusArmor);
            Debug.Log($"[Cheat] +{bonusArmor} Armor ACTIVADO");
        }
        else
        {
            ph.AddArmor(-bonusArmor);
            Debug.Log($"[Cheat] -{bonusArmor} Armor DESACTIVADO");
        }
    }

   
    // CHEAT SPEED (toggle)
    
    private void ToggleSpeedCheat()
    {
        if (gameManager == null || gameManager.Player == null)
        {
            Debug.LogWarning("[Cheat] No hay Player asignado en GameManager.");
            return;
        }

        var pc = gameManager.Player.GetComponent<PlayerController>();
        var agent = gameManager.Player.GetComponent<NavMeshAgent>();

        if (pc == null)
        {
            Debug.LogWarning("[Cheat] PlayerController no encontrado.");
            return;
        }

        speedCheatActive = !speedCheatActive;

        var speedField = typeof(PlayerController)
            .GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (speedField == null)
        {
            Debug.LogWarning("[Cheat] No se encontró el campo privado 'speed'.");
            return;
        }

        float currentSpeed = (float)speedField.GetValue(pc);

        if (speedCheatActive)
        {
            speedField.SetValue(pc, currentSpeed + bonusSpeed);
            if (agent != null) agent.speed += bonusSpeed;

            Debug.Log($"[Cheat] +{bonusSpeed} Speed ACTIVADO");
        }
        else
        {
            speedField.SetValue(pc, currentSpeed - bonusSpeed);
            if (agent != null) agent.speed -= bonusSpeed;

            Debug.Log("[Cheat] Speed restaurado (desactivado)");
        }
    }

   
    // CHEAT TRIGGERS (toggle)
  
    private void ToggleTriggers()
    {
        if (triggersContainer == null) { Debug.LogWarning("[Cheat] triggersContainer no asignado."); return; }

        triggersDisabled = !triggersDisabled;
        triggersContainer.SetActive(!triggersDisabled);

        Debug.Log(triggersDisabled ?
            "[Cheat] Triggers DESACTIVADOS" :
            "[Cheat] Triggers RESTAURADOS");
    }

   
    // CHEAT: UNLOCK ATAQUE A DISTANCIA
    
    private void UnlockRanged()
    {
        if (gameManager == null) { Debug.LogWarning("[Cheat] GameManager no asignado."); return; }

        gameManager.TriggerRangedUnlocked();
        Debug.Log("[Cheat] Ataque a distancia ACTIVADO");
    }

    
    // CHEAT: + MANA
    
    private void AddMana()
    {
        if (gameManager == null || gameManager.Player == null) { Debug.LogWarning("[Cheat] No hay Player asignado en GameManager."); return; }

        var pc = gameManager.Player.GetComponent<PlayerController>();
        if (pc == null) { Debug.LogWarning("[Cheat] PlayerController no encontrado."); return; }

        pc.AddMana(bonusMana);
        Debug.Log($"[Cheat] +{bonusMana} Mana ACTIVADO");
    }
}
#endif
