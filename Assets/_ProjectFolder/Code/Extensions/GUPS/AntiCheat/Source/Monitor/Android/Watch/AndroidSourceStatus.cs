// System
using System;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Monitor.Android
{
    /// <summary>
    /// Status payload produced by <see cref="AndroidPackageSourceMonitor"/> identifying the app store the app was installed from.
    /// </summary>
    public struct AndroidSourceStatus : IAndroidStatus
    {
        /// <inheritdoc/>
        public bool FailedToRetrieveData { get; private set; }

        /// <summary>
        /// Gets the recognized app store the app was installed from, or <see cref="EAppStore.Unknown"/>.
        /// </summary>
        public EAppStore AppStoreSource { get; private set; }

        /// <summary>
        /// Gets the raw installer package name reported by the OS.
        /// </summary>
        public String AppStoreSourcePackage { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidSourceStatus"/> struct.
        /// </summary>
        /// <param name="_FailedToRetrieveData"><c>true</c> if the installer could not be retrieved; otherwise <c>false</c>.</param>
        /// <param name="_Source">The recognized app store, or <see cref="EAppStore.Unknown"/>.</param>
        /// <param name="_AppStoreSourcePackage">The raw installer package name, or <c>null</c> on failure.</param>
        public AndroidSourceStatus(bool _FailedToRetrieveData, EAppStore _Source, String _AppStoreSourcePackage)
        {
            this.FailedToRetrieveData = _FailedToRetrieveData;
            this.AppStoreSource = _Source;
            this.AppStoreSourcePackage = _AppStoreSourcePackage;
        }
    }
}