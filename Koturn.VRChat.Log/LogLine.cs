using System;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Represents one log item.
    /// </summary>
    public struct LogLine
    {
        /// <summary>
        /// Logging timestamp format.
        /// </summary>
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Timestamp of the log.
        /// </summary>
        public DateTime DateTime;
        /// <summary>
        /// Level of the log.
        /// </summary>
        public LogLevel Level;
        /// <summary>
        /// Log message.
        /// </summary>
        public string Message;

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
        public override string ToString()
        {
            return $"[{DateTime.ToString(DateTimeFormat)}][{Level,-9}] {Message}";
        }
    }
}
