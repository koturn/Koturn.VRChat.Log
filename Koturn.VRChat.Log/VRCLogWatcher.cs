using System;
using System.Collections.Generic;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Events;
using Koturn.VRChat.Log.Internals;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Log Watcher class.
    /// </summary>
    public class VRCLogWatcher : VRCBaseLogWatcher, IVRCCoreLogEvent
    {
        /// <summary>
        /// First timestamp of current log file.
        /// </summary>
        public DateTime CurrentLogFrom { get; protected set; }
        /// <summary>
        /// Last timestamp of current log file.
        /// </summary>
        public DateTime CurrentLogUntil { get; protected set; }
        /// <summary>
        /// Authenticated user information.
        /// </summary>
        public AuthUserInfo? AuthUserInfo { get; protected set; }


        /// <inheritdoc/>
        public event VRCLogEventHandler<UserAuthenticatedEventArgs>? UserAuthenticated
        {
            add => EventHelper.Add(ref _userAuthenticated, value);
            remove => EventHelper.Remove(ref _userAuthenticated, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<ApplicationQuittedEventArgs>? ApplicationQuitted
        {
            add => EventHelper.Add(ref _applicationQuitted, value);
            remove => EventHelper.Remove(ref _applicationQuitted, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<InstanceResetNotifiedEventArgs>? InstanceResetNotified
        {
            add => EventHelper.Add(ref _instanceCloseNotified, value);
            remove => EventHelper.Remove(ref _instanceCloseNotified, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<InstanceEventArgs>? InstanceClosed
        {
            add => EventHelper.Add(ref _instanceClosed, value);
            remove => EventHelper.Remove(ref _instanceClosed, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<LogEventArgs>? InstanceClosedByReset
        {
            add => EventHelper.Add(ref _instanceClosedByReset, value);
            remove => EventHelper.Remove(ref _instanceClosedByReset, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<InstanceEventArgs>? JoinedToInstance
        {
            add => EventHelper.Add(ref _joinedToInstance, value);
            remove => EventHelper.Remove(ref _joinedToInstance, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<InstanceEventArgs>? LeftFromInstance
        {
            add => EventHelper.Add(ref _leftFromInstance, value);
            remove => EventHelper.Remove(ref _leftFromInstance, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<UserJoinLeaveEventArgs>? UserJoined
        {
            add => EventHelper.Add(ref _userJoined, value);
            remove => EventHelper.Remove(ref _userJoined, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<UserJoinLeaveEventArgs>? UserLeft
        {
            add => EventHelper.Add(ref _userLeft, value);
            remove => EventHelper.Remove(ref _userLeft, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<UserJoinLeaveEventArgs>? UserUnregistering
        {
            add => EventHelper.Add(ref _userUnregistering, value);
            remove => EventHelper.Remove(ref _userUnregistering, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<ObjectPickedupEventArgs>? ObjectPickedup
        {
            add => EventHelper.Add(ref _objectPickedup, value);
            remove => EventHelper.Remove(ref _objectPickedup, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<ObjectDroppedEventArgs>? ObjectDropped
        {
            add => EventHelper.Add(ref _objectDropped, value);
            remove => EventHelper.Remove(ref _objectDropped, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<ScreenshotTakeEventArgs>? ScreenshotTook
        {
            add => EventHelper.Add(ref _screenshotTook, value);
            remove => EventHelper.Remove(ref _screenshotTook, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<VideoUrlResolveEventArgs>? VideoUrlResolving
        {
            add => EventHelper.Add(ref _videoUrlResolving, value);
            remove => EventHelper.Remove(ref _videoUrlResolving, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<VideoUrlResolveEventArgs>? VideoUrlResolved
        {
            add => EventHelper.Add(ref _videoUrlResolved, value);
            remove => EventHelper.Remove(ref _videoUrlResolved, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<DownloadEventArgs>? Downloaded
        {
            add => EventHelper.Add(ref _downloaded, value);
            remove => EventHelper.Remove(ref _downloaded, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<ErrorLogEventArgs>? WarningDetected
        {
            add => EventHelper.Add(ref _warningDetected, value);
            remove => EventHelper.Remove(ref _warningDetected, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<ErrorLogEventArgs>? ErrorDetected
        {
            add => EventHelper.Add(ref _errorDetected, value);
            remove => EventHelper.Remove(ref _errorDetected, value);
        }
        /// <inheritdoc/>
        public event VRCLogEventHandler<ErrorLogEventArgs>? ExceptionDetected
        {
            add => EventHelper.Add(ref _exceptionDetected, value);
            remove => EventHelper.Remove(ref _exceptionDetected, value);
        }


        /// <summary>
        /// The substance event handler delegate of <see cref="UserAuthenticated"/>.
        /// </summary>
        protected VRCLogEventHandler<UserAuthenticatedEventArgs>? _userAuthenticated;
        /// <summary>
        /// The substance event handler delegate of <see cref="ApplicationQuitted"/>.
        /// </summary>
        protected VRCLogEventHandler<ApplicationQuittedEventArgs>? _applicationQuitted;
        /// <summary>
        /// The substance event handler delegate of <see cref="InstanceResetNotified"/>.
        /// </summary>
        protected VRCLogEventHandler<InstanceResetNotifiedEventArgs>? _instanceCloseNotified;
        /// <summary>
        /// The substance event handler delegate of <see cref="InstanceClosed"/>.
        /// </summary>
        protected VRCLogEventHandler<InstanceEventArgs>? _instanceClosed;
        /// <summary>
        /// The substance event handler delegate of <see cref="InstanceClosedByReset"/>.
        /// </summary>
        protected VRCLogEventHandler<LogEventArgs>? _instanceClosedByReset;
        /// <summary>
        /// The substance event handler delegate of <see cref="JoinedToInstance"/>.
        /// </summary>
        protected VRCLogEventHandler<InstanceEventArgs>? _joinedToInstance;
        /// <summary>
        /// The substance event handler delegate of <see cref="LeftFromInstance"/>.
        /// </summary>
        protected VRCLogEventHandler<InstanceEventArgs>? _leftFromInstance;
        /// <summary>
        /// The substance event handler delegate of <see cref="UserJoined"/>.
        /// </summary>
        protected VRCLogEventHandler<UserJoinLeaveEventArgs>? _userJoined;
        /// <summary>
        /// The substance event handler delegate of <see cref="UserLeft"/>.
        /// </summary>
        protected VRCLogEventHandler<UserJoinLeaveEventArgs>? _userLeft;
        /// <summary>
        /// The substance event handler delegate of <see cref="UserUnregistering"/>.
        /// </summary>
        protected VRCLogEventHandler<UserJoinLeaveEventArgs>? _userUnregistering;
        /// <summary>
        /// The substance event handler delegate of <see cref="ObjectPickedup"/>.
        /// </summary>
        protected VRCLogEventHandler<ObjectPickedupEventArgs>? _objectPickedup;
        /// <summary>
        /// The substance event handler delegate of <see cref="ObjectDropped"/>.
        /// </summary>
        protected VRCLogEventHandler<ObjectDroppedEventArgs>? _objectDropped;
        /// <summary>
        /// The substance event handler delegate of <see cref="ScreenshotTook"/>.
        /// </summary>
        protected VRCLogEventHandler<ScreenshotTakeEventArgs>? _screenshotTook;
        /// <summary>
        /// The substance event handler delegate of <see cref="VideoUrlResolving"/>.
        /// </summary>
        protected VRCLogEventHandler<VideoUrlResolveEventArgs>? _videoUrlResolving;
        /// <summary>
        /// The substance event handler delegate of <see cref="VideoUrlResolved"/>.
        /// </summary>
        protected VRCLogEventHandler<VideoUrlResolveEventArgs>? _videoUrlResolved;
        /// <summary>
        /// The substance event handler delegate of <see cref="Downloaded"/>.
        /// </summary>
        protected VRCLogEventHandler<DownloadEventArgs>? _downloaded;
        /// <summary>
        /// The substance event handler delegate of <see cref="WarningDetected"/>.
        /// </summary>
        protected VRCLogEventHandler<ErrorLogEventArgs>? _warningDetected;
        /// <summary>
        /// The substance event handler delegate of <see cref="ErrorDetected"/>.
        /// </summary>
        protected VRCLogEventHandler<ErrorLogEventArgs>? _errorDetected;
        /// <summary>
        /// The substance event handler delegate of <see cref="ExceptionDetected"/>.
        /// </summary>
        protected VRCLogEventHandler<ErrorLogEventArgs>? _exceptionDetected;


        /// <summary>
        /// Create <see cref="VRCLogWatcher"/> instance.
        /// </summary>
        public VRCLogWatcher()
            : this(InternalDefaultWatchCycle)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogWatcher"/> instance.
        /// </summary>
        /// <param name="watchCycle">File watch cycle.</param>
        public VRCLogWatcher(int watchCycle)
            : base(watchCycle)
        {
        }


        /// <summary>
        /// Create <see cref="VRCWatcherLogParser"/> instance with specified file path.
        /// </summary>
        /// <param name="filePath">File path to parse.</param>
        /// <returns>Created <see cref="VRCWatcherLogParser"/> instance.</returns>
        protected override VRCBaseLogParser CreateLogParser(string filePath)
        {
            CurrentLogFrom = default;
            CurrentLogUntil = default;
            AuthUserInfo = null;
            return new VRCWatcherLogParser(filePath, this);
        }


        /// <summary>
        /// VRChat log file parser for <see cref="VRCLogWatcher"/>.
        /// </summary>
        private sealed class VRCWatcherLogParser : VRCCoreLogParser
        {
            /// <summary>
            /// Reference to <see cref="VRCLogWatcher"/> instance.
            /// </summary>
            private readonly VRCLogWatcher _logWatcher;

            /// <summary>
            /// Create <see cref="VRCWatcherLogParser"/> instance.
            /// </summary>
            /// <param name="filePath">VRChat log file path.</param>
            /// <param name="logWatcher"><see cref="VRCLogWatcher"/> instance.</param>
            public VRCWatcherLogParser(string filePath, VRCLogWatcher logWatcher)
                : base(filePath)
            {
                _logWatcher = logWatcher;
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
                var logWatcher = _logWatcher;
                if (logWatcher.CurrentLogFrom == default)
                {
                    logWatcher.CurrentLogFrom = logAt;
                }
                logWatcher.CurrentLogUntil = logAt;
                return base.OnLogDetected(logAt, level, logLines);
            }

            /// <summary>
            /// Set <see cref="VRCLogWatcher.AuthUserInfo"/>.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="authUserInfo">Authenticated user information.</param>
            protected override void OnUserAuthenticated(DateTime logAt, AuthUserInfo authUserInfo)
            {
                _logWatcher.AuthUserInfo = authUserInfo;
                _logWatcher._userAuthenticated?.Invoke(_logWatcher, new UserAuthenticatedEventArgs(logAt, authUserInfo));
            }

            /// <summary>
            /// Fire <see cref="ApplicationQuitted"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="activeTime">Active time (in seconds).</param>
            protected override void OnApplicationQuit(DateTime logAt, double activeTime)
            {
                _logWatcher._applicationQuitted?.Invoke(_logWatcher, new ApplicationQuittedEventArgs(logAt, activeTime));
            }

            /// <summary>
            /// Fire <see cref="InstanceResetNotified"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="closeMinutes">Time until instance is reset (minutes).</param>
            protected override void OnInstanceResetNotified(DateTime logAt, int closeMinutes)
            {
                _logWatcher._instanceCloseNotified?.Invoke(_logWatcher, new InstanceResetNotifiedEventArgs(logAt, closeMinutes));
            }

            /// <summary>
            /// Fire <see cref="InstanceClosed"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnInstanceClosed(DateTime logAt, InstanceInfo instanceInfo)
            {
                _logWatcher._instanceClosed?.Invoke(_logWatcher, new InstanceEventArgs(logAt, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="InstanceClosedByReset"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            protected override void OnInstanceClosedByReset(DateTime logAt)
            {
                _logWatcher._instanceClosedByReset?.Invoke(_logWatcher, new LogEventArgs(logAt));
            }

            /// <summary>
            /// Fire <see cref="JoinedToInstance"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnJoinedToInstance(DateTime logAt, InstanceInfo instanceInfo)
            {
                _logWatcher._joinedToInstance?.Invoke(_logWatcher, new InstanceEventArgs(logAt, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="LeftFromInstance"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnLeftFromInstance(DateTime logAt, InstanceInfo instanceInfo)
            {
                _logWatcher._leftFromInstance?.Invoke(_logWatcher, new InstanceEventArgs(logAt, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="UserJoined"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="userName">User name.</param>
            /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserJoined(DateTime logAt, string userName, string? userId, DateTime stayFrom, InstanceInfo instanceInfo)
            {
                _logWatcher._userJoined?.Invoke(_logWatcher, new UserJoinLeaveEventArgs(logAt, userName, userId, stayFrom, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="UserLeft"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="userName">User name.</param>
            /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="stayUntil">A timestamp the user left.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserLeft(DateTime logAt, string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
            {
                _logWatcher._userLeft?.Invoke(_logWatcher, new UserJoinLeaveEventArgs(logAt, userName, userId, stayFrom, stayUntil, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="UserUnregistering"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="userName">User name.</param>
            /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="stayUntil">A timestamp the user left.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserUnregistering(DateTime logAt, string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
            {
                _logWatcher._userUnregistering?.Invoke(_logWatcher, new UserJoinLeaveEventArgs(logAt, userName, userId, stayFrom, stayUntil, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="ObjectPickedup"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="objectName">Pickedup object name.</param>
            /// <param name="isEquipped">True if equipped.</param>
            /// <param name="isEquippable">True if the object is equippable.</param>
            /// <param name="lastInputMethod">Last input method name.</param>
            /// <param name="isAutoEquipController">True if the object is auto equip controller.</param>
            protected override void OnPickupObject(DateTime logAt, string objectName, bool isEquipped, bool isEquippable, string lastInputMethod, bool isAutoEquipController)
            {
                _logWatcher._objectPickedup?.Invoke(_logWatcher, new ObjectPickedupEventArgs(logAt, objectName, isEquipped, isEquippable, lastInputMethod, isAutoEquipController));
            }

            /// <summary>
            /// Fire <see cref="ObjectDropped"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="objectName">Pickedup object name.</param>
            /// <param name="isEquipped">True if the object was equipped.</param>
            /// <param name="dropReason">Reason for dropping the object.</param>
            /// <param name="lastInputMethod">Last input method name.</param>
            protected override void OnDropObject(DateTime logAt, string objectName, bool isEquipped, string dropReason, string lastInputMethod)
            {
                _logWatcher._objectDropped?.Invoke(_logWatcher, new ObjectDroppedEventArgs(logAt, objectName, isEquipped, dropReason, lastInputMethod));
            }

            /// <summary>
            /// Fire <see cref="ScreenshotTook"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="filePath">Screenshort file path.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnScreenshotTook(DateTime logAt, string filePath, InstanceInfo instanceInfo)
            {
                _logWatcher._screenshotTook?.Invoke(_logWatcher, new ScreenshotTakeEventArgs(logAt, filePath, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VideoUrlResolving"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="url">Video URL.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnVideoUrlResolving(DateTime logAt, string url, InstanceInfo instanceInfo)
            {
                _logWatcher._videoUrlResolving?.Invoke(_logWatcher, new VideoUrlResolveEventArgs(logAt, url, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VideoUrlResolved"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="url">Video URL.</param>
            /// <param name="resolvedUrl">Resolved Video URL.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnVideoUrlResolved(DateTime logAt, string url, string resolvedUrl, InstanceInfo instanceInfo)
            {
                _logWatcher._videoUrlResolved?.Invoke(_logWatcher, new VideoUrlResolveEventArgs(logAt, url, resolvedUrl, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="Downloaded"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="url">Download URL.</param>
            /// <param name="type"></param>
            /// <param name="instanceInfo"></param>
            protected override void OnDownloaded(DateTime logAt, string url, DownloadType type, InstanceInfo instanceInfo)
            {
                _logWatcher._downloaded?.Invoke(_logWatcher, new DownloadEventArgs(logAt, url, type, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="WarningDetected"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnWarningDetected(DateTime logAt, LogLevel level, List<string> logLines)
            {
                _logWatcher._warningDetected?.Invoke(_logWatcher, new ErrorLogEventArgs(logAt, level, logLines));
            }

            /// <summary>
            /// Fire <see cref="ErrorDetected"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnErrorDetected(DateTime logAt, LogLevel level, List<string> logLines)
            {
                _logWatcher._errorDetected?.Invoke(_logWatcher, new ErrorLogEventArgs(logAt, level, logLines));
            }

            /// <summary>
            /// Fire <see cref="ExceptionDetected"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnExceptionDetected(DateTime logAt, LogLevel level, List<string> logLines)
            {
                _logWatcher._exceptionDetected?.Invoke(_logWatcher, new ErrorLogEventArgs(logAt, level, logLines));
            }
        }
    }
}
