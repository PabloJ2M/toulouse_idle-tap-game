using System.Collections.Generic;

namespace UnityEngine.Pool
{
    public abstract class ScriptablePoolListDictionary<TKey, TValue> : ScriptablePoolListBase, ISerializationCallbackReceiver
    {
        [SerializeField] protected List<TKey> keys;
        [SerializeField] protected List<TValue> values;
        
        protected readonly Dictionary<TKey, TValue> Dictionary = new();
        
        #region Dictionary Validation
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Dictionary.Clear();

            var length = Mathf.Min(keys.Count, values.Count);
            for (var i = 0; i < length; i++) {
                Dictionary.Add(keys[i], values[i]);
            }
        }
        #endregion
    }
}