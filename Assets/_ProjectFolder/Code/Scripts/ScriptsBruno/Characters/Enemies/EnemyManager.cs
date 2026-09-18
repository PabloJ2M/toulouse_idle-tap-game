using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : SingletonBasic<EnemyManager>
{
    [SerializeField] private List<SoEnemy> availableEnemies;
    [SerializeField] private SerializedDictionary<EnemyID, int> totalEnemies;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform container;

    [SerializeField] private EnemyGroup actualGroup;
    protected override void Awake()
    {
        base.Awake();
        totalEnemies.Clear();
    }
    private SoEnemy GetEnemy(EnemyID enemyID)
    {
        if (availableEnemies.Exists(x => x.enemyID == enemyID))
            return availableEnemies.Find(x => x.enemyID == enemyID);
        return null;
    }
    public EnemyGroup GetEnemyGroup() => actualGroup;
    public void AssignManualEnemies(EnemyID enemyID, int amount) => SetEnemies(enemyID, amount);
    public void AssingRandomEnemies() 
    {
        int typeAmount = Random.Range(1, availableEnemies.Count);
        for (int i = 0; i < typeAmount; i++) 
        {
            int enemySelected = Random.Range(1, availableEnemies.Count);
            SetEnemies(availableEnemies[enemySelected].enemyID, 1);
        }
        SpawnEnemies();
    }
    private void SetEnemies(EnemyID enemyID, int amount) => totalEnemies.Add(enemyID, amount);
    private void SpawnEnemies() 
    {
        int con = 0;
        foreach (var item in totalEnemies)
        {
            while (con < item.Value)
            {
                var generatedEnemy = Instantiate(enemyPrefab, container);
                Enemy enemy = generatedEnemy.transform.GetChild(0).GetComponent<Enemy>();
                enemy.SetStats(GetEnemy(item.Key));
                actualGroup.Add(enemy);
                con++;
            }
            con = 0;
        }
    }
}
