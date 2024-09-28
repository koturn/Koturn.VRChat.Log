using System;
using Koturn.VRChat.Log.Events;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Provides some events that occures when detect specific logs.
    /// </summary>
    public interface IVRCCoreLogEvent
    {
        /// <summary>
        /// Occurs when detect a log that you joined to instance.
        /// </summary>
        event EventHandler<JoinLeaveInstanceEventArgs>? JoinedToInstance;
        /// <summary>
        /// Occurs when detect a log that you left from instance.
        /// </summary>
        event EventHandler<JoinLeaveInstanceEventArgs>? LeftFromInstance;
        /// <summary>
        /// Occurs when detect a log that any player joined to your instance.
        /// </summary>
        event EventHandler<UserJoinLeaveEventArgs>? UserJoined;
        /// <summary>
        /// Occurs when detect a log that any player left from your instance.
        /// </summary>
        event EventHandler<UserJoinLeaveEventArgs>? UserLeft;
        /// <summary>
        /// Occurs when detect a log that any player unregistering from your instance.
        /// </summary>
        event EventHandler<UserJoinLeaveEventArgs>? UserUnregistering;
        /// <summary>
        /// Occurs when detect a log that object pickedup.
        /// </summary>
        event EventHandler<ObjectPickedupEventArgs>? ObjectPickedup;
        /// <summary>
        /// Occurs when detect a log that object dropped.
        /// </summary>
        event EventHandler<ObjectDroppedEventArgs>? ObjectDropped;
        /// <summary>
        /// Occurs when detect a log that you take a screenshot.
        /// </summary>
        event EventHandler<ScreenshotTakeEventArgs>? ScreenshotTook;
        /// <summary>
        /// Occurs when detect a log that video URL resolving.
        /// </summary>
        event EventHandler<VideoUrlResolveEventArgs>? VideoUrlResolving;
        /// <summary>
        /// Occurs when detect a log that video URL resolved.
        /// </summary>
        event EventHandler<VideoUrlResolveEventArgs>? VideoUrlResolved;
        /// <summary>
        /// Occurs when detect a log that string or image is downloaded.
        /// </summary>
        event EventHandler<DownloadEventArgs>? Downloaded;
        /// <summary>
        /// Occurs when detect a log that save data text of Idle Home is generated.
        /// </summary>
        event EventHandler<SaveEventArgs>? IdleHomeSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Idle Defense is generated.
        /// </summary>
        event EventHandler<SaveEventArgs>? IdleDefenseSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Terrors of Nowhere is generated.
        /// </summary>
        event EventHandler<SaveEventArgs>? TerrorsOfNowhereSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Rhapsody is generated.
        /// </summary>
        event EventHandler<SaveEventArgs>? RhapsodySaved;
        /// <summary>
        /// Occurs when detect a warning log.
        /// </summary>
        event EventHandler<ErrorLogEventArgs>? WarningDetected;
        /// <summary>
        /// Occurs when detect a error log.
        /// </summary>
        event EventHandler<ErrorLogEventArgs>? ErrorDetected;
        /// <summary>
        /// Occurs when detect a exception log.
        /// </summary>
        event EventHandler<ErrorLogEventArgs>? ExceptionDetected;
    }
}
