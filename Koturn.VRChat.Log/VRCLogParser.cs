using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Events;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// VRChat log file parser.
    /// </summary>
    public class VRCLogParser
    {
        /// <summary>
        /// Log file filter.
        /// </summary>
        public const string VRChatLogFileFilter = "output_log_*.txt";
        /// <summary>
        /// ToN save data preamble log line.
        /// </summary>
        public const string TonSaveDataPreamble = "[TERRORS SAVE CODE CREATED. PLEASE MAKE SURE YOU COPY THE ENTIRE THING. DO NOT INCLUDE [START] or [END]]";

        /// <summary>
        /// Default VRChat log directory.
        /// </summary>
        public static string DefaultVRChatLogDirectory { get; }
        /// <summary>
        /// <para>Regex to detect log message with timestamp.</para>
        /// <para><c>^(\d{4})\.(\d{2})\.(\d{2}) (\d{2}):(\d{2}):(\d{2}) (\w+)\s+-  (.+)$</c></para>
        /// </summary>
        private static readonly Regex _regexLogLine;
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
        private static readonly Regex _regexJoinLeft;
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
        static VRCLogParser()
        {
            DefaultVRChatLogDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
                "VRChat",
                "VRChat");
            _regexLogLine = new Regex(@"^(\d{4})\.(\d{2})\.(\d{2}) (\d{2}):(\d{2}):(\d{2}) (\w+)\s+-  (.+)$", RegexOptions.Compiled);
            _regexWorldName = new Regex(@"\[Behaviour\] Joining or Creating Room: (.+)$", RegexOptions.Compiled);
            _regexWorldLeft = new Regex(@"\[Behaviour\] OnLeftRoom$", RegexOptions.Compiled);
            _regexJoinLeft = new Regex(@"\[Behaviour\] OnPlayer(Joined|Left) (.+)$", RegexOptions.Compiled);
            _regexUnregistering = new Regex(@"\[Behaviour\] Unregistering (.+)$", RegexOptions.Compiled);
            _regexScreenshot = new Regex(@"\[VRC Camera\] Took screenshot to: (.+\.png)$", RegexOptions.Compiled);
            _regexVideoResolved = new Regex(@"URL '(.+)' resolved to '(.+)'", RegexOptions.Compiled);
            _regexVideoResolving = new Regex(@"Resolving URL '(.+)'", RegexOptions.Compiled);
            _regexDlString = new Regex(@"\[String Download\] Attempting to load String from URL '([^']+)'$", RegexOptions.Compiled);
            _regexDlImage = new Regex(@"\[Image Download\] Attempting to load image from URL '([^']+)'$", RegexOptions.Compiled);
            _regexIdleHomeSave = new Regex(@"\[🦀 Idle Home 🦀\] Saved \d{2}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}: (.+)$", RegexOptions.Compiled);
            _regexTonSave = new Regex(@"^\[START\]([0-9,_]+)\[END\]$", RegexOptions.Compiled);
        }


        /// <summary>
        /// Occurs when detect a log that you joined to instance.
        /// </summary>
        public event EventHandler<JoinLeaveInstanceEventArgs>? JoinedToInstance;
        /// <summary>
        /// Occurs when detect a log that you left from instance.
        /// </summary>
        public event EventHandler<JoinLeaveInstanceEventArgs>? LeftFromInstance;
        /// <summary>
        /// Occurs when detect a log that any player joined to your instance.
        /// </summary>
        public event EventHandler<UserJoinLeaveEventArgs>? UserJoined;
        /// <summary>
        /// Occurs when detect a log that any player left from your instance.
        /// </summary>
        public event EventHandler<UserJoinLeaveEventArgs>? UserLeft;
        /// <summary>
        /// Occurs when detect a log that any player unregistering from your instance.
        /// </summary>
        public event EventHandler<UserJoinLeaveEventArgs>? UserUnregistering;
        /// <summary>
        /// Occurs when detect a log that you take a screenshot.
        /// </summary>
        public event EventHandler<ScreenshotTakeEventArgs>? ScreenshotTook;
        /// <summary>
        /// Occurs when detect a log that video URL resolving.
        /// </summary>
        public event EventHandler<VideoUrlResolveEventArgs>? VideoUrlResolving;
        /// <summary>
        /// Occurs when detect a log that video URL resolved.
        /// </summary>
        public event EventHandler<VideoUrlResolveEventArgs>? VideoUrlResolved;
        /// <summary>
        /// Occurs when detect a log that string or image is downloaded.
        /// </summary>
        public event EventHandler<DownloadEventArgs>? Downloaded;
        /// <summary>
        /// Occurs when detect a log that save data text of Idle Home is generated.
        /// </summary>
        public event EventHandler<SaveEventArgs>? IdleHomeSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Terrors of Nowhere is generated.
        /// </summary>
        public event EventHandler<SaveEventArgs>? TerrorsOfNowhereSaved;
        /// <summary>
        /// Occurs when detect a warning log.
        /// </summary>
        public event EventHandler<ErrorLogEventArgs>? WarningDetected;
        /// <summary>
        /// Occurs when detect a error log.
        /// </summary>
        public event EventHandler<ErrorLogEventArgs>? ErrorDetected;
        /// <summary>
        /// Occurs when detect a exception log.
        /// </summary>
        public event EventHandler<ErrorLogEventArgs>? ExceptionDetected;


        /// <summary>
        /// Log line counter.
        /// </summary>
        public ulong LineCount { get; private set; }
        /// <summary>
        /// First timestamp of log file.
        /// </summary>
        public DateTime LogFrom { get; private set; }
        /// <summary>
        /// Last timestamp of log file.
        /// </summary>
        public DateTime LogUntil { get; private set; }
        /// <summary>
        /// Dictionary to contain user name and join timestamp of the user.
        /// </summary>
        private Dictionary<string, DateTime> _userJoinTimeDict;
        /// <summary>
        /// Instance information.
        /// </summary>
        private InstanceInfo _instanceInfo;
        /// <summary>
        /// Log line stack.
        /// </summary>
        private List<string> _lineStack;
        /// <summary>
        /// Empty line count.
        /// </summary>
        private int _emptyLineCount;
        /// <summary>
        /// Indicate next log line is ToN save data.
        /// </summary>
        private bool _isTonSaveData;

        /// <summary>
        /// Create <see cref="VRCLogParser"/> instance.
        /// </summary>
        public VRCLogParser()
        {
            LineCount = 0;
            LogFrom = default;
            LogUntil = default;
            _userJoinTimeDict = new Dictionary<string, DateTime>();
            _instanceInfo = new InstanceInfo(default);
            _lineStack = new List<string>(128);
            _isTonSaveData = false;
        }

        /// <summary>
        /// Open file stream and parse end of the file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        public void Parse(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.SequentialScan))
            {
                Parse(fs);
            }
        }

        /// <summary>
        /// Read and parse to end of the <paramref name="fs"/>.
        /// </summary>
        /// <param name="fs">A <see cref="FileStream"/> of log file.</param>
        public void Parse(FileStream fs)
        {
            using (var sr = new StreamReader(fs, Encoding.UTF8, false, 65536, true))
            {
                Parse(sr);
            }
        }

        /// <summary>
        /// Read and parse to end of the <paramref name="sr"/>.
        /// </summary>
        /// <param name="sr">A <see cref="StreamReader"/> of log file.</param>
        public void Parse(StreamReader sr)
        {
            for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
            {
                LoadLine(line);
            }
        }

        /// <summary>
        /// Clear internal informations.
        /// </summary>
        public void Terminate()
        {
            foreach (var kv in _userJoinTimeDict)
            {
                UserLeft?.Invoke(this, new UserJoinLeaveEventArgs(LogUntil, kv.Key, kv.Value, null, _instanceInfo));
            }
            _userJoinTimeDict.Clear();

            if (!_instanceInfo.IsEmitted && _instanceInfo.StayFrom != default)
            {
                LeftFromInstance?.Invoke(this, new JoinLeaveInstanceEventArgs(LogUntil, _instanceInfo));
                _instanceInfo.IsEmitted = true;
            }

            _instanceInfo = new InstanceInfo(default);

            _emptyLineCount = 0;
            LineCount = 0;
            LogFrom = default;
            LogUntil = default;
        }

        /// <summary>
        /// Load one line of log file and parse it, and fire each event as needed.
        /// </summary>
        /// <param name="line">Log line.</param>
        /// <exception cref="InvalidDataException"></exception>
        public void LoadLine(string line)
        {
            LineCount++;
            if (line != string.Empty)
            {
                if (_emptyLineCount == 1)
                {
                    _lineStack.Add(string.Empty);
                }
                _emptyLineCount = 0;
                _lineStack.Add(line);
                return;
            }

            _emptyLineCount++;
            if (_emptyLineCount < 2 || _lineStack.Count == 0)
            {
                return;
            }

            var parsed = ParseLogLine(_lineStack[0]);
            _lineStack[0] = parsed.Message;
            if (LogFrom == default)
            {
                LogFrom = parsed.DateTime;
            }
            LogUntil = parsed.DateTime;

            switch (parsed.Level)
            {
                case LogLevel.Warning:
                case LogLevel.Error:
                case LogLevel.Exception:
                    EmitLineStack(parsed);
                    return;
                default:
                    break;
            }

            Match match;
            if ((match = _regexJoinLeft.Match(parsed.Message)).Success)
            {
                var groups = match.Groups;
                var joinLeaveKind = groups[1].Value;
                var userName = groups[2].Value;
                switch (joinLeaveKind)
                {
                    case "Joined":
                        UserJoined?.Invoke(this, new UserJoinLeaveEventArgs(parsed.DateTime, userName, parsed.DateTime, _instanceInfo));
                        if (_userJoinTimeDict.ContainsKey(userName))
                        {
                            throw new InvalidDataException("Join log already exists.");
                        }
                        _userJoinTimeDict.Add(userName, parsed.DateTime);
                        break;
                    case "Left":
                        if (_userJoinTimeDict.ContainsKey(userName))
                        {
                            UserLeft?.Invoke(this, new UserJoinLeaveEventArgs(parsed.DateTime, userName, _userJoinTimeDict[userName], parsed.DateTime, _instanceInfo));
                            _userJoinTimeDict.Remove(userName);
                        }
                        else
                        {
                            UserLeft?.Invoke(this, new UserJoinLeaveEventArgs(parsed.DateTime, userName, parsed.DateTime, parsed.DateTime, _instanceInfo));
                        }
                        break;
                    default:
                        break;
                }
            }
            else if ((match = _regexUnregistering.Match(parsed.Message)).Success)
            {
                var groups = match.Groups;
                var userName = groups[1].Value;
                if (_userJoinTimeDict.ContainsKey(userName))
                {
                    UserUnregistering?.Invoke(this, new UserJoinLeaveEventArgs(parsed.DateTime, userName, _userJoinTimeDict[userName], parsed.DateTime, _instanceInfo));
                    _userJoinTimeDict.Remove(userName);
                }
            }
            else if ((match = _regexScreenshot.Match(parsed.Message)).Success)
            {
                ScreenshotTook?.Invoke(this, new ScreenshotTakeEventArgs(parsed.DateTime, match.Groups[1].Value, _instanceInfo));
            }
            else if (parsed.Message.StartsWith("[Video Playback] "))
            {
                var content = parsed.Message.Substring(17);
                if ((match = _regexVideoResolved.Match(content)).Success)
                {
                    VideoUrlResolved?.Invoke(
                        this,
                        new VideoUrlResolveEventArgs(
                            parsed.DateTime,
                            match.Groups[1].Value,
                            match.Groups[2].Value,
                            _instanceInfo));
                }
                else if ((match = _regexVideoResolving.Match(content)).Success)
                {
                    VideoUrlResolving?.Invoke(this, new VideoUrlResolveEventArgs(parsed.DateTime, match.Groups[1].Value, _instanceInfo));
                }
            }
            else if ((match = _regexDlString.Match(parsed.Message)).Success)
            {
                Downloaded?.Invoke(
                    this,
                    new DownloadEventArgs(parsed.DateTime, match.Groups[1].Value, DownloadType.String, _instanceInfo));
            }
            else if ((match = _regexDlImage.Match(parsed.Message)).Success)
            {
                Downloaded?.Invoke(
                    this,
                    new DownloadEventArgs(parsed.DateTime, match.Groups[1].Value, DownloadType.Image, _instanceInfo));
            }
            else if (parsed.Message.StartsWith("[Behaviour] Joining wrld_"))
            {
                var instanceString = parsed.Message.Substring(20);
                var tokens = instanceString.Split('~');
                var ids = tokens[0].Split(':');

                var instanceInfo = new InstanceInfo(parsed.DateTime);
                instanceInfo.WorldId = ids[0];
                instanceInfo.InstanceString = instanceString;
                instanceInfo.InstanceId = ids[1];
                instanceInfo.InstanceType = InstanceType.Public;
                instanceInfo.LogFrom = LogFrom;

                // Options
                var canRequestInvite = false;
                foreach (var token in tokens.Skip(1))
                {
                    var optPair = ParseInstanceStringOption(token);
                    switch (optPair.OptionName)
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
                            instanceInfo.UserOrGroupId = optPair.OptionArg;
                            break;
                        case "hidden":
                            instanceInfo.InstanceType = InstanceType.FriendPlus;
                            instanceInfo.UserOrGroupId = optPair.OptionArg;
                            break;
                        case "friends":
                            instanceInfo.InstanceType = InstanceType.Friend;
                            instanceInfo.UserOrGroupId = optPair.OptionArg;
                            break;
                        case "private":
                            instanceInfo.InstanceType = canRequestInvite ? InstanceType.InvitePlus : InstanceType.Invite;
                            instanceInfo.UserOrGroupId = optPair.OptionArg;
                            break;
                        case "region":
                            instanceInfo.Region = optPair.OptionArg switch
                            {
                                "us" => Region.USW,
                                "use" => Region.USE,
                                "eu" => Region.EU,
                                "jp" => Region.JP,
                                _ => throw new InvalidDataException($"Unrecognized region is detected: {optPair.OptionArg}")
                            }; ;
                            break;
                        case "nonce":
                            instanceInfo.Nonce = optPair.OptionArg;
                            break;
                        case "group":
                            instanceInfo.UserOrGroupId = optPair.OptionArg;
                            break;
                        case "groupAccessType":
                            instanceInfo.InstanceType = optPair.OptionArg switch
                            {
                                "public" => InstanceType.GroupPublic,
                                "plus" => InstanceType.GroupPlus,
                                "members" => InstanceType.GroupMembers,
                                _ => instanceInfo.InstanceType
                            };
                            break;
                        default:
                            Console.Error.WriteLine($"Unknown option: {token}");
                            break;
                    }
                }

                _instanceInfo = instanceInfo;
            }
            else if ((match = _regexWorldName.Match(parsed.Message)).Success)
            {
                _instanceInfo.WorldName = match.Groups[1].Value;
                JoinedToInstance?.Invoke(this, new JoinLeaveInstanceEventArgs(parsed.DateTime, _instanceInfo));
            }
            else if ((match = _regexWorldLeft.Match(parsed.Message)).Success)
            {
                _instanceInfo.StayUntil = parsed.DateTime;
                LeftFromInstance?.Invoke(this, new JoinLeaveInstanceEventArgs(parsed.DateTime, _instanceInfo));
                _instanceInfo.IsEmitted = true;
            }
            else if ((match = _regexIdleHomeSave.Match(parsed.Message)).Success)
            {
                IdleHomeSaved?.Invoke(this, new SaveEventArgs(parsed.DateTime, match.Groups[1].Value));
            }
            else if (parsed.Message == TonSaveDataPreamble)
            {
                _isTonSaveData = true;
            }
            else if (_isTonSaveData && (match = _regexTonSave.Match(parsed.Message)).Success)
            {
                _isTonSaveData = false;
                TerrorsOfNowhereSaved?.Invoke(this, new SaveEventArgs(parsed.DateTime, match.Groups[1].Value));
            }

            _lineStack.Clear();
        }

        /// <summary>
        /// Clear line stack and fire <see cref="WarningDetected"/>, <see cref="ErrorDetected"/> or <see cref="ExceptionDetected"/>.
        /// </summary>
        private void EmitLineStack(LogLine parsed)
        {
            switch (parsed.Level)
            {
                case LogLevel.Warning:
                    WarningDetected?.Invoke(this, new ErrorLogEventArgs(parsed.DateTime, parsed.Level, _lineStack));
                    break;
                case LogLevel.Error:
                    ErrorDetected?.Invoke(this, new ErrorLogEventArgs(parsed.DateTime, parsed.Level, _lineStack));
                    break;
                case LogLevel.Exception:
                    ExceptionDetected?.Invoke(this, new ErrorLogEventArgs(parsed.DateTime, parsed.Level, _lineStack));
                    break;
                default:
                    break;
            }

            _lineStack.Clear();
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
        /// Parse one line of log.
        /// </summary>
        /// <param name="line">One line of log.</param>
        /// <returns>Parsed result.</returns>
        private static LogLine ParseLogLine(string line)
        {
            var match = _regexLogLine.Match(line);
            var groups = match.Groups;
            if (!match.Success || groups.Count < 9)
            {
                // return new LogLine(default, LogLevel.Other, string.Empty);
                throw new Exception("Invalid log line: " + line);
            }

            var logLevel = groups[7].Value switch
            {
                "Log" => LogLevel.Log,
                "Warning" => LogLevel.Warning,
                "Error" => LogLevel.Error,
                "Exception" => LogLevel.Exception,
                _ => LogLevel.Other
            };

            return new LogLine(
                new DateTime(
                    int.Parse(groups[1].Value),
                    int.Parse(groups[2].Value),
                    int.Parse(groups[3].Value),
                    int.Parse(groups[4].Value),
                    int.Parse(groups[5].Value),
                    int.Parse(groups[6].Value),
                    DateTimeKind.Utc),
                logLevel,
                groups[8].Value);
        }

        /// <summary>
        /// Parse instance string optipn, "~XXX(YYY)".
        /// </summary>
        /// <param name="option">Option string.</param>
        /// <returns>Parsed result, tuple of optioh name and arguments.</returns>
        /// <exception cref="InvalidDataException">Thrown when mismatch parent detected.</exception>
        private static (string OptionName, string? OptionArg) ParseInstanceStringOption(string option)
        {
            var idxParenStart = option.IndexOf('(');
            if (idxParenStart == -1)
            {
                return (option, null);
            }

            var idxParenEnd = option.IndexOf(')', idxParenStart + 1);
            if (idxParenStart == -1)
            {
                throw new InvalidDataException($"Corresponding parens in instance string are not found.: {option}");
            }

            return (option.Substring(0, idxParenStart), option.Substring(idxParenStart + 1, idxParenEnd - idxParenStart - 1));
            // return (option[..idxParenStart], option[(idxParenStart + 1)..idxParenEnd]);
        }
    }
}
