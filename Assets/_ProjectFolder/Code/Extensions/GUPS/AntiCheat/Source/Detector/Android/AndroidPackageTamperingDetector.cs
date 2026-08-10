// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEngine.Networking;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

// GUPS - AntiCheat
using GUPS.AntiCheat.Monitor.Android;
using GUPS.AntiCheat.Settings;

namespace GUPS.AntiCheat.Detector.Android
{
    /// <summary>
    /// Detects tampering with the Android package (APK/AAB) by validating installation source, hash, fingerprint and
    /// loaded native libraries.
    /// </summary>
    /// <remarks>
    /// Subscribes on awake to the package monitors sitting on the same <see cref="GameObject"/>:
    /// <see cref="AndroidPackageSourceMonitor"/>, <see cref="AndroidPackageHashMonitor"/>,
    /// <see cref="AndroidPackageFingerprintMonitor"/> and <see cref="AndroidPackageLibraryMonitor"/>.
    /// Each detection is forwarded through <see cref="ADetector.Notify"/> and the inspector-friendly
    /// <see cref="OnCheatingDetectionEvent"/>.
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public class AndroidPackageTamperingDetector : ADetector
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Android Package Tampering Detector";

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
        /// Receives status updates from the subscribed Android package monitors and dispatches them to the matching
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

            if (!(_Subject is IAndroidStatus var_AndroidStatus))
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

            // Dispatch the matching validation coroutine for the concrete Android package status type.
            if (_Subject is AndroidSourceStatus var_SourceStatus)
            {
                this.StartCoroutine(this.ValidatePackageSource(var_SourceStatus));
            }
            else if(_Subject is AndroidHashStatus var_HashStatus)
            {
                this.StartCoroutine(this.ValidatePackageHash(var_HashStatus));
            }
            else if(_Subject is AndroidFingerprintStatus var_FingerprintStatus)
            {
                this.StartCoroutine(this.ValidatePackageFingerprint(var_FingerprintStatus));
            }
            else if(_Subject is AndroidLibraryStatus var_LibraryStatus)
            {
                this.StartCoroutine(this.ValidatePackageLibrary(var_LibraryStatus));
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
        /// Subscribes to the Android package monitors on the same <see cref="GameObject"/>.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Look up every package monitor expected to live on the same GameObject and subscribe to whichever ones
            // are present - missing monitors simply disable that particular check.
            this.packageSourceMonitor = this.GetComponent<AndroidPackageSourceMonitor>();

            if (this.packageSourceMonitor != null)
            {
                this.packageSourceMonitor.Subscribe(this);
            }

            this.packageHashMonitor = this.GetComponent<AndroidPackageHashMonitor>();

            if (this.packageHashMonitor != null)
            {
                this.packageHashMonitor.Subscribe(this);
            }

            this.packageFingerprintMonitor = this.GetComponent<AndroidPackageFingerprintMonitor>();

            if (this.packageFingerprintMonitor != null)
            {
                this.packageFingerprintMonitor.Subscribe(this);
            }

            this.packageLibraryMonitor = this.GetComponent<AndroidPackageLibraryMonitor>();

            if (this.packageLibraryMonitor != null)
            {
                this.packageLibraryMonitor.Subscribe(this);
            }
        }

        #endregion

        // Package Tampering
        #region Package Tampering

        /// <summary>
        /// Placeholder in the configured hash endpoint URL that is replaced with <see cref="Application.version"/>.
        /// </summary>
        private const String CHashVersionParameter = "{version}";

        /// <summary>
        /// The package source monitor this detector observes, if any.
        /// </summary>
        private AndroidPackageSourceMonitor packageSourceMonitor;

        /// <summary>
        /// The package hash monitor this detector observes, if any.
        /// </summary>
        private AndroidPackageHashMonitor packageHashMonitor;

        /// <summary>
        /// The package fingerprint monitor this detector observes, if any.
        /// </summary>
        private AndroidPackageFingerprintMonitor packageFingerprintMonitor;

        /// <summary>
        /// The package library monitor this detector observes, if any.
        /// </summary>
        private AndroidPackageLibraryMonitor packageLibraryMonitor;

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
        /// Validates the package installation source against the global settings and notifies observers on a mismatch.
        /// </summary>
        /// <param name="_SourceStatus">The source status to validate.</param>
        /// <returns>A coroutine enumerator.</returns>
        private IEnumerator ValidatePackageSource(AndroidSourceStatus _SourceStatus)
        {
            // Short-circuit when the global settings allow any installation source.
            if(GlobalSettings.Instance.Android_AllowAllAppStores)
            {
                Debug.Log(String.Format("[GUPS][AntiCheat] All app stores are allowed. The installation source '{0}' is allowed!", _SourceStatus.AppStoreSource.ToString()));

                yield break;
            }

            // Treat retrieval failure as suspicious - the underlying native call could not classify the source.
            if (_SourceStatus.FailedToRetrieveData)
            {
                Debug.LogWarning("[GUPS][AntiCheat] The installation source could not be retrieved!");

                this.OnDetectCheating(EAndroidCheatingType.PACKAGE_SOURCE, true);

                yield break;
            }

            // First whitelist match: known app store enum (Google Play, Amazon, ...).
            if (GlobalSettings.Instance.Android_AllowedAppStores.Contains(_SourceStatus.AppStoreSource))
            {
                Debug.Log(String.Format("[GUPS][AntiCheat] The installation source '{0}' is allowed!", _SourceStatus.AppStoreSource.ToString()));

                yield break;
            }

            // Second whitelist match: custom app store identified by its installer package name.
            if(GlobalSettings.Instance.Android_AllowedCustomAppStores.Contains(_SourceStatus.AppStoreSourcePackage))
            {
                Debug.Log(String.Format("[GUPS][AntiCheat] The installation source '{0}' is allowed!", _SourceStatus.AppStoreSourcePackage));

                yield break;
            }

            // No whitelist matched - the install came from a non-approved store (e.g. sideloaded APK).
            Debug.LogWarning(String.Format("[GUPS][AntiCheat] The installation source '{0}' is not allowed!", _SourceStatus.AppStoreSource.ToString()));

            this.OnDetectCheating(EAndroidCheatingType.PACKAGE_SOURCE, false);
        }

        /// <summary>
        /// Validates the package hash against the remote endpoint configured in the global settings.
        /// </summary>
        /// <param name="_HashStatus">The hash status to validate.</param>
        /// <returns>A coroutine enumerator.</returns>
        private IEnumerator ValidatePackageHash(AndroidHashStatus _HashStatus)
        {
            // Skip the check when hash verification is disabled in the global settings.
            if(!GlobalSettings.Instance.Android_VerifyAppHash)
            {
                Debug.Log(String.Format("[GUPS][AntiCheat] The app hash verification is disabled. The hash '{0}' is not validated!", _HashStatus.Hash));

                yield break;
            }

            // Treat retrieval failure as suspicious - cannot compare a missing hash against the remote expectation.
            if (_HashStatus.FailedToRetrieveData)
            {
                Debug.LogWarning("[GUPS][AntiCheat] The app hash could not be retrieved!");

                this.OnDetectCheating(EAndroidCheatingType.PACKAGE_HASH, true);

                yield break;
            }

            // Get the endpoint from the global settings to request the hash from.
            String var_HashEndpoint = GlobalSettings.Instance.Android_AppHashEndpoint;

            // Replace the version parameter in the url with the current Application.version. To set the version in
            // Unity, go to Edit > Project Settings > Player (same as PlayerSettings.bundleVersion).
            var_HashEndpoint = var_HashEndpoint.ToLower().Replace(CHashVersionParameter, Application.version);

            using (UnityWebRequest var_Request = UnityWebRequest.Get(var_HashEndpoint))
            {
                // Fire the GET and yield until the web request completes (Unity coroutine pattern).
                var var_RequestWaiter = var_Request.SendWebRequest();

                yield return var_RequestWaiter;

                // Network or HTTP failure: cannot validate, treat as suspicious.
                if (var_Request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(String.Format("[GUPS][AntiCheat] Failed to request hash from server '{0}' with error: {1}", var_HashEndpoint, var_Request.error));

                    this.OnDetectCheating(EAndroidCheatingType.PACKAGE_HASH, true);

                    yield break;
                }

                String var_DownloadedHash = var_Request.downloadHandler.text;

                var_DownloadedHash = var_DownloadedHash?.Trim() ?? "";

                // Normalize the hashes by removing the common separators ('-', ' ', ':').
                var_DownloadedHash = var_DownloadedHash.Replace("-", "").Replace(" ", "").Replace(":", "");

                String var_CompareHash = _HashStatus.Hash?.Trim() ?? "";

                var_CompareHash = var_CompareHash.Replace("-", "").Replace(" ", "").Replace(":", "");

                // Hash match - the installed APK is genuine for this version.
                if (!String.IsNullOrEmpty(var_DownloadedHash) && var_DownloadedHash.Equals(var_CompareHash, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log(String.Format("[GUPS][AntiCheat] The app hash '{0}' is equals to the remote hash read from endpoint '{1}'!", var_CompareHash, var_HashEndpoint));

                    yield break;
                }

                Debug.LogWarning(String.Format("[GUPS][AntiCheat] The app hash '{0}' is not equals to the remote hash '{1}' read from endpoint '{2}'!", var_CompareHash, var_DownloadedHash, var_HashEndpoint));
            }

            // Mismatch - the APK was modified after publishing.
            this.OnDetectCheating(EAndroidCheatingType.PACKAGE_HASH, false);
        }

        /// <summary>
        /// Validates the package signing fingerprint against the expected fingerprint from the global settings.
        /// </summary>
        /// <param name="_FingerprintStatus">The fingerprint status to validate.</param>
        /// <returns>A coroutine enumerator.</returns>
        private IEnumerator ValidatePackageFingerprint(AndroidFingerprintStatus _FingerprintStatus)
        {
            // Skip the check when fingerprint verification is disabled in the global settings.
            if(!GlobalSettings.Instance.Android_VerifyAppFingerprint)
            {
                Debug.Log(String.Format("[GUPS][AntiCheat] The app fingerprint verification is disabled. The fingerprint '{0}' is not validated!", _FingerprintStatus.Fingerprint));

                yield break;
            }

            // Treat retrieval failure as suspicious - cannot compare a missing fingerprint against the expectation.
            if (_FingerprintStatus.FailedToRetrieveData)
            {
                Debug.LogWarning("[GUPS][AntiCheat] The app fingerprint could not be retrieved!");

                this.OnDetectCheating(EAndroidCheatingType.PACKAGE_FINGERPRINT, true);

                yield break;
            }

            String var_SettingsFingerprint = GlobalSettings.Instance.Android_AppFingerprint?.Trim() ?? "";

            // Normalize the fingerprints by removing the common separators ('-', ' ', ':').
            var_SettingsFingerprint = var_SettingsFingerprint.Replace("-", "").Replace(" ", "").Replace(":", "");

            String var_CompareHash = _FingerprintStatus.Fingerprint?.Trim() ?? "";

            var_CompareHash = var_CompareHash.Replace("-", "").Replace(" ", "").Replace(":", "");

            // Fingerprint match - the APK is signed with the expected developer certificate.
            if (var_SettingsFingerprint.Equals(var_CompareHash, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log(String.Format("[GUPS][AntiCheat] The app fingerprint '{0}' is equals to the expected fingerprint!", var_CompareHash));

                yield break;
            }

            // Mismatch - the APK was re-signed (typical sign of repackaging).
            Debug.LogWarning(String.Format("[GUPS][AntiCheat] The app fingerprint '{0}' is not equals to the expected fingerprint '{1}'!", var_CompareHash, var_SettingsFingerprint));

            this.OnDetectCheating(EAndroidCheatingType.PACKAGE_FINGERPRINT, false);
        }

        /// <summary>
        /// Validates the package native libraries against the whitelist and blacklist from the global settings.
        /// </summary>
        /// <param name="_LibraryStatus">The library status to validate.</param>
        /// <returns>A coroutine enumerator.</returns>
        private IEnumerator ValidatePackageLibrary(AndroidLibraryStatus _LibraryStatus)
        {
            // Skip the check when both whitelisting and blacklisting are disabled in the global settings.
            if (!GlobalSettings.Instance.Android_UseWhitelistingForLibraries && !GlobalSettings.Instance.Android_UseBlacklistingforApplication)
            {
                Debug.Log("[GUPS][AntiCheat] The library validation is disabled. The libraries are not validated!");

                yield break;
            }

            // Treat retrieval failure as suspicious - cannot verify libraries we cannot enumerate.
            if (_LibraryStatus.FailedToRetrieveData)
            {
                Debug.LogWarning("[GUPS][AntiCheat] The libraries could not be retrieved!");

                this.OnDetectCheating(EAndroidCheatingType.PACKAGE_LIBRARY, true);

                yield break;
            }

            bool var_TamperedLibraries = false;

            // Whitelist sweep: any package library that is not on the whitelist is treated as tampering.
            if (GlobalSettings.Instance.Android_UseWhitelistingForLibraries)
            {
                List<String> var_PackageLibraries = new List<String>(_LibraryStatus.Libraries);

                List<String> var_WhitelistedLibraries = new List<String>(GlobalSettings.Instance.Android_WhitelistedLibraries);

                for(int w = 0; w < var_WhitelistedLibraries.Count; w++)
                {
                    for (int p = 0; p < var_PackageLibraries.Count; p++)
                    {
                        if (var_WhitelistedLibraries[w].Equals(var_PackageLibraries[p], StringComparison.OrdinalIgnoreCase))
                        {
                            var_PackageLibraries.RemoveAt(p);

                            p -= 1;
                        }
                    }
                }

                if (var_PackageLibraries.Count > 0)
                {
                    foreach(String var_Library in var_PackageLibraries)
                    {
                        Debug.LogWarning(String.Format("[GUPS][AntiCheat] The library '{0}' is not whitelisted!", var_Library));
                    }

                    var_TamperedLibraries = true;
                }
            }

            // Blacklist sweep: any package library that is on the blacklist is treated as tampering.
            if (GlobalSettings.Instance.Android_UseWhitelistingForLibraries)
            {
                List<String> var_PackageLibraries = new List<String>(_LibraryStatus.Libraries);

                List<String> var_BlacklistedLibraries = new List<String>(GlobalSettings.Instance.Android_BlacklistedLibraries);

                List<string> var_FoundBlacklistedLibraries = new List<string>();

                for (int b = 0; b < var_BlacklistedLibraries.Count; b++)
                {
                    for (int p = 0; p < var_PackageLibraries.Count; p++)
                    {
                        if (var_BlacklistedLibraries[b].Equals(var_PackageLibraries[p], StringComparison.OrdinalIgnoreCase))
                        {
                            var_FoundBlacklistedLibraries.Add(var_PackageLibraries[p]);
                        }
                    }
                }

                if (var_FoundBlacklistedLibraries.Count > 0)
                {
                    foreach (String var_Library in var_FoundBlacklistedLibraries)
                    {
                        Debug.LogWarning(String.Format("[GUPS][AntiCheat] The library '{0}' is blacklisted!", var_Library));
                    }

                    var_TamperedLibraries = true;
                }
            }

            // Neither sweep flagged anything - the library set is as expected.
            if(!var_TamperedLibraries)
            {
                Debug.Log(String.Format("[GUPS][AntiCheat] Found the expected libraries. The libraries are not tampered with!"));

                yield break;
            }

            // At least one library was flagged - escalate as package library tampering.
            this.OnDetectCheating(EAndroidCheatingType.PACKAGE_LIBRARY, false);
        }

        #endregion
    }
}
