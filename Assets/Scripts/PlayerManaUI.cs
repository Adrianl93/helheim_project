using UnityEngine;
using UnityEngine.UI;

public class PlayerManaUI : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private Image manaFillImage;
    [SerializeField] private Image manaBackground;

    void Update()
    {
        if (player == null || manaFillImage == null) return;

        float fillValue = (float)player.CurrentMana / player.MaxMana;
        manaFillImage.fillAmount = Mathf.Clamp01(fillValue);
    }
}
