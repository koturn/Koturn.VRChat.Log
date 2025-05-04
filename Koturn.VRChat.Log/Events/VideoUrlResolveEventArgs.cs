using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.VideoUrlResolving"/> or <see cref="IVRCCoreLogEvent.VideoUrlResolved"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with log timestamp, Video URL and instance information.
    /// </remarks>
    /// <param name="logFilePath">Log file path.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="url">Video URL.</param>
    /// <param name="instanceInfo">Instance information.</param>
    public class VideoUrlResolveEventArgs(string? logFilePath, DateTime logAt, string url, InstanceInfo instanceInfo)
        : VRCLogEventArgs(logFilePath, logAt)
    {
        /// <summary>
        /// Video URL.
        /// </summary>
        public string Url { get; } = url;
        /// <summary>
        /// Resolved Video URL.
        /// </summary>
        public string? ResolvedUrl { get; }
        /// <summary>
        /// Instance information.
        /// </summary>
        public InstanceInfo InstanceInfo { get; } = instanceInfo;

        /// <summary>
        /// Create instance with log timestamp, Video URL, Resolved Video URL and instance information.
        /// </summary>
        /// <param name="logFilePath">Log file path.</param>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="url">Video URL.</param>
        /// <param name="resolvedUrl">Resolved Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        public VideoUrlResolveEventArgs(string? logFilePath, DateTime logAt, string url, string resolvedUrl, InstanceInfo instanceInfo)
            : this(logFilePath, logAt, url, instanceInfo)
        {
            ResolvedUrl = resolvedUrl;
        }
    }
}
