using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFillImage;  
    [SerializeField] private Image healthBackground;  
    void Update()
    {
        if (playerHealth == null || healthFillImage == null) return;

        float fillValue = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
        healthFillImage.fillAmount = Mathf.Clamp01(fillValue);
    }
}
