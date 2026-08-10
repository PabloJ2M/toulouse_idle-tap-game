// System
using System;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Monitor.Time
{
    /// <summary>
    /// Monitors device wall-clock time against in-game time across focus and pause transitions and notifies observers with a <see cref="DeviceTimeStatus"/>.
    /// </summary>
    /// <remarks>
    /// Compares the elapsed <see cref="DateTime.UtcNow"/> span with <see cref="UnityEngine.Time.realtimeSinceStartup"/> when the app regains focus or resumes from pause.
    /// </remarks>
    /// <seealso cref="GameTimeMonitor"/>
    public class DeviceTimeMonitor : AMonitor
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Device Time Monitor";

        #endregion

        // Time Deviation
        #region Time Deviation

        /// <summary>
        /// Allowed deviation in seconds before a difference is reported (recommended: 3-10).
        /// </summary>
        [SerializeField]
        [Header("Time Deviation - Settings")]
        [Tooltip("Tolerance level in seconds for allowed time deviation until an actual deviation got detected (Recommend: 3-10).")]
        private float tolerance = 5f;

        /// <summary>
        /// UTC time at which the application last lost focus.
        /// </summary>
        private DateTime? previousUnFocusDateTime = null;

        /// <summary>
        /// <see cref="UnityEngine.Time.realtimeSinceStartup"/> at which the application last lost focus.
        /// </summary>
        private float previousUnFocusRealtimeSinceStartup;

        /// <summary>
        /// UTC time at which the application was last paused.
        /// </summary>
        private DateTime? previousPauseDateTime = null;

        /// <summary>
        /// <see cref="UnityEngine.Time.realtimeSinceStartup"/> at which the application was last paused.
        /// </summary>
        private float previousPauseRealtimeSinceStartup;

        /// <summary>
        /// Classifies the deviation between device-clock and game-clock elapsed time.
        /// </summary>
        /// <param name="_PassedDeviceTime">Seconds elapsed on the device clock.</param>
        /// <param name="_PassedGameTime">Seconds elapsed on the Unity real-time clock.</param>
        /// <returns>The detected <see cref="ETimeDeviation"/>.</returns>
        private ETimeDeviation GetTimeDeviation(float _PassedDeviceTime, float _PassedGameTime)
        {
            if (Math.Abs(_PassedDeviceTime - _PassedGameTime) < this.tolerance)
            {
                return ETimeDeviation.None;
            }

            // Classify the kind of deviation.
            if (_PassedDeviceTime <= 0.001f)
            {
                return ETimeDeviation.Stopped;
            }
            else if (_PassedDeviceTime < _PassedGameTime)
            {
                return ETimeDeviation.SlowedDown;
            }
            else if (_PassedDeviceTime > _PassedGameTime)
            {
                return ETimeDeviation.SpeedUp;
            }
            return ETimeDeviation.None;
        }

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Validates the device time when the application regains focus and notifies observers of any deviation.
        /// </summary>
        /// <param name="_HasFocus"><c>true</c> when the application gains focus; <c>false</c> when it loses focus.</param>
        protected virtual void OnApplicationFocus(bool _HasFocus)
        {
            if (_HasFocus)
            {
                if (this.previousUnFocusDateTime.HasValue)
                {
                    var var_PassedDeviceTime = DateTime.UtcNow - this.previousUnFocusDateTime.Value;
                    var var_PassedGameTime = UnityEngine.Time.realtimeSinceStartup - this.previousUnFocusRealtimeSinceStartup;

                    ETimeDeviation var_TimeDeviation = this.GetTimeDeviation((float)var_PassedDeviceTime.TotalSeconds, var_PassedGameTime);

                    this.Notify(new DeviceTimeStatus(var_TimeDeviation));

                    this.previousUnFocusDateTime = null;
                }
            }
            else
            {
                this.previousUnFocusDateTime = DateTime.UtcNow;

                this.previousUnFocusRealtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// Validates the device time when the application resumes from pause and notifies observers of any deviation.
        /// </summary>
        /// <param name="_IsPaused"><c>true</c> when the application is paused; <c>false</c> when it resumes.</param>
        protected virtual void OnApplicationPause(bool _IsPaused)
        {
            if (_IsPaused)
            {
                this.previousPauseDateTime = DateTime.UtcNow;

                this.previousPauseRealtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
            }
            else
            {
                if (this.previousPauseDateTime.HasValue)
                {
                    var var_PassedDeviceTime = DateTime.UtcNow - this.previousPauseDateTime.Value;
                    var var_PassedGameTime = UnityEngine.Time.realtimeSinceStartup - this.previousPauseRealtimeSinceStartup;

                    ETimeDeviation var_TimeDeviation = this.GetTimeDeviation((float)var_PassedDeviceTime.TotalSeconds, var_PassedGameTime);

                    this.Notify(new DeviceTimeStatus(var_TimeDeviation));

                    this.previousPauseDateTime = null;
                }
            }
        }

        #endregion
    }
}
