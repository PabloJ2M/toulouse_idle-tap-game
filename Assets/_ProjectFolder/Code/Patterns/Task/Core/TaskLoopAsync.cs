public class TaskLoopAsync : TaskAsync
{
    private float taskInterval;

    public float TaskInterval => taskInterval;
    public float Duration => Time;

    public TaskLoopAsync(float duration, float taskInterval) : base(duration)
    {
        this.taskInterval = taskInterval;
    }
}