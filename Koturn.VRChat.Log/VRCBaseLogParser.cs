using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Exceptions;


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
        /// Initialize regexes.
        /// </summary>
        static VRCBaseLogParser()
        {
            DefaultVRChatLogDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
                "VRChat",
                "VRChat");
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
        /// Log item counter.
        /// </summary>
        public ulong LogCount { get; private set; }
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
        /// true to leave the <see cref="Reader"/> open after the <see cref="VRCBaseLogParser"/> object is disposed; otherwise, false.
        /// </summary>
        private bool _leaveOpen;

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
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCBaseLogParser"/> object is disposed; otherwise, false.</param>
        public VRCBaseLogParser(Stream stream, int bufferSize = 65536, bool leaveOpen = false)
            : this(new StreamReader(stream, Encoding.UTF8, false, bufferSize, leaveOpen))
        {
        }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="reader"><see cref="TextReader"/> of VRChat log file.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="reader"/> open
        /// after the <see cref="VRCBaseLogParser"/> object is disposed; otherwise, false.</param>
        public VRCBaseLogParser(TextReader reader, bool leaveOpen = false)
        {
            Reader = reader;
            LineCount = 0;
            LogCount = 0;
            LogFrom = default;
            LogUntil = default;
            _lineStack = new List<string>(128);
            _emptyLineCount = 0;
            _leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Read and parse to end of the log.
        /// </summary>
        public void Parse()
        {
            var sr = Reader;

            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                LoadLine(line);
            }
        }

        /// <summary>
        /// Load one line of log file and parse it, and fire each event as needed.
        /// </summary>
        /// <param name="line">Log line.</param>
        public void LoadLine(string line)
        {
            LineCount++;

            var lineStack = _lineStack;
            var emptyLineCount = _emptyLineCount;
            if (line.Length > 0)
            {
                if (emptyLineCount == 1)
                {
                    lineStack.Add(string.Empty);
                }
                _emptyLineCount = 0;
                lineStack.Add(line);
                return;
            }

            emptyLineCount++;
            _emptyLineCount = emptyLineCount;
            if (emptyLineCount < 2 || lineStack.Count == 0)
            {
                return;
            }

            lineStack[0] = ParseFirstLogLine(lineStack[0], out var logDateTime, out var logLevel);
            if (LogFrom == default)
            {
                LogFrom = logDateTime;
            }
            LogUntil = logDateTime;

            LogCount++;
            OnLogDetected(logDateTime, logLevel, lineStack);

            lineStack.Clear();
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
                if (!_leaveOpen)
                {
                    Reader.Dispose();
                }
                _emptyLineCount = 0;
                LineCount = 0;
                LogCount = 0;
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
            return new InvalidLogException(message, GetFilePath(), LineCount - (ulong)_lineStack.Count - 3UL, LogCount);
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
        /// <param name="line">Entire first line of log item.</param>
        /// <param name="logDateTime">Timestamp of the log.</param>
        /// <param name="logLevel">Log level.</param>
        /// <returns>Log message of the first line of log item without timestamp and log level.</returns>
        private string ParseFirstLogLine(string line, out DateTime logDateTime, out LogLevel logLevel)
        {
            if (line.Length < 34)
            {
                ThrowInvalidLogException("Invalid log line detected: " + line);
            }

            // Avoid to call Substring() method to reduce number of memory allocations.
            unsafe
            {
                fixed (char *pcLineBase = line)
                {
                    logDateTime = new DateTime(
                        ParseIntSimple(&pcLineBase[0], 4),
                        ParseIntSimple(&pcLineBase[5], 2),
                        ParseIntSimple(&pcLineBase[8], 2),
                        ParseIntSimple(&pcLineBase[11], 2),
                        ParseIntSimple(&pcLineBase[14], 2),
                        ParseIntSimple(&pcLineBase[17], 2),
                        DateTimeKind.Utc);

                    if (pcLineBase[4] != '.'
                        || pcLineBase[7] != '.'
                        || pcLineBase[10] != ' '
                        || pcLineBase[13] != ':'
                        || pcLineBase[16] != ':'
                        || pcLineBase[19] != ' ')
                    {
                        ThrowInvalidLogException("Invalid log line detected: " + line);
                    }
                }
            }

            const int levelOffset = 20;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            const int levelLength = 11;
            var s = line.AsSpan(levelOffset, levelLength);
            if (s.SequenceEqual("Log        ".AsSpan()))
            {
                logLevel = LogLevel.Log;
            }
            else if (s.SequenceEqual("Warning    ".AsSpan()))
            {
                logLevel = LogLevel.Warning;
            }
            else if (s.SequenceEqual("Error      ".AsSpan()))
            {
                logLevel = LogLevel.Error;
            }
            else if (s.SequenceEqual("Exception  ".AsSpan()))
            {
                logLevel = LogLevel.Exception;
            }
#else
            if (IsSubstringAt("Log        ", line, levelOffset))
            {
                logLevel = LogLevel.Log;
            }
            else if (IsSubstringAt("Warning    ", line, levelOffset))
            {
                logLevel = LogLevel.Warning;
            }
            else if (IsSubstringAt("Error      ", line, levelOffset))
            {
                logLevel = LogLevel.Error;
            }
            else if (IsSubstringAt("Exception  ", line, levelOffset))
            {
                logLevel = LogLevel.Exception;
            }
#endif
            else
            {
                ThrowInvalidLogException("Invalid log level detected: " + line.Substring(20, 11));
                logLevel = LogLevel.Other;
                return string.Empty;
            }

            if (!IsSubstringAt("-  ", line, 31))
            {
                ThrowInvalidLogException("Invalid log line detected: " + line);
            }

            return line.Substring(34);
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


        /// <summary>
        /// Determine if one string is a substring at the specified position of the other string.
        /// </summary>
        /// <param name="part">One string to compare as substring.</param>
        /// <param name="s">The other string.</param>
        /// <param name="index">Index of <paramref name="s"/>.</param>
        /// <returns>true if <paramref name="part"/> is substring of <paramref name="s"/> at <paramref name="index"/>, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool IsSubstringAt(string part, string s, int index)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            return s.Length - index >= part.Length
                && s.AsSpan(index, part.Length).SequenceEqual(part.AsSpan());
#else
            return s.Length - index >= part.Length
                && s.IndexOf(part, index, part.Length, StringComparison.Ordinal) == index;
#endif
        }

        /// <summary>
        /// <para>Converts the string representation of a number to its 32-bit signed integer equivalent with very simple way.</para>
        /// <para>No boundary checks or overflow detection.</para>
        /// </summary>
        /// <param name="pcLine">Pointer to log line.</param>
        /// <param name="count">Number of characters.</param>
        /// <returns>A 32-bit signed integer equivalent to the number contained in <paramref name="pcLine"/>.</returns>
        /// <exception cref="FormatException">Thrown when non digit character is detected.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static unsafe int ParseIntSimple(char* pcLine, int count)
        {
            [DoesNotReturn]
            static void ThrowFormatException(char c)
            {
                throw new FormatException($"Non digit character detected: '{c}'");
            }

            int val = 0;
            for (int i = 0; i < count; i++)
            {
                var d = pcLine[i] - '0';
                if ((uint)d > 9)
                {
                    ThrowFormatException(pcLine[i]);
                }
                val = val * 10 + d;
            }
            return val;
        }
    }
}
