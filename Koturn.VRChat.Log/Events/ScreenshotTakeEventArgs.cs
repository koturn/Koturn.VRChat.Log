using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.ScreenshotTook"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp, screenshort file path and instance information.
    /// </remarks>
    /// <param name="logFileName">Log file name.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="filePath">Screenshort file path.</param>
    /// <param name="instanceInfo">Instance information.</param>
    public class ScreenshotTakeEventArgs(string? logFileName, DateTime logAt, string filePath, InstanceInfo instanceInfo)
        : VRCLogEventArgs(logFileName, logAt)
    {
        /// <summary>
        /// Screenshort file path.
        /// </summary>
        public string FilePath { get; } = filePath;
        /// <summary>
        /// Instance information.
        /// </summary>
        public InstanceInfo InstanceInfo { get; } = instanceInfo;
    }
}
