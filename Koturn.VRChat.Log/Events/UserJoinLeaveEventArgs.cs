using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for the <see cref="IVRCCoreLogEvent.UserJoined"/>, <see cref="IVRCCoreLogEvent.UserLeft"/>
    /// or <see cref="IVRCCoreLogEvent.UserUnregistering"/>.
    /// </summary>
    public class UserJoinLeaveEventArgs : LogEventArgs
    {
        /// <summary>
        /// User name.
        /// </summary>
        public string UserName { get; }
        /// <summary>
        /// User ID.
        /// </summary>
        public string? UserId { get; }
        /// <summary>
        /// A timestamp the user joined.
        /// </summary>
        public DateTime StayFrom { get; }
        /// <summary>
        /// A timestamp the user left.
        /// </summary>
        public DateTime? StayUntil { get; }
        /// <summary>
        /// Instance information.
        /// </summary>
        public InstanceInfo InstanceInfo { get; }

        /// <summary>
        /// Create instance with user name, Join timestamp and instance information.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="instanceInfo">Instance information.</param>
        public UserJoinLeaveEventArgs(DateTime logAt, string userName, string? userId, DateTime stayFrom, InstanceInfo instanceInfo)
            : base(logAt)
        {
            UserName = userName;
            UserId = userId;
            StayFrom = stayFrom;
            InstanceInfo = instanceInfo;
        }

        /// <summary>
        /// Create instance with user name, Join timestamp, left timestamp and instance information.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID (This value may null on the logs before 2024-10-31).</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="stayUntil">A timestamp the user left.</param>
        /// <param name="instanceInfo">Instance information.</param>
        public UserJoinLeaveEventArgs(DateTime logAt, string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
            : this(logAt, userName, userId, stayFrom, instanceInfo)
        {
            StayUntil = stayUntil;
        }
    }
}
