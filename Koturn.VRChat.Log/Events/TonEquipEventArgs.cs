using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonEquipped"/> event.
    /// </summary>
    public class TonEquipEventArgs : LogEventArgs
    {
        /// <summary>
        /// New equipped item index.
        /// </summary>
        public int ItemIndex { get; }
        /// <summary>
        /// Last equipped item index.
        /// </summary>
        public int LastItemIndex { get; }

        /// <summary>
        /// Create instance with timestamp, item index and last equiped item index.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="itemIndex">Equipped item index.</param>
        /// <param name="lastItemIndex">Last equipped item index.</param>
        public TonEquipEventArgs(DateTime logAt, int itemIndex, int lastItemIndex)
            : base(logAt)
        {
            ItemIndex = itemIndex;
            LastItemIndex = lastItemIndex;
        }
    }
}
