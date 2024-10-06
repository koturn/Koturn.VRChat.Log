using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonKillerSet"/> event.
    /// </summary>
    public class TonKillerSetEventArgs : LogEventArgs
    {
        /// <summary>
        /// First terror index.
        /// </summary>
        public int TerrorIndex1 { get; }
        /// <summary>
        /// Second terror index.
        /// </summary>
        public int TerrorIndex2 { get; }
        /// <summary>
        /// Third terror index.
        /// </summary>
        public int TerrorIndex3 { get; }
        /// <summary>
        /// Round name.
        /// </summary>
        public string RoundName { get; }

        /// <summary>
        /// Create instance with timestamp, terror indices and round name.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="terrorIndex1">First terror index.</param>
        /// <param name="terrorIndex2">Second terror index.</param>
        /// <param name="terrorIndex3">Third terror index.</param>
        /// <param name="roundName">Round name.</param>
        public TonKillerSetEventArgs(DateTime logAt, int terrorIndex1, int terrorIndex2, int terrorIndex3, string roundName)
            : base(logAt)
        {
            TerrorIndex1 = terrorIndex1;
            TerrorIndex2 = terrorIndex2;
            TerrorIndex3 = terrorIndex3;
            RoundName = roundName;
        }
    }
}
