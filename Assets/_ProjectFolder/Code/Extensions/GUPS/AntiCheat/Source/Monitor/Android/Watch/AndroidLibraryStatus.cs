// System
using System;
using System.Collections.Generic;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Monitor.Android
{
    /// <summary>
    /// Status payload produced by <see cref="AndroidPackageLibraryMonitor"/> listing the native libraries bundled in the running app.
    /// </summary>
    public struct AndroidLibraryStatus : IAndroidStatus
    {
        /// <inheritdoc/>
        public bool FailedToRetrieveData { get; private set; }

        /// <summary>
        /// Gets the names of the native libraries bundled in the running app (APK/AAB).
        /// </summary>
        public List<String> Libraries { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidLibraryStatus"/> struct.
        /// </summary>
        /// <param name="_FailedToRetrieveData"><c>true</c> if the libraries could not be retrieved; otherwise <c>false</c>.</param>
        /// <param name="_Libraries">The bundled library names, or an empty list on failure.</param>
        public AndroidLibraryStatus(bool _FailedToRetrieveData, List<String> _Libraries)
        {
            this.FailedToRetrieveData = _FailedToRetrieveData;
            this.Libraries = _Libraries;
        }
    }
}