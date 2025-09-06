using UnityEngine;

public class Projectile : MonoBehaviour
{
    int damage;
    GameObject owner;
    [SerializeField] float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);

        Collider2D projCol = GetComponent<Collider2D>();
        if (projCol != null && owner != null)
        {
            foreach (var c in owner.GetComponentsInChildren<Collider2D>())
                if (c != null)
                    Physics2D.IgnoreCollision(projCol, c);
        }
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
                enemy.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        if (!collision.isTrigger) Destroy(gameObject);
    }
}
