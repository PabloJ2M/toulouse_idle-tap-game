using UnityEngine;
public static class GruExpoStat
{
    public static float GetTotalPrice(SoUpgradeSlots slot, long level) => slot.basePrice * Mathf.Pow(slot.growthRate, level);
    public static float GetTotalStat(SoUpgradeSlots slot, long level) => 1 + slot.increment * level;
}