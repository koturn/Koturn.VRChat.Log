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
    public class VRCLogParser : VRCCoreLogParser, IVRCCoreLogEvent
    {
        /// <inheritdoc/>
        public event VRCLogEventHandler<UserAuthenticatedEventArgs>? UserAuthenticated;
        /// <inheritdoc/>
        public event VRCLogEventHandler<ApplicationQuittedEventArgs>? ApplicationQuitted;
        /// <inheritdoc/>
        public event VRCLogEventHandler<InstanceResetNotifiedEventArgs>? InstanceResetNotified;
        /// <inheritdoc/>
        public event VRCLogEventHandler<InstanceEventArgs>? InstanceClosed;
        /// <inheritdoc/>
        public event VRCLogEventHandler<VRCLogEventArgs>? InstanceClosedByReset;
        /// <inheritdoc/>
        public event VRCLogEventHandler<InstanceEventArgs>? JoinedToInstance;
        /// <inheritdoc/>
        public event VRCLogEventHandler<InstanceEventArgs>? LeftFromInstance;
        /// <inheritdoc/>
        public event VRCLogEventHandler<UserJoinEventArgs>? UserJoined;
        /// <inheritdoc/>
        public event VRCLogEventHandler<UserLeaveEventArgs>? UserLeft;
        /// <inheritdoc/>
        public event VRCLogEventHandler<UserLeaveEventArgs>? UserUnregistering;
        /// <inheritdoc/>
        public event VRCLogEventHandler<ObjectPickedupEventArgs>? ObjectPickedup;
        /// <inheritdoc/>
        public event VRCLogEventHandler<ObjectDroppedEventArgs>? ObjectDropped;
        /// <inheritdoc/>
        public event VRCLogEventHandler<ScreenshotTakeEventArgs>? ScreenshotTook;
        /// <inheritdoc/>
        public event VRCLogEventHandler<VideoUrlResolveEventArgs>? VideoUrlResolving;
        /// <inheritdoc/>
        public event VRCLogEventHandler<VideoUrlResolveEventArgs>? VideoUrlResolved;
        /// <inheritdoc/>
        public event VRCLogEventHandler<DownloadEventArgs>? Downloaded;
        /// <inheritdoc/>
        public event VRCLogEventHandler<ErrorLogEventArgs>? WarningDetected;
        /// <inheritdoc/>
        public event VRCLogEventHandler<ErrorLogEventArgs>? ErrorDetected;
        /// <inheritdoc/>
        public event VRCLogEventHandler<ErrorLogEventArgs>? ExceptionDetected;


        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified file path, then initialize instance with it.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        public VRCLogParser(string filePath)
            : base(filePath)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified file path and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/> and internal <see cref="FileStream"/> of <see cref="VRCLogReader"/>.</param>
        public VRCLogParser(string filePath, int bufferSize)
            : base(filePath, bufferSize)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/>, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        public VRCLogParser(Stream stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/> and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/>.</param>
        public VRCLogParser(Stream stream, int bufferSize)
            : base(stream, bufferSize)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/>, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCLogParser"/> object is disposed; otherwise, false.</param>
        public VRCLogParser(Stream stream, bool leaveOpen)
            : base(stream, leaveOpen)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/> and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/>.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCLogParser"/> object is disposed; otherwise, false.</param>
        public VRCLogParser(Stream stream, int bufferSize, bool leaveOpen)
            : base(stream, bufferSize, leaveOpen)
        {
        }

        /// <summary>
        /// Initialize instance with specified <see cref="VRCLogReader"/>.
        /// </summary>
        /// <param name="logReader">VRChat Log Reader.</param>
        public VRCLogParser(VRCLogReader logReader)
            : base(logReader)
        {
        }

        /// <summary>
        /// Initialize instance with specified <see cref="VRCLogReader"/>.
        /// </summary>
        /// <param name="logReader">VRChat Log Reader.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="logReader"/> open
        /// after the <see cref="VRCLogParser"/> object is disposed; otherwise, false.</param>
        public VRCLogParser(VRCLogReader logReader, bool leaveOpen)
            : base(logReader, leaveOpen)
        {
        }


        /// <summary>
        /// Fire <see cref="UserAuthenticated"/> event.
        /// </summary>
        /// <param name="authUserInfo">Authenticated user information.</param>
        protected override void OnUserAuthenticated(AuthUserInfo authUserInfo)
        {
            UserAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs(FileName, LogUntil, authUserInfo));
        }

        /// <summary>
        /// Fire <see cref="ApplicationQuitted"/> event.
        /// </summary>
        /// <param name="activeTime">Active time (in seconds).</param>
        protected override void OnApplicationQuit(double activeTime)
        {
            ApplicationQuitted?.Invoke(this, new ApplicationQuittedEventArgs(FileName, LogUntil, activeTime));
        }

        /// <summary>
        /// Fire <see cref="InstanceResetNotified"/> event.
        /// </summary>
        /// <param name="closeMinutes">Time until instance is reset (minutes).</param>
        protected override void OnInstanceResetNotified(int closeMinutes)
        {
            InstanceResetNotified?.Invoke(this, new InstanceResetNotifiedEventArgs(FileName, LogUntil, closeMinutes));
        }

        /// <summary>
        /// This method is called when instance closed log is detected.
        /// </summary>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnInstanceClosed(InstanceInfo instanceInfo)
        {
            InstanceClosed?.Invoke(this, new InstanceEventArgs(FileName, LogUntil, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="InstanceClosedByReset"/> event.
        /// </summary>
        protected override void OnInstanceClosedByReset()
        {
            InstanceClosedByReset?.Invoke(this, new VRCLogEventArgs(FileName, LogUntil));
        }

        /// <summary>
        /// Fire <see cref="JoinedToInstance"/> event.
        /// </summary>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnJoinedToInstance(InstanceInfo instanceInfo)
        {
            JoinedToInstance?.Invoke(this, new InstanceEventArgs(FileName, LogUntil, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="LeftFromInstance"/> event.
        /// </summary>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnLeftFromInstance(InstanceInfo instanceInfo)
        {
            LeftFromInstance?.Invoke(this, new InstanceEventArgs(FileName, LogUntil, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="UserJoined"/> event.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnUserJoined(string userName, string? userId, DateTime stayFrom, InstanceInfo instanceInfo)
        {
            UserJoined?.Invoke(this, new UserJoinEventArgs(FileName, LogUntil, userName, userId, stayFrom, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="UserLeft"/> event.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="stayUntil">A timestamp the user left.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnUserLeft(string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
        {
            UserLeft?.Invoke(this, new UserLeaveEventArgs(FileName, LogUntil, userName, userId, stayFrom, stayUntil, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="UserUnregistering"/> event.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="stayUntil">A timestamp the user left.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnUserUnregistering(string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
        {
            UserUnregistering?.Invoke(this, new UserLeaveEventArgs(FileName, LogUntil, userName, userId, stayFrom, stayUntil, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="ObjectPickedup"/> event.
        /// </summary>
        /// <param name="objectName">Pickedup object name.</param>
        /// <param name="isEquipped">True if equipped.</param>
        /// <param name="isAutoEquipType">True if the object is auto equip type.</param>
        /// <param name="lastInputMethod">Last input method name.</param>
        /// <param name="isAutoHoldEnabled">True if auto hold is enabled for the controller.</param>
        protected override void OnPickupObject(string objectName, bool isEquipped, bool isAutoEquipType, string lastInputMethod, bool isAutoHoldEnabled)
        {
            ObjectPickedup?.Invoke(this, new ObjectPickedupEventArgs(FileName, LogUntil, objectName, isEquipped, isAutoEquipType, lastInputMethod, isAutoHoldEnabled));
        }

        /// <summary>
        /// Fire <see cref="ObjectDropped"/> event.
        /// </summary>
        /// <param name="objectName">Pickedup object name.</param>
        /// <param name="isEquipped">True if the object was equipped.</param>
        /// <param name="dropReason">Reason for dropping the object.</param>
        /// <param name="lastInputMethod">Last input method name.</param>
        protected override void OnDropObject(string objectName, bool isEquipped, string dropReason, string lastInputMethod)
        {
            ObjectDropped?.Invoke(this, new ObjectDroppedEventArgs(FileName, LogUntil, objectName, isEquipped, dropReason, lastInputMethod));
        }

        /// <summary>
        /// Fire <see cref="ScreenshotTook"/> event.
        /// </summary>
        /// <param name="filePath">Screenshort file path.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnScreenshotTook(string filePath, InstanceInfo instanceInfo)
        {
            ScreenshotTook?.Invoke(this, new ScreenshotTakeEventArgs(FileName, LogUntil, filePath, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="VideoUrlResolving"/> event.
        /// </summary>
        /// <param name="url">Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnVideoUrlResolving(string url, InstanceInfo instanceInfo)
        {
            VideoUrlResolving?.Invoke(this, new VideoUrlResolveEventArgs(FileName, LogUntil, url, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="VideoUrlResolved"/> event.
        /// </summary>
        /// <param name="url">Video URL.</param>
        /// <param name="resolvedUrl">Resolved Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnVideoUrlResolved(string url, string resolvedUrl, InstanceInfo instanceInfo)
        {
            VideoUrlResolved?.Invoke(this, new VideoUrlResolveEventArgs(FileName, LogUntil, url, resolvedUrl, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="Downloaded"/> event.
        /// </summary>
        /// <param name="url">Download URL.</param>
        /// <param name="type"></param>
        /// <param name="instanceInfo"></param>
        protected override void OnDownloaded(string url, DownloadType type, InstanceInfo instanceInfo)
        {
            Downloaded?.Invoke(this, new DownloadEventArgs(FileName, LogUntil, url, type, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="WarningDetected"/> event.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        protected override void OnWarningDetected(VRCLogLevel level, List<string> logLines)
        {
            WarningDetected?.Invoke(this, new ErrorLogEventArgs(FileName, LogUntil, level, logLines));
        }

        /// <summary>
        /// Fire <see cref="ErrorDetected"/> event.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        protected override void OnErrorDetected(VRCLogLevel level, List<string> logLines)
        {
            ErrorDetected?.Invoke(this, new ErrorLogEventArgs(FileName, LogUntil, level, logLines));
        }

        /// <summary>
        /// Fire <see cref="ExceptionDetected"/> event.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        protected override void OnExceptionDetected(VRCLogLevel level, List<string> logLines)
        {
            ExceptionDetected?.Invoke(this, new ErrorLogEventArgs(FileName, LogUntil, level, logLines));
        }
    }
}
