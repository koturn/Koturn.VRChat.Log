using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonPlayerDead"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp, player name and message.
    /// </remarks>
    /// <param name="logFileName">Log file name.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="playerName">Player name.</param>
    /// <param name="message">Message.</param>
    public class TonPlayerDeadEventArgs(string? logFileName, DateTime logAt, string playerName, string message)
        : VRCLogEventArgs(logFileName, logAt)
    {
        /// <summary>
        /// Player name.
        /// </summary>
        public string PlayerName { get; } = playerName;
        /// <summary>
        /// Message.
        /// </summary>
        public string Message { get; } = message;
    }
}
