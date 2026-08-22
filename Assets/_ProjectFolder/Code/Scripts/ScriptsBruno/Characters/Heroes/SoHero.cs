using UnityEngine;
[CreateAssetMenu(fileName = "SoEnemy", menuName = "Scriptable Objects/SoHero")]
public class SoHero : ScriptableObject
{
    public string defaultName;
    public int hp;
    public int atk;
    public int def;
    public BuffID pasive;
}
