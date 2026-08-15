using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Economy
{
    using Core;
    
    public abstract class EconomyModule : MonoBehaviour, IServiceAuthModule
    {
        private IEconomyService _service;
        
        protected readonly Dictionary<BalanceType, long> Snapshot = new(), Current = new();
        protected readonly HashSet<BalanceType> DirtyBalances = new();
        protected const float kDebounce = 2f;

        private bool _isSaving;
        
        public virtual void OnUserSignedIn(string playerID) => _service = EconomyService.Instance;
        public abstract void OnUserSignedOut();
        
        protected async Awaitable Load(Action<BalanceType, long> onSetBalance)
        {
            if (!ServicesStatus.IsSignedIn) return;
            
            await _service.SyncConfiguration();
            var result = await _service.GetBalances();
            
            foreach (var balance in result.Balances) {
                if (!Enum.TryParse<BalanceType>(balance.CurrencyId, out var type)) continue;

                var start = Snapshot.GetValueOrDefault(type, 0);
                var delta = Current.GetValueOrDefault(type, 0) - start;
                
                onSetBalance(type, balance.Balance + delta);
            }
        }
        private async Awaitable PendingSaveAsync()
        {
            if (_isSaving || DirtyBalances.Count == 0 || !ServicesStatus.IsSignedIn) return;
            _isSaving = true;
            
            var toFlush = new List<BalanceType>(DirtyBalances);
            DirtyBalances.Clear();

            foreach (var balance in toFlush) {
                var success = await _service.SetBalance(balance, Current.GetValueOrDefault(balance, 0));
                
                if (!success)
                    DirtyBalances.Add(balance);
            }
            
            _isSaving = false;
        }
 
        [ContextMenu("Force Save Cloud")]
        public void Save() => _ = PendingSaveAsync();
    }
}