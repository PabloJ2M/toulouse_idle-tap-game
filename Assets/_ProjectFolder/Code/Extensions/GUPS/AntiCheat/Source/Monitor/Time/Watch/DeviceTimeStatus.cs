// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Monitor.Time
{
    /// <summary>
    /// Status payload produced by <see cref="DeviceTimeMonitor"/> carrying the detected device-clock deviation.
    /// </summary>
    public struct DeviceTimeStatus : IWatchedSubject
    {
        /// <summary>
        /// Gets the detected device-clock deviation.
        /// </summary>
        public ETimeDeviation Deviation { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTimeStatus"/> struct.
        /// </summary>
        /// <param name="_Deviation">The detected device-clock deviation.</param>
        public DeviceTimeStatus(ETimeDeviation _Deviation)
        {
            this.Deviation = _Deviation;
        }
    }
}
