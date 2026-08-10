// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

// GUPS - AntiCheat
using GUPS.AntiCheat.Detector.IOS.Platform;
using GUPS.AntiCheat.Settings;

namespace GUPS.AntiCheat.Detector.IOS
{
    /// <summary>
    /// Detects whether the running iOS device is jailbroken using native heuristics (URL schemes, suspicious paths,
    /// sandbox violation, optional <c>fork()</c>, <c>DYLD_INSERT_LIBRARIES</c>, and loaded tweak dylibs).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Delegates the actual scanning to a native Objective-C++ plugin (<c>libGupsAntiCheatJailbreak</c>) through
    /// <see cref="IIOSJailbreakProbe"/>. The probe is selected at compile time via
    /// <see cref="IOSJailbreakProbeFactory.Get"/>; in the editor and on every non-iOS platform a no-op probe is used.
    /// </para>
    /// <para>
    /// Detection categories can be toggled individually through the global AntiCheat settings (Project Settings -
    /// GuardingPearSoftware - AntiCheat - iOS). The detector reports each distinct <see cref="EIOSJailbreakType"/>
    /// only once per game session to keep the punisher chain quiet on devices that match several categories at the
    /// same time.
    /// </para>
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public class IOSJailbreakDetector : ADetector
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "iOS Jailbreak Detector";

        #endregion

        // Platform
        #region Platform

        /// <inheritdoc/>
#if UNITY_IOS
        public override bool IsSupported => true;
#else
        public override bool IsSupported => false;
#endif

        #endregion

        // Threat Rating
        #region Threat Rating

        /// <summary>
        /// The false-positive likelihood reported with every detection. Defaults to a low value because every
        /// native probe checks for evidence that has no legitimate cause on a stock iOS device.
        /// </summary>
        [SerializeField]
        [Header("Threat Rating - Settings")]
        [Tooltip("The possibility of a false positive when assessing a jailbreak detection. Defaults to 0.01 because every probe checks for evidence that has no legitimate cause on a stock iOS device.")]
        [Range(0f, 1f)]
        private float possibilityOfFalsePositive = 0.01f;

        /// <summary>
        /// Gets the false-positive likelihood reported with each detection.
        /// </summary>
        public float PossibilityOfFalsePositive => this.possibilityOfFalsePositive;

        /// <summary>
        /// The threat rating reported on every detection (Recommended: 500).
        /// </summary>
        [SerializeField]
        [Tooltip("The threat rating of this detector. It is set to a very high value, because false positives are very unlikely and the impact of cheating is very high (Recommended: 500).")]
        private uint threatRating = 500;

        /// <inheritdoc/>
        public override uint ThreatRating { get => this.threatRating; protected set => this.threatRating = value; }

        /// <inheritdoc/>
        public override bool PossibleCheatingDetected { get; protected set; } = false;

        #endregion

        // Observable
        #region Observable

        /// <summary>
        /// Unity event raised on every detection. Useful to wire up reactions through the inspector without writing
        /// an <see cref="IObserver{T}"/>.
        /// </summary>
        [Header("Observable - Settings")]
        [Tooltip("A unity event that is used to subscribe to the cheating detection events. It is useful if you do not want to write custom observers to subscribe to the detectors and simply attach a callback to the detector event through the inspector.")]
        public CheatingDetectionEvent<IOSJailbreakDetectionStatus> OnCheatingDetectionEvent = new CheatingDetectionEvent<IOSJailbreakDetectionStatus>();

        #endregion

        // Observer
        #region Observer

        /// <inheritdoc/>
        public override void OnNext(IWatchedSubject _Subject)
        {
            // Does not observe any subjects - the detector queries the native plugin directly.
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
        /// Run the jailbreak scan only once on game start. Disable to also run a periodic recheck.
        /// </summary>
        [Header("Jailbreak - Settings")]
        [Tooltip("Run the jailbreak scan only once on game start. Disable to also run a periodic recheck. Recommended: false (a tweak framework can be loaded at any time on a jailbroken device).")]
        public bool CheckOnlyOnGameStart = false;

        /// <summary>
        /// Interval in seconds between jailbreak scans.
        /// </summary>
        [Tooltip("Interval in seconds between jailbreak scans. Recommended: 30")]
        [Range(0.1f, 600f)]
        public float RecheckIntervalForPossibleCheating = 30f;

        #endregion

        // Detection
        #region Detection

        /// <summary>
        /// The cached probe used to query the native plugin.
        /// </summary>
        private IIOSJailbreakProbe probe;

        /// <summary>
        /// Tracks which categories have already been reported so the same evidence does not fire the punisher chain
        /// on every recheck.
        /// </summary>
        private readonly HashSet<EIOSJailbreakType> reported = new HashSet<EIOSJailbreakType>();

        /// <summary>
        /// Performs a single jailbreak scan and notifies observers and event listeners for every new hit.
        /// </summary>
        /// <returns><c>true</c> if at least one new hit was detected; otherwise <c>false</c>.</returns>
        public bool ManualScan()
        {
            // Probe lookup is lazy so the detector still works when ManualScan is called before Start (e.g. from a
            // bootstrap script that runs in Awake order).
            if (this.probe == null)
            {
                this.probe = IOSJailbreakProbeFactory.Get();
            }

            GlobalSettings var_Settings = GlobalSettings.Instance;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            // Honor the global development build switch - exactly the same pattern as AndroidDeviceCheatingDetector
            // and the desktop detectors. Without this every developer would get a detection in the editor when they
            // dropped the prefab into a scene.
            if (var_Settings != null && !var_Settings.IOS_Enable_Development)
            {
                return false;
            }
#endif

            // Resolve every toggle from the global settings, defaulting to the safer (more aggressive) value when no
            // settings asset is present.
            bool var_DetectUrlSchemes = var_Settings == null || var_Settings.IOS_DetectUrlSchemes;
            bool var_DetectSuspiciousPaths = var_Settings == null || var_Settings.IOS_DetectSuspiciousPaths;
            bool var_DetectSandboxViolation = var_Settings == null || var_Settings.IOS_DetectSandboxViolation;
            // The fork probe is intentionally locked off for now.
            // App Store review heuristics flag binaries that check for forks, and we want to ship the safe baseline
            // first. The toggle will be re-enabled in a future release once the feature had been gone through more
            // testing and auditing.
            bool var_DetectFork = false;
            bool var_DetectDyldInjection = var_Settings == null || var_Settings.IOS_DetectDyldInjection;
            bool var_DetectSuspiciousDylibs = var_Settings == null || var_Settings.IOS_DetectSuspiciousDylibs;

            // All validation lists come from the settings asset - the native side holds no built-in lists. Without
            // the asset there is nothing to validate against, so the list-based probes simply report no hits.
            IReadOnlyList<string> var_UrlSchemes = var_Settings?.IOS_SuspiciousUrlSchemes;
            IReadOnlyList<string> var_SuspiciousPaths = var_Settings?.IOS_SuspiciousPaths;
            IReadOnlyList<string> var_SuspiciousDylibs = var_Settings?.IOS_SuspiciousDylibs;
            IReadOnlyList<string> var_DyldAllowedPrefixes = var_Settings?.IOS_DyldAllowedPrefixes;

            IReadOnlyList<IOSJailbreakDetectionStatus> var_Hits = this.probe.Scan(var_DetectUrlSchemes,
                                                                                  var_DetectSuspiciousPaths,
                                                                                  var_DetectSandboxViolation,
                                                                                  var_DetectFork,
                                                                                  var_DetectDyldInjection,
                                                                                  var_DetectSuspiciousDylibs,
                                                                                  var_UrlSchemes,
                                                                                  var_SuspiciousPaths,
                                                                                  var_SuspiciousDylibs,
                                                                                  var_DyldAllowedPrefixes);

            if (var_Hits == null || var_Hits.Count == 0)
            {
                return false;
            }

            bool var_AnyNew = false;

            for (int i = 0; i < var_Hits.Count; i++)
            {
                IOSJailbreakDetectionStatus var_Hit = var_Hits[i];

                // Dedupe by type - the native side already reports at most one hit per category, but a recheck can
                // produce the same category again on the next pass. Reporting once per game session is enough.
                if (!this.reported.Add(var_Hit.JailbreakType))
                {
                    continue;
                }

                this.OnDetectCheating(var_Hit.JailbreakType, var_Hit.Evidence);

                var_AnyNew = true;
            }

            return var_AnyNew;
        }

        /// <summary>
        /// Builds a detection status and notifies observers and event listeners.
        /// </summary>
        /// <param name="_Type">The detected type of jailbreak evidence.</param>
        /// <param name="_Evidence">A short evidence string describing what was found.</param>
        private void OnDetectCheating(EIOSJailbreakType _Type, String _Evidence)
        {
            this.PossibleCheatingDetected = true;

            IOSJailbreakDetectionStatus var_Status = new IOSJailbreakDetectionStatus(this.possibilityOfFalsePositive,
                                                                                    this.threatRating,
                                                                                    _Type,
                                                                                    _Evidence);

            UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] IOSJailbreakDetector detected '{0}': {1}", _Type, _Evidence));

            // Broadcast to managed observers (the punisher chain) and inspector-wired Unity event listeners.
            this.Notify(var_Status);
            this.OnCheatingDetectionEvent?.Invoke(var_Status);
        }

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Caches the platform probe, runs the first scan and starts the periodic recheck loop when configured.
        /// </summary>
        protected virtual void Start()
        {
            this.probe = IOSJailbreakProbeFactory.Get();

            // Run the first scan immediately so detection fires before the player can do anything meaningful.
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
                // Use realtime so a tampered Time.timeScale cannot stall the loop.
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
