#define USE_WIN32_API

using System;
using System.IO;
#if USE_WIN32_API
using System.Runtime.InteropServices;
using System.Security;
#endif
using System.Threading;
using Koturn.VRChat.Log.Events;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Log Watcher class.
    /// </summary>
    public abstract class VRCBaseLogWatcher : IDisposable
    {
        /// <summary>
        /// File watch cycle.
        /// </summary>
        public int WatchCycle { get; set; }
        /// <summary>
        /// Current watching log file path.
        /// </summary>
        public string? CurrentFilePath { get; protected set; }
        /// <summary>
        /// True if disposed, otherwise false.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Notify when new log file opended.
        /// </summary>
        public event EventHandler<FileOpenEventArgs>? FileOpened;
        /// <summary>
        /// Notify when a log file closed.
        /// </summary>
        public event EventHandler<FileCloseEventArgs>? FileClosed;

        /// <summary>
        /// <see cref="FileSystemWatcher"/> for VRChat log directory.
        /// </summary>
        private FileSystemWatcher? _watcher;
        /// <summary>
        /// Flush log file thread.
        /// </summary>
        private Thread? _thread;

        /// <summary>
        /// Create <see cref="VRCBaseLogWatcher"/> instance.
        /// </summary>
        public VRCBaseLogWatcher()
            : this(1000)
        {
        }

        /// <summary>
        /// Create <see cref="VRCBaseLogWatcher"/> instance.
        /// </summary>
        /// <param name="watchCycle">File watch cycle.</param>
        public VRCBaseLogWatcher(int watchCycle)
        {
            _watcher = null;
            _thread = null;
            WatchCycle = watchCycle;
            CurrentFilePath = null;
        }

        /// <summary>
        /// Start watching log file on default VRChat log directory.
        /// </summary>
        /// <param name="watchCycle">Watch cycle. (in milliseconds)</param>
        public void Start(int watchCycle = 1000)
        {
            Start(VRCBaseLogParser.DefaultVRChatLogDirectory, watchCycle);
        }

        /// <summary>
        /// Start watching log file on specified log directory.
        /// </summary>
        /// <param name="dirPath">VRChat log directory.</param>
        /// <param name="watchCycle">Watch cycle. (in milliseconds)</param>
        public void Start(string dirPath, int watchCycle = 1000)
        {
            WatchCycle = watchCycle;

            Stop();

            var filePath = GetLatestLogFile(dirPath);
            if (filePath != null)
            {
                StartFileWatchingThread(filePath, true);
            }
            var watcher = new FileSystemWatcher(dirPath, VRCBaseLogParser.VRChatLogFileFilter)
            {
                InternalBufferSize = 1024 * 64,
                NotifyFilter =
                    NotifyFilters.LastAccess
                    | NotifyFilters.LastWrite
                    | NotifyFilters.FileName
                    | NotifyFilters.Size
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
            var thread = _thread;
            if (thread != null)
            {
                StopThread(thread, 1000);
                _thread = null;
            }
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }
        }

        /// <summary>
        /// Release all resources used by the <see cref="VRCLogWatcher"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
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
                using (var logParser = (VRCBaseLogParser)param)
                {
                    var fs = (FileStream)((StreamReader)logParser.Reader).BaseStream;
                    var filePath = fs.Name;
                    CurrentFilePath = filePath;
                    FileOpened?.Invoke(this, new FileOpenEventArgs(filePath));

                    try
                    {
                        var prevFileSize = fs.Position;
#if !USE_WIN32_API
                        var fi = new FileInfo(filePath);
#endif
                        for (; ; )
                        {
                            Thread.Sleep(WatchCycle);
#if USE_WIN32_API
                            var fileSize = (long)GetFileSize(filePath);
#else
                            fi.Refresh();
                            if (!fi.Exists)
                            {
                                continue;
                            }
                            var fileSize = fi.Length;
#endif  // USE_WIN32_API
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
        /// Get latest log file path.
        /// </summary>
        /// <param name="logDirPath">Log file directory.</param>
        /// <returns>Latest log file path.</returns>
        private static string? GetLatestLogFile(string logDirPath)
        {
            var filePaths = VRCBaseLogParser.GetLogFilePaths(logDirPath);
            if (filePaths.Length == 0)
            {
                return null;
            }
            // Assume the array is in lexicographic order of file paths.
            return filePaths[filePaths.Length - 1];
        }

        /// <summary>
        /// Request to stop thread.
        /// </summary>
        /// <param name="thread">Target thread.</param>
        /// <param name="interruptWait">Wait time for interrupt. (in milliseconds; -1 means infinity)</param>
        /// <returns>True when interrupt or abort succseeded, otherwise false.</returns>
        private static bool StopThread(Thread thread, int interruptWait = -1)
        {
            thread.Interrupt();
            return thread.Join(interruptWait);
        }

#if USE_WIN32_API
        /// <summary>
        /// Get specified file size.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>File size of <paramref name="filePath"/>.</returns>
        private static ulong GetFileSize(string filePath)
        {
            var result = NativeMethods.GetFileAttributesEx(filePath, GetFileExInfoLevels.InfoStandard, out var fileAttrData);
            return result ? ((ulong)fileAttrData.FileSizeHigh << 32) | (ulong)fileAttrData.FileSizeLow : 0;
        }
#endif

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref='FileSystemWatcher.Path'/> is created.
        /// </summary>
        /// <param name="sender">The source of the event. (<see cref="_watcher"/>)</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> that contains the event data.</param>
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            var thread = _thread;
            if (thread != null)
            {
                StopThread(thread, 1000);
                _thread = null;
            }
            _thread = StartFileWatchingThread(e.FullPath);
        }

#if USE_WIN32_API
        /// <summary>
        /// Defines values that are used with the <see cref="NativeMethods.GetFileAttributesEx(string, GetFileExInfoLevels, out Win32FileAttributeData)"/>
        /// and <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winbase/nf-winbase-getfileattributestransacteda">GetFileAttributesTransacted</see> functions
        /// to specify the information level of the returned data.
        /// </summary>
        /// <remarks>
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ne-minwinbase-get_fileex_info_levels"/>
        /// </remarks>
        private enum GetFileExInfoLevels : int
        {
            /// <summary>
            /// The <see cref="NativeMethods.GetFileAttributesEx(string, GetFileExInfoLevels, out Win32FileAttributeData)"/>
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
        /// The <see cref="NativeMethods.GetFileAttributesEx(string, GetFileExInfoLevels, out Win32FileAttributeData)"/> function
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
        private static class NativeMethods
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
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool GetFileAttributesEx([In] string fileName, GetFileExInfoLevels infoLevelId, out Win32FileAttributeData fileAttrData);
        }
#endif
    }
}
