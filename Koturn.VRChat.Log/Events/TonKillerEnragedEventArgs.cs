using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonKillerEnraged"/> event.
    /// </summary>
    public class TonKillerEnragedEventArgs : TonKillerNameEventArgs
    {
        /// <summary>
        /// Enrage level.
        /// </summary>
        public int EnrageLevel { get; }
        
        /// <summary>
        /// Create instance with timestamp, terror name and enrage level.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="terrorName">Terror name.</param>
        /// <param name="enrageLevel">Enrage level.</param>
        public TonKillerEnragedEventArgs(DateTime logAt, string terrorName, int enrageLevel)
            : base(logAt, terrorName)
        {
            EnrageLevel = enrageLevel;
        }
    }
}
