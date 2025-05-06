using System;

namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.JoinedToInstance"/> or <see cref="IVRCCoreLogEvent.LeftFromInstance"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with log timestamp and instance information.
    /// </remarks>
    /// <param name="logFileName">Log file name.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="instanceInfo">Instance information.</param>
    public class InstanceEventArgs(string? logFileName, DateTime logAt, InstanceInfo instanceInfo)
        : VRCLogEventArgs(logFileName, logAt)
    {
        /// <summary>
        /// Instance information.
        /// </summary>
        public InstanceInfo InstanceInfo { get; } = instanceInfo;
    }
}
