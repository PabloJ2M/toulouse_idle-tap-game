using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class ScheduleVersion
{
    private int _current;

    public int Next() => ++_current;
    public bool IsCurrent(int version) => version == _current;
}

public static class ScheduleExtension
{
    public static void Schedule(this Object owner, float time, ScheduleVersion version, Action onComplete) =>
        _ = owner.ScheduleAsync(time, version, onComplete);
    
    private static async Awaitable ScheduleAsync(this Object owner, float time, ScheduleVersion version, Action onComplete)
    {
        int currentVersion = version.Next();
        await Awaitable.WaitForSecondsAsync(time);
 
        if (owner && version.IsCurrent(currentVersion))
            onComplete?.Invoke();
    }
}