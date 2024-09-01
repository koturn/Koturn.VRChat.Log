using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="VRCLogParser.VideoUrlResolving"/> or <see cref="VRCLogParser.VideoUrlResolved"/> event.
    /// </summary>
    public class VideoUrlResolveEventArgs : LogEventArgs
    {
        /// <summary>
        /// Video URL.
        /// </summary>
        public string Url { get; }
        /// <summary>
        /// Resolved Video URL.
        /// </summary>
        public string? ResolvedUrl { get; }
        /// <summary>
        /// Instance information.
        /// </summary>
        public InstanceInfo InstanceInfo { get; }

        /// <summary>
        /// Create instance with log timestamp, Video URL and instance information.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="url">Video URL.</param>
        /// <param name="instanceInfo">Instance information.</param>
        public VideoUrlResolveEventArgs(DateTime logAt, string url, InstanceInfo instanceInfo)
            : base(logAt)
        {
            Url = url;
            InstanceInfo = instanceInfo;
        }

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
