// System
using System;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Monitor.Android
{
    /// <summary>
    /// Common interface for Android monitor status payloads.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Monitor.Android.AndroidPackageHashMonitor"/>
    public interface IAndroidStatus : IWatchedSubject
    {
        /// <summary>
        /// Gets a value indicating whether the underlying data point could not be retrieved (e.g. JNI failure).
        /// </summary>
        bool FailedToRetrieveData { get; }
    }
}