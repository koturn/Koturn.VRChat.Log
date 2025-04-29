using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Exceptions;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// VRChat log file parser.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Initialize instance with specified <see cref="VRCLogReader"/>.
    /// </remarks>
    /// <param name="logReader">VRChat Log Reader.</param>
    /// <param name="leaveOpen">true to leave the <paramref name="logReader"/> open
    /// after the <see cref="VRCBaseLogParser"/> object is disposed; otherwise, false.</param>
    public abstract class VRCBaseLogParser(VRCLogReader logReader, bool leaveOpen) : IDisposable
    {
        /// <summary>
        /// VRChat log file name pattern.
        /// </summary>
        internal const string InternalVRChatLogFileFilter = "output_log_????-??-??_??-??-??.txt";
        /// <summary>
        /// Default list capacity.
        /// </summary>
        internal const int InternalDefaultListCapacity = 128;

        /// <summary>
        /// VRChat log file name pattern.
        /// </summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        public static string VRChatLogFileFilter { get; } = new string(InternalVRChatLogFileFilter.AsSpan());
#else
        public static string VRChatLogFileFilter { get; } = string.Copy(InternalVRChatLogFileFilter);
#endif  // NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Default buffer size.
        /// </summary>
        public static int DefaultBufferSize { get; } = VRCLogReader.InternalDefaultBufferSize;
        /// <summary>
        /// <para>Default capacity of list of message lines.</para>
        /// <para>As of 2025-02-24, up to 74 lines are output in the initialization log,
        /// so 74 or more is recommended for a capacity of the list.</para>
        /// </summary>
        public static int DefaultListCapacity { get; } = InternalDefaultListCapacity;
        /// <summary>
        /// Default VRChat log directory (<c>%LOCALAPPDATA%Low\VRChat\VRChat</c>).
        /// </summary>
        public static string DefaultVRChatLogDirectory { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
            "VRChat",
            "VRChat");


        /// <summary>
        /// VRChat log reader.
        /// </summary>
        public VRCLogReader LogReader { get; } = logReader;
        /// <summary>
        /// First timestamp of log file.
        /// </summary>
        public DateTime LogFrom { get; private set; }
        /// <summary>
        /// Last timestamp of log file.
        /// </summary>
        public DateTime LogUntil { get; private set; }
        /// <summary>
        /// Get underlying file path from specified <see cref="LogReader"/>.
        /// </summary>
        public string? FilePath => LogReader.FilePath;
        /// <summary>
        /// A flag property which indicates this instance is disposed or not.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Log lines.
        /// </summary>
        private readonly List<string> _messages = new(InternalDefaultListCapacity);
        /// <summary>
        /// true to leave the <see cref="LogReader"/> open after the <see cref="VRCBaseLogParser"/> object is disposed; otherwise, false.
        /// </summary>
        private readonly bool _leaveOpen = leaveOpen;


        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified file path, then initialize instance with it.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        public VRCBaseLogParser(string filePath)
            : this(new VRCLogReader(filePath), false)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified file path and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/> and internal <see cref="FileStream"/> of <see cref="VRCLogReader"/>.</param>
        public VRCBaseLogParser(string filePath, int bufferSize)
            : this(new VRCLogReader(filePath, bufferSize), false)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/>, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        public VRCBaseLogParser(Stream stream)
            : this(new VRCLogReader(stream), false)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/> and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/>.</param>
        public VRCBaseLogParser(Stream stream, int bufferSize)
            : this(new VRCLogReader(stream, bufferSize), false)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/>, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCBaseLogParser"/> object is disposed; otherwise, false.</param>
        public VRCBaseLogParser(Stream stream, bool leaveOpen)
            : this(new VRCLogReader(stream, leaveOpen), false)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/> and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/>.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCBaseLogParser"/> object is disposed; otherwise, false.</param>
        public VRCBaseLogParser(Stream stream, int bufferSize, bool leaveOpen)
            : this(new VRCLogReader(stream, bufferSize, leaveOpen), false)
        {
        }

        /// <summary>
        /// Initialize instance with specified <see cref="VRCLogReader"/>.
        /// </summary>
        /// <param name="logReader">VRChat Log Reader.</param>
        public VRCBaseLogParser(VRCLogReader logReader)
            : this(logReader, false)
        {
        }


        /// <summary>
        /// Read and parse to end of the log.
        /// </summary>
        public void Parse()
        {
            var logReader = LogReader;
            var messages = _messages;
            while (logReader.ReadLog(messages, out var logDateTime, out var logLevel))
            {
                if (LogFrom == default)
                {
                    LogFrom = logDateTime;
                }
                LogUntil = logDateTime;
                OnLogDetected(logDateTime, logLevel, messages);
            }
        }

        /// <summary>
        /// Release all resources used by the <see cref="VRCBaseLogParser"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
        protected abstract bool OnLogDetected(DateTime logAt, VRCLogLevel level, List<string> logLines);

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
                    LogReader.Dispose();
                }
            }

            IsDisposed = true;
        }

        /// <summary>
        /// Create <see cref="InvalidLogException"/>.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="lineOffset">Line offset for error position.</param>
        protected InvalidLogException CreateInvalidLogException(string message, long lineOffset = -1)
        {
            var logReader = LogReader;
            return new InvalidLogException(message, FilePath, logReader.LineCount + (ulong)lineOffset, logReader.LogCount);
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
        /// Throws <see cref="InvalidLogException"/>.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="lineOffset">Line offset for error position.</param>
        /// <exception cref="InvalidLogException">Always thrown.</exception>
        [DoesNotReturn]
        protected void ThrowInvalidLogException(string message, long lineOffset = -1)
        {
            throw CreateInvalidLogException(message, lineOffset);
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
            return Directory.GetFiles(logDirPath, InternalVRChatLogFileFilter);
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

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// <para>Converts the string representation of a number to its 32-bit signed integer equivalent with very simple way.</para>
        /// <para>No boundary checks or overflow detection.</para>
        /// </summary>
        /// <param name="lineSpan"><see cref="Span{T}"/> of log line.</param>
        /// <returns>A 32-bit signed integer equivalent to the number contained in <paramref name="lineSpan"/>.</returns>
        /// <exception cref="FormatException">Thrown when non digit character is detected.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static unsafe int ParseIntSimple(ReadOnlySpan<char> lineSpan)
        {
            [DoesNotReturn]
            static void ThrowFormatException(char c)
            {
                throw new FormatException($"Non digit character detected: '{c}'");
            }

            int val = 0;
            foreach (var c in lineSpan)
            {
                var d = c - '0';
                if ((uint)d > 9)
                {
                    ThrowFormatException(c);
                }
                val = val * 10 + d;
            }
            return val;
        }
#endif
    }
}
