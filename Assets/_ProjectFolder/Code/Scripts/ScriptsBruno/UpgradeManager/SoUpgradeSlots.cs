using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "SoUpgradeSlots", menuName = "Scriptable Objects/SoUpgradeSlots")]
public class SoUpgradeSlots : ScriptableObject
{
    public string naime;
    public string upgradesDetails;

    public long baseStat;
    public float totalStat;
    public long basePrice;
    public float totalPrice;

    public float growthRate;
    public int increment;

    public Sprite Icon;
}
