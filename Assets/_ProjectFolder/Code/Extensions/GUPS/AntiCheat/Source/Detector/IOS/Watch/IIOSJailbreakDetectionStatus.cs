// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Detector;

namespace GUPS.AntiCheat.Detector.IOS
{
    /// <summary>
    /// Extends <see cref="IDetectorStatus"/> with the type of jailbreak evidence detected on iOS and a short human
    /// readable evidence string describing what triggered the detection.
    /// </summary>
    public interface IIOSJailbreakDetectionStatus : IDetectorStatus
    {
        /// <summary>
        /// Gets the type of jailbreak evidence detected on the device.
        /// </summary>
        EIOSJailbreakType JailbreakType { get; }

        /// <summary>
        /// Gets a short human readable evidence string describing what triggered the detection
        /// (e.g. a file path, a URL scheme, a loaded dylib name). May be empty if no specific
        /// evidence is available.
        /// </summary>
        string Evidence { get; }
    }
}
