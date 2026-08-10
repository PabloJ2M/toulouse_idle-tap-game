namespace GUPS.AntiCheat.Detector.IOS
{
    /// <summary>
    /// Default <see cref="IIOSJailbreakDetectionStatus"/> implementation carrying the false-positive likelihood,
    /// threat rating, detected jailbreak type and a short human readable evidence string.
    /// </summary>
    public struct IOSJailbreakDetectionStatus : IIOSJailbreakDetectionStatus
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
        /// Gets the type of jailbreak evidence detected on iOS, or <see cref="EIOSJailbreakType.UNKNOWN"/> if no
        /// specific type could be classified.
        /// </summary>
        public EIOSJailbreakType JailbreakType { get; private set; }

        /// <summary>
        /// Gets a short human readable evidence string describing what triggered the detection. May be empty.
        /// </summary>
        public string Evidence { get; private set; }

        /// <summary>
        /// Creates a new <see cref="IOSJailbreakDetectionStatus"/> with the given fields.
        /// </summary>
        /// <param name="_PossibilityOfFalsePositive">The false-positive likelihood in the range <c>[0.0, 1.0]</c>.</param>
        /// <param name="_ThreatRating">The threat rating reported with the detection.</param>
        /// <param name="_JailbreakType">The type of jailbreak evidence detected on iOS.</param>
        /// <param name="_Evidence">A short human readable evidence string. May be <c>null</c> or empty.</param>
        public IOSJailbreakDetectionStatus(float _PossibilityOfFalsePositive, uint _ThreatRating, EIOSJailbreakType _JailbreakType, string _Evidence)
        {
            this.PossibilityOfFalsePositive = _PossibilityOfFalsePositive;
            this.ThreatRating = _ThreatRating;
            this.JailbreakType = _JailbreakType;
            this.Evidence = _Evidence ?? string.Empty;
        }
    }
}
