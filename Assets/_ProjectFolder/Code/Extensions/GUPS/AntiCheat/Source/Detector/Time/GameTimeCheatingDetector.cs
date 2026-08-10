// System
using System;

// Unity
using UnityEngine;
using UnityEngine.SceneManagement;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

// GUPS - AntiCheat
using GUPS.AntiCheat.Monitor.Time;

namespace GUPS.AntiCheat.Detector
{
    /// <summary>
    /// Detects manipulation of <see cref="UnityEngine.Time"/> (delta time and fixed delta time) and exposes a
    /// system-tick based fallback time once cheating is detected, used by
    /// <c>GUPS.AntiCheat.Protected.Time.ProtectedTime</c>.
    /// </summary>
    /// <remarks>
    /// Subscribes on awake to the <see cref="GameTimeMonitor"/> on the same <see cref="GameObject"/>.
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public class GameTimeCheatingDetector : ADetector
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Game Time Cheating Detector";

        #endregion

        // Platform
        #region Platform

        /// <inheritdoc/>
        public override bool IsSupported => true;

        #endregion

        // Threat Rating
        #region Threat Rating

        /// <summary>
        /// Gets the false-positive likelihood reported with each detection. Set high because the game time monitor
        /// is sensitive to legitimate game hickups.
        /// </summary>
        public float PossibilityOfFalsePositive { get; private set; } = 0.45f;

        /// <summary>
        /// The threat rating reported on every detection (Recommended: 25; low because false positives are likely).
        /// </summary>
        [SerializeField]
        [Header("Threat Rating - Settings")]
        [Tooltip("The threat rating of this detector. It is set to a low value, because false positives are likely and a high amount of possible cheating detections will be send to the monitor (Recommended: 25).")]
        private uint threatRating = 25;

        /// <inheritdoc/>
        public override uint ThreatRating { get => this.threatRating; protected set => this.threatRating = value; }

        /// <inheritdoc/>
        public override bool PossibleCheatingDetected { get; protected set; } = false;

        #endregion

        // Detection
        #region Detection

        /// <summary>
        /// Whether to react to delta time cheating (commonly used to speed up or slow down the game) (Recommended: true).
        /// </summary>
        [SerializeField]
        [Header("Detection - Settings")]
        [Tooltip("Enable if the detector should react on possible detected delta time cheating and notify listeners. Delta time cheating is commonly used to speed up or slow down your game. (Recommended: true).")]
        private bool DetectDeltaTimeCheating = true;

        /// <summary>
        /// Whether to react to fixed delta time cheating (used to skip physics updates, e.g. to walk through walls).
        /// Requires using the <c>ProtectedTime.fixedDeltaTime</c> setter to update the value (Recommended: true).
        /// </summary>
        [Tooltip("Enable if the detector should react on possible detected fixed delta time cheating and notify listeners. Fixed delta time is responsible for physics update. Cheaters often set the fixed delta time to a very high value, to prevent physics updates, allowing them for example to walk through walls. Note: When enabling you have to use the ProtectedTime.fixedDeltaTime setter to update the fixedDeltaTime. (Recommended: true).")]
        private bool DetectFixedDeltaTimeCheating = false;

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
        /// Receives notifications from the subscribed <see cref="GameTimeMonitor"/>, records the deviation and
        /// forwards any tampering to observers and event listeners.
        /// </summary>
        /// <param name="_Subject">The status published by the game time monitor.</param>
        public override void OnNext(IWatchedSubject _Subject)
        {
            if (this.IsActive)
            {
                if (_Subject is GameTimeStatus var_GameTimeStatus)
                {
                    // Once cheating is detected, ignore further notifications.
                    if (this.PossibleCheatingDetected)
                    {
                    }
                    else
                    {
                        this.ProcessDeltaTime(var_GameTimeStatus);

                        this.ProcessFixedDeltaTime(var_GameTimeStatus);
                    }
                }
            }
        }

        /// <summary>
        /// Records the delta time deviation and notifies observers when the rolling window exceeds the threshold.
        /// </summary>
        /// <param name="_GameTimeStatus">The game time status reported by the monitor.</param>
        private void ProcessDeltaTime(GameTimeStatus _GameTimeStatus)
        {
            if (!this.DetectDeltaTimeCheating)
            {
                return;
            }

            if (this.Record(_GameTimeStatus.DeltaDeviation))
            {
                this.PossibleCheatingDetected = true;
            }

            if (_GameTimeStatus.DeltaDeviation == ETimeDeviation.None)
            {
                return;
            }

            this.Notify(new CheatingDetectionStatus(this.PossibilityOfFalsePositive, this.ThreatRating));

            this.OnCheatingDetectionEvent?.Invoke(new CheatingDetectionStatus(this.PossibilityOfFalsePositive, this.ThreatRating));
        }

        /// <summary>
        /// Verifies the fixed delta time against the Unity value and notifies observers when they diverge.
        /// </summary>
        /// <param name="_GameTimeStatus">The game time status reported by the monitor.</param>
        private void ProcessFixedDeltaTime(GameTimeStatus _GameTimeStatus)
        {
            if (!this.DetectFixedDeltaTimeCheating)
            {
                return;
            }

            if (_GameTimeStatus.FixedDeltaDeviation == ETimeDeviation.None)
            {
                return;
            }

            if (Math.Abs(this.fixedDeltaTime - UnityEngine.Time.fixedDeltaTime) > 0.001f)
            {
                this.Notify(new CheatingDetectionStatus(this.PossibilityOfFalsePositive, this.ThreatRating));

                this.OnCheatingDetectionEvent?.Invoke(new CheatingDetectionStatus(this.PossibilityOfFalsePositive, this.ThreatRating));
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

        // Game Time Monitor
        #region Game Time Monitor

        /// <summary>
        /// The game time monitor this detector observes, if any.
        /// </summary>
        private GameTimeMonitor gameTimeMonitor;

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Subscribes to the <see cref="GameTimeMonitor"/> on the same <see cref="GameObject"/> and resets the time
        /// state.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            this.gameTimeMonitor = this.GetComponent<GameTimeMonitor>();

            if (this.gameTimeMonitor == null)
            {
                UnityEngine.Debug.LogWarning("GameTimeCheatingDetector requires a GameTimeMonitor to be present on the same game object.");
                return;
            }

            this.gameTimeMonitor.Subscribe(this);

            this.values = new ETimeDeviation[this.maxHistorySize];

            this.ResetFixedDeltaTime();
        }

        /// <summary>
        /// Refreshes the internal time state on every frame.
        /// </summary>
        protected virtual void Update()
        {
            this.UpdateTime();
        }

        /// <summary>
        /// Resets the previous UTC time when the application focus changes.
        /// </summary>
        /// <param name="_Focus"><c>true</c> if focus was gained; <c>false</c> if lost.</param>
        private void OnApplicationFocus(bool _Focus)
        {
            this.ResetUtcTime();
        }

        /// <summary>
        /// Resets the previous UTC time when the application is paused or unpaused.
        /// </summary>
        /// <param name="_Pause"><c>true</c> if the application is being paused; <c>false</c> if unpaused.</param>
        private void OnApplicationPause(bool _Pause)
        {
            this.ResetUtcTime();
        }

        /// <summary>
        /// Resets the time since level loaded when a new scene is loaded.
        /// </summary>
        /// <param name="_Scene">The loaded scene.</param>
        /// <param name="_Mode">The scene loading mode.</param>
        protected virtual void OnLevelFinishedLoading(Scene _Scene, LoadSceneMode _Mode)
        {
            this.ResetLevelTime();
        }

        #endregion

        // History
        #region History

        /// <summary>
        /// The maximum number of recent deviation samples kept in <see cref="values"/>.
        /// </summary>
        private int maxHistorySize = 25;

        /// <summary>
        /// Rolling buffer of the most recent deviation samples.
        /// </summary>
        private ETimeDeviation[] values = new ETimeDeviation[0];

        /// <summary>
        /// Records a new deviation sample and returns whether the rolling window indicates time cheating.
        /// </summary>
        /// <param name="_Value">The deviation sample to record.</param>
        /// <returns><c>true</c> if at least half of the window is a non-<c>None</c> deviation of the same kind.</returns>
        private bool Record(ETimeDeviation _Value)
        {
            int var_None = 0;
            int var_Stopped = 0;
            int var_SlowedDown = 0;
            int var_SpeedUp = 0;

            // Shift the rolling buffer left and append the new value at the end, counting each kind.
            for (int i = 0; i < this.maxHistorySize; i++)
            {
                if (i < this.maxHistorySize - 1)
                {
                    this.values[i] = this.values[i + 1];
                }
                else
                {
                    this.values[i] = _Value;
                }

                switch (this.values[i])
                {
                    case ETimeDeviation.None:
                        var_None++;
                        break;
                    case ETimeDeviation.Stopped:
                        var_Stopped++;
                        break;
                    case ETimeDeviation.SlowedDown:
                        var_SlowedDown++;
                        break;
                    case ETimeDeviation.SpeedUp:
                        var_SpeedUp++;
                        break;
                }
            }

            return var_Stopped >= this.maxHistorySize / 2 || var_SlowedDown >= this.maxHistorySize / 2 || var_SpeedUp >= this.maxHistorySize / 2;
        }

        #endregion

        // Time
        #region Time

        // Device Time
        private long previousUtcTime;

        // Unity Time
        private float time;
        private float deltaTime;
        private float fixedDeltaTime;
        private float unscaledTime;
        private float unscaledDeltaTime;
        private float realtimeSinceStartup;
        private float timeSinceLevelLoad;

        internal float Time { get { return this.PossibleCheatingDetected ? this.time : UnityEngine.Time.time; } set { this.time = value; } }
        internal float DeltaTime { get { return this.PossibleCheatingDetected ? this.deltaTime : UnityEngine.Time.deltaTime; } set { this.deltaTime = value; } }
        internal float FixedDeltaTime { get { return this.PossibleCheatingDetected ? this.fixedDeltaTime : UnityEngine.Time.fixedDeltaTime; } set { this.fixedDeltaTime = value; UnityEngine.Time.fixedDeltaTime = value; } }
        internal float UnscaledTime { get { return this.PossibleCheatingDetected ? this.unscaledTime : UnityEngine.Time.unscaledTime; } set { this.unscaledTime = value; } }
        internal float UnscaledDeltaTime { get { return this.PossibleCheatingDetected ? this.unscaledDeltaTime : UnityEngine.Time.unscaledDeltaTime; } set { this.unscaledDeltaTime = value; } }
        internal float RealtimeSinceStartup { get { return this.PossibleCheatingDetected ? this.realtimeSinceStartup : UnityEngine.Time.realtimeSinceStartup; } set { this.realtimeSinceStartup = value; } }
        internal float TimeSinceLevelLoad { get { return this.PossibleCheatingDetected ? this.timeSinceLevelLoad : UnityEngine.Time.timeSinceLevelLoad; } set { this.timeSinceLevelLoad = value; } }
        internal float TimeScale { get { return UnityEngine.Time.timeScale; } }

        /// <summary>
        /// Updates the internal time state. Once cheating is detected, time is derived from system ticks instead of
        /// <see cref="UnityEngine.Time"/>.
        /// </summary>
        private void UpdateTime()
        {
            long var_UtcTimeNow = DateTime.UtcNow.Ticks;
            long var_SpanUtcTime = var_UtcTimeNow - this.previousUtcTime;

            this.previousUtcTime = var_UtcTimeNow;

            if (this.PossibleCheatingDetected)
            {
                // Cheating active: derive time from system ticks so timeScale tampering cannot affect it.
                this.unscaledDeltaTime = TickToSec(var_SpanUtcTime);
                this.unscaledTime += this.unscaledDeltaTime;
                this.realtimeSinceStartup += this.unscaledDeltaTime;

                this.deltaTime = this.unscaledDeltaTime * this.TimeScale;
                this.time += this.deltaTime;
                this.timeSinceLevelLoad += this.deltaTime;
            }
            else
            {
                // No cheat detected: mirror Unity's time so the values stay in lockstep.
                this.time = UnityEngine.Time.time;
                this.unscaledTime = UnityEngine.Time.unscaledTime;
                this.deltaTime = UnityEngine.Time.deltaTime;
                this.unscaledDeltaTime = UnityEngine.Time.unscaledDeltaTime;
                this.realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
                this.timeSinceLevelLoad = UnityEngine.Time.timeSinceLevelLoad;
            }
        }

        /// <summary>
        /// Resets the time since level loaded back to zero.
        /// </summary>
        private void ResetLevelTime()
        {
            this.timeSinceLevelLoad = 0.0f;
        }

        /// <summary>
        /// Resets the previous UTC time anchor to the current time.
        /// </summary>
        private void ResetUtcTime()
        {
            this.previousUtcTime = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Resets the cached fixed delta time to the current Unity value.
        /// </summary>
        private void ResetFixedDeltaTime()
        {
            this.fixedDeltaTime = UnityEngine.Time.fixedDeltaTime;
        }

        /// <summary>
        /// Converts a tick count to a number of seconds.
        /// </summary>
        /// <param name="_Tick">The tick count to convert.</param>
        /// <returns>The corresponding number of seconds.</returns>
        private static float TickToSec(long _Tick)
        {
            return Convert.ToSingle(_Tick) / TimeSpan.TicksPerSecond;
        }

        #endregion
    }
}
