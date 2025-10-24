using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [SerializeField] private int healAmount = 50;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float volume = 1f;

    [Header("Popup")]
    [SerializeField] private GameObject itemPopupPrefab; 
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 2f, 0f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);

            // Pop up de curación
            if (itemPopupPrefab != null)
            {
                Vector3 spawnPos = transform.position + popupOffset;
                GameObject popup = Instantiate(itemPopupPrefab, spawnPos, Quaternion.identity);
                PopupUI popupScript = popup.GetComponentInChildren<PopupUI>();
                if (popupScript != null)
                {
                    popupScript.Setup("+" + healAmount);
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
