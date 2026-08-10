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
    /// Monitors Android devices to compute the signing certificate fingerprint of the installed app (APK/AAB) and notify observers with an <see cref="AndroidFingerprintStatus"/>.
    /// </summary>
    /// <remarks>
    /// The hash algorithm is read from <see cref="GlobalSettings.Android_AppFingerprintAlgorithm"/> and defaults to SHA-256.
    /// The fingerprint is computed once on <see cref="AMonitor.OnStart"/> via a JNI call to a bundled Java helper class
    /// (<c>com.gups.anticheat.android.signature.SignatureReader#getSigningSignature</c>) and is only available on Android player builds.
    /// Pin the expected fingerprint against the value reported by your release-signing certificate to detect rebuilds signed by a different key.
    /// On Android 9+ (API level 28) the helper uses <c>PackageInfo.signingInfo</c>; older API levels fall back to <c>PackageInfo.signatures</c>.
    /// </remarks>
    /// <seealso cref="AndroidPackageHashMonitor"/>
    public class AndroidPackageFingerprintMonitor : AMonitor
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Android Package Fingerprint Monitor";

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Computes the signing fingerprint on start and notifies observers with an <see cref="AndroidFingerprintStatus"/>.
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

#if UNITY_ANDROID && !UNITY_EDITOR

            // Resolve the configured algorithm to the canonical name the Java helper expects (e.g. "SHA-256").
            String var_Algorithm = HashHelper.GetName(this.GetAlgorithm());

            // Hash the signing certificate via JNI; success/failure is captured for the status payload.
            bool var_Success = this.TryGetFingerprint(var_Algorithm, out String var_Fingerprint);

            // Notify observers with a single status so they can compare against the pinned fingerprint.
            this.Notify(new AndroidFingerprintStatus(!var_Success, var_Algorithm, var_Fingerprint));
            
#else

            UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] {0} is only available on Android devices!", this.Name));

#endif
        }

        #endregion

        // Fingerprint
        #region Fingerprint

        /// <summary>
        /// Gets the hash algorithm to use, falling back to <see cref="EHashAlgorithm.SHA256"/> when no global settings are available.
        /// </summary>
        /// <returns>The hash algorithm to use.</returns>
        private EHashAlgorithm GetAlgorithm()
        {
            return GlobalSettings.Instance?.Android_AppFingerprintAlgorithm ?? EHashAlgorithm.SHA256;
        }

        /// <summary>
        /// Tries to compute the signing certificate fingerprint via JNI.
        /// </summary>
        /// <param name="_Algorithm">The algorithm name passed to the Java helper.</param>
        /// <param name="_Fingerprint">When the call succeeds, the hex-encoded fingerprint; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the fingerprint was retrieved; otherwise <c>false</c>.</returns>
        private bool TryGetFingerprint(String _Algorithm, out String _Fingerprint)
        {
            try
            {
                // JNI: invoke the bundled Java helper that hashes the signing certificate.
                using (AndroidJavaClass var_JavaClass = new AndroidJavaClass("com.gups.anticheat.android.signature.SignatureReader"))
                {
                    String var_Result = var_JavaClass.CallStatic<String>("getSigningSignature", _Algorithm);

                    // A null response indicates the Java side could not read the signing info; treat as a soft failure.
                    if (var_Result == null)
                    {
                        _Fingerprint = null;

                        return false;
                    }

                    UnityEngine.Debug.Log(String.Format("[GUPS][AntiCheat] Calculated app signature: '{0}'", var_Result));

                    _Fingerprint = var_Result;

                    return true;
                }
            }
            catch (Exception var_Exception)
            {
                // Any JNI exception (e.g. missing helper class) is downgraded to a warning to avoid crashing the player.
                UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] Could not read android fingerprint / signature: {0}!", var_Exception));

                _Fingerprint = null;

                return false;
            }
        }

        #endregion
    }
}
