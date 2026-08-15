using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.CloudSave
{
    using Core;
    using Models.Data.Player;
    
    public abstract class CloudSaveModule : MonoBehaviour, IServiceAuthModule
    {
        private static ICloudSaveService _service;
        
        public virtual void OnUserSignedIn(string playerID) => _service = CloudSaveService.Instance;
        public abstract void OnUserSignedOut();
        
        protected static async Awaitable LoadAllDataAsync(Action<ItemsWrapper> onLoadData, PublicReadAccessClassOptions options = null)
        {
            if (!ServicesStatus.IsSignedIn) return;
            
            var dictionary = await _service.LoadAll(options) as ItemsWrapper;
            onLoadData(dictionary);
        }
        protected static async Awaitable LoadDataAsync(ISet<string> keys, Action<ItemsWrapper> onLoadData, PublicReadAccessClassOptions options = null)
        {
            if (!ServicesStatus.IsSignedIn) return;
            
            var dictionary = await _service.Load(keys, options) as ItemsWrapper;
            onLoadData(dictionary);
        }
        
        public static async Awaitable SaveAsync(Payload payload, SaveAccessType accessType = SaveAccessType.Private)
        {
            if (!ServicesStatus.IsSignedIn) return;
            
            if (accessType == SaveAccessType.Private) await _service.Save(payload);
            else await _service.Save(payload, CloudSaveExtensions.PublicWrite);
        }
        public static async Awaitable SaveAsync(string key, object value, SaveAccessType accessType = SaveAccessType.Private)
        {
            if (!ServicesStatus.IsSignedIn) return;
            
            var payload = new Payload { { key, value } };
            await SaveAsync(payload, accessType);
        }
    }
}