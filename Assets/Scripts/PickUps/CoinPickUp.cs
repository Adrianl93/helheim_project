using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float volume = 1f;

    [Header("Popup")]
    [SerializeField] private GameObject coinPopup; 
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 2f, 0f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController inventory = other.GetComponent<PlayerController>();
        if (inventory != null)
        {
            inventory.AddCoins(coinValue);
            Debug.Log($"{other.name} recogió {coinValue} moneda(s). Total: {inventory.Coins}");

            // Pop up de moneda
            if (coinPopup != null)
            {
                Vector3 spawnPos = transform.position + popupOffset;
                GameObject popup = Instantiate(coinPopup, spawnPos, Quaternion.identity);
                PopupUI popupScript = popup.GetComponentInChildren<PopupUI>();
                if (popupScript != null)
                {
                    popupScript.Setup("+" + coinValue );
                }
            }

            // sonido
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
            }

            Destroy(gameObject);
        }
    }
}
