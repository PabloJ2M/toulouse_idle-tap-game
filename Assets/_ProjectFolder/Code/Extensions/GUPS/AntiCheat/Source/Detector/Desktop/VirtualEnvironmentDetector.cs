// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;

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
    /// Detects whether the game is running inside a virtual machine (VirtualBox, VMware, Parallels, Hyper-V, ...).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Combines a cross-platform sweep over <see cref="NetworkInterface.GetAllNetworkInterfaces"/> for vendor MAC OUIs
    /// (e.g. <c>08:00:27</c> for VirtualBox) with an OS specific check via
    /// <see cref="IPlatformProbe.IsRunningInVirtualMachine"/> and <see cref="IPlatformProbe.IsHypervisorBitSet"/>
    /// (the latter currently always returns false in managed code).
    /// </para>
    /// <para>
    /// The default threat rating is intentionally low and the false-positive rate intentionally high because many
    /// legitimate users run inside Hyper-V, WSL2 or cloud streamed Windows. Games that explicitly want to ban VMs
    /// should raise the threat rating and reduce the false-positive rate via the inspector.
    /// </para>
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public class VirtualEnvironmentDetector : ADetector
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Virtual Environment Detector";

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
        /// The false-positive likelihood reported with each detection. Set high by default because many legitimate
        /// users run inside Hyper-V or WSL2.
        /// </summary>
        [SerializeField]
        [Header("Threat Rating - Settings")]
        [Tooltip("The possibility of a false positive when assessing a virtual machine detection. Many legitimate users run inside Hyper-V / WSL2 / cloud streamed Windows.")]
        [Range(0f, 1f)]
        private float possibilityOfFalsePositive = 0.15f;

        /// <summary>
        /// Gets the false-positive likelihood reported with each detection.
        /// </summary>
        public float PossibilityOfFalsePositive => this.possibilityOfFalsePositive;

        /// <summary>
        /// The threat rating reported on every detection (Recommended: 200, raise if your game must not run in a VM).
        /// </summary>
        [SerializeField]
        [Tooltip("The threat rating of this detector. Set high if your game must not run in a VM (Recommended: 200).")]
        private uint threatRating = 200;

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
        /// Run the VM check only once on game start. The result of a VM check rarely changes during a session, so the
        /// default is <c>true</c>.
        /// </summary>
        [Header("Virtual Environment - Settings")]
        [Tooltip("Run the VM check only once on game start. The result of a VM check rarely changes during a session.")]
        public bool CheckOnlyOnGameStart = true;

        /// <summary>
        /// Interval in seconds between VM checks.
        /// </summary>
        [Tooltip("Interval in seconds between VM checks. Recommended: 120")]
        [Range(1f, 600f)]
        public float RecheckIntervalForPossibleCheating = 120f;

        /// <summary>
        /// Shared empty list returned when <see cref="GlobalSettings"/> is unavailable. The detector holds no
        /// built-in fallback lists - <see cref="GlobalSettings"/> is the single source of truth and a missing asset
        /// means nothing is scanned for.
        /// </summary>
        private static readonly String[] EmptyList = new String[0];

        /// <summary>
        /// Returns the configured MAC OUI prefixes from <see cref="GlobalSettings"/>, or an empty list if the
        /// settings are unavailable.
        /// </summary>
        /// <returns>The MAC OUI prefixes to scan for.</returns>
        private IList<String> GetMacOuis()
        {
            GlobalSettings var_Settings = GlobalSettings.Instance;
            if (var_Settings != null && var_Settings.Desktop_VirtualMachine_MacOuis != null)
            {
                return var_Settings.Desktop_VirtualMachine_MacOuis;
            }
            return EmptyList;
        }

        /// <summary>
        /// Returns the full BIOS / DMI keyword list from <see cref="GlobalSettings"/>, or an empty list if the
        /// settings are unavailable. The platform probes hold no built-in keywords.
        /// </summary>
        /// <returns>The BIOS keywords to scan for.</returns>
        private IEnumerable<String> GetBiosKeywords()
        {
            GlobalSettings var_Settings = GlobalSettings.Instance;
            if (var_Settings != null && var_Settings.Desktop_VirtualMachine_BiosKeywords != null)
            {
                return var_Settings.Desktop_VirtualMachine_BiosKeywords;
            }
            return EmptyList;
        }

        /// <summary>
        /// Returns the Windows guest-tool service registry keys from <see cref="GlobalSettings"/>, or an empty list
        /// if the settings are unavailable. Only consumed by the Windows platform probe.
        /// </summary>
        /// <returns>The service registry keys (relative to HKLM) to probe for.</returns>
        private IEnumerable<String> GetWindowsServiceKeys()
        {
            GlobalSettings var_Settings = GlobalSettings.Instance;
            if (var_Settings != null && var_Settings.Desktop_VirtualMachine_WindowsServiceKeys != null)
            {
                return var_Settings.Desktop_VirtualMachine_WindowsServiceKeys;
            }
            return EmptyList;
        }

        #endregion

        // Detection
        #region Detection

        /// <summary>
        /// The cached platform probe used to query OS specific VM evidence.
        /// </summary>
        private IPlatformProbe platformProbe;

        /// <summary>
        /// Whether BIOS evidence has already been reported. Prevents spamming the punisher chain on every recheck.
        /// </summary>
        private bool reportedBios;

        /// <summary>
        /// Whether MAC evidence has already been reported. Prevents spamming the punisher chain on every recheck.
        /// </summary>
        private bool reportedMac;

        /// <summary>
        /// Whether hypervisor evidence has already been reported. Prevents spamming the punisher chain on every recheck.
        /// </summary>
        private bool reportedHypervisor;

        /// <summary>
        /// Performs all enabled checks once and notifies observers if any evidence is found.
        /// </summary>
        /// <returns><c>true</c> if any evidence of a virtual machine was found; otherwise <c>false</c>.</returns>
        public bool ManualScan()
        {
            GlobalSettings var_Settings = GlobalSettings.Instance;
            if (var_Settings != null && !var_Settings.Desktop_DetectVirtualMachine)
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

            if (this.platformProbe != null)
            {
                if (this.platformProbe.IsRunningInVirtualMachine(this.GetBiosKeywords(), this.GetWindowsServiceKeys(), out String var_BiosEvidence))
                {
                    if (!this.reportedBios)
                    {
                        this.reportedBios = true;
                        this.OnDetectCheating(EDesktopCheatingType.VIRTUAL_MACHINE_BIOS, var_BiosEvidence);
                    }
                    var_AnyDetected = true;
                }

                if (this.platformProbe.IsHypervisorBitSet(out String var_HypervisorEvidence))
                {
                    if (!this.reportedHypervisor)
                    {
                        this.reportedHypervisor = true;
                        this.OnDetectCheating(EDesktopCheatingType.VIRTUAL_MACHINE_HYPERVISOR_BIT, var_HypervisorEvidence);
                    }
                    var_AnyDetected = true;
                }
            }

            if (this.ScanMacAddresses(out String var_MacEvidence))
            {
                if (!this.reportedMac)
                {
                    this.reportedMac = true;
                    this.OnDetectCheating(EDesktopCheatingType.VIRTUAL_MACHINE_MAC, var_MacEvidence);
                }
                var_AnyDetected = true;
            }

            return var_AnyDetected;
        }

        /// <summary>
        /// Sweeps every <see cref="NetworkInterface"/> on the host for a MAC address starting with one of the
        /// configured vendor OUIs.
        /// </summary>
        /// <param name="evidence">On <c>true</c> return, the evidence string describing the matched interface.</param>
        /// <returns><c>true</c> if a vendor OUI was matched; otherwise <c>false</c>.</returns>
        private bool ScanMacAddresses(out String evidence)
        {
            evidence = String.Empty;

            try
            {
                IList<String> var_Ouis = this.GetMacOuis();
                NetworkInterface[] var_Interfaces = NetworkInterface.GetAllNetworkInterfaces();
                for (int i = 0; i < var_Interfaces.Length; i++)
                {
                    PhysicalAddress var_Physical;
                    try
                    {
                        var_Physical = var_Interfaces[i].GetPhysicalAddress();
                    }
                    catch
                    {
                        continue;
                    }

                    if (var_Physical == null)
                    {
                        continue;
                    }

                    byte[] var_Bytes = var_Physical.GetAddressBytes();
                    if (var_Bytes == null || var_Bytes.Length < 3)
                    {
                        continue;
                    }

                    String var_Hex = String.Format("{0:X2}{1:X2}{2:X2}", var_Bytes[0], var_Bytes[1], var_Bytes[2]);

                    for (int p = 0; p < var_Ouis.Count; p++)
                    {
                        String var_Prefix = var_Ouis[p];
                        if (String.IsNullOrEmpty(var_Prefix))
                        {
                            continue;
                        }

                        var_Prefix = var_Prefix.Replace(":", String.Empty).Replace("-", String.Empty).Replace(" ", String.Empty);

                        if (var_Hex.Equals(var_Prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            evidence = String.Format("Network interface '{0}' has VM vendor MAC OUI '{1}'", var_Interfaces[i].Name, var_Prefix);
                            return true;
                        }
                    }
                }
            }
            catch (Exception var_Exception)
            {
                UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] VirtualEnvironmentDetector failed to enumerate network interfaces: {0}", var_Exception.Message));
            }

            return false;
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

            UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] VirtualEnvironmentDetector detected '{0}': {1}", _Type, _Evidence));

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
