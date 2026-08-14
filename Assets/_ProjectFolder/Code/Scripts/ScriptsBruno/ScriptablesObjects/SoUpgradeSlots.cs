using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "SoUpgradeSlots", menuName = "Scriptable Objects/SoUpgradeSlots")]
public class SoUpgradeSlots : ScriptableObject
{
    public string naime;
    public string upgradesDetails;

    public long baseStat;
    public long basePrice;

    public StatType statType;
    public float growthRate;
    public float increment;

    public Sprite Icon;
}
