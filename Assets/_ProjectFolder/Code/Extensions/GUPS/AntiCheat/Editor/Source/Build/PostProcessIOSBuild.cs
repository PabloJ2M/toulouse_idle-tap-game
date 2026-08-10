// Microsoft
using System;
using System.IO;

// Unity
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

// GUPS - AntiCheat
using GUPS.AntiCheat.Settings;

namespace GUPS.AntiCheat.Editor.Build
{
    /// <summary>
    /// Post-processor that updates the generated Xcode project's <c>Info.plist</c> so the
    /// <c>IOSJailbreakDetector</c> URL-scheme probe works on modern iOS.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Since iOS 9, <c>UIApplication.canOpenURL</c> only honors schemes that are declared under
    /// <c>LSApplicationQueriesSchemes</c>. Without these entries the jailbreak URL-scheme probe is a silent no-op.
    /// </para>
    /// <para>
    /// The processor runs at <see cref="Int32.MaxValue"/> - 1 so it executes after Unity's own iOS post-processing
    /// has assembled the Xcode project. It is a no-op for non-iOS builds and when
    /// <see cref="GlobalSettings.IOS_DetectUrlSchemes"/> is disabled. The added schemes are merged with whatever the
    /// game project already declares so existing entries are preserved.
    /// </para>
    /// </remarks>
    internal class PostProcessIOSBuild : IPostprocessBuildWithReport
    {
        /// <summary>
        /// Gets the callback order. Set to <see cref="Int32.MaxValue"/> - 1 so the plist patch runs at the very end of
        /// the iOS build pipeline, after Unity has assembled the Xcode project.
        /// </summary>
        public int callbackOrder => Int32.MaxValue - 1;

        /// <summary>
        /// On iOS builds: when <see cref="GlobalSettings.IOS_DetectUrlSchemes"/> is enabled, merges the jailbreak
        /// URL schemes into <c>LSApplicationQueriesSchemes</c> in the generated <c>Info.plist</c>.
        /// </summary>
        /// <param name="_Report">Build report describing the completed build.</param>
        public void OnPostprocessBuild(BuildReport _Report)
        {
#if UNITY_IOS

            // Bail out for every non-iOS build target. The callback is invoked once per build regardless of platform.
            if (_Report.summary.platform != BuildTarget.iOS)
            {
                return;
            }

            // The URL-scheme probe is opt-in; respect the user's toggle.
            GlobalSettings var_Settings = GlobalSettings.Instance;
            if (var_Settings != null && !var_Settings.IOS_DetectUrlSchemes)
            {
                UnityEngine.Debug.Log("[GUPS][AntiCheat] IOS_DetectUrlSchemes is disabled; skipping LSApplicationQueriesSchemes injection.");
                return;
            }

            // The schemes come from the settings asset; there is no built-in fallback list. Without the asset (or
            // with an empty list) there is nothing to probe and nothing to inject.
            if (var_Settings == null || var_Settings.IOS_SuspiciousUrlSchemes == null || var_Settings.IOS_SuspiciousUrlSchemes.Count == 0)
            {
                UnityEngine.Debug.Log("[GUPS][AntiCheat] No suspicious URL schemes configured; skipping LSApplicationQueriesSchemes injection.");
                return;
            }

            String var_PlistPath = Path.Combine(_Report.summary.outputPath, "Info.plist");
            if (!File.Exists(var_PlistPath))
            {
                UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] iOS post-processor could not find Info.plist at '{0}'; LSApplicationQueriesSchemes was not patched.", var_PlistPath));
                return;
            }

            try
            {
                PlistDocument var_Plist = new PlistDocument();
                var_Plist.ReadFromFile(var_PlistPath);

                // Fetch or create the LSApplicationQueriesSchemes array; preserve whatever the game project already
                // ships so we never clobber a publisher's manual entries.
                PlistElement var_Existing = var_Plist.root["LSApplicationQueriesSchemes"];
                PlistElementArray var_SchemesArray = var_Existing as PlistElementArray;
                if (var_SchemesArray == null)
                {
                    var_SchemesArray = var_Plist.root.CreateArray("LSApplicationQueriesSchemes");
                }

                int var_AddedCount = 0;

                foreach (String var_Scheme in var_Settings.IOS_SuspiciousUrlSchemes)
                {
                    if (String.IsNullOrWhiteSpace(var_Scheme))
                    {
                        continue;
                    }

                    if (!ContainsString(var_SchemesArray, var_Scheme))
                    {
                        var_SchemesArray.AddString(var_Scheme);
                        var_AddedCount++;
                    }
                }

                var_Plist.WriteToFile(var_PlistPath);

                UnityEngine.Debug.Log(String.Format("[GUPS][AntiCheat] Patched Info.plist - added {0} jailbreak URL scheme(s) to LSApplicationQueriesSchemes.", var_AddedCount));
            }
            catch (Exception var_Exception)
            {
                // Never fail the build over a plist patch - just warn so the developer can fix it manually.
                UnityEngine.Debug.LogWarning(String.Format("[GUPS][AntiCheat] Failed to patch Info.plist with jailbreak URL schemes: {0}", var_Exception));
            }
#endif
        }

#if UNITY_IOS

        /// <summary>
        /// Returns whether the supplied <see cref="PlistElementArray"/> already contains a string entry with the given value.
        /// </summary>
        /// <param name="_Array">The array to inspect.</param>
        /// <param name="_Value">The string value to look for.</param>
        /// <returns><c>true</c> if the value is already present; otherwise <c>false</c>.</returns>
        private static bool ContainsString(PlistElementArray _Array, String _Value)
        {
            foreach (PlistElement var_Element in _Array.values)
            {
                if (var_Element is PlistElementString var_StringElement && String.Equals(var_StringElement.value, _Value, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

#endif
    }
}
