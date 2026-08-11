using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class ScheduleVersion
{
    private ulong _current;

    public ulong Next() => ++_current;
    public bool IsCurrent(ulong version) => version == _current;
}

public static class ScheduleExtension
{
    public static void Schedule(this Object owner, float time, ScheduleVersion version, Action onComplete) =>
        _ = owner.ScheduleAsync(time, version, onComplete);
    
    private static async Awaitable ScheduleAsync(this Object owner, float time, ScheduleVersion version, Action onComplete)
    {
        ulong currentVersion = version.Next();
        await Awaitable.WaitForSecondsAsync(time);
 
        if (owner && version.IsCurrent(currentVersion))
            onComplete?.Invoke();
    }
}