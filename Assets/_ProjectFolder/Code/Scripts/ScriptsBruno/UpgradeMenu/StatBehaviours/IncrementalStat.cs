using UnityEngine;
public static class IncrementalStat
{
    public static float GetTotalPrice(SoUpgradeSlots slot, long level) => Mathf.Round(slot.basePrice * Mathf.Pow(slot.growthRate, level));
    public static float GetTotalStat(SoUpgradeSlots slot, long level) => slot.baseStat + slot.increment * level;
}