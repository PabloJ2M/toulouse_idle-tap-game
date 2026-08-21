using UnityEngine;

namespace Unity.Services.Economy.Samples
{
    using Core;
    
    [RequireComponent(typeof(EconomyManager))]
    public class EconomySynchronize : EconomyModule
    {
        private EconomyManager _economyManager;
        private readonly TaskAsync _saveTask = new(kDebounce);

        private void Awake() => _economyManager = GetComponent<EconomyManager>();
        private void OnEnable() => EconomyManager.OnBalanceUpdated += HandleBalanceChanged;
        private void OnDisable() => EconomyManager.OnBalanceUpdated -= HandleBalanceChanged;
        private void HandleBalanceChanged(BalanceType type, long newValue)
        {
            DirtyBalances.Add(type);
            Current[type] = newValue;
            
            if (ServicesStatus.IsSignedIn)
                this.OverrideTask(_saveTask, Save);
        }

        public override void OnUserSignedIn(string playerID)
        {
            base.OnUserSignedIn(playerID);
            _ = StartSynchronization();
        }
        public override void OnUserSignedOut() => _economyManager?.ClearBalance();

        private async Awaitable StartSynchronization()
        {
            Snapshot.Clear();

            foreach (var kvp in _economyManager.Balances)
                Snapshot[kvp.Key] = kvp.Value;
 
            await Load(_economyManager.SetBalanceID);
        }
        
        private void OnApplicationPause(bool paused)
        {
            if (paused)
                _saveTask.Next();

            Save();
        }
        private void OnApplicationQuit()
        {
            _saveTask.Next();
            Save();
        }
    }
}