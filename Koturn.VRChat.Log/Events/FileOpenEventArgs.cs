using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="VRCLogWatcher.FileOpened"/> or <see cref="VRCLogWatcher.FileClosed"/> event.
    /// </summary>
    public class FileEventArgs : EventArgs
    {
        /// <summary>
        /// Opened or closed file path.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Create instance with specified file path.
        /// </summary>
        /// <param name="filePath">Opened or closed file path.</param>
        public FileEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}
