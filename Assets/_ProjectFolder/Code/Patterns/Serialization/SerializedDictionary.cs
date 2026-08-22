using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();

    public void OnBeforeSerialize()
    {
        if (keys.Count != Count) return;

        keys.Clear();
        values.Clear();
        
        foreach (var kvp in this) {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }
    public void OnAfterDeserialize()
    {
        Clear();

        var length = Mathf.Min(keys.Count, values.Count);
        for (var i = 0; i < length; i++) {
            if (keys[i] is not null && !ContainsKey(keys[i]))
                this[keys[i]] = values[i];
        }
    }

    public Dictionary<TKey, TValue> Parse() => this;
    public void Parse(Dictionary<TKey, TValue> dictionary)
    {
        var entries = new List<KeyValuePair<TKey, TValue>>(dictionary);
        
        Clear();
        keys.Clear();
        values.Clear();

        foreach (var kvp in entries) {
            this[kvp.Key] = kvp.Value;
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }
}