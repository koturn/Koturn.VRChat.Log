using Koturn.VRChat.Log.Enums;
using System;

namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonKillerUnlocked"/> event.
    /// </summary>
    public class TonKillerUnlockedEventArgs : LogEventArgs
    {
        /// <summary>
        /// Terror index type.
        /// </summary>
        public TonTerrorIndexType IndexType { get; }
        /// <summary>
        /// Terror (Killer) index.
        /// </summary>
        public int TerrorIndex { get; }

        /// <summary>
        /// Create instance with timestamp and killer index.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="indexType">Terror index type.</param>
        /// <param name="terrorIndex">Terror (Killer) index.</param>
        public TonKillerUnlockedEventArgs(DateTime logAt, TonTerrorIndexType indexType, int terrorIndex)
            : base(logAt)
        {
            IndexType = indexType;
            TerrorIndex = terrorIndex;  
        }
    }
}
