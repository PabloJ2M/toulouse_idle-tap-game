using System;
using System.Collections.Generic;
using Unity.Services.Economy;
using UnityEngine;

public class ChestUpgradeManager : SingletonBasic<ChestUpgradeManager>{

    [SerializeField] private SerializedDictionary<SlotPriceType, long> upgradesSaves; // el nivel

    private readonly SaveDataDictionary<SlotPriceType, long> saveData = new("chestUpgrades");
    public static event Action<SlotPriceType, long> OnChestUpgradesUpdated;

    protected override void Awake()
    {
        base.Awake();
        saveData.Load(ref upgradesSaves.Dictionary);
    }
    public void SetUpgradeID(SlotPriceType priceID, long level) 
    {
        upgradesSaves[priceID] = level;
        OnChestUpgradesUpdated?.Invoke(priceID, level);
        saveData.Save(upgradesSaves.Dictionary);
        // guardado
    }
    private void ModifyUpgradeID(SlotPriceType type, long amount) => SetUpgradeID(type, upgradesSaves[type] + amount);
    public void AddUpgradeID(SlotPriceType type, uint amount) => ModifyUpgradeID(type, amount);
    public void RemoveUpgradeID(SlotPriceType type, uint amount) => ModifyUpgradeID(type, -amount);

    public void LevelUpChest(SlotPriceType priceID, UpgradeSlotDetails slot) 
    {
        if (slot.totalPrice <= EconomyManager.Instance.GetBalance(BalanceType.GOLD))
        {
            EconomyManager.Instance.RemoveBalanceID(BalanceType.GOLD, (uint)slot.totalPrice);
            upgradesSaves[priceID] = IncrementalStat.LevelUp(slot, upgradesSaves[priceID]);
            OnChestUpgradesUpdated?.Invoke(priceID, (long)slot.totalPrice);
            SetUpgradeID(priceID, upgradesSaves[priceID]);
        }
        else print("No hay dinero suficiente");
    }
    public void UpdateLevel(UpgradeSlotDetails slot, SlotPriceType priceID)
    {
        IncrementalStat.UpdateLevel(slot, upgradesSaves[priceID]);
        OnChestUpgradesUpdated?.Invoke(priceID, (long)slot.totalPrice);
    }
}
