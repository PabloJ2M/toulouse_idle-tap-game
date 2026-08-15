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
            OnUpdateBalance();
        }
        
        public long GetBalance(BalanceType type) => balances.TryGetValue(type, out var value) ? value : 0;
        public void SetBalanceID(BalanceType type, long amount)
        {
            balances[type] = amount;
            OnBalanceUpdated?.Invoke(type, balances[type]);
            saveData.Save(balances.Dictionary);
        }
        public void AddBalanceID(BalanceType type, uint amount) => ModifyBalanceID(type, amount);
        public void RemoveBalanceID(BalanceType type, uint amount) => ModifyBalanceID(type, -amount);
        private void ModifyBalanceID(BalanceType type, long amount) => SetBalanceID(type, GetBalance(type) + amount);

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