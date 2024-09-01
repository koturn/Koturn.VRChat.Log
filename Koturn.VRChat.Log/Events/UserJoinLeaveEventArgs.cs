using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for the <see cref="VRCLogParser.UserJoined"/>, <see cref="VRCLogParser.UserLeft"/>
    /// or <see cref="VRCLogParser.UserUnregistering"/>.
    /// </summary>
    public class UserJoinLeaveEventArgs : LogEventArgs
    {
        /// <summary>
        /// User name.
        /// </summary>
        public string UserName { get; }
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
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="instanceInfo">Instance information.</param>
        public UserJoinLeaveEventArgs(DateTime logAt, string userName, DateTime stayFrom, InstanceInfo instanceInfo)
            : base(logAt)
        {
            UserName = userName;
            StayFrom = stayFrom;
            InstanceInfo = instanceInfo;
        }

        /// <summary>
        /// Create instance with user name, Join timestamp, left timestamp and instance information.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="userName">User name.</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="stayUntil">A timestamp the user left.</param>
        /// <param name="instanceInfo">Instance information.</param>
        public UserJoinLeaveEventArgs(DateTime logAt, string userName, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
            : this(logAt, userName, stayFrom, instanceInfo)
        {
            StayUntil = stayUntil;
        }
    }
}
