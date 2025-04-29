using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Base class of log event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with log timestamp.
    /// </remarks>
    /// <param name="logAt">Log timestamp.</param>
    public class VRCLogEventArgs(DateTime logAt)
        : EventArgs
    {
        /// <summary>
        /// Log timestamp.
        /// </summary>
        public DateTime LogAt { get; } = logAt;
    }
}
