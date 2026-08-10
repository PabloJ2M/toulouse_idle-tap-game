// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

// Unity
using UnityEngine;
using Debug = UnityEngine.Debug;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

// GUPS - AntiCheat
using GUPS.AntiCheat.Detector.Desktop.Platform;
using GUPS.AntiCheat.Settings;

namespace GUPS.AntiCheat.Detector.Desktop
{
    /// <summary>
    /// Detects whether a third party mod loader (BepInEx, MelonLoader, UnityDoorstop, UnityExplorer, ...) has been
    /// injected into the running game.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Performs three kinds of scans: a managed assembly sweep over <see cref="AppDomain.CurrentDomain"/> for
    /// assemblies whose name matches one of the configured prefixes, a native module sweep via
    /// <see cref="IPlatformProbe.GetSuspiciousLoadedModules"/> plus a managed
    /// <see cref="Process.Modules"/> sweep, and a file system sweep next to the game executable for typical mod loader
    /// artifacts.
    /// </para>
    /// <para>
    /// Detection cannot prevent a mod loader from loading - mod loaders run before any C# code in the player. The
    /// detector can only react after the fact by raising the threat level via <see cref="ADetector.Notify"/> so the
    /// configured punisher chain (e.g. <c>ExitGamePunisher</c>) can quit the game.
    /// </para>
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public class ModLoaderDetector : ADetector
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Mod Loader Detector";

        #endregion

        // Platform
        #region Platform

        /// <inheritdoc/>
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || UNITY_EDITOR
        public override bool IsSupported => true;
#else
        public override bool IsSupported => false;
#endif

        #endregion

        // Threat Rating
        #region Threat Rating

        /// <summary>
        /// The false-positive likelihood reported with each detection. Defaults to a low value because the configured
        /// prefixes / file names are very specific.
        /// </summary>
        [SerializeField]
        [Header("Threat Rating - Settings")]
        [Tooltip("The possibility of a false positive when assessing a mod loader detection. Defaults to a low value because the configured prefixes / file names are very specific.")]
        [Range(0f, 1f)]
        private float possibilityOfFalsePositive = 0.02f;

        /// <summary>
        /// Gets the false-positive likelihood reported with each detection.
        /// </summary>
        public float PossibilityOfFalsePositive => this.possibilityOfFalsePositive;

        /// <summary>
        /// The threat rating reported on every detection. Defaults to a high value because mod loaders give the user
        /// complete access to the game (Recommended: 500).
        /// </summary>
        [SerializeField]
        [Tooltip("The threat rating of this detector. Defaults to a high value because mod loaders give the user complete access to the game (Recommended: 500).")]
        private uint threatRating = 500;

        /// <inheritdoc/>
        public override uint ThreatRating { get => this.threatRating; protected set => this.threatRating = value; }

        /// <inheritdoc/>
        public override bool PossibleCheatingDetected { get; protected set; } = false;

        #endregion

        // Observable
        #region Observable

        /// <summary>
        /// Unity event raised on every detection. Useful to wire up reactions through the inspector without writing an
        /// <see cref="IObserver{T}"/>.
        /// </summary>
        [Header("Observable - Settings")]
        [Tooltip("A unity event that is used to subscribe to the cheating detection events. It is useful if you do not want to write custom observers to subscribe to the detectors and simply attach a callback to the detector event through the inspector.")]
        public CheatingDetectionEvent<DesktopCheatingDetectionStatus> OnCheatingDetectionEvent = new CheatingDetectionEvent<DesktopCheatingDetectionStatus>();

        #endregion

        // Observer
        #region Observer

        /// <inheritdoc/>
        public override void OnNext(IWatchedSubject _Subject)
        {
            // Does not observe any subjects.
        }

        /// <inheritdoc/>
        public override void OnError(Exception _Error)
        {
            // Does nothing.
        }

        /// <inheritdoc/>
        public override void OnCompleted()
        {
            // Does nothing.
        }

        #endregion

        // Configuration
        #region Configuration

        /// <summary>
        /// Run the mod loader scan only once on game start. Disable to also run a periodic recheck. The scans are
        /// cheap but parsing every loaded module on every interval is not free.
        /// </summary>
        [Header("Mod Loader - Settings")]
        [Tooltip("Run the mod loader scan only once on game start. Disable to also run a periodic recheck. Recommended: false (rechecks catch late-injected mod loaders).")]
        public bool CheckOnlyOnGameStart = false;

        /// <summary>
        /// Interval in seconds between mod loader scans.
        /// </summary>
        [Tooltip("Interval in seconds between mod loader scans. Recommended: 30")]
        [Range(1f, 600f)]
        public float RecheckIntervalForPossibleCheating = 30f;

        /// <summary>
        /// Shared empty list returned when <see cref="GlobalSettings"/> is unavailable. The detector holds no
        /// built-in fallback lists - <see cref="GlobalSettings"/> is the single source of truth and a missing asset
        /// means nothing is scanned for.
        /// </summary>
        private static readonly String[] EmptyList = new String[0];

        /// <summary>
        /// Returns the configured assembly name prefixes from <see cref="GlobalSettings"/>, or an empty list if the
        /// settings are unavailable.
        /// </summary>
        /// <returns>The assembly name prefixes to scan for.</returns>
        private IList<String> GetAssemblyPrefixes()
        {
            GlobalSettings var_Settings = GlobalSettings.Instance;
            if (var_Settings != null && var_Settings.Desktop_ModLoader_AssemblyPrefixes != null)
            {
                return var_Settings.Desktop_ModLoader_AssemblyPrefixes;
            }
            return EmptyList;
        }

        /// <summary>
        /// Returns the configured file / folder names from <see cref="GlobalSettings"/>, or an empty list if the
        /// settings are unavailable.
        /// </summary>
        /// <returns>The file / folder names to scan for.</returns>
        private IList<String> GetFileNames()
        {
            GlobalSettings var_Settings = GlobalSettings.Instance;
            if (var_Settings != null && var_Settings.Desktop_ModLoader_FileNames != null)
            {
                return var_Settings.Desktop_ModLoader_FileNames;
            }
            return EmptyList;
        }

        /// <summary>
        /// Returns the configured hijack DLL names from <see cref="GlobalSettings"/>, or an empty list if the
        /// settings are unavailable. These DLLs are only suspicious when loaded from outside the Windows system
        /// directory (Unity Doorstop hijacking).
        /// </summary>
        /// <returns>The hijack DLL names to scan for.</returns>
        private IList<String> GetHijackDlls()
        {
            GlobalSettings var_Settings = GlobalSettings.Instance;
            if (var_Settings != null && var_Settings.Desktop_ModLoader_HijackDlls != null)
            {
                return var_Settings.Desktop_ModLoader_HijackDlls;
            }
            return EmptyList;
        }

        #endregion

        // Detection
        #region Detection

        /// <summary>
        /// The cached platform probe used to query OS specific module evidence.
        /// </summary>
        private IPlatformProbe platformProbe;

        /// <summary>
        /// Performs all enabled scans once and notifies observers if any evidence is found.
        /// </summary>
        /// <returns><c>true</c> if any evidence of a mod loader was found; otherwise <c>false</c>.</returns>
        public bool ManualScan()
        {
            GlobalSettings var_Settings = GlobalSettings.Instance;
            if (var_Settings != null && !var_Settings.Desktop_DetectModLoader)
            {
                return false;
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (var_Settings != null && !var_Settings.Desktop_Enable_Development)
            {
                return false;
            }
#endif

            bool var_AnyDetected = false;

            if (this.ScanLoadedAssemblies(out String var_AssemblyEvidence))
            {
                this.OnDetectCheating(EDesktopCheatingType.MOD_LOADER_ASSEMBLY, var_AssemblyEvidence);
                var_AnyDetected = true;
            }

            if (this.ScanLoadedModules(out String var_ModuleEvidence))
            {
                this.OnDetectCheating(EDesktopCheatingType.MOD_LOADER_INJECTOR, var_ModuleEvidence);
                var_AnyDetected = true;
            }

            if (this.ScanFileSystem(out String var_FileEvidence))
            {
                this.OnDetectCheating(EDesktopCheatingType.MOD_LOADER_FILE, var_FileEvidence);
                var_AnyDetected = true;
            }

            return var_AnyDetected;
        }

        /// <summary>
        /// Sweeps <see cref="AppDomain.CurrentDomain"/> for assemblies whose name starts with one of the configured
        /// prefixes.
        /// </summary>
        /// <param name="evidence">On <c>true</c> return, the evidence string describing the matched assembly.</param>
        /// <returns><c>true</c> if a matching assembly was found; otherwise <c>false</c>.</returns>
        private bool ScanLoadedAssemblies(out String evidence)
        {
            evidence = String.Empty;

            try
            {
                IList<String> var_Prefixes = this.GetAssemblyPrefixes();
                Assembly[] var_Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < var_Assemblies.Length; i++)
                {
                    String var_Name;
                    try
                    {
                        var_Name = var_Assemblies[i].GetName().Name;
                    }
                    catch
                    {
                        continue;
                    }

                    if (String.IsNullOrEmpty(var_Name))
                    {
                        continue;
                    }

                    for (int p = 0; p < var_Prefixes.Count; p++)
                    {
                        String var_Prefix = var_Prefixes[p];
                        if (String.IsNullOrEmpty(var_Prefix))
                        {
                            continue;
                        }

                        if (var_Name.StartsWith(var_Prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            evidence = String.Format("Assembly '{0}' matches prefix '{1}'", var_Name, var_Prefix);
                            return true;
                        }
                    }
                }
            }
            catch (Exception var_Exception)
            {
                Debug.LogWarning(String.Format("[GUPS][AntiCheat] ModLoaderDetector failed to enumerate loaded assemblies: {0}", var_Exception.Message));
            }

            return false;
        }

        /// <summary>
        /// Sweeps the native modules of the current process. Asks the platform probe for OS-specific evidence first,
        /// then performs an additional managed sweep against the configured file names.
        /// </summary>
        /// <param name="evidence">On <c>true</c> return, the evidence string describing the matched module.</param>
        /// <returns><c>true</c> if a matching module was found; otherwise <c>false</c>.</returns>
        private bool ScanLoadedModules(out String evidence)
        {
            evidence = String.Empty;

            try
            {
                if (this.platformProbe != null)
                {
                    foreach (String var_Module in this.platformProbe.GetSuspiciousLoadedModules(this.GetFileNames(), this.GetHijackDlls()))
                    {
                        if (!String.IsNullOrEmpty(var_Module))
                        {
                            evidence = String.Format("Suspicious native module '{0}' loaded", var_Module);
                            return true;
                        }
                    }
                }

                ProcessModuleCollection var_Modules;
                try
                {
                    var_Modules = Process.GetCurrentProcess().Modules;
                }
                catch
                {
                    return false;
                }

                IList<String> var_FileNames = this.GetFileNames();
                foreach (ProcessModule var_Module in var_Modules)
                {
                    String var_ModuleName;
                    String var_ModulePath;
                    try
                    {
                        var_ModuleName = var_Module.ModuleName;
                        var_ModulePath = var_Module.FileName;
                    }
                    catch
                    {
                        continue;
                    }

                    if (String.IsNullOrEmpty(var_ModuleName))
                    {
                        continue;
                    }

                    for (int f = 0; f < var_FileNames.Count; f++)
                    {
                        String var_File = var_FileNames[f];
                        if (String.IsNullOrEmpty(var_File) || var_File.EndsWith("/"))
                        {
                            continue;
                        }

                        if (String.Equals(var_ModuleName, var_File, StringComparison.OrdinalIgnoreCase))
                        {
                            evidence = String.Format("Native module '{0}' is in the configured mod loader list (path: '{1}')", var_ModuleName, var_ModulePath ?? "?");
                            return true;
                        }
                    }
                }
            }
            catch (Exception var_Exception)
            {
                Debug.LogWarning(String.Format("[GUPS][AntiCheat] ModLoaderDetector failed to enumerate process modules: {0}", var_Exception.Message));
            }

            return false;
        }

        /// <summary>
        /// Looks for mod loader artifacts next to the game executable, derived from <see cref="Application.dataPath"/>.
        /// </summary>
        /// <param name="evidence">On <c>true</c> return, the evidence string describing the matched file or folder.</param>
        /// <returns><c>true</c> if a matching file or folder was found; otherwise <c>false</c>.</returns>
        private bool ScanFileSystem(out String evidence)
        {
            evidence = String.Empty;

            try
            {
                String var_GameRoot = this.GetGameRootDirectory();
                if (String.IsNullOrEmpty(var_GameRoot) || !Directory.Exists(var_GameRoot))
                {
                    return false;
                }

                IList<String> var_FileNames = this.GetFileNames();
                for (int i = 0; i < var_FileNames.Count; i++)
                {
                    String var_Entry = var_FileNames[i];
                    if (String.IsNullOrEmpty(var_Entry))
                    {
                        continue;
                    }

                    if (var_Entry.EndsWith("/"))
                    {
                        String var_FolderName = var_Entry.TrimEnd('/');
                        String var_FolderPath = Path.Combine(var_GameRoot, var_FolderName);
                        if (Directory.Exists(var_FolderPath))
                        {
                            evidence = String.Format("Folder '{0}' exists next to the game executable", var_FolderPath);
                            return true;
                        }
                    }
                    else
                    {
                        String var_FilePath = Path.Combine(var_GameRoot, var_Entry);
                        if (File.Exists(var_FilePath))
                        {
                            evidence = String.Format("File '{0}' exists next to the game executable", var_FilePath);
                            return true;
                        }
                    }
                }
            }
            catch (Exception var_Exception)
            {
                Debug.LogWarning(String.Format("[GUPS][AntiCheat] ModLoaderDetector failed to scan the game directory: {0}", var_Exception.Message));
            }

            return false;
        }

        /// <summary>
        /// Returns the directory that contains the game executable.
        /// </summary>
        /// <remarks>
        /// <c>Application.dataPath</c> points to the <c>*_Data</c> folder on Windows / Linux and to the
        /// <c>*.app/Contents/Resources/Data</c> folder on macOS, so the parent directory is returned.
        /// </remarks>
        /// <returns>The full path to the game install directory, or an empty string on failure.</returns>
        private String GetGameRootDirectory()
        {
            try
            {
                String var_DataPath = Application.dataPath;
                if (String.IsNullOrEmpty(var_DataPath))
                {
                    return String.Empty;
                }

                DirectoryInfo var_Directory = new DirectoryInfo(var_DataPath);
                if (var_Directory.Parent == null)
                {
                    return var_DataPath;
                }

                return var_Directory.Parent.FullName;
            }
            catch
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Builds a detection status and notifies observers and event listeners.
        /// </summary>
        /// <param name="_Type">The detected type of cheating.</param>
        /// <param name="_Evidence">A short evidence string describing what was found.</param>
        private void OnDetectCheating(EDesktopCheatingType _Type, String _Evidence)
        {
            this.PossibleCheatingDetected = true;

            DesktopCheatingDetectionStatus var_Status = new DesktopCheatingDetectionStatus(this.PossibilityOfFalsePositive, this.ThreatRating, _Type, _Evidence);

            Debug.LogWarning(String.Format("[GUPS][AntiCheat] ModLoaderDetector detected '{0}': {1}", _Type, _Evidence));

            this.Notify(var_Status);
            this.OnCheatingDetectionEvent?.Invoke(var_Status);
        }

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Caches the platform probe, runs the first scan and starts the periodic recheck loop.
        /// </summary>
        protected virtual void Start()
        {
            this.platformProbe = PlatformProbeFactory.Get();

            if (this.IsActive)
            {
                this.ManualScan();
            }

            if (!this.CheckOnlyOnGameStart)
            {
                this.StartCoroutine(this.RecheckLoop());
            }
        }

        /// <summary>
        /// Coroutine that performs a periodic recheck. The first check is already performed in <see cref="Start"/>.
        /// </summary>
        /// <returns>A coroutine enumerator.</returns>
        private IEnumerator RecheckLoop()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(this.RecheckIntervalForPossibleCheating);

                if (this.IsActive)
                {
                    this.ManualScan();
                }
            }
        }

        #endregion
    }
}
