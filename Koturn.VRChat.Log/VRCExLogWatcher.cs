using System;
using System.Collections.Generic;
using System.Threading;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Events;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Log Watcher class.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create <see cref="VRCExLogWatcher"/> instance.
    /// </remarks>
    /// <param name="watchCycle">File watch cycle.</param>
    public class VRCExLogWatcher(int watchCycle) : VRCLogWatcher(watchCycle), IVRCExLogEvent
    {
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
        /// Create <see cref="VRCExLogWatcher"/> instance.
        /// </summary>
        public VRCExLogWatcher()
            : this(InternalDefaultWatchCycle)
        {
        }


        /// <summary>
        /// Create <see cref="VRCWatcherExLogParser"/> instance with specified file path.
        /// </summary>
        /// <param name="filePath">File path to parse.</param>
        /// <returns>Created <see cref="VRCWatcherExLogParser"/> instance.</returns>
        protected override VRCBaseLogParser CreateLogParser(string filePath)
        {
            return new VRCWatcherExLogParser(filePath, this);
        }


        /// <summary>
        /// VRChat log file parser for <see cref="VRCExLogWatcher"/>.
        /// </summary>
        /// <remarks>
        /// Primary ctor: Create <see cref="VRCWatcherExLogParser"/> instance.
        /// </remarks>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="logWatcher"><see cref="VRCExLogWatcher"/> instance.</param>
        private sealed class VRCWatcherExLogParser(string filePath, VRCExLogWatcher logWatcher)
            : VRCCoreExLogParser(filePath)
        {
            /// <summary>
            /// Reference to <see cref="VRCExLogWatcher"/> instance.
            /// </summary>
            private readonly VRCExLogWatcher _logWatcher = logWatcher;
            /// <summary>
            /// Log file path.
            /// </summary>
            private readonly string _logFilePath = filePath;

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.UserAuthenticated"/> event.
            /// </summary>
            /// <param name="authUserInfo">Authenticated user information.</param>
            protected override void OnUserAuthenticated(AuthUserInfo authUserInfo)
            {
                _logWatcher._userAuthenticated?.Invoke(_logWatcher, new UserAuthenticatedEventArgs(_logFilePath, LogUntil, authUserInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.ApplicationQuitted"/> event.
            /// </summary>
            /// <param name="activeTime">Active time (in seconds).</param>
            protected override void OnApplicationQuit(double activeTime)
            {
                _logWatcher._applicationQuitted?.Invoke(_logWatcher, new ApplicationQuittedEventArgs(_logFilePath, LogUntil, activeTime));
                _logWatcher.Stop(Thread.CurrentThread);
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher._instanceCloseNotified"/> event.
            /// </summary>
            /// <param name="closeMinutes">Time until instance is reset (minutes).</param>
            protected override void OnInstanceResetNotified(int closeMinutes)
            {
                _logWatcher._instanceCloseNotified?.Invoke(_logWatcher, new InstanceResetNotifiedEventArgs(_logFilePath, LogUntil, closeMinutes));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher._instanceClosed"/> event.
            /// </summary>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnInstanceClosed(InstanceInfo instanceInfo)
            {
                _logWatcher._instanceClosed?.Invoke(_logWatcher, new InstanceEventArgs(_logFilePath, LogUntil, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher._instanceClosedByReset"/> event.
            /// </summary>
            protected override void OnInstanceClosedByReset()
            {
                _logWatcher._instanceClosedByReset?.Invoke(_logWatcher, new VRCLogEventArgs(_logFilePath, LogUntil));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.JoinedToInstance"/> event.
            /// </summary>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnJoinedToInstance(InstanceInfo instanceInfo)
            {
                base.OnJoinedToInstance(instanceInfo);
                _logWatcher._joinedToInstance?.Invoke(_logWatcher, new InstanceEventArgs(_logFilePath, LogUntil, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.LeftFromInstance"/> event.
            /// </summary>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnLeftFromInstance(InstanceInfo instanceInfo)
            {
                _logWatcher._leftFromInstance?.Invoke(_logWatcher, new InstanceEventArgs(_logFilePath, LogUntil, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.UserJoined"/> event.
            /// </summary>
            /// <param name="userName">User name.</param>
            /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserJoined(string userName, string? userId, DateTime stayFrom, InstanceInfo instanceInfo)
            {
                _logWatcher._userJoined?.Invoke(_logWatcher, new UserJoinEventArgs(_logFilePath, LogUntil, userName, userId, stayFrom, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.UserLeft"/> event.
            /// </summary>
            /// <param name="userName">User name.</param>
            /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="stayUntil">A timestamp the user left.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserLeft(string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
            {
                _logWatcher._userLeft?.Invoke(_logWatcher, new UserLeaveEventArgs(_logFilePath, LogUntil, userName, userId, stayFrom, stayUntil, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.UserUnregistering"/> event.
            /// </summary>
            /// <param name="userName">User name.</param>
            /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
            /// <param name="stayFrom">A timestamp the user joined.</param>
            /// <param name="stayUntil">A timestamp the user left.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnUserUnregistering(string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
            {
                _logWatcher._userUnregistering?.Invoke(_logWatcher, new UserLeaveEventArgs(_logFilePath, LogUntil, userName, userId, stayFrom, stayUntil, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.ObjectPickedup"/> event.
            /// </summary>
            /// <param name="objectName">Pickedup object name.</param>
            /// <param name="isEquipped">True if equipped.</param>
            /// <param name="isEquippable">True if the object is equippable.</param>
            /// <param name="lastInputMethod">Last input method name.</param>
            /// <param name="isAutoEquipController">True if the object is auto equip controller.</param>
            protected override void OnPickupObject(string objectName, bool isEquipped, bool isEquippable, string lastInputMethod, bool isAutoEquipController)
            {
                _logWatcher._objectPickedup?.Invoke(_logWatcher, new ObjectPickedupEventArgs(_logFilePath, LogUntil, objectName, isEquipped, isEquippable, lastInputMethod, isAutoEquipController));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.ObjectDropped"/> event.
            /// </summary>
            /// <param name="objectName">Pickedup object name.</param>
            /// <param name="isEquipped">True if the object was equipped.</param>
            /// <param name="dropReason">Reason for dropping the object.</param>
            /// <param name="lastInputMethod">Last input method name.</param>
            protected override void OnDropObject(string objectName, bool isEquipped, string dropReason, string lastInputMethod)
            {
                _logWatcher._objectDropped?.Invoke(_logWatcher, new ObjectDroppedEventArgs(_logFilePath, LogUntil, objectName, isEquipped, dropReason, lastInputMethod));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.ScreenshotTook"/> event.
            /// </summary>
            /// <param name="filePath">Screenshort file path.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnScreenshotTook(string filePath, InstanceInfo instanceInfo)
            {
                _logWatcher._screenshotTook?.Invoke(_logWatcher, new ScreenshotTakeEventArgs(_logFilePath, LogUntil, filePath, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.VideoUrlResolving"/> event.
            /// </summary>
            /// <param name="url">Video URL.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnVideoUrlResolving(string url, InstanceInfo instanceInfo)
            {
                _logWatcher._videoUrlResolving?.Invoke(_logWatcher, new VideoUrlResolveEventArgs(_logFilePath, LogUntil, url, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.VideoUrlResolved"/> event.
            /// </summary>
            /// <param name="url">Video URL.</param>
            /// <param name="resolvedUrl">Resolved Video URL.</param>
            /// <param name="instanceInfo">Instance information.</param>
            protected override void OnVideoUrlResolved(string url, string resolvedUrl, InstanceInfo instanceInfo)
            {
                _logWatcher._videoUrlResolved?.Invoke(_logWatcher, new VideoUrlResolveEventArgs(_logFilePath, LogUntil, url, resolvedUrl, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.Downloaded"/> event.
            /// </summary>
            /// <param name="url">Download URL.</param>
            /// <param name="type"></param>
            /// <param name="instanceInfo"></param>
            protected override void OnDownloaded(string url, DownloadType type, InstanceInfo instanceInfo)
            {
                _logWatcher._downloaded?.Invoke(_logWatcher, new DownloadEventArgs(_logFilePath, LogUntil, url, type, instanceInfo));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.WarningDetected"/> event.
            /// </summary>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnWarningDetected(VRCLogLevel level, List<string> logLines)
            {
                _logWatcher._warningDetected?.Invoke(_logWatcher, new ErrorLogEventArgs(_logFilePath, LogUntil, level, logLines));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.ErrorDetected"/> event.
            /// </summary>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnErrorDetected(VRCLogLevel level, List<string> logLines)
            {
                _logWatcher._errorDetected?.Invoke(_logWatcher, new ErrorLogEventArgs(_logFilePath, LogUntil, level, logLines));
            }

            /// <summary>
            /// Fire <see cref="VRCLogWatcher.ExceptionDetected"/> event.
            /// </summary>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            protected override void OnExceptionDetected(VRCLogLevel level, List<string> logLines)
            {
                _logWatcher._exceptionDetected?.Invoke(_logWatcher, new ErrorLogEventArgs(_logFilePath, LogUntil, level, logLines));
            }

            /// <summary>
            /// Fire <see cref="BulletTimeAgentSaved"/> event.
            /// </summary>
            /// <param name="saveText">Save data text.</param>
            protected override void OnBulletTimeAgentSaved(string saveText)
            {
                _logWatcher.BulletTimeAgentSaved?.Invoke(_logWatcher, new SaveEventArgs(_logFilePath, LogUntil, saveText));
            }

            /// <summary>
            /// Fire <see cref="IdleCubeSaved"/> event.
            /// </summary>
            /// <param name="saveText">Save data text.</param>
            protected override void OnIdleCubeSaved(string saveText)
            {
                _logWatcher.IdleCubeSaved?.Invoke(_logWatcher, new SaveEventArgs(_logFilePath, LogUntil, saveText));
            }

            /// <summary>
            /// Fire <see cref="IdleDefenseSaved"/> event.
            /// </summary>
            /// <param name="saveText">Save data text.</param>
            protected override void OnIdleDefenseSaved(string saveText)
            {
                _logWatcher.IdleDefenseSaved?.Invoke(_logWatcher, new SaveEventArgs(_logFilePath, LogUntil, saveText));
            }

            /// <summary>
            /// Fire <see cref="IdleHomeSaved"/> event.
            /// </summary>
            /// <param name="saveText">Save data text.</param>
            protected override void OnIdleHomeSaved(string saveText)
            {
                _logWatcher.IdleHomeSaved?.Invoke(_logWatcher, new SaveEventArgs(_logFilePath, LogUntil, saveText));
            }

            /// <summary>
            /// Fire <see cref="MagicalCursedLandSaved"/> event.
            /// </summary>
            /// <param name="saveText">Save data text.</param>
            protected override void OnMagicalCursedLandSaved(string saveText)
            {
                _logWatcher.MagicalCursedLandSaved?.Invoke(_logWatcher, new SaveEventArgs(_logFilePath, LogUntil, saveText));
            }

            /// <summary>
            /// Fire <see cref="RhapsodySaved"/> event.
            /// </summary>
            /// <param name="saveText">Save data text.</param>
            protected override void OnRhapsodySaved(string saveText)
            {
                _logWatcher.RhapsodySaved?.Invoke(_logWatcher, new SaveEventArgs(_logFilePath, LogUntil, saveText));
            }

            /// <summary>
            /// Fire <see cref="TerrorsOfNowhereSaved"/> event.
            /// </summary>
            /// <param name="saveText">Save data text.</param>
            protected override void OnTerrorsOfNowhereSaved(string saveText)
            {
                _logWatcher.TerrorsOfNowhereSaved?.Invoke(_logWatcher, new SaveEventArgs(_logFilePath, LogUntil, saveText));
            }

            /// <summary>
            /// Fire <see cref="TonKillerTargetChanged"/> event.
            /// </summary>
            /// <param name="terrorName">Terror name.</param>
            protected override void OnTonKillerTargetChanged(string terrorName)
            {
                _logWatcher.TonKillerTargetChanged?.Invoke(_logWatcher, new TonKillerNameEventArgs(_logFilePath, LogUntil, terrorName));
            }

            /// <summary>
            /// Fire <see cref="TonPlayerDead"/> event.
            /// </summary>
            /// <param name="playerName">Player name.</param>
            /// <param name="message">Message.</param>
            protected override void OnTonPlayerDead(string playerName, string message)
            {
                _logWatcher.TonPlayerDead?.Invoke(_logWatcher, new TonPlayerDeadEventArgs(_logFilePath, LogUntil, playerName, message));
            }

            /// <summary>
            /// Fire <see cref="TonPlayerDamaged"/> event.
            /// </summary>
            /// <param name="damage">Damage point.</param>
            protected override void OnTonPlayerDamaged(int damage)
            {
                _logWatcher.TonPlayerDamaged?.Invoke(_logWatcher, new TonPlayerDamagedEventArgs(_logFilePath, LogUntil, damage));
            }

            /// <summary>
            /// Fire <see cref="TonKillerStunned"/> event.
            /// </summary>
            /// <param name="terrorName">Terror name.</param>
            protected override void OnTonKillerStunned(string terrorName)
            {
                _logWatcher.TonKillerStunned?.Invoke(_logWatcher, new TonKillerNameEventArgs(_logFilePath, LogUntil, terrorName));
            }

            /// <summary>
            /// Fire <see cref="TonKillerEnraged"/> event.
            /// </summary>
            /// <param name="terrorName">Terror name.</param>
            /// <param name="enrageLevel">Enrage level.</param>
            protected override void OnTonKillerEnraged(string terrorName, int enrageLevel)
            {
                _logWatcher.TonKillerEnraged?.Invoke(_logWatcher, new TonKillerEnragedEventArgs(_logFilePath, LogUntil, terrorName, enrageLevel));
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
                _logWatcher.TonKillerSet?.Invoke(_logWatcher, new TonKillerSetEventArgs(_logFilePath, LogUntil, terrorIndex1, terrorIndex2, terrorIndex3, roundName));
            }

            /// <summary>
            /// Fire <see cref="TonKillerUnlocked"/> event.
            /// </summary>
            /// <param name="indexType">Terror index type.</param>
            /// <param name="terrorIndex">Terror (Killer) index.</param>
            protected override void OnTonKillerUnlocked(TonTerrorIndexType indexType, int terrorIndex)
            {
                _logWatcher.TonKillerUnlocked?.Invoke(_logWatcher, new TonKillerUnlockedEventArgs(_logFilePath, LogUntil, indexType, terrorIndex));
            }

            /// <summary>
            /// Fire <see cref="TonEquipped"/> event.
            /// </summary>
            /// <param name="itemIndex">Equipped item index.</param>
            /// <param name="lastItemIndex">Last equipped item index.</param>
            protected override void OnTonEquipped(int itemIndex, int lastItemIndex)
            {
                _logWatcher.TonEquipped?.Invoke(_logWatcher, new TonEquipEventArgs(_logFilePath, LogUntil, itemIndex, lastItemIndex));
            }

            /// <summary>
            /// Fire <see cref="TonRoundStarted"/> event.
            /// </summary>
            /// <param name="placeName">Place name.</param>
            /// <param name="placeIndex">Place index.</param>
            /// <param name="roundName">Round name.</param>
            protected override void OnTonRoundStart(string placeName, int placeIndex, string roundName)
            {
                _logWatcher.TonRoundStarted?.Invoke(_logWatcher, new TonRoundStartedEventArgs(_logFilePath, LogUntil, placeName, placeIndex, roundName));
            }

            /// <summary>
            /// Fire <see cref="TonRoundFinished"/> event.
            /// </summary>
            /// <param name="result">Round result.</param>
            protected override void OnTonRoundFinished(TonRoundResult result)
            {
                _logWatcher.TonRoundFinished?.Invoke(_logWatcher, new TonRoundFinishedEventArgs(_logFilePath, LogUntil, result));
            }
        }
    }
}
