using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Base class of log event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with log timestamp.
    /// </remarks>
    /// <param name="logFileName">Log file name.</param>
    /// <param name="logAt">Log timestamp.</param>
    public class VRCLogEventArgs(string? logFileName, DateTime logAt)
        : EventArgs
    {
        /// <summary>
        /// Log file path.
        /// </summary>
        public string? LogFileName { get; } = logFileName;
        /// <summary>
        /// Log timestamp.
        /// </summary>
        public DateTime LogAt { get; } = logAt;
    }
}
