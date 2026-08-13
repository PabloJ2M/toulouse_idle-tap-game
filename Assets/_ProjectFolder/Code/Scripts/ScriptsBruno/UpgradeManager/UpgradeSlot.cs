using TMPro;
using Unity.Services.Economy;
using UnityEngine;
using UnityEngine.UI;


public class UpgradeSlot : MonoBehaviour
{
    [SerializeField] private SoUpgradeSlots data;
    [SerializeField] private UpgradeSlotDetails slot;
    [SerializeField] private SlotPriceType priceID;

    [Header("Prefab details")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI slotName;
    [SerializeField] private TextMeshProUGUI slotPrice;

    private void Awake() => slot.InitializeVariables(data);
    private void OnEnable()
    {
        ChestUpgradeManager.OnChestUpgradesUpdated += UpdateUI;        
        ChestUpgradeManager.Instance.UpdateLevel(slot, priceID);
    }
    private void OnDisable() => ChestUpgradeManager.OnChestUpgradesUpdated -= UpdateUI;
    private void UpdateUI(SlotPriceType type, long amount) 
    {
        if (type == priceID)
        {
            //image.sprite = data?.Icon;
            slotName.SetText(data.naime);
            slotPrice.SetText(amount.ToShortString());
        }
    }
    public void PayPrice() => ChestUpgradeManager.Instance.LevelUpChest(priceID, slot);
}
