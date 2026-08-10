// System
using System;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Hash;

// GUPS - AntiCheat
using GUPS.AntiCheat.Settings;

namespace GUPS.AntiCheat.Monitor.Android
{
    /// <summary>
    /// Monitors Android devices to compute the hash of the installed app (APK/AAB) and notify observers with an <see cref="AndroidHashStatus"/>.
    /// </summary>
    /// <remarks>
    /// The hash algorithm is read from <see cref="GlobalSettings.Android_AppFingerprintAlgorithm"/> and defaults to SHA-256.
    /// The hash is computed once on <see cref="AMonitor.OnStart"/> via a JNI call to a bundled Java helper class
    /// (<c>com.gups.anticheat.android.hash.HashReader#getAppHash</c>) and is only available on Android player builds.
    /// Compare the reported hash against a known-good value (e.g. from a remote endpoint) to detect repackaging or tampering.
    /// </remarks>
    /// <example>
    /// Read the latest computed APK / AAB hash from the monitor:
    /// <code>
    /// using System;
    /// using GUPS.AntiCheat.Core.Watch;
    /// using GUPS.AntiCheat.Monitor.Android;
    /// using UnityEngine;
    ///
    /// public class HashLogger : MonoBehaviour, IObserver&lt;IWatchedSubject&gt;
    /// {
    ///     private void Start()
    ///     {
    ///         var monitor = this.GetComponent&lt;AndroidPackageHashMonitor&gt;();
    ///         monitor.Subscribe(this);
    ///     }
    ///
    ///     public void OnNext(IWatchedSubject value)
    ///     {
    ///         if (value is AndroidHashStatus status &amp;&amp; !status.FailedToRetrieveData)
    ///         {
    ///             Debug.Log($"App hash: {status.Hash} (algorithm: {status.Algorithm})");
    ///         }
    ///     }
    ///
    ///     public void OnError(Exception error) { }
    ///     public void OnCompleted() { }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="AndroidHashStatus"/>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public class AndroidPackageHashMonitor : AMonitor
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Android Package Hash Monitor";

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Computes the app (APK/AAB) hash on start and notifies observers with an <see cref="AndroidHashStatus"/>.
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

#if UNITY_ANDROID && !UNITY_EDITOR

            String var_Algorithm = HashHelper.GetName(this.GetAlgorithm());

            bool var_Success = this.TryGetHash(var_Algorithm, out String var_Hash);

            this.Notify(new AndroidHashStatus(!var_Success, var_Algorithm, var_Hash));
            
#else

            UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] {0} is only available on Android devices!", this.Name));

#endif
        }

        #endregion

        // Hash
        #region Hash

        /// <summary>
        /// Gets the hash algorithm to use, falling back to <see cref="EHashAlgorithm.SHA256"/> when no global settings are available.
        /// </summary>
        /// <returns>The hash algorithm to use.</returns>
        private EHashAlgorithm GetAlgorithm()
        {
            return GlobalSettings.Instance?.Android_AppFingerprintAlgorithm ?? EHashAlgorithm.SHA256;
        }

        /// <summary>
        /// Tries to compute the hash of the installed app via JNI.
        /// </summary>
        /// <param name="_Algorithm">The algorithm name passed to the Java helper.</param>
        /// <param name="_Hash">When the call succeeds, the hex-encoded app hash; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the hash was retrieved; otherwise <c>false</c>.</returns>
        private bool TryGetHash(String _Algorithm, out String _Hash)
        {
            try
            {
                // JNI: invoke the bundled Java helper that hashes the running APK / AAB.
                using (AndroidJavaClass var_JavaClass = new AndroidJavaClass("com.gups.anticheat.android.hash.HashReader"))
                {
                    String var_Result = var_JavaClass.CallStatic<String>("getAppHash", _Algorithm);

                    if (var_Result == null)
                    {
                        _Hash = null;

                        return false;
                    }

                    UnityEngine.Debug.Log(String.Format("[GUPS][AntiCheat] Calculated app hash: '{0}'", var_Result));

                    _Hash = var_Result;

                    return true;
                }
            }
            catch (Exception var_Exception)
            {
                UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] Could not read android app hash: {0}!", var_Exception));

                _Hash = null;

                return false;
            }
        }

        #endregion
    }
}
