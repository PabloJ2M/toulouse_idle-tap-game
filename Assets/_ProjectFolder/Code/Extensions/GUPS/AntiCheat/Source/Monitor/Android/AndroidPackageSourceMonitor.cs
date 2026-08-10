// System
using System;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Monitor.Android
{
    /// <summary>
    /// Monitors the installation source (app store) of the installed app on Android and notifies observers with an <see cref="AndroidSourceStatus"/>.
    /// </summary>
    /// <remarks>
    /// The lookup is performed once on <see cref="AMonitor.OnStart"/> via a JNI call to a bundled Java helper class
    /// (<c>com.gups.anticheat.android.store.PackageInstallerReader#getAppStore</c>) and is only available on Android player builds.
    /// On Android 11+ (API level 30) the helper uses <c>PackageManager.getInstallSourceInfo</c>; older API levels fall back to the deprecated <c>getInstallerPackageName</c>.
    /// When the installer cannot be determined the status reports <see cref="EAppStore.Unknown"/> together with a failure flag.
    /// </remarks>
    /// <seealso cref="AndroidPackageHashMonitor"/>
    public class AndroidPackageSourceMonitor : AMonitor
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Android Package Source Monitor";

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Reads the installation source on start and notifies observers with an <see cref="AndroidSourceStatus"/>.
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

#if UNITY_ANDROID && !UNITY_EDITOR

            bool var_Success = this.TryGetAppStoreSource(out EAppStore var_AppStore, out String _AppStorePackage);

            this.Notify(new AndroidSourceStatus(!var_Success, var_AppStore, _AppStorePackage));
            
#else

            UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] {0} is only available on Android devices!", this.Name));

#endif
        }

        #endregion

        // Source
        #region Source

        /// <summary>
        /// Tries to read the installer package name of the running app via JNI and map it to a known <see cref="EAppStore"/>.
        /// </summary>
        /// <param name="_AppStore">The mapped app store, or <see cref="EAppStore.Unknown"/> on failure.</param>
        /// <param name="_AppStorePackage">The raw installer package name, or <c>null</c> on failure.</param>
        /// <returns><c>true</c> if the installer package was retrieved; otherwise <c>false</c>.</returns>
        private bool TryGetAppStoreSource(out EAppStore _AppStore, out String _AppStorePackage)
        {
            try
            {
                // JNI: invoke the bundled Java helper that returns the installer package name.
                using (AndroidJavaClass var_JavaClass = new AndroidJavaClass("com.gups.anticheat.android.store.PackageInstallerReader"))
                {
                    String var_Result = var_JavaClass.CallStatic<String>("getAppStore");

                    if (var_Result == null)
                    {
                        _AppStore = EAppStore.Unknown;
                        _AppStorePackage = null;

                        return false;
                    }

                    UnityEngine.Debug.Log(String.Format("[GUPS][AntiCheat] App installation source: '{0}'", var_Result));

                    _AppStore = AppStoreHelper.GetStore(var_Result);
                    _AppStorePackage = var_Result;

                    return true;
                }
            }
            catch (Exception var_Exception)
            {
                UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] Could not read android app store source: {0}!", var_Exception));

                _AppStore = EAppStore.Unknown;
                _AppStorePackage = null;

                return false;
            }
        }

        #endregion
    }
}
