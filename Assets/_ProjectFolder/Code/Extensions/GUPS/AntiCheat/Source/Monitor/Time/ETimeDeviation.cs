namespace GUPS.AntiCheat.Monitor.Time
{
    /// <summary>
    /// Types of time deviation reported by the time monitors.
    /// </summary>
    public enum ETimeDeviation : byte
    {
        /// <summary>
        /// No deviation detected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Time has effectively stopped.
        /// </summary>
        Stopped = 1,

        /// <summary>
        /// Time is running slower than expected.
        /// </summary>
        SlowedDown = 2,

        /// <summary>
        /// Time is running faster than expected.
        /// </summary>
        SpeedUp = 3
    }
}