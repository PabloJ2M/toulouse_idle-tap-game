// System
using System.Collections.Generic;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Detector.Desktop.Platform
{
    /// <summary>
    /// Returns the <see cref="IPlatformProbe"/> implementation matching the current Unity runtime platform.
    /// </summary>
    /// <remarks>
    /// The factory caches a single shared instance. Detectors should call <see cref="Get"/> once and reuse the
    /// returned probe for their lifetime. On unsupported platforms (mobile, console, web) a
    /// <see cref="NoOpPlatformProbe"/> is returned whose probes always report negative.
    /// </remarks>
    public static class PlatformProbeFactory
    {
        /// <summary>
        /// The cached probe instance, created lazily on the first <see cref="Get"/> call.
        /// </summary>
        private static IPlatformProbe cached;

        /// <summary>
        /// Returns the platform probe matching the current Unity runtime platform.
        /// </summary>
        public static IPlatformProbe Get()
        {
            if (cached != null)
            {
                return cached;
            }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            cached = new WindowsPlatformProbe();
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            cached = new MacPlatformProbe();
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            cached = new LinuxPlatformProbe();
#else
            cached = new NoOpPlatformProbe();
#endif

            return cached;
        }

        /// <summary>
        /// No-op probe used on platforms that the desktop detectors do not support. All probes report negative.
        /// </summary>
        private sealed class NoOpPlatformProbe : IPlatformProbe
        {
            /// <inheritdoc/>
            public string Name => "NoOp";

            /// <inheritdoc/>
            public bool IsUserModeDebuggerPresent(out string evidence)
            {
                evidence = string.Empty;
                return false;
            }

            /// <inheritdoc/>
            public bool IsKernelModeDebuggerPresent(out string evidence)
            {
                evidence = string.Empty;
                return false;
            }

            /// <inheritdoc/>
            public bool IsRunningInVirtualMachine(IEnumerable<string> biosKeywords, IEnumerable<string> windowsServiceKeys, out string evidence)
            {
                evidence = string.Empty;
                return false;
            }

            /// <inheritdoc/>
            public bool IsHypervisorBitSet(out string evidence)
            {
                evidence = string.Empty;
                return false;
            }

            /// <inheritdoc/>
            public IEnumerable<string> GetSuspiciousLoadedModules(IEnumerable<string> fileNames, IEnumerable<string> hijackDlls)
            {
                yield break;
            }
        }
    }
}
