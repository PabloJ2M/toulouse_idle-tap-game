#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

// System
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace GUPS.AntiCheat.Detector.Desktop.Platform
{
    /// <summary>
    /// Windows specific implementation of <see cref="IPlatformProbe"/>.
    /// </summary>
    /// <remarks>
    /// All probes use plain P/Invoke and registry reads and never throw - any internal exception is swallowed and
    /// converted to a conservative <c>false</c> result so detectors stay in a safe state if Windows changes its
    /// internal layout.
    /// </remarks>
    internal sealed class WindowsPlatformProbe : IPlatformProbe
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public string Name => "Windows";

        #endregion

        // P/Invoke - kernel32
        #region P/Invoke - kernel32

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] ref bool isDebuggerPresent);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        #endregion

        // P/Invoke - ntdll
        #region P/Invoke - ntdll

        /// <summary>
        /// <c>ProcessInformationClass</c> values consumed by <see cref="NtQueryInformationProcess"/>.
        /// </summary>
        private const int ProcessDebugPort = 0x07;
        private const int ProcessDebugObjectHandle = 0x1E;
        private const int ProcessDebugFlags = 0x1F;

        /// <summary>
        /// <c>SystemInformationClass</c> value used to retrieve kernel debugger state via
        /// <see cref="NtQuerySystemInformation"/>.
        /// </summary>
        private const int SystemKernelDebuggerInformation = 0x23;

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref IntPtr processInformation, int processInformationLength, ref int returnLength);

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref int processInformation, int processInformationLength, ref int returnLength);

        [DllImport("ntdll.dll")]
        private static extern int NtQuerySystemInformation(int systemInformationClass, ref SystemKernelDebuggerInformationStruct systemInformation, int systemInformationLength, ref int returnLength);

        /// <summary>
        /// Layout of <c>SYSTEM_KERNEL_DEBUGGER_INFORMATION</c> as returned by <c>NtQuerySystemInformation</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SystemKernelDebuggerInformationStruct
        {
            [MarshalAs(UnmanagedType.U1)] public bool KernelDebuggerEnabled;
            [MarshalAs(UnmanagedType.U1)] public bool KernelDebuggerNotPresent;
        }

        #endregion

        // P/Invoke - advapi32
        #region P/Invoke - advapi32

        private const int ERROR_SUCCESS = 0;
        private const int KEY_READ = 0x20019;
        private const int REG_SZ = 1;
        private const int REG_EXPAND_SZ = 2;
        private static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyExW", SetLastError = true)]
        private static extern int RegOpenKeyExW(IntPtr hKey, string lpSubKey, int ulOptions, int samDesired, out IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        private static extern int RegQueryValueExW(IntPtr hKey, string lpValueName, IntPtr lpReserved, out int lpType, IntPtr lpData, ref int lpcbData);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(IntPtr hKey);

        /// <summary>
        /// Opens an HKLM subkey for read access. Returns <see cref="IntPtr.Zero"/> if the key does not exist or cannot
        /// be opened. Caller is responsible for <c>RegCloseKey</c> on success.
        /// </summary>
        /// <param name="subKey">The HKLM subkey path to open.</param>
        /// <returns>A handle to the opened key, or <see cref="IntPtr.Zero"/> on failure.</returns>
        private static IntPtr OpenLocalMachineSubKey(string subKey)
        {
            if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, subKey, 0, KEY_READ, out IntPtr var_Handle) == ERROR_SUCCESS)
            {
                return var_Handle;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Reads a <c>REG_SZ</c> / <c>REG_EXPAND_SZ</c> value from an open registry key. Returns <c>null</c> if the
        /// value does not exist or is not a string.
        /// </summary>
        /// <param name="key">An open registry key handle.</param>
        /// <param name="valueName">The name of the value to read.</param>
        /// <returns>The string value, or <c>null</c> on failure.</returns>
        private static string ReadRegistryString(IntPtr key, string valueName)
        {
            int var_Size = 0;
            int var_Type;
            if (RegQueryValueExW(key, valueName, IntPtr.Zero, out var_Type, IntPtr.Zero, ref var_Size) != ERROR_SUCCESS)
            {
                return null;
            }
            if (var_Size <= 0 || (var_Type != REG_SZ && var_Type != REG_EXPAND_SZ))
            {
                return null;
            }
            IntPtr var_Buffer = Marshal.AllocHGlobal(var_Size);
            try
            {
                if (RegQueryValueExW(key, valueName, IntPtr.Zero, out var_Type, var_Buffer, ref var_Size) != ERROR_SUCCESS)
                {
                    return null;
                }
                return Marshal.PtrToStringUni(var_Buffer, var_Size / 2).TrimEnd('\0');
            }
            finally
            {
                Marshal.FreeHGlobal(var_Buffer);
            }
        }

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

                if (IsDebuggerPresent())
                {
                    evidence = "kernel32!IsDebuggerPresent";
                    return true;
                }

                bool var_RemoteDebugger = false;
                if (CheckRemoteDebuggerPresent(GetCurrentProcess(), ref var_RemoteDebugger) && var_RemoteDebugger)
                {
                    evidence = "kernel32!CheckRemoteDebuggerPresent";
                    return true;
                }

                IntPtr var_DebugPort = IntPtr.Zero;
                int var_Returned = 0;
                if (NtQueryInformationProcess(GetCurrentProcess(), ProcessDebugPort, ref var_DebugPort, IntPtr.Size, ref var_Returned) == 0 && var_DebugPort != IntPtr.Zero)
                {
                    evidence = "ntdll!NtQueryInformationProcess(ProcessDebugPort)";
                    return true;
                }

                IntPtr var_DebugObject = IntPtr.Zero;
                if (NtQueryInformationProcess(GetCurrentProcess(), ProcessDebugObjectHandle, ref var_DebugObject, IntPtr.Size, ref var_Returned) == 0 && var_DebugObject != IntPtr.Zero)
                {
                    evidence = "ntdll!NtQueryInformationProcess(ProcessDebugObjectHandle)";
                    return true;
                }

                int var_DebugFlags = 0;
                if (NtQueryInformationProcess(GetCurrentProcess(), ProcessDebugFlags, ref var_DebugFlags, sizeof(int), ref var_Returned) == 0 && var_DebugFlags == 0)
                {
                    // ProcessDebugFlags returns the inverse of NoDebugInherit. A value of 0 means a debugger is
                    // present (the kernel cleared the inherit flag). Treated as a soft signal only.
                    evidence = "ntdll!NtQueryInformationProcess(ProcessDebugFlags)";
                    return true;
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
            evidence = string.Empty;

            try
            {
                SystemKernelDebuggerInformationStruct var_Info = new SystemKernelDebuggerInformationStruct();
                int var_Returned = 0;
                int var_Status = NtQuerySystemInformation(SystemKernelDebuggerInformation, ref var_Info, Marshal.SizeOf(typeof(SystemKernelDebuggerInformationStruct)), ref var_Returned);

                if (var_Status != 0)
                {
                    return false;
                }

                if (var_Info.KernelDebuggerEnabled && !var_Info.KernelDebuggerNotPresent)
                {
                    evidence = "ntdll!NtQuerySystemInformation(SystemKernelDebuggerInformation): enabled and present";
                    return true;
                }
            }
            catch
            {
                // Swallow - probe must never throw.
            }

            return false;
        }

        #endregion

        // Virtual Machine
        #region Virtual Machine

        /// <summary>
        /// Registry values under <c>HKLM\HARDWARE\DESCRIPTION\System\BIOS</c> sampled for vendor strings. This is a
        /// structural table (where to read) - the vendor keywords themselves (what is suspicious) are supplied by the
        /// caller from <c>GlobalSettings</c>.
        /// </summary>
        private static readonly string[] BiosValueNames = new[]
        {
            "SystemManufacturer",
            "SystemProductName",
            "SystemFamily",
            "SystemVersion",
            "BIOSVendor",
            "BIOSVersion",
            "BaseBoardManufacturer",
            "BaseBoardProduct",
        };

        /// <inheritdoc/>
        public bool IsRunningInVirtualMachine(IEnumerable<string> biosKeywords, IEnumerable<string> windowsServiceKeys, out string evidence)
        {
            evidence = string.Empty;

            try
            {
                if (biosKeywords != null)
                {
                    IntPtr var_BiosKey = OpenLocalMachineSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                    if (var_BiosKey != IntPtr.Zero)
                    {
                        try
                        {
                            for (int i = 0; i < BiosValueNames.Length; i++)
                            {
                                string var_Value = ReadRegistryString(var_BiosKey, BiosValueNames[i]);
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
                                        evidence = string.Format("BIOS\\{0}='{1}'", BiosValueNames[i], var_Value);
                                        return true;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            RegCloseKey(var_BiosKey);
                        }
                    }
                }

                if (windowsServiceKeys != null)
                {
                    foreach (string var_ServiceKey in windowsServiceKeys)
                    {
                        if (string.IsNullOrEmpty(var_ServiceKey))
                        {
                            continue;
                        }

                        IntPtr var_Service = OpenLocalMachineSubKey(var_ServiceKey);
                        if (var_Service != IntPtr.Zero)
                        {
                            RegCloseKey(var_Service);
                            evidence = string.Format("Service '{0}' present", var_ServiceKey);
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
            // Reading CPUID without a native helper requires unsafe code and platform-specific assembly. Deferred to
            // a future native plugin probe.
            evidence = string.Empty;
            return false;
        }

        #endregion

        // Loaded Modules
        #region Loaded Modules

        /// <inheritdoc/>
        public IEnumerable<string> GetSuspiciousLoadedModules(IEnumerable<string> fileNames, IEnumerable<string> hijackDlls)
        {
            // Split the supplied file-name list into plain module names ('MelonLoader.dll') and folder entries
            // ('BepInEx/'). Folder entries become normalized path substrings ('/bepinex/') so any DLL loaded from
            // such a folder is flagged. The probe holds no built-in lists - GlobalSettings supplies everything.
            List<string> var_ModuleNames = new List<string>();
            List<string> var_FolderSubstrings = new List<string>();
            if (fileNames != null)
            {
                foreach (string var_Entry in fileNames)
                {
                    if (string.IsNullOrEmpty(var_Entry))
                    {
                        continue;
                    }

                    if (var_Entry.EndsWith("/"))
                    {
                        string var_Folder = var_Entry.Trim('/').ToLowerInvariant();
                        if (var_Folder.Length > 0)
                        {
                            var_FolderSubstrings.Add("/" + var_Folder + "/");
                        }
                    }
                    else
                    {
                        var_ModuleNames.Add(var_Entry);
                    }
                }
            }

            List<string> var_HijackNames = new List<string>();
            if (hijackDlls != null)
            {
                foreach (string var_Entry in hijackDlls)
                {
                    if (!string.IsNullOrEmpty(var_Entry))
                    {
                        var_HijackNames.Add(var_Entry);
                    }
                }
            }

            if (var_ModuleNames.Count == 0 && var_FolderSubstrings.Count == 0 && var_HijackNames.Count == 0)
            {
                yield break;
            }

            string var_SystemDir = null;
            try
            {
                var_SystemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
            }
            catch
            {
                // Swallow - GetFolderPath can throw on locked down systems.
            }

            ProcessModuleCollection var_Modules = null;
            try
            {
                var_Modules = Process.GetCurrentProcess().Modules;
            }
            catch
            {
                yield break;
            }

            foreach (ProcessModule var_Module in var_Modules)
            {
                string var_Name;
                string var_Path;
                try
                {
                    var_Name = var_Module.ModuleName;
                    var_Path = var_Module.FileName;
                }
                catch
                {
                    continue;
                }

                if (string.IsNullOrEmpty(var_Name))
                {
                    continue;
                }

                for (int i = 0; i < var_ModuleNames.Count; i++)
                {
                    if (string.Equals(var_Name, var_ModuleNames[i], StringComparison.OrdinalIgnoreCase))
                    {
                        yield return var_Path ?? var_Name;
                        goto next;
                    }
                }

                for (int i = 0; i < var_HijackNames.Count; i++)
                {
                    if (string.Equals(var_Name, var_HijackNames[i], StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(var_Path))
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(var_SystemDir) && var_Path.StartsWith(var_SystemDir, StringComparison.OrdinalIgnoreCase))
                        {
                            // Loaded from System32 - the legitimate location.
                            continue;
                        }

                        // Loaded from anywhere else (e.g. game directory) - the UnityDoorstop signature.
                        yield return var_Path;
                        goto next;
                    }
                }

                // Heuristic: any DLL loaded from one of the configured mod loader folders is suspicious.
                if (!string.IsNullOrEmpty(var_Path) && var_FolderSubstrings.Count > 0)
                {
                    string var_LowerPath = var_Path.Replace('\\', '/').ToLowerInvariant();
                    for (int i = 0; i < var_FolderSubstrings.Count; i++)
                    {
                        if (var_LowerPath.Contains(var_FolderSubstrings[i]))
                        {
                            yield return var_Path;
                            break;
                        }
                    }
                }

                next: ;
            }
        }

        #endregion
    }
}

#endif
