using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private int damage;
    private float lifetime = 0.2f;
    private LayerMask enemyLayer;

    public void Initialize(int dmg, LayerMask targetLayer)
    {
        damage = dmg;
        enemyLayer = targetLayer;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }
    }
}
