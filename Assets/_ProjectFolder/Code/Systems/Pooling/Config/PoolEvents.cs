using System;

namespace UnityEngine.Pool
{
    public readonly struct PoolEvents
    {
        public readonly Action<IObjectPooled> OnGet;
        public readonly Action<IObjectPooled> OnRelease;
        public readonly Action<IObjectPooled> OnDestroy;

        public PoolEvents(Action<IObjectPooled> onGet, Action<IObjectPooled> onRelease, Action<IObjectPooled> onDestroy)
        {
            OnGet = onGet;
            OnRelease = onRelease;
            OnDestroy = onDestroy;
        }
    }
}