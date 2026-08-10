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
    /// Punisher that mirrors <see cref="Camera.main"/>'s projection matrix horizontally or vertically.
    /// Particularly disruptive in first-person shooters.
    /// </summary>
    /// <seealso cref="ExitGamePunisher"/>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class FlipCameraPunisher : MonoBehaviour, IPunisher
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public String Name => "Flip Camera Punisher";

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
        /// Backing field for <see cref="ThreatRating"/>. Defaults to <c>450</c> as a moderate-impact punishment.
        /// </summary>
        [SerializeField]
        [Tooltip("Is a funny punishment, and can in first persion shooters be very annoying for cheaters (Default: 450).")]
        private uint threatRating = 450;

        /// <inheritdoc/>
        public uint ThreatRating => this.threatRating;

        #endregion

        // Punishment
        #region Punishment

        /// <summary>
        /// True once <see cref="Punish"/> has flipped the camera.
        /// </summary>
        private bool isFlipped = false;

        /// <summary>
        /// When <c>true</c> flip horizontally, otherwise flip vertically.
        /// </summary>
        [SerializeField]
        [Tooltip("Flip / mirror the camera view horizontally or vertically.")]
        private bool flipHorizontal = true;

        /// <inheritdoc/>
        public bool PunishOnce => true;

        /// <inheritdoc/>
        public bool HasPunished => this.isFlipped;

        /// <summary>
        /// Mirrors <see cref="Camera.main"/>'s projection matrix according to <see cref="flipHorizontal"/>. No-op
        /// if already flipped.
        /// </summary>
        public void Punish()
        {
            // Punish only once: subsequent calls would re-mirror the matrix and cancel the flip.
            if(this.isFlipped)
            {
                return;
            }

            // Operate on the main scene camera; if the project tags no camera as MainCamera this will be null and throw.
            var targetCamera = Camera.main;

            if (this.flipHorizontal)
            {
                // Horizontal flip: negate the Y row of the projection so the image is mirrored top-to-bottom,
                // then invert culling so back-facing triangles are not erroneously drawn front.
                Matrix4x4 proj = targetCamera.projectionMatrix;
                proj.m11 = -proj.m11;
                proj.m13 = -proj.m13;
                targetCamera.projectionMatrix = proj;
                GL.invertCulling = true;
            }
            else
            {
                // Vertical flip: negate the X row of the projection so the image is mirrored left-to-right,
                // again pairing it with inverted culling to keep face orientation visually correct.
                Matrix4x4 proj = targetCamera.projectionMatrix;
                proj.m00 = -proj.m00;
                proj.m01 = -proj.m01;
                targetCamera.projectionMatrix = proj;
                GL.invertCulling = true;
            }

            // Record that the flip has been applied so PunishOnce semantics are honoured on the next call.
            this.isFlipped = true;
        }

        #endregion
    }
}
