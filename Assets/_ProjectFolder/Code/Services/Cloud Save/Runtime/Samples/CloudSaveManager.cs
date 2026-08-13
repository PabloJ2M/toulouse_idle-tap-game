using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.CloudSave
{
    using Authentication.Components;
    using Models;
    using Models.Data.Player;
    using CloudSaveOptions = Models.Data.Player.SaveOptions;

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
            // if (_lastSyncedModified.Count == 0) {
            //     await LoadAllDataAsync().Response();
            //     return;
            // }
            
            await LoadChangedDataAsync().Response();
            await LoadChangedDataAsync(new PublicReadAccessClassOptions()).Response();
        }

        private async Awaitable LoadAllDataAsync()
        {
            var dictionary = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
            DispatchPlayerData(dictionary);
        }
        private async Awaitable LoadChangedDataAsync(PublicReadAccessClassOptions options = null)
        {
            var keyList = options == null
                ? await CloudSaveService.Instance.Data.Player.ListAllKeysAsync()
                : await CloudSaveService.Instance.Data.Player.ListAllKeysAsync(new ListAllKeysOptions(options));
            
            var changedKeys = new HashSet<string>();
            
            foreach (var itemKey in keyList) {
                if (!_lastSyncedModified.TryGetValue(itemKey.Key, out var dateTime) || itemKey.Modified > dateTime)
                    changedKeys.Add(itemKey.Key);
            }
            
            if (changedKeys.Count != 0)
                await LoadKeysDataAsync(changedKeys, options);
        }
        private async Awaitable LoadKeysDataAsync(ISet<string> keys, PublicReadAccessClassOptions options = null)
        {
            var dictionary = options == null
                ? await CloudSaveService.Instance.Data.Player.LoadAsync(keys)
                : await CloudSaveService.Instance.Data.Player.LoadAsync(keys, new LoadOptions(options));
            
            DispatchPlayerData(dictionary);
        }
        
        // public static async Awaitable<Dictionary<string, Item>> LoadAllPlayerDataAsync(string playerId) =>
        //     await CloudSaveService.Instance.Data.Player.LoadAllAsync(new LoadAllOptions(new PublicReadAccessClassOptions(playerId))());
        public static async Awaitable<Dictionary<string, Item>> LoadPlayerDataAsync(string playerId, ISet<string> keys) =>
            await CloudSaveService.Instance.Data.Player.LoadAsync(keys, new LoadOptions(new PublicReadAccessClassOptions(playerId)));

        private void DispatchPlayerData(Dictionary<string, Item> dictionary)
        {
            foreach (var kvp in dictionary) {
                OnCloudSaveFetch?.Invoke(kvp.Key, kvp.Value);
                _lastSyncedModified[kvp.Key] = kvp.Value.Modified ?? DateTime.MinValue;
            }
            
            if (dictionary.Count > 0)
                _syncState.Save(_lastSyncedModified);
        }

        public static async Awaitable SaveDataAsync(string key, object value, CloudSaveAccess access = CloudSaveAccess.Private)
        {
            var payload = new Dictionary<string, object> { { key, value } };
            await SaveDataAsync(payload, access);
        }
        public static async Awaitable SaveDataAsync(Dictionary<string, object> payload, CloudSaveAccess access = CloudSaveAccess.Private)
        {
            if (access == CloudSaveAccess.Private)
                await CloudSaveService.Instance.Data.Player.SaveAsync(payload);
            else
                await CloudSaveService.Instance.Data.Player.SaveAsync(payload, new CloudSaveOptions(new PublicWriteAccessClassOptions()));
        }
    }
}