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

    public static void LoopTask(this Object owner, TaskLoopAsync task, DateTime startTime, Action onTick, Action onComplete) =>
        _ = LoopTaskAsync(owner, task, startTime, onTick, onComplete);
    private static async Awaitable LoopTaskAsync(this Object owner, TaskLoopAsync task, DateTime startTime, Action onTick, Action onComplete)
    {
        ulong currentVersion = task.Next();

        double elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
        int ticksOutRuntime = Mathf.FloorToInt((float)elapsedSeconds / task.TaskInterval);

        for (int i = 0; i < ticksOutRuntime; i++)
            onTick?.Invoke();

        if (elapsedSeconds >= task.Duration) {
            onComplete?.Invoke();
            return;
        }

        float remaining = task.Duration - (float)elapsedSeconds;

        while (remaining > 0)
        {
            float sinceLastTick = (float)(elapsedSeconds % task.TaskInterval);
            float nextTickIn = task.TaskInterval - sinceLastTick;
            float wait = Mathf.Min(nextTickIn, remaining);

            await Awaitable.WaitForSecondsAsync(wait);

            if (!owner || !task.IsCurrent(currentVersion))
                return;

            elapsedSeconds += wait;
            remaining -= wait;

            onTick?.Invoke();
        }

        if (owner && task.IsCurrent(currentVersion))
            onComplete?.Invoke();
    }
}