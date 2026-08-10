namespace GUPS.AntiCheat.Detector.IOS.Platform
{
    /// <summary>
    /// Returns the <see cref="IIOSJailbreakProbe"/> implementation matching the current Unity runtime platform.
    /// </summary>
    /// <remarks>
    /// The factory caches a single shared instance. The detector should call <see cref="Get"/> once and reuse the
    /// returned probe for its lifetime. On the Unity editor and on every non-iOS platform a
    /// <see cref="NoOpIOSJailbreakProbe"/> is returned whose scans always report negative.
    /// </remarks>
    public static class IOSJailbreakProbeFactory
    {
        /// <summary>
        /// The cached probe instance, created lazily on the first <see cref="Get"/> call.
        /// </summary>
        private static IIOSJailbreakProbe cached;

        /// <summary>
        /// Returns the iOS jailbreak probe matching the current Unity runtime platform.
        /// </summary>
        /// <returns>The cached probe instance.</returns>
        public static IIOSJailbreakProbe Get()
        {
            if (cached != null)
            {
                return cached;
            }

#if UNITY_IOS && !UNITY_EDITOR
            cached = new IOSJailbreakProbeNative();
#else
            cached = new NoOpIOSJailbreakProbe();
#endif

            return cached;
        }
    }
}
