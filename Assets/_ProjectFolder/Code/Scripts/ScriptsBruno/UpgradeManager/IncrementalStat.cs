using UnityEngine;

public static class IncrementalStat
{
    public static long LevelUp(UpgradeSlotDetails slot, long level) => ModifyLevel(slot, level+ 1);
    public static long LevelDown(UpgradeSlotDetails slot, long level) => ModifyLevel(slot, level - 1);
    public static long UpdateLevel(UpgradeSlotDetails slot, long level) => ModifyLevel(slot, level);
    public static long ModifyLevel(UpgradeSlotDetails slot, long level) 
    {
        ChangeVariables(slot, level);
        return level;
    }
    private static void ChangeVariables(UpgradeSlotDetails slot, long level) 
    {
        slot.totalStat = GetTotalStat(slot.baseStat, level, slot.increment);
        slot.totalPrice = GetTotalPrice(slot.basePrice, level, slot.growthRate);
    }
    public static float GetTotalPrice(long basePrice, long level, float growthRate) => basePrice * Mathf.Pow(growthRate, level);
    public static float GetTotalStat(long baseStat, long level, float increment) => baseStat + increment * level;
}
