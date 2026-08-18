using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyID enemyID;

    [SerializeField] private int hp;
    [SerializeField] private int atk;
    [SerializeField] private int def;
    public void SetStats(SoEnemy details) 
    {
        hp = details.hp;
        atk = details.atk;
        def = details.def;
        enemyID = details.enemyID;
    }
    public EnemyID EnemyID { get {  return enemyID; } }
    public void EnemyAttack(Hero hero) => hero.ReciveDamage(10); // calculadora de danio + buffs
    public void ReciveDamage(int amount)
    {
        int totalDamage = def - amount;
        if (totalDamage > 0) hp -= totalDamage;
    }
}
