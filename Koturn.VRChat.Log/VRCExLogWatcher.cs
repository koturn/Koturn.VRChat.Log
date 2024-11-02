using System;
using System.Collections.Generic;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Events;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Log Watcher class.
    /// </summary>
    public class VRCExLogWatcher : VRCLogWatcher, IVRCExLogEvent
    {
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
        /// Create <see cref="VRCExLogWatcher"/> instance.
        /// </summary>
        public VRCExLogWatcher()
            : this(1000)
        {
        }

        /// <summary>
        /// Create <see cref="VRCExLogWatcher"/> instance.
        /// </summary>
        /// <param name="watchCycle">File watch cycle.</param>
        public VRCExLogWatcher(int watchCycle)
            : base(watchCycle)
        {
        }


        /// <summary>
        /// Create <see cref="VRCWatcherExLogParser"/> instance with specified file path.
        /// </summary>
        /// <param name="filePath">File path to parse.</param>
        /// <returns>Created <see cref="VRCWatcherExLogParser"/> instance.</returns>
        protected override VRCBaseLogParser CreateLogParser(string filePath)
        {
            CurrentLogFrom = default;
            CurrentLogUntil = default;
            return new VRCWatcherExLogParser(filePath, this);
        }


        /// <summary>
        /// VRChat log file parser for <see cref="VRCExLogWatcher"/>.
        /// </summary>
        private sealed class VRCWatcherExLogParser : VRCCoreExLogParser
        {
            /// <summary>
            /// Reference to <see cref="VRCExLogWatcher"/> instance.
            /// </summary>
            private readonly VRCExLogWatcher _logWatcher;

            /// <summary>
            /// Create <see cref="VRCWatcherExLogParser"/> instance.
            /// </summary>
            /// <param name="filePath">VRChat log file path.</param>
            /// <param name="logWatcher"><see cref="VRCExLogWatcher"/> instance.</param>
            public VRCWatcherExLogParser(string filePath, VRCExLogWatcher logWatcher)
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
            /// Set <see cref="VRCLogWatcher.AuthUserInfo"/>.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="authUserInfo">Authenticated user information.</param>
            protected override void OnUserAuthenticated(DateTime logAt, AuthUserInfo authUserInfo)
            {
                _logWatcher.AuthUserInfo = authUserInfo;
                _logWatcher._userAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs(logAt, authUserInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.JoinedToInstance"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnJoinedToInstance(DateTime logAt, InstanceInfo instanceInfo)
            {
                base.OnJoinedToInstance(logAt, instanceInfo);
                _logWatcher._joinedToInstance?.Invoke(this, new JoinLeaveInstanceEventArgs(logAt, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.LeftFromInstance"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnLeftFromInstance(DateTime logAt, InstanceInfo instanceInfo)
            {
                _logWatcher._leftFromInstance?.Invoke(this, new JoinLeaveInstanceEventArgs(logAt, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.UserJoined"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="userName">User name.</param>
            /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserJoined(DateTime logAt, string userName, string? userId, DateTime stayFrom, InstanceInfo instanceInfo)
            {
                _logWatcher._userJoined?.Invoke(this, new UserJoinLeaveEventArgs(logAt, userName, userId, stayFrom, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.UserLeft"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="userName">User name.</param>
            /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="stayUntil">A timestamp the user left.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserLeft(DateTime logAt, string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
            {
                _logWatcher._userLeft?.Invoke(this, new UserJoinLeaveEventArgs(logAt, userName, userId, stayFrom, stayUntil, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.UserUnregistering"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="userName">User name.</param>
            /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="stayUntil">A timestamp the user left.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserUnregistering(DateTime logAt, string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
            {
                _logWatcher._userUnregistering?.Invoke(this, new UserJoinLeaveEventArgs(logAt, userName, userId, stayFrom, stayUntil, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.ObjectPickedup"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="objectName">Pickedup object name.</param>
            /// <param name="isEquipped">True if equipped.</param>
            /// <param name="isEquippable">True if the object is equippable.</param>
            /// <param name="lastInputMethod">Last input method name.</param>
            /// <param name="isAutoEquipController">True if the object is auto equip controller.</param>
            protected override void OnPickupObject(DateTime logAt, string objectName, bool isEquipped, bool isEquippable, string lastInputMethod, bool isAutoEquipController)
            {
                _logWatcher._objectPickedup?.Invoke(this, new ObjectPickedupEventArgs(logAt, objectName, isEquipped, isEquippable, lastInputMethod, isAutoEquipController));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.ObjectDropped"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="objectName">Pickedup object name.</param>
            /// <param name="isEquipped">True if the object was equipped.</param>
            /// <param name="dropReason">Reason for dropping the object.</param>
            /// <param name="lastInputMethod">Last input method name.</param>
            protected override void OnDropObject(DateTime logAt, string objectName, bool isEquipped, string dropReason, string lastInputMethod)
            {
                _logWatcher._objectDropped?.Invoke(this, new ObjectDroppedEventArgs(logAt, objectName, isEquipped, dropReason, lastInputMethod));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.ScreenshotTook"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="filePath">Screenshort file path.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnScreenshotTook(DateTime logAt, string filePath, InstanceInfo instanceInfo)
            {
                _logWatcher._screenshotTook?.Invoke(this, new ScreenshotTakeEventArgs(logAt, filePath, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.VideoUrlResolving"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="url">Video URL.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnVideoUrlResolving(DateTime logAt, string url, InstanceInfo instanceInfo)
            {
                _logWatcher._videoUrlResolving?.Invoke(this, new VideoUrlResolveEventArgs(logAt, url, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.VideoUrlResolved"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="url">Video URL.</param>
            /// <param name="resolvedUrl">Resolved Video URL.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnVideoUrlResolved(DateTime logAt, string url, string resolvedUrl, InstanceInfo instanceInfo)
            {
                _logWatcher._videoUrlResolved?.Invoke(this, new VideoUrlResolveEventArgs(logAt, url, resolvedUrl, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.Downloaded"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="url">Download URL.</param>
            /// <param name="type"></param>
            /// <param name="instanceInfo"></param>
            protected override void OnDownloaded(DateTime logAt, string url, DownloadType type, InstanceInfo instanceInfo)
            {
                _logWatcher._downloaded?.Invoke(this, new DownloadEventArgs(logAt, url, type, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.WarningDetected"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnWarningDetected(DateTime logAt, LogLevel level, List<string> logLines)
            {
                _logWatcher._warningDetected?.Invoke(this, new ErrorLogEventArgs(logAt, level, logLines));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.ErrorDetected"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnErrorDetected(DateTime logAt, LogLevel level, List<string> logLines)
            {
                _logWatcher._errorDetected?.Invoke(this, new ErrorLogEventArgs(logAt, level, logLines));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.ExceptionDetected"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnExceptionDetected(DateTime logAt, LogLevel level, List<string> logLines)
            {
                _logWatcher._exceptionDetected?.Invoke(this, new ErrorLogEventArgs(logAt, level, logLines));
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
            /// Fire <see cref="RhapsodySaved"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="saveText">Save data text.</param>
            protected override void OnRhapsodySaved(DateTime logAt, string saveText)
            {
                _logWatcher.RhapsodySaved?.Invoke(this, new SaveEventArgs(logAt, saveText));
            }

            /// <summary>
            /// Fire <see cref="TonKillerTargetChanged"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="terrorName">Terror name.</param>
            protected override void OnTonKillerTargetChanged(DateTime logAt, string terrorName)
            {
                _logWatcher.TonKillerTargetChanged?.Invoke(this, new TonKillerNameEventArgs(logAt, terrorName));
            }

            /// <summary>
            /// Fire <see cref="TonPlayerDead"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="playerName">Player name.</param>
            /// <param name="message">Message.</param>
            protected override void OnTonPlayerDead(DateTime logAt, string playerName, string message)
            {
                _logWatcher.TonPlayerDead?.Invoke(this, new TonPlayerDeadEventArgs(logAt, playerName, message));
            }

            /// <summary>
            /// Fire <see cref="TonPlayerDamaged"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="damage">Damage point.</param>
            protected override void OnTonPlayerDamaged(DateTime logAt, int damage)
            {
                _logWatcher.TonPlayerDamaged?.Invoke(this, new TonPlayerDamagedEventArgs(logAt, damage));
            }

            /// <summary>
            /// Fire <see cref="TonKillerStunned"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="terrorName">Terror name.</param>
            protected override void OnTonKillerStunned(DateTime logAt, string terrorName)
            {
                _logWatcher.TonKillerStunned?.Invoke(this, new TonKillerNameEventArgs(logAt, terrorName));
            }

            /// <summary>
            /// Fire <see cref="TonKillerEnraged"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="terrorName">Terror name.</param>
            /// <param name="enrageLevel">Enrage level.</param>
            protected override void OnTonKillerEnraged(DateTime logAt, string terrorName, int enrageLevel)
            {
                _logWatcher.TonKillerEnraged?.Invoke(this, new TonKillerEnragedEventArgs(logAt, terrorName, enrageLevel));
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
                _logWatcher.TonKillerSet?.Invoke(this, new TonKillerSetEventArgs(logAt, terrorIndex1, terrorIndex2, terrorIndex3, roundName));
            }

            /// <summary>
            /// Fire <see cref="TonKillerUnlocked"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="indexType">Terror index type.</param>
            /// <param name="terrorIndex">Terror (Killer) index.</param>
            protected override void OnTonKillerUnlocked(DateTime logAt, TonTerrorIndexType indexType, int terrorIndex)
            {
                _logWatcher.TonKillerUnlocked?.Invoke(this, new TonKillerUnlockedEventArgs(logAt, indexType, terrorIndex));
            }

            /// <summary>
            /// Fire <see cref="TonEquipped"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="itemIndex">Equipped item index.</param>
            /// <param name="lastItemIndex">Last equipped item index.</param>
            protected override void OnTonEquipped(DateTime logAt, int itemIndex, int lastItemIndex)
            {
                _logWatcher.TonEquipped?.Invoke(this, new TonEquipEventArgs(logAt, itemIndex, lastItemIndex));
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
                _logWatcher.TonRoundStarted?.Invoke(this, new TonRoundStartedEventArgs(logAt, placeName, placeIndex, roundName));
            }

            /// <summary>
            /// Fire <see cref="TonRoundFinished"/> event.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="result">Round result.</param>
            protected override void OnTonRoundFinished(DateTime logAt, TonRoundResult result)
            {
                _logWatcher.TonRoundFinished?.Invoke(this, new TonRoundFinishedEventArgs(logAt, result));
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
        }
    }
}
