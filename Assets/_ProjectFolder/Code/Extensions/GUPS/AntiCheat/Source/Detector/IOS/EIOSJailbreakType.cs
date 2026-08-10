namespace GUPS.AntiCheat.Detector.IOS
{
    /// <summary>
    /// Kinds of jailbreak evidence that can be reported by <see cref="IOSJailbreakDetector"/>.
    /// </summary>
    /// <remarks>
    /// The numeric values mirror the <c>GupsJailbreakType</c> enum in the native Objective-C++ plugin
    /// (<c>Native/iOS/GupsAntiCheatJailbreak/GupsAntiCheatJailbreak.h</c>). Keep both sides in sync when adding new
    /// categories - the native side writes the int directly into <see cref="IOSJailbreakDetectionStatus"/>.
    /// </remarks>
    public enum EIOSJailbreakType : byte
    {
        /// <summary>
        /// Default sentinel used when the type of jailbreak evidence could not be classified.
        /// </summary>
        UNKNOWN = 0,

        /// <summary>
        /// A jailbreak-related URL scheme (cydia, sileo, zbra, filza, ...) returned <c>true</c> from
        /// <c>UIApplication.canOpenURL</c>. Requires the matching schemes under
        /// <c>LSApplicationQueriesSchemes</c> in <c>Info.plist</c>.
        /// </summary>
        URL_SCHEME = 1,

        /// <summary>
        /// A path typical for a jailbreak (Cydia.app, /private/var/lib/apt, /usr/sbin/sshd,
        /// /Library/MobileSubstrate, ..., or a rootless /var/jb prefix) was found on the device.
        /// </summary>
        SUSPICIOUS_PATH = 2,

        /// <summary>
        /// The process was able to create a file outside of its app sandbox. On a stock device the
        /// sandbox blocks the write; success indicates the sandbox is not enforced.
        /// </summary>
        SANDBOX_VIOLATION = 3,

        /// <summary>
        /// <c>fork()</c> returned a valid child PID. Sandboxed iOS apps cannot fork - a positive
        /// return is a strong jailbreak signal. Opt-in via <c>GlobalSettings.IOS_DetectFork</c>
        /// (default <c>false</c>).
        /// </summary>
        FORK_SUCCESS = 4,

        /// <summary>
        /// <c>DYLD_INSERT_LIBRARIES</c> or <c>DYLD_FORCE_FLAT_NAMESPACE</c> is populated in the
        /// process environment. iOS strips these for sandboxed apps; a populated value indicates
        /// runtime library injection.
        /// </summary>
        DYLD_INJECTION = 5,

        /// <summary>
        /// A loaded dyld image matches a known tweak framework substring (MobileSubstrate,
        /// CydiaSubstrate, libsubstitute, libhooker, FridaGadget, ...).
        /// </summary>
        SUSPICIOUS_DYLIB = 6,
    }
}
