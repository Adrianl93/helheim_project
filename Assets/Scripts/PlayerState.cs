using UnityEngine;

[System.Serializable]
public class PlayerState
{
    public Vector3 position;
    public int meleeAttack;
    public int rangedAttack;
    public int armor;
    public int coins;
    public int mana;
    public float remainingTime;
    public int score; 

    public PlayerState(
        Vector3 pos,
        int melee,
        int ranged,
        int arm,
        int coin,
        int manaValue,
        float time,
        int currentScore
    )
    {
        position = pos;
        meleeAttack = melee;
        rangedAttack = ranged;
        armor = arm;
        coins = coin;
        mana = manaValue;
        remainingTime = time;
        score = currentScore; 
    }
}
