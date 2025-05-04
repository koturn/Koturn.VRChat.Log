using System;
using System.Collections.Generic;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.WarningDetected"/>, <see cref="IVRCCoreLogEvent.ErrorDetected"/>
    /// or <see cref="IVRCCoreLogEvent.ExceptionDetected"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with log timestamp, log level and log lines.
    /// </remarks>
    /// <param name="logFilePath">Log file path.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="level">Log level.</param>
    /// <param name="lines">Log lines.</param>
    public class ErrorLogEventArgs(string? logFilePath, DateTime logAt, VRCLogLevel level, List<string> lines)
        : VRCLogEventArgs(logFilePath, logAt)
    {
        /// <summary>
        /// Log level.
        /// </summary>
        public VRCLogLevel Level { get; } = level;
        /// <summary>
        /// Log lines.
        /// </summary>
        public List<string> Lines { get; } = lines;
    }
}
