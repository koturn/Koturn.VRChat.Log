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
    public class VRCExLogParser : VRCCoreExLogParser, IVRCCoreLogEvent, IVRCExLogEvent
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
        /// <inheritdoc/>
        public event VRCLogEventHandler<SaveEventArgs>? BulletTimeAgentSaved;
        /// <inheritdoc/>
        public event VRCLogEventHandler<SaveEventArgs>? IdleCubeSaved;
        /// <inheritdoc/>
        public event VRCLogEventHandler<SaveEventArgs>? IdleDefenseSaved;
        /// <inheritdoc/>
        public event VRCLogEventHandler<SaveEventArgs>? IdleHomeSaved;
        /// <inheritdoc/>
        public event VRCLogEventHandler<SaveEventArgs>? MagicalCursedLandSaved;
        /// <inheritdoc/>
        public event VRCLogEventHandler<SaveEventArgs>? RhapsodySaved;
        /// <inheritdoc/>
        public event VRCLogEventHandler<TonKillerNameEventArgs>? TonKillerTargetChanged;
        /// <inheritdoc/>
        public event VRCLogEventHandler<TonPlayerDeadEventArgs>? TonPlayerDead;
        /// <inheritdoc/>
        public event VRCLogEventHandler<TonPlayerDamagedEventArgs>? TonPlayerDamaged;
        /// <inheritdoc/>
        public event VRCLogEventHandler<TonKillerNameEventArgs>? TonKillerStunned;
        /// <inheritdoc/>
        public event VRCLogEventHandler<TonKillerEnragedEventArgs>? TonKillerEnraged;
        /// <inheritdoc/>
        public event VRCLogEventHandler<TonKillerSetEventArgs>? TonKillerSet;
        /// <inheritdoc/>
        public event VRCLogEventHandler<TonKillerUnlockedEventArgs>? TonKillerUnlocked;
        /// <inheritdoc/>
        public event VRCLogEventHandler<TonEquipEventArgs>? TonEquipped;
        /// <inheritdoc/>
        public event VRCLogEventHandler<TonRoundStartedEventArgs>? TonRoundStarted;
        /// <inheritdoc/>
        public event VRCLogEventHandler<TonRoundFinishedEventArgs>? TonRoundFinished;
        /// <inheritdoc/>
        public event VRCLogEventHandler<SaveEventArgs>? TerrorsOfNowhereSaved;


        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified file path, then initialize instance with it.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        public VRCExLogParser(string filePath)
            : base(filePath)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified file path and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/> and internal <see cref="FileStream"/> of <see cref="VRCLogReader"/>.</param>
        public VRCExLogParser(string filePath, int bufferSize)
            : base(filePath, bufferSize)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/>, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        public VRCExLogParser(Stream stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/> and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/>.</param>
        public VRCExLogParser(Stream stream, int bufferSize)
            : base(stream, bufferSize)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/>, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCExLogParser"/> object is disposed; otherwise, false.</param>
        public VRCExLogParser(Stream stream, bool leaveOpen)
            : base(stream, leaveOpen)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogReader"/> with specified <see cref="Stream"/> and buffer size, then initialize instance with it.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="VRCLogReader"/>.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCExLogParser"/> object is disposed; otherwise, false.</param>
        public VRCExLogParser(Stream stream, int bufferSize, bool leaveOpen)
            : base(stream, bufferSize, leaveOpen)
        {
        }

        /// <summary>
        /// Initialize instance with specified <see cref="VRCLogReader"/>.
        /// </summary>
        /// <param name="logReader">VRChat Log Reader.</param>
        public VRCExLogParser(VRCLogReader logReader)
            : base(logReader)
        {
        }

        /// <summary>
        /// Initialize instance with specified <see cref="VRCLogReader"/>.
        /// </summary>
        /// <param name="logReader">VRChat Log Reader.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="logReader"/> open
        /// after the <see cref="VRCExLogParser"/> object is disposed; otherwise, false.</param>
        public VRCExLogParser(VRCLogReader logReader, bool leaveOpen)
            : base(logReader, leaveOpen)
        {
        }


        /// <summary>
        /// Fire <see cref="UserAuthenticated"/> event.
        /// </summary>
        /// <param name="authUserInfo">Authenticated user information.</param>
        protected override void OnUserAuthenticated(AuthUserInfo authUserInfo)
        {
            UserAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs(FilePath, LogUntil, authUserInfo));
        }

        /// <summary>
        /// Fire <see cref="ApplicationQuitted"/> event.
        /// </summary>
        /// <param name="activeTime">Active time (in seconds).</param>
        protected override void OnApplicationQuit(double activeTime)
        {
            ApplicationQuitted?.Invoke(this, new ApplicationQuittedEventArgs(FilePath, LogUntil, activeTime));
        }

        /// <summary>
        /// Fire <see cref="InstanceResetNotified"/> event.
        /// </summary>
        /// <param name="closeMinutes">Time until instance is reset (minutes).</param>
        protected override void OnInstanceResetNotified(int closeMinutes)
        {
            InstanceResetNotified?.Invoke(this, new InstanceResetNotifiedEventArgs(FilePath, LogUntil, closeMinutes));
        }

        /// <summary>
        /// This method is called when instance closed log is detected.
        /// </summary>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnInstanceClosed(InstanceInfo instanceInfo)
        {
            InstanceClosed?.Invoke(this, new InstanceEventArgs(FilePath, LogUntil, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="InstanceClosedByReset"/> event.
        /// </summary>
        protected override void OnInstanceClosedByReset()
        {
            InstanceClosedByReset?.Invoke(this, new VRCLogEventArgs(FilePath, LogUntil));
        }

        /// <summary>
        /// Fire <see cref="JoinedToInstance"/> event.
        /// </summary>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnJoinedToInstance(InstanceInfo instanceInfo)
        {
            base.OnJoinedToInstance(instanceInfo);
            JoinedToInstance?.Invoke(this, new InstanceEventArgs(FilePath, LogUntil, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="LeftFromInstance"/> event.
        /// </summary>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnLeftFromInstance(InstanceInfo instanceInfo)
        {
            LeftFromInstance?.Invoke(this, new InstanceEventArgs(FilePath, LogUntil, instanceInfo));
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
            UserJoined?.Invoke(this, new UserJoinEventArgs(FilePath, LogUntil, userName, userId, stayFrom, instanceInfo));
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
            UserLeft?.Invoke(this, new UserLeaveEventArgs(FilePath, LogUntil, userName, userId, stayFrom, stayUntil, instanceInfo));
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
            UserUnregistering?.Invoke(this, new UserLeaveEventArgs(FilePath, LogUntil, userName, userId, stayFrom, stayUntil, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="ObjectPickedup"/> event.
        /// </summary>
        /// <param name="objectName">Pickedup object name.</param>
        /// <param name="isEquipped">True if equipped.</param>
        /// <param name="isEquippable">True if the object is equippable.</param>
        /// <param name="lastInputMethod">Last input method name.</param>
        /// <param name="isAutoEquipController">True if the object is auto equip controller.</param>
        protected override void OnPickupObject(string objectName, bool isEquipped, bool isEquippable, string lastInputMethod, bool isAutoEquipController)
        {
            ObjectPickedup?.Invoke(this, new ObjectPickedupEventArgs(FilePath, LogUntil, objectName, isEquipped, isEquippable, lastInputMethod, isAutoEquipController));
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
            ObjectDropped?.Invoke(this, new ObjectDroppedEventArgs(FilePath, LogUntil, objectName, isEquipped, dropReason, lastInputMethod));
        }

        /// <summary>
        /// Fire <see cref="ScreenshotTook"/> event.
        /// </summary>
        /// <param name="filePath">Screenshort file path.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnScreenshotTook(string filePath, InstanceInfo instanceInfo)
        {
            ScreenshotTook?.Invoke(this, new ScreenshotTakeEventArgs(FilePath, LogUntil, filePath, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="VideoUrlResolving"/> event.
        /// </summary>
        /// <param name="url">Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnVideoUrlResolving(string url, InstanceInfo instanceInfo)
        {
            VideoUrlResolving?.Invoke(this, new VideoUrlResolveEventArgs(FilePath, LogUntil, url, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="VideoUrlResolved"/> event.
        /// </summary>
        /// <param name="url">Video URL.</param>
        /// <param name="resolvedUrl">Resolved Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnVideoUrlResolved(string url, string resolvedUrl, InstanceInfo instanceInfo)
        {
            VideoUrlResolved?.Invoke(this, new VideoUrlResolveEventArgs(FilePath, LogUntil, url, resolvedUrl, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="Downloaded"/> event.
        /// </summary>
        /// <param name="url">Download URL.</param>
        /// <param name="type"></param>
        /// <param name="instanceInfo"></param>
        protected override void OnDownloaded(string url, DownloadType type, InstanceInfo instanceInfo)
        {
            Downloaded?.Invoke(this, new DownloadEventArgs(FilePath, LogUntil, url, type, instanceInfo));
        }

        /// <summary>
        /// Fire <see cref="WarningDetected"/> event.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        protected override void OnWarningDetected(VRCLogLevel level, List<string> logLines)
        {
            WarningDetected?.Invoke(this, new ErrorLogEventArgs(FilePath, LogUntil, level, logLines));
        }

        /// <summary>
        /// Fire <see cref="ErrorDetected"/> event.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        protected override void OnErrorDetected(VRCLogLevel level, List<string> logLines)
        {
            ErrorDetected?.Invoke(this, new ErrorLogEventArgs(FilePath, LogUntil, level, logLines));
        }

        /// <summary>
        /// Fire <see cref="ExceptionDetected"/> event.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        protected override void OnExceptionDetected(VRCLogLevel level, List<string> logLines)
        {
            ExceptionDetected?.Invoke(this, new ErrorLogEventArgs(FilePath, LogUntil, level, logLines));
        }

        /// <summary>
        /// Fire <see cref="BulletTimeAgentSaved"/> event.
        /// </summary>
        /// <param name="saveText">Save data text.</param>
        protected override void OnBulletTimeAgentSaved(string saveText)
        {
            BulletTimeAgentSaved?.Invoke(this, new SaveEventArgs(FilePath, LogUntil, saveText));
        }

        /// <summary>
        /// Fire <see cref="IdleCubeSaved"/> event.
        /// </summary>
        /// <param name="saveText">Save data text.</param>
        protected override void OnIdleCubeSaved(string saveText)
        {
            IdleCubeSaved?.Invoke(this, new SaveEventArgs(FilePath, LogUntil, saveText));
        }

        /// <summary>
        /// Fire <see cref="IdleDefenseSaved"/> event.
        /// </summary>
        /// <param name="saveText">Save data text.</param>
        protected override void OnIdleDefenseSaved(string saveText)
        {
            IdleDefenseSaved?.Invoke(this, new SaveEventArgs(FilePath, LogUntil, saveText));
        }

        /// <summary>
        /// Fire <see cref="IdleHomeSaved"/> event.
        /// </summary>
        /// <param name="saveText">Save data text.</param>
        protected override void OnIdleHomeSaved(string saveText)
        {
            IdleHomeSaved?.Invoke(this, new SaveEventArgs(FilePath, LogUntil, saveText));
        }

        /// <summary>
        /// Fire <see cref="MagicalCursedLandSaved"/> event.
        /// </summary>
        /// <param name="saveText">Save data text.</param>
        protected override void OnMagicalCursedLandSaved(string saveText)
        {
            MagicalCursedLandSaved?.Invoke(this, new SaveEventArgs(FilePath, LogUntil, saveText));
        }

        /// <summary>
        /// Fire <see cref="RhapsodySaved"/> event.
        /// </summary>
        /// <param name="saveText">Save data text.</param>
        protected override void OnRhapsodySaved(string saveText)
        {
            RhapsodySaved?.Invoke(this, new SaveEventArgs(FilePath, LogUntil, saveText));
        }

        /// <summary>
        /// Fire <see cref="TerrorsOfNowhereSaved"/> event.
        /// </summary>
        /// <param name="saveText">Save data text.</param>
        protected override void OnTerrorsOfNowhereSaved(string saveText)
        {
            TerrorsOfNowhereSaved?.Invoke(this, new SaveEventArgs(FilePath, LogUntil, saveText));
        }

        /// <summary>
        /// Fire <see cref="TonKillerTargetChanged"/> event.
        /// </summary>
        /// <param name="terrorName">Terror name.</param>
        protected override void OnTonKillerTargetChanged(string terrorName)
        {
            TonKillerTargetChanged?.Invoke(this, new TonKillerNameEventArgs(FilePath, LogUntil, terrorName));
        }

        /// <summary>
        /// Fire <see cref="TonPlayerDead"/> event.
        /// </summary>
        /// <param name="playerName">Player name.</param>
        /// <param name="message">Message.</param>
        protected override void OnTonPlayerDead(string playerName, string message)
        {
            TonPlayerDead?.Invoke(this, new TonPlayerDeadEventArgs(FilePath, LogUntil, playerName, message));
        }

        /// <summary>
        /// Fire <see cref="TonPlayerDamaged"/> event.
        /// </summary>
        /// <param name="damage">Damage point.</param>
        protected override void OnTonPlayerDamaged(int damage)
        {
            TonPlayerDamaged?.Invoke(this, new TonPlayerDamagedEventArgs(FilePath, LogUntil, damage));
        }

        /// <summary>
        /// Fire <see cref="TonKillerStunned"/> event.
        /// </summary>
        /// <param name="terrorName">Terror name.</param>
        protected override void OnTonKillerStunned(string terrorName)
        {
            TonKillerStunned?.Invoke(this, new TonKillerNameEventArgs(FilePath, LogUntil, terrorName));
        }

        /// <summary>
        /// Fire <see cref="TonKillerEnraged"/> event.
        /// </summary>
        /// <param name="terrorName">Terror name.</param>
        /// <param name="enrageLevel">Enrage level.</param>
        protected override void OnTonKillerEnraged(string terrorName, int enrageLevel)
        {
            TonKillerEnraged?.Invoke(this, new TonKillerEnragedEventArgs(FilePath, LogUntil, terrorName, enrageLevel));
        }

        /// <summary>
        /// Fire <see cref="TonKillerSet"/> event.
        /// </summary>
        /// <param name="terrorIndex1">First terror index.</param>
        /// <param name="terrorIndex2">Second terror index.</param>
        /// <param name="terrorIndex3">Third terror index.</param>
        /// <param name="roundName">Round name.</param>
        protected override void OnTonKillerSet(int terrorIndex1, int terrorIndex2, int terrorIndex3, string roundName)
        {
            TonKillerSet?.Invoke(this, new TonKillerSetEventArgs(FilePath, LogUntil, terrorIndex1, terrorIndex2, terrorIndex3, roundName));
        }

        /// <summary>
        /// Fire <see cref="TonKillerUnlocked"/> event.
        /// </summary>
        /// <param name="indexType">Terror index type.</param>
        /// <param name="terrorIndex">Terror (Killer) index.</param>
        protected override void OnTonKillerUnlocked(TonTerrorIndexType indexType, int terrorIndex)
        {
            TonKillerUnlocked?.Invoke(this, new TonKillerUnlockedEventArgs(FilePath, LogUntil, indexType, terrorIndex));
        }

        /// <summary>
        /// Fire <see cref="TonEquipped"/> event.
        /// </summary>
        /// <param name="itemIndex">Equipped item index.</param>
        /// <param name="lastItemIndex">Last equipped item index.</param>
        protected override void OnTonEquipped(int itemIndex, int lastItemIndex)
        {
            TonEquipped?.Invoke(this, new TonEquipEventArgs(FilePath, LogUntil, itemIndex, lastItemIndex));
        }

        /// <summary>
        /// Fire <see cref="TonRoundStarted"/> event.
        /// </summary>
        /// <param name="placeName">Place name.</param>
        /// <param name="placeIndex">Place index.</param>
        /// <param name="roundName">Round name.</param>
        protected override void OnTonRoundStart(string placeName, int placeIndex, string roundName)
        {
            TonRoundStarted?.Invoke(this, new TonRoundStartedEventArgs(FilePath, LogUntil, placeName, placeIndex, roundName));
        }

        /// <summary>
        /// Fire <see cref="TonRoundFinished"/> event.
        /// </summary>
        /// <param name="result">Round result.</param>
        protected override void OnTonRoundFinished(TonRoundResult result)
        {
            TonRoundFinished?.Invoke(this, new TonRoundFinishedEventArgs(FilePath, LogUntil, result));
        }
    }
}
