using UnityEngine;

public abstract class BasicPickup : MonoBehaviour
{


    [SerializeField] protected int value = 10; 
    [SerializeField] private AudioClip pickupSound;

    protected abstract void ApplyEffect(GameObject player);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyEffect(other.gameObject);

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject);
        }
    }
}
