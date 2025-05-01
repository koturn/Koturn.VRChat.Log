using System;
using System.Runtime.Serialization;


namespace Koturn.VRChat.Log.Exceptions
{
    /// <summary>
    /// Represents errors that occur during parsing VRChat log.
    /// </summary>
    [Serializable]
    public sealed class InvalidLogException : Exception
    {
        /// <summary>
        /// Log file path.
        /// </summary>
        public string? FilePath { get; }
        /// <summary>
        /// Log line counter.
        /// </summary>
        public ulong LineCount { get; }
        /// <summary>
        /// Log item counter.
        /// </summary>
        public ulong LogCount { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLogException"/> class.
        /// </summary>
        /// <param name="filePath">Log file path.</param>
        /// <param name="lineCount">Log line counter.</param>
        /// <param name="logCount">Log item counter.</param>
        public InvalidLogException(string? filePath, ulong lineCount, ulong logCount)
            : this("An error occured", filePath, lineCount, logCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLogException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="filePath">Log file path.</param>
        /// <param name="lineCount">Log line counter.</param>
        /// <param name="logCount">Log item counter.</param>
        public InvalidLogException(string message, string? filePath, ulong lineCount, ulong logCount)
            : base($"{message} at {(filePath == null ? "" : filePath + ", ")}Line {lineCount.ToString()} (at log {logCount.ToString()})")
        {
            FilePath = filePath;
            LineCount = lineCount;
            LogCount = logCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLogException"/> class with
        /// a specified error message and a reference to the inner exception that
        /// is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="filePath">Log file path.</param>
        /// <param name="lineCount">Log line counter.</param>
        /// <param name="logCount">Log item counter.</param>
        /// <param name="inner">The exception that is the cause of the current exception.
        /// If the innerException parameter is not a null reference,
        /// the current exception is raised in a catch block that handles the inner exception.</param>
        public InvalidLogException(string message, string? filePath, ulong lineCount, ulong logCount, Exception inner)
            : base($"{message} at {(filePath == null ? "" : filePath + ", ")}Line {lineCount.ToString()} (at log {logCount.ToString()})", inner)
        {
            FilePath = filePath;
            LineCount = lineCount;
            LogCount = logCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLogException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
#if NET8_0_OR_GREATER
        [Obsolete("This ctor is only for .NET Framework", DiagnosticId = "SYSLIB0051")]
#endif  // NET8_0_OR_GREATER
        private InvalidLogException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
