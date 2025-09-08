using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 150;
    public int MaxHealth => maxHealth;

    [SerializeField] private int armor = 3;
    public int Armor => armor;
    
    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;




    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(damage - armor, 0);
        currentHealth -= finalDamage;

        Debug.Log($"Player recibi� {finalDamage} de da�o (HP restante: {currentHealth})");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("El jugador muri�.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            Debug.LogWarning("No se encontr� GameManager en la escena.");
        }
    }

 

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player se cur� {amount}. HP actual: {currentHealth}");
    }

  
    public int AddArmor(int amount)
    {
        armor += amount;
        return armor;
    }

   
 
}
