using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Economy
{
    using Authentication;
    using Authentication.Components;
    
    [RequireComponent(typeof(EconomyManager))]
    public class EconomySynchronize : MonoBehaviour, IAuthEvents
    {
        private readonly Dictionary<BalanceType, long> _delta = new();
        private readonly ScheduleVersion _saveVersion = new();
        private const float CloudSaveDebounceSeconds = 2f;
        
        private EconomyManager _economyManager;
        private bool _isSyncing, _isFlushing;
        
        private void Awake() => _economyManager = GetComponent<EconomyManager>();
        private void OnEnable() => EconomyManager.OnBalanceUpdated += HandleBalanceChanged;
        private void OnDisable() => EconomyManager.OnBalanceUpdated -= HandleBalanceChanged;
        private void HandleBalanceChanged(BalanceType type, long delta)
        {
            if (_isSyncing)
                _delta[type] = _delta.GetValueOrDefault(type, 0) + delta;
 
            this.Schedule(CloudSaveDebounceSeconds, _saveVersion, () => _ = FlushPendingSaveAsync());
        }

        public void OnSignIn() => _ = StartSynchronization();
        public void OnSignOut() => _economyManager.ClearBalance();

        private async Awaitable StartSynchronization()
        {
            _isSyncing = true;
            _delta.Clear();
 
            await LoadEconomyData().Response();
            _isSyncing = false;
        }
        private async Task FlushPendingSaveAsync()
        {
            if (_isFlushing || !AuthenticationService.Instance.IsSignedIn) return;
            _isFlushing = true;
            
            foreach (var kvp in _economyManager.Balances)
                await EconomyService.Instance.PlayerBalances.SetBalanceAsync(kvp.Key.ToString(), kvp.Value).Response();
            
            _isFlushing = false;
        }
        private async Task LoadEconomyData()
        {
            await EconomyService.Instance.Configuration.SyncConfigurationAsync();
            var result = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
            
            foreach (var balance in result.Balances)
            {
                if (!Enum.TryParse<BalanceType>(balance.CurrencyId, out var type)) continue;
 
                var delta = _delta.GetValueOrDefault(type, 0);
                _economyManager?.SetBalanceID(type, balance.Balance + delta);
            }
        }
        
        private void OnApplicationPause(bool paused)
        {
            if (!paused) return;
            _saveVersion.Next();
            _ = FlushPendingSaveAsync();
        }
        private void OnApplicationQuit()
        {
            _saveVersion.Next();
            _ = FlushPendingSaveAsync();
        }
 
        [ContextMenu("Force Save Cloud")]
        public void ForceCloudSaveNow() => _ = FlushPendingSaveAsync();
    }
}