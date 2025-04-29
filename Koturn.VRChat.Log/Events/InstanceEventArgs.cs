using System;

namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.JoinedToInstance"/> or <see cref="IVRCCoreLogEvent.LeftFromInstance"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with log timestamp and instance information.
    /// </remarks>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="instanceInfo">Instance information.</param>
    public class InstanceEventArgs(DateTime logAt, InstanceInfo instanceInfo)
        : VRCLogEventArgs(logAt)
    {
        /// <summary>
        /// Instance information.
        /// </summary>
        public InstanceInfo InstanceInfo { get; } = instanceInfo;
    }
}
