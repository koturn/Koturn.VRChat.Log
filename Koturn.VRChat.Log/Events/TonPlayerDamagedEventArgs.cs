using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonPlayerDamaged"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp and damage point.
    /// </remarks>
    /// <param name="logFileName">Log file name.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="damage">Damage point.</param>
    public class TonPlayerDamagedEventArgs(string? logFileName, DateTime logAt, int damage)
        : VRCLogEventArgs(logFileName, logAt)
    {
        /// <summary>
        /// Damage point.
        /// </summary>
        public int Damage { get; } = damage;
    }
}
