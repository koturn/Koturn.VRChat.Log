using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="VRCLogWatcher.FileClosed"/>.
    /// </summary>
    public class FileCloseEventArgs : FileOpenEventArgs
    {
        /// <summary>
        /// First log timestamp.
        /// </summary>
        public DateTime LogFrom { get; }
        /// <summary>
        /// Last log timestamp.
        /// </summary>
        public DateTime LogUntil { get; }

        /// <summary>
        /// Create instance with specified file path and timestamps.
        /// </summary>
        /// <param name="filePath">Opened or closed file path.</param>
        /// <param name="logFrom">First log timestamp.</param>
        /// <param name="logUntil">Last log timestamp.</param>
        public FileCloseEventArgs(string filePath, DateTime logFrom, DateTime logUntil)
            : base(filePath)
        {
            LogFrom = logFrom;
            LogUntil = logUntil;
        }
    }
}
