// System
using System.Collections.Generic;

namespace GUPS.AntiCheat.Detector.IOS.Platform
{
    /// <summary>
    /// No-op <see cref="IIOSJailbreakProbe"/> used in the editor and on every non-iOS player platform.
    /// </summary>
    /// <remarks>
    /// All probes return an empty hit list so the detector can be safely added to a scene during development without
    /// firing false positives. The real scanning happens in <see cref="IOSJailbreakProbeNative"/> on iOS device builds.
    /// </remarks>
    internal sealed class NoOpIOSJailbreakProbe : IIOSJailbreakProbe
    {
        /// <summary>
        /// Reusable empty result array so the no-op probe does not allocate per call.
        /// </summary>
        private static readonly IOSJailbreakDetectionStatus[] Empty = new IOSJailbreakDetectionStatus[0];

        /// <inheritdoc/>
        public string Name => "NoOp";

        /// <inheritdoc/>
        public IReadOnlyList<IOSJailbreakDetectionStatus> Scan(bool _DetectUrlSchemes,
                                                               bool _DetectSuspiciousPaths,
                                                               bool _DetectSandboxViolation,
                                                               bool _DetectFork,
                                                               bool _DetectDyldInjection,
                                                               bool _DetectSuspiciousDylibs,
                                                               IReadOnlyList<string> _UrlSchemes,
                                                               IReadOnlyList<string> _SuspiciousPaths,
                                                               IReadOnlyList<string> _SuspiciousDylibs,
                                                               IReadOnlyList<string> _DyldAllowedPrefixes)
        {
            return Empty;
        }
    }
}
