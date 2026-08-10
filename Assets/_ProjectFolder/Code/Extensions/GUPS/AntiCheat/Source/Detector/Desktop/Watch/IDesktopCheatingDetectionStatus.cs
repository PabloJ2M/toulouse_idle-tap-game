// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Detector;

namespace GUPS.AntiCheat.Detector.Desktop
{
    /// <summary>
    /// Extends <see cref="IDetectorStatus"/> with the type of cheating detected on the desktop and a short human
    /// readable evidence string.
    /// </summary>
    public interface IDesktopCheatingDetectionStatus : IDetectorStatus
    {
        /// <summary>
        /// Gets the type of cheating detected on the desktop.
        /// </summary>
        EDesktopCheatingType DesktopCheatingType { get; }

        /// <summary>
        /// Gets a short human readable evidence string describing what triggered the detection (e.g. an assembly name,
        /// a file path, a BIOS string, a MAC OUI). May be empty if no specific evidence is available.
        /// </summary>
        string Evidence { get; }
    }
}
