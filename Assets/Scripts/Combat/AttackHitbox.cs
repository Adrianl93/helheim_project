using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private int damage;
    private LayerMask targetLayer;
    private Vector2 attackOrigin;
    private GameObject owner;

    public void Initialize(int damage, LayerMask targetLayer, Vector2 attackOrigin, GameObject owner = null)
    {
        this.damage = damage;
        this.targetLayer = targetLayer;
        this.attackOrigin = attackOrigin;
        this.owner = owner;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (owner != null && collision.gameObject == owner) return;
        if (((1 << collision.gameObject.layer) & targetLayer) == 0) return;

        
        EnemyController enemy = collision.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, attackOrigin);
            Destroy(gameObject);
            return;
        }

        
        PlayerHealth player = collision.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
    }
}
