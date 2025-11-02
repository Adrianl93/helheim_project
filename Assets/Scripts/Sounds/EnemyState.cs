using UnityEngine;

public class EnemyState : MonoBehaviour
{
    public interface IEnemyState
    {
        bool IsChasing { get; }
        bool IsAttacking { get; }
    }
}
