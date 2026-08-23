using System;

namespace UnityEngine.Animations
{
    public interface ITweenCallback
    {
        event Action<bool> OnTweenStatusChanged, OnTweenCompleted;
        event Action OnEnabled, OnDisabled;
        event Action OnCompleted;

        void OnComplete();
    }
}