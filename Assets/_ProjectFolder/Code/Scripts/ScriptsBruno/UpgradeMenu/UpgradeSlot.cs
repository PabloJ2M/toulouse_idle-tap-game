using TMPro;
using Unity.Services.Economy;
using UnityEngine;
using UnityEngine.UI;


public class UpgradeSlot : MonoBehaviour
{
    [SerializeField] private SlotID priceID;

    [Header("Prefab details")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI slotName;
    [SerializeField] private TextMeshProUGUI slotPrice;
    [SerializeField] private Button button;

    private void OnEnable()
    {
        SlotUpgradeManager.OnChestUpgradesUpdated += UpdateUI;        
        SlotUpgradeManager.Instance.UpdateLevel(priceID);
        EconomyManager.OnBalanceUpdated += EvaluatePrice;
    }
    private void OnDisable()
    {
        SlotUpgradeManager.OnChestUpgradesUpdated -= UpdateUI;
        EconomyManager.OnBalanceUpdated -= EvaluatePrice;
    }
    private void UpdateUI(SlotID type, SoUpgradeSlots slot, float amount) 
    {
        if (type == priceID)
        {
            slotName.SetText($"{slot.naime}\n {GetDetails(slot)}");
            slotPrice.SetText($"${amount}");
        }
    }
    private string GetDetails(SoUpgradeSlots slot) => $"{slot.startDetails}{SlotUpgradeManager.Instance.GetStat(priceID)}{slot.finalDetails} > {slot.startDetails}{SlotUpgradeManager.Instance.GetStat(priceID, true)}{slot.finalDetails}";
    private void EvaluatePrice(BalanceType balance, long amount) 
    {
        if (balance == BalanceType.GOLD) 
        {
            if (amount > SlotUpgradeManager.Instance.GetPrice(priceID)) 
                button.interactable = true;
            else
                button.interactable = false;
        }
    }
    public void PayPrice() => SlotUpgradeManager.Instance.LevelUpChest(priceID);
}
