#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX

// System
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GUPS.AntiCheat.Detector.Desktop.Platform
{
    /// <summary>
    /// macOS specific implementation of <see cref="IPlatformProbe"/>.
    /// </summary>
    /// <remarks>
    /// User mode debugger detection uses <c>sysctl</c> to read the <c>P_TRACED</c> flag from the process'
    /// <c>kinfo_proc</c>. Virtual machine detection reads <c>hw.model</c> and <c>kern.hv_vmm_present</c>.
    /// </remarks>
    internal sealed class MacPlatformProbe : IPlatformProbe
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public string Name => "macOS";

        #endregion

        // P/Invoke
        #region P/Invoke

        /// <summary>
        /// Top-level sysctl namespace for kernel state (<c>CTL_KERN</c>).
        /// </summary>
        private const int CTL_KERN = 1;

        /// <summary>
        /// Sub-level for process information (<c>KERN_PROC</c>).
        /// </summary>
        private const int KERN_PROC = 14;

        /// <summary>
        /// Selector to retrieve a single process by PID (<c>KERN_PROC_PID</c>).
        /// </summary>
        private const int KERN_PROC_PID = 1;

        /// <summary>
        /// Bit set in <c>extern_proc.p_flag</c> when the process is being traced (ptrace / debugger attached).
        /// </summary>
        private const int P_TRACED = 0x00000800;

        /// <summary>
        /// Offset of <c>p_flag</c> inside <c>extern_proc</c> on macOS (timeval (16) + vmspace ptr (8) + sigacts ptr (8)).
        /// </summary>
        private const int P_FLAG_OFFSET = 32;

        /// <summary>
        /// Conservative upper bound for the size of <c>kinfo_proc</c> across macOS versions. The actual struct is
        /// around 648 bytes - 1024 is used to stay on the safe side.
        /// </summary>
        private const int KinfoProcBufferSize = 1024;

        [DllImport("libc")]
        private static extern int sysctl([In] int[] name, uint namelen, IntPtr oldp, ref IntPtr oldlenp, IntPtr newp, IntPtr newlen);

        [DllImport("libc")]
        private static extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string name, IntPtr oldp, ref IntPtr oldlenp, IntPtr newp, IntPtr newlen);

        [DllImport("libc")]
        private static extern int getpid();

        #endregion

        // Debugger - User mode
        #region Debugger - User mode

        /// <inheritdoc/>
        public bool IsUserModeDebuggerPresent(out string evidence)
        {
            evidence = string.Empty;

            try
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    evidence = "System.Diagnostics.Debugger.IsAttached";
                    return true;
                }

                int[] var_Name = new int[] { CTL_KERN, KERN_PROC, KERN_PROC_PID, getpid() };
                IntPtr var_Buffer = Marshal.AllocHGlobal(KinfoProcBufferSize);
                try
                {
                    IntPtr var_Length = (IntPtr)KinfoProcBufferSize;
                    if (sysctl(var_Name, (uint)var_Name.Length, var_Buffer, ref var_Length, IntPtr.Zero, IntPtr.Zero) != 0)
                    {
                        return false;
                    }

                    int var_PFlag = Marshal.ReadInt32(var_Buffer, P_FLAG_OFFSET);
                    if ((var_PFlag & P_TRACED) != 0)
                    {
                        evidence = string.Format("kinfo_proc.p_flag has P_TRACED (0x{0:X})", var_PFlag);
                        return true;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(var_Buffer);
                }
            }
            catch
            {
                // Swallow - probe must never throw.
            }

            return false;
        }

        #endregion

        // Debugger - Kernel mode
        #region Debugger - Kernel mode

        /// <inheritdoc/>
        public bool IsKernelModeDebuggerPresent(out string evidence)
        {
            // Reliable kernel debugger detection on macOS requires a kext or SIP checks not available from a managed
            // process. Deferred to a future native plugin probe.
            evidence = string.Empty;
            return false;
        }

        #endregion

        // Virtual Machine
        #region Virtual Machine

        /// <summary>
        /// sysctl names sampled for VM vendor strings. This is a structural table (where to read) - the vendor
        /// keywords themselves (what is suspicious) are supplied by the caller from <c>GlobalSettings</c>.
        /// </summary>
        private static readonly string[] VmVendorSysctlNames = new[]
        {
            "hw.model",
            "machdep.cpu.brand_string",
            "hw.targettype",
        };

        /// <inheritdoc/>
        public bool IsRunningInVirtualMachine(IEnumerable<string> biosKeywords, IEnumerable<string> windowsServiceKeys, out string evidence)
        {
            evidence = string.Empty;

            // The service-key list is Windows-only and ignored here.

            try
            {
                // Modern macOS (Big Sur+) exposes a direct hypervisor flag.
                int var_HvPresent = ReadSysctlInt32("kern.hv_vmm_present");
                if (var_HvPresent != 0)
                {
                    evidence = "kern.hv_vmm_present=1";
                    return true;
                }

                if (biosKeywords == null)
                {
                    return false;
                }

                for (int i = 0; i < VmVendorSysctlNames.Length; i++)
                {
                    string var_Value = ReadSysctlString(VmVendorSysctlNames[i]);
                    if (string.IsNullOrEmpty(var_Value))
                    {
                        continue;
                    }

                    foreach (string var_Keyword in biosKeywords)
                    {
                        if (string.IsNullOrEmpty(var_Keyword))
                        {
                            continue;
                        }

                        if (var_Value.IndexOf(var_Keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            evidence = string.Format("{0}='{1}'", VmVendorSysctlNames[i], var_Value);
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // Swallow - probe must never throw.
            }

            return false;
        }

        /// <inheritdoc/>
        public bool IsHypervisorBitSet(out string evidence)
        {
            evidence = string.Empty;
            return false;
        }

        #endregion

        // Loaded Modules
        #region Loaded Modules

        /// <inheritdoc/>
        public IEnumerable<string> GetSuspiciousLoadedModules(IEnumerable<string> fileNames, IEnumerable<string> hijackDlls)
        {
            // Mac specific module probing requires libdyld and is best implemented in a native plugin. The detector
            // performs its own managed Process.GetCurrentProcess().Modules sweep on top.
            yield break;
        }

        #endregion

        // Helpers
        #region Helpers

        /// <summary>
        /// Reads a string sysctl value by name. Returns an empty string on failure.
        /// </summary>
        /// <param name="name">The sysctl name to read.</param>
        /// <returns>The string value or an empty string on failure.</returns>
        private static string ReadSysctlString(string name)
        {
            try
            {
                IntPtr var_Length = IntPtr.Zero;
                if (sysctlbyname(name, IntPtr.Zero, ref var_Length, IntPtr.Zero, IntPtr.Zero) != 0)
                {
                    return string.Empty;
                }

                int var_Size = var_Length.ToInt32();
                if (var_Size <= 0 || var_Size > 4096)
                {
                    return string.Empty;
                }

                IntPtr var_Buffer = Marshal.AllocHGlobal(var_Size);
                try
                {
                    if (sysctlbyname(name, var_Buffer, ref var_Length, IntPtr.Zero, IntPtr.Zero) != 0)
                    {
                        return string.Empty;
                    }

                    return Marshal.PtrToStringAnsi(var_Buffer, var_Length.ToInt32()).TrimEnd('\0').Trim();
                }
                finally
                {
                    Marshal.FreeHGlobal(var_Buffer);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Reads a single int32 sysctl value by name. Returns 0 on failure.
        /// </summary>
        /// <param name="name">The sysctl name to read.</param>
        /// <returns>The int32 value or 0 on failure.</returns>
        private static int ReadSysctlInt32(string name)
        {
            try
            {
                IntPtr var_Buffer = Marshal.AllocHGlobal(sizeof(int));
                try
                {
                    IntPtr var_Length = (IntPtr)sizeof(int);
                    if (sysctlbyname(name, var_Buffer, ref var_Length, IntPtr.Zero, IntPtr.Zero) != 0)
                    {
                        return 0;
                    }

                    return Marshal.ReadInt32(var_Buffer);
                }
                finally
                {
                    Marshal.FreeHGlobal(var_Buffer);
                }
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }
}

#endif
