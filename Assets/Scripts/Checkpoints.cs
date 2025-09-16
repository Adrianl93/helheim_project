using UnityEngine;

public class Checkpoints : MonoBehaviour

{
    void Start()
    {
        // animator = GetComponent<Animator>();
    }

    //private Animator animator;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.SaveCheckpoint(gameObject);
        }
    }
}
