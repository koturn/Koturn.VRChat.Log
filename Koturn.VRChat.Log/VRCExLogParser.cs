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
        public event EventHandler<UserAuthenticatedEventArgs>? UserAuthenticated;
        /// <inheritdoc/>
        public event EventHandler<JoinLeaveInstanceEventArgs>? JoinedToInstance;
        /// <inheritdoc/>
        public event EventHandler<JoinLeaveInstanceEventArgs>? LeftFromInstance;
        /// <inheritdoc/>
        public event EventHandler<UserJoinLeaveEventArgs>? UserJoined;
        /// <inheritdoc/>
        public event EventHandler<UserJoinLeaveEventArgs>? UserLeft;
        /// <inheritdoc/>
        public event EventHandler<UserJoinLeaveEventArgs>? UserUnregistering;
        /// <inheritdoc/>
        public event EventHandler<ObjectPickedupEventArgs>? ObjectPickedup;
        /// <inheritdoc/>
        public event EventHandler<ObjectDroppedEventArgs>? ObjectDropped;
        /// <inheritdoc/>
        public event EventHandler<ScreenshotTakeEventArgs>? ScreenshotTook;
        /// <inheritdoc/>
        public event EventHandler<VideoUrlResolveEventArgs>? VideoUrlResolving;
        /// <inheritdoc/>
        public event EventHandler<VideoUrlResolveEventArgs>? VideoUrlResolved;
        /// <inheritdoc/>
        public event EventHandler<DownloadEventArgs>? Downloaded;
        /// <inheritdoc/>
        public event EventHandler<ErrorLogEventArgs>? WarningDetected;
        /// <inheritdoc/>
        public event EventHandler<ErrorLogEventArgs>? ErrorDetected;
        /// <inheritdoc/>
        public event EventHandler<ErrorLogEventArgs>? ExceptionDetected;
        /// <inheritdoc/>
        public event EventHandler<SaveEventArgs>? IdleHomeSaved;
        /// <inheritdoc/>
        public event EventHandler<SaveEventArgs>? IdleDefenseSaved;
        /// <inheritdoc/>
        public event EventHandler<SaveEventArgs>? RhapsodySaved;
        /// <inheritdoc/>
        public event EventHandler<TonKillerNameEventArgs>? TonKillerTargetChanged;
        /// <inheritdoc/>
        public event EventHandler<TonPlayerDeadEventArgs>? TonPlayerDead;
        /// <inheritdoc/>
        public event EventHandler<TonPlayerDamagedEventArgs>? TonPlayerDamaged;
        /// <inheritdoc/>
        public event EventHandler<TonKillerNameEventArgs>? TonKillerStunned;
        /// <inheritdoc/>
        public event EventHandler<TonKillerEnragedEventArgs>? TonKillerEnraged;
        /// <inheritdoc/>
        public event EventHandler<TonKillerSetEventArgs>? TonKillerSet;
        /// <inheritdoc/>
        public event EventHandler<TonKillerUnlockedEventArgs>? TonKillerUnlocked;
        /// <inheritdoc/>
        public event EventHandler<TonEquipEventArgs>? TonEquipped;
        /// <inheritdoc/>
        public event EventHandler<TonRoundStartedEventArgs>? TonRoundStarted;
        /// <inheritdoc/>
        public event EventHandler<TonRoundFinishedEventArgs>? TonRoundFinished;
        /// <inheritdoc/>
        public event EventHandler<SaveEventArgs>? TerrorsOfNowhereSaved;


        /// <summary>
        /// Create <see cref="VRCLogParser"/> instance.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for <see cref="FileStream"/> and <see cref="StreamReader"/>.</param>
        public VRCExLogParser(string filePath, int bufferSize = 65536)
            : base(filePath, bufferSize)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogParser"/> instance.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="StreamReader"/>.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCLogParser"/> object is disposed; otherwise, false.</param>
        public VRCExLogParser(Stream stream, int bufferSize = 65536, bool leaveOpen = false)
            : base(stream, bufferSize, leaveOpen)
        {
        }

        /// <summary>
        /// Create <see cref="VRCLogParser"/> instance.
        /// </summary>
        /// <param name="reader"><see cref="TextReader"/> of VRChat log file.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="reader"/> open
        /// after the <see cref="VRCLogParser"/> object is disposed; otherwise, false.</param>
        public VRCExLogParser(TextReader reader, bool leaveOpen = false)
            : base(reader, leaveOpen)
        {
        }


        /// <summary>
        /// Fire <see cref="UserAuthenticated"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="authUserInfo">Authenticated user information.</param>
        protected override void OnUserAuthenticated(DateTime logAt, AuthUserInfo authUserInfo)
        {
            UserAuthenticated?.Invoke(logAt, new UserAuthenticatedEventArgs(logAt, authUserInfo));
        }

        /// <summary>
        /// Fire <see cref="JoinedToInstance"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnJoinedToInstance(DateTime logAt, InstanceInfo instanceInfo)
        {
            base.OnJoinedToInstance(logAt, instanceInfo);
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
            ObjectPickedup?.Invoke(this, new ObjectPickedupEventArgs(logAt, objectName, isEquipped, isEquippable, lastInputMethod, isAutoEquipController));
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
            ObjectDropped?.Invoke(this, new ObjectDroppedEventArgs(logAt, objectName, isEquipped, dropReason, lastInputMethod));
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
        /// Fire <see cref="IdleDefenseSaved"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="saveText">Save data text.</param>
        protected override void OnIdleDefenseSaved(DateTime logAt, string saveText)
        {
            IdleDefenseSaved?.Invoke(this, new SaveEventArgs(logAt, saveText));
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
        /// Fire <see cref="TonKillerTargetChanged"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="terrorName">Terror name.</param>
        protected override void OnTonKillerTargetChanged(DateTime logAt, string terrorName)
        {
            TonKillerTargetChanged?.Invoke(this, new TonKillerNameEventArgs(logAt, terrorName));
        }

        /// <summary>
        /// Fire <see cref="TonPlayerDead"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="playerName">Player name.</param>
        /// <param name="message">Message.</param>
        protected override void OnTonPlayerDead(DateTime logAt, string playerName, string message)
        {
            TonPlayerDead?.Invoke(this, new TonPlayerDeadEventArgs(logAt, playerName, message));
        }

        /// <summary>
        /// Fire <see cref="TonPlayerDamaged"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="damage">Damage point.</param>
        protected override void OnTonPlayerDamaged(DateTime logAt, int damage)
        {
            TonPlayerDamaged?.Invoke(this, new TonPlayerDamagedEventArgs(logAt, damage));
        }

        /// <summary>
        /// Fire <see cref="TonKillerStunned"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="terrorName">Terror name.</param>
        protected override void OnTonKillerStunned(DateTime logAt, string terrorName)
        {
            TonKillerStunned?.Invoke(this, new TonKillerNameEventArgs(logAt, terrorName));
        }

        /// <summary>
        /// Fire <see cref="TonKillerEnraged"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="terrorName">Terror name.</param>
        /// <param name="enrageLevel">Enrage level.</param>
        protected override void OnTonKillerEnraged(DateTime logAt, string terrorName, int enrageLevel)
        {
            TonKillerEnraged?.Invoke(this, new TonKillerEnragedEventArgs(logAt, terrorName, enrageLevel));
        }

        /// <summary>
        /// Fire <see cref="TonKillerSet"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="terrorIndex1">First terror index.</param>
        /// <param name="terrorIndex2">Second terror index.</param>
        /// <param name="terrorIndex3">Third terror index.</param>
        /// <param name="roundName">Round name.</param>
        protected override void OnTonKillerSet(DateTime logAt, int terrorIndex1, int terrorIndex2, int terrorIndex3, string roundName)
        {
            TonKillerSet?.Invoke(this, new TonKillerSetEventArgs(logAt, terrorIndex1, terrorIndex2, terrorIndex3, roundName));
        }

        /// <summary>
        /// Fire <see cref="TonKillerUnlocked"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="indexType">Terror index type.</param>
        /// <param name="terrorIndex">Terror (Killer) index.</param>
        protected override void OnTonKillerUnlocked(DateTime logAt, TonTerrorIndexType indexType, int terrorIndex)
        {
            TonKillerUnlocked?.Invoke(this, new TonKillerUnlockedEventArgs(logAt, indexType, terrorIndex));
        }

        /// <summary>
        /// Fire <see cref="TonEquipped"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="itemIndex">Equipped item index.</param>
        /// <param name="lastItemIndex">Last equipped item index.</param>
        protected override void OnTonEquipped(DateTime logAt, int itemIndex, int lastItemIndex)
        {
            TonEquipped?.Invoke(this, new TonEquipEventArgs(logAt, itemIndex, lastItemIndex));
        }

        /// <summary>
        /// Fire <see cref="TonRoundStarted"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="placeName">Place name.</param>
        /// <param name="placeIndex">Place index.</param>
        /// <param name="roundName">Round name.</param>
        protected override void OnTonRoundStart(DateTime logAt, string placeName, int placeIndex, string roundName)
        {
            TonRoundStarted?.Invoke(this, new TonRoundStartedEventArgs(logAt, placeName, placeIndex, roundName));
        }

        /// <summary>
        /// Fire <see cref="TonRoundFinished"/> event.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="result">Round result.</param>
        protected override void OnTonRoundFinished(DateTime logAt, TonRoundResult result)
        {
            TonRoundFinished?.Invoke(this, new TonRoundFinishedEventArgs(logAt, result));
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
    }
}
