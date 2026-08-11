using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Economy
{
    public class EconomyManager : SingletonBasic<EconomyManager>
    {
        [SerializeField] private SerializedDictionary<BalanceType, long> balances;
        public IReadOnlyDictionary<BalanceType, long> Balances => balances.Dictionary;
        
        private readonly SaveDataDictionary<BalanceType, long> saveData = new("currency");
        public static event Action<BalanceType, long> OnBalanceUpdated;
        
        protected override void Awake()
        {
            base.Awake();
            saveData.Load(ref balances.Dictionary);

            foreach (var pair in balances)
                ForceUpdateBalance(pair.Key);
        }
        
        public void ApplyCloudBalances(IReadOnlyDictionary<BalanceType, long> resolvedBalances)
        {
            foreach (var pair in resolvedBalances) {
                balances[pair.Key] = pair.Value;
                OnBalanceUpdated?.Invoke(pair.Key, pair.Value);
            }
            
            saveData.Save(balances.Dictionary);
        }
        public void ClearAllBalances()
        {
            foreach (var balance in Balances.Keys)
                balances[balance] = 0;
 
            saveData.Delete();
        }
        
        public void AddBalanceID(BalanceType type, uint amount) => ModifyBalanceID(type, amount);
        public void RemoveBalanceID(BalanceType type, uint amount) => ModifyBalanceID(type, -amount);
        private void ForceUpdateBalance(BalanceType type) => OnBalanceUpdated?.Invoke(type, GetBalance(type));
        
        public long GetBalance(BalanceType type) => balances.TryGetValue(type, out var value) ? value : 0;
        private void ModifyBalanceID(BalanceType type, long amount)
        {
            balances[type] = GetBalance(type) + amount;
            OnBalanceUpdated?.Invoke(type, balances[type]);
            saveData.Save(balances.Dictionary);
        }
    }
}