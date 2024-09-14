using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Exceptions;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// VRChat log file parser.
    /// </summary>
    public abstract class VRCCoreLogParser : VRCBaseLogParser
    {
        /// <summary>
        /// ToN save data preamble log line.
        /// </summary>
        public const string TonSaveDataPreamble = "[TERRORS SAVE CODE CREATED. PLEASE MAKE SURE YOU COPY THE ENTIRE THING. DO NOT INCLUDE [START] or [END]]";

        /// <summary>
        /// <para>Regex to extract world name.</para>
        /// <para><c>\[Behaviour\] Joining or Creating Room: (.+)$</c></para>
        /// </summary>
        private static readonly Regex _regexWorldName;
        /// <summary>
        /// <para>Regex to detect leaving from the instance.</para>
        /// <para><c>\[Behaviour\] OnLeftRoom$</c></para>
        /// </summary>
        private static readonly Regex _regexWorldLeft;
        /// <summary>
        /// <para>Regex to extract joined or lefted player's name.</para>
        /// <para><c>\[Behaviour\] OnPlayer(Joined|Left) (.+)$</c></para>
        /// </summary>
        private static readonly Regex _regexJoinLeave;
        /// <summary>
        /// <para>Regex to extract unregistered player's name.</para>
        /// <para><c>\[Behaviour\] Unregistering (.+)$</c></para>
        /// </summary>
        private static readonly Regex _regexUnregistering;
        /// <summary>
        /// <para>Regex to extract screenshort log.</para>
        /// <para><c>\[VRC Camera\] Took screenshot to: (.+\.png)$</c></para>
        /// </summary>
        private static readonly Regex _regexScreenshot;
        /// <summary>
        /// <para>Regex to extract video URL resolved log.</para>
        /// <para><c>URL '(.+)' resolved to '(.+)'</c></para>
        /// </summary>
        private static readonly Regex _regexVideoResolved;
        /// <summary>
        /// <para>Regex to extract video URL resolving log.</para>
        /// <para><c>Resolving URL '(.+)'</c></para>
        /// </summary>
        private static readonly Regex _regexVideoResolving;
        /// <summary>
        /// <para>Regex to extract string download log.</para>
        /// <para><c>\[String Download\] Attempting to load String from URL '([^']+)'$</c></para>
        /// </summary>
        private static readonly Regex _regexDlString;
        /// <summary>
        /// <para>Regex to extract image download log.</para>
        /// <para><c>\[Image Download\] Attempting to load image from URL '([^']+)'$</c></para>
        /// </summary>
        private static readonly Regex _regexDlImage;
        /// <summary>
        /// <para>Regex to extract Idle Home save data.</para>
        /// <para><c>\[🦀 Idle Home 🦀\] Saved \d{2}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}: (.+)$</c></para>
        /// </summary>
        private static readonly Regex _regexIdleHomeSave;
        /// <summary>
        /// <para>Regex to extract ToN save data.</para>
        /// <para><c>^\[START\]([0-9,_]+)\[END\]$</c></para>
        /// </summary>
        private static readonly Regex _regexTonSave;


        /// <summary>
        /// Initialize regexes.
        /// </summary>
        static VRCCoreLogParser()
        {
            const RegexOptions regexOpt = RegexOptions.Compiled | RegexOptions.CultureInvariant;
            _regexWorldName = new Regex(@"\[Behaviour\] Joining or Creating Room: (.+)$", regexOpt);
            _regexWorldLeft = new Regex(@"\[Behaviour\] OnLeftRoom$", regexOpt);
            _regexJoinLeave = new Regex(@"\[Behaviour\] OnPlayer(Joined|Left) (.+)$", regexOpt);
            _regexUnregistering = new Regex(@"\[Behaviour\] Unregistering (.+)$", regexOpt);
            _regexScreenshot = new Regex(@"\[VRC Camera\] Took screenshot to: (.+\.png)$", regexOpt);
            _regexVideoResolved = new Regex(@"URL '(.+)' resolved to '(.+)'", regexOpt);
            _regexVideoResolving = new Regex(@"Resolving URL '(.+)'", regexOpt);
            _regexDlString = new Regex(@"\[String Download\] Attempting to load String from URL '([^']+)'$", regexOpt);
            _regexDlImage = new Regex(@"\[Image Download\] Attempting to load image from URL '([^']+)'$", regexOpt);
            _regexIdleHomeSave = new Regex(@"\[🦀 Idle Home 🦀\] Saved \d{2}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}: (.+)$", regexOpt);
            _regexTonSave = new Regex(@"^\[START\]([0-9,_]+)\[END\]$", regexOpt);
        }


        /// <summary>
        /// A flag property which indicates this instance is disposed or not.
        /// </summary>
        public new bool IsDisposed { get; private set; }

        /// <summary>
        /// Dictionary to contain user name and join timestamp of the user.
        /// </summary>
        private readonly Dictionary<string, DateTime> _userJoinTimeDict;
        /// <summary>
        /// Instance information.
        /// </summary>
        private InstanceInfo _instanceInfo;
        /// <summary>
        /// Indicate next log line is ToN save data.
        /// </summary>
        private bool _isTonSaveData;


        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for <see cref="FileStream"/> and <see cref="StreamReader"/>.</param>
        public VRCCoreLogParser(string filePath, int bufferSize = 65536)
            : base(filePath, bufferSize)
        {
            _userJoinTimeDict = new Dictionary<string, DateTime>();
            _instanceInfo = new InstanceInfo(default);
            _isTonSaveData = false;
            IsDisposed = false;
        }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="StreamReader"/>.</param>
        public VRCCoreLogParser(Stream stream, int bufferSize = 65536)
            : base(stream, bufferSize)
        {
            _userJoinTimeDict = new Dictionary<string, DateTime>();
            _instanceInfo = new InstanceInfo(default);
            _isTonSaveData = false;
            IsDisposed = false;
        }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="reader"><see cref="TextReader"/> of VRChat log file.</param>
        public VRCCoreLogParser(TextReader reader)
            : base(reader)
        {
            _userJoinTimeDict = new Dictionary<string, DateTime>();
            _instanceInfo = new InstanceInfo(default);
            _isTonSaveData = false;
            IsDisposed = false;
        }


        /// <summary>
        /// Load one line of log file and parse it, and fire each event as needed.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        protected override void OnLogDetected(DateTime logAt, LogLevel level, List<string> logLines)
        {
            switch (level)
            {
                case LogLevel.Warning:
                    OnWarningDetected(logAt, level, logLines);
                    return;
                case LogLevel.Error:
                    OnErrorDetected(logAt, level, logLines);
                    return;
                case LogLevel.Exception:
                    OnExceptionDetected(logAt, level, logLines);
                    return;
                default:
                    break;
            }

            var firstLine = logLines[0];

            _ = ParseAsUserJoinLeaveLog(logAt, firstLine)
                || ParseAsUserUnregisteringLog(logAt, firstLine)
                || ParseAsScreenshotLog(logAt, firstLine)
                || ParseAsVideoPlaybackLog(logAt, firstLine)
                || ParseAsStringDownloadLog(logAt, firstLine)
                || ParseAsImageDownloadLog(logAt, firstLine)
                || ParseAsJoiningLog(logAt, firstLine)
                || ParseAsJoinedLog(logAt, firstLine)
                || ParseAsLeftLog(logAt, firstLine)
                || ParseAsIdleHomeSaveData(logAt, firstLine)
                || ParseAsTonSaveDataPreamble(firstLine)
                || ParseAsTonSaveData(logAt, firstLine);
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
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsUserJoinLeaveLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnUserJoined(DateTime logAt, string userName, DateTime stayFrom, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when user leave log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="userName">User name.</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="stayUntil">A timestamp the user left.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsUserJoinLeaveLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnUserLeft(DateTime logAt, string userName, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
        {
        }

        /// <summary>
        /// This method is called when unregistering user log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="userName">User name.</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="stayUntil">A timestamp the user left.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsUserUnregisteringLog(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnUserUnregistering(DateTime logAt, string userName, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
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
        /// This method is called when Idle Home save data log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="saveText">Save data text.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsIdleHomeSaveData(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnIdleHomeSaved(DateTime logAt, string saveText)
        {
        }

        /// <summary>
        /// This method is called when Terrors of nowhere save data log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="saveText">Save data text.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsTonSaveData(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnTerrorsOfNowhereSaved(DateTime logAt, string saveText)
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
                foreach (var kv in _userJoinTimeDict)
                {
                    OnUserLeft(LogUntil, kv.Key, kv.Value, null, _instanceInfo);
                }
                _userJoinTimeDict.Clear();

                if (!_instanceInfo.IsEmitted && _instanceInfo.StayFrom != default)
                {
                    OnLeftFromInstance(LogUntil, _instanceInfo);
                }

                _instanceInfo = new InstanceInfo(default);
            }

            IsDisposed = true;
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
            var match = _regexJoinLeave.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;
            var joinLeaveKind = groups[1].Value;
            var userName = groups[2].Value;
            switch (joinLeaveKind)
            {
                case "Joined":
                    OnUserJoined(logAt, userName, logAt, _instanceInfo);
                    if (_userJoinTimeDict.ContainsKey(userName))
                    {
                        ThrowInvalidLogException(
                            $@"User join log already exists; {userName} {_userJoinTimeDict[userName]:yyyy-MM-dd HH\:mm\:ss} ({logAt:yyyy-MM-dd HH\:mm\:ss}).");
                    }
                    _userJoinTimeDict.Add(userName, logAt);
                    return true;
                case "Left":
                    if (_userJoinTimeDict.ContainsKey(userName))
                    {
                        OnUserLeft(logAt, userName, _userJoinTimeDict[userName], logAt, _instanceInfo);
                        _userJoinTimeDict.Remove(userName);
                    }
                    else
                    {
                        OnUserLeft(logAt, userName, logAt, logAt, _instanceInfo);
                    }
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Parse first log line as user unregistering log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsUserUnregisteringLog(DateTime logAt, string firstLine)
        {
            var match = _regexUnregistering.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;
            var userName = groups[1].Value;
            if (_userJoinTimeDict.ContainsKey(userName))
            {
                OnUserUnregistering(logAt, userName, _userJoinTimeDict[userName], logAt, _instanceInfo);
                _userJoinTimeDict.Remove(userName);
            }

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

            var content = firstLine.Substring(17);

            Match match;
            if ((match = _regexVideoResolved.Match(content)).Success)
            {
                OnVideoUrlResolved(logAt, match.Groups[1].Value, match.Groups[2].Value, _instanceInfo);
            }
            else if ((match = _regexVideoResolving.Match(content)).Success)
            {
                OnVideoUrlResolving(logAt, match.Groups[1].Value, _instanceInfo);
            }

            return true;
        }

        /// <summary>
        /// Parse first log line as screenshot log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsScreenshotLog(DateTime logAt, string firstLine)
        {
            var match = _regexScreenshot.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            OnScreenshotTook(logAt, match.Groups[1].Value, _instanceInfo);

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
            var match = _regexDlString.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            OnDownloaded(logAt, match.Groups[1].Value, DownloadType.String, _instanceInfo);

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
            var match = _regexDlImage.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            OnDownloaded(logAt, match.Groups[1].Value, DownloadType.Image, _instanceInfo);

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
            if (!firstLine.StartsWith("[Behaviour] Joining wrld_"))
            {
                return false;
            }

            var instanceString = firstLine.Substring(20);
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
            var match = _regexWorldName.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            _instanceInfo.WorldName = match.Groups[1].Value;
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
            var match = _regexWorldLeft.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            _instanceInfo.StayUntil = logAt;
            OnLeftFromInstance(logAt, _instanceInfo);
            _instanceInfo.IsEmitted = true;

            return true;
        }

        /// <summary>
        /// Parse first log line as Idle Home save data log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsIdleHomeSaveData(DateTime logAt, string firstLine)
        {
            var match = _regexIdleHomeSave.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            OnIdleHomeSaved(logAt, match.Groups[1].Value);

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere save data preamble log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonSaveDataPreamble(string firstLine)
        {
            if (firstLine != TonSaveDataPreamble)
            {
                return false;
            }

            _isTonSaveData = true;

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere save data log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonSaveData(DateTime logAt, string firstLine)
        {
            if (!_isTonSaveData)
            {
                return false;
            }

            _isTonSaveData = false;

            var match = _regexTonSave.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            OnTerrorsOfNowhereSaved(logAt, match.Groups[1].Value);

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
