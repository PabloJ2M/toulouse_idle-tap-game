using System;
using System.Collections.Generic;
using Unity.Services.Economy;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class ChestUpgradeManager : SingletonBasic<ChestUpgradeManager>
{
    [SerializeField] private List<UpgradeSlotDetails> chestUpgrades;
    private Dictionary<SlotPriceType, UpgradeSlotDetails> upgradeDictionary;

    private readonly SaveDataDictionary<SlotPriceType, UpgradeSlotDetails> saveData = new("chestUpgrades");
    public static event Action<SlotPriceType, long> OnChestUpgradesUpdated;

    protected override void Awake()
    {
        base.Awake();
        upgradeDictionary = new();

        foreach (var entry in chestUpgrades)        
            upgradeDictionary.Add(entry.priceType, entry);

        saveData.Load(ref upgradeDictionary);
    }
    public UpgradeSlotDetails GetUpgrade(SlotPriceType type) => upgradeDictionary.TryGetValue(type, out var value) ? value : null;
    public void SetUpgradeID(SlotPriceType type, long amount) 
    {
        upgradeDictionary[type].basePrice = amount;
        OnChestUpgradesUpdated?.Invoke(type, amount);
        saveData.Save(upgradeDictionary);
        // guardado
    }
    private void ModifyUpgradeID(SlotPriceType type, long amount) => SetUpgradeID(type, GetUpgrade(type).basePrice + amount);
    public void AddUpgradeID(SlotPriceType type, uint amount) => ModifyUpgradeID(type, amount);
    public void RemoveUpgradeID(SlotPriceType type, uint amount) => ModifyUpgradeID(type, -amount);

    public void LevelUpChest(SlotPriceType type) 
    {
        if (upgradeDictionary[type].totalPrice <= EconomyManager.Instance.GetBalance(BalanceType.GOLD))
        {
            EconomyManager.Instance.RemoveBalanceID(BalanceType.GOLD, (uint)upgradeDictionary[type].totalPrice);
            IncrementalStat.LevelUp(upgradeDictionary[type]);
            OnChestUpgradesUpdated?.Invoke(type, (uint)upgradeDictionary[type].totalPrice);
        }
        else print("No hay dinero suficiente");
    }
}
