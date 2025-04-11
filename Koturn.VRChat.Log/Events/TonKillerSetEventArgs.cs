using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCExLogEvent.TonKillerSet"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with timestamp, terror indices and round name.
    /// </remarks>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="terrorIndex1">First terror index.</param>
    /// <param name="terrorIndex2">Second terror index.</param>
    /// <param name="terrorIndex3">Third terror index.</param>
    /// <param name="roundName">Round name.</param>
    public class TonKillerSetEventArgs(DateTime logAt, int terrorIndex1, int terrorIndex2, int terrorIndex3, string roundName) : LogEventArgs(logAt)
    {
        /// <summary>
        /// First terror index.
        /// </summary>
        public int TerrorIndex1 { get; } = terrorIndex1;
        /// <summary>
        /// Second terror index.
        /// </summary>
        public int TerrorIndex2 { get; } = terrorIndex2;
        /// <summary>
        /// Third terror index.
        /// </summary>
        public int TerrorIndex3 { get; } = terrorIndex3;
        /// <summary>
        /// Round name.
        /// </summary>
        public string RoundName { get; } = roundName;
    }
}
