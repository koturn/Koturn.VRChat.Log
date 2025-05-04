using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for the <see cref="IVRCCoreLogEvent.UserJoined"/>.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with user name, Join timestamp and instance information.
    /// </remarks>
    /// <param name="logFilePath">Log file path.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="userName">User name.</param>
    /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
    /// <param name="stayFrom">A timestamp the user joined.</param>
    /// <param name="instanceInfo">Instance information.</param>
    public class UserJoinEventArgs(string? logFilePath, DateTime logAt, string userName, string? userId, DateTime stayFrom, InstanceInfo instanceInfo)
        : VRCLogEventArgs(logFilePath, logAt)
    {
        /// <summary>
        /// User name.
        /// </summary>
        public string UserName { get; } = userName;
        /// <summary>
        /// User ID.
        /// </summary>
        public string? UserId { get; } = userId;
        /// <summary>
        /// A timestamp the user joined.
        /// </summary>
        public DateTime StayFrom { get; } = stayFrom;
        /// <summary>
        /// Instance information.
        /// </summary>
        public InstanceInfo InstanceInfo { get; } = instanceInfo;
    }
}
