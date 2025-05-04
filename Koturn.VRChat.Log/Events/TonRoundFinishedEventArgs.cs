using System;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonPlayerDead"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp, place name and its idex and round name.
    /// </remarks>
    /// <param name="logFilePath">Log file path.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="result">Round result.</param>
    public class TonRoundFinishedEventArgs(string? logFilePath, DateTime logAt, TonRoundResult result)
        : VRCLogEventArgs(logFilePath, logAt)
    {
        /// <summary>
        /// Round result.
        /// </summary>
        public TonRoundResult Result { get; } = result;
    }
}
