using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializedHashSet<T> : ISerializationCallbackReceiver, IEnumerable<T>
{
    [SerializeField]
    private List<T> items = new();
    
    private readonly HashSet<T> _hashSet = new();

    public T this[int index] => items[index];
    public IReadOnlyList<T> List => items;
    public int Count => _hashSet.Count;

    public bool Contains(T item) => _hashSet.Contains(item);
    public bool Add(T item)
    {
        if (!_hashSet.Add(item)) return false;
        items.Add(item);
        return true;
    }
    public bool Remove(T item)
    {
        if (!_hashSet.Remove(item)) return false;
        items.Remove(item);
        return true;
    }
    public void Clear()
    {
        _hashSet.Clear();
        items.Clear();
    }

    public bool TryGetAt(int index, out T value)
    {
        if (index >= 0 && index < _hashSet.Count) {
            value = items[index];
            return true;
        }
        
        value = default;
        return false;
    }
    public int IndexOf(T item) => items.IndexOf(item);

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        _hashSet.Clear();
        
        for (int i = items.Count - 1; i >= 0; i--) {
            if (!_hashSet.Add(items[i]))
                items.RemoveAt(i);
        }
    }

    public IEnumerator<T> GetEnumerator() => _hashSet.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}