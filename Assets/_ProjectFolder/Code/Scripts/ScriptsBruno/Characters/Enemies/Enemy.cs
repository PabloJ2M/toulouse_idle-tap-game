using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyID enemyID;

    [SerializeField] private float hp;
    [SerializeField] private float atk;
    [SerializeField] private float def;
    public void SetStats(SoEnemy details) 
    {
        hp = details.hp;
        atk = details.atk;
        def = details.def;
        enemyID = details.enemyID;
    }
    public EnemyID EnemyID { get {  return enemyID; } }
    public void EnemyAttack(Hero hero) => hero.ReciveDamage(atk); // calculadora de danio + buffs
    public void ReciveDamage(float amount)
    {
        float totalDamage = def - amount;
        if (totalDamage > 0) hp -= totalDamage;
        else print("se bloqueo todo el danio");
    }
    public bool IsAlive() => hp > 0;
}
