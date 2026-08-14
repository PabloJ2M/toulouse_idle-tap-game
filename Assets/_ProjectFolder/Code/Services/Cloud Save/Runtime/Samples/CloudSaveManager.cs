using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.CloudSave
{
    using Authentication.Components;
    using Models;
    using Models.Data.Player;

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
            await LoadChangedDataAsync().Response();
            await LoadChangedDataAsync(CloudSaveExtensions.PublicRead).Response();
        }
        private async Awaitable LoadAllDataAsync(PublicReadAccessClassOptions options = null)
        {
            var dictionary = await CloudSaveService.Instance.LoadAll(options);
            DispatchPlayerData(dictionary);
        }
        private async Awaitable LoadDataAsync(ISet<string> keys, PublicReadAccessClassOptions options = null)
        {
            var dictionary = await CloudSaveService.Instance.Load(keys, options);
            DispatchPlayerData(dictionary);
        }
        private async Awaitable LoadChangedDataAsync(PublicReadAccessClassOptions options = null)
        {
            var keyList = await CloudSaveService.Instance.LoadKeys(options);
            var changedKeys = new HashSet<string>();
            
            foreach (var itemKey in keyList) {
                if (!_lastSyncedModified.TryGetValue(itemKey.Key, out var dateTime) || itemKey.Modified > dateTime)
                    changedKeys.Add(itemKey.Key);
            }
            
            if (changedKeys.Count != 0)
                await LoadDataAsync(changedKeys, options);
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
        
        public static async Awaitable SaveAsync(string key, object value, CloudSaveAccess access = CloudSaveAccess.Private)
        {
            var payload = new Dictionary<string, object> { { key, value } };
            await SaveAsync(payload, access);
        }
        public static async Awaitable SaveAsync(Dictionary<string, object> payload, CloudSaveAccess access = CloudSaveAccess.Private)
        {
            if (access == CloudSaveAccess.Private) await CloudSaveService.Instance.Save(payload);
            else await CloudSaveService.Instance.Save(payload, CloudSaveExtensions.PublicWrite);
        }
    }
}