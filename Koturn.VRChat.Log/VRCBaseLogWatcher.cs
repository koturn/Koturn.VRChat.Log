#if NET7_0_OR_GREATER
#    define SUPPORT_LIBRARY_IMPORT
#endif  // NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
#if !NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif  // !NET8_0_OR_GREATER
using System.IO;
using System.Runtime.InteropServices;
#if WINDOWS
using System.Security;
#endif  // WINDOWS
using System.Threading;
using Koturn.VRChat.Log.Events;

#if !NET9_0_OR_GREATER
using Lock = object;
#endif  // !NET9_0_OR_GREATER


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Log Watcher class.
    /// </summary>
#if SUPPORT_LIBRARY_IMPORT && WINDOWS
    public abstract partial class VRCBaseLogWatcher : IDisposable
#else
    public abstract class VRCBaseLogWatcher : IDisposable
#endif  // SUPPORT_LIBRARY_IMPORT && WINDOWS
    {
        /// <summary>
        /// Default watch cycle (milliseconds).
        /// </summary>
        internal const int InternalDefaultWatchCycle = 1000;
        /// <summary>
        /// Initial thread list capacity.
        /// </summary>
        private const int InitialThreadListCapacity = 4;
#if !WINDOWS
        /// <summary>
        /// A flag whether current running platform is Windows or not.
        /// </summary>
        private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif  // !WINDOWS

        /// <summary>
        /// Default watch cycle (milliseconds).
        /// </summary>
        public static int DefaultWatchCycle { get; } = InternalDefaultWatchCycle;

        /// <summary>
        /// File watch cycle.
        /// </summary>
        public int WatchCycle
        {
            get => _watchCycle;
            set
            {
#if NET8_0_OR_GREATER
                ArgumentOutOfRangeException.ThrowIfLessThan(value, Timeout.Infinite, nameof(WatchCycle));
#else
                ThrowIfLessThan(value, Timeout.Infinite, nameof(WatchCycle));
#endif  // NET8_0_OR_GREATER
                _watchCycle = value;
            }
        }
        /// <summary>
        /// true if watching thread started, otherwise false.
        /// </summary>
        public bool IsThreadStarted => _threadList.Count > 0;
        /// <summary>
        /// True if disposed, otherwise false.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Notify when new log file opended.
        /// </summary>
        public event VRCLogEventHandler<FileOpenEventArgs>? FileOpened;
        /// <summary>
        /// Notify when a log file closed.
        /// </summary>
        public event VRCLogEventHandler<FileCloseEventArgs>? FileClosed;

        /// <summary>
        /// <see cref="FileSystemWatcher"/> for VRChat log directory.
        /// </summary>
        private FileSystemWatcher? _watcher;
        /// <summary>
        /// <see cref="List{T}"/> of log file watching thread.
        /// </summary>
        private readonly List<Thread> _threadList = new(InitialThreadListCapacity);
        /// <summary>
        /// <see cref="Lock"/> object of <see cref="_threadList"/>.
        /// </summary>
        private readonly Lock _threadListLock = new();
        /// <summary>
        /// File watch cycle.
        /// </summary>
        private int _watchCycle;

        /// <summary>
        /// Create <see cref="VRCBaseLogWatcher"/> instance.
        /// </summary>
        public VRCBaseLogWatcher()
            : this(InternalDefaultWatchCycle)
        {
        }

        /// <summary>
        /// Create <see cref="VRCBaseLogWatcher"/> instance.
        /// </summary>
        /// <param name="watchCycle">File watch cycle.</param>
        public VRCBaseLogWatcher(int watchCycle)
        {
            WatchCycle = watchCycle;
        }

        /// <summary>
        /// Start watching log file on default VRChat log directory.
        /// </summary>
        public void Start()
        {
            Start(VRCBaseLogParser.DefaultVRChatLogDirectory);
        }

        /// <summary>
        /// Start watching log file on specified log directory.
        /// </summary>
        /// <param name="dirPath">VRChat log directory.</param>
        public void Start(string dirPath)
        {
            Stop();

            foreach (var filePath in GetWriteLockedLogFiles(dirPath))
            {
                // Lock is unneccessary here because any other thread stopped.
                _threadList.Add(StartFileWatchingThread(filePath, true));
            }

            var watcher = new FileSystemWatcher(dirPath, VRCBaseLogParser.InternalVRChatLogFileFilter)
            {
                NotifyFilter = NotifyFilters.FileName
            };
            watcher.Created += Watcher_Created;
            watcher.EnableRaisingEvents = true;

            _watcher = watcher;
        }

        /// <summary>
        /// Stop watching.
        /// </summary>
        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }

            lock (_threadListLock)
            {
                foreach (var thread in _threadList)
                {
                    thread.Interrupt();
                }
                foreach (var thread in _threadList)
                {
                    thread.Join(1000);
                }
                _threadList.Clear();
            }
        }

        /// <summary>
        /// Release all resources used by the <see cref="VRCLogWatcher"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Stop specified thread.
        /// </summary>
        /// <param name="thread">A <see cref="Thread"/> to stop.</param>
        protected void Stop(Thread thread)
        {
            bool hasThread;
            lock (_threadListLock)
            {
                hasThread = _threadList.Remove(thread);
            }
            if (hasThread)
            {
                thread.Interrupt();
            }
        }


        /// <summary>
        /// Create <see cref="VRCBaseLogParser"/> instance with specified file path.
        /// </summary>
        /// <param name="filePath">File path to parse.</param>
        /// <returns>Created <see cref="VRCBaseLogParser"/> instance.</returns>
        protected abstract VRCBaseLogParser CreateLogParser(string filePath);

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
                Stop();
            }
            IsDisposed = true;
        }

        /// <summary>
        /// Start flush log file thread.
        /// </summary>
        /// <param name="filePath">Target file path.</param>
        /// <param name="isParseToEnd">True to parse to end.</param>
        /// <returns>Started thread.</returns>
        private Thread StartFileWatchingThread(string filePath, bool isParseToEnd = false)
        {
            VRCBaseLogParser? logParser = null;
            try
            {
                logParser = CreateLogParser(filePath);
                if (isParseToEnd)
                {
                    logParser.Parse();
                }
                return StartFileWatchingThread(logParser);
            }
            catch
            {
                logParser?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Start flush log file thread.
        /// </summary>
        /// <returns>Started thread.</returns>
        private Thread StartFileWatchingThread(VRCBaseLogParser logParser)
        {
            var thread = new Thread(param =>
            {
                using (var logParser = (VRCBaseLogParser)param!)
                {
                    var fs = (FileStream)logParser.LogReader.BaseStream;
                    var filePath = fs.Name;
                    FileOpened?.Invoke(this, new FileOpenEventArgs(filePath));

                    try
                    {
                        var prevFileSize = fs.Position;
#if !WINDOWS
                        var fi = new FileInfo(filePath);
#endif
                        for (; ; )
                        {
                            Thread.Sleep(WatchCycle);
#if WINDOWS
                            var fileSize = (long)GetFileSize(filePath);
#else
                            fi.Refresh();
                            if (!fi.Exists)
                            {
                                continue;
                            }
                            var fileSize = fi.Length;
#endif  // WINDOWS
                            if (fileSize == prevFileSize)
                            {
                                continue;
                            }
                            logParser.Parse();
                            prevFileSize = fs.Position;
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        // Do nothing
                    }
                    finally
                    {
                        logParser.Dispose();
                        FileClosed?.Invoke(this, new FileCloseEventArgs(filePath, logParser.LogFrom, logParser.LogUntil));
                    }
                }
            })
            {
                IsBackground = true
            };
            thread.Start(logParser);
            return thread;
        }

        /// <summary>
        /// Get write locked-log file paths.
        /// </summary>
        /// <param name="logDirPath">Log file directory.</param>
        /// <returns>Latest log file path.</returns>
        private static List<string> GetWriteLockedLogFiles(string logDirPath)
        {
            var filePathList = new List<string>(InitialThreadListCapacity);
            foreach (var filePath in VRCBaseLogParser.GetLogFilePaths(logDirPath))
            {
                if (IsWriteLocked(filePath))
                {
                    filePathList.Add(filePath);
                }
            }

            return filePathList;
        }

        /// <summary>
        /// Determine whether specified file is write-locked or not.
        /// </summary>
        /// <param name="filePath">File path to determine.</param>
        /// <returns>true if specified file is write-locked, otherwise false.</returns>
        /// <remarks>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/io/handling-io-errors"/>
        /// </remarks>
        private static bool IsWriteLocked(string filePath)
        {
            try
            {
                // Try to open for write.
                // .NET: Buffer size must be greater than or equal to 0.
                // .NET Standard: Buffer size must be greater than 0; 0 is not allowed.
                new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read, 1).Dispose();
            }
            catch (IOException ex)
            {
                // Assume that VRChat process owns the log file.
#if WINDOWS
                if ((ex.HResult & 0x0000ffff) == 0x00000020)
#else
                if (!_isWindows || (ex.HResult & 0x0000ffff) == 0x00000020)
#endif  // WINDOWS
                {
                    return true;
                }
            }

            return false;
        }

#if !NET8_0_OR_GREATER
        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as greater than or equal to <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static void ThrowIfLessThan<T>(T value, T other, string? paramName)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) < 0)
            {
                ThrowLess(value, other, paramName);
            }
        }

        /// <summary>
        /// Throw <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <typeparam name="T">The type of the objects.</typeparam>
        /// <param name="value">The value of the argument that causes this exception.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [DoesNotReturn]
        private static void ThrowLess<T>(T value, T other, string? paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, value, $"'{value}' must be greater than or equal to '{other}'.");
        }
#endif  // !NET8_0_OR_GREATER

#if WINDOWS
        /// <summary>
        /// Get specified file size.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>File size of <paramref name="filePath"/>.</returns>
        private static ulong GetFileSize(string filePath)
        {
            var result = SafeNativeMethods.GetFileAttributesEx(filePath, GetFileExInfoLevels.InfoStandard, out var fileAttrData);
            return result ? ((ulong)fileAttrData.FileSizeHigh << 32) | (ulong)fileAttrData.FileSizeLow : 0;
        }
#endif  // WINDOWS

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref='FileSystemWatcher.Path'/> is created.
        /// </summary>
        /// <param name="sender">The source of the event. (<see cref="_watcher"/>)</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Thread? thread = null;
            try
            {
                thread = StartFileWatchingThread(e.FullPath);
                lock (_threadListLock)
                {
                    _threadList.Add(thread);
                }
                thread = null;
            }
            catch (Exception)
            {
                thread?.Interrupt();
            }
        }

#if WINDOWS
        /// <summary>
        /// Defines values that are used with the <see cref="SafeNativeMethods.GetFileAttributesEx(string, GetFileExInfoLevels, out Win32FileAttributeData)"/>
        /// and <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winbase/nf-winbase-getfileattributestransacteda">GetFileAttributesTransacted</see> functions
        /// to specify the information level of the returned data.
        /// </summary>
        /// <remarks>
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ne-minwinbase-get_fileex_info_levels"/>
        /// </remarks>
        private enum GetFileExInfoLevels : int
        {
            /// <summary>
            /// The <see cref="SafeNativeMethods.GetFileAttributesEx(string, GetFileExInfoLevels, out Win32FileAttributeData)"/>
            /// or GetFileAttributesTransacted function retrieves a standard set of attribute information.
            /// The data is returned in a <see cref="Win32FileAttributeData"/> structure.
            /// </summary>
            InfoStandard = 0,
            /// <summary>
            /// One greater than the maximum value. Valid values for this enumeration will be less than this value.
            /// </summary>
            MaxInfoLevel = 1
        }

        /// <summary>
        /// <pare>Contains a 64-bit value representing the number of 100-nanosecond intervals since January 1, 1601 (UTC).</pare>
        /// <pare>This structure is same as <see cref="System.Runtime.InteropServices.ComTypes.FILETIME"/></pare>
        /// </summary>
        /// <remarks>
        /// <para><see href="https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-filetime"/></para>
        /// <para>To convert a FILETIME structure into a time that is easy to display to a user,
        /// use the <see href="https://learn.microsoft.com/en-us/windows/win32/api/timezoneapi/nf-timezoneapi-filetimetosystemtime">FileTimeToSystemTime</see> function.</para>
        /// <para>It is not recommended that you add and subtract values from the FILETIME structure to obtain relative times.
        /// Instead, you should copy the low- and high-order parts of the file time
        /// to a <see href="https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-ularge_integer-r1">ULARGE_INTEGER</see> structure,
        /// perform 64-bit arithmetic on the QuadPart member, and copy the LowPart and HighPart members
        /// into the <see cref="Win32FileTime"/> structure.</para>
        /// <para>Do not cast a pointer to a <see cref="Win32FileTime"/> structure
        /// to either a ULARGE_INTEGER* or __int64* value because it can cause alignment faults on 64-bit Windows.</para>
        /// <para>Not all file systems can record creation and last access time and not all file systems record them in the same manner.
        /// For example, on NT FAT, create time has a resolution of 10 milliseconds, write time has a resolution of 2 seconds, and access time has a resolution of 1 day (really, the access date).
        /// On NTFS, access time has a resolution of 1 hour.
        /// Therefore, the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/fileapi/nf-fileapi-getfiletime">GetFileTime</see> function
        /// may not return the same file time information set using
        /// the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/fileapi/nf-fileapi-setfiletime">SetFileTime</see> function.
        /// Furthermore, FAT records times on disk in local time.
        /// However, NTFS records times on disk in UTC.
        /// For more information, see <see href="https://learn.microsoft.com/en-us/windows/desktop/SysInfo/file-times">File Times</see>.</para>
        /// <para>A function using the FILETIME structure can allow for values outside of zero or positive values typically specified by the dwLowDateTime and dwHighDateTime members.
        /// For example, the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/fileapi/nf-fileapi-setfiletime">SetFileTime</see> function
        /// uses 0xFFFFFFFF to specify that a file's previous access time should be preserved.
        /// For more information, see the topic for the function you are calling.</para>
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct Win32FileTime
        {
            /// <summary>
            /// The low-order part of the file time.
            /// </summary>
            public uint LowDateTime { get; }
            /// <summary>
            /// The high-order part of the file time.
            /// </summary>
            public uint HighDateTime { get; }
        }


        /// <summary>
        /// Contains attribute information for a file or directory.
        /// The <see cref="SafeNativeMethods.GetFileAttributesEx(string, GetFileExInfoLevels, out Win32FileAttributeData)"/> function
        /// uses this structure.
        /// </summary>
        /// <remarks>
        /// <para><see href="https://learn.microsoft.com/en-us/windows/win32/api/fileapi/ns-fileapi-win32_file_attribute_data"/></para>
        /// <para><see href="https://learn.microsoft.com/en-us/windows/win32/sysinfo/file-times"/></para>
        /// <para>
        /// Not all file systems can record creation and last access time, and not all file systems record them in the same manner.
        /// For example, on the FAT file system, create time has a resolution of 10 milliseconds,
        /// write time has a resolution of 2 seconds, and access time has a resolution of 1 day.
        /// On the NTFS file system, access time has a resolution of 1 hour.
        /// For more information, see File Times.
        /// </para>
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct Win32FileAttributeData
        {
            /// <summary>
            /// <para>The file system attribute information for a file or directory.</para>
            /// <para>For possible values and their descriptions, see <see cref="FileAttributes"/>.</para>
            /// </summary>
            public FileAttributes FileAttributes { get; }
            /// <summary>
            /// <para>A <see cref="Win32FileTime"/> structure that specifies when the file or directory is created.</para>
            /// <para>If the underlying file system does not support creation time, this member is zero.</para>
            /// </summary>
            public Win32FileTime CreationTime { get; }
            /// <summary>
            /// <para>A <see cref="Win32FileTime"/> structure.</para>
            /// <para>For a file, the structure specifies when the file is last read from or written to.</para>
            /// <para>For a directory, the structure specifies when the directory is created.</para>
            /// <para>For both files and directories, the specified date is correct, but the time of day is always set to midnight.
            /// If the underlying file system does not support last access time, this member is zero.</para>
            /// </summary>
            public Win32FileTime LastAccessTime { get; }
            /// <summary>
            /// <para>A <see cref="Win32FileTime"/> structure.</para>
            /// <para>For a file, the structure specifies when the file is last written to.</para>
            /// <para>For a directory, the structure specifies when the directory is created.</para>
            /// <para>If the underlying file system does not support last write time, this member is zero.</para>
            /// </summary>
            public Win32FileTime LastWriteTime { get; }
            /// <summary>
            /// <para>The high-order <see cref="uint"/> of the file size.</para>
            /// <para>This member does not have a meaning for directories.</para>
            /// </summary>
            public uint FileSizeHigh { get; }
            /// <summary>
            /// <para>The low-order <see cref="uint"/> of the file size.</para>
            /// <para>This member does not have a meaning for directories.</para>
            /// </summary>
            public uint FileSizeLow { get; }
        }

        /// <summary>
        /// Provides native methods.
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
#if SUPPORT_LIBRARY_IMPORT
        private static partial class SafeNativeMethods
#else
        private static class SafeNativeMethods
#endif  // SUPPORT_LIBRARY_IMPORT
        {
            /// <summary>
            /// <para>Retrieves attributes for a specified file or directory.</para>
            /// <para>To perform this operation as a transacted operation,
            /// use the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winbase/nf-winbase-getfileattributestransacteda">GetFileAttributesTransacted</see> function.</para>
            /// </summary>
            /// <param name="fileName">
            /// <para>The name of the file or directory.</para>
            /// <para>By default, the name is limited to MAX_PATH characters.
            /// To extend this limit to 32,767 wide characters, prepend "\\?\" to the path.
            /// For more information, see <see href="https://learn.microsoft.com/en-us/windows/win32/fileio/naming-a-file">Naming Files, Paths, and Namespaces.</see></para>
            /// </param>
            /// <param name="infoLevelId">
            /// <para>A class of attribute information to retrieve.</para>
            /// <para>This parameter can be <see cref="GetFileExInfoLevels.InfoStandard"/> enumeration.</para>
            /// </param>
            /// <param name="fileAttrData">
            /// <para>An output buffer that receives the attribute information.</para>
            /// <para>The type of attribute information that is stored into this buffer is determined by the value of <paramref name="infoLevelId"/>.</para>
            /// </param>
            /// <returns>
            /// <para>If the function succeeds, the return value is a true.</para>
            /// <para>If the function fails, the return value is false.
            /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</para>
            /// </returns>
            /// <remarks>
            /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getfileattributesexw"/>
            /// </remarks>
#if SUPPORT_LIBRARY_IMPORT
            [LibraryImport("kernel32.dll", EntryPoint = nameof(GetFileAttributesEx) + "W", StringMarshalling = StringMarshalling.Utf16)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool GetFileAttributesEx(string fileName, GetFileExInfoLevels infoLevelId, out Win32FileAttributeData fileAttrData);
#else
            [DllImport("kernel32.dll", EntryPoint = nameof(GetFileAttributesEx) + "W", ExactSpelling = true, CharSet = CharSet.Unicode)]
            public static extern bool GetFileAttributesEx([In] string fileName, GetFileExInfoLevels infoLevelId, out Win32FileAttributeData fileAttrData);
#endif  // SUPPORT_LIBRARY_IMPORT
        }
#endif  // WINDOWS
    }
}
