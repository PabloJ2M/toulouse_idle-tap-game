using Unity.Services.Economy;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class StatUtil : SingletonBasic<StatUtil>
{
    [SerializeField] private EconomyModifier gold;
    public void AddGold(SlotID ID) => gold.Add((uint)SlotUpgradeManager.Instance.GetStatByID(ID));
}
