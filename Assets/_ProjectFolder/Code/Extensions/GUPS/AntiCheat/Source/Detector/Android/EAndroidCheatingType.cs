namespace GUPS.AntiCheat.Detector.Android
{
    /// <summary>
    /// Kinds of cheating that can be reported by the Android detectors.
    /// </summary>
    public enum EAndroidCheatingType : byte
    {
        /// <summary>
        /// Default sentinel used when the type of cheating could not be classified.
        /// </summary>
        UNKNOWN = 0,

        /// <summary>
        /// The app package was installed from a store that is not on the allow list.
        /// </summary>
        PACKAGE_SOURCE = 1,

        /// <summary>
        /// The app package hash does not match the expected hash.
        /// </summary>
        PACKAGE_HASH = 2,

        /// <summary>
        /// The app package certificate fingerprint does not match the expected fingerprint.
        /// </summary>
        PACKAGE_FINGERPRINT = 3,

        /// <summary>
        /// The app package contains a native library that is not allowed.
        /// </summary>
        PACKAGE_LIBRARY = 4,

        /// <summary>
        /// The device has installed applications that are on the blacklist.
        /// </summary>
        DEVICE_INSTALLED_APPS = 10,
    }
}
