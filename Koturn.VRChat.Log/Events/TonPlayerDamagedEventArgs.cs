using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonPlayerDamaged"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp and damage point.
    /// </remarks>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="damage">Damage point.</param>
    public class TonPlayerDamagedEventArgs(DateTime logAt, int damage)
        : LogEventArgs(logAt)
    {
        /// <summary>
        /// Damage point.
        /// </summary>
        public int Damage { get; } = damage;
    }
}
