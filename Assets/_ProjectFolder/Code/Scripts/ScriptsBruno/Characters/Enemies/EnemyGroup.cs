using UnityEngine;
using System.Collections.Generic;
public class EnemyGroup : MonoBehaviour
{
    [SerializeField] private List<Enemy> enemies;
    private Queue<Enemy> lastAttacker;
    private int enemyOrder = 0;
    public void Add(Enemy enemy) => enemies.Add(enemy);
    public void Remove(Enemy enemy)
    {
        enemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }
    public void AttackHero(Hero hero) 
    {
        var enemy = enemies[GetEnemyOrder()];
        enemy.EnemyAttack(hero);
        lastAttacker.Enqueue(enemy);        
    }
    private int GetEnemyOrder() 
    {
        if (enemyOrder >= enemies.Count) enemyOrder = 0;
        var order = enemyOrder;
        enemyOrder++;
        return order;      
    }
    public void CheckEnemiesAlive() 
    {
        if (enemies.Count > 0)
        foreach (var enemy in enemies) 
        {
            if (!enemy.IsAlive())
                Remove(enemy);
        }
    }
    public bool IsEnemiesAlive() 
    {
        if (enemies.Count <= 0) return false;
        foreach (var enemy in enemies) 
        {
            if (enemy.IsAlive())
                return true;
        }
        return false;
    }
    public Enemy GetLastAttacker() => lastAttacker.TryDequeue(out Enemy result) ? result : null;
}
