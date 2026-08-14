using System;
using Unity.Services.Economy;
using UnityEngine;
public class ChestUpgradeManager : SingletonBasic<ChestUpgradeManager>{

    [SerializeField] private SerializedDictionary<SlotPriceType, long> upgradesLevelSaves; // el nivel
    [SerializeField] private SerializedDictionary<SlotPriceType, SoUpgradeSlots> data;

    private readonly SaveDataDictionary<SlotPriceType, long> saveData = new("chestUpgrades");
    public static event Action<SlotPriceType, SoUpgradeSlots, float> OnChestUpgradesUpdated;

    protected override void Awake()
    {
        base.Awake();
        saveData.Load(ref upgradesLevelSaves.Dictionary);
    }
    public float GetStatByID(SlotPriceType priceID) => IncrementalStat.GetTotalStat(data[priceID], upgradesLevelSaves[priceID]);
    public void SetUpgradeID(SlotPriceType priceID, long level) 
    {
        upgradesLevelSaves[priceID] = level;
        saveData.Save(upgradesLevelSaves.Dictionary);
    }
    private void ModifyUpgradeID(SlotPriceType type, long amount) => SetUpgradeID(type, upgradesLevelSaves[type] + amount);
    public void AddUpgradeID(SlotPriceType type, uint amount) => ModifyUpgradeID(type, amount);
    public void RemoveUpgradeID(SlotPriceType type, uint amount) => ModifyUpgradeID(type, -amount);
    public void ClearUpgrades()
    {
        foreach (var upgrade in upgradesLevelSaves.Keys)
            ClearUpgrades(upgrade);

        saveData.Delete();
        OnUpdateBalance();
    }
    private void ClearUpgrades(SlotPriceType type) => upgradesLevelSaves[type] = 0;    
    private void OnUpdateBalance()
    {
        foreach (var pair in data.Keys)
            OnUpgradeChest(pair);
    }
    private void OnUpgradeChest(SlotPriceType type) => OnChestUpgradesUpdated?.Invoke(type,data[type], data[type].basePrice);
    public void LevelUpChest(SlotPriceType priceID) 
    {
        uint totalPrice = (uint)IncrementalStat.GetTotalPrice(data[priceID], upgradesLevelSaves[priceID]);
        if (totalPrice <= EconomyManager.Instance.GetBalance(BalanceType.GOLD))
        {            
            EconomyManager.Instance.RemoveBalanceID(BalanceType.GOLD, totalPrice);
            upgradesLevelSaves[priceID]++;
            OnChestUpgradesUpdated?.Invoke(priceID, data[priceID], (uint)IncrementalStat.GetTotalPrice(data[priceID], upgradesLevelSaves[priceID]));
            SetUpgradeID(priceID, upgradesLevelSaves[priceID]);
        }
        else print("No hay dinero suficiente");
    }
    public void UpdateLevel(SlotPriceType priceID) => OnChestUpgradesUpdated?.Invoke(priceID, data[priceID], IncrementalStat.GetTotalPrice(data[priceID], upgradesLevelSaves[priceID]));    
}