using System;
using System.Collections.Generic;
using System.IO;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Events;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// VRChat log file parser.
    /// </summary>
    public class VRCLogParser : VRCCoreLogParser
    {
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
        /// Create <see cref="VRCLogParser"/> instance.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for <see cref="FileStream"/> and <see cref="StreamReader"/>.</param>
        public VRCLogParser(string filePath, int bufferSize = 65536)
            : base(filePath, bufferSize)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogParser"/> instance.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="StreamReader"/>.</param>
        public VRCLogParser(Stream stream, int bufferSize = 65536)
            : base(stream, bufferSize)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogParser"/> instance.
        /// </summary>
        /// <param name="reader"><see cref="TextReader"/> of VRChat log file.</param>
        public VRCLogParser(TextReader reader)
            : base(reader)
        {
        }


        /// <summary>
        /// Fire <see cref="JoinedToInstance"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnJoinedToInstance(DateTime logAt, InstanceInfo instanceInfo)
        {
            JoinedToInstance?.Invoke(this, new JoinLeaveInstanceEventArgs(logAt, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="LeftFromInstance"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnLeftFromInstance(DateTime logAt, InstanceInfo instanceInfo)
        {
            LeftFromInstance?.Invoke(this, new JoinLeaveInstanceEventArgs(logAt, instanceInfo));
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
            UserJoined?.Invoke(this, new UserJoinLeaveEventArgs(logAt, userName, stayFrom, instanceInfo));
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
            UserLeft?.Invoke(this, new UserJoinLeaveEventArgs(logAt, userName, stayFrom, stayUntil, instanceInfo));
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
            UserUnregistering?.Invoke(this, new UserJoinLeaveEventArgs(logAt, userName, stayFrom, stayUntil, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="ScreenshotTook"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="filePath">Screenshort file path.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnScreenshotTook(DateTime logAt, string filePath, InstanceInfo instanceInfo)
        {
            ScreenshotTook?.Invoke(this, new ScreenshotTakeEventArgs(logAt, filePath, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="VideoUrlResolving"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="url">Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnVideoUrlResolving(DateTime logAt, string url, InstanceInfo instanceInfo)
        {
            VideoUrlResolving?.Invoke(this, new VideoUrlResolveEventArgs(logAt, url, instanceInfo));
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
            VideoUrlResolved?.Invoke(this, new VideoUrlResolveEventArgs(logAt, url, resolvedUrl, instanceInfo));
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
            Downloaded?.Invoke(this, new DownloadEventArgs(logAt, url, type, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="IdleHomeSaved"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="saveText">Save data text.</param>
        protected override void OnIdleHomeSaved(DateTime logAt, string saveText)
        {
            IdleHomeSaved?.Invoke(this, new SaveEventArgs(logAt, saveText));
        }

        /// <summary>
        /// Fire <see cref="TerrorsOfNowhereSaved"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="saveText">Save data text.</param>
        protected override void OnTerrorsOfNowhereSaved(DateTime logAt, string saveText)
        {
            TerrorsOfNowhereSaved?.Invoke(this, new SaveEventArgs(logAt, saveText));
        }

        /// <summary>
        /// Fire <see cref="RhapsodySaved"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="saveText">Save data text.</param>
        protected override void OnRhapsodySaved(DateTime logAt, string saveText)
        {
            RhapsodySaved?.Invoke(this, new SaveEventArgs(logAt, saveText));
        }

        /// <summary>
        /// Fire <see cref="WarningDetected"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        protected override void OnWarningDetected(DateTime logAt, LogLevel level, List<string> logLines)
        {
            WarningDetected?.Invoke(this, new ErrorLogEventArgs(logAt, level, logLines));
        }

        /// <summary>
        /// Fire <see cref="ErrorDetected"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        protected override void OnErrorDetected(DateTime logAt, LogLevel level, List<string> logLines)
        {
            ErrorDetected?.Invoke(this, new ErrorLogEventArgs(logAt, level, logLines));
        }

        /// <summary>
        /// Fire <see cref="ExceptionDetected"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        protected override void OnExceptionDetected(DateTime logAt, LogLevel level, List<string> logLines)
        {
            ExceptionDetected?.Invoke(this, new ErrorLogEventArgs(logAt, level, logLines));
        }
    }
}
