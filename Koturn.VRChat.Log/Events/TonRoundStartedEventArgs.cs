using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonRoundStarted"/> event.
    /// </summary>
    public class TonRoundStartedEventArgs : LogEventArgs
    {
        /// <summary>
        /// Place name.
        /// </summary>
        public string PlaceName { get; }
        /// <summary>
        /// Place index.
        /// </summary>
        public int PlaceIndex { get; }
        /// <summary>
        /// Round name.
        /// </summary>
        public string RoundName { get; }

        /// <summary>
        /// Create instance with timestamp, place name and its idex and round name.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="placeName">Place name.</param>
        /// <param name="placeIndex">Place index.</param>
        /// <param name="roundName">Round name.</param>
        public TonRoundStartedEventArgs(DateTime logAt, string placeName, int placeIndex, string roundName)
            : base(logAt)
        {
            PlaceName = placeName;
            PlaceIndex = placeIndex;
            RoundName = roundName;
        }
    }
}
