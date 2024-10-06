using System;

namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.JoinedToInstance"/> or <see cref="IVRCCoreLogEvent.LeftFromInstance"/> event.
    /// </summary>
    public class JoinLeaveInstanceEventArgs : LogEventArgs
    {
        /// <summary>
        /// Instance information.
        /// </summary>
        public InstanceInfo InstanceInfo { get; }

        /// <summary>
        /// Create instance with log timestamp and instance information.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="instanceInfo">Instance information.</param>
        public JoinLeaveInstanceEventArgs(DateTime logAt, InstanceInfo instanceInfo)
            : base(logAt)
        {
            InstanceInfo = instanceInfo;
        }
    }
}
