using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int coinValue = 1; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddCoins(coinValue);
            Debug.Log($"{other.name} recogió {coinValue} moneda(s). Total: {inventory.GetCoins()}");
            Destroy(gameObject); 
        }
    }
}
