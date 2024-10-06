using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Base class of log event.
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// Log timestamp.
        /// </summary>
        public DateTime LogAt { get; }

        /// <summary>
        /// Create instance with log timestamp.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        public LogEventArgs(DateTime logAt)
        {
            LogAt = logAt;
        }
    }
}
