using UnityEngine;

public class Hero : MonoBehaviour
{
    [SerializeField] private int hp;
    [SerializeField] private int atk;
    [SerializeField] private int def;
    public void SetStats(int hp, int atk, int def)
    {
        this.hp = hp;
        this.atk = atk;
        this.def = def;
    }
    public void ReciveDamage(int amount)
    {
        int totalDamage = def - amount;
        if (totalDamage > 0) hp -= totalDamage;
    }
    public bool IsAlive() => hp > 0;
}