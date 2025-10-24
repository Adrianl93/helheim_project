using UnityEngine;

public class AttackBoost : MonoBehaviour
{
    [SerializeField] private int boostAmount = 2;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;

    [Header("Popup")]
    [SerializeField] private GameObject AttackBoostPopup; 
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 2f, 0f);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.AddAttackBoost(boostAmount);
            Debug.Log($"{player.name} recogió AttackBoost. Daño melee: {player.MeleeDamage}, daño ranged: {player.RangedDamage}");

            // Pop up de boost de ataque
            if (AttackBoostPopup != null)
            {
                Vector3 spawnPos = transform.position + popupOffset;
                GameObject popup = Instantiate(AttackBoostPopup, spawnPos, Quaternion.identity);
                PopupUI popupScript = popup.GetComponentInChildren<PopupUI>();
                if (popupScript != null)
                {
                    popupScript.Setup("+" + boostAmount);
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
