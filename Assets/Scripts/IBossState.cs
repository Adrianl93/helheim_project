using UnityEngine;


    public interface IBossState
    {
        bool IsDead { get; }
        bool IsChasing { get; }
    }

