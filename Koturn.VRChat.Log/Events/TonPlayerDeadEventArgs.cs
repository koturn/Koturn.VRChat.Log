using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonPlayerDead"/> event.
    /// </summary>
    public class TonPlayerDeadEventArgs : LogEventArgs
    {
        /// <summary>
        /// Player name.
        /// </summary>
        public string PlayerName { get; }
        /// <summary>
        /// Message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Create instance with timestamp, player name and message.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="playerName">Player name.</param>
        /// <param name="message">Message.</param>
        public TonPlayerDeadEventArgs(DateTime logAt, string playerName, string message)
            : base(logAt)
        {
            PlayerName = playerName;
            Message = message;
        }
    }
}
