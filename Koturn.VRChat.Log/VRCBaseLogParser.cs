using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Exceptions;
using Koturn.VRChat.Log.Internals;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// VRChat log file parser.
    /// </summary>
    public abstract class VRCBaseLogParser : IDisposable
    {
        /// <summary>
        /// Log file filter.
        /// </summary>
        public const string VRChatLogFileFilter = "output_log_*.txt";
        /// <summary>
        /// Default VRChat log directory (<c>%LOCALAPPDATA%Low\VRChat\VRChat</c>).
        /// </summary>
        public static string DefaultVRChatLogDirectory { get; }

        /// <summary>
        /// <para>Regex to detect log message with timestamp.</para>
        /// <para><c>^(\d{4})\.(\d{2})\.(\d{2}) (\d{2}):(\d{2}):(\d{2}) (\w+)\s+-  (.+)$</c></para>
        /// </summary>
        private static readonly Regex _regexLogLine;

        /// <summary>
        /// Initialize regexes.
        /// </summary>
        static VRCBaseLogParser()
        {
            DefaultVRChatLogDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
                "VRChat",
                "VRChat");
            _regexLogLine = RegexHelper.GetLogLineRegex();
        }

        /// <summary>
        /// <see cref="TextReader"/> for VRChat log file.
        /// </summary>
        public TextReader Reader { get; }
        /// <summary>
        /// Log line counter.
        /// </summary>
        public ulong LineCount { get; private set; }
        /// <summary>
        /// First timestamp of log file.
        /// </summary>
        public DateTime LogFrom { get; private set; }
        /// <summary>
        /// Last timestamp of log file.
        /// </summary>
        public DateTime LogUntil { get; private set; }
        /// <summary>
        /// A flag property which indicates this instance is disposed or not.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Log line stack.
        /// </summary>
        private readonly List<string> _lineStack;
        /// <summary>
        /// Empty line count.
        /// </summary>
        private int _emptyLineCount;

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for <see cref="FileStream"/> and <see cref="StreamReader"/>.</param>
        public VRCBaseLogParser(string filePath, int bufferSize = 65536)
            : this(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, FileOptions.SequentialScan), bufferSize)
        {
        }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="StreamReader"/>.</param>
        public VRCBaseLogParser(Stream stream, int bufferSize = 65536)
            : this(new StreamReader(stream, Encoding.UTF8, false, bufferSize, true))
        {
        }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="reader"><see cref="TextReader"/> of VRChat log file.</param>
        public VRCBaseLogParser(TextReader reader)
        {
            Reader = reader;
            LineCount = 0;
            LogFrom = default;
            LogUntil = default;
            _lineStack = new List<string>(128);
        }

        /// <summary>
        /// Read and parse to end of the log.
        /// </summary>
        public void Parse()
        {
            var sr = Reader;
            for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
            {
                LoadLine(line);
            }
        }

        /// <summary>
        /// Load one line of log file and parse it, and fire each event as needed.
        /// </summary>
        /// <param name="line">Log line.</param>
        /// <exception cref="InvalidDataException"></exception>
        public void LoadLine(string line)
        {
            LineCount++;
            if (line.Length > 0)
            {
                if (_emptyLineCount == 1)
                {
                    _lineStack.Add(string.Empty);
                }
                _emptyLineCount = 0;
                _lineStack.Add(line);
                return;
            }

            _emptyLineCount++;
            if (_emptyLineCount < 2 || _lineStack.Count == 0)
            {
                return;
            }

            var parsed = ParseFirstLogLine(_lineStack[0]);
            _lineStack[0] = parsed.Message;
            if (LogFrom == default)
            {
                LogFrom = parsed.DateTime;
            }
            LogUntil = parsed.DateTime;

            OnLogDetected(parsed.DateTime, parsed.Level, _lineStack);

            _lineStack.Clear();
        }

        /// <summary>
        /// Release all resources used by the <see cref="VRCBaseLogParser"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Process detected log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines (First line does not contain timestamp and level part, just message only).</param>
        /// <returns>True if any of log parsing succeeds, otherwise false.</returns>
        /// <remarks>
        /// Return values is not used in <see cref="VRCBaseLogParser"/>, just for inherited classes.
        /// </remarks>
        protected abstract bool OnLogDetected(DateTime logAt, LogLevel level, List<string> logLines);

        /// <summary>
        /// Get underlying file path from specified <see cref="Reader"/>.
        /// </summary>
        /// <returns>
        /// Obtained file path.
        /// Null if <see cref="Reader"/> is not <see cref="StreamReader"/> or <see cref="StreamReader.BaseStream"/> is not <see cref="FileStream"/>.
        /// </returns>
        protected string? GetFilePath()
        {
            return GetFilePath(Reader);
        }

        /// <summary>
        /// Release resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                Reader.Dispose();
                _emptyLineCount = 0;
                LineCount = 0;
                LogFrom = default;
                LogUntil = default;
            }

            IsDisposed = true;
        }

        /// <summary>
        /// Create <see cref="InvalidLogException"/>.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        protected InvalidLogException CreateInvalidLogException(string message)
        {
            return new InvalidLogException(message, GetFilePath(), LineCount - (ulong)_lineStack.Count - 3UL);
        }

        /// <summary>
        /// Throws <see cref="InvalidLogException"/>.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <exception cref="InvalidLogException">Always thrown.</exception>
        [DoesNotReturn]
        protected void ThrowInvalidLogException(string message)
        {
            throw CreateInvalidLogException(message);
        }


        /// <summary>
        /// Parse one line of log.
        /// </summary>
        /// <param name="line">One line of log.</param>
        /// <returns>Parsed result.</returns>
        private LogLine ParseFirstLogLine(string line)
        {
            var match = _regexLogLine.Match(line);
            var groups = match.Groups;
            if (!match.Success || groups.Count < 9)
            {
                ThrowInvalidLogException("Invalid log line: " + line);
            }

            var logLevel = groups[7].Value switch
            {
                "Log" => LogLevel.Log,
                "Warning" => LogLevel.Warning,
                "Error" => LogLevel.Error,
                "Exception" => LogLevel.Exception,
                _ => throw CreateInvalidLogException("Invalid log level: " + groups[7].Value)
            };

            return new LogLine(
                new DateTime(
                    int.Parse(groups[1].Value),
                    int.Parse(groups[2].Value),
                    int.Parse(groups[3].Value),
                    int.Parse(groups[4].Value),
                    int.Parse(groups[5].Value),
                    int.Parse(groups[6].Value),
                    DateTimeKind.Utc),
                logLevel,
                groups[8].Value);
        }


        /// <summary>
        /// Get all log file from <see cref="DefaultVRChatLogDirectory"/>.
        /// </summary>
        /// <returns>Array of log file paths.</returns>
        public static string[] GetLogFilePaths()
        {
            return GetLogFilePaths(DefaultVRChatLogDirectory);
        }

        /// <summary>
        /// Get all log file from <paramref name="logDirPath"/>.
        /// </summary>
        /// <param name="logDirPath">Log file directory.</param>
        /// <returns>Array of log file paths.</returns>
        public static string[] GetLogFilePaths(string logDirPath)
        {
            return Directory.GetFiles(logDirPath, VRChatLogFileFilter);
        }


        /// <summary>
        /// Get underlying file path from specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="TextReader"/> instance.</param>
        /// <returns>
        /// Obtained file path.
        /// Null if <paramref name="reader"/> is not <see cref="StreamReader"/> or <see cref="StreamReader.BaseStream"/> is not <see cref="FileStream"/>.
        /// </returns>
        protected static string? GetFilePath(TextReader reader)
        {
            return reader is StreamReader streamReader ? GetFilePath(streamReader.BaseStream) : null;
        }

        /// <summary>
        /// Get underlying file path from specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> instance.</param>
        /// <returns>
        /// Obtained file path.
        /// Null if <paramref name="stream"/> is not <see cref="FileStream"/>.
        /// </returns>
        protected static string? GetFilePath(Stream stream)
        {
            return (stream as FileStream)?.Name;
        }
    }
}
