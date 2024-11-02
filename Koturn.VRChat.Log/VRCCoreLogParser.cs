using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        /// Regex to extract pickedup object.
        /// </summary>
        private static readonly Regex _regexPickupObject;
        /// <summary>
        /// Regex to extract dropped object.
        /// </summary>
        private static readonly Regex _regexDropObject;


        /// <summary>
        /// Initialize regexes.
        /// </summary>
        static VRCCoreLogParser()
        {
            _regexPickupObject = RegexHelper.GetPickupObjectRegex();
            _regexDropObject = RegexHelper.GetDropObjectRegex();
        }


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
        private readonly Dictionary<string, UserInfo> _userInfoDict;
        /// <summary>
        /// Instance information.
        /// </summary>
        private InstanceInfo _instanceInfo;


        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for <see cref="FileStream"/> and <see cref="StreamReader"/>.</param>
        public VRCCoreLogParser(string filePath, int bufferSize = 65536)
            : base(filePath, bufferSize)
        {
            _userInfoDict = new Dictionary<string, UserInfo>();
            _instanceInfo = new InstanceInfo(default);
            AuthUserInfo = null;
            IsDisposed = false;
        }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="StreamReader"/>.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCCoreLogParser"/> object is disposed; otherwise, false.</param>
        public VRCCoreLogParser(Stream stream, int bufferSize = 65536, bool leaveOpen = false)
            : base(stream, bufferSize, leaveOpen)
        {
            _userInfoDict = new Dictionary<string, UserInfo>();
            _instanceInfo = new InstanceInfo(default);
            AuthUserInfo = null;
            IsDisposed = false;
        }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="reader"><see cref="TextReader"/> of VRChat log file.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="reader"/> open
        /// after the <see cref="VRCCoreLogParser"/> object is disposed; otherwise, false.</param>
        public VRCCoreLogParser(TextReader reader, bool leaveOpen = false)
            : base(reader, leaveOpen)
        {
            _userInfoDict = new Dictionary<string, UserInfo>();
            _instanceInfo = new InstanceInfo(default);
            AuthUserInfo = null;
            IsDisposed = false;
        }


        /// <summary>
        /// Load one line of log file and parse it, and fire each event as needed.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        /// <returns>True if any of the log parsing defined in this class succeeds, otherwise false.</returns>
        protected override bool OnLogDetected(DateTime logAt, LogLevel level, List<string> logLines)
        {
            switch (level)
            {
                case LogLevel.Warning:
                    OnWarningDetected(logAt, level, logLines);
                    return false;
                case LogLevel.Error:
                    OnErrorDetected(logAt, level, logLines);
                    return false;
                case LogLevel.Exception:
                    OnExceptionDetected(logAt, level, logLines);
                    return false;
                default:
                    break;
            }

            var firstLine = logLines[0];

            return ParseAsBehaviourLog(logAt, firstLine)
                || ParseAsScreenshotLog(logAt, firstLine)
                || ParseAsVideoPlaybackLog(logAt, firstLine)
                || ParseAsStringDownloadLog(logAt, firstLine)
                || ParseAsImageDownloadLog(logAt, firstLine)
                || ParseAsUserAuthenticatedLog(logAt, logLines);
        }

        /// <summary>
        /// This method is called when user authenticated log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="authUserInfo">Authenticated user information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsUserAuthenticatedLog(DateTime, List{string})"/></para>
        /// </remarks>
        protected virtual void OnUserAuthenticated(DateTime logAt, AuthUserInfo authUserInfo)
        {
        }

        /// <summary>
        /// This method is called when join log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsJoinedLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnJoinedToInstance(DateTime logAt, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when leave log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsLeftLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnLeftFromInstance(DateTime logAt, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when user join log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsUserJoinLeaveLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnUserJoined(DateTime logAt, string userName, string? userId, DateTime stayFrom, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when user leave log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="stayUntil">A timestamp the user left.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsUserJoinLeaveLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnUserLeft(DateTime logAt, string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when unregistering user log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="stayUntil">A timestamp the user left.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsUserUnregisteringLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnUserUnregistering(DateTime logAt, string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when pickup object log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="objectName">Pickedup object name.</param>
        /// <param name="isEquipped">True if equipped.</param>
        /// <param name="isEquippable">True if the object is equippable.</param>
        /// <param name="lastInputMethod">Last input method name.</param>
        /// <param name="isAutoEquipController">True if the object is auto equip controller.</param>
        protected virtual void OnPickupObject(DateTime logAt, string objectName, bool isEquipped, bool isEquippable, string lastInputMethod, bool isAutoEquipController)
        {
        }

        /// <summary>
        /// This method is called when drop object log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="objectName">Pickedup object name.</param>
        /// <param name="isEquipped">True if the object was equipped.</param>
        /// <param name="dropReason">Reason for dropping the object.</param>
        /// <param name="lastInputMethod">Last input method name.</param>
        protected virtual void OnDropObject(DateTime logAt, string objectName, bool isEquipped, string dropReason, string lastInputMethod)
        {
        }

        /// <summary>
        /// This method is called when screenshot log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="filePath">Screenshort file path.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsScreenshotLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnScreenshotTook(DateTime logAt, string filePath, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when video URL resolving log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="url">Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsVideoPlaybackLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnVideoUrlResolving(DateTime logAt, string url, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when video URL resolved log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="url">Video URL.</param>
        /// <param name="resolvedUrl">Resolved Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsVideoPlaybackLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnVideoUrlResolved(DateTime logAt, string url, string resolvedUrl, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when string/image download log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="url">Download URL.</param>
        /// <param name="type"></param>
        /// <param name="instanceInfo"></param>
        /// <remarks>
        /// <para>Called from following methods.</para>
        /// <para><see cref="ParseAsStringDownloadLog(DateTime, string)"/></para>
        /// <para><see cref="ParseAsImageDownloadLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnDownloaded(DateTime logAt, string url, DownloadType type, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when warning log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="OnLogDetected(DateTime, LogLevel, List{string})"/></para>
        /// </remarks>
        protected virtual void OnWarningDetected(DateTime logAt, LogLevel level, List<string> logLines)
        {
        }

        /// <summary>
        /// This method is called when error log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="OnLogDetected(DateTime, LogLevel, List{string})"/></para>
        /// </remarks>
        protected virtual void OnErrorDetected(DateTime logAt, LogLevel level, List<string> logLines)
        {
        }

        /// <summary>
        /// This method is called when exception log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="OnLogDetected(DateTime, LogLevel, List{string})"/></para>
        /// </remarks>
        protected virtual void OnExceptionDetected(DateTime logAt, LogLevel level, List<string> logLines)
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
                foreach (var kv in _userInfoDict)
                {
                    var userInfo = kv.Value;
                    OnUserLeft(LogUntil, kv.Key, userInfo.UserId, userInfo.JoinAt, null, _instanceInfo);
                }
                _userInfoDict.Clear();

                if (!_instanceInfo.IsEmitted && _instanceInfo.StayFrom != default)
                {
                    OnLeftFromInstance(LogUntil, _instanceInfo);
                }

                _instanceInfo = new InstanceInfo(default);
            }

            IsDisposed = true;
        }


        /// <summary>
        /// Parse first log line as "[Behaviour]" log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        /// <exception cref="InvalidDataException">Thrown when duplicate joined timestamp.</exception>
        private bool ParseAsBehaviourLog(DateTime logAt, string firstLine)
        {
            if (!firstLine.StartsWith("[Behaviour] ", StringComparison.Ordinal))
            {
                return false;
            }

            return ParseAsUserJoinLeaveLog(logAt, firstLine)
                || ParseAsUserUnregisteringLog(logAt, firstLine)
                || ParseAsPickupObjectLog(logAt, firstLine)
                || ParseAsDropObjectLog(logAt, firstLine)
                || ParseAsJoiningLog(logAt, firstLine)
                || ParseAsJoinedLog(logAt, firstLine)
                || ParseAsLeftLog(logAt, firstLine);
        }


        /// <summary>
        /// Parse first log line as user joined or left log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        /// <exception cref="InvalidDataException">Thrown when duplicate joined timestamp.</exception>
        private bool ParseAsUserJoinLeaveLog(DateTime logAt, string firstLine)
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

            if (IsSubstringAt("Joined ", firstLine, JoinLeaveOffset))
            {
                var userName = ExtractUserNameAndId(firstLine, 27, out var userId);

                OnUserJoined(logAt, userName, userId, logAt, _instanceInfo);
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
                    OnUserLeft(logAt, userName, userId, userInfo.JoinAt, logAt, _instanceInfo);
                    userInfoDict.Remove(userName);
                }
                else
                {
                    // Rare case.
                    // e.g.) Detect left user while you joining an instance.
                    OnUserLeft(logAt, userName, userId, logAt, logAt, _instanceInfo);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parse first log line as user unregistering log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsUserUnregisteringLog(DateTime logAt, string firstLine)
        {
            if (!IsSubstringAt("Unregistering ", firstLine, BehaviourLogOffset))
            {
                return false;
            }

            var userInfoDict = _userInfoDict;
            var userName = firstLine.Substring(BehaviourLogOffset + 14);
            if (userInfoDict.TryGetValue(userName, out var userInfo))
            {
                OnUserUnregistering(logAt, userName, userInfo.UserId, userInfo.JoinAt, logAt, _instanceInfo);
                userInfoDict.Remove(userName);
            }

            return true;
        }

        /// <summary>
        /// Parse first log line as pickup object log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsPickupObjectLog(DateTime logAt, string firstLine)
        {
            var match = _regexPickupObject.Match(firstLine, BehaviourLogOffset);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;
            OnPickupObject(
                logAt,
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
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsDropObjectLog(DateTime logAt, string firstLine)
        {
            var match = _regexDropObject.Match(firstLine, BehaviourLogOffset);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;
            OnDropObject(
                logAt,
                groups[1].Value,
                bool.Parse(groups[2].Value),
                groups[3].Value,
                groups[4].Value);

            return true;
        }

        /// <summary>
        /// Parse first log line as joining to instance log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsJoiningLog(DateTime logAt, string firstLine)
        {
            if (!IsSubstringAt("Joining wrld_", firstLine, BehaviourLogOffset))
            {
                return false;
            }

            var instanceString = firstLine.Substring(BehaviourLogOffset + 8);
            var tokens = instanceString.Split('~');
            var ids = tokens[0].Split(':');

            var instanceInfo = new InstanceInfo(logAt)
            {
                WorldId = ids[0],
                InstanceString = instanceString,
                InstanceId = ids[1],
                InstanceType = InstanceType.Public,
                LogFrom = LogFrom
            };

            // Options
            var canRequestInvite = false;
            foreach (var token in tokens.Skip(1))
            {
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

            _instanceInfo = instanceInfo;

            return true;
        }

        /// <summary>
        /// Parse first log line as joined to instance log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsJoinedLog(DateTime logAt, string firstLine)
        {
            if (!IsSubstringAt("Joining or Creating Room: ", firstLine, BehaviourLogOffset))
            {
                return false;
            }

            _instanceInfo.WorldName = firstLine.Substring(BehaviourLogOffset + 26);
            OnJoinedToInstance(logAt, _instanceInfo);

            return true;
        }

        /// <summary>
        /// Parse first log line as left from instance log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsLeftLog(DateTime logAt, string firstLine)
        {
            if (!IsSubstringAt("OnLeftRoom", firstLine, BehaviourLogOffset))
            {
                return false;
            }

            _instanceInfo.StayUntil = logAt;
            OnLeftFromInstance(logAt, _instanceInfo);
            _instanceInfo.IsEmitted = true;

            return true;
        }

        /// <summary>
        /// Parse first log line as video playback log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsVideoPlaybackLog(DateTime logAt, string firstLine)
        {
            if (!firstLine.StartsWith("[Video Playback] "))
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

                OnVideoUrlResolving(logAt, firstLine.Substring(32, firstLine.Length - 33), _instanceInfo);

                return true;
            }

            // if (SubstringEquals(firstLine, 44, "Attempting to resolve URL '"))
            // {
            //     OnVideoUrlResolving(logAt, firstLine.Substring(44, firstLine.Length - 45), _instanceInfo);
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

                OnVideoUrlResolved(logAt, url, resolvedUrl, _instanceInfo);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Parse first log line as screenshot log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsScreenshotLog(DateTime logAt, string firstLine)
        {
            if (!firstLine.StartsWith("[VRC Camera] Took screenshot to: ", StringComparison.Ordinal))
            {
                return false;
            }

            OnScreenshotTook(logAt, firstLine.Substring(33), _instanceInfo);

            return true;
        }

        /// <summary>
        /// Parse first log line as string download log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsStringDownloadLog(DateTime logAt, string firstLine)
        {
            if (!firstLine.StartsWith("[String Download] Attempting to load String from URL '", StringComparison.Ordinal)
                || firstLine[firstLine.Length - 1] != '\'')
            {
                return false;
            }

            OnDownloaded(logAt, firstLine.Substring(54, firstLine.Length - 55), DownloadType.String, _instanceInfo);

            return true;
        }

        /// <summary>
        /// Parse first log line as image download log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsImageDownloadLog(DateTime logAt, string firstLine)
        {
            if (!firstLine.StartsWith("[Image Download] Attempting to load image from URL '", StringComparison.Ordinal)
                || firstLine[firstLine.Length - 1] != '\'')
            {
                return false;
            }

            OnDownloaded(logAt, firstLine.Substring(52, firstLine.Length - 53), DownloadType.Image, _instanceInfo);

            return true;
        }

        /// <summary>
        /// Parse first log line as user authenticated log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="logLines">Log lines.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsUserAuthenticatedLog(DateTime logAt, List<string> logLines)
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
                if (line.StartsWith("- hasEmail: "))
                {
#if NET7_0_OR_GREATER
                    hasEmail = bool.Parse(line.AsSpan(12));
#else
                    hasEmail = bool.Parse(line.Substring(12));
#endif
                }
                else if (line.StartsWith("- hasBirthday: "))
                {
#if NET7_0_OR_GREATER
                    hasBirthday = bool.Parse(line.AsSpan(15));
#else
                    hasBirthday = bool.Parse(line.Substring(15));
#endif
                }
                else if (line.StartsWith("- tos: "))
                {
#if NET7_0_OR_GREATER
                    tos = int.Parse(line.AsSpan(7));
#else
                    tos = int.Parse(line.Substring(7));
#endif
                }
                // else if (line.StartsWith("- avatar: "))
                // {
                //
                // }
            }

            var authUserInfo = new AuthUserInfo(userName, userId, hasEmail, hasBirthday, tos);
            AuthUserInfo = authUserInfo;

            OnUserAuthenticated(logAt, authUserInfo);

            return true;
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
    }
}
