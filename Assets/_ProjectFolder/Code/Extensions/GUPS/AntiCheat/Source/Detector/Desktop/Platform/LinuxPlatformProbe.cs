#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX

// System
using System;
using System.Collections.Generic;
using System.IO;

namespace GUPS.AntiCheat.Detector.Desktop.Platform
{
    /// <summary>
    /// Linux specific implementation of <see cref="IPlatformProbe"/>.
    /// </summary>
    /// <remarks>
    /// Uses <c>/proc/self/status</c> for ptrace detection, <c>/sys/class/dmi/id/*</c> for VM vendor strings and
    /// <c>/proc/cpuinfo</c> for the hypervisor flag. No native code is required.
    /// </remarks>
    internal sealed class LinuxPlatformProbe : IPlatformProbe
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public string Name => "Linux";

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

                if (!File.Exists("/proc/self/status"))
                {
                    return false;
                }

                string[] var_Lines = File.ReadAllLines("/proc/self/status");
                for (int i = 0; i < var_Lines.Length; i++)
                {
                    string var_Line = var_Lines[i];
                    if (!var_Line.StartsWith("TracerPid:", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string var_Value = var_Line.Substring("TracerPid:".Length).Trim();
                    int var_Pid;
                    if (int.TryParse(var_Value, out var_Pid) && var_Pid != 0)
                    {
                        evidence = string.Format("/proc/self/status TracerPid={0}", var_Pid);
                        return true;
                    }

                    return false;
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
            // Linux kernel debug detection (kgdb, kdb) requires reading /proc/kallsyms or kernel module state which is
            // not meaningful from a managed user mode process. Deferred to a future native plugin probe.
            evidence = string.Empty;
            return false;
        }

        #endregion

        // Virtual Machine
        #region Virtual Machine

        /// <summary>
        /// The DMI files sampled for vendor strings. This is a structural table (where to read) - the vendor
        /// keywords themselves (what is suspicious) are supplied by the caller from <c>GlobalSettings</c>.
        /// </summary>
        private static readonly string[] DmiFiles = new[]
        {
            "/sys/class/dmi/id/product_name",
            "/sys/class/dmi/id/sys_vendor",
            "/sys/class/dmi/id/board_vendor",
            "/sys/class/dmi/id/bios_vendor",
            "/sys/class/dmi/id/chassis_vendor",
        };

        /// <inheritdoc/>
        public bool IsRunningInVirtualMachine(IEnumerable<string> biosKeywords, IEnumerable<string> windowsServiceKeys, out string evidence)
        {
            evidence = string.Empty;

            // The service-key list is Windows-only and ignored here.

            if (biosKeywords == null)
            {
                return false;
            }

            try
            {
                for (int i = 0; i < DmiFiles.Length; i++)
                {
                    if (!File.Exists(DmiFiles[i]))
                    {
                        continue;
                    }

                    string var_Content;
                    try
                    {
                        var_Content = File.ReadAllText(DmiFiles[i]).Trim();
                    }
                    catch
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(var_Content))
                    {
                        continue;
                    }

                    foreach (string var_Keyword in biosKeywords)
                    {
                        if (string.IsNullOrEmpty(var_Keyword))
                        {
                            continue;
                        }

                        if (var_Content.IndexOf(var_Keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            evidence = string.Format("{0}='{1}'", DmiFiles[i], var_Content);
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

            try
            {
                if (!File.Exists("/proc/cpuinfo"))
                {
                    return false;
                }

                string[] var_Lines = File.ReadAllLines("/proc/cpuinfo");
                for (int i = 0; i < var_Lines.Length; i++)
                {
                    string var_Line = var_Lines[i];
                    if (!var_Line.StartsWith("flags", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int var_Colon = var_Line.IndexOf(':');
                    if (var_Colon < 0)
                    {
                        continue;
                    }

                    string var_Flags = var_Line.Substring(var_Colon + 1);
                    if (var_Flags.IndexOf(" hypervisor ", StringComparison.Ordinal) >= 0
                        || var_Flags.TrimEnd().EndsWith(" hypervisor", StringComparison.Ordinal))
                    {
                        evidence = "/proc/cpuinfo flags has 'hypervisor'";
                        return true;
                    }

                    return false;
                }
            }
            catch
            {
                // Swallow - probe must never throw.
            }

            return false;
        }

        #endregion

        // Loaded Modules
        #region Loaded Modules

        /// <inheritdoc/>
        public IEnumerable<string> GetSuspiciousLoadedModules(IEnumerable<string> fileNames, IEnumerable<string> hijackDlls)
        {
            // Linux specific module probing requires libdl and is best implemented in a native plugin. The detector
            // performs its own managed Process.GetCurrentProcess().Modules sweep on top.
            yield break;
        }

        #endregion
    }
}

#endif
