using System;
using Unity.Services.Economy;
using Unity.Services.Economy.Samples;
using UnityEngine;

public class SlotUpgradeManager : SingletonBasic<SlotUpgradeManager>{

    [SerializeField] private SerializedDictionary<SlotID, long> upgradesLevelSaves; // el nivel
    [SerializeField] private SerializedDictionary<SlotID, SoUpgradeSlots> data;

    private readonly SaveDataDictionary<SlotID, long> saveData = new("chestUpgrades");
    public static event Action<SlotID, SoUpgradeSlots, float> OnChestUpgradesUpdated;

    protected override void Awake()
    {
        base.Awake();
        OnLoadSaves();
    }
    private void OnLoadSaves()
    {
        var saves = upgradesLevelSaves.Parse();
        saveData.Load(ref saves);
    }
    public float GetStat(SlotID slotID, bool nextLevel = false) => GetStatByID(slotID, nextLevel);
    public float GetPrice(SlotID slotID) => GetPriceByID(slotID);
    public void SetUpgradeID(SlotID slotID, long level) 
    {
        upgradesLevelSaves[slotID] = level;
        saveData.Save(upgradesLevelSaves);
    }
    private void ModifyUpgradeID(SlotID slotID, long amount) => SetUpgradeID(slotID, upgradesLevelSaves[slotID] + amount);
    public void AddUpgradeID(SlotID slotID, uint amount) => ModifyUpgradeID(slotID, amount);
    public void RemoveUpgradeID(SlotID slotID, uint amount) => ModifyUpgradeID(slotID, -amount);
    public void ClearUpgrades()
    {
        foreach (var upgrade in upgradesLevelSaves.Keys)
            ClearUpgrades(upgrade);

        saveData.Delete();
        OnUpdateBalance();
    }
    private void ClearUpgrades(SlotID slotID) => upgradesLevelSaves[slotID] = 0;    
    private void OnUpdateBalance()
    {
        foreach (var pair in data.Keys)
            OnUpgradeChest(pair);
    }
    private void OnUpgradeChest(SlotID slotID) => OnChestUpgradesUpdated?.Invoke(slotID,data[slotID], data[slotID].basePrice);
    public void LevelUpChest(SlotID slotID) 
    {
        uint totalPrice = (uint)IncrementalStat.GetTotalPrice(data[slotID], upgradesLevelSaves[slotID]);
        if (totalPrice <= EconomyManager.Instance.GetBalance(BalanceType.GOLD))
        {            
            EconomyManager.Instance.RemoveBalanceID(BalanceType.GOLD, totalPrice);
            upgradesLevelSaves[slotID]++;
            SetUpgradeID(slotID, upgradesLevelSaves[slotID]);
            OnChestUpgradesUpdated?.Invoke(slotID, data[slotID], (uint)IncrementalStat.GetTotalPrice(data[slotID], upgradesLevelSaves[slotID]));
        }
        else print("No hay dinero suficiente");
    }
    private float GetStatByID(SlotID slotID, bool nextLevel = false) 
    {
        long level;
        if (!nextLevel) level = upgradesLevelSaves[slotID];
        else level = upgradesLevelSaves[slotID] + 1;

        return data[slotID].statType switch
        {
            StatType.Incremental => IncrementalStat.GetTotalStat(data[slotID], level),
            StatType.GruExponential => GruExpoStat.GetTotalStat(data[slotID], level),
            _ => 0
        };
    }
    private uint GetPriceByID(SlotID slotID) => (uint)IncrementalStat.GetTotalPrice(data[slotID], upgradesLevelSaves[slotID]);
    public void UpdateLevel(SlotID priceID) => OnChestUpgradesUpdated?.Invoke(priceID, data[priceID], IncrementalStat.GetTotalPrice(data[priceID], upgradesLevelSaves[priceID]));    
}