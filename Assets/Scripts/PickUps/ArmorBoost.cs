using UnityEngine;

public class ArmorBoost : MonoBehaviour
{
    [SerializeField] private int armorIncrease = 2;
    [SerializeField] private AudioClip pickupSound; 
    [SerializeField] private float soundVolume = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            int finalArmor = playerHealth.AddArmor(armorIncrease);
            Debug.Log($"{other.name} recogió ArmorBoost. Armadura actual: {finalArmor}");

            
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
            }

            Destroy(gameObject);
        }
    }
}
