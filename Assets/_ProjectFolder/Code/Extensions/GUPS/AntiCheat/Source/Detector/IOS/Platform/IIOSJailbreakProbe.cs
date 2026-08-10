// System
using System.Collections.Generic;

namespace GUPS.AntiCheat.Detector.IOS.Platform
{
    /// <summary>
    /// Platform abstraction used by <see cref="IOSJailbreakDetector"/> to delegate the actual native scan.
    /// </summary>
    /// <remarks>
    /// The abstraction mirrors <see cref="GUPS.AntiCheat.Detector.Desktop.Platform.IPlatformProbe"/> for the desktop
    /// detectors. <see cref="IOSJailbreakProbeNative"/> P/Invokes into the bundled Objective-C++ plugin on real iOS
    /// builds; <see cref="NoOpIOSJailbreakProbe"/> short-circuits in the editor and on every non-iOS platform so that
    /// the detector can still be added to a scene during development.
    /// </remarks>
    public interface IIOSJailbreakProbe
    {
        /// <summary>
        /// Gets a short human readable name of the probe (e.g. "iOS-Native", "NoOp"). Used for logging.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Performs a single batched jailbreak scan and returns all hits. All validation lists are owned by the
        /// managed layer (defaults live in the GlobalSettings asset); the native side holds no built-in lists and
        /// only validates what it receives.
        /// </summary>
        /// <param name="_DetectUrlSchemes">When <c>true</c>, scan the supplied URL schemes via <c>canOpenURL</c>.</param>
        /// <param name="_DetectSuspiciousPaths">When <c>true</c>, <c>stat()</c> the supplied path list.</param>
        /// <param name="_DetectSandboxViolation">When <c>true</c>, attempt to write outside the app sandbox.</param>
        /// <param name="_DetectFork">When <c>true</c>, probe whether <c>fork()</c> returns a positive child PID.</param>
        /// <param name="_DetectDyldInjection">When <c>true</c>, inspect <c>DYLD_INSERT_LIBRARIES</c> and friends.</param>
        /// <param name="_DetectSuspiciousDylibs">When <c>true</c>, scan the loaded dyld images for the supplied substrings.</param>
        /// <param name="_UrlSchemes">Blacklist of URL schemes probed via <c>canOpenURL</c>. May be <c>null</c>.</param>
        /// <param name="_SuspiciousPaths">Blacklist of filesystem paths to <c>stat()</c>. May be <c>null</c>.</param>
        /// <param name="_SuspiciousDylibs">Blacklist of dylib substrings matched against every loaded image. May be <c>null</c>.</param>
        /// <param name="_DyldAllowedPrefixes">Whitelist of anchored path prefixes for <c>DYLD_INSERT_LIBRARIES</c>
        /// entries considered benign (Xcode debug support libraries). May be <c>null</c>.</param>
        /// <returns>
        /// The list of detected hits. The list is empty when no evidence was found.
        /// </returns>
        IReadOnlyList<IOSJailbreakDetectionStatus> Scan(bool _DetectUrlSchemes,
                                                       bool _DetectSuspiciousPaths,
                                                       bool _DetectSandboxViolation,
                                                       bool _DetectFork,
                                                       bool _DetectDyldInjection,
                                                       bool _DetectSuspiciousDylibs,
                                                       IReadOnlyList<string> _UrlSchemes,
                                                       IReadOnlyList<string> _SuspiciousPaths,
                                                       IReadOnlyList<string> _SuspiciousDylibs,
                                                       IReadOnlyList<string> _DyldAllowedPrefixes);
    }
}
