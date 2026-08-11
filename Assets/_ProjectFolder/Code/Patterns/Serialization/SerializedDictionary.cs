using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializedDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();

    public Dictionary<TKey, TValue> Dictionary = new();

    public int Count => Dictionary.Count;
    public List<TKey> Keys => keys;
    public List<TValue> Values => values;

    public TValue this[TKey key]
    {
        get => Dictionary[key];
        set => Dictionary[key] = value;
    }

    public void Add(TKey key, TValue value) => Dictionary.Add(key, value);
    public bool Remove(TKey key) => Dictionary.Remove(key);
    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
    public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);
    public void Clear() => Dictionary.Clear();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        
        foreach (var kvp in Dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }
    public void OnAfterDeserialize()
    {
        Dictionary.Clear();
        
        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
        {
            if (!Dictionary.ContainsKey(keys[i]))
                Dictionary.Add(keys[i], values[i]);
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}