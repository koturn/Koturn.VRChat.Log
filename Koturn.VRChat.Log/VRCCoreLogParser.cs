using System;
using System.Collections.Generic;
using System.IO;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Exceptions;
using Koturn.VRChat.Log.Internals;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// VRChat log file parser.
    /// </summary>
    public abstract class VRCCoreLogParser : VRCBaseLogParser
    {
        /// <summary>
        /// "[Behaviour]" log offset.
        /// </summary>
        private const int BehaviourLogOffset = 12;


        /// <summary>
        /// Authenticated user information.
        /// </summary>
        public AuthUserInfo? AuthUserInfo { get; private set; }
        /// <summary>
        /// A flag property which indicates this instance is disposed or not.
        /// </summary>
        public new bool IsDisposed { get; private set; }

        /// <summary>
        /// Dictionary to contain user name and join timestamp of the user.
        /// </summary>
        private readonly Dictionary<string, UserInfo> _userInfoDict = [];
        /// <summary>
        /// Instance information.
        /// </summary>
        private InstanceInfo _instanceInfo = new(default);


        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified file path, then initialize instance with it.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        public VRCCoreLogParser(string filePath)
            : base(filePath)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified file path and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/> and internal <see cref="FileStream"/> of <see cref="VRCLogReader"/>.</param>
        public VRCCoreLogParser(string filePath, int bufferSize)
            : base(filePath, bufferSize)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/>, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        public VRCCoreLogParser(Stream stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/> and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/>.</param>
        public VRCCoreLogParser(Stream stream, int bufferSize)
            : base(stream, bufferSize)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/>, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCCoreLogParser"/> object is disposed; otherwise, false.</param>
        public VRCCoreLogParser(Stream stream, bool leaveOpen)
            : base(stream, leaveOpen)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/> and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/>.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCCoreLogParser"/> object is disposed; otherwise, false.</param>
        public VRCCoreLogParser(Stream stream, int bufferSize, bool leaveOpen)
            : base(stream, bufferSize, leaveOpen)
        {
        }

        /// <summary>
        /// Initialize instance with specified <see cref="VRCLogReader"/>.
        /// </summary>
        /// <param name="logReader">VRChat Log Reader.</param>
        public VRCCoreLogParser(VRCLogReader logReader)
            : base(logReader)
        {
        }

        /// <summary>
        /// Initialize instance with specified <see cref="VRCLogReader"/>.
        /// </summary>
        /// <param name="logReader">VRChat Log Reader.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="logReader"/> open
        /// after the <see cref="VRCCoreLogParser"/> object is disposed; otherwise, false.</param>
        public VRCCoreLogParser(VRCLogReader logReader, bool leaveOpen)
            : base(logReader, leaveOpen)
        {
        }


        /// <summary>
        /// Load one line of log file and parse it, and fire each event as needed.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        /// <returns>True if any of the log parsing defined in this class succeeds, otherwise false.</returns>
        protected override bool OnLogDetected(VRCLogLevel level, List<string> logLines)
        {
            switch (level)
            {
                case VRCLogLevel.Warning:
                    OnWarningDetected(level, logLines);
                    return false;
                case VRCLogLevel.Error:
                    OnErrorDetected(level, logLines);
                    return false;
                case VRCLogLevel.Exception:
                    OnExceptionDetected(level, logLines);
                    return false;
                default:
                    break;
            }

            var firstLine = logLines[0];

            return ParseAsBehaviourLog(firstLine)
                || ParseAsScreenshotLog(firstLine)
                || ParseAsVideoPlaybackLog(firstLine)
                || ParseAsStringDownloadLog(firstLine)
                || ParseAsImageDownloadLog(firstLine)
                || ParseAsUserAuthenticatedLog(logLines)
                || ParseAsApplicationQuitLog(firstLine)
                || ParseAsInstanceResetNotificationLog(firstLine)
                || ParseAsInstanceClosedLog(firstLine);
        }

        /// <summary>
        /// This method is called when user authenticated log is detected.
        /// </summary>
        /// <param name="authUserInfo">Authenticated user information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsUserAuthenticatedLog(List{string})"/></para>
        /// </remarks>
        protected virtual void OnUserAuthenticated(AuthUserInfo authUserInfo)
        {
        }

        /// <summary>
        /// This method is called when application quit log is detected.
        /// </summary>
        /// <param name="activeTime">Active time (in seconds).</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsApplicationQuitLog(string)"/></para>
        /// </remarks>
        protected virtual void OnApplicationQuit(double activeTime)
        {
        }

        /// <summary>
        /// This method is called when instance reset notification log is detected.
        /// </summary>
        /// <param name="closeMinutes">Time until instance is reset (minutes).</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsInstanceResetNotificationLog(string)"/></para>
        /// </remarks>
        protected virtual void OnInstanceResetNotified(int closeMinutes)
        {
        }

        /// <summary>
        /// This method is called when instance closed log is detected.
        /// </summary>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsInstanceClosedLog(string)"/></para>
        /// </remarks>
        protected virtual void OnInstanceClosed(InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when instance closed by reset log is detected.
        /// </summary>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsInstanceClosedByResetLog(string)"/></para>
        /// </remarks>
        protected virtual void OnInstanceClosedByReset()
        {
        }

        /// <summary>
        /// This method is called when join log is detected.
        /// </summary>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsJoiningLog(string)"/></para>
        /// </remarks>
        protected virtual void OnJoinedToInstance(InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when leave log is detected.
        /// </summary>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsLeftLog(string)"/></para>
        /// </remarks>
        protected virtual void OnLeftFromInstance(InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when user join log is detected.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsUserJoinLeaveLog(string)"/></para>
        /// </remarks>
        protected virtual void OnUserJoined(string userName, string? userId, DateTime stayFrom, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when user leave log is detected.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="stayUntil">A timestamp the user left.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsUserJoinLeaveLog(string)"/></para>
        /// </remarks>
        protected virtual void OnUserLeft(string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when unregistering user log is detected.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="stayUntil">A timestamp the user left.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsUserUnregisteringLog(string)"/></para>
        /// </remarks>
        protected virtual void OnUserUnregistering(string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when pickup object log is detected.
        /// </summary>
        /// <param name="objectName">Pickedup object name.</param>
        /// <param name="isEquipped">True if equipped.</param>
        /// <param name="isEquippable">True if the object is equippable.</param>
        /// <param name="lastInputMethod">Last input method name.</param>
        /// <param name="isAutoEquipController">True if the object is auto equip controller.</param>
        protected virtual void OnPickupObject(string objectName, bool isEquipped, bool isEquippable, string lastInputMethod, bool isAutoEquipController)
        {
        }

        /// <summary>
        /// This method is called when drop object log is detected.
        /// </summary>
        /// <param name="objectName">Pickedup object name.</param>
        /// <param name="isEquipped">True if the object was equipped.</param>
        /// <param name="dropReason">Reason for dropping the object.</param>
        /// <param name="lastInputMethod">Last input method name.</param>
        protected virtual void OnDropObject(string objectName, bool isEquipped, string dropReason, string lastInputMethod)
        {
        }

        /// <summary>
        /// This method is called when screenshot log is detected.
        /// </summary>
        /// <param name="filePath">Screenshort file path.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsScreenshotLog(string)"/></para>
        /// </remarks>
        protected virtual void OnScreenshotTook(string filePath, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when video URL resolving log is detected.
        /// </summary>
        /// <param name="url">Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsVideoPlaybackLog(string)"/></para>
        /// </remarks>
        protected virtual void OnVideoUrlResolving(string url, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when video URL resolved log is detected.
        /// </summary>
        /// <param name="url">Video URL.</param>
        /// <param name="resolvedUrl">Resolved Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsVideoPlaybackLog(string)"/></para>
        /// </remarks>
        protected virtual void OnVideoUrlResolved(string url, string resolvedUrl, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when string/image download log is detected.
        /// </summary>
        /// <param name="url">Download URL.</param>
        /// <param name="type"></param>
        /// <param name="instanceInfo"></param>
        /// <remarks>
        /// <para>Called from following methods.</para>
        /// <para><see cref="ParseAsStringDownloadLog(string)"/></para>
        /// <para><see cref="ParseAsImageDownloadLog(string)"/></para>
        /// </remarks>
        protected virtual void OnDownloaded(string url, DownloadType type, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when warning log is detected.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="OnLogDetected(VRCLogLevel, List{string})"/></para>
        /// </remarks>
        protected virtual void OnWarningDetected(VRCLogLevel level, List<string> logLines)
        {
        }

        /// <summary>
        /// This method is called when error log is detected.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="OnLogDetected(VRCLogLevel, List{string})"/></para>
        /// </remarks>
        protected virtual void OnErrorDetected(VRCLogLevel level, List<string> logLines)
        {
        }

        /// <summary>
        /// This method is called when exception log is detected.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="OnLogDetected(VRCLogLevel, List{string})"/></para>
        /// </remarks>
        protected virtual void OnExceptionDetected(VRCLogLevel level, List<string> logLines)
        {
        }

        /// <summary>
        /// Release resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose(disposing);

            if (disposing)
            {
                ClearInfo();
            }

            IsDisposed = true;
        }


        /// <summary>
        /// Parse first log line as "[Behaviour]" log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        /// <exception cref="InvalidDataException">Thrown when duplicate joined timestamp.</exception>
        private bool ParseAsBehaviourLog(string firstLine)
        {
            if (!firstLine.StartsWith("[Behaviour] ", StringComparison.Ordinal))
            {
                return false;
            }

            return ParseAsUserJoinLeaveLog(firstLine)
                || ParseAsUserUnregisteringLog(firstLine)
                || ParseAsPickupObjectLog(firstLine)
                || ParseAsDropObjectLog(firstLine)
                || ParseAsJoiningLog(firstLine)
                || ParseAsLeftLog(firstLine)
                || ParseAsInstanceClosedByResetLog(firstLine);
        }


        /// <summary>
        /// Parse first log line as user joined or left log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        /// <exception cref="InvalidDataException">Thrown when duplicate joined timestamp.</exception>
        private bool ParseAsUserJoinLeaveLog(string firstLine)
        {
            static string ExtractUserNameAndId(string firstLine, int offset, out string? userId)
            {
                var userNameEndIndex = firstLine.Length - 43;

                if (userNameEndIndex > offset
                    && IsSubstringAt(" (usr_", firstLine, userNameEndIndex)
                    && firstLine[firstLine.Length - 1] == ')')
                {
                    userId = firstLine.Substring(userNameEndIndex + 2, 40);
                    return firstLine.Substring(offset, userNameEndIndex - offset);
                }
                else
                {
                    userId = null;
                    return firstLine.Substring(offset);
                }
            }

            if (!IsSubstringAt("OnPlayer", firstLine, BehaviourLogOffset))
            {
                return false;
            }

            const int JoinLeaveOffset = BehaviourLogOffset + 8;
            var userInfoDict = _userInfoDict;
            var logAt = LogUntil;

            if (IsSubstringAt("Joined ", firstLine, JoinLeaveOffset))
            {
                var userName = ExtractUserNameAndId(firstLine, 27, out var userId);

                OnUserJoined(userName, userId, logAt, _instanceInfo);
                if (userInfoDict.TryGetValue(userName, out var userInfo))
                {
                    ThrowInvalidLogException(
                        $@"User join log already exists; {userName} ({userInfo.UserId}) {userInfo.JoinAt:yyyy-MM-dd HH\:mm\:ss} ({logAt:yyyy-MM-dd HH\:mm\:ss}).");
                }
                userInfoDict.Add(userName, new UserInfo(userName, userId, logAt));
                return true;
            }

            if (IsSubstringAt("Left ", firstLine, JoinLeaveOffset))
            {
                var userName = ExtractUserNameAndId(firstLine, 25, out var userId);

                if (userInfoDict.TryGetValue(userName, out var userInfo))
                {
                    OnUserLeft(userName, userId, userInfo.JoinAt, logAt, _instanceInfo);
                    userInfoDict.Remove(userName);
                }
                else
                {
                    // Rare case.
                    // e.g.) Detect left user while you joining an instance.
                    OnUserLeft(userName, userId, logAt, logAt, _instanceInfo);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parse first log line as user unregistering log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsUserUnregisteringLog(string firstLine)
        {
            if (!IsSubstringAt("Unregistering ", firstLine, BehaviourLogOffset))
            {
                return false;
            }

            var userInfoDict = _userInfoDict;
            var userName = firstLine.Substring(BehaviourLogOffset + 14);
            if (userInfoDict.TryGetValue(userName, out var userInfo))
            {
                OnUserUnregistering(userName, userInfo.UserId, userInfo.JoinAt, LogUntil, _instanceInfo);
                userInfoDict.Remove(userName);
            }

            return true;
        }

        /// <summary>
        /// Parse first log line as pickup object log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsPickupObjectLog(string firstLine)
        {
            var match = RegexHelper.PickupObjectRegex.Match(firstLine, BehaviourLogOffset);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;
            OnPickupObject(
                groups[1].Value,
                bool.Parse(groups[2].Value),
                bool.Parse(groups[3].Value),
                groups[4].Value,
                bool.Parse(groups[5].Value));

            return true;
        }

        /// <summary>
        /// Parse first log line as drop object log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsDropObjectLog(string firstLine)
        {
            var match = RegexHelper.DropObjectRegex.Match(firstLine, BehaviourLogOffset);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;
            OnDropObject(
                groups[1].Value,
                bool.Parse(groups[2].Value),
                groups[3].Value,
                groups[4].Value);

            return true;
        }

        /// <summary>
        /// Parse first log line as joining to instance log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsJoiningLog(string firstLine)
        {
            if (!IsSubstringAt("Joining ", firstLine, BehaviourLogOffset))
            {
                return false;
            }

            const int instanceStringOffset = BehaviourLogOffset + 8;
            if (IsSubstringAt("or Creating Room: ", firstLine, instanceStringOffset))
            {
                _instanceInfo.WorldName = firstLine.Substring(BehaviourLogOffset + 26);
                OnJoinedToInstance(_instanceInfo);
                return true;
            }
            else if (IsSubstringAt("wrld_", firstLine, instanceStringOffset))
            {
                _instanceInfo = ParseInstanceString(firstLine.Substring(instanceStringOffset), LogUntil);
                return true;
            }
            else if (IsSubstringAt("local:", firstLine, instanceStringOffset))
            {
                _instanceInfo = ParseLocalInstanceString(firstLine.Substring(instanceStringOffset), LogUntil);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parse first log line as left from instance log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsLeftLog(string firstLine)
        {
            if (!IsSubstringAt("OnLeftRoom", firstLine, BehaviourLogOffset))
            {
                return false;
            }

            _instanceInfo.StayUntil = LogUntil;
            OnLeftFromInstance(_instanceInfo);
            _instanceInfo.IsEmitted = true;

            return true;
        }

        /// <summary>
        /// Parse first log line as instance closed by reset log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsInstanceClosedByResetLog(string firstLine)
        {
            // 2025.04.02 05:21:25 Debug      -  [Behaviour] Received executive message: Instance closed
            if (!IsSubstringAt("Received executive message: Instance closed", firstLine, BehaviourLogOffset))
            {
                return false;
            }

            OnInstanceClosedByReset();

            return true;
        }

        /// <summary>
        /// Parse first log line as video playback log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsVideoPlaybackLog(string firstLine)
        {
            if (!firstLine.StartsWith("[Video Playback] ", StringComparison.Ordinal))
            {
                return false;
            }

            // Almost same as regex pattern, /^\[Video Playback\] Resolving URL '(.+)'$/
            if (IsSubstringAt("Resolving URL '", firstLine, 17))
            {
                if (firstLine.IndexOf('\'', 32) != firstLine.Length - 1)
                {
                    return false;
                }

                OnVideoUrlResolving(firstLine.Substring(32, firstLine.Length - 33), _instanceInfo);

                return true;
            }

            // if (SubstringEquals(firstLine, 44, "Attempting to resolve URL '"))
            // {
            //     OnVideoUrlResolving(stayFrom, firstLine.Substring(44, firstLine.Length - 45), _instanceInfo);
            //     return true;
            // }

            // Almost same as regex pattern, /^\[Video Playback\] URL '(.+)' resolved to '(.+)'$/
            if (IsSubstringAt("URL '", firstLine, 17))
            {
                var idx = firstLine.IndexOf('\'', 22);
                if (idx == -1)
                {
                    return false;
                }

                var url = firstLine.Substring(22, idx - 22);

                idx++;
                if (!IsSubstringAt(" resolved to '", firstLine, idx))
                {
                    return false;
                }

                idx += 14;
                if (firstLine.IndexOf('\'', idx) != firstLine.Length - 1)
                {
                    return false;
                }

                var resolvedUrl = firstLine.Substring(idx, firstLine.Length - idx - 1);

                OnVideoUrlResolved(url, resolvedUrl, _instanceInfo);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Parse first log line as screenshot log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsScreenshotLog(string firstLine)
        {
            if (!firstLine.StartsWith("[VRC Camera] Took screenshot to: ", StringComparison.Ordinal))
            {
                return false;
            }

            OnScreenshotTook(firstLine.Substring(33), _instanceInfo);

            return true;
        }

        /// <summary>
        /// Parse first log line as string download log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsStringDownloadLog(string firstLine)
        {
            if (!firstLine.StartsWith("[String Download] Attempting to load String from URL '", StringComparison.Ordinal)
                || firstLine[firstLine.Length - 1] != '\'')
            {
                return false;
            }

            OnDownloaded(firstLine.Substring(54, firstLine.Length - 55), DownloadType.String, _instanceInfo);

            return true;
        }

        /// <summary>
        /// Parse first log line as image download log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsImageDownloadLog(string firstLine)
        {
            if (!firstLine.StartsWith("[Image Download] Attempting to load image from URL '", StringComparison.Ordinal)
                || firstLine[firstLine.Length - 1] != '\'')
            {
                return false;
            }

            OnDownloaded(firstLine.Substring(52, firstLine.Length - 53), DownloadType.Image, _instanceInfo);

            return true;
        }

        /// <summary>
        /// Parse first log line as user authenticated log.
        /// </summary>
        /// <param name="logLines">Log lines.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsUserAuthenticatedLog(List<string> logLines)
        {
            var firstLine = logLines[0];
            if (!firstLine.StartsWith("User Authenticated: ", StringComparison.Ordinal)
                || firstLine[firstLine.Length - 1] != ')')
            {
                return false;
            }

            const int offset = 20;

            var idx = firstLine.IndexOf(" (", offset, StringComparison.Ordinal);
            var userName = firstLine.Substring(offset, idx - offset);
            var userId = firstLine.Substring(idx + 2, firstLine.Length - idx - 3);

            var count = logLines.Count;
            var hasEmail = false;
            var hasBirthday = false;
            var tos = 0;
            for (int i = 1; i < count; i++)
            {
                var line = logLines[i];
                if (line.StartsWith("- hasEmail: ", StringComparison.Ordinal))
                {
#if NET7_0_OR_GREATER
                    hasEmail = bool.Parse(line.AsSpan(12));
#else
                    hasEmail = bool.Parse(line.Substring(12));
#endif
                }
                else if (line.StartsWith("- hasBirthday: ", StringComparison.Ordinal))
                {
#if NET7_0_OR_GREATER
                    hasBirthday = bool.Parse(line.AsSpan(15));
#else
                    hasBirthday = bool.Parse(line.Substring(15));
#endif
                }
                else if (line.StartsWith("- tos: ", StringComparison.Ordinal))
                {
#if NET7_0_OR_GREATER
                    tos = int.Parse(line.AsSpan(7));
#else
                    tos = int.Parse(line.Substring(7));
#endif
                }
                // else if (line.StartsWith("- avatar: ", StringComparison.Ordinal))
                // {
                //
                // }
            }

            var authUserInfo = new AuthUserInfo(userName, userId, hasEmail, hasBirthday, tos);
            AuthUserInfo = authUserInfo;

            OnUserAuthenticated(authUserInfo);

            return true;
        }

        /// <summary>
        /// Parse first log line as application quit log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsApplicationQuitLog(string firstLine)
        {
            if (!firstLine.StartsWith("VRCApplication: HandleApplicationQuit at ", StringComparison.Ordinal))
            {
                return false;
            }

#if NET7_0_OR_GREATER
            var activeTime = double.Parse(firstLine.AsSpan(41));
#else
            var activeTime = double.Parse(firstLine.Substring(41));
#endif  // NET7_0_OR_GREATER
            ClearInfo();
            OnApplicationQuit(activeTime);

            return true;
        }

        /// <summary>
        /// Parse first log line as instance reset notification log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsInstanceResetNotificationLog(string firstLine)
        {
            var match = RegexHelper.InstanceResetNotificationRegex.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;

            OnInstanceResetNotified(int.Parse(groups[1].Value));

            return true;
        }

        /// <summary>
        /// Parse first log line as instance closed log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsInstanceClosedLog(string firstLine)
        {
            if (!firstLine.StartsWith("Instance closed: ", StringComparison.Ordinal))
            {
                return false;
            }

            var instanceInfo = ParseInstanceString(firstLine.Substring(17), _instanceInfo.StayFrom);

            OnInstanceClosed(instanceInfo);

            return true;
        }

        /// <summary>
        /// Parse instance string.
        /// </summary>
        /// <param name="instanceString">Instance string.</param>
        /// <param name="stayFrom">Log timestamp.</param>
        /// <returns>Parsed result.</returns>
        /// <exception cref="InvalidLogException">Thrown when mismatch parent detected.</exception>
        private InstanceInfo ParseInstanceString(string instanceString, DateTime stayFrom)
        {
            var tokens = instanceString.Split('~');
            var ids = tokens[0].Split(':');

            var instanceInfo = new InstanceInfo(stayFrom)
            {
                WorldId = ids[0],
                InstanceString = instanceString,
                InstanceId = ids[1],
                InstanceType = InstanceType.Public,
                LogFrom = LogFrom
            };

            // Options
            var canRequestInvite = false;
            for (int i = 1; i < tokens.Length; i++)
            {
                var token = tokens[i];
                var (optName, optArg) = ParseInstanceStringOption(token);
                switch (optName)
                {
                    case "canRequestInvite":
                        canRequestInvite = true;
                        if (instanceInfo.InstanceType == InstanceType.Invite)
                        {
                            instanceInfo.InstanceType = InstanceType.InvitePlus;
                        }
                        break;
                    case "public":
                        instanceInfo.InstanceType = InstanceType.Public;
                        instanceInfo.UserOrGroupId = optArg;
                        break;
                    case "hidden":
                        instanceInfo.InstanceType = InstanceType.FriendPlus;
                        instanceInfo.UserOrGroupId = optArg;
                        break;
                    case "friends":
                        instanceInfo.InstanceType = InstanceType.Friend;
                        instanceInfo.UserOrGroupId = optArg;
                        break;
                    case "private":
                        instanceInfo.InstanceType = canRequestInvite ? InstanceType.InvitePlus : InstanceType.Invite;
                        instanceInfo.UserOrGroupId = optArg;
                        break;
                    case "region":
                        instanceInfo.Region = optArg switch
                        {
                            "us" => Region.USW,
                            "use" => Region.USE,
                            "eu" => Region.EU,
                            "jp" => Region.JP,
                            _ => throw CreateInvalidLogException($"Unrecognized region is detected: {optArg}")
                        }; ;
                        break;
                    case "nonce":
                        instanceInfo.Nonce = optArg;
                        break;
                    case "group":
                        instanceInfo.UserOrGroupId = optArg;
                        break;
                    case "groupAccessType":
                        instanceInfo.InstanceType = optArg switch
                        {
                            "public" => InstanceType.GroupPublic,
                            "plus" => InstanceType.GroupPlus,
                            "members" => InstanceType.GroupMembers,
                            _ => instanceInfo.InstanceType
                        };
                        break;
                    default:
                        ThrowInvalidLogException($"Unknown option detected: {token}");
                        break;
                }
            }

            return instanceInfo;
        }

        /// <summary>
        /// Parse local instance string.
        /// </summary>
        /// <param name="instanceString">Instance string.</param>
        /// <param name="stayFrom">Log timestamp.</param>
        /// <returns>Parsed result.</returns>
        private InstanceInfo ParseLocalInstanceString(string instanceString, DateTime stayFrom)
        {
            var ids = instanceString.Split(':');
            return new InstanceInfo(stayFrom)
            {
                WorldId = ids[0],
                InstanceString = instanceString,
                InstanceId = ids[1],
                InstanceType = InstanceType.Public,  // VRChat says "Public".
                Region = Region.LocalTest,
                LogFrom = LogFrom
            };
        }

        /// <summary>
        /// Parse instance string optipn, "~XXX(YYY)".
        /// </summary>
        /// <param name="option">Option string.</param>
        /// <returns>Parsed result, tuple of optioh name and arguments.</returns>
        /// <exception cref="InvalidLogException">Thrown when mismatch parent detected.</exception>
        private (string OptionName, string? OptionArg) ParseInstanceStringOption(string option)
        {
            var idxParenStart = option.IndexOf('(');
            if (idxParenStart == -1)
            {
                return (option, null);
            }

            var idxParenEnd = option.IndexOf(')', idxParenStart + 1);
            if (idxParenStart == -1)
            {
                ThrowInvalidLogException($"Corresponding parens in instance string are not found: {option}");
            }

            return (option.Substring(0, idxParenStart), option.Substring(idxParenStart + 1, idxParenEnd - idxParenStart - 1));
            // return (option[..idxParenStart], option[(idxParenStart + 1)..idxParenEnd]);
        }

        /// <summary>
        /// Call callback methods with remaining <see cref="_userInfoDict"/> and <see cref="_instanceInfo"/>, then clear them.
        /// </summary>
        private void ClearInfo()
        {
            foreach (var kv in _userInfoDict)
            {
                var userInfo = kv.Value;
                OnUserLeft(kv.Key, userInfo.UserId, userInfo.JoinAt, null, _instanceInfo);
            }
            _userInfoDict.Clear();

            if (!_instanceInfo.IsEmitted && _instanceInfo.StayFrom != default)
            {
                OnLeftFromInstance(_instanceInfo);
            }

            _instanceInfo = new InstanceInfo(default);
        }
    }
}
