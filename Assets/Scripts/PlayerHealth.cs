using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 150;
    [SerializeField] private int armor = 3;

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(damage - armor, 0);
        currentHealth -= finalDamage;

        Debug.Log($"Player recibio {finalDamage} de daño (HP restante: {currentHealth})");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("El jugador murio.");

        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            Debug.LogWarning("No se encontro GameManager en la escena.");
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player se curo {amount}. HP actual: {currentHealth}");
    }
}