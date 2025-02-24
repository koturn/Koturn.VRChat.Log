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
        public const string VRChatLogFileFilter = "output_log_????-??-??_??-??-??.txt";
        /// <summary>
        /// <para>Default capacity of list of message lines.</para>
        /// <para>As of 2025-02-24, up to 74 lines are output in the initialization log,
        /// so 74 or more is recommended for a capacity of the list.</para>
        /// </summary>
        public const int DefaultListCapacity = 128;
        /// <summary>
        /// Char code of whitespace.
        /// </summary>
        private const byte CodeSp = (byte)' ';

        /// <summary>
        /// Default VRChat log directory (<c>%LOCALAPPDATA%Low\VRChat\VRChat</c>).
        /// </summary>
        public static string DefaultVRChatLogDirectory { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
            "VRChat",
            "VRChat");
        /// <summary>
        /// Byte sequence of " -  ".
        /// </summary>
        private static readonly byte[] LogSeparatorSequence = new[]
        {
            CodeSp, (byte)'-', CodeSp, CodeSp
        };
        /// <summary>
        /// Byte sequence of "Debug     ".
        /// </summary>
        private static readonly byte[] LogDebugSequence = new[]
        {
            (byte)'D', (byte)'e', (byte)'b', (byte)'u', (byte)'g', CodeSp, CodeSp, CodeSp, CodeSp, CodeSp
        };
        /// <summary>
        /// Byte sequence of "Warning   ".
        /// </summary>
        private static readonly byte[] LogWarningSequence = new[]
        {
            (byte)'W', (byte)'a', (byte)'r', (byte)'n', (byte)'i', (byte)'n', (byte)'g', CodeSp, CodeSp, CodeSp
        };
        /// <summary>
        /// Byte sequence of "Error     ".
        /// </summary>
        private static readonly byte[] LogErrorSequence = new[]
        {
            (byte)'E', (byte)'r', (byte)'r', (byte)'o', (byte)'r', CodeSp, CodeSp, CodeSp, CodeSp, CodeSp
        };
        /// <summary>
        /// Byte sequence of "Exception ".
        /// </summary>
        private static readonly byte[] LogExceptionSequence = new[]
        {
            (byte)'E', (byte)'x', (byte)'c', (byte)'e', (byte)'p', (byte)'t', (byte)'i', (byte)'o', (byte)'n', CodeSp
        };


        /// <summary>
        /// Input <see cref="Stream"/>.
        /// </summary>
        public Stream BaseStream { get; }
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
        /// Read buffer.
        /// </summary>
        private byte[] _buffer;
        /// <summary>
        /// Read byte offset.
        /// </summary>
        private int _offset = 0;
        /// <summary>
        /// Parsed byte offset.
        /// </summary>
        private int _offsetParsed = 0;
        /// <summary>
        /// First log line message.
        /// </summary>
        private string? _firstLineMessage = null;
        /// <summary>
        /// Log lines.
        /// </summary>
        private readonly List<string> _messages = new List<string>(DefaultListCapacity);
        /// <summary>
        /// Log timestamp.
        /// </summary>
        private DateTime _logDateTime;
        /// <summary>
        /// Log level.
        /// </summary>
        private LogLevel _logLevel;
        /// <summary>
        /// true to leave the <see cref="Reader"/> open after the <see cref="VRCBaseLogParser"/> object is disposed; otherwise, false.
        /// </summary>
        private readonly bool _leaveOpen;

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
        {
            BaseStream = stream;
            _buffer = new byte[bufferSize];
            _leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Read and parse to end of the log.
        /// </summary>
        public void Parse()
        {
            var messages = _messages;
            while (ReadLog(messages, out var logDateTime, out var logLevel))
            {
                OnLogDetected(logDateTime, logLevel, messages);
            }
        }

        /// <summary>
        /// Get one log item.
        /// </summary>
        /// <param name="logDateTime">Log timestamp.</param>
        /// <param name="logLevel">Log level.</param>
        /// <returns>Log messages (null if no log item is detected.</returns>
        public List<string>? ReadLog(out DateTime logDateTime, out LogLevel logLevel)
        {
            var messages = new List<string>();
            return ReadLog(messages, out logDateTime, out logLevel) ? messages : null;
        }

        /// <summary>
        /// Get one log item.
        /// </summary>
        /// <param name="messages">Log lines of one log item.</param>
        /// <param name="logDateTime">Log timestamp.</param>
        /// <param name="logLevel">Log level.</param>
        /// <returns>True if one log item detected, otherwise false.</returns>
        public bool ReadLog(List<string> messages, out DateTime logDateTime, out LogLevel logLevel)
        {
            messages.Clear();
            if (_firstLineMessage != null)
            {
                messages.Add(_firstLineMessage);
            }
            logDateTime = _logDateTime;
            logLevel = _logLevel;

            var buffer = _buffer;
            var offset = _offset;
            var offsetParsed = _offsetParsed;
            var isContinue = true;
            var isReadRequired = offset == offsetParsed;

            do
            {
                // New data required.
                if (isReadRequired)
                {
                    isReadRequired = false;

                    // offset reached to the end of buffer.
                    if (offset == buffer.Length)
                    {
                        offset = 0;
                        offsetParsed = 0;
                    }

                    var readCount = BaseStream.Read(buffer, offset, buffer.Length - offset);
                    if (readCount == 0)
                    {
                        // EOF detected.
                        offset = 0;
                        offsetParsed = 0;
                        isContinue = _firstLineMessage == null;
                        _firstLineMessage = null;
                        break;
                    }
                    offset += readCount;
                }

                // Find end of line.
                var idx = Array.IndexOf(buffer, (byte)'\n', offsetParsed, offset - offsetParsed);
                if (idx == -1)
                {
                    //
                    // Extend buffer or move data.
                    //
                    isReadRequired = true;
                    if (offsetParsed > (buffer.Length >> 1))
                    {
                        // Move data from the back of the buffer to the front.
                        offset -= offsetParsed;
                        Buffer.BlockCopy(buffer, offsetParsed, buffer, 0, offset);
                        offsetParsed = 0;
                    }
                    else if (offset == buffer.Length)
                    {
                        // Extend buffer.
                        offset -= offsetParsed;
                        var newBuffer = new byte[buffer.Length << 1];
                        Buffer.BlockCopy(buffer, offsetParsed, newBuffer, 0, offset);
                        buffer = newBuffer;
                        offsetParsed = 0;
                    }
                    isReadRequired = true;
                }
                else
                {
                    //
                    // Try parse log.
                    //
                    LineCount++;
                    var count = idx - offsetParsed;
                    // Remove CR if exists.
                    if (count > 0 && buffer[idx - 1] == (byte)'\r')
                    {
                        // Remove CR and emit created list.
                        count--;
                    }
                    unsafe
                    {
                        fixed (byte *pBuffer = &buffer[offsetParsed])
                        {
                            var firstLineMessage = ParseFirstLogLine(pBuffer, count, out var logDateTime2, out var logLevel2);
                            if (firstLineMessage == null)
                            {
                                // Current line is not first line of log because failed to detect timestamp, log level and message separator.
                                // So, add the entire line as a log message.
                                messages.Add(count > 0 ? Encoding.UTF8.GetString(buffer, offsetParsed, count) : string.Empty);
                            }
                            else
                            {
                                LogCount++;

                                // Timestamp, log level and message separator is detected.
                                // Need to emit previous log and save timestamp and log level for next read.
                                if (_firstLineMessage == null)
                                {
                                    // The first line of log file.
                                    messages.Add(firstLineMessage);
                                }
                                else
                                {
                                    isContinue = false;
                                }
                                _firstLineMessage = firstLineMessage;
                                logDateTime = _logDateTime;
                                _logDateTime = logDateTime2;
                                logLevel = _logLevel;
                                _logLevel = logLevel2;
                            }
                        }
                    }
                    offsetParsed = idx + 1;
                    isReadRequired = offsetParsed == offset;
                }
            } while (isContinue);

            _buffer = buffer;
            _offset = offset;
            _offsetParsed = offsetParsed;

            return !isContinue;
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
            return GetFilePath(BaseStream);
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
                    BaseStream.Dispose();
                }
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
            return new InvalidLogException(message, GetFilePath(), LineCount, LogCount + 1);
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
        private unsafe string? ParseFirstLogLine(byte *pBuffer, int count, out DateTime logDateTime, out LogLevel logLevel)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static unsafe int ParseIntSimple(byte* pBuffer, int count)
            {
                int val = 0;
                for (int i = 0; i < count; i++)
                {
                    var d = pBuffer[i] - (byte)'0';
                    if ((uint)d > 9)
                    {
                        return -1;
                    }
                    val = val * 10 + d;
                }
                return val;
            }

            logDateTime = default;
            logLevel = default;

            if (count < 34)
            {
                return null;
            }

            int year, month, day, hour, minute, second;
            if (pBuffer[4] != (byte)'.'
                || pBuffer[7] != (byte)'.'
                || pBuffer[10] != (byte)' '
                || pBuffer[13] != (byte)':'
                || pBuffer[16] != (byte)':'
                || pBuffer[19] != (byte)' '
                || !IsMatchSequence(&pBuffer[30], LogSeparatorSequence)
                || (year = ParseIntSimple(&pBuffer[0], 4)) == -1
                || (month = ParseIntSimple(&pBuffer[5], 2)) == -1
                || (day = ParseIntSimple(&pBuffer[8], 2)) == -1
                || (hour = ParseIntSimple(&pBuffer[11], 2)) == -1
                || (minute = ParseIntSimple(&pBuffer[14], 2)) == -1
                || (second = ParseIntSimple(&pBuffer[17], 2)) == -1)
            {
                return null;
            }
            logDateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

            var pLogLevel = &pBuffer[20];
            if (IsMatchSequence(pLogLevel, LogDebugSequence))
            {
                logLevel = LogLevel.Debug;
            }
            else if (IsMatchSequence(pLogLevel, LogWarningSequence))
            {
                logLevel = LogLevel.Warning;
            }
            else if (IsMatchSequence(pLogLevel, LogErrorSequence))
            {
                logLevel = LogLevel.Error;
            }
            else if (IsMatchSequence(pLogLevel, LogExceptionSequence))
            {
                logLevel = LogLevel.Exception;
            }
            else
            {
                ThrowInvalidLogException("Invalid log level detected: " + Encoding.UTF8.GetString(pLogLevel, LogDebugSequence.Length));
                return string.Empty;
            }

            return Encoding.UTF8.GetString(&pBuffer[34], count - 34);
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

        /// <summary>
        /// Determine whether the data at pointer has specified byte sequence.
        /// </summary>
        /// <param name="pBuffer">Pointer to <see cref="byte"/> buffer.</param>
        /// <param name="sequence">Byte sequence to comparison.</param>
        /// <returns>True if the data at pointer has specified byte sequence, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe bool IsMatchSequence(byte *pBuffer, byte[] sequence)
        {
            for (int i = 0; i < sequence.Length; i++)
            {
                if (pBuffer[i] != sequence[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
