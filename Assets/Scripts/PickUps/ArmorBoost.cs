using UnityEngine;

public class ArmorBoost : MonoBehaviour
{
    [SerializeField] private int armorIncrease = 2;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;

    [Header("Popup")]
    [SerializeField] private GameObject armorPopup;
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 2f, 0f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            int finalArmor = playerHealth.AddArmor(armorIncrease);
            Debug.Log($"{other.name} recogió ArmorBoost. Armadura actual: {finalArmor}");

            // Pop up de armadura
            if (armorPopup != null)
            {
                Vector3 spawnPos = transform.position + popupOffset;
                GameObject popup = Instantiate(armorPopup, spawnPos, Quaternion.identity);
                PopupUI popupScript = popup.GetComponentInChildren<PopupUI>();
                if (popupScript != null)
                {
                    popupScript.Setup("+" + armorIncrease);
                }
            }

            // sonido
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
            }

            Destroy(gameObject);
        }
    }
}
