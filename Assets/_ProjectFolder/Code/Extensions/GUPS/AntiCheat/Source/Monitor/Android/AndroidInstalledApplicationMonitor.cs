// System
using System;
using System.Collections.Generic;

// Unity
using UnityEngine;

// GUPS - AntiCheat
using GUPS.AntiCheat.Settings;

namespace GUPS.AntiCheat.Monitor.Android
{
    /// <summary>
    /// Monitors which blacklisted user applications are installed on an Android device and notifies observers with an <see cref="AndroidInstalledApplicationStatus"/>.
    /// </summary>
    /// <remarks>
    /// System apps are ignored. The blacklist is read from <see cref="GlobalSettings.Android_BlacklistedApplications"/>.
    /// The lookup is performed once on <see cref="AMonitor.OnStart"/> via a JNI call to a bundled Java helper class
    /// (<c>com.gups.anticheat.android.app.ApplicationReader#getSpecificInstalledAppPackages</c>) and is only available on Android player builds.
    /// On Android 11+ (API level 30) querying installed packages requires the <c>QUERY_ALL_PACKAGES</c> permission or matching <c>queries</c> manifest entries.
    /// </remarks>
    /// <seealso cref="AndroidPackageHashMonitor"/>
    public class AndroidInstalledApplicationMonitor : AMonitor
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Android Installed Applications Monitor";

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Reads the blacklisted installed applications on start and notifies observers with an <see cref="AndroidInstalledApplicationStatus"/>.
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

#if UNITY_ANDROID && !UNITY_EDITOR

            // Query the device for the subset of blacklisted packages that are actually installed.
            bool var_Success = this.TryGetInstalledApplications(out List<String> var_Applications, this.GetAppPackagesToFind());

            // Push a single status to observers; the failure flag mirrors a JNI lookup that returned null / threw.
            this.Notify(new AndroidInstalledApplicationStatus(!var_Success, var_Applications));

#else

            UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] {0} is only available on Android devices!", this.Name));

#endif
        }

        #endregion

        // Application
        #region Application

        /// <summary>
        /// Gets the blacklist of package names to look for from <see cref="GlobalSettings"/>.
        /// </summary>
        /// <returns>The blacklisted package names, or an empty list if no global settings are available.</returns>
        private List<String> GetAppPackagesToFind()
        {
            return GlobalSettings.Instance?.Android_BlacklistedApplications ?? new List<String>();
        }

        /// <summary>
        /// Tries to retrieve the subset of installed packages that match the supplied blacklist via JNI.
        /// </summary>
        /// <param name="_FoundApplications">When the call succeeds, the matching installed package names; otherwise an empty list.</param>
        /// <param name="_SearchApplications">The blacklisted package names to look up on the device.</param>
        /// <returns><c>true</c> if the lookup succeeded; otherwise <c>false</c>.</returns>
        private bool TryGetInstalledApplications(out List<String> _FoundApplications, List<String> _SearchApplications)
        {
            try
            {
                // JNI: invoke the bundled Java helper that filters installed packages by the supplied blacklist.
                using (AndroidJavaClass var_JavaClass = new AndroidJavaClass("com.gups.anticheat.android.app.ApplicationReader"))
                {
                    // The CallStatic varargs path treats String[] as multiple arguments, so cast to object to pass it as a single array argument.
                    String[] var_Result = var_JavaClass.CallStatic<String[]>("getSpecificInstalledAppPackages", (object)_SearchApplications.ToArray());

                    // A null response signals the Java side failed to enumerate packages; treat as a soft failure.
                    if (var_Result == null)
                    {
                        _FoundApplications = new List<String>();

                        return false;
                    }

                    UnityEngine.Debug.Log(String.Format("[GUPS][AntiCheat] Found installed applications: '{0}'", String.Join(", ", var_Result)));

                    // Materialise to a List so consumers can mutate / iterate without surprises.
                    _FoundApplications = new List<String>(var_Result);

                    return true;
                }
            }
            catch (Exception var_Exception)
            {
                // JNI exceptions (missing class, permission denied, etc.) downgrade to a logged warning.
                UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] Could not read android applications: {0}!", var_Exception));

                _FoundApplications = new List<String>();

                return false;
            }
        }

        #endregion
    }
}
