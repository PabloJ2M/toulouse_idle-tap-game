// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Detector;

namespace GUPS.AntiCheat.Detector.Android
{
    /// <summary>
    /// Extends <see cref="IDetectorStatus"/> with the type of cheating that was detected on the Android device.
    /// </summary>
    public interface IAndroidCheatingDetectionStatus : IDetectorStatus
    {
        /// <summary>
        /// Gets the type of cheating that was detected on the Android device.
        /// </summary>
        EAndroidCheatingType AndroidCheatingType { get; }
    }
}
