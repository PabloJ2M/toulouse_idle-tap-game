using UnityEngine;

public static class IncrementalStat
{
    public static void LevelUp(UpgradeSlotDetails slot) 
    {
        var level = slot.level + 1;
        slot.totalStat = slot.baseStat + (slot.baseIncrement * level);
        slot.totalPrice = slot.basePrice * (long)Mathf.Pow(2f, level);
        slot.level++;
    }
    public static void LevelDown(UpgradeSlotDetails slot)
    {
        var level = slot.level - 1;
        slot.totalStat = slot.baseStat + (slot.baseIncrement * level);
        slot.totalPrice = slot.basePrice * (long)Mathf.Pow(1.2f, level);
        slot.level--;
    }
}
