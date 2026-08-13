using System;
using UnityEngine;
using UnityEngine.UI;
[Serializable]
public class UpgradeSlotDetails
{
    public string naime;
    public string upgradesDetails;

    public long baseStat;
    public long totalStat;
    public long basePrice;
    public long totalPrice;

    public int level;
    public int baseIncrement;
    public int totalIncrement;

    public SlotPriceType priceType;
    public UpgradeStatType valueType;
    public Sprite Icon; 
}
