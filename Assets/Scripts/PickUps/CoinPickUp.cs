using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    [SerializeField] private AudioClip pickupSound; 
    [SerializeField] private float volume = 1f;     

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController inventory = other.GetComponent<PlayerController>();
        if (inventory != null)
        {
            inventory.AddCoins(coinValue);
            Debug.Log($"{other.name} recogió {coinValue} moneda(s). Total: {inventory.Coins}");

            
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
            }

            Destroy(gameObject);
        }
    }
}
