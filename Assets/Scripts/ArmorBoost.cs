using UnityEngine;

public class ArmorBoost : MonoBehaviour
{
    [SerializeField] private int armorIncrease = 2;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            int finalArmor = playerHealth.AddArmor(armorIncrease);
            Debug.Log($"{other.name} recogió ArmorBoost. Armadura actual: {finalArmor}");
            Destroy(gameObject);
        }
    }
}
