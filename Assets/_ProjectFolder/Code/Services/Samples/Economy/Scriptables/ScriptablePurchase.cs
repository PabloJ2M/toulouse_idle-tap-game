using UnityEngine;

namespace Unity.Services.Economy.Samples
{
    public abstract class ScriptablePurchase : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private int cost;
        
        public string Id => id;
        public int Cost => cost;

        private void Reset() => id = string.IsNullOrEmpty(id) ? Random.Range(10000, 99999).ToString() : id;
    }
}