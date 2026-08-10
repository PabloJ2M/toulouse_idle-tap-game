// System
using System;
using System.Collections.Generic;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Monitor.Android
{
    /// <summary>
    /// Monitors the native libraries bundled inside the installed app (APK/AAB) and notifies observers with an <see cref="AndroidLibraryStatus"/>.
    /// </summary>
    /// <remarks>
    /// Reads entries such as <c>lib/armeabi-v7a/*.so</c> from the running package; cheaters often inject extra libraries to hook the runtime.
    /// The lookup is performed once on <see cref="AMonitor.OnStart"/> via a JNI call to a bundled Java helper class
    /// (<c>com.gups.anticheat.android.binary.LibraryReader#getLibraryNames</c>) and is only available on Android player builds.
    /// </remarks>
    /// <seealso cref="AndroidPackageHashMonitor"/>
    public class AndroidPackageLibraryMonitor : AMonitor
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Android Package Library Monitor";

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Reads the bundled native libraries on start and notifies observers with an <see cref="AndroidLibraryStatus"/>.
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

#if UNITY_ANDROID && !UNITY_EDITOR

            bool var_Success = this.TryGetLibraries(out List<String> var_Libraries);

            this.Notify(new AndroidLibraryStatus(!var_Success, var_Libraries));
            
#else

            UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] {0} is only available on Android devices!", this.Name));

#endif
        }

        #endregion

        // Library
        #region Library

        /// <summary>
        /// Tries to read the native libraries bundled in the running app via JNI.
        /// </summary>
        /// <param name="_Libraries">When the call succeeds, the bundled library names; otherwise an empty list.</param>
        /// <returns><c>true</c> if the list was retrieved; otherwise <c>false</c>.</returns>
        private bool TryGetLibraries(out List<String> _Libraries)
        {
            try
            {
                // JNI: invoke the bundled Java helper that enumerates the .so entries in the APK / AAB.
                using (AndroidJavaClass var_JavaClass = new AndroidJavaClass("com.gups.anticheat.android.binary.LibraryReader"))
                {
                    String[] var_Result = var_JavaClass.CallStatic<String[]>("getLibraryNames");

                    if (var_Result == null)
                    {
                        _Libraries = new List<String>();

                        return false;
                    }

                    UnityEngine.Debug.Log(String.Format("[GUPS][AntiCheat] Found package libraries in app: '{0}'", String.Join(", ", var_Result)));

                    _Libraries = new List<String>(var_Result);

                    return true;
                }
            }
            catch (Exception var_Exception)
            {
                UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] Could not read android app libraries: {0}!", var_Exception));

                _Libraries = new List<String>();

                return false;
            }
        }

        #endregion
    }
}
