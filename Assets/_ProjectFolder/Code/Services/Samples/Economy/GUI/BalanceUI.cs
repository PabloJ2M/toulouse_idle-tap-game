using UnityEngine;
using TMPro;

namespace Unity.Services.Economy
{
    public class BalanceUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textUI;
        [SerializeField] private BalanceType balanceID = BalanceType.GOLD;

        private void OnEnable()
        {
            EconomyManager.OnBalanceUpdated += OnUpdateBalance;
            OnUpdateBalance(balanceID, EconomyManager.Instance.GetBalance(balanceID));
        }
        private void OnDisable() => EconomyManager.OnBalanceUpdated -= OnUpdateBalance;

        private void OnUpdateBalance(BalanceType type, long amount)
        {
            if (type == balanceID)
                textUI.SetText(amount.ToShortString());
        }
    }
}