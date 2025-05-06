using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonEquipped"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp, item index and last equiped item index.
    /// </remarks>
    /// <param name="logFileName">Log file name.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="itemIndex">Equipped item index.</param>
    /// <param name="lastItemIndex">Last equipped item index.</param>
    public class TonEquipEventArgs(string? logFileName, DateTime logAt, int itemIndex, int lastItemIndex)
        : VRCLogEventArgs(logFileName, logAt)
    {
        /// <summary>
        /// New equipped item index.
        /// </summary>
        public int ItemIndex { get; } = itemIndex;
        /// <summary>
        /// Last equipped item index.
        /// </summary>
        public int LastItemIndex { get; } = lastItemIndex;
    }
}
