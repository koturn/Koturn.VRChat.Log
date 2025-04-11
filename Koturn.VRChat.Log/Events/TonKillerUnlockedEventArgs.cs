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
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="indexType">Terror index type.</param>
    /// <param name="terrorIndex">Terror (Killer) index.</param>
    public class TonKillerUnlockedEventArgs(DateTime logAt, TonTerrorIndexType indexType, int terrorIndex)
        : LogEventArgs(logAt)
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
