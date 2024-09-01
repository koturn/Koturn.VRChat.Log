using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="VRCLogParser.ScreenshotTook"/> event.
    /// </summary>
    public class ScreenshotTakeEventArgs : LogEventArgs
    {
        /// <summary>
        /// Screenshort file path.
        /// </summary>
        public string FilePath { get; }
        /// <summary>
        /// Instance information.
        /// </summary>
        public InstanceInfo InstanceInfo { get; }

        /// <summary>
        /// Create instance with timestamp, screenshort file path and instance information.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="filePath">Screenshort file path.</param>
        /// <param name="instanceInfo">Instance information.</param>
        public ScreenshotTakeEventArgs(DateTime logAt, string filePath, InstanceInfo instanceInfo)
            : base(logAt)
        {
            FilePath = filePath;
            InstanceInfo = instanceInfo;
        }
    }
}
