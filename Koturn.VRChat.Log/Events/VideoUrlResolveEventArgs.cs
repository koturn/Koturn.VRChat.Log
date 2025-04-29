using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.VideoUrlResolving"/> or <see cref="IVRCCoreLogEvent.VideoUrlResolved"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with log timestamp, Video URL and instance information.
    /// </remarks>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="url">Video URL.</param>
    /// <param name="instanceInfo">Instance information.</param>
    public class VideoUrlResolveEventArgs(DateTime logAt, string url, InstanceInfo instanceInfo)
        : VRCLogEventArgs(logAt)
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
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="url">Video URL.</param>
        /// <param name="resolvedUrl">Resolved Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        public VideoUrlResolveEventArgs(DateTime logAt, string url, string resolvedUrl, InstanceInfo instanceInfo)
            : this(logAt, url, instanceInfo)
        {
            ResolvedUrl = resolvedUrl;
        }
    }
}
