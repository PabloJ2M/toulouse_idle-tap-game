using System;
using System.Collections.Generic;
using Unity.Services.Economy;
using UnityEngine;

public class ChestUpgradeManager : SingletonBasic<ChestUpgradeManager>{

    [SerializeField] private SerializedDictionary<SlotPriceType, long> upgradesLevelSaves; // el nivel

    [SerializeField] private SerializedDictionary<SlotPriceType, float> statsSaves;
    private readonly SaveDataDictionary<SlotPriceType, long> saveData = new("chestUpgrades");
    public static event Action<SlotPriceType, long> OnChestUpgradesUpdated;

    protected override void Awake()
    {
        base.Awake();
        saveData.Load(ref upgradesLevelSaves.Dictionary);
    }
    public float GetStatByID(SlotPriceType priceID) => statsSaves[priceID];
    public void SetUpgradeID(SlotPriceType priceID, long level) 
    {
        upgradesLevelSaves[priceID] = level;
        saveData.Save(upgradesLevelSaves.Dictionary);
        // guardado
    }
    private void ModifyUpgradeID(SlotPriceType type, long amount) => SetUpgradeID(type, upgradesLevelSaves[type] + amount);
    public void AddUpgradeID(SlotPriceType type, uint amount) => ModifyUpgradeID(type, amount);
    public void RemoveUpgradeID(SlotPriceType type, uint amount) => ModifyUpgradeID(type, -amount);

    public void ClearUpgrades()
    {
        foreach (var upgrade in upgradesLevelSaves.Keys)
            ClearUpgrades(upgrade);

        saveData.Delete();
        //OnUpdateBalance();
    }
    private void ClearUpgrades(SlotPriceType type)
    {
        statsSaves[type] = IncrementalStat.GetBaseStat(statsSaves[type], upgradesLevelSaves[type]);
        upgradesLevelSaves[type] = 0;
    }

    //private void OnUpdateBalance()
    //{
    //    foreach (var pair in upgradesLevelSaves)
    //        OnUpgradeChest(pair.Key);
    //}
    //private void OnUpgradeChest(SlotPriceType type) => OnChestUpgradesUpdated?.Invoke(type, upgradesLevelSaves[type]);

    public void LevelUpChest(SlotPriceType priceID, UpgradeSlotDetails slot) 
    {
        if (slot.totalPrice <= EconomyManager.Instance.GetBalance(BalanceType.GOLD))
        {
            EconomyManager.Instance.RemoveBalanceID(BalanceType.GOLD, (uint)slot.totalPrice);
            upgradesLevelSaves[priceID] = IncrementalStat.LevelUp(slot, upgradesLevelSaves[priceID]);
            OnChestUpgradesUpdated?.Invoke(priceID, (long)slot.totalPrice);
            SetUpgradeID(priceID, upgradesLevelSaves[priceID]);
            statsSaves[priceID] = slot.totalStat;
        }
        else print("No hay dinero suficiente");
    }
    public void UpdateLevel(UpgradeSlotDetails slot, SlotPriceType priceID)
    {
        IncrementalStat.LoadLevel(slot, upgradesLevelSaves[priceID]);        
        OnChestUpgradesUpdated?.Invoke(priceID, (long)slot.totalPrice);
    }


}
