using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonPlayerDamaged"/> event.
    /// </summary>
    public class TonPlayerDamagedEventArgs : LogEventArgs
    {
        /// <summary>
        /// Damage point.
        /// </summary>
        public int Damage { get; }

        /// <summary>
        /// Create instance with timestamp and damage point.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="damage">Damage point.</param>
        public TonPlayerDamagedEventArgs(DateTime logAt, int damage)
            : base(logAt)
        {
            Damage = damage;
        }
    }
}
