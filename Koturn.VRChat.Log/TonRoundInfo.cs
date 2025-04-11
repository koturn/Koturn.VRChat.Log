using System;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Represents Terrors of Nowhere round information.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with round timestamp, round name and terror index.
    /// </remarks>
    /// <param name="roundAt">Time stamp at start of round.</param>
    /// <param name="roundName">Round name.</param>
    /// <param name="terrorIndex">First terror index.</param>
    public class TonRoundInfo(DateTime roundAt, string roundName, int terrorIndex)
    {
        /// <summary>
        /// Timestamp at start of round.
        /// </summary>
        public DateTime RoundAt { get; } = roundAt;
        /// <summary>
        /// Round result.
        /// </summary>
        public TonRoundResult Result { get; internal set; }
        /// <summary>
        /// Round index.
        /// </summary>
        public int RoundIndex { get; internal set; }
        /// <summary>
        /// Round name.
        /// </summary>
        public string RoundName { get; } = roundName;
        /// <summary>
        /// First terror index.
        /// </summary>
        public int TerrorIndex1 { get; } = terrorIndex;
        /// <summary>
        /// Second terror index.
        /// </summary>
        public int? TerrorIndex2 { get; }
        /// <summary>
        /// Third terror index.
        /// </summary>
        public int? TerrorIndex3 { get; }
        /// <summary>
        /// Place index.
        /// </summary>
        public int? PlaceIndex { get; internal set; }
        /// <summary>
        /// Place name.
        /// </summary>
        public string? PlaceName { get; internal set; }
        /// <summary>
        /// Equipped item index.
        /// </summary>
        public int? ItemIndex { get; set; }

        /// <summary>
        /// Create instance with round timestamp, round name and two terror indice.
        /// </summary>
        /// <param name="roundAt">Time stamp at start of round.</param>
        /// <param name="roundName">Round name.</param>
        /// <param name="terrorIndex1">First terror index.</param>
        /// <param name="terrorIndex2">Second terror index.</param>
        public TonRoundInfo(DateTime roundAt, string roundName, int terrorIndex1, int terrorIndex2)
            : this(roundAt, roundName, terrorIndex1)
        {
            TerrorIndex2 = terrorIndex2;
        }

        /// <summary>
        /// Create instance with round timestamp, round name and three terror indice.
        /// </summary>
        /// <param name="roundAt">Time stamp at start of round.</param>
        /// <param name="roundName">Round name.</param>
        /// <param name="terrorIndex1">First terror index.</param>
        /// <param name="terrorIndex2">Second terror index.</param>
        /// <param name="terrorIndex3">Third terror index.</param>
        public TonRoundInfo(DateTime roundAt, string roundName, int terrorIndex1, int terrorIndex2, int terrorIndex3)
            : this(roundAt, roundName, terrorIndex1, terrorIndex2)
        {
            TerrorIndex3 = terrorIndex3;
        }
    }
}
