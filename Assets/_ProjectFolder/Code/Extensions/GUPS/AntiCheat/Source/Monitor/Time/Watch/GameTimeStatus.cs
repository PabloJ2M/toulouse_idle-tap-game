// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Monitor.Time
{
    /// <summary>
    /// Status payload produced by <see cref="GameTimeMonitor"/> carrying the detected game-time and physics-time deviations.
    /// </summary>
    public struct GameTimeStatus : IWatchedSubject
    {
        /// <summary>
        /// Gets the detected deviation between Unity's frame delta time and wall-clock time.
        /// </summary>
        public ETimeDeviation DeltaDeviation { get; private set; }

        /// <summary>
        /// Gets the detected deviation of the fixed (physics) delta time.
        /// </summary>
        public ETimeDeviation FixedDeltaDeviation { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTimeStatus"/> struct.
        /// </summary>
        /// <param name="_DeltaDeviation">The detected frame delta-time deviation.</param>
        /// <param name="_FixedDeltaDeviation">The detected fixed delta-time (physics) deviation.</param>
        public GameTimeStatus(ETimeDeviation _DeltaDeviation, ETimeDeviation _FixedDeltaDeviation)
        {
            this.DeltaDeviation = _DeltaDeviation;
            this.FixedDeltaDeviation = _FixedDeltaDeviation;
        }
    }
}
