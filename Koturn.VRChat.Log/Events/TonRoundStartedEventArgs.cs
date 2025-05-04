using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonRoundStarted"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp, place name and its idex and round name.
    /// </remarks>
    /// <param name="logFilePath">Log file path.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="placeName">Place name.</param>
    /// <param name="placeIndex">Place index.</param>
    /// <param name="roundName">Round name.</param>
    public class TonRoundStartedEventArgs(string? logFilePath, DateTime logAt, string placeName, int placeIndex, string roundName)
        : VRCLogEventArgs(logFilePath, logAt)
    {
        /// <summary>
        /// Place name.
        /// </summary>
        public string PlaceName { get; } = placeName;
        /// <summary>
        /// Place index.
        /// </summary>
        public int PlaceIndex { get; } = placeIndex;
        /// <summary>
        /// Round name.
        /// </summary>
        public string RoundName { get; } = roundName;
    }
}
