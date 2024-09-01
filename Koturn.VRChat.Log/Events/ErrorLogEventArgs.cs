using System;
using System.Collections.Generic;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="VRCLogParser.WarningDetected"/>, <see cref="VRCLogParser.ErrorDetected"/>
    /// or <see cref="VRCLogParser.ExceptionDetected"/> event.
    /// </summary>
    public class ErrorLogEventArgs : LogEventArgs
    {
        /// <summary>
        /// Log level.
        /// </summary>
        public LogLevel Level { get; }
        /// <summary>
        /// Log lines.
        /// </summary>
        public List<string> Lines { get; }

        /// <summary>
        /// Create instance with log timestamp, log level and log lines.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="lines">Log lines.</param>
        public ErrorLogEventArgs(DateTime logAt, LogLevel level, List<string> lines)
            : base(logAt)
        {
            Level = level;
            Lines = lines;
        }
    }
}
