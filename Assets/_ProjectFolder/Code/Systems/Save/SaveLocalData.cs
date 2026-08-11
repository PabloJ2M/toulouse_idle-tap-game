using GUPS.AntiCheat.Protected.Storage.Prefs;
using UnityEngine;

public abstract class SaveLocalData<T> : MonoBehaviour
{
    protected abstract string LocalDataID { get; }

    protected virtual void LoadData(ref T data)
    {
        if (ProtectedPlayerPrefs.HasKey(LocalDataID))
            data = JsonUtility.FromJson<T>(ProtectedPlayerPrefs.GetString(LocalDataID));
    }
    protected virtual void SaveData(T data)
    {
        ProtectedPlayerPrefs.SetString(LocalDataID, JsonUtility.ToJson(data));
        ProtectedPlayerPrefs.Save();
    }
    protected virtual void DeleteData()
    {
        if (ProtectedPlayerPrefs.HasKey(LocalDataID))
            ProtectedPlayerPrefs.DeleteKey(LocalDataID);
    }
}

public abstract class SaveLocalDataObject : SaveLocalData<Object> { }
public abstract class SaveLocalDataInt : SaveLocalData<int>
{
    protected override void LoadData(ref int data) => data = ProtectedPlayerPrefs.GetInt(LocalDataID);
    protected override void SaveData(int data)
    {
        ProtectedPlayerPrefs.SetInt(LocalDataID, data);
        ProtectedPlayerPrefs.Save();
    }
}
public abstract class SaveLocalDataBool : SaveLocalData<bool>
{
    protected override void LoadData(ref bool data) => data = ProtectedPlayerPrefs.GetInt(LocalDataID, 0) == 1;
    protected override void SaveData(bool data)
    {
        ProtectedPlayerPrefs.SetInt(LocalDataID, data ? 1 : 0);
        ProtectedPlayerPrefs.Save();
    }
}
public abstract class SaveLocalDataFloat : SaveLocalData<float>
{
    protected override void LoadData(ref float data) => data = ProtectedPlayerPrefs.GetFloat(LocalDataID);
    protected override void SaveData(float data)
    {
        ProtectedPlayerPrefs.SetFloat(LocalDataID, data);
        ProtectedPlayerPrefs.Save();
    }
}
public abstract class SaveLocalDataString : SaveLocalData<string>
{
    protected override void LoadData(ref string data) => data = ProtectedPlayerPrefs.GetString(LocalDataID);
    protected override void SaveData(string data)
    {
        ProtectedPlayerPrefs.SetString(LocalDataID, data);
        ProtectedPlayerPrefs.Save();
    }
}