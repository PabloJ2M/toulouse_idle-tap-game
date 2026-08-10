// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

// GUPS - AntiCheat
using GUPS.AntiCheat.Monitor.Android;
using GUPS.AntiCheat.Settings;

namespace GUPS.AntiCheat.Detector.Android
{
    /// <summary>
    /// Detects when the Android device has unallowed (blacklisted) applications installed that can be used to cheat.
    /// </summary>
    /// <remarks>
    /// Subscribes on awake to the <see cref="AndroidInstalledApplicationMonitor"/> sitting on the same
    /// <see cref="GameObject"/> and forwards any detection through <see cref="ADetector.Notify"/> and the
    /// inspector-friendly <see cref="OnCheatingDetectionEvent"/>.
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public class AndroidDeviceCheatingDetector : ADetector
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Android Device Cheating Detector";

        #endregion

        // Platform
        #region Platform

        /// <inheritdoc/>
#if UNITY_ANDROID
        public override bool IsSupported => true;
#else
        public override bool IsSupported => false;
#endif

        #endregion

        // Threat Rating
        #region Threat Rating

        /// <summary>
        /// Gets the false-positive likelihood reported with each detection.
        /// </summary>
        public float PossibilityOfFalsePositive => 0.01f;

        /// <summary>
        /// The threat rating reported on every detection (Recommended: 500).
        /// </summary>
        [SerializeField]
        [Header("Threat Rating - Settings")]
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
        /// Unity event raised on every detection. Useful to wire up reactions through the inspector without writing an
        /// <see cref="IObserver{T}"/>.
        /// </summary>
        [Header("Observable - Settings")]
        [Tooltip("A unity event that is used to subscribe to the cheating detection events. It is useful if you do not want to write custom observers to subscribe to the detectors and simply attach a callback to the detector event through the inspector.")]
        public CheatingDetectionEvent<AndroidCheatingDetectionStatus> OnCheatingDetectionEvent = new CheatingDetectionEvent<AndroidCheatingDetectionStatus>();

        #endregion

        // Observer
        #region Observer

        /// <summary>
        /// Receives status updates from the subscribed Android device monitors and dispatches them to the matching
        /// validation coroutine.
        /// </summary>
        /// <param name="_Subject">The status published by the source monitor.</param>
        public override void OnNext(IWatchedSubject _Subject)
        {
            // Drop the notification if the detector is disabled or the subject is from an unrelated monitor.
            if (!this.IsActive)
            {
                return;
            }

            if (!(_Subject is IAndroidStatus))
            {
                return;
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR

            // Honor the development build switch from the global settings.
            if (!GlobalSettings.Instance.Android_Enable_Development)
            {
                return;
            }

#endif

            // Dispatch the matching validation coroutine for the concrete Android status type.
            if (_Subject is AndroidInstalledApplicationStatus applicationStatus)
            {
                this.StartCoroutine(this.ValidateDeviceApplications(applicationStatus));
            }
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

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Subscribes to the Android device monitors on the same <see cref="GameObject"/>.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Look up the installed-application monitor expected to live on the same GameObject and subscribe to it
            // for future application list updates.
            this.installedApplicationMonitor = this.GetComponent<AndroidInstalledApplicationMonitor>();

            if (this.installedApplicationMonitor != null)
            {
                this.installedApplicationMonitor.Subscribe(this);
            }
        }

        #endregion

        // Device Apps
        #region Device Apps

        /// <summary>
        /// The installed-application monitor this detector observes, if any.
        /// </summary>
        private AndroidInstalledApplicationMonitor installedApplicationMonitor;

        /// <summary>
        /// Builds a detection status and notifies observers and event listeners.
        /// </summary>
        /// <param name="_AndroidCheatingType">The detected type of cheating.</param>
        /// <param name="_FailedToRetrieveData">Whether the source monitor failed to retrieve its data.</param>
        private void OnDetectCheating(EAndroidCheatingType _AndroidCheatingType, bool _FailedToRetrieveData)
        {
            this.PossibleCheatingDetected = true;

            // Build the detection status; raise the false-positive likelihood when the monitor could not retrieve
            // its data, because the verdict is then based on missing information.
            AndroidCheatingDetectionStatus var_DetectionStatus = new AndroidCheatingDetectionStatus(_FailedToRetrieveData ? 0.75f : this.PossibilityOfFalsePositive, this.ThreatRating, _AndroidCheatingType, _FailedToRetrieveData);

            // Broadcast to managed observers (the punisher chain) and inspector-wired Unity event listeners.
            this.Notify(var_DetectionStatus);

            this.OnCheatingDetectionEvent?.Invoke(var_DetectionStatus);
        }

        /// <summary>
        /// Validates the installed applications against the configured blacklist and notifies observers on a match.
        /// </summary>
        /// <param name="_ApplicationStatus">The installed-application status to validate.</param>
        /// <returns>A coroutine enumerator.</returns>
        private IEnumerator ValidateDeviceApplications(AndroidInstalledApplicationStatus _ApplicationStatus)
        {
            // Skip the check when the global settings have blacklisting turned off.
            if (!GlobalSettings.Instance.Android_UseBlacklistingforApplication)
            {
                Debug.Log(String.Format("[GUPS][AntiCheat] Blacklisting of applications is disabled!"));

                yield break;
            }

            // Treat retrieval failure as suspicious - the monitor side already detected the underlying issue.
            if (_ApplicationStatus.FailedToRetrieveData)
            {
                Debug.LogWarning("[GUPS][AntiCheat] The installed applications on the device could not be retrieved!");

                this.OnDetectCheating(EAndroidCheatingType.DEVICE_INSTALLED_APPS, true);

                yield break;
            }

            bool var_UnallowedApps = false;

            // The monitor already filtered the device's installed apps down to the blacklisted matches, so any
            // remaining entry is a hit.
            List<String> var_FoundBlacklistedApplications = new List<String>(_ApplicationStatus.Applications);

            if (var_FoundBlacklistedApplications.Count > 0)
            {
                foreach (String var_App in var_FoundBlacklistedApplications)
                {
                    Debug.LogWarning(String.Format("[GUPS][AntiCheat] The installed app '{0}' is blacklisted!", var_App));
                }

                var_UnallowedApps = true;
            }

            // No matches: the device is clean, nothing to report.
            if (!var_UnallowedApps)
            {
                Debug.Log(String.Format("[GUPS][AntiCheat] No unallowed applications found on the device!"));

                yield break;
            }

            // At least one blacklisted app was found - escalate as device cheating.
            this.OnDetectCheating(EAndroidCheatingType.DEVICE_INSTALLED_APPS, false);
        }

        #endregion
    }
}
