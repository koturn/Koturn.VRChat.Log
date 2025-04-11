using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonKillerStunned"/>
    /// and <see cref="IVRCExLogEvent.TonKillerTargetChanged"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp, terror name.
    /// </remarks>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="terrorName">Terror name.</param>
    public class TonKillerNameEventArgs(DateTime logAt, string terrorName)
        : LogEventArgs(logAt)
    {
        /// <summary>
        /// Terror name.
        /// </summary>
        public string TerrorName { get; } = terrorName;
    }
}
