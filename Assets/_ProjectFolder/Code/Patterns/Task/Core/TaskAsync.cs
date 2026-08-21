public class TaskAsync
{
    private ulong _version;
    private float _time;

    public float Time => _time;

    public TaskAsync(float time)
    {
        _time = time;
        _version = 0;
    }

    public void SetTime(float time) => _time = time;
    public bool IsCurrent(ulong version) => version == _version;
    public ulong Next() => ++_version;
}