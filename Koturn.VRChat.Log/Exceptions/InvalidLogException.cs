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
        /// Initializes a new instance of the <see cref="InvalidLogException"/> class.
        /// </summary>
        /// <param name="filePath">Log file path.</param>
        /// <param name="lineCount">Log line counter.</param>
        public InvalidLogException(string? filePath, ulong lineCount)
        {
            FilePath = filePath;
            LineCount = lineCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLogException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="filePath">Log file path.</param>
        /// <param name="lineCount">Log line counter.</param>
        public InvalidLogException(string message, string? filePath, ulong lineCount)
            : base($"{message} at {(filePath == null ? "" : filePath + ", ")}Line {lineCount}")
        {
            FilePath = filePath;
            LineCount = lineCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLogException"/> class with
        /// a specified error message and a reference to the inner exception that
        /// is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="filePath">Log file path.</param>
        /// <param name="lineCount">Log line counter.</param>
        /// <param name="inner">The exception that is the cause of the current exception.
        /// If the innerException parameter is not a null reference,
        /// the current exception is raised in a catch block that handles the inner exception.</param>
        public InvalidLogException(string message, string? filePath, ulong lineCount, Exception inner)
            : base($"{message} at {(filePath == null ? "" : filePath + ", ")}Line {lineCount}", inner)
        {
            FilePath = filePath;
            LineCount = lineCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLogException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        private InvalidLogException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            FilePath = string.Empty;
            LineCount = 0;
        }
    }
}
