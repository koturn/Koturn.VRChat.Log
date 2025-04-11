using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="VRCBaseLogWatcher.FileOpened"/>.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with specified file path.
    /// </remarks>
    /// <param name="filePath">Opened or closed file path.</param>
    public class FileOpenEventArgs(string filePath)
        : EventArgs
    {
        /// <summary>
        /// Opened or closed file path.
        /// </summary>
        public string FilePath { get; } = filePath;
    }
}
