using UnityEngine;

public class Chest : MonoBehaviour
{
    // Chest.cs (ejemplo)
    [SerializeField] private int chestScore = 100;

    public void OpenChest()
    {
        GameManager.Instance.AddScore(chestScore);
        // lógica de abrir cofre (loot, animación, etc.)
    }

}
