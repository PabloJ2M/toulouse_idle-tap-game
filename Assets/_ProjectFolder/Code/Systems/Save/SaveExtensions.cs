using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DictionaryData<TKey, TValue>
{
    public List<TKey> keys;
    public List<TValue> values;

    public DictionaryData(Dictionary<TKey, TValue> dict)
    {
        keys = new(dict.Keys);
        values = new(dict.Values);
    }
    public DictionaryData(IDictionary<TKey, TValue> dict)
    {
        keys = new(dict.Keys);
        values = new(dict.Values);
    }
    public Dictionary<TKey, TValue> ToDictionary()
    {
        var dict = new Dictionary<TKey, TValue>();
        if (keys == null || values == null) return dict;

        int count = Mathf.Min(keys.Count, values.Count);
        for (int i = 0; i < count; i++)
        {
            if (!dict.ContainsKey(keys[i]))
                dict.Add(keys[i], values[i]);
        }
        
        return dict;
    }
}

public static class SaveExtensions
{
    
}