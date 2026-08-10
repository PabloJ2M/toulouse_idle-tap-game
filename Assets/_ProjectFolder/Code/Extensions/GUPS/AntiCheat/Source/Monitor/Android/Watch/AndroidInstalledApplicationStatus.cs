// System
using System;
using System.Collections.Generic;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Monitor.Android
{
    /// <summary>
    /// Status payload produced by <see cref="AndroidInstalledApplicationMonitor"/> listing the blacklisted apps detected on the device.
    /// </summary>
    public struct AndroidInstalledApplicationStatus : IAndroidStatus
    {
        /// <inheritdoc/>
        public bool FailedToRetrieveData { get; private set; }

        /// <summary>
        /// Gets the blacklisted package names that are currently installed on the device.
        /// </summary>
        public List<String> Applications { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidInstalledApplicationStatus"/> struct.
        /// </summary>
        /// <param name="_FailedToRetrieveData"><c>true</c> if the list could not be retrieved; otherwise <c>false</c>.</param>
        /// <param name="_Applications">The matching installed package names, or an empty list on failure.</param>
        public AndroidInstalledApplicationStatus(bool _FailedToRetrieveData, List<String> _Applications)
        {
            this.FailedToRetrieveData = _FailedToRetrieveData;
            this.Applications = _Applications;
        }
    }
}