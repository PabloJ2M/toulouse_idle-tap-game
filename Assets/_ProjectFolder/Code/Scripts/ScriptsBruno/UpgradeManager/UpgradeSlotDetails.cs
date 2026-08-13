using System;
using UnityEngine;
[Serializable]
public class UpgradeSlotDetails
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
    public void InitializeVariables(SoUpgradeSlots data)
    {
        naime = data.naime;
        upgradesDetails = data.upgradesDetails;
        baseStat = data.baseStat;
        totalStat = data.totalStat;
        basePrice = data.basePrice;
        totalPrice = data.totalPrice;
        increment = data.increment;
        Icon = data.Icon;
        growthRate = data.growthRate;
    }
}
