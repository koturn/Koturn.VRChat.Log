using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// VRChat Log Reader.
    /// </summary>
    public class VRCLogReader : IDisposable
    {
        /// <summary>
        /// Default buffer size.
        /// </summary>
        internal const int InternalDefaultBufferSize = 65536;
        /// <summary>
        /// Char code of whitespace.
        /// </summary>
        private const byte CodeSp = (byte)' ';

        /// <summary>
        /// Default buffer size.
        /// </summary>
        public static int DefaultBufferSize { get; } = InternalDefaultBufferSize;

        /// <summary>
        /// Byte sequence of " -  ".
        /// </summary>
        private static readonly byte[] LogSeparatorSequence =
        [
            CodeSp, (byte)'-', CodeSp, CodeSp
        ];
        /// <summary>
        /// Byte sequence of "Debug     ".
        /// </summary>
        private static readonly byte[] LogDebugSequence =
        [
            (byte)'D', (byte)'e', (byte)'b', (byte)'u', (byte)'g', CodeSp, CodeSp, CodeSp, CodeSp, CodeSp
        ];
        /// <summary>
        /// Byte sequence of "Warning   ".
        /// </summary>
        private static readonly byte[] LogWarningSequence =
        [
            (byte)'W', (byte)'a', (byte)'r', (byte)'n', (byte)'i', (byte)'n', (byte)'g', CodeSp, CodeSp, CodeSp
        ];
        /// <summary>
        /// Byte sequence of "Error     ".
        /// </summary>
        private static readonly byte[] LogErrorSequence =
        [
            (byte)'E', (byte)'r', (byte)'r', (byte)'o', (byte)'r', CodeSp, CodeSp, CodeSp, CodeSp, CodeSp
        ];
        /// <summary>
        /// Byte sequence of "Exception ".
        /// </summary>
        private static readonly byte[] LogExceptionSequence =
        [
            (byte)'E', (byte)'x', (byte)'c', (byte)'e', (byte)'p', (byte)'t', (byte)'i', (byte)'o', (byte)'n', CodeSp
        ];


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
        /// Get underlying file path from <see cref="BaseStream"/>.
        /// </summary>
        public string? FilePath => GetFilePath(BaseStream);
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
        /// Log timestamp.
        /// </summary>
        private DateTime _logDateTime;
        /// <summary>
        /// Log level.
        /// </summary>
        private LogLevel _logLevel;
        /// <summary>
        /// true to leave the <see cref="BaseStream"/> open after the <see cref="VRCBaseLogParser"/> object is disposed; otherwise, false.
        /// </summary>
        private readonly bool _leaveOpen;


        /// <summary>
        /// Open a <see cref="FileStream"/> for the specified file path, then initialize instance with it.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        public VRCLogReader(string filePath)
            : this(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, InternalDefaultBufferSize, FileOptions.SequentialScan), InternalDefaultBufferSize)
        {
        }

        /// <summary>
        /// Open a <see cref="FileStream"/> for the specified file path and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for instance of this class and <see cref="FileStream"/>.</param>
        public VRCLogReader(string filePath, int bufferSize)
            : this(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, FileOptions.SequentialScan), bufferSize)
        {
        }

        /// <summary>
        /// Initialize instance with specified <see cref="FileStream"/> and default buffer size.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        public VRCLogReader(Stream stream)
            : this(stream, InternalDefaultBufferSize, false)
        {
        }

        /// <summary>
        /// Initialize instance with specified <see cref="FileStream"/> and buffer size.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="StreamReader"/>.</param>
        public VRCLogReader(Stream stream, int bufferSize)
            : this(stream, bufferSize, false)
        {
        }

        /// <summary>
        /// Initialize instance with specified <see cref="FileStream"/> and default buffer size.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCLogReader"/> object is disposed; otherwise, false.</param>
        public VRCLogReader(Stream stream, bool leaveOpen)
            : this(stream, InternalDefaultBufferSize, leaveOpen)
        {
        }

        /// <summary>
        /// Initialize instance with specified <see cref="FileStream"/>and buffer size.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="StreamReader"/>.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCLogReader"/> object is disposed; otherwise, false.</param>
        public VRCLogReader(Stream stream, int bufferSize, bool leaveOpen)
        {
            BaseStream = stream;
            _buffer = new byte[bufferSize];
            _leaveOpen = leaveOpen;
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
                        logDateTime = _logDateTime;
                        logLevel = _logLevel;
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
            }

            IsDisposed = true;
        }


        /// <summary>
        /// Parse one line of log.
        /// </summary>
        /// <param name="pBuffer">Pointer to read buffer.</param>
        /// <param name="count">Number of byte to parse from <paramref name="pBuffer"/>.</param>
        /// <param name="logDateTime">Timestamp of the log.</param>
        /// <param name="logLevel">Log level.</param>
        /// <returns>Log message of the first line of log item without timestamp and log level.</returns>
        private unsafe string? ParseFirstLogLine(byte *pBuffer, int count, out DateTime logDateTime, out LogLevel logLevel)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static unsafe bool TryParseIntSimple(byte* pBuffer, int count, out int val)
            {
                val = 0;
                for (int i = 0; i < count; i++)
                {
                    var d = pBuffer[i] - (byte)'0';
                    if ((uint)d > 9)
                    {
                        return false;
                    }
                    val = val * 10 + d;
                }
                return true;
            }

            logDateTime = default;
            logLevel = default;

            if (count < 34
                || pBuffer[4] != (byte)'.'
                || pBuffer[7] != (byte)'.'
                || pBuffer[10] != (byte)' '
                || pBuffer[13] != (byte)':'
                || pBuffer[16] != (byte)':'
                || pBuffer[19] != (byte)' '
                || !IsMatchSequence(&pBuffer[30], LogSeparatorSequence)
                || !TryParseIntSimple(&pBuffer[0], 4, out int year)
                || !TryParseIntSimple(&pBuffer[5], 2, out int month)
                || !TryParseIntSimple(&pBuffer[8], 2, out int day)
                || !TryParseIntSimple(&pBuffer[11], 2, out int hour)
                || !TryParseIntSimple(&pBuffer[14], 2, out int minute)
                || !TryParseIntSimple(&pBuffer[17], 2, out int second))
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
                logLevel = LogLevel.Other;
            }

            return Encoding.UTF8.GetString(&pBuffer[34], count - 34);
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
