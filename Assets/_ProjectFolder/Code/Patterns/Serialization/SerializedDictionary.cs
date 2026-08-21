using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();

    public void OnBeforeSerialize()
    {
        if (Count == 0 && keys.Count > 0) return;

        keys.Clear();
        values.Clear();
        
        foreach (var kvp in this)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }
    public void OnAfterDeserialize()
    {
        Clear();
        
        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
        {
            if (keys[i] is null) continue;

            if (!ContainsKey(keys[i]))
                Add(keys[i], values[i]);
        }
    }

    public void Parse(Dictionary<TKey, TValue> dictionary)
    {
        Clear();

        foreach (var kvp in dictionary)
            Add(kvp.Key, kvp.Value);
    }
}