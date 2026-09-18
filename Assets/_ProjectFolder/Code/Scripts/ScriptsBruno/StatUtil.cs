using Unity.Services.Economy.Samples;
using UnityEngine;

public class StatUtil : SingletonBasic<StatUtil>
{
    [SerializeField] private EconomyModifier gold;
    
    public void AddGold(SlotID ID) => gold.Add((uint)SlotUpgradeManager.Instance.GetStat(ID));
    public float GetBonusDamage() => SlotUpgradeManager.Instance.GetStat(SlotID.Damage);
    public float GetBonusDefense() => SlotUpgradeManager.Instance.GetStat(SlotID.defense);
    public float GetBonusHealth() => SlotUpgradeManager.Instance.GetStat(SlotID.Health);
    public float GetBonusHeal() => SlotUpgradeManager.Instance.GetStat(SlotID.Heal);
}