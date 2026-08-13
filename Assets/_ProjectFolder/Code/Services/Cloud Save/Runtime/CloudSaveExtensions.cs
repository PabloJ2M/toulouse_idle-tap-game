using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.CloudSave
{
    public static class CloudSaveExtensions
    {
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
    }
}