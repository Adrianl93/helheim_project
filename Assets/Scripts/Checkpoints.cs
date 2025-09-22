using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    void Start()
    {
        // animator = GetComponent<Animator>();
    }

    [SerializeField] private AudioClip activateSound;  
    [SerializeField] private float volume = 1f;        
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.SaveCheckpoint(gameObject);

            //private Animator animator;

            // Reproducir sonido al activar
            if (activateSound != null)
            {
                AudioSource.PlayClipAtPoint(activateSound, transform.position, volume);
            }

          

            Debug.Log($"Checkpoint activado: {gameObject.name}");
        }
    }
}
