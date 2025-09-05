using UnityEngine;

public class PlayerInventory : MonoBehaviour
{

    [SerializeField] private int coins = 0;


    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"[Inventory] Monedas actuales: {coins}");
    }

  
    public int GetCoins()
    {
        return coins;
    }
}
