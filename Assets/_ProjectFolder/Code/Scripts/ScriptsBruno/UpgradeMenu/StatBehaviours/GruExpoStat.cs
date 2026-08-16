using Unity.Mathematics;
using UnityEngine;
public static class GruExpoStat
{
    public static float GetTotalPrice(SoUpgradeSlots slot, long level) => Mathf.Round(slot.basePrice * Mathf.Pow(slot.growthRate, level));
    public static float GetTotalStat(SoUpgradeSlots slot, long level) => Mathf.Round((1f + slot.increment * level) * 100f) / 100f;
}