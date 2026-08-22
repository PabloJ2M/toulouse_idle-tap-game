using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Economy
{
    using Model;
    
    public static class EconomyExtensions
    {
        private static async Awaitable<T> Response<T>(this Task<T> action) where T : class
        {
            try { return await action; }
            catch (EconomyValidationException e) { Debug.LogError(e); }
            catch (EconomyRateLimitedException e) { Debug.LogError(e); }
            catch (EconomyException e) { Debug.LogError(e); }
            return null;
        }
        public static async Awaitable<T> Response<T>(this Awaitable<T> action) where T : class
        {
            try { return await action; }
            catch (EconomyValidationException e) { Debug.LogError(e); }
            catch (EconomyRateLimitedException e) { Debug.LogError(e); }
            catch (EconomyException e) { Debug.LogError(e); }
            return null;
        }

        public static async Awaitable SyncConfiguration(this IEconomyService service) =>
            await service.Configuration.SyncConfigurationAsync();
        
        public static async Awaitable<GetBalancesResult> GetBalances(this IEconomyService service) =>
            await service.PlayerBalances.GetBalancesAsync();
        
        public static async Awaitable<PlayerBalance> SetBalance(this IEconomyService service, BalanceType balance, long amount) =>
            await service.PlayerBalances.SetBalanceAsync(balance.ToString(), amount).Response();
        
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T" };
        
        public static string ToShortString(this long amount)
        {
            if (amount < 1000) return amount.ToString();
 
            int magnitude = Mathf.Min((int)Mathf.Log(amount, 1000), Suffixes.Length - 1);
            double shortValue = amount / Mathf.Pow(1000, magnitude);
 
            return $"{shortValue:0.#}{Suffixes[magnitude]}";
        }
    }
}