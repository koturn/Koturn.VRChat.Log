using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonPlayerDamaged"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp and damage point.
    /// </remarks>
    /// <param name="logFilePath">Log file path.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="damage">Damage point.</param>
    public class TonPlayerDamagedEventArgs(string? logFilePath, DateTime logAt, int damage)
        : VRCLogEventArgs(logFilePath, logAt)
    {
        /// <summary>
        /// Damage point.
        /// </summary>
        public int Damage { get; } = damage;
    }
}
