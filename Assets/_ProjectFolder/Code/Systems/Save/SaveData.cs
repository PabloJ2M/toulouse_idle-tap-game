using System.Collections.Generic;
using GUPS.AntiCheat.Protected.Storage.Prefs;
using UnityEngine;

public abstract class SaveData<T>
{
    protected readonly string LocalDataID;
    protected SaveData(string localDataID) => LocalDataID = localDataID;
    
    public virtual void Load(ref T value)
    {
        if (ProtectedPlayerPrefs.HasKey(LocalDataID))
            value = JsonUtility.FromJson<T>(ProtectedPlayerPrefs.GetString(LocalDataID));
    }
    public virtual void Save(T value)
    {
        ProtectedPlayerPrefs.SetString(LocalDataID, JsonUtility.ToJson(value));
        ProtectedPlayerPrefs.Save();
    }
    public void Delete()
    {
        if (ProtectedPlayerPrefs.HasKey(LocalDataID))
            ProtectedPlayerPrefs.DeleteKey(LocalDataID);
    }
}

public class SaveDataObject : SaveData<object> { public SaveDataObject(string localDataID) : base(localDataID) { } }
public class SaveDataInt : SaveData<int>
{
    public SaveDataInt(string localDataID) : base(localDataID) { }

    public override void Load(ref int value) => value = ProtectedPlayerPrefs.GetInt(LocalDataID);
    public override void Save(int value)
    {
        ProtectedPlayerPrefs.SetInt(LocalDataID, value);
        ProtectedPlayerPrefs.Save();
    }
}
public class SaveDataBool : SaveData<bool>
{
    public SaveDataBool(string localDataID) : base(localDataID) { }

    public override void Load(ref bool value) => value = ProtectedPlayerPrefs.GetInt(LocalDataID, 0) == 1;
    public override void Save(bool value)
    {
        ProtectedPlayerPrefs.SetInt(LocalDataID, value ? 1 : 0);
        ProtectedPlayerPrefs.Save();
    }
}
public class SaveDataFloat : SaveData<float>
{
    public SaveDataFloat(string localDataID) : base(localDataID) { }
    
    public override void Load(ref float value) => value = ProtectedPlayerPrefs.GetFloat(LocalDataID);
    public override void Save(float value)
    {
        ProtectedPlayerPrefs.SetFloat(LocalDataID, value);
        ProtectedPlayerPrefs.Save();
    }
}
public class SaveDataString : SaveData<string>
{
    public SaveDataString(string localDataID) : base(localDataID) { }
    
    public override void Load(ref string value) => value = ProtectedPlayerPrefs.GetString(LocalDataID);
    public override void Save(string value)
    {
        ProtectedPlayerPrefs.SetString(LocalDataID, value);
        ProtectedPlayerPrefs.Save();
    }
}

public class SaveDataDictionary<TKey, TValue> : SaveData<Dictionary<TKey, TValue>>
{
    public SaveDataDictionary(string localDataID) : base(localDataID) { }
    
    public override void Load(ref Dictionary<TKey, TValue> value)
    {
        if (!ProtectedPlayerPrefs.HasKey(LocalDataID)) return;

        string json = ProtectedPlayerPrefs.GetString(LocalDataID);
        var data = JsonUtility.FromJson<DictionaryData<TKey, TValue>>(json);

        if (value == null) {
            value = data.ToDictionary();
            return;
        }
        
        value.Clear();
        foreach (var kvp in data.ToDictionary())
            value[kvp.Key] = kvp.Value;
    }
    public override void Save(Dictionary<TKey, TValue> value)
    {
        if (value == null) return;

        var data = new DictionaryData<TKey, TValue>(value);
        string json = JsonUtility.ToJson(data);

        ProtectedPlayerPrefs.SetString(LocalDataID, json);
        ProtectedPlayerPrefs.Save();
    }
}