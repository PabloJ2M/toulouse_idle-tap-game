// System
using System;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Monitor.Android
{
    /// <summary>
    /// Status payload produced by <see cref="AndroidPackageFingerprintMonitor"/> carrying the signing certificate fingerprint.
    /// </summary>
    public struct AndroidFingerprintStatus : IAndroidStatus
    {
        /// <inheritdoc/>
        public bool FailedToRetrieveData { get; private set; }

        /// <summary>
        /// Gets the algorithm used to compute <see cref="Fingerprint"/>.
        /// </summary>
        public String Algorithm { get; private set; }

        /// <summary>
        /// Gets the hex-encoded signing certificate fingerprint of the running app.
        /// </summary>
        public String Fingerprint { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidFingerprintStatus"/> struct.
        /// </summary>
        /// <param name="_FailedToRetrieveData"><c>true</c> if the fingerprint could not be retrieved; otherwise <c>false</c>.</param>
        /// <param name="_Algorithm">The algorithm used to compute the fingerprint.</param>
        /// <param name="_Fingerprint">The hex-encoded signing fingerprint, or <c>null</c> on failure.</param>
        public AndroidFingerprintStatus(bool _FailedToRetrieveData, String _Algorithm, String _Fingerprint)
        {
            this.FailedToRetrieveData = _FailedToRetrieveData;
            this.Algorithm = _Algorithm;
            this.Fingerprint = _Fingerprint;
        }
    }
}