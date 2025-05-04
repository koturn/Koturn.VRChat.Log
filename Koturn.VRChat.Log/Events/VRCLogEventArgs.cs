using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Base class of log event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with log timestamp.
    /// </remarks>
    /// <param name="logFilePath">Log file path.</param>
    /// <param name="logAt">Log timestamp.</param>
    public class VRCLogEventArgs(string? logFilePath, DateTime logAt)
        : EventArgs
    {
        /// <summary>
        /// Log file path.
        /// </summary>
        public string? LogFilePath { get; } = logFilePath;
        /// <summary>
        /// Log timestamp.
        /// </summary>
        public DateTime LogAt { get; } = logAt;
    }
}
