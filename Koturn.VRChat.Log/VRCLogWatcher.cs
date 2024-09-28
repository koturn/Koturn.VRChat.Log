#define USE_WIN32_API

using System;
using System.Collections.Generic;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Events;


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
        public DateTime CurrentLogFrom { get; private set; }
        /// <summary>
        /// Last timestamp of current log file.
        /// </summary>
        public DateTime CurrentLogUntil { get; private set; }

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
        /// Occurs when detect a log that object pickedup.
        /// </summary>
        public event EventHandler<ObjectPickedupEventArgs>? ObjectPickedup;
        /// <summary>
        /// Occurs when detect a log that object dropped.
        /// </summary>
        public event EventHandler<ObjectDroppedEventArgs>? ObjectDropped;
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
        /// Occurs when detect a log that save data text of Idle Defense is generated.
        /// </summary>
        public event EventHandler<SaveEventArgs>? IdleDefenseSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Terrors of Nowhere is generated.
        /// </summary>
        public event EventHandler<SaveEventArgs>? TerrorsOfNowhereSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Rhapsody is generated.
        /// </summary>
        public event EventHandler<SaveEventArgs>? RhapsodySaved;
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
        /// Create <see cref="VRCLogWatcher"/> instance.
        /// </summary>
        public VRCLogWatcher()
            : this(1000)
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
                if (logWatcher.CurrentLogFrom != default)
                {
                    logWatcher.CurrentLogFrom = logAt;
                }
                logWatcher.CurrentLogUntil = logAt;
                return base.OnLogDetected(logAt, level, logLines);
            }

            /// <summary>
            /// Fire <see cref="JoinedToInstance"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnJoinedToInstance(DateTime logAt, InstanceInfo instanceInfo)
            {
                _logWatcher.JoinedToInstance?.Invoke(this, new JoinLeaveInstanceEventArgs(logAt, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="LeftFromInstance"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnLeftFromInstance(DateTime logAt, InstanceInfo instanceInfo)
            {
                _logWatcher.LeftFromInstance?.Invoke(this, new JoinLeaveInstanceEventArgs(logAt, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="UserJoined"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="userName">User name.</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserJoined(DateTime logAt, string userName, DateTime stayFrom, InstanceInfo instanceInfo)
            {
                _logWatcher.UserJoined?.Invoke(this, new UserJoinLeaveEventArgs(logAt, userName, stayFrom, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="UserLeft"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="userName">User name.</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="stayUntil">A timestamp the user left.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserLeft(DateTime logAt, string userName, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
            {
                _logWatcher.UserLeft?.Invoke(this, new UserJoinLeaveEventArgs(logAt, userName, stayFrom, stayUntil, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="UserUnregistering"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="userName">User name.</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="stayUntil">A timestamp the user left.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserUnregistering(DateTime logAt, string userName, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
            {
                _logWatcher.UserUnregistering?.Invoke(this, new UserJoinLeaveEventArgs(logAt, userName, stayFrom, stayUntil, instanceInfo));
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
                _logWatcher.ObjectPickedup?.Invoke(this, new ObjectPickedupEventArgs(logAt, objectName, isEquipped, isEquippable, lastInputMethod, isAutoEquipController));
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
                _logWatcher.ObjectDropped?.Invoke(this, new ObjectDroppedEventArgs(logAt, objectName, isEquipped, dropReason, lastInputMethod));
            }

            /// <summary>
            /// Fire <see cref="ScreenshotTook"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="filePath">Screenshort file path.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnScreenshotTook(DateTime logAt, string filePath, InstanceInfo instanceInfo)
            {
                _logWatcher.ScreenshotTook?.Invoke(this, new ScreenshotTakeEventArgs(logAt, filePath, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VideoUrlResolving"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="url">Video URL.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnVideoUrlResolving(DateTime logAt, string url, InstanceInfo instanceInfo)
            {
                _logWatcher.VideoUrlResolving?.Invoke(this, new VideoUrlResolveEventArgs(logAt, url, instanceInfo));
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
                _logWatcher.VideoUrlResolved?.Invoke(this, new VideoUrlResolveEventArgs(logAt, url, resolvedUrl, instanceInfo));
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
                _logWatcher.Downloaded?.Invoke(this, new DownloadEventArgs(logAt, url, type, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="IdleHomeSaved"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="saveText">Save data text.</param>
            protected override void OnIdleHomeSaved(DateTime logAt, string saveText)
            {
                _logWatcher.IdleHomeSaved?.Invoke(this, new SaveEventArgs(logAt, saveText));
            }

            /// <summary>
            /// Fire <see cref="IdleDefenseSaved"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="saveText">Save data text.</param>
            protected override void OnIdleDefenseSaved(DateTime logAt, string saveText)
            {
                _logWatcher.IdleDefenseSaved?.Invoke(this, new SaveEventArgs(logAt, saveText));
            }

            /// <summary>
            /// Fire <see cref="TerrorsOfNowhereSaved"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="saveText">Save data text.</param>
            protected override void OnTerrorsOfNowhereSaved(DateTime logAt, string saveText)
            {
                _logWatcher.TerrorsOfNowhereSaved?.Invoke(this, new SaveEventArgs(logAt, saveText));
            }

            /// <summary>
            /// Fire <see cref="RhapsodySaved"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="saveText">Save data text.</param>
            protected override void OnRhapsodySaved(DateTime logAt, string saveText)
            {
                _logWatcher.RhapsodySaved?.Invoke(this, new SaveEventArgs(logAt, saveText));
            }

            /// <summary>
            /// Fire <see cref="WarningDetected"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnWarningDetected(DateTime logAt, LogLevel level, List<string> logLines)
            {
                _logWatcher.WarningDetected?.Invoke(this, new ErrorLogEventArgs(logAt, level, logLines));
            }

            /// <summary>
            /// Fire <see cref="ErrorDetected"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnErrorDetected(DateTime logAt, LogLevel level, List<string> logLines)
            {
                _logWatcher.ErrorDetected?.Invoke(this, new ErrorLogEventArgs(logAt, level, logLines));
            }

            /// <summary>
            /// Fire <see cref="ExceptionDetected"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnExceptionDetected(DateTime logAt, LogLevel level, List<string> logLines)
            {
                _logWatcher.ExceptionDetected?.Invoke(this, new ErrorLogEventArgs(logAt, level, logLines));
            }
        }
    }
}
