using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Economy
{
    public static class EconomyExtensions
    {
        public static async Awaitable<bool> Response(this Task action)
        {
            try { await action; return true; }
            catch (EconomyValidationException e) { Debug.LogError(e); }
            catch (EconomyRateLimitedException e) { Debug.LogError(e); }
            catch (EconomyException e) { Debug.LogError(e); }
            return false;
        }
        
        public static async Awaitable<bool> Response(this Awaitable action)
        {
            try { await action; return true; }
            catch (EconomyValidationException e) { Debug.LogError(e); }
            catch (EconomyRateLimitedException e) { Debug.LogError(e); }
            catch (EconomyException e) { Debug.LogError(e); }
            return false;
        }
 
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