// Microsoft
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

// Unity
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Hash;

// GUPS - AntiCheat
using GUPS.AntiCheat.Settings;

namespace GUPS.AntiCheat.Editor.Build
{
    /// <summary>
    /// Post-processor for Android builds that:
    /// <list type="bullet">
    ///   <item><description>injects <c>&lt;package&gt;</c> query entries into the generated Gradle
    ///   <c>AndroidManifest.xml</c> so the runtime can query blacklisted applications under Android's
    ///   package visibility model, and</description></item>
    ///   <item><description>hashes the produced APK or AAB after the build and logs the result.</description></item>
    /// </list>
    /// </summary>
    internal class PostProcessAndroidBuild : IPostGenerateGradleAndroidProject, IPostprocessBuildWithReport
    {
        /// <summary>
        /// Gets the callback order. Set to <see cref="Int32.MaxValue"/> - 1 so the manifest is patched and the
        /// build hash is computed after all other post-processors.
        /// </summary>
        public int callbackOrder => Int32.MaxValue - 1;

        /// <summary>
        /// Builds the path to <c>AndroidManifest.xml</c> inside the generated Gradle project.
        /// </summary>
        /// <param name="_BasePath">Base path of the Gradle project.</param>
        /// <returns>Full path to the manifest file.</returns>
        private String GetManifestPath(String _BasePath)
        {
            return Path.Combine(_BasePath, "src", "main", "AndroidManifest.xml");
        }

        /// <summary>
        /// Loads the generated <c>AndroidManifest.xml</c>, adds a <c>&lt;package&gt;</c> query entry per
        /// blacklisted application from <see cref="GlobalSettings"/>, and saves it.
        /// </summary>
        /// <param name="_BasePath">Base path of the generated Gradle project.</param>
        public void OnPostGenerateGradleAndroidProject(String _BasePath)
        {
#if UNITY_ANDROID

            String var_ManifestPath = this.GetManifestPath(_BasePath);

            AndroidManifest var_Manifest = new AndroidManifest(var_ManifestPath);

            List<String> var_AppPackages = this.GetAppPackagesToFind();

            for (int i = 0; i < var_AppPackages.Count; i++)
            {
                var_Manifest.AddQueryPackage(var_AppPackages[i]);
            }

            var_Manifest.Save();

#endif
        }

        /// <summary>
        /// Gets the application package names to add to the manifest's <c>&lt;queries&gt;</c> section.
        /// </summary>
        /// <returns>The configured blacklisted applications, or an empty list when no global settings exist.</returns>
        private List<String> GetAppPackagesToFind()
        {
            return GlobalSettings.Instance?.Android_BlacklistedApplications ?? new List<String>();
        }

        /// <summary>
        /// On Android builds: hashes the produced APK or AAB with the algorithm from <see cref="GlobalSettings"/>
        /// (skipped if <see cref="EHashAlgorithm.NONE"/>) and logs the result, version and output path.
        /// </summary>
        /// <param name="_Report">Build report describing the completed build.</param>
        public void OnPostprocessBuild(BuildReport _Report)
        {
#if UNITY_ANDROID

            String var_OutputPath = _Report.summary.outputPath;

            EHashAlgorithm var_HashAlgorithm = GlobalSettings.Instance?.Android_AppHashAlgorithm ?? EHashAlgorithm.SHA256;

            if (var_HashAlgorithm == EHashAlgorithm.NONE)
            {
                UnityEngine.Debug.LogWarning("[GUPS][AntiCheat] The hash algorithm is set to NONE. The hash of the build APK/AAB file will not be calculated.");
                return;
            }

            String var_Hash = CalculateHexedHash(var_OutputPath, var_HashAlgorithm);

            UnityEngine.Debug.Log(String.Format(
                "[GUPS][AntiCheat] App hash: {0} with algorithm: {1} for version: {2} at path: {3}.",
                var_Hash,
                HashHelper.GetName(var_HashAlgorithm),
                UnityEngine.Application.version,
                var_OutputPath));

#endif
        }

        /// <summary>
        /// Computes the hash of the file at <paramref name="_Path"/> and returns it as a hexadecimal string.
        /// </summary>
        /// <param name="_Path">Path to the APK or AAB to hash.</param>
        /// <param name="_HashAlgorithm">Hash algorithm to use.</param>
        /// <returns>Hex-encoded hash of the file.</returns>
        private String CalculateHexedHash(String _Path, EHashAlgorithm _HashAlgorithm)
        {
            using (FileStream var_FileStream = new FileStream(_Path, FileMode.Open, FileAccess.Read))
            {
                byte[] var_HashedBytes = HashHelper.ComputeHash(_HashAlgorithm, var_FileStream);

                return HashHelper.ToHex(var_HashedBytes, true, true);
            }
        }

        /// <summary>
        /// XML document specialized for Android resources; tracks the source path and pre-registers the Android namespace.
        /// </summary>
        internal class AndroidXmlDocument : XmlDocument
        {
            /// <summary>
            /// Path the document was loaded from.
            /// </summary>
            private String filePath;

            /// <summary>
            /// Namespace manager pre-populated with the Android namespace.
            /// </summary>
            protected XmlNamespaceManager namespaceManager;

            /// <summary>
            /// Standard Android XML namespace URI.
            /// </summary>
            public readonly String AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

            /// <summary>
            /// Loads the document from <paramref name="_Path"/> and registers the Android namespace.
            /// </summary>
            /// <param name="_Path">Path of the XML file to load.</param>
            public AndroidXmlDocument(String _Path)
            {
                this.filePath = _Path;
                using (var var_Reader = new XmlTextReader(this.filePath))
                {
                    var_Reader.Read();
                    this.Load(var_Reader);
                }
                this.namespaceManager = new XmlNamespaceManager(this.NameTable);
                this.namespaceManager.AddNamespace("android", this.AndroidXmlNamespace);
            }

            /// <summary>
            /// Saves the document back to the path it was loaded from.
            /// </summary>
            public void Save()
            {
                this.SaveAt(this.filePath);
            }

            /// <summary>
            /// Saves the document to <paramref name="_Path"/> with indented formatting and UTF-8 without BOM.
            /// </summary>
            /// <param name="_Path">Destination path.</param>
            public void SaveAt(String _Path)
            {
                using (var var_Writer = new XmlTextWriter(_Path, new UTF8Encoding(false)))
                {
                    var_Writer.Formatting = Formatting.Indented;
                    this.Save(var_Writer);
                }
            }
        }

        /// <summary>
        /// Convenience wrapper around <see cref="AndroidXmlDocument"/> for editing <c>AndroidManifest.xml</c>
        /// (uses-permission entries, query packages).
        /// </summary>
        internal class AndroidManifest : AndroidXmlDocument
        {
            /// <summary>
            /// The root <c>&lt;manifest&gt;</c> element.
            /// </summary>
            private readonly XmlElement ManifestElement;

            /// <summary>
            /// Loads the manifest from <paramref name="_Path"/> and caches the root <c>&lt;manifest&gt;</c> element.
            /// </summary>
            /// <param name="_Path">Path of the manifest file.</param>
            public AndroidManifest(String _Path) : base(_Path)
            {
                this.ManifestElement = this.SelectSingleNode("/manifest") as XmlElement;
            }

            /// <summary>
            /// Creates an attribute in the Android namespace with the given key and value.
            /// </summary>
            /// <param name="key">Attribute key (e.g. "name").</param>
            /// <param name="value">Attribute value.</param>
            /// <returns>The created <see cref="XmlAttribute"/>.</returns>
            private XmlAttribute CreateAndroidAttribute(String key, String value)
            {
                XmlAttribute attr = CreateAttribute("android", key, this.AndroidXmlNamespace);
                attr.Value = value;
                return attr;
            }

            /// <summary>
            /// Appends a <c>&lt;uses-permission&gt;</c> element with the given permission name.
            /// </summary>
            /// <param name="_Permission">Permission identifier (e.g. "android.permission.INTERNET").</param>
            public void AddUsesPermission(String _Permission)
            {
                XmlElement child = this.CreateElement("uses-permission");
                this.ManifestElement.AppendChild(child);

                XmlAttribute newAttribute = this.CreateAndroidAttribute("name", _Permission);
                child.Attributes.Append(newAttribute);
            }

            /// <summary>
            /// Adds a <c>&lt;package&gt;</c> entry under <c>&lt;queries&gt;</c>, creating the section if missing.
            /// </summary>
            /// <param name="_Name">Package name to add (e.g. "com.example.app").</param>
            public void AddQueryPackage(String _Name)
            {
                XmlElement queryPackageElement = this.ManifestElement.SelectSingleNode("queries") as XmlElement;

                if (queryPackageElement == null)
                {
                    queryPackageElement = this.CreateElement("queries");
                    this.ManifestElement.AppendChild(queryPackageElement);
                }

                XmlElement packageElement = this.CreateElement("package");
                queryPackageElement.AppendChild(packageElement);

                XmlAttribute newAttribute = this.CreateAndroidAttribute("name", _Name);
                packageElement.Attributes.Append(newAttribute);
            }
        }
    }
}
