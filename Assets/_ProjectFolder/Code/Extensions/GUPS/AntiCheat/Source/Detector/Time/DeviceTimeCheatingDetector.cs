// System
using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

// GUPS - AntiCheat
using GUPS.AntiCheat.Monitor.Time;

namespace GUPS.AntiCheat.Detector
{
    /// <summary>
    /// Detects device or system clock manipulation by observing a <see cref="DeviceTimeMonitor"/> and exposing a
    /// trustworthy <see cref="CurrentUtcTime"/> derived from either internet time or device time.
    /// </summary>
    /// <remarks>
    /// The detector subscribes on awake to the <see cref="DeviceTimeMonitor"/> on the same <see cref="GameObject"/>
    /// and forwards deviation notifications. The computed UTC time is consumed by
    /// <c>GUPS.AntiCheat.Protected.Time.ProtectedTime</c>.
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public class DeviceTimeCheatingDetector : ADetector
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Device Time Cheating Detector";

        #endregion

        // Platform
        #region Platform

        /// <inheritdoc/>
        public override bool IsSupported => true;

        #endregion

        // Threat Rating
        #region Threat Rating

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
        public CheatingDetectionEvent<CheatingDetectionStatus> OnCheatingDetectionEvent = new CheatingDetectionEvent<CheatingDetectionStatus>();

        #endregion

        // Observer
        #region Observer

        /// <summary>
        /// Receives notifications from the subscribed <see cref="DeviceTimeMonitor"/> and forwards any deviation to
        /// observers and event listeners.
        /// </summary>
        /// <param name="_Subject">The status published by the device time monitor.</param>
        public override void OnNext(IWatchedSubject _Subject)
        {
            if (this.IsActive)
            {
                if (_Subject is DeviceTimeStatus var_DeviceTimeStatus)
                {
                    if(var_DeviceTimeStatus.Deviation == ETimeDeviation.None)
                    {
                        return;
                    }

                    this.PossibleCheatingDetected = true;

                    this.Notify(new CheatingDetectionStatus(0.35f, this.ThreatRating));

                    this.OnCheatingDetectionEvent?.Invoke(new CheatingDetectionStatus(0.35f, this.ThreatRating));
                }
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

        // Device Time Monitor
        #region Device Time Monitor

        /// <summary>
        /// The device time monitor this detector observes, if any.
        /// </summary>
        private DeviceTimeMonitor deviceTimeMonitor;

        #endregion

        // Device Time
        #region Device Time

        /// <summary>
        /// Backing field for <see cref="UseInternetTime"/>. When <c>true</c>, the application start time is fetched
        /// from an internet endpoint instead of the device clock.
        /// </summary>
        [SerializeField]
        [Header("Device Time - Settings")]
        [Tooltip("Determines whether the detector uses internet time for fetching the application start time.")]
        private bool useInternetTime = true;

        /// <summary>
        /// Gets a value indicating whether the detector uses internet time for the application start time.
        /// </summary>
        public bool UseInternetTime { get => this.useInternetTime; }

        /// <summary>
        /// Shared HTTP client used for time synchronization.
        /// </summary>
        private HttpClient client;

        /// <summary>
        /// The address of the server used to fetch the current UTC time.
        /// </summary>
        [SerializeField]
        [Tooltip("The address of the server used to fetch the current utc time.")]
        private string serverAddress = "https://google.com";

        /// <summary>
        /// Optional hash of the server X509 certificate used for validation (enhances security).
        /// </summary>
        [SerializeField]
        [Tooltip("Hash of the server X509 certificate used for validation (Optional, but enhances security).")]
        private string serverCertificateHash = null;

        /// <summary>
        /// The application start time, either fetched from the internet or read from the device clock.
        /// </summary>
        private DateTime? applicationStartDateTime =  null;

        /// <summary>
        /// The Unity-reported real time since startup at the moment <see cref="applicationStartDateTime"/> was captured.
        /// </summary>
        private float applicationStartRealTimeSinceStartup = 0;

        /// <summary>
        /// The computed UTC time. May differ from <see cref="DateTime.UtcNow"/> because it is calculated to be as
        /// trustworthy as possible.
        /// </summary>
        public DateTime CurrentUtcTime = DateTime.UtcNow;

        /// <summary>
        /// Asynchronously calculates the comparison time, from either the internet or the device clock.
        /// </summary>
        /// <returns>The current UTC time used as reference.</returns>
        private async Task<DateTime> CalculateCompareTime()
        {
            if (this.UseInternetTime)
            {
                return await this.GetInternetCompareTime();
            }
            else
            {
                return DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Asynchronously retrieves the comparison time from the configured internet server.
        /// </summary>
        /// <returns>The UTC time reported by the server, or <see cref="DateTime.UtcNow"/> on failure.</returns>
        /// <remarks>
        /// Throws <see cref="AuthenticationException"/> when the server certificate validation fails and a certificate
        /// hash was configured, indicating possible tampering.
        /// </remarks>
        private async Task<DateTime> GetInternetCompareTime()
        {
            try
            {
                if (this.client == null)
                {
                    // Custom certificate validation callback - prevents users from tampering with the response.
                    var var_Handler = new HttpClientHandler();
                    var_Handler.ServerCertificateCustomValidationCallback = (var_HttpRequestMessage, var_Certificate, var_Chain, var_PolicyError) =>
                    {
                        if (String.IsNullOrEmpty(this.serverCertificateHash))
                        {
                            return true;
                        }

                        return this.serverCertificateHash.Equals(var_Certificate.GetCertHashString());
                    };
                    var_Handler.CheckCertificateRevocationList = true;

                    this.client = new HttpClient(var_Handler);

                    this.client.Timeout = TimeSpan.FromSeconds(5);
                }

                var var_Response = await this.client.GetAsync(this.serverAddress, HttpCompletionOption.ResponseHeadersRead);
                return var_Response.Headers.Date.HasValue ? var_Response.Headers.Date.Value.UtcDateTime : DateTime.UtcNow;
            }
            catch (HttpRequestException var_HttpRequestException)
            {
                // SSL certificate validation failure - escalate as possible tampering.
                if (var_HttpRequestException.InnerException is HttpRequestException var_InnerException &&
                    var_InnerException.InnerException is AuthenticationException var_SSLException)
                {
                    throw new AuthenticationException("SSL certificate validation failed. Possible tampering!", var_SSLException);
                }
                return DateTime.UtcNow;
            }
            catch
            {
                return DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Synchronously fetches the comparison time, adjusts for the round-trip and stores it as the application
        /// start time.
        /// </summary>
        private void GetApplicationStartTime()
        {
            DateTime var_DeviceUtcTime_Pre = DateTime.UtcNow;

            var var_TimeTask = Task.Run(async () => { return await this.CalculateCompareTime(); });
            var_TimeTask.Wait();

            DateTime var_DeviceUtcTime_Post = DateTime.UtcNow;

            float var_DeviceUtcTimeDifference = (float)(var_DeviceUtcTime_Post - var_DeviceUtcTime_Pre).TotalSeconds;

            // Adjust the fetched time by half the round-trip to reduce request latency bias.
            this.applicationStartDateTime = var_TimeTask.Result.AddSeconds(var_DeviceUtcTimeDifference / 2f);

            this.applicationStartRealTimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        }

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Subscribes to the <see cref="DeviceTimeMonitor"/> on the same <see cref="GameObject"/> and captures the
        /// application start time.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            this.deviceTimeMonitor = this.GetComponent<DeviceTimeMonitor>();

            if (this.deviceTimeMonitor == null)
            {
                UnityEngine.Debug.LogWarning("DeviceTimeCheatingDetector requires a DeviceTimeMonitor to be present on the same game object.");
                return;
            }

            this.deviceTimeMonitor.Subscribe(this);

            if (this.applicationStartDateTime == null)
            {
                this.GetApplicationStartTime();
            }
        }

        /// <summary>
        /// Checks for a startup deviation and notifies observers if the device clock differs noticeably from the
        /// reference time.
        /// </summary>
        protected virtual void Start()
        {
            if (this.applicationStartDateTime.HasValue && Math.Abs((DateTime.UtcNow - this.applicationStartDateTime.Value).TotalSeconds) > 15f)
            {
                this.PossibleCheatingDetected = true;

                this.Notify(new CheatingDetectionStatus(0.35f, this.ThreatRating));

                this.OnCheatingDetectionEvent?.Invoke(new CheatingDetectionStatus(0.35f, this.ThreatRating));
            }
        }

        /// <summary>
        /// Recalculates <see cref="CurrentUtcTime"/> from <see cref="applicationStartDateTime"/> and the Unity real
        /// time since startup.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (this.applicationStartDateTime == null)
            {
                this.GetApplicationStartTime();
            }

            this.CurrentUtcTime = this.applicationStartDateTime.Value.AddSeconds(UnityEngine.Time.realtimeSinceStartup - this.applicationStartRealTimeSinceStartup);
        }

        /// <summary>
        /// Recalculates the application start time when the application regains focus.
        /// </summary>
        /// <param name="_HasFocus"><c>true</c> if focus was gained; <c>false</c> if lost.</param>
        protected virtual void OnApplicationFocus(bool _HasFocus)
        {
            if (_HasFocus)
            {
                this.GetApplicationStartTime();
            }
        }

        /// <summary>
        /// Recalculates the application start time when the application is unpaused.
        /// </summary>
        /// <param name="_IsPaused"><c>true</c> if the application is being paused; <c>false</c> if unpaused.</param>
        protected virtual void OnApplicationPause(bool _IsPaused)
        {
            if (!_IsPaused)
            {
                this.GetApplicationStartTime();
            }
        }

        #endregion
    }
}
