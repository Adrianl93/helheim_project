using UnityEngine;
using System.Collections; 
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    
    [SerializeField] private int maxHealth = 150;
    public int MaxHealth => maxHealth;

    [SerializeField] private int armor = 3;
    public int Armor => armor;

    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;

    
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float deathSoundVolume = 1f;
    private Animator animator;
    private PlayerController playerController;

    
    [SerializeField] private float deathDelay = 4f; 

    private bool isDead = false;

    private void OnEnable()
    {
        GameManager.OnTimeout += Die;
    }

    private void OnDisable()
    {
        GameManager.OnTimeout -= Die;
    }

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        int finalDamage = Mathf.Max(damage - armor, 0);
        currentHealth -= finalDamage;

        Debug.Log($"Player recibi� {finalDamage} de da�o (HP restante: {currentHealth})");

        if (currentHealth <= 0)
        {
            StartCoroutine(HandleDeath());
        }
    }

    private IEnumerator HandleDeath()
    {
        if (isDead) yield break;
        isDead = true;

        Debug.Log("El jugador muri�.");

        
        if (playerController != null)
            playerController.enabled = false;

        
        if (animator != null)
            animator.SetTrigger("Die");

        
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathSoundVolume);

        
        yield return new WaitForSeconds(deathDelay);

       
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartScene();
        }
        else
        {
            Debug.LogWarning("No se encontr� GameManager en la escena.");
        }
    }

    private void Die()
    {
        
        StartCoroutine(HandleDeath());
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player se cur� {amount}. HP actual: {currentHealth}");
    }

    public int AddArmor(int amount)
    {
        armor += amount;
        return armor;
    }

    public void SetArmor(int value)
    {
        armor = value;
    }
}
