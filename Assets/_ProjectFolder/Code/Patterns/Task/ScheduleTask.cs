using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ScheduleTask
{
    public static void OverrideTask(this Object owner, TaskAsync task, Action onComplete) =>
        _ = owner.OverrideTaskAsync(task, onComplete);
    
    private static async Awaitable OverrideTaskAsync(this Object owner, TaskAsync task, Action onComplete)
    {
        ulong currentVersion = task.Next();
        await Awaitable.WaitForSecondsAsync(task.Time);
 
        if (owner && task.IsCurrent(currentVersion))
            onComplete?.Invoke();
    }

    public static void LoopTask(this  Object owner, TaskLoopAsync task,
        Action onTick, Action onComplete = null) =>
        _ = owner.LoopTaskAsync(task, onTick, onComplete);

    private static async Awaitable LoopTaskAsync(this Object owner, TaskLoopAsync task,
        Action onTick, Action onComplete = null)
    {
        var currentVersion = task.Next();
        await owner.RunTicksAsync(task, currentVersion, 0f, onTick, onComplete);
    }
    
    public static void LoopTaskOffline(this Object owner, TaskLoopAsync task, DateTime startTime,
        Action onTick, Action onComplete) =>
        _ = owner.LoopTaskOfflineAsync(task, startTime, onTick, onComplete);
    
    private static async Awaitable LoopTaskOfflineAsync(this Object owner, TaskLoopAsync task, DateTime startTime,
        Action onTick, Action onComplete)
    {
        var currentVersion = task.Next();

        var elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
        var ticksOutRuntime = Mathf.FloorToInt((float)elapsedSeconds / task.TaskInterval);

        for (var i = 0; i < ticksOutRuntime; i++)
            onTick?.Invoke();

        if (elapsedSeconds >= task.Duration) {
            onComplete?.Invoke();
            return;
        }

        await owner.RunTicksAsync(task, currentVersion, elapsedSeconds, onTick, onComplete);
    }

    private static async Awaitable RunTicksAsync(this Object owner, TaskLoopAsync task,
        ulong currentVersion, double elapsedSeconds,
        Action onTick, Action onComplete = null)
    {
        var remaining = task.Duration - (float)elapsedSeconds;
        
        while (remaining > 0 || task.Duration == 0)
        {
            var sinceLastTick = (float)(elapsedSeconds % task.TaskInterval);
            var nextTickIn = task.TaskInterval - sinceLastTick;
            var wait = Mathf.Min(nextTickIn, remaining);

            await Awaitable.WaitForSecondsAsync(wait);
            elapsedSeconds += wait;
            remaining -= wait;

            if (owner && task.IsCurrent(currentVersion))
                onTick();
        }

        if (owner && task.IsCurrent(currentVersion))
            onComplete?.Invoke();
    }
}