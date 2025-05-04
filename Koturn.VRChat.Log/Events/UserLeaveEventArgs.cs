using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for the <see cref="IVRCCoreLogEvent.UserLeft"/> or <see cref="IVRCCoreLogEvent.UserUnregistering"/>.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with user name, Join timestamp and instance information.
    /// </remarks>
    /// <param name="logFilePath">Log file path.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="userName">User name.</param>
    /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
    /// <param name="stayFrom">A timestamp the user joined.</param>
    /// <param name="stayUntil">A timestamp the user left.</param>
    /// <param name="instanceInfo">Instance information.</param>
    public class UserLeaveEventArgs(string? logFilePath, DateTime logAt, string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
        : UserJoinEventArgs(logFilePath, logAt, userName, userId, stayFrom, instanceInfo)
    {
        /// <summary>
        /// A timestamp the user left.
        /// </summary>
        public DateTime? StayUntil { get; } = stayUntil;
    }
}
