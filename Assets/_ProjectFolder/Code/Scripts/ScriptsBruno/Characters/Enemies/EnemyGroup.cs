using UnityEngine;
using System.Collections.Generic;
public class EnemyGroup : MonoBehaviour
{
    [SerializeField] private List<Enemy> enemies;
    private Queue<Enemy> lastAttacker;
    public void Add(Enemy enemy) => enemies.Add(enemy);
    public void Remove(Enemy enemy) => enemies.Remove(enemy);
    public void AttackEnemy(EnemyID enemyID, Hero hero) 
    {
        if (enemies.Exists(x => x.EnemyID == enemyID))
        {
            var enemy = enemies.Find(x => x.EnemyID == enemyID);
            enemy.EnemyAttack(hero);
            lastAttacker.Enqueue(enemy);
        }
        else print("No se encontró al enemigo");
    }
    public Enemy GetLastAttacker() => lastAttacker.TryDequeue(out Enemy result) ? result : null;
}
