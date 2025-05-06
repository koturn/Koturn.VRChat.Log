using System;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.Downloaded"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with specified log timestamp, URL, Download type and instance information.
    /// </remarks>
    /// <param name="logFileName">Log file name.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="url">Download URL.</param>
    /// <param name="type">Download type.</param>
    /// <param name="instanceInfo">Instance information.</param>
    public class DownloadEventArgs(string? logFileName, DateTime logAt, string url, DownloadType type, InstanceInfo instanceInfo)
        : VRCLogEventArgs(logFileName, logAt)
    {
        /// <summary>
        /// Download URL.
        /// </summary>
        public string Url { get; } = url;
        /// <summary>
        /// Download type.
        /// </summary>
        public DownloadType Type { get; } = type;
        /// <summary>
        /// Instance information.
        /// </summary>
        public InstanceInfo InstanceInfo { get; } = instanceInfo;
    }
}
