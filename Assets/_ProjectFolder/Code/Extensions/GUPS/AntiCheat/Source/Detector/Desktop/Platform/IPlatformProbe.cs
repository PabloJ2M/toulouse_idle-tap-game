// System
using System.Collections.Generic;

namespace GUPS.AntiCheat.Detector.Desktop.Platform
{
    /// <summary>
    /// Platform abstraction used by the desktop tampering detectors to perform OS specific probes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The desktop detectors (<see cref="ModLoaderDetector"/>, <see cref="DebuggerDetector"/>,
    /// <see cref="VirtualEnvironmentDetector"/>) request a concrete probe from the <see cref="PlatformProbeFactory"/>
    /// instead of calling into the OS directly. This keeps the per-OS P/Invoke isolated in
    /// <see cref="WindowsPlatformProbe"/>, <see cref="MacPlatformProbe"/> and <see cref="LinuxPlatformProbe"/>.
    /// </para>
    /// <para>
    /// All members must be safe to call from <c>Awake</c> / <c>Start</c> / coroutines on the Unity main thread and
    /// must never throw. Implementations must swallow internal errors and return a conservative default
    /// (typically <c>false</c> / empty).
    /// </para>
    /// </remarks>
    public interface IPlatformProbe
    {
        /// <summary>
        /// Gets a short human readable name of the probe (e.g. "Windows", "macOS", "Linux", "NoOp"). Used for logging.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns whether a user mode debugger is currently attached to the running process.
        /// </summary>
        /// <param name="evidence">A short evidence string describing what was found. Empty when nothing was found.</param>
        /// <returns><c>true</c> if a user mode debugger was detected; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// On Windows this combines <c>IsDebuggerPresent</c>, <c>CheckRemoteDebuggerPresent</c> and an
        /// <c>NtQueryInformationProcess</c> sweep over <c>ProcessDebugPort</c> / <c>ProcessDebugObjectHandle</c> /
        /// <c>ProcessDebugFlags</c>. On Linux this reads <c>/proc/self/status</c> and checks <c>TracerPid</c>. On
        /// macOS this reads <c>kinfo_proc</c> via <c>sysctl</c> and checks the <c>P_TRACED</c> flag.
        /// </remarks>
        bool IsUserModeDebuggerPresent(out string evidence);

        /// <summary>
        /// Returns whether a kernel mode debugger (WinDbg in kernel mode, Syser, SoftICE, ...) is currently active on
        /// the system.
        /// </summary>
        /// <param name="evidence">A short evidence string describing what was found. Empty when nothing was found.</param>
        /// <returns><c>true</c> if a kernel mode debugger was detected; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Implemented on Windows via <c>NtQuerySystemInformation(SystemKernelDebuggerInformation)</c>. Returns
        /// <c>false</c> on Linux and macOS - meaningful kernel debug probing on those platforms is deferred to a
        /// future native plugin.
        /// </remarks>
        bool IsKernelModeDebuggerPresent(out string evidence);

        /// <summary>
        /// Returns whether the process is running inside a virtual machine, based on platform specific evidence
        /// (BIOS / DMI strings on Windows and Linux, hardware model on macOS).
        /// </summary>
        /// <param name="biosKeywords">
        /// The full vendor keyword list to scan for in the BIOS / DMI / hardware model strings, supplied from
        /// <c>GlobalSettings</c>. The probes hold no built-in keywords - a <c>null</c> or empty list skips the
        /// keyword check.
        /// </param>
        /// <param name="windowsServiceKeys">
        /// The guest-tool service registry keys (relative to HKLM) to probe for, supplied from <c>GlobalSettings</c>.
        /// Only consumed by the Windows probe - a <c>null</c> or empty list skips the service check.
        /// </param>
        /// <param name="evidence">A short evidence string describing what was found (vendor name, model, ...). Empty when nothing was found.</param>
        /// <returns><c>true</c> if VM evidence was found; otherwise <c>false</c>.</returns>
        bool IsRunningInVirtualMachine(IEnumerable<string> biosKeywords, IEnumerable<string> windowsServiceKeys, out string evidence);

        /// <summary>
        /// Returns whether the CPUID hypervisor present bit is set.
        /// </summary>
        /// <param name="evidence">A short evidence string describing what was found. Empty when nothing was found.</param>
        /// <returns><c>true</c> if the hypervisor bit was detected; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Reading CPUID from managed code without unsafe code is not portable, so the managed probes always return
        /// <c>false</c>. A future native plugin can override this to return the real CPUID value.
        /// </remarks>
        bool IsHypervisorBitSet(out string evidence);

        /// <summary>
        /// Returns the names (or full paths) of native modules currently loaded into the process that look suspicious
        /// to this probe.
        /// </summary>
        /// <param name="fileNames">
        /// The mod loader file / folder names from <c>GlobalSettings</c>. <c>.dll</c> entries are matched against
        /// module file names; entries ending with '/' are treated as folder names and matched as path substrings.
        /// A <c>null</c> or empty list skips those checks.
        /// </param>
        /// <param name="hijackDlls">
        /// DLL names from <c>GlobalSettings</c> that are legitimate inside the Windows system directory but indicate
        /// Unity Doorstop hijacking when loaded from anywhere else. Only consumed by the Windows probe - a
        /// <c>null</c> or empty list skips the hijack check.
        /// </param>
        /// <remarks>
        /// On Windows this typically reports <c>winhttp.dll</c> / <c>version.dll</c> loaded from the game directory
        /// (the UnityDoorstop signature) instead of from System32. On Linux and macOS this returns an empty sequence
        /// by default; <see cref="ModLoaderDetector"/> still performs its own
        /// <see cref="System.Diagnostics.Process.GetCurrentProcess"/>.<c>Modules</c> scan on top.
        /// </remarks>
        IEnumerable<string> GetSuspiciousLoadedModules(IEnumerable<string> fileNames, IEnumerable<string> hijackDlls);
    }
}
