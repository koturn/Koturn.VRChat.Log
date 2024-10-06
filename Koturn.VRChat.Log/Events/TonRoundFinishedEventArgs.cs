using System;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonPlayerDead"/> event.
    /// </summary>
    public class TonRoundFinishedEventArgs : LogEventArgs
    {
        /// <summary>
        /// Round result.
        /// </summary>
        public TonRoundResult Result { get; }

        /// <summary>
        /// Create instance with timestamp, place name and its idex and round name.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="result">Round result.</param>
        public TonRoundFinishedEventArgs(DateTime logAt, TonRoundResult result)
            : base(logAt)
        {
            Result = result;
        }
    }
}
