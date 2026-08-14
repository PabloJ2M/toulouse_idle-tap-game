using TMPro;
using Unity.Services.Economy;
using UnityEngine;
using UnityEngine.UI;


public class UpgradeSlot : MonoBehaviour
{
    [SerializeField] private SlotPriceType priceID;

    [Header("Prefab details")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI slotName;
    [SerializeField] private TextMeshProUGUI slotPrice;

    private void OnEnable()
    {
        ChestUpgradeManager.OnChestUpgradesUpdated += UpdateUI;        
        ChestUpgradeManager.Instance.UpdateLevel(priceID);
    }
    private void OnDisable() => ChestUpgradeManager.OnChestUpgradesUpdated -= UpdateUI;
    private void UpdateUI(SlotPriceType type, SoUpgradeSlots slot, float amount) 
    {
        if (type == priceID)
        {
            slotName.SetText(slot.naime);
            slotPrice.SetText(amount.ToString());
        }
    }
    public void PayPrice() => ChestUpgradeManager.Instance.LevelUpChest(priceID);
}
