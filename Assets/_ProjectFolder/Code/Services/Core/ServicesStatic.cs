using UnityEngine;

namespace Unity.Services.Core
{
    public abstract class ServicesStatic<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }
        
        protected virtual void Awake() => Instance = this as T;
    }
}