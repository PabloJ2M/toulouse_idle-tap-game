using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Economy
{
    using Authentication;
    using Authentication.Components;
    
    [RequireComponent(typeof(EconomyManager))]
    public class EconomySynchronize : MonoBehaviour, IAuthEvents
    {
        private readonly Dictionary<BalanceType, long> _localSnapshotAtSyncStart = new();
        private readonly HashSet<BalanceType> _dirtyBalances = new();
        
        private readonly ScheduleVersion _saveVersion = new();
        private const float CloudSaveDebounceSeconds = 2f;
        
        private EconomyManager _economyManager;
        private bool _isFlushing;
        
        private void Awake() => _economyManager = GetComponent<EconomyManager>();
        private void OnEnable() => EconomyManager.OnBalanceUpdated += HandleBalanceChanged;
        private void OnDisable() => EconomyManager.OnBalanceUpdated -= HandleBalanceChanged;
        private void HandleBalanceChanged(BalanceType type, long newValue)
        {
            _dirtyBalances.Add(type);
            this.Schedule(CloudSaveDebounceSeconds, _saveVersion, EconomyCloudSave);
        }

        public void OnSignIn() => _ = StartSynchronization();
        public void OnSignOut() => _economyManager?.ClearBalance();

        private async Awaitable StartSynchronization()
        {
            _localSnapshotAtSyncStart.Clear();

            foreach (var kvp in _economyManager.Balances)
                _localSnapshotAtSyncStart[kvp.Key] = kvp.Value;
 
            await LoadEconomyData().Response();
        }
        private async Awaitable LoadEconomyData()
        {
            await EconomyService.Instance.Configuration.SyncConfigurationAsync();
            var result = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
            
            foreach (var balance in result.Balances)
            {
                if (!Enum.TryParse<BalanceType>(balance.CurrencyId, out var type)) continue;

                var baseLine = _localSnapshotAtSyncStart.GetValueOrDefault(type, 0);
                var localDelta = _economyManager?.GetBalance(type) ?? 0 - baseLine;
                
                _economyManager?.SetBalanceID(type, balance.Balance + localDelta);
            }
        }
        private async Awaitable PendingSaveAsync()
        {
            if (_isFlushing || _dirtyBalances.Count == 0 || !AuthenticationService.Instance.IsSignedIn) return;
            _isFlushing = true;
            
            var toFlush = new List<BalanceType>(_dirtyBalances);
            _dirtyBalances.Clear();

            foreach (var balance in toFlush)
            {
                bool success = await EconomyService.Instance.PlayerBalances
                    .SetBalanceAsync(balance.ToString(), _economyManager.GetBalance(balance)).Response();
                
                if (!success)
                    _dirtyBalances.Add(balance);
            }
            
            _isFlushing = false;
        }
        
        private void OnApplicationPause(bool paused)
        {
            if (paused)
                _saveVersion.Next();

            EconomyCloudSave();
        }
        private void OnApplicationQuit()
        {
            _saveVersion.Next();
            EconomyCloudSave();
        }
 
        [ContextMenu("Force Save Cloud")]
        public void EconomyCloudSave() => _ = PendingSaveAsync();
    }
}