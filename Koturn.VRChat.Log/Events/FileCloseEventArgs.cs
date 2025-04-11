using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="VRCBaseLogWatcher.FileClosed"/>.
    /// </summary>
    /// <remarks>
    /// Create instance with specified file path and timestamps.
    /// </remarks>
    /// <param name="filePath">Opened or closed file path.</param>
    /// <param name="logFrom">First log timestamp.</param>
    /// <param name="logUntil">Last log timestamp.</param>
    public class FileCloseEventArgs(string filePath, DateTime logFrom, DateTime logUntil)
        : FileOpenEventArgs(filePath)
    {
        /// <summary>
        /// First log timestamp.
        /// </summary>
        public DateTime LogFrom { get; } = logFrom;
        /// <summary>
        /// Last log timestamp.
        /// </summary>
        public DateTime LogUntil { get; } = logUntil;
    }
}
