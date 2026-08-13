using System;
using Unity.Services.Economy;
using UnityEngine;

public class ChestDetection : SingletonBasic<ChestDetection>, IClickable
{
    [SerializeField] private EconomyModifier goldModifier;
    public void click() => goldModifier.Add((uint)ChestUpgradeManager.Instance.GetStatByID(SlotPriceType.Cash));
}
