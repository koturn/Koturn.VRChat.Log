using System;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log.Internals
{
    /// <summary>
    /// Represents one log item.
    /// </summary>
    internal readonly struct LogLine
    {
        /// <summary>
        /// Timestamp of the log.
        /// </summary>
        public DateTime DateTime { get; }
        /// <summary>
        /// Level of the log.
        /// </summary>
        public LogLevel Level { get; }
        /// <summary>
        /// Log message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        public LogLine(DateTime dateTime, LogLevel level, string message)
        {
            DateTime = dateTime;
            Level = level;
            Message = message;
        }

        /// <summary>
        /// Get string representation of this instance.
        /// </summary>
        public override readonly string ToString()
        {
            return $@"[{DateTime:yyyy-MM-dd HH\:mm\:ss}][{Level,-9}] {Message}";
        }
    }
}
