using UnityEngine;

[CreateAssetMenu(fileName = "SoEnemy", menuName = "Scriptable Objects/SoEnemy")]
public class SoEnemy : ScriptableObject
{
    public string defaultName;
    public int hp;
    public int atk;
    public int def;
    public EnemyID enemyID;
}
