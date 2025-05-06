using System;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonKillerUnlocked"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp and killer index.
    /// </remarks>
    /// <param name="logFileName">Log file name.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="indexType">Terror index type.</param>
    /// <param name="terrorIndex">Terror (Killer) index.</param>
    public class TonKillerUnlockedEventArgs(string? logFileName, DateTime logAt, TonTerrorIndexType indexType, int terrorIndex)
        : VRCLogEventArgs(logFileName, logAt)
    {
        /// <summary>
        /// Terror index type.
        /// </summary>
        public TonTerrorIndexType IndexType { get; } = indexType;
        /// <summary>
        /// Terror (Killer) index.
        /// </summary>
        public int TerrorIndex { get; } = terrorIndex;
    }
}
