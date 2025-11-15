#if NET7_0_OR_GREATER
#    define SUPPORT_LIBRARY_IMPORT
#endif  // NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Diagnostics;

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
#if WINDOWS
using Microsoft.Win32.SafeHandles;
#endif  // WINDOWS

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
        /// <see cref="Dictionary{TKey, TValue}"/> of <see cref="Thread.ManagedThreadId"/> and instance of <see cref="ManualResetEvent"/>.
        /// </summary>
        private readonly Dictionary<int, ManualResetEvent> _threadResetEventDict = new();
        /// <summary>
        /// <see cref="Lock"/> object of <see cref="_threadList"/> and <see cref="_threadResetEventDict"/>.
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
                var mre = new ManualResetEvent(false);
                var thread = StartFileWatchingThread(filePath, mre, true);
                _threadList.Add(thread);
                _threadResetEventDict.Add(thread.ManagedThreadId, mre);
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
            Stop(Timeout.Infinite);
        }

        /// <summary>
        /// Stop watching.
        /// </summary>
        /// <param name="millisecondsTimeout">Join timeout for each thread in milliseconds.</param>
        public void Stop(int millisecondsTimeout)
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }

            Thread[] threads;
            ManualResetEvent[] mres;
            lock (_threadListLock)
            {
                if (_threadList.Count == 0)
                {
                    return;
                }
                threads = _threadList.ToArray();
                _threadList.Clear();

                mres = new ManualResetEvent[_threadResetEventDict.Count];
                _threadResetEventDict.Values.CopyTo(mres, 0);
                _threadResetEventDict.Clear();
            }

            foreach (var mre in mres)
            {
                mre.Set();
            }
            foreach (var thread in threads)
            {
                thread.Join(millisecondsTimeout);
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
        /// <param name="mre"><see cref="ManualResetEvent"/> instance to stop thread.</param>
        /// <param name="isParseToEnd">True to parse to end.</param>
        /// <returns>Started thread.</returns>
        private Thread StartFileWatchingThread(string filePath, ManualResetEvent mre, bool isParseToEnd = false)
        {
            VRCBaseLogParser? logParser = null;
            try
            {
                logParser = CreateLogParser(filePath);
                if (isParseToEnd)
                {
                    logParser.Parse();
                }
                return StartFileWatchingThread(logParser, mre);
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
        /// <param name="logParser"><see cref="VRCBaseLogParser"/> instance.</param>
        /// <param name="mre"><see cref="ManualResetEvent"/> instance to stop thread.</param>
        /// <returns>Started thread.</returns>
        private Thread StartFileWatchingThread(VRCBaseLogParser logParser, ManualResetEvent mre)
        {
            var thread = new Thread(param =>
            {
#if NET7_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(param);
#else
                if (param == null)
                {
                    throw new ArgumentNullException(nameof(param));
                }
#endif  // NET7_0_OR_GREATER
                var paramArray = (object[])param;
                using (var logParser = (VRCBaseLogParser)paramArray[0])
                {
                    var mre = (ManualResetEvent)paramArray[1];
                    var fs = (FileStream)logParser.LogReader.BaseStream;
                    var filePath = fs.Name;
                    FileOpened?.Invoke(this, new FileOpenEventArgs(filePath));

                    var sw = Stopwatch.StartNew();
                    try
                    {
                        var prevFileSize = fs.Position;
#if !WINDOWS
                        var fi = new FileInfo(filePath);
#endif
                        while (!mre.WaitOne(WatchCycle))
                        {
#if WINDOWS
                            var fileSize = (long)GetFileSize(filePath);
#else
                            fi.Refresh();
                            if (!fi.Exists)
                            {
                                break;
                            }
                            var fileSize = fi.Length;
#endif  // WINDOWS
                            if (fileSize == prevFileSize)
                            {
                                if (sw.ElapsedMilliseconds > 60 * 1000 && !IsWriteLocked(filePath))
                                {
                                    break;
                                }
                                continue;
                            }
                            sw.Restart();

                            logParser.Parse();
                            prevFileSize = fs.Position;
                        }
                    }
                    finally
                    {
                        logParser.Dispose();
                        FileClosed?.Invoke(this, new FileCloseEventArgs(filePath, logParser.LogFrom, logParser.LogUntil));

                        lock (_threadListLock)
                        {
                            mre.Dispose();
#if NET6_0_OR_GREATER
                            _threadResetEventDict.Remove(Environment.CurrentManagedThreadId);
#else
                            _threadResetEventDict.Remove(Thread.CurrentThread.ManagedThreadId);
#endif  // NET6_0_OR_GREATER
                            _threadList.Remove(Thread.CurrentThread);
                        }
                    }
                }
            })
            {
                IsBackground = true
            };
            thread.Start(new object[] {logParser, mre});
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
            const int ErrorSharingViolation = 0x00000020;
#if WINDOWS
            // Try to open for write.
            const nint invalidHandleValue = -1;
            using (var hFile = SafeNativeMethods.CreateFile(
                filePath,
                GenericAccessRights.Write,
                FileShare.Read,
                IntPtr.Zero,
                FileMode.Open))
            {
                return hFile.DangerousGetHandle() == invalidHandleValue && Marshal.GetLastWin32Error() == ErrorSharingViolation;
            }
#else
#if NETCOREAPP1_0_OR_GREATER
            // .NET: Buffer size must be greater than or equal to 0.
            const int bufferSize = 0;
#else
            // .NET Standard: Buffer size must be greater than 0; 0 is not allowed.
            const int bufferSize = 1;
#endif  // NETCOREAPP1_0_OR_GREATER
            try
            {
                // Try to open for write.
                new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read, bufferSize).Dispose();
            }
            catch (IOException ex)
            {
                // Assume that VRChat process owns the log file.
                if (!_isWindows || (ex.HResult & 0x0000ffff) == ErrorSharingViolation)
                {
                    return true;
                }
            }

            return false;
#endif  // WINDOWS
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
            ManualResetEvent? mre = null;
            Thread? thread = null;
            try
            {
                mre = new ManualResetEvent(false);
                thread = StartFileWatchingThread(e.FullPath, mre);
                lock (_threadListLock)
                {
                    _threadList.Add(thread);
                    _threadResetEventDict.Add(thread.ManagedThreadId, mre);
                }
                thread = null;
            }
            catch (Exception)
            {
                mre?.Dispose();
                thread?.Interrupt();
            }
        }

#if WINDOWS
        /// <summary>
        /// <para>Securable objects use an access mask format in which the four high-order bits specify generic access rights.
        /// Each type of securable object maps these bits to a set of its standard and object-specific access rights.
        /// For example, a Windows file object maps the <see cref="Read"/> bit to the <c>READ_CONTROL</c> and <c>SYNCHRONIZE</c> standard access rights
        /// and to the <c>FILE_READ_DATA</c>, <c>FILE_READ_EA</c>, and <c>FILE_READ_ATTRIBUTES</c> object-specific access rights.
        /// Other types of objects map the <see cref="Read"/> bit to whatever set of access rights is appropriate for that type of object.</para>
        /// <para>You can use generic access rights to specify the type of access you need when you are opening a handle to an object.
        /// This is typically simpler than specifying all the corresponding standard and specific rights.</para>
        /// </summary>
        [Flags]
        internal enum GenericAccessRights : uint
        {
            /// <summary>
            /// Possible access rights.
            /// </summary>
            All = 0x10000000,
            /// <summary>
            /// Execute access
            /// </summary>
            Execute = 0x20000000,
            /// <summary>
            /// Write access.
            /// </summary>
            Write = 0x40000000,
            /// <summary>
            /// Read access.
            /// </summary>
            Read = 0x80000000,
        }

        /// <summary>
        /// File attributes are metadata values stored by the file system on disk and are used
        /// by the system and are available to developers via various file I/O APIs. For a list of related APIs and topics,
        /// see the See also section.
        /// </summary>
        /// <remarks>
        /// <para><seealso href="https://learn.microsoft.com/en-us/windows/win32/fileio/file-attribute-constants"/></para>
        /// <para>
        /// See also
        /// <list type="bullet">
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/fileio/compression-attribute">Compression Attribute</see></item>
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/fileio/creating-and-opening-files">Creating and Opening Files</see></item>
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/api/FileAPI/nf-fileapi-createfilea">CreateFile</see></item>
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/api/WinBase/nf-winbase-createfiletransacteda">CreateFileTransacted</see></item>
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/api/FileAPI/nf-fileapi-getfileattributesa">GetFileAttributes</see></item>
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/api/FileAPI/nf-fileapi-getfileattributesexa">GetFileAttributesEx</see></item>
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/api/WinBase/nf-winbase-getfileattributestransacteda">GetFileAttributesTransacted</see></item>
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/api/FileAPI/nf-fileapi-getfileinformationbyhandle">GetFileInformationByHandle</see></item>
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/api/WinBase/nf-winbase-getfileinformationbyhandleex">GetFileInformationByHandleEx</see></item>
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/api/FileAPI/nf-fileapi-setfileattributesa">SetFileAttributes</see></item>
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/api/WinBase/nf-winbase-setfileattributestransacteda">SetFileAttributesTransacted</see></item>
        ///   <item><see href="https://learn.microsoft.com/en-us/windows/win32/api/FileAPI/nf-fileapi-setfileinformationbyhandle">SetFileInformationByHandle</see></item>
        /// </list>
        /// </para>
        /// </remarks>
        [Flags]
        internal enum FileFlagAndAttributes : uint
        {
            /// <summary>
            /// The file is read only.
            /// Applications can read the file, but cannot write to or delete it.
            /// </summary>
            AttrReadonly = 0x00000001,
            /// <summary>
            /// The file is hidden. Do not include it in an ordinary directory listing.
            /// </summary>
            AttrHidden = 0x00000002,
            /// <summary>
            /// The file is part of or used exclusively by an operating system.
            /// </summary>
            AttrSystem = 0x00000004,
            /// <summary>
            /// The file should be archived. Applications use this attribute to mark files for backup or removal.
            /// </summary>
            AttrArchive = 0x00000020,
            /// <summary>
            /// The file does not have other attributes set.
            /// This attribute is valid only if used alone.
            /// </summary>
            AttrNormal = 0x00000080,
            /// <summary>
            /// <para>The file is being used for temporary storage.</para>
            /// <para>For more information, see the <see href="https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew#caching_behavior">Caching Behavior</see> section of this topic.</para>
            /// </summary>
            AttrTemporary = 0x00000100,
            /// <summary>
            /// The data of a file is not immediately available.
            /// This attribute indicates that file data is physically moved to offline storage.
            /// This attribute is used by Remote Storage, the hierarchical storage management software.
            /// Applications should not arbitrarily change this attribute.
            /// </summary>
            AttrOffline = 0x00001000,
            /// <summary>
            /// <para>The file or directory is encrypted.
            /// For a file, this means that all data in the file is encrypted.
            /// For a directory, this means that encryption is the default for newly created files and subdirectories.
            /// For more information, see <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/file-encryption">File Encryption</see>.</para>
            /// <para>This flag has no effect if <see cref="AttrSystem"/> is also specified.</para>
            /// <para>This flag is not supported on Home, Home Premium, Starter, or ARM editions of Windows.</para>
            /// </summary>
            AttrEncrypted = 0x00004000,
            /// <summary>
            /// Impersonates a client at the Anonymous impersonation level.
            /// </summary>
            SecurityAnonymous = 0x00000000,
            /// <summary>
            /// Impersonates a client at the Identification impersonation level.
            /// </summary>
            SecurityIdentification = 0x00010000,
            /// <summary>
            /// Impersonate a client at the impersonation level.
            /// This is the default behavior if no other flags are specified along with the SECURITY_SQOS_PRESENT flag.
            /// </summary>
            SecurityImpersonation = 0x00020000,
            /// <summary>
            ///	Impersonates a client at the Delegation impersonation level.
            /// </summary>
            SecurityDelegation = 0x00030000,
            /// <summary>
            /// The security tracking mode is dynamic. If this flag is not specified, the security tracking mode is static.
            /// </summary>
            SecurityContextTracking = 0x00040000,
            /// <summary>
            /// <para>Only the enabled aspects of the client's security context are available to the server.
            /// If you do not specify this flag, all aspects of the client's security context are available.</para>
            /// <para>This allows the client to limit the groups and privileges that a server can use while impersonating the client.</para>
            /// </summary>
            SecurityEffectiveOnly = 0x00080000,
            /// <summary>
            /// The file data is requested, but it should continue to be located in remote storage.
            /// It should not be transported back to local storage.
            /// This flag is for use by remote storage systems.
            /// </summary>
            FlagOpenNoRecall = 0x00100000,
            /// <summary>
            /// <para>Normal <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/reparse-points">reparse point</see> processing will not occur; <see cref="SafeNativeMethods.CreateFile"/> will attempt to open the reparse point.
            /// When a file is opened, a file handle is returned, whether or not the filter that controls the reparse point is operational.</para>
            /// <para>This flag cannot be used with the <see cref="FileMode.Create"/> flag.</para>
            /// <para>If the file is not a reparse point, then this flag is ignored.</para>
            /// <para>For more information, see the Remarks section.</para>
            /// </summary>
            FlagOpenReparsePoint = 0x00200000,
            /// <summary>
            /// <para>The file or device is being opened with session awareness.
            /// If this flag is not specified, then per-session devices (such as a device using RemoteFX USB Redirection)
            /// cannot be opened by processes running in session 0.
            /// This flag has no effect for callers not in session 0.
            /// This flag is supported only on server editions of Windows.</para>
            /// <para>Windows Server 2008 R2 and Windows Server 2008:  This flag is not supported before Windows Server 2012.</para>
            /// </summary>
            FlagSessionAware = 0x00800000,
            /// <summary>
            /// Access will occur according to POSIX rules.
            /// This includes allowing multiple files with names, differing only in case, for file systems that support that naming.
            /// Use care when using this option, because files created with this flag may not be accessible
            /// by applications that are written for MS-DOS or 16-bit Windows.
            /// </summary>
            FlagPosixSemantics = 0x01000000,
            /// <summary>
            /// <para>The file is being opened or created for a backup or restore operation.
            /// The system ensures that the calling process overrides file security checks when the process has SE_BACKUP_NAME and SE_RESTORE_NAME privileges.
            /// For more information, see Changing Privileges in a Token.</para>
            /// <para>You must set this flag to obtain a handle to a directory.
            /// A directory handle can be passed to some functions instead of a file handle.
            /// For more information, see the Remarks section.</para>
            /// </summary>
            FlagBackupSemantics = 0x02000000,
            /// <summary>
            /// <para>The file is to be deleted immediately after all of its handles are closed, which includes the specified handle and any other open or duplicated handles.</para>
            /// <para>If there are existing open handles to a file, the call fails unless they were all opened with the <see cref="FileShare.Delete"/> share mode.</para>
            /// <para>Subsequent open requests for the file fail, unless the <see cref="FileShare.Delete"/> share mode is specified.</para>
            /// </summary>
            FlagDeleteOnClose = 0x04000000,
            /// <summary>
            /// <para>Access is intended to be sequential from beginning to end.
            /// The system can use this as a hint to optimize file caching.
            /// This flag should not be used if read-behind (that is, reverse scans) will be used.</para>
            /// <para>This flag has no effect if the file system does not support cached I/O and <see cref="FlagNoBuffering"/>.</para>
            /// <para>For more information, see the <see href="https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew#caching_behavior">Caching Behavior</see> section of this topic.</para>
            /// </summary>
            FlagSequentialScan = 0x08000000,
            /// <summary>
            /// <para>Access is intended to be random.
            /// The system can use this as a hint to optimize file caching.
            /// This flag has no effect if the file system does not support cached I/O and <see cref="FlagNoBuffering"/>.</para>
            /// <para>For more information, see the <see href="https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew#caching_behavior">Caching Behavior</see> section of this topic.</para>
            /// </summary>
            FlagRandomAccess = 0x10000000,
            /// <summary>
            /// <para>The file or device is being opened with no system caching for data reads and writes.
            /// This flag does not affect hard disk caching or memory mapped files.</para>
            /// <para>There are strict requirements for successfully working with files opened with <see cref="SafeNativeMethods.CreateFile"/> using the <see cref="FlagNoBuffering"/> flag,
            /// for details see <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/file-buffering">File Buffering</see>.</para>
            /// </summary>
            FlagNoBuffering = 0x20000000,
            /// <summary>
            /// <para>The file or device is being opened or created for asynchronous I/O.
            /// When subsequent I/O operations are completed on this handle, the event specified
            /// in the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/minwinbase/ns-minwinbase-overlapped">OVERLAPPED</see> structure will be set to the signaled state.</para>
            /// <para>If this flag is specified, the file can be used for simultaneous read and write operations.</para>
            /// <para>If this flag is not specified, then I/O operations are serialized, even if the calls to the read
            /// and write functions specify an <see href="https://learn.microsoft.com/en-us/windows/desktop/api/minwinbase/ns-minwinbase-overlapped">OVERLAPPED</see> structure.</para>
            /// <para>For information about considerations when using a file handle created with this flag,
            /// see the Synchronous and <see href="https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew#synchronous_and_asynchronous_i_o_handles">Asynchronous I/O Handles</see> section of this topic.</para>
            /// </summary>
            FlagOverlapped = 0x40000000,
            /// <summary>
            /// <para>Write operations will not go through any intermediate cache, they will go directly to disk.</para>
            /// <para>For additional information, see the <see href="https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew#caching_behavior">Caching Behavior</see> section of this topic.</para>
            /// </summary>
            FlagWriteThrough = 0x80000000
        }

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
            /// <para>Creates or opens a file or I/O device.
            /// The most commonly used I/O devices are as follows:
            /// file, file stream, directory, physical disk, volume, console buffer, tape drive, communications resource, mailslot, and pipe.
            /// The function returns a handle that can be used to access the file or device for various types of I/O
            /// depending on the file or device and the flags and attributes specified.</para>
            /// <para>To perform this operation as a transacted operation, which results in a handle
            /// that can be used for transacted I/O,
            /// use the <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winbase/nf-winbase-createfiletransactedw">CreateFileTransacted</see> function.</para>
            /// </summary>
            /// <param name="fileName">
            /// <para>The name of the file or device to be created or opened.
            /// You may use either forward slashes (/) or backslashes (\) in this name.</para>
            /// <para>For information on special device names,
            /// see <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/defining-an-ms-dos-device-name">Defining an MS-DOS Device Name</see>.</para>
            /// <para>To create a file stream, specify the name of the file, a colon, and then the name of the stream.
            /// For more information, see <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/file-streams">File Streams</see>.</para>
            /// <para>By default, the name is limited to MAX_PATH characters.
            /// To extend this limit to 32,767 wide characters, prepend "\\?\" to the path.
            /// For more information, see <see href="https://learn.microsoft.com/en-us/windows/win32/fileio/naming-a-file">Naming Files, Paths, and Namespaces</see>.</para>
            /// <para>Tip</para>
            /// <para>Starting with Windows 10, Version 1607, you can opt-in to remove the MAX_PATH limitation without prepending "\\?\".
            /// See the "Maximum Path Length Limitation" section of <see href="https://learn.microsoft.com/en-us/windows/win32/fileio/naming-a-file">Naming Files, Paths, and Namespaces</see> for details.</para>
            /// </param>
            /// <param name="desiredAccess">
            /// <para>The requested access to the file or device, which can be summarized as read, write, both or neither zero).</para>
            /// <para>The most commonly used values are <see cref="GenericAccessRights.Read"/>, <see cref="GenericAccessRights.Write"/>,
            /// or both (<c><see cref="GenericAccessRights.Read"/> | <see cref="GenericAccessRights.Write"/></c>).
            /// For more information, see
            /// <see href="https://learn.microsoft.com/en-us/windows/desktop/SecAuthZ/generic-access-rights">Generic Access Rights</see>,
            /// <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/file-security-and-access-rights">File Security and Access Rights</see>,
            /// <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/file-access-rights-constants">File Access Rights Constants</see>,
            /// and <see href="https://learn.microsoft.com/en-us/windows/desktop/SecAuthZ/access-mask">ACCESS_MASK</see>.</para>
            /// <para>If this parameter is zero, the application can query certain metadata such as file, directory, or device
            /// attributes without accessing that file or device, even if <see cref="GenericAccessRights.Read"/> access would
            /// have been denied.</para>
            /// <para>You cannot request an access mode that conflicts with the sharing mode that is specified by the
            /// <paramref name="shareMode"></paramref> parameter in an open request that already has an open handle.</para>
            /// <para>For more information, see the Remarks section of this topic and
            /// <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/creating-and-opening-files">Creating and Opening Files</see>.</para>
            /// </param>
            /// <param name="shareMode">
            /// <para>The requested sharing mode of the file or device, which can be read, write, both, delete, all of these, or
            /// none (refer to the following table). Access requests to attributes or extended attributes are not affected by this flag.</para>
            /// <para>If this parameter is zero and <see cref="CreateFile"/> succeeds,
            /// the file or device cannot be shared and cannot be opened again until the handle to the file or device is closed.
            /// For more information, see the Remarks section.</para>
            /// <para>You cannot request a sharing mode that conflicts with the access mode that is specified in an existing
            /// request that has an open handle. <see cref="CreateFile"/> would fail and
            /// the <see cref="Marshal.GetLastWin32Error"/> function would return <c>ERROR_SHARING_VIOLATION</c>.</para>
            /// <para>To enable a process to share a file or device while another process has the file or device open,
            /// use a compatible combination of one or more of the <see cref="FileShare"/> values.
            /// For more information about valid combinations of this parameter with the <paramref name="desiredAccess"/> parameter,
            /// see <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/creating-and-opening-files">Creating and Opening Files</see>.</para>
            /// </param>
            /// <param name="pSecurityAttributes">
            /// <para>A pointer to a <see href="https://learn.microsoft.com/en-us/windows/win32/api/wtypesbase/ns-wtypesbase-security_attributes">SECURITY_ATTRIBUTES</see>
            /// structure that contains two separate but related data members: an optional security descriptor,
            /// and a Boolean value that determines whether the returned handle can be inherited by child processes.</para>
            /// <para>This parameter can be <see cref="IntPtr.Zero"/>.</para>
            /// <para>If this parameter is <see cref="IntPtr.Zero"/>, the handle returned by <see cref="CreateFile"/> cannot be inherited
            /// by any child processes the application may create and the file or device associated with the returned handle gets a default security descriptor.</para>
            /// <para>The <c>lpSecurityDescriptor</c> member of the structure specifies a
            /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/winnt/ns-winnt-security_descriptor">SECURITY_DESCRIPTOR</see> for a file or device.
            /// If this member is <see cref="IntPtr.Zero"/>, the file or device associated with the returned handle is assigned a default security descriptor.</para>
            /// <para><see cref="CreateFile"/> ignores the <c>lpSecurityDescriptor</c> member when opening an existing file or device,
            /// but continues to use the <c>bInheritHandle</c> member.</para>
            /// <para>The <c>bInheritHandle</c> member of the structure specifies whether the returned handle can be inherited.</para>
            /// <para>For more information, see the Remarks section.</para>
            /// </param>
            /// <param name="creationDisposition">
            /// <para>An action to take on a file or device that exists or does not exist.</para>
            /// <para>For devices other than files, this parameter is usually set to <see cref="FileMode.Open"/>.</para>
            /// <para>For more information, see the Remarks section.</para>
            /// <para>This parameter must be one of the <see cref="FileMode"/> values, which cannot be combined.</para>
            /// </param>
            /// <param name="flagsAndAttributes">
            /// <para>The file or device attributes and flags, <see cref="FileFlagAndAttributes.AttrNormal"/> being the most common default value for files.</para>
            /// <para>This parameter can include any combination of the available file attributes (<see cref="FileFlagAndAttributes"/><c>.Attr*</c>).
            /// All other file attributes override <see cref="FileFlagAndAttributes.AttrNormal"/>.</para>
            /// <para>This parameter can also contain combinations of flags (<see cref="FileFlagAndAttributes"/><c>.Flag*</c>)
            /// for control of file or device caching behavior, access modes, and other special-purpose flags.
            /// These combine with any <see cref="FileFlagAndAttributes"/><c>.Flag*</c> values.</para>
            /// <para>This parameter can also contain Security Quality of Service (SQOS) information by specifying the <c>SECURITY_SQOS_PRESENT</c> flag.
            /// Additional SQOS-related flags information is presented in the table following the attributes and flags tables.</para>
            /// Note  When <see cref="CreateFile"/> opens an existing file, it generally combines the file flags with the file attributes of the existing file,
            /// and ignores any file attributes supplied as part of <paramref name="flagsAndAttributes"/>.
            /// Special cases are detailed in <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/creating-and-opening-files">Creating and Opening Files</see>.
            /// Some of the following file attributes and flags may only apply to files and not necessarily all other types of devices that <see cref="CreateFile"/> can open.
            /// For additional information, see the Remarks section of this topic and <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/creating-and-opening-files">Creating and Opening Files</see>.
            /// <para>For more advanced access to file attributes, see
            /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/fileapi/nf-fileapi-setfileattributesa">SetFileAttributes</see>.
            /// For a complete list of all file attributes with their values and descriptions,
            /// see <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/file-attribute-constants">File Attribute Constants</see>.</para>
            /// </param>
            /// <param name="hTemplateFile">
            /// <para>A valid handle to a template file with the <see cref="GenericAccessRights.Read"/> access right.
            /// The template file supplies file attributes and extended attributes for the file that is being created.</para>
            /// <para>This parameter can be <see cref="IntPtr.Zero"/>.</para>
            /// <para>When opening an existing file, <see cref="CreateFile"/> ignores this parameter.</para>
            /// <para>When opening a new encrypted file, the file inherits the discretionary access control list from its parent directory.
            /// For additional information, see <see href="https://learn.microsoft.com/en-us/windows/desktop/FileIO/file-encryption">File Encryption</see>.</para>
            /// </param>
            /// <returns>
            /// <para>If the function succeeds, the return value is an open handle to the specified file, device, named pipe, or mail slot.</para>
            /// <para>If the function fails, the return value is <c>INVALID_HANDLE_VALUE</c> (-1).
            /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</para>
            /// </returns>
            /// <remarks>
            /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew"/>
            /// </remarks>
#if SUPPORT_LIBRARY_IMPORT
            [LibraryImport("kernel32.dll", EntryPoint = nameof(CreateFile) + "W", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
            public static partial SafeFileHandle CreateFile(
                string fileName,
                GenericAccessRights desiredAccess,
                FileShare shareMode,
                IntPtr pSecurityAttributes,
                FileMode creationDisposition,
                FileFlagAndAttributes flagsAndAttributes = FileFlagAndAttributes.AttrNormal,
                IntPtr hTemplateFile = default);
#else
            [DllImport("kernel32.dll", EntryPoint = nameof(CreateFile) + "W", ExactSpelling = true, CharSet = CharSet.Unicode)]
            public static extern SafeFileHandle CreateFile(
                string fileName,
                uint dwDesiredAccess,
                uint dwShareMode,
                IntPtr securityAttributes,
                uint dwCreationDisposition,
                uint dwFlagsAndAttributes,
                IntPtr hTemplateFile);
#endif  // SUPPORT_LIBRARY_IMPORT

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
