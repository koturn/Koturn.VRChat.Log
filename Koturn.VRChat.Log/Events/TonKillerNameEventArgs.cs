using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonKillerStunned"/>
    /// and <see cref="IVRCExLogEvent.TonKillerTargetChanged"/> event.
    /// </summary>
    public class TonKillerNameEventArgs : LogEventArgs
    {
        /// <summary>
        /// Terror name.
        /// </summary>
        public string TerrorName { get; }

        /// <summary>
        /// Create instance with timestamp, terror name.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="terrorName">Terror name.</param>
        public TonKillerNameEventArgs(DateTime logAt, string terrorName)
            : base(logAt)
        {
            TerrorName = terrorName;
        }
    }
}
