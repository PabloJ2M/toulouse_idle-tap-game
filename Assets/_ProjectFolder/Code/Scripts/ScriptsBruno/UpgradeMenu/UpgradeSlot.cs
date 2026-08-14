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

    private void OnEnable()
    {
        SlotUpgradeManager.OnChestUpgradesUpdated += UpdateUI;        
        SlotUpgradeManager.Instance.UpdateLevel(priceID);
    }
    private void OnDisable() => SlotUpgradeManager.OnChestUpgradesUpdated -= UpdateUI;
    private void UpdateUI(SlotID type, SoUpgradeSlots slot, float amount) 
    {
        if (type == priceID)
        {
            slotName.SetText(slot.naime);
            slotPrice.SetText(amount.ToString());
        }
    }
    public void PayPrice() => SlotUpgradeManager.Instance.LevelUpChest(priceID);
}
