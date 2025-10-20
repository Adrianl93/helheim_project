using UnityEngine;

public class AttackBoost : MonoBehaviour
{
    [SerializeField] private int boostAmount = 2;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.AddAttackBoost(boostAmount);
            Debug.Log($"{player.name} recogió AttackBoost. Daño melee: {player.MeleeDamage}, daño ranged: {player.RangedDamage}");

    
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
            }

            Destroy(gameObject);
        }
    }
}
