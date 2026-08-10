// System
using System;
using System.Collections;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

// GUPS - AntiCheat
using GUPS.AntiCheat.Detector.Desktop.Platform;
using GUPS.AntiCheat.Settings;

namespace GUPS.AntiCheat.Detector.Desktop
{
    /// <summary>
    /// Detects whether a user mode or kernel mode debugger is attached to the running game.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each scan queries <see cref="System.Diagnostics.Debugger.IsAttached"/> for managed debuggers (Visual Studio,
    /// Rider, dnSpy), <see cref="IPlatformProbe.IsUserModeDebuggerPresent"/> for native user mode debuggers (x64dbg,
    /// OllyDbg, ptrace on Linux/macOS) and <see cref="IPlatformProbe.IsKernelModeDebuggerPresent"/> for kernel mode
    /// debuggers (WinDbg in kernel mode, Syser, SoftICE on Windows).
    /// </para>
    /// <para>
    /// User mode and kernel mode evidence carry separate threat ratings because a kernel debugger on a player machine
    /// is far more suspicious than a managed debugger attached during development.
    /// </para>
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public class DebuggerDetector : ADetector
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Debugger Detector";

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
        /// The false-positive likelihood for user mode debugger detections. Non-zero because the managed
        /// <see cref="System.Diagnostics.Debugger.IsAttached"/> fires when a developer attaches their own IDE.
        /// </summary>
        [SerializeField]
        [Header("Threat Rating - Settings")]
        [Tooltip("The possibility of a false positive when assessing a user mode debugger detection.")]
        [Range(0f, 1f)]
        private float possibilityOfFalsePositiveUserMode = 0.05f;

        /// <summary>
        /// The false-positive likelihood for kernel mode debugger detections. Defaults to near zero because kernel
        /// debuggers are almost never present on a normal player machine.
        /// </summary>
        [SerializeField]
        [Tooltip("The possibility of a false positive when assessing a kernel mode debugger detection. Kernel debuggers are extremely rare on player machines.")]
        [Range(0f, 1f)]
        private float possibilityOfFalsePositiveKernelMode = 0.001f;

        /// <summary>
        /// The threat rating reported on user mode debugger detections (Recommended: 400).
        /// </summary>
        [SerializeField]
        [Tooltip("The threat rating for a user mode debugger detection (Recommended: 400).")]
        private uint threatRatingUserMode = 400;

        /// <summary>
        /// The threat rating reported on kernel mode debugger detections. Higher than the user mode rating because
        /// kernel debugging is usually a strong sign of memory tampering tools (Recommended: 750).
        /// </summary>
        [SerializeField]
        [Tooltip("The threat rating for a kernel mode debugger detection (Recommended: 750).")]
        private uint threatRatingKernelMode = 750;

        /// <summary>
        /// Gets the larger of the user mode and kernel mode threat ratings, satisfying the <see cref="ADetector"/> contract.
        /// </summary>
        public override uint ThreatRating
        {
            get => Math.Max(this.threatRatingUserMode, this.threatRatingKernelMode);
            protected set => this.threatRatingUserMode = value;
        }

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
        [Tooltip("A unity event that is used to subscribe to the cheating detection events.")]
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
        /// Run the debugger check only once on game start. Disable to also run a periodic recheck.
        /// </summary>
        [Header("Debugger - Settings")]
        [Tooltip("Run the debugger check only once on game start. Disable to also run a periodic recheck. Recommended: false (a debugger can be attached at any time).")]
        public bool CheckOnlyOnGameStart = false;

        /// <summary>
        /// Interval in seconds between debugger checks. Lower than the mod loader interval because attaching a
        /// debugger mid-game is a common workflow.
        /// </summary>
        [Tooltip("Interval in seconds between debugger checks. Recommended: 5")]
        [Range(0.1f, 600f)]
        public float RecheckIntervalForPossibleCheating = 5f;

        #endregion

        // Detection
        #region Detection

        /// <summary>
        /// The cached platform probe used to query OS specific debugger evidence.
        /// </summary>
        private IPlatformProbe platformProbe;

        /// <summary>
        /// Whether a user mode debugger has already been reported. Prevents spamming the punisher chain on every recheck.
        /// </summary>
        private bool reportedUserMode;

        /// <summary>
        /// Whether a kernel mode debugger has already been reported. Prevents spamming the punisher chain on every recheck.
        /// </summary>
        private bool reportedKernelMode;

        /// <summary>
        /// Performs all enabled checks once and notifies observers if any evidence is found.
        /// </summary>
        /// <returns><c>true</c> if any evidence of a debugger was found; otherwise <c>false</c>.</returns>
        public bool ManualScan()
        {
            GlobalSettings var_Settings = GlobalSettings.Instance;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (var_Settings != null && !var_Settings.Desktop_Enable_Development)
            {
                return false;
            }
#endif

            // The global settings expose individual toggles per kind. Default to true if the asset is missing.
            bool var_DetectUserMode = var_Settings == null || var_Settings.Desktop_DetectUserModeDebugger;
            bool var_DetectKernelMode = var_Settings == null || var_Settings.Desktop_DetectKernelModeDebugger;
            bool var_ReportManagedOnly = var_Settings == null || var_Settings.Desktop_Debugger_ReportManagedOnly;

            bool var_AnyDetected = false;

            // Probe-driven sweep: ask the OS-specific probe for native debugger evidence and report once per kind.
            if (this.platformProbe != null)
            {
                if (var_DetectKernelMode && this.platformProbe.IsKernelModeDebuggerPresent(out String var_KernelEvidence))
                {
                    if (!this.reportedKernelMode)
                    {
                        this.reportedKernelMode = true;
                        this.OnDetectCheating(EDesktopCheatingType.DEBUGGER_KERNEL_MODE, this.threatRatingKernelMode, this.possibilityOfFalsePositiveKernelMode, var_KernelEvidence);
                    }
                    var_AnyDetected = true;
                }

                if (var_DetectUserMode && this.platformProbe.IsUserModeDebuggerPresent(out String var_UserEvidence))
                {
                    if (!this.reportedUserMode)
                    {
                        this.reportedUserMode = true;
                        this.OnDetectCheating(EDesktopCheatingType.DEBUGGER_USER_MODE, this.threatRatingUserMode, this.possibilityOfFalsePositiveUserMode, var_UserEvidence);
                    }
                    var_AnyDetected = true;
                }
            }

            // Even on platforms without a probe (or when the probe missed it) the managed runtime can still report a
            // .NET debugger attached. Treated as a separate, lighter signal.
            if (var_DetectUserMode && var_ReportManagedOnly)
            {
                if (System.Diagnostics.Debugger.IsAttached || System.Diagnostics.Debugger.IsLogging())
                {
                    if (!this.reportedUserMode)
                    {
                        this.reportedUserMode = true;
                        String var_Source = System.Diagnostics.Debugger.IsAttached ? "Debugger.IsAttached" : "Debugger.IsLogging";
                        this.OnDetectCheating(EDesktopCheatingType.DEBUGGER_USER_MODE, this.threatRatingUserMode, this.possibilityOfFalsePositiveUserMode, var_Source);
                    }
                    var_AnyDetected = true;
                }
            }

            return var_AnyDetected;
        }

        /// <summary>
        /// Builds a detection status and notifies observers and event listeners.
        /// </summary>
        /// <param name="_Type">The detected type of cheating.</param>
        /// <param name="_ThreatRating">The threat rating reported with the detection.</param>
        /// <param name="_PossibilityOfFalsePositive">The false-positive likelihood reported with the detection.</param>
        /// <param name="_Evidence">A short evidence string describing what was found.</param>
        private void OnDetectCheating(EDesktopCheatingType _Type, uint _ThreatRating, float _PossibilityOfFalsePositive, String _Evidence)
        {
            this.PossibleCheatingDetected = true;

            // Package the detection (kind, rating, evidence) into a status DTO for downstream observers.
            DesktopCheatingDetectionStatus var_Status = new DesktopCheatingDetectionStatus(_PossibilityOfFalsePositive, _ThreatRating, _Type, _Evidence);

            UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] DebuggerDetector detected '{0}': {1}", _Type, _Evidence));

            // Broadcast to managed observers and inspector-wired Unity event listeners.
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
            // Cache the matching OS-specific probe once - probe lookup is otherwise repeated on every scan.
            this.platformProbe = PlatformProbeFactory.Get();

            // Run the first scan immediately so detection fires before the player can do anything.
            if (this.IsActive)
            {
                this.ManualScan();
            }

            // Schedule the periodic recheck loop when the user has not opted into start-only scanning.
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
                // Wait the configured interval using realtime so a tampered Time.timeScale cannot stall the loop.
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
