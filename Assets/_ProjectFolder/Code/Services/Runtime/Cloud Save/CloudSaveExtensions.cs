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
        
        public static async Awaitable Response(this Task action)
        {
            try { await action; }
            catch (CloudSaveValidationException e) { Debug.LogError(e); }
            catch (CloudSaveRateLimitedException e) { Debug.LogError(e); }
            catch (CloudSaveException e) { Debug.LogError(e); }
        }
        public static async Awaitable Response(this Awaitable action)
        {
            try { await action; }
            catch (CloudSaveValidationException e) { Debug.LogError(e); }
            catch (CloudSaveRateLimitedException e) { Debug.LogError(e); }
            catch (CloudSaveException e) { Debug.LogError(e); }
        }

        public static async Awaitable<ItemsWrapper> LoadAll(this ICloudSaveService service) =>
            await service.Data.Player.LoadAllAsync() as ItemsWrapper;
        
        public static async Awaitable<ItemsWrapper> LoadAll(this ICloudSaveService service, PublicRead read) => read != null
            ? await service.Data.Player.LoadAllAsync(new LoadAllOptions(read)) as ItemsWrapper
            : await service.LoadAll();
        
        public static async Awaitable<ItemsWrapper> LoadAll(this ICloudSaveService service, string playerId) =>
            await service.Data.Player.LoadAllAsync(new LoadAllOptions(new PublicRead(playerId))) as ItemsWrapper;
        
        public static async Awaitable<ItemsWrapper> Load(this ICloudSaveService service, ISet<string> keys) =>
            await service.Data.Player.LoadAsync(keys) as ItemsWrapper;
        
        public static async Awaitable<ItemsWrapper> Load(this ICloudSaveService service, ISet<string> keys, PublicRead read) => read != null
            ? await service.Data.Player.LoadAsync(keys, new LoadOptions(read)) as ItemsWrapper
            : await service.Load(keys);
        
        public static async Awaitable<ItemsWrapper> Load(this ICloudSaveService service, ISet<string> keys, string playerId) =>
            await service.Data.Player.LoadAsync(keys, new LoadOptions(new PublicRead(playerId))) as ItemsWrapper;

        public static async Awaitable<List<ItemKey>> LoadKeys(this ICloudSaveService service) =>
            await service.Data.Player.ListAllKeysAsync();
        
        public static async Awaitable<List<ItemKey>> LoadKeys(this ICloudSaveService service, PublicRead read) => read != null
            ? await service.Data.Player.ListAllKeysAsync(new ListAllKeysOptions(read))
            : await service.LoadKeys();
        
        public static async Awaitable Save(this ICloudSaveService service, Payload payload) =>
            await service.Data.Player.SaveAsync(payload);
        
        public static async Awaitable Save(this ICloudSaveService service, Payload payload, PublicWrite write) =>
            await service.Data.Player.SaveAsync(payload, new CloudSave(write));
    }
}