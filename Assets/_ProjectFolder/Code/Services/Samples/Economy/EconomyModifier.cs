using UnityEngine;

namespace Unity.Services.Economy.Samples
{
    public class EconomyModifier : MonoBehaviour
    {
        [SerializeField] private BalanceType balanceType;
        
        private EconomyManager _manager;

        private void Awake() => _manager = EconomyManager.Instance;

        public void Add(uint amount) => _manager.AddBalanceID(balanceType, amount);
        public void Remove(uint amount) => _manager.RemoveBalanceID(balanceType, amount);
    }
}