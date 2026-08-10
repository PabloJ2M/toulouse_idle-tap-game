using System;
using UnityEngine;

public class ScheduleVersion
{
    private int _current;

    public int Next() => ++_current;
    public bool IsCurrent(int version) => version == _current;
}

public abstract class ScheduleBehaviour : MonoBehaviour
{
    protected async void ScheduleAction(float time, ScheduleVersion version, int currentVersion, Action onComplete)
    {
        await Awaitable.WaitForSecondsAsync(time);
        
        if (this && version.IsCurrent(currentVersion))
            onComplete?.Invoke();
    }
}

public abstract class ScheduleScriptable : ScriptableObject
{
    protected async void ScheduleAction(float time, ScheduleVersion version, Action onComplete)
    {
        int nextVersion = version.Next();
        await Awaitable.WaitForSecondsAsync(time);
        
        if (this && version.IsCurrent(nextVersion))
            onComplete?.Invoke();
    }
}