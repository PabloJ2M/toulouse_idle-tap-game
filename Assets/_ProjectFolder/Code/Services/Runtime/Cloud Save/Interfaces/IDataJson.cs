using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.CloudSave
{
    public class IDataJson
    {
        string ToJson() => JsonUtility.ToJson(this);
        string ToJsonDictionary() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}