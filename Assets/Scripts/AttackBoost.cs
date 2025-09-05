using UnityEngine;

public class AttackBoost : MonoBehaviour
{
    [SerializeField] private int boostAmount = 2;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.AddAttackBoost(boostAmount);
            Destroy(gameObject); 
        }
    }
}
