using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.CloudSave
{
    using Models;
    using Models.Data.Player;
    using CloudSave = Models.Data.Player.SaveOptions;
    using PublicRead = Models.Data.Player.PublicReadAccessClassOptions;
    using PublicWrite = Models.Data.Player.PublicWriteAccessClassOptions;
    
    public static class CloudSaveExtensions
    {
        public static readonly PublicWrite PublicWrite = new();
        public static readonly PublicRead PublicRead = new();
        
        private static async Awaitable<T> Response<T>(this Task<T> action) where T : class
        {
            try { return await action; }
            catch (CloudSaveValidationException e) { Debug.LogError(e); }
            catch (CloudSaveRateLimitedException e) { Debug.LogError(e); }
            catch (CloudSaveException e) { Debug.LogError(e); }
            return null;
        }
        public static async Awaitable<T> Response<T>(this Awaitable<T> action) where T : class
        {
            try { return await action; }
            catch (CloudSaveValidationException e) { Debug.LogError(e); }
            catch (CloudSaveRateLimitedException e) { Debug.LogError(e); }
            catch (CloudSaveException e) { Debug.LogError(e); }
            return null;
        }

        public static async Awaitable<ItemsWrapper> LoadAll(this ICloudSaveService service) =>
            await service.Data.Player.LoadAllAsync().Response() as ItemsWrapper;
        
        public static async Awaitable<ItemsWrapper> LoadAll(this ICloudSaveService service, PublicRead read) => read != null
            ? await service.Data.Player.LoadAllAsync(new LoadAllOptions(read)).Response() as ItemsWrapper
            : await service.LoadAll();
        
        public static async Awaitable<ItemsWrapper> LoadAll(this ICloudSaveService service, string playerId) =>
            await service.Data.Player.LoadAllAsync(new LoadAllOptions(new PublicRead(playerId))).Response() as ItemsWrapper;
        
        public static async Awaitable<ItemsWrapper> Load(this ICloudSaveService service, ISet<string> keys) =>
            await service.Data.Player.LoadAsync(keys).Response() as ItemsWrapper;
        
        public static async Awaitable<ItemsWrapper> Load(this ICloudSaveService service, ISet<string> keys, PublicRead read) => read != null
            ? await service.Data.Player.LoadAsync(keys, new LoadOptions(read)).Response() as ItemsWrapper
            : await service.Load(keys);
        
        public static async Awaitable<ItemsWrapper> Load(this ICloudSaveService service, ISet<string> keys, string playerId) =>
            await service.Data.Player.LoadAsync(keys, new LoadOptions(new PublicRead(playerId))).Response() as ItemsWrapper;

        public static async Awaitable<List<ItemKey>> LoadKeys(this ICloudSaveService service) =>
            await service.Data.Player.ListAllKeysAsync().Response();
        
        public static async Awaitable<List<ItemKey>> LoadKeys(this ICloudSaveService service, PublicRead read) => read != null
            ? await service.Data.Player.ListAllKeysAsync(new ListAllKeysOptions(read)).Response()
            : await service.LoadKeys();
        
        public static async Awaitable Save(this ICloudSaveService service, Payload payload) =>
            await service.Data.Player.SaveAsync(payload).Response();
        
        public static async Awaitable Save(this ICloudSaveService service, Payload payload, PublicWrite write) =>
            await service.Data.Player.SaveAsync(payload, new CloudSave(write)).Response();
    }
}