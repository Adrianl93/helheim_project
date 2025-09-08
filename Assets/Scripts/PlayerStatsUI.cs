using UnityEngine;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInventory playerInventory;

    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text meleeAttackText;
    [SerializeField] private TMP_Text rangedAttackText;
    [SerializeField] private TMP_Text coinsText;

    void Update()
    {
        if (playerHealth != null && armorText != null)
            armorText.text = $"Armor: {playerHealth.Armor}";

        if (playerController != null)
        {
            if (meleeAttackText != null)
                meleeAttackText.text = $"Melee: {playerController.MeleeDamage}";

            if (rangedAttackText != null)
                rangedAttackText.text = $"Ranged: {playerController.RangedDamage}";
        }

        if (playerInventory != null && coinsText != null)
            coinsText.text = $"Coins: {playerInventory.GetCoins()}";
    }
}
