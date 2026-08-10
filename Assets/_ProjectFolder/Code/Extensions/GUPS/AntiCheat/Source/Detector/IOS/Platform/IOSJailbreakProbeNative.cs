// System
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Detector.IOS.Platform
{
    /// <summary>
    /// Native <see cref="IIOSJailbreakProbe"/> implementation that delegates to the bundled Objective-C++ plugin
    /// (<c>libGupsAntiCheatJailbreak</c> / <c>GupsAntiCheatJailbreak.mm</c>) via P/Invoke.
    /// </summary>
    /// <remarks>
    /// Compiles to a stub on every non-iOS platform; on iOS the P/Invoke resolves to <c>__Internal</c> so the symbol
    /// is statically linked into the Unity binary by Xcode. The native side never throws across the ABI boundary -
    /// failures are swallowed and reported as an empty hit list.
    /// </remarks>
    internal sealed class IOSJailbreakProbeNative : IIOSJailbreakProbe
    {
        /// <summary>
        /// Hard cap on the number of hits returned by a single scan. Path and dylib probes emit one hit per matching
        /// entry (not one per category), so the cap is sized generously to avoid truncating evidence on a heavily
        /// jailbroken device.
        /// </summary>
        private const int MaxHits = 32;

        /// <summary>
        /// Native counterpart of <see cref="EIOSJailbreakType"/>. Must stay in sync with <c>GupsJailbreakHit</c> in
        /// <c>GupsAntiCheatJailbreak.h</c>; the evidence buffer is a fixed-size null-terminated UTF-8 string.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct NativeHit
        {
            public int Type;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Evidence;
        }

        /// <summary>
        /// Native counterpart of <c>JailbreakConfig</c>. All arrays are NULL terminated; any pointer may be
        /// <see cref="IntPtr.Zero"/> when the matching list is not configured. The native side holds no built-in
        /// lists - these fields are the only validation input the probes see.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeConfig
        {
            public IntPtr UrlSchemes;
            public IntPtr SuspiciousPaths;
            public IntPtr SuspiciousDylibs;
            public IntPtr DyldAllowedPrefixes;
        }

#if UNITY_IOS && !UNITY_EDITOR

        /// <summary>
        /// P/Invoke binding for <c>GupsAntiCheat_ScanJailbreak</c> exported by the iOS native plugin.
        /// </summary>
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GupsAntiCheat_ScanJailbreak")]
        private static extern int GupsAntiCheat_ScanJailbreak(int enableUrlSchemes,
                                                              int enableSuspiciousPaths,
                                                              int enableSandboxViolation,
                                                              int enableFork,
                                                              int enableDyldInjection,
                                                              int enableSuspiciousDylibs,
                                                              ref NativeConfig config,
                                                              [Out] NativeHit[] outHits,
                                                              int maxHits);

#else

        /// <summary>
        /// Stub used on every non-iOS platform so the assembly still loads. Always returns zero hits.
        /// </summary>
        private static int GupsAntiCheat_ScanJailbreak(int enableUrlSchemes,
                                                      int enableSuspiciousPaths,
                                                      int enableSandboxViolation,
                                                      int enableFork,
                                                      int enableDyldInjection,
                                                      int enableSuspiciousDylibs,
                                                      ref NativeConfig config,
                                                      NativeHit[] outHits,
                                                      int maxHits)
        {
            return 0;
        }

#endif

        /// <inheritdoc/>
        public string Name => "iOS-Native";

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
            NativeHit[] var_Hits = new NativeHit[MaxHits];

            // Allocate the NULL-terminated C string arrays for every validation list. Each AllocAnsi block is freed
            // in the finally so the native side never sees dangling memory and the managed side never leaks.
            IntPtr var_SchemesBuffer = IntPtr.Zero;
            IntPtr[] var_SchemesAlloc = null;
            IntPtr var_PathsBuffer = IntPtr.Zero;
            IntPtr[] var_PathsAlloc = null;
            IntPtr var_DylibsBuffer = IntPtr.Zero;
            IntPtr[] var_DylibsAlloc = null;
            IntPtr var_PrefixesBuffer = IntPtr.Zero;
            IntPtr[] var_PrefixesAlloc = null;

            int var_HitCount = 0;

            try
            {
                NativeConfig var_Config = default;
                var_Config.UrlSchemes = AllocateNullTerminatedAnsiArray(_UrlSchemes, out var_SchemesAlloc, out var_SchemesBuffer);
                var_Config.SuspiciousPaths = AllocateNullTerminatedAnsiArray(_SuspiciousPaths, out var_PathsAlloc, out var_PathsBuffer);
                var_Config.SuspiciousDylibs = AllocateNullTerminatedAnsiArray(_SuspiciousDylibs, out var_DylibsAlloc, out var_DylibsBuffer);
                var_Config.DyldAllowedPrefixes = AllocateNullTerminatedAnsiArray(_DyldAllowedPrefixes, out var_PrefixesAlloc, out var_PrefixesBuffer);

                try
                {
                    var_HitCount = GupsAntiCheat_ScanJailbreak(_DetectUrlSchemes ? 1 : 0,
                                                               _DetectSuspiciousPaths ? 1 : 0,
                                                               _DetectSandboxViolation ? 1 : 0,
                                                               _DetectFork ? 1 : 0,
                                                               _DetectDyldInjection ? 1 : 0,
                                                               _DetectSuspiciousDylibs ? 1 : 0,
                                                               ref var_Config,
                                                               var_Hits,
                                                               MaxHits);
                }
                catch (Exception var_Exception)
                {
                    // The native side does not throw across the ABI; a managed exception here means the plugin is
                    // missing from the build (e.g. the developer built for iOS without dropping the plugin into
                    // Plugins/iOS). Degrade gracefully so the detector still ticks without spamming the player.
                    Debug.LogWarning("[GUPS][AntiCheat] IOSJailbreakProbeNative could not invoke native plugin: " + var_Exception.Message);

                    return Array.Empty<IOSJailbreakDetectionStatus>();
                }
            }
            finally
            {
                FreeAnsiArray(var_SchemesAlloc, var_SchemesBuffer);
                FreeAnsiArray(var_PathsAlloc, var_PathsBuffer);
                FreeAnsiArray(var_DylibsAlloc, var_DylibsBuffer);
                FreeAnsiArray(var_PrefixesAlloc, var_PrefixesBuffer);
            }

            if (var_HitCount <= 0)
            {
                return Array.Empty<IOSJailbreakDetectionStatus>();
            }

            // The threat rating and false-positive likelihood are filled in by the detector based on its inspector
            // configuration; the probe only forwards the (type, evidence) pair returned by the native side.
            IOSJailbreakDetectionStatus[] var_Result = new IOSJailbreakDetectionStatus[var_HitCount];
            for (int i = 0; i < var_HitCount; i++)
            {
                var_Result[i] = new IOSJailbreakDetectionStatus(0f, 0u, (EIOSJailbreakType)var_Hits[i].Type, var_Hits[i].Evidence ?? string.Empty);
            }
            return var_Result;
        }

        /// <summary>
        /// Packs the supplied managed strings into a NULL-terminated array of ANSI-encoded pointers suitable for the
        /// native <c>const char* const*</c> parameter. Returns <see cref="IntPtr.Zero"/> when the input is empty.
        /// </summary>
        /// <param name="_Source">The managed string list. May be <c>null</c> or empty.</param>
        /// <param name="_StringPointers">Receives the individual <see cref="Marshal.StringToHGlobalAnsi"/> allocations so they can be freed.</param>
        /// <param name="_Buffer">Receives the pointer to the contiguous pointer array, or <see cref="IntPtr.Zero"/>.</param>
        /// <returns>The pointer to pass to the native side. Equal to <paramref name="_Buffer"/>.</returns>
        private static IntPtr AllocateNullTerminatedAnsiArray(IReadOnlyList<string> _Source, out IntPtr[] _StringPointers, out IntPtr _Buffer)
        {
            _StringPointers = null;
            _Buffer = IntPtr.Zero;

            if (_Source == null || _Source.Count == 0)
            {
                return IntPtr.Zero;
            }

            // Filter out null / whitespace entries up front so the native side never sees a NULL ptr in the middle
            // (which would terminate the array prematurely and silently drop the remaining entries).
            List<string> var_Filtered = new List<string>(_Source.Count);
            for (int i = 0; i < _Source.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(_Source[i]))
                {
                    var_Filtered.Add(_Source[i]);
                }
            }

            if (var_Filtered.Count == 0)
            {
                return IntPtr.Zero;
            }

            _StringPointers = new IntPtr[var_Filtered.Count];
            for (int i = 0; i < var_Filtered.Count; i++)
            {
                _StringPointers[i] = Marshal.StringToHGlobalAnsi(var_Filtered[i]);
            }

            // The array layout is { ptr_0, ptr_1, ..., ptr_n-1, NULL } so the native side can iterate until it sees
            // a zero pointer without needing a separate count parameter.
            _Buffer = Marshal.AllocHGlobal((var_Filtered.Count + 1) * IntPtr.Size);
            for (int i = 0; i < var_Filtered.Count; i++)
            {
                Marshal.WriteIntPtr(_Buffer, i * IntPtr.Size, _StringPointers[i]);
            }
            Marshal.WriteIntPtr(_Buffer, var_Filtered.Count * IntPtr.Size, IntPtr.Zero);

            return _Buffer;
        }

        /// <summary>
        /// Frees the pointer array and the individual ANSI strings allocated by
        /// <see cref="AllocateNullTerminatedAnsiArray"/>.
        /// </summary>
        /// <param name="_StringPointers">The individual string allocations.</param>
        /// <param name="_Buffer">The contiguous pointer array.</param>
        private static void FreeAnsiArray(IntPtr[] _StringPointers, IntPtr _Buffer)
        {
            if (_StringPointers != null)
            {
                for (int i = 0; i < _StringPointers.Length; i++)
                {
                    if (_StringPointers[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(_StringPointers[i]);
                    }
                }
            }

            if (_Buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_Buffer);
            }
        }
    }
}
