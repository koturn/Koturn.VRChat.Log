using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="VRCLogParser.IdleHomeSaved"/> or <see cref="VRCLogParser.TerrorsOfNowhereSaved"/> event.
    /// </summary>
    public class SaveEventArgs : LogEventArgs
    {
        /// <summary>
        /// Save data text.
        /// </summary>
        public string SaveText { get; }

        /// <summary>
        /// Create instance with log timestamp and save data text.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="saveText">Save data text.</param>
        public SaveEventArgs(DateTime logAt, string saveText)
            : base(logAt)
        {
            SaveText = saveText;
        }
    }
}
