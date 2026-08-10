// System
using System;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Monitor.Android
{
    /// <summary>
    /// Status payload produced by <see cref="AndroidPackageHashMonitor"/> carrying the computed app hash.
    /// </summary>
    public struct AndroidHashStatus : IAndroidStatus
    {
        /// <inheritdoc/>
        public bool FailedToRetrieveData { get; private set; }

        /// <summary>
        /// Gets the algorithm used to compute <see cref="Hash"/>.
        /// </summary>
        public String Algorithm { get; private set; }

        /// <summary>
        /// Gets the hex-encoded hash of the running app (APK/AAB).
        /// </summary>
        public String Hash { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidHashStatus"/> struct.
        /// </summary>
        /// <param name="_FailedToRetrieveData"><c>true</c> if the hash could not be retrieved; otherwise <c>false</c>.</param>
        /// <param name="_Algorithm">The algorithm used to compute the hash.</param>
        /// <param name="_Hash">The hex-encoded app hash, or <c>null</c> on failure.</param>
        public AndroidHashStatus(bool _FailedToRetrieveData, String _Algorithm, String _Hash)
        {
            this.FailedToRetrieveData = _FailedToRetrieveData;
            this.Algorithm = _Algorithm;
            this.Hash = _Hash;
        }
    }
}