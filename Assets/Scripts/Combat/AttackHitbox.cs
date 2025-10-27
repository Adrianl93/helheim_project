using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private int damage;
    private float lifetime = 0.2f;
    private LayerMask enemyLayer;
    private Vector2 attackOrigin;

    public void Initialize(int dmg, LayerMask targetLayer, Vector2 origin)
    {
        damage = dmg;
        enemyLayer = targetLayer;
        attackOrigin = origin;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(damage, attackOrigin);
        }
    }
}
