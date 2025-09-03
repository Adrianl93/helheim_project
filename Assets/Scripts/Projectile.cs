using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float lifeTime = 3f;

    private void Start()
    {

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyController enemy = collision.GetComponent<EnemyController>();
        if (enemy != null)
        {

            int finalDamage = Mathf.Max(damage - enemy.Armor, 0);

            enemy.TakeDamage(finalDamage);


            Debug.Log($"Proyectil impacto en {enemy.name}, daño: {finalDamage}, HP restante: {enemy.CurrentHealth}");

            Destroy(gameObject);
        }
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

}
