using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for following events:
    /// <list type="bullet">
    ///   <item><see cref="IVRCExLogEvent.BulletTimeAgentSaved"/></item>
    ///   <item><see cref="IVRCExLogEvent.IdleCubeSaved"/></item>
    ///   <item><see cref="IVRCExLogEvent.IdleDefenseSaved"/></item>
    ///   <item><see cref="IVRCExLogEvent.IdleHomeSaved"/></item>
    ///   <item><see cref="IVRCExLogEvent.MagicalCursedLandSaved"/></item>
    ///   <item><see cref="IVRCExLogEvent.RhapsodySaved"/></item>
    ///   <item><see cref="IVRCExLogEvent.TerrorsOfNowhereSaved"/></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with log timestamp and save data text.
    /// </remarks>
    /// <param name="logFilePath">Log file path.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="saveText">Save data text.</param>
    public class SaveEventArgs(string? logFilePath, DateTime logAt, string saveText)
        : VRCLogEventArgs(logFilePath, logAt)
    {
        /// <summary>
        /// Save data text.
        /// </summary>
        public string SaveText { get; } = saveText;
    }
}
