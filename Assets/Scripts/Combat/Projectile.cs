using UnityEngine;

public class Projectile : MonoBehaviour
{
    int damage;
    GameObject owner;
    [SerializeField] float lifetime = 5f;

    [Header("FX de Sonido")]
    [SerializeField] private AudioClip shootSFX;   // Sonido al disparar
    [SerializeField] private AudioClip impactSFX;  // Sonido al impactar
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Destroy(gameObject, lifetime);

        // Ignorar colisión con el dueño
        Collider2D projCol = GetComponent<Collider2D>();
        if (projCol != null && owner != null)
        {
            foreach (var c in owner.GetComponentsInChildren<Collider2D>())
                if (c != null)
                    Physics2D.IgnoreCollision(projCol, c);
        }

        // Reproducir sonido de disparo
        PlaySFX(shootSFX);
    }

    public void SetDamage(int dmg) => damage = dmg;
    public void SetOwner(GameObject o) => owner = o;

    private bool OwnerIsPlayer()
    {
        return owner != null && owner.GetComponent<PlayerController>() != null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == owner) return;

        if (OwnerIsPlayer())
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                int prevHP = enemy.CurrentHealth;
                enemy.TakeDamage(damage);
                int newHP = enemy.CurrentHealth;
                int dmgApplied = prevHP - newHP;

                Debug.Log($"[Proyectil Player] Impactó a {enemy.name}. Daño aplicado: {dmgApplied}. HP enemigo restante: {newHP}");

                PlaySFX(impactSFX);
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                int prevHP = playerHealth.CurrentHealth;
                playerHealth.TakeDamage(damage);
                int newHP = playerHealth.CurrentHealth;
                int dmgApplied = prevHP - newHP;

                Debug.Log($"[Proyectil Enemigo] Impactó al Player. Daño aplicado: {dmgApplied}. HP Player restante: {newHP}");

                PlaySFX(impactSFX);
                Destroy(gameObject);
                return;
            }
        }

        if (!collision.isTrigger)
        {
            PlaySFX(impactSFX);
            Destroy(gameObject);
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
