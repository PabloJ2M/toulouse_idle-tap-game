using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.CloudSave
{
    using Authentication.Components;
    using Models;

    [RequireComponent(typeof(AuthEventHandler))]
    public class CloudSaveManager : SingletonBasic<CloudSaveManager>, IAuthEvents
    {
        private readonly SaveDataDictionary<string, DateTime> _syncState = new("cloud-save-sync-state");
        private Dictionary<string, DateTime> _lastSyncedModified = new();
        
        public event Action<string, Item> OnCloudSaveFetch;
        public event Action OnCloudSaveClear;

        protected override void Awake()
        {
            base.Awake();
            _syncState.Load(ref _lastSyncedModified);
        }
        
        public void OnSignIn() => _ = SyncCloudData();
        public void OnSignOut()
        {
            OnCloudSaveClear?.Invoke();
            _lastSyncedModified.Clear();
            _syncState.Delete();
        }

        private async Awaitable SyncCloudData()
        {
            if (_lastSyncedModified.Count == 0)
                await LoadAllDataAsync().Response();
            else
                await LoadChangedDataAsync().Response();
        }
        private async Awaitable LoadAllDataAsync()
        {
            var dictionary = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
            DispatchPlayerData(dictionary);
        }
        private async Awaitable LoadChangedDataAsync()
        {
            var keyList = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
            var changedKeys = new HashSet<string>();
            
            foreach (var itemKey in keyList) {
                if (!_lastSyncedModified.TryGetValue(itemKey.Key, out var dateTime) || itemKey.Modified > dateTime)
                    changedKeys.Add(itemKey.Key);
            }
            
            if (changedKeys.Count != 0)
                await LoadKeysDataAsync(changedKeys);
        }
        private async Awaitable LoadKeysDataAsync(ISet<string> keys)
        {
            var dictionary = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
            DispatchPlayerData(dictionary);
        }

        private void DispatchPlayerData(Dictionary<string, Item> dictionary)
        {
            foreach (var kvp in dictionary) {
                OnCloudSaveFetch?.Invoke(kvp.Key, kvp.Value);
                _lastSyncedModified[kvp.Key] = kvp.Value.Modified ?? DateTime.MinValue;
            }
            
            if (dictionary.Count > 0)
                _syncState.Save(_lastSyncedModified);
        }
        
        public static async Awaitable SaveDataAsync(Dictionary<string, object> payload) =>
            await CloudSaveService.Instance.Data.Player.SaveAsync(payload);
        
        public static async Awaitable SaveDataAsync(string key, object value) =>
            await SaveDataAsync(new Dictionary<string, object> { { key, value } });
    }
}