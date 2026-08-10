namespace GUPS.AntiCheat.Detector.Desktop
{
    /// <summary>
    /// Default <see cref="IDesktopCheatingDetectionStatus"/> implementation carrying the false-positive likelihood,
    /// threat rating, detected desktop cheating type and a short human readable evidence string.
    /// </summary>
    public struct DesktopCheatingDetectionStatus : IDesktopCheatingDetectionStatus
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
        /// Gets the type of cheating detected on the desktop, or <see cref="EDesktopCheatingType.UNKNOWN"/> if no
        /// specific type could be classified.
        /// </summary>
        public EDesktopCheatingType DesktopCheatingType { get; private set; }

        /// <summary>
        /// Gets a short human readable evidence string describing what triggered the detection. May be empty.
        /// </summary>
        public string Evidence { get; private set; }

        /// <summary>
        /// Creates a new <see cref="DesktopCheatingDetectionStatus"/> with the given fields.
        /// </summary>
        /// <param name="_PossibilityOfFalsePositive">The false-positive likelihood in the range <c>[0.0, 1.0]</c>.</param>
        /// <param name="_ThreatRating">The threat rating reported with the detection.</param>
        /// <param name="_DesktopCheatingType">The type of cheating detected on the desktop.</param>
        /// <param name="_Evidence">A short human readable evidence string. May be <c>null</c> or empty.</param>
        public DesktopCheatingDetectionStatus(float _PossibilityOfFalsePositive, uint _ThreatRating, EDesktopCheatingType _DesktopCheatingType, string _Evidence)
        {
            this.PossibilityOfFalsePositive = _PossibilityOfFalsePositive;
            this.ThreatRating = _ThreatRating;
            this.DesktopCheatingType = _DesktopCheatingType;
            this.Evidence = _Evidence ?? string.Empty;
        }
    }
}
