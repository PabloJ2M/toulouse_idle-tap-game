using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.CloudSave.Samples
{
    using Models;
    using Models.Data.Player;
    
    public class CloudSaveManager : CloudSaveModule
    {
        private readonly SaveDataDictionary<string, DateTime> _syncState = new("cloud-save-sync-state");
        private Dictionary<string, DateTime> _lastSyncedModified = new();
        
        public static event Action<string, Item> OnCloudSaveFetch;
        public static event Action OnCloudSaveClear;
        
        private void Awake() => _syncState.Load(ref _lastSyncedModified);

        public override void OnUserSignedIn(string playerID)
        {
            base.OnUserSignedIn(playerID);
            _ = SyncCloudData();
        }
        public override void OnUserSignedOut()
        {
            OnCloudSaveClear?.Invoke();
            _lastSyncedModified.Clear();
            _syncState.Delete();
        }

        private async Awaitable SyncCloudData()
        {
            await LoadChangedDataAsync();
            await LoadChangedDataAsync(CloudSaveExtensions.PublicRead);
        }
        private async Awaitable LoadChangedDataAsync(PublicReadAccessClassOptions options = null)
        {
            var keyList = await CloudSaveService.Instance.LoadKeys(options);
            if (keyList == null) return;
            
            var changedKeys = new HashSet<string>();
            
            foreach (var itemKey in keyList) {
                if (!_lastSyncedModified.TryGetValue(itemKey.Key, out var dateTime) || itemKey.Modified > dateTime)
                    changedKeys.Add(itemKey.Key);
            }
            
            if (changedKeys.Count != 0)
                await LoadDataAsync(changedKeys, DispatchPlayerData, options);
        }

        private void DispatchPlayerData(ItemsWrapper items)
        {
            foreach (var kvp in items) {
                OnCloudSaveFetch?.Invoke(kvp.Key, kvp.Value);
                _lastSyncedModified[kvp.Key] = kvp.Value.Modified ?? DateTime.MinValue;
            }
            
            if (items.Count > 0)
                _syncState.Save(_lastSyncedModified);
        }
    }
}