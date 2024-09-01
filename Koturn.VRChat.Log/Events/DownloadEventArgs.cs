using System;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="VRCLogParser.Downloaded"/> event.
    /// </summary>
    public class DownloadEventArgs : LogEventArgs
    {
        /// <summary>
        /// Download URL.
        /// </summary>
        public string Url { get; }
        /// <summary>
        /// Download type.
        /// </summary>
        public DownloadType Type { get; }
        /// <summary>
        /// Instance information.
        /// </summary>
        public InstanceInfo InstanceInfo { get; }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="url">Download URL.</param>
        /// <param name="type"></param>
        /// <param name="instanceInfo"></param>
        public DownloadEventArgs(DateTime logAt, string url, DownloadType type, InstanceInfo instanceInfo)
            : base(logAt)
        {
            Url = url;
            Type = type;
            InstanceInfo = instanceInfo;
        }
    }
}
