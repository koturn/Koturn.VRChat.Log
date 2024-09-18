using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="VRCBaseLogWatcher.FileOpened"/>.
    /// </summary>
    public class FileOpenEventArgs : EventArgs
    {
        /// <summary>
        /// Opened or closed file path.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Create instance with specified file path.
        /// </summary>
        /// <param name="filePath">Opened or closed file path.</param>
        public FileOpenEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}
