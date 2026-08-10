// System
using System;
using System.Reflection;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Punisher;

namespace GUPS.AntiCheat.Punisher
{
    /// <summary>
    /// Punisher that caps <see cref="Application.targetFrameRate"/> to a low value, throttling the game once a
    /// cheat is confirmed.
    /// </summary>
    /// <seealso cref="ExitGamePunisher"/>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class ReduceFpsPunisher : MonoBehaviour, IPunisher
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public String Name => "Reduce FPS Punisher";

        #endregion

        // Platform
        #region Platform

        /// <inheritdoc/>
        public bool IsSupported => true;

        /// <summary>
        /// Backing field for <see cref="IsActive"/>.
        /// </summary>
        [SerializeField]
        [Header("Punisher - Settings")]
        [Tooltip("Gets or sets whether the punisher is active and can administer punitive actions (Default: true).")]
        private bool isActive = true;

        /// <inheritdoc/>
        public bool IsActive { get => this.isActive; set => this.isActive = value; }

        #endregion

        // Threat Rating
        #region Threat Rating

        /// <summary>
        /// Backing field for <see cref="ThreatRating"/>. Defaults to <c>550</c>.
        /// </summary>
        [SerializeField]
        [Tooltip("Is a funny punishment, and can be very annoying for cheaters (Default: 550).")]
        private uint threatRating = 550;

        /// <inheritdoc/>
        public uint ThreatRating => this.threatRating;

        #endregion

        // Punishment
        #region Punishment

        /// <summary>
        /// Target frame rate applied by <see cref="Punish"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("The target frame rate. Reduce it to a low value to annoy players once caught cheating!")]
        private int punishFrameRate = 30;

        /// <inheritdoc/>
        public bool PunishOnce => true;

        /// <inheritdoc/>
        public bool HasPunished { get; private set; } = false;

        /// <summary>
        /// Disables VSync and sets <see cref="Application.targetFrameRate"/> to <see cref="punishFrameRate"/>.
        /// </summary>
        public void Punish()
        {
            this.HasPunished = true;

            // VSync must be disabled first; while vSyncCount is non-zero Unity ignores targetFrameRate on PC
            // and the FPS cap below would silently have no effect.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = this.punishFrameRate;
        }

        #endregion
    }
}
