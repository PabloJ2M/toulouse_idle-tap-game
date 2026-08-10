namespace GUPS.AntiCheat.Detector.Android
{
    /// <summary>
    /// Default <see cref="IAndroidCheatingDetectionStatus"/> implementation carrying the false-positive likelihood,
    /// threat rating, detected Android cheating type and whether the source monitor failed to retrieve its data.
    /// </summary>
    public struct AndroidCheatingDetectionStatus : IAndroidCheatingDetectionStatus
    {
        /// <summary>
        /// Gets the probability that the reported threat is a false positive, in the range <c>[0.0, 1.0]</c>.
        /// </summary>
        public float PossibilityOfFalsePositive { get; private set; }

        /// <summary>
        /// Gets the threat rating reported with this detection. Higher values denote greater perceived threats.
        /// </summary>
        public uint ThreatRating { get; private set; }

        /// <summary>
        /// Gets the type of cheating that was detected on the Android device, or
        /// <see cref="EAndroidCheatingType.UNKNOWN"/> if no specific type could be classified.
        /// </summary>
        public EAndroidCheatingType AndroidCheatingType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the source monitor failed to retrieve its data over the native interface.
        /// </summary>
        /// <remarks>
        /// When <c>true</c>, the detector could not perform a real validation - the most common cause is that parts of
        /// the native implementation are not available on the current Android device (requires at least SDK v19).
        /// </remarks>
        public bool MonitorFailedToRetrieveData { get; private set; }

        /// <summary>
        /// Creates a new <see cref="AndroidCheatingDetectionStatus"/> with the given fields.
        /// </summary>
        /// <param name="_PossibilityOfFalsePositive">The false-positive likelihood in the range <c>[0.0, 1.0]</c>.</param>
        /// <param name="_ThreatRating">The threat rating reported with the detection.</param>
        /// <param name="_AndroidCheatingType">The type of cheating detected on the Android device.</param>
        /// <param name="_MonitorFailedToRetrieveData">Whether the source monitor failed to retrieve its data.</param>
        public AndroidCheatingDetectionStatus(float _PossibilityOfFalsePositive, uint _ThreatRating, EAndroidCheatingType _AndroidCheatingType, bool _MonitorFailedToRetrieveData)
        {
            this.PossibilityOfFalsePositive = _PossibilityOfFalsePositive;
            this.ThreatRating = _ThreatRating;
            this.AndroidCheatingType = _AndroidCheatingType;
            this.MonitorFailedToRetrieveData = _MonitorFailedToRetrieveData;
        }
    }
}
