// System
using System;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Monitor.Time
{
    /// <summary>
    /// Monitors Unity game time (delta and fixed-delta) against UTC wall-clock time and notifies observers with a <see cref="GameTimeStatus"/>.
    /// </summary>
    /// <remarks>
    /// Keeps a rolling history of delta-time samples and compares their mean against the corresponding UTC span to surface possible time-scale manipulation (e.g. SpeedHack-style tools).
    /// </remarks>
    /// <seealso cref="DeviceTimeMonitor"/>
    public class GameTimeMonitor : AMonitor
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Game Time Monitor";

        #endregion

        // Time Deviation
        #region Time Deviation

        /// <summary>
        /// Allowed difference in milliseconds between game time and real time before a deviation is reported (recommended: 10-20).
        /// </summary>
        [SerializeField]
        [Range(1f, 5000.0f)]
        [Header("Time Deviation - Settings")]
        [Tooltip("Allowed time difference in milliseconds between 'Game Time' and 'Real Time' until possible deviation get monitored (Recommend: 10-20).")]
        private float tolerance = 10f;

        /// <summary>
        /// The number of samples currently held in the rolling history.
        /// </summary>
        private int historySize = 0;

        /// <summary>
        /// The maximum number of samples to keep in the rolling history.
        /// </summary>
        private int maxHistorySize = 25;

        /// <summary>
        /// The most recent <see cref="UnityEngine.Time.deltaTime"/> samples.
        /// </summary>
        private float[] deltaTimeValues = new float[0];

        /// <summary>
        /// The mean of <see cref="deltaTimeValues"/>.
        /// </summary>
        private float deltaTimeValueMean = 0;

        /// <summary>
        /// UTC ticks of the previous sample.
        /// </summary>
        private long previousUtcTime;

        /// <summary>
        /// The most recent UTC delta samples (in seconds, scaled by Unity's time scale).
        /// </summary>
        private float[] utcTimeValues = new float[0];

        /// <summary>
        /// The mean of <see cref="utcTimeValues"/>.
        /// </summary>
        private float utcTimeValueMean = 0;

        /// <summary>
        /// The fixed delta time captured on the previous reset; compared in <see cref="OnUpdate"/>.
        /// </summary>
        private float previousFixedDeltaTime = 0.0f;

        /// <summary>
        /// Resets the rolling history and the captured timestamps.
        /// </summary>
        private void Reset()
        {
            this.historySize = 0;

            this.deltaTimeValues = new float[this.maxHistorySize];
            this.utcTimeValues = new float[this.maxHistorySize];

            this.deltaTimeValueMean = 0;
            this.utcTimeValueMean = 0;

            this.previousUtcTime = DateTime.UtcNow.Ticks;

            this.previousFixedDeltaTime = UnityEngine.Time.fixedDeltaTime;
        }

        /// <summary>
        /// Resets the UTC reference timestamp to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        private void ResetUtcTime()
        {
            this.previousUtcTime = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Pushes a new sample into the rolling buffer and returns the updated mean.
        /// </summary>
        /// <param name="_Value">The new sample to push.</param>
        /// <param name="_ValueArray">The rolling buffer to update in place.</param>
        /// <returns>The mean of <paramref name="_ValueArray"/> after pushing the new sample.</returns>
        private float Record(float _Value, float[] _ValueArray)
        {
            float var_MeanValue = 0;

            // Shift the buffer left by one and append the new sample at the end.
            for (int i = 0; i < this.maxHistorySize; i++)
            {
                if (i < this.maxHistorySize - 1)
                {
                    _ValueArray[i] = _ValueArray[i + 1];
                }
                else
                {
                    _ValueArray[i] = _Value;
                }

                var_MeanValue += _ValueArray[i];
            }

            return var_MeanValue / this.maxHistorySize;
        }

        /// <summary>
        /// Classifies the deviation between the mean Unity delta time and the mean UTC delta.
        /// </summary>
        /// <returns>The detected <see cref="ETimeDeviation"/>.</returns>
        private ETimeDeviation GetDeltaTimeDeviation()
        {
            if (Math.Abs(this.deltaTimeValueMean - this.utcTimeValueMean) < this.tolerance / 1000.0f)
            {
                return ETimeDeviation.None;
            }

            // Classify the kind of deviation.
            if (this.deltaTimeValueMean <= 0.0001f)
            {
                return ETimeDeviation.Stopped;
            }
            else if (this.deltaTimeValueMean < this.utcTimeValueMean)
            {
                return ETimeDeviation.SlowedDown;
            }
            else if (this.deltaTimeValueMean > this.utcTimeValueMean)
            {
                return ETimeDeviation.SpeedUp;
            }
            return ETimeDeviation.None;
        }

        /// <summary>
        /// Classifies the deviation between the previous and current <see cref="UnityEngine.Time.fixedDeltaTime"/>.
        /// </summary>
        /// <returns>The detected <see cref="ETimeDeviation"/>.</returns>
        private ETimeDeviation GetFixedDeltaTimeDeviation()
        {
            if (Math.Abs(this.previousFixedDeltaTime - UnityEngine.Time.fixedDeltaTime) < this.tolerance / 1000.0f)
            {
                return ETimeDeviation.None;
            }

            // Classify the kind of deviation.
            if (UnityEngine.Time.fixedDeltaTime <= 0.0001f)
            {
                return ETimeDeviation.Stopped;
            }
            else if (this.previousFixedDeltaTime < UnityEngine.Time.fixedDeltaTime)
            {
                return ETimeDeviation.SlowedDown;
            }
            else if (this.previousFixedDeltaTime > UnityEngine.Time.fixedDeltaTime)
            {
                return ETimeDeviation.SpeedUp;
            }
            return ETimeDeviation.None;
        }

        /// <summary>
        /// Converts <see cref="DateTime"/> ticks to seconds.
        /// </summary>
        /// <param name="_Tick">The tick span to convert.</param>
        /// <returns>The equivalent number of seconds.</returns>
        private static float TickToSec(long _Tick)
        {
            return Convert.ToSingle(_Tick) / TimeSpan.TicksPerSecond;
        }

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <inheritdoc/>
        protected override void OnStart()
        {
            this.Reset();
        }

        /// <inheritdoc/>
        protected override void OnResume()
        {
            this.Reset();
        }

        /// <summary>
        /// Records delta-time samples each frame and notifies observers once the rolling history is full.
        /// </summary>
        protected override void OnUpdate()
        {
            long var_UtcTimeNow = DateTime.UtcNow.Ticks;
            long var_SpanUtcTime = var_UtcTimeNow - this.previousUtcTime;

            this.previousUtcTime = var_UtcTimeNow;

            // Record the UTC span scaled by Unity's time scale so a paused game does not appear as a slow-down.
            this.utcTimeValueMean = this.Record(TickToSec(var_SpanUtcTime) * UnityEngine.Time.timeScale, this.utcTimeValues);

            this.deltaTimeValueMean = this.Record(UnityEngine.Time.deltaTime, this.deltaTimeValues);

            this.historySize++;

            if (this.historySize > this.maxHistorySize)
            {
                GameTimeStatus var_GameTimeStatus = new GameTimeStatus(this.GetDeltaTimeDeviation(), this.GetFixedDeltaTimeDeviation());

                this.Notify(var_GameTimeStatus);

                this.Reset();
            }
        }

        /// <summary>
        /// Resets the UTC reference timestamp when the application loses or regains focus to avoid spurious deviations.
        /// </summary>
        /// <param name="_Focus"><c>true</c> when the application gains focus; <c>false</c> when it loses focus.</param>
        private void OnApplicationFocus(bool _Focus)
        {
            this.ResetUtcTime();
        }

        /// <summary>
        /// Resets the UTC reference timestamp on application pause and resume to avoid spurious deviations.
        /// </summary>
        /// <param name="_Pause"><c>true</c> when the application is paused; <c>false</c> when it resumes.</param>
        private void OnApplicationPause(bool _Pause)
        {
            this.ResetUtcTime();
        }

        #endregion
    }
}
