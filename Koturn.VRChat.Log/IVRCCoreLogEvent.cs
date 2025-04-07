using Koturn.VRChat.Log.Events;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Provides some events that occures when detect specific logs.
    /// </summary>
    public interface IVRCCoreLogEvent
    {
        /// <summary>
        /// Occurs when detect a log that user authenticated.
        /// </summary>
        event VRCLogEventHandler<UserAuthenticatedEventArgs>? UserAuthenticated;
        /// <summary>
        /// Occurs when detect a log that application quitted.
        /// </summary>
        event VRCLogEventHandler<ApplicationQuittedEventArgs>? ApplicationQuitted;
        /// <summary>
        /// Occurs when detect a log that instance close notification.
        /// </summary>
        event VRCLogEventHandler<InstanceResetNotifiedEventArgs>? InstanceResetNotified;
        /// <summary>
        /// Occurs when detect a log that instance closed by reset.
        /// </summary>
        event VRCLogEventHandler<LogEventArgs>? InstanceClosedByReset;
        /// <summary>
        /// Occurs when detect a log that you joined to instance.
        /// </summary>
        event VRCLogEventHandler<InstanceEventArgs>? JoinedToInstance;
        /// <summary>
        /// Occurs when detect a log that you left from instance.
        /// </summary>
        event VRCLogEventHandler<InstanceEventArgs>? LeftFromInstance;
        /// <summary>
        /// Occurs when detect a log that any player joined to your instance.
        /// </summary>
        event VRCLogEventHandler<UserJoinLeaveEventArgs>? UserJoined;
        /// <summary>
        /// Occurs when detect a log that any player left from your instance.
        /// </summary>
        event VRCLogEventHandler<UserJoinLeaveEventArgs>? UserLeft;
        /// <summary>
        /// Occurs when detect a log that any player unregistering from your instance.
        /// </summary>
        event VRCLogEventHandler<UserJoinLeaveEventArgs>? UserUnregistering;
        /// <summary>
        /// Occurs when detect a log that instance is closed.
        /// </summary>
        event VRCLogEventHandler<InstanceEventArgs>? InstanceClosed;
        /// <summary>
        /// Occurs when detect a log that object pickedup.
        /// </summary>
        event VRCLogEventHandler<ObjectPickedupEventArgs>? ObjectPickedup;
        /// <summary>
        /// Occurs when detect a log that object dropped.
        /// </summary>
        event VRCLogEventHandler<ObjectDroppedEventArgs>? ObjectDropped;
        /// <summary>
        /// Occurs when detect a log that you take a screenshot.
        /// </summary>
        event VRCLogEventHandler<ScreenshotTakeEventArgs>? ScreenshotTook;
        /// <summary>
        /// Occurs when detect a log that video URL resolving.
        /// </summary>
        event VRCLogEventHandler<VideoUrlResolveEventArgs>? VideoUrlResolving;
        /// <summary>
        /// Occurs when detect a log that video URL resolved.
        /// </summary>
        event VRCLogEventHandler<VideoUrlResolveEventArgs>? VideoUrlResolved;
        /// <summary>
        /// Occurs when detect a log that string or image is downloaded.
        /// </summary>
        event VRCLogEventHandler<DownloadEventArgs>? Downloaded;
        /// <summary>
        /// Occurs when detect a warning log.
        /// </summary>
        event VRCLogEventHandler<ErrorLogEventArgs>? WarningDetected;
        /// <summary>
        /// Occurs when detect a error log.
        /// </summary>
        event VRCLogEventHandler<ErrorLogEventArgs>? ErrorDetected;
        /// <summary>
        /// Occurs when detect a exception log.
        /// </summary>
        event VRCLogEventHandler<ErrorLogEventArgs>? ExceptionDetected;
    }
}
