using UnityEngine;
using UnityEngine.Rendering;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRadius = 5.0f;
    [SerializeField] private float speed;
    [SerializeField] private int health = 100;
    [SerializeField] private int damage = 10;
    [SerializeField] private int armor = 3;

    private Rigidbody2D rb; 
    private Vector2 movement;

    void Start()
    {
     rb = GetComponent<Rigidbody2D>();
    }

    
    void Update()
    {
     float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            movement = direction;


        }
        else
        {
            movement = Vector2.zero;
        }
  
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

}
