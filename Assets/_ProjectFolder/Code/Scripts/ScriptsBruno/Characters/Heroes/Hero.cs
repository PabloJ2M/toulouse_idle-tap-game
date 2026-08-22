using UnityEngine;

public class Hero : MonoBehaviour
{
    [SerializeField] private float hp;
    [SerializeField] private float atk;
    [SerializeField] private float def;
    public void SetStats(float hp, float atk, float def)
    {
        this.hp = hp;
        this.atk = atk;
        this.def = def;
    }
    public void ReciveDamage(float amount)
    {
        float totalDamage = def - amount;
        if (totalDamage > 0) hp -= totalDamage;
    }
    public float Attack() => atk;
    public float GetTotalAttack() => atk;
    public bool IsAlive() => hp > 0;
}