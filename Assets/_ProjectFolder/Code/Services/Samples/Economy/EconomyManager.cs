using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Economy.Samples
{
    using Core;
    
    public class EconomyManager : ServicesStatic<EconomyManager>
    {
        [SerializeField] private SerializedDictionary<BalanceType, long> balances;
        public IReadOnlyDictionary<BalanceType, long> Balances => balances;
        
        private readonly SaveDataDictionary<BalanceType, long> saveData = new("currency");
        public static event Action<BalanceType, long> OnBalanceUpdated;
        
        protected override void Awake()
        {
            base.Awake();
            OnLoadLocalData();
            OnUpdateBalance();
        }
        
        public long GetBalance(BalanceType type) => balances.GetValueOrDefault(type, 0);
        public void SetBalanceID(BalanceType type, long amount)
        {
            balances[type] = amount;
            OnBalanceUpdated?.Invoke(type, balances[type]);
            saveData.Save(balances);
        }
        public void AddBalanceID(BalanceType type, uint amount) => ModifyBalanceID(type, amount);
        public void RemoveBalanceID(BalanceType type, uint amount) => ModifyBalanceID(type, -amount);
        private void ModifyBalanceID(BalanceType type, long amount) => SetBalanceID(type, GetBalance(type) + amount);

        private void OnLoadLocalData()
        {
            var dictionary = balances.Parse();
            saveData.Load(ref dictionary);
        }
        private void OnUpdateBalance()
        {
            foreach (var pair in balances)
                OnUpdateBalance(pair.Key);
        }
        private void OnUpdateBalance(BalanceType type) => OnBalanceUpdated?.Invoke(type, GetBalance(type));
        
        public void ClearBalance()
        {
            foreach (var balance in Balances.Keys)
                ClearBalance(balance);
 
            saveData.Delete();
            OnUpdateBalance();
        }
        private void ClearBalance(BalanceType type) => balances[type] = 0;
    }
}