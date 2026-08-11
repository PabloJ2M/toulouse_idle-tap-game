using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Economy
{
    public class EconomyManager : SaveLocalData<SerializedDictionary<BalanceType, long>>
    {
        [SerializeField] private SerializedDictionary<BalanceType, long> balances;
        protected override string LocalDataID => "currency";

        public IReadOnlyDictionary<BalanceType, long> Balances => balances.Dictionary;
        public Func<SerializedDictionary<BalanceType, long>> OnSyncData;

        public static EconomyManager Instance { get; private set; }
        public static event Action<BalanceType, long> OnBalanceUpdated;
        
        private void Awake()
        {
            Instance = this;
            LoadData(ref balances);

            foreach (var pair in balances)
                ForceUpdateBalance(pair.Key);
        }
        
        public void ApplyCloudBalances(IReadOnlyDictionary<BalanceType, long> resolvedBalances)
        {
            foreach (var pair in resolvedBalances) {
                balances[pair.Key] = pair.Value;
                OnBalanceUpdated?.Invoke(pair.Key, pair.Value);
            }
            
            SaveData(balances);
        }
        public void ClearAllBalances()
        {
            foreach (var key in balances.Keys)
                balances[key] = 0;
 
            DeleteData();
        }
        
        public void ForceUpdateBalance(BalanceType type) => OnBalanceUpdated?.Invoke(type, GetBalance(type));
        public void AddBalanceID(BalanceType type, uint amount) => ModifyBalanceID(type, amount);
        public void RemoveBalanceID(BalanceType type, uint amount) => ModifyBalanceID(type, -amount);
        
        public long GetBalance(BalanceType type) => balances.TryGetValue(type, out var value) ? value : 0;
        private void ModifyBalanceID(BalanceType type, long amount)
        {
            balances[type] = GetBalance(type) + amount;
            OnBalanceUpdated?.Invoke(type, balances[type]);
            SaveData(balances);
        }
    }
}