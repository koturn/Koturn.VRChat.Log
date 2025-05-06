using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonKillerEnraged"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp, terror name and enrage level.
    /// </remarks>
    /// <param name="logFileName">Log file name.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="terrorName">Terror name.</param>
    /// <param name="enrageLevel">Enrage level.</param>
    public class TonKillerEnragedEventArgs(string? logFileName, DateTime logAt, string terrorName, int enrageLevel)
        : TonKillerNameEventArgs(logFileName, logAt, terrorName)
    {
        /// <summary>
        /// Enrage level.
        /// </summary>
        public int EnrageLevel { get; } = enrageLevel;
    }
}
