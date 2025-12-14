#if NET7_0_OR_GREATER
#    define SUPPORT_LIBRARY_IMPORT
#endif  // NET7_0_OR_GREATER

using System;
#if !NET5_0_OR_GREATER
using System.Diagnostics;
#endif  // !NET5_0_OR_GREATER
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;
#if !WINDOWS
using Mono.Unix;
#endif  // !WINDOWS


namespace Koturn.VRChat.Log.Internals
{
#if SUPPORT_LIBRARY_IMPORT
    internal static partial class FileHelper
#else
    internal static class FileHelper
#endif  // SUPPORT_LIBRARY_IMPORT && WINDOWS
    {
#if !WINDOWS
        /// <summary>
        /// A flag whether current running platform is Windows or not.
        /// </summary>
        private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif  // !WINDOWS

        /// <summary>
        /// Determine whether specified file is write-locked or not.
        /// </summary>
        /// <param name="filePath">File path to determine.</param>
        /// <returns>true if specified file is write-locked, otherwise false.</returns>
        public static bool IsWriteLocked(string filePath)
        {
#if WINDOWS
            return IsWriteLockedWindows(filePath);
#else
            if (_isWindows)
            {
                return IsWriteLockedWindows(filePath);
            }
            else
            {
                return IsWriteLockedOther(filePath);
            }
#endif  // WINDOWS
        }

        /// <summary>
        /// Determine whether specified file is write-locked or not by Windows specific method.
        /// </summary>
        /// <param name="filePath">File path to determine.</param>
        /// <returns>true if specified file is write-locked, otherwise false.</returns>
        /// <remarks>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/io/handling-io-errors"/>
        /// </remarks>
        private static bool IsWriteLockedWindows(string filePath)
        {
            const int ErrorSharingViolation = 0x00000020;
            const nint invalidHandleValue = -1;

            // Try to open for write.
            using (var hFile = SafeNativeMethods.CreateFile(
                filePath,
                GenericAccessRights.Write,
                FileShare.Read,
                IntPtr.Zero,
                FileMode.Open))
            {
                return hFile.DangerousGetHandle() == invalidHandleValue && Marshal.GetLastWin32Error() == ErrorSharingViolation;
            }
        }

#if !WINDOWS
        /// <summary>
        /// Determine whether specified file is write-locked or not by general method.
        /// </summary>
        /// <param name="filePath">File path to determine.</param>
        /// <returns>true if specified file is write-locked, otherwise false.</returns>
        /// <remarks>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/io/handling-io-errors"/>
        /// </remarks>
        private static bool IsWriteLockedOther(string filePath)
        {
            var targetInode = new UnixFileInfo(filePath).Inode;
#if NET5_0_OR_GREATER
            var currentPid = Environment.ProcessId;
#else
            var currentPid = Process.GetCurrentProcess().Id;
#endif  // NET5_0_OR_GREATER
            foreach (var procDir in Directory.EnumerateDirectories("/proc"))
            {
                if (!int.TryParse(Path.GetFileName(procDir), out var pid))
                {
                    continue;
                }

                if (pid == currentPid)
                {
                    continue;
                }

                var fdDir = Path.Combine(procDir, "fd");
                if (!Directory.Exists(fdDir))
                {
                    continue;
                }

                try
                {
                    foreach (var fd in Directory.EnumerateFiles(fdDir))
                    {
                        try
                        {
                            var link = new UnixSymbolicLinkInfo(fd);
                            if (!link.HasContents)
                            {
                                continue;
                            }

                            var stat = new UnixFileInfo(link.ContentsPath);
                            if (stat.Inode == targetInode)
                            {
                                return true;
                            }
                        }
                        catch (Exception)
                        {
                            // Do nothing.
                        }
                    }
                }
                catch (Exception)
                {
                    // Do nothing.
                }
            }

            return false;
        }
#endif  // !WINDOWS

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
                GenericAccessRights desiredAccess,
                FileShare shareMode,
                IntPtr pSecurityAttributes,
                FileMode creationDisposition,
                FileFlagAndAttributes flagsAndAttributes = FileFlagAndAttributes.AttrNormal,
                IntPtr hTemplateFile = default);
#endif  // SUPPORT_LIBRARY_IMPORT
        }
    }
}
