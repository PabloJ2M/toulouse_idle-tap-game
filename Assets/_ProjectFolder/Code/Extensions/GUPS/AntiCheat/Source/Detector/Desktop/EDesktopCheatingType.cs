namespace GUPS.AntiCheat.Detector.Desktop
{
    /// <summary>
    /// Kinds of cheating that can be reported by the desktop detectors (Windows, macOS, Linux).
    /// </summary>
    /// <remarks>
    /// Used by <see cref="ModLoaderDetector"/>, <see cref="DebuggerDetector"/> and
    /// <see cref="VirtualEnvironmentDetector"/> to classify the evidence that triggered the notification.
    /// </remarks>
    public enum EDesktopCheatingType : byte
    {
        /// <summary>
        /// Default sentinel used when the type of cheating could not be classified.
        /// </summary>
        UNKNOWN = 0,

        /// <summary>
        /// A managed assembly belonging to a known mod loader (BepInEx, MelonLoader, HarmonyLib, MonoMod, ...) was
        /// found loaded into the current <see cref="System.AppDomain"/>.
        /// </summary>
        MOD_LOADER_ASSEMBLY = 10,

        /// <summary>
        /// A file or folder belonging to a known mod loader (e.g. <c>BepInEx/</c>, <c>MelonLoader/</c>,
        /// <c>doorstop_config.ini</c>, <c>Mods/</c>) was found next to the game executable.
        /// </summary>
        MOD_LOADER_FILE = 11,

        /// <summary>
        /// A native module typical for a mod loader injector was found loaded into the process (e.g. a
        /// <c>winhttp.dll</c> or <c>version.dll</c> loaded from the game directory instead of System32 - the
        /// UnityDoorstop signature).
        /// </summary>
        MOD_LOADER_INJECTOR = 12,

        /// <summary>
        /// A user-mode debugger (Visual Studio, dnSpy, x64dbg, OllyDbg, ...) is currently attached to the process.
        /// </summary>
        DEBUGGER_USER_MODE = 20,

        /// <summary>
        /// A kernel-mode debugger (WinDbg in kernel mode, Syser, SoftICE, ...) is currently active on the system.
        /// </summary>
        DEBUGGER_KERNEL_MODE = 21,

        /// <summary>
        /// A remote debugger is currently attached or a debug port has been assigned to the process.
        /// </summary>
        DEBUGGER_REMOTE = 22,

        /// <summary>
        /// The system BIOS / DMI strings indicate a virtual machine vendor (VirtualBox, VMware, QEMU, Xen, Parallels,
        /// Hyper-V, ...).
        /// </summary>
        VIRTUAL_MACHINE_BIOS = 30,

        /// <summary>
        /// A network interface MAC address belongs to a known virtual machine vendor OUI (e.g. <c>08:00:27</c> for
        /// VirtualBox, <c>00:0C:29</c> for VMware, <c>00:15:5D</c> for Hyper-V).
        /// </summary>
        VIRTUAL_MACHINE_MAC = 31,

        /// <summary>
        /// The CPUID hypervisor present bit is set, indicating the OS is running inside a hypervisor.
        /// </summary>
        /// <remarks>
        /// Currently only reachable through a future native plugin probe; the managed probes always return false.
        /// </remarks>
        VIRTUAL_MACHINE_HYPERVISOR_BIT = 32,

        /// <summary>
        /// A process belonging to a virtual machine guest additions package was found running on the system
        /// (VBoxService, vmtoolsd, prl_tools_service, ...).
        /// </summary>
        VIRTUAL_MACHINE_PROCESS = 33,
    }
}
