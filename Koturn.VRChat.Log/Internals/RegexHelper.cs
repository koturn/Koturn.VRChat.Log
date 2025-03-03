#if NET7_0_OR_GREATER
#    define SUPPORT_GENERATED_REGEX
#endif

using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;


namespace Koturn.VRChat.Log.Internals
{
    /// <summary>
    /// Provides some <see cref="Regex"/> instances.
    /// </summary>
#if SUPPORT_GENERATED_REGEX
    internal static partial class RegexHelper
#else
    internal static class RegexHelper
#endif
    {
        /// <summary>
        /// Options for <see cref="Regex"/> instances.
        /// </summary>
        private const RegexOptions Options = RegexOptions.Compiled | RegexOptions.CultureInvariant;

        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect pickup object log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        internal const string PickupObjectPattern = @"^Pickup object: '([^']+)' equipped = (True|False), is equippable = (True|False), last input method = (.+), is auto equip controller = (True|False)$";
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect drop object log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        internal const string DropObjectPattern = @"^Drop object: '([^']+), was equipped = (True|False)' (.+), last input method = (.+)$";
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect Idle Home save log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        internal const string IdleHomeSavePattern = @"^\[🦀 Idle Home 🦀\] Saved \d{2}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}: (.+)$";
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect Terros of Nowhere equipping item log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        internal const string TonEquipPattern = @"^Equipping (\d+)\. Was using (\d+)$";
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect ToN place log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        internal const string TonPlacePattern = @"^This round is taking place at (.+) \((\d+)\) and the round type is (.+)$";
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect Killer setting log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        internal const string TonKillerSettingPattern = @"^Killers have been set - (\d+) (\d+) (\d+) // Round type is (.+)$";
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect Killer enrage level changed log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        internal const string TonKillerEnragePattern = @"^(.+)triggered an Enrage(\d)? State!$";
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect ToN save log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        internal const string TonWinPattern = @"^WE WON, THE ROUND WAS A (\d+) AGAINST (\d+) , (\d+) , (\d+) AND WE WERE HOLDING (\d+)$";


        /// <summary>
        /// Get <see cref="Regex"/> instance to detect pickup object log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect pickup object log.</returns>
#if SUPPORT_GENERATED_REGEX
        [GeneratedRegex(PickupObjectPattern, Options)]
        public static partial Regex GetPickupObjectRegex();
#else
        public static Regex GetPickupObjectRegex() => new Regex(PickupObjectPattern, Options);
#endif

        /// <summary>
        /// Get <see cref="Regex"/> instance to detect drop object log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect drop object log.</returns>
#if SUPPORT_GENERATED_REGEX
        [GeneratedRegex(DropObjectPattern, Options)]
        public static partial Regex GetDropObjectRegex();
#else
        public static Regex GetDropObjectRegex() => new Regex(DropObjectPattern, Options);
#endif

        /// <summary>
        /// Get <see cref="Regex"/> instance to detect Idle Home save log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Idle Home save log.</returns>
#if SUPPORT_GENERATED_REGEX
        [GeneratedRegex(IdleHomeSavePattern, Options)]
        public static partial Regex GetIdleHomeSaveRegex();
#else
        public static Regex GetIdleHomeSaveRegex() => new Regex(IdleHomeSavePattern, Options);
#endif

        /// <summary>
        /// Get <see cref="Regex"/> instance to detect Terros of Nowhere equipping item log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Terros of Nowhere equipping item log.</returns>
#if SUPPORT_GENERATED_REGEX
        [GeneratedRegex(TonEquipPattern, Options)]
        public static partial Regex GetTonEquipRegex();
#else
        public static Regex GetTonEquipRegex() => new Regex(TonEquipPattern, Options);
#endif

        /// <summary>
        /// Get <see cref="Regex"/> instance to detect ToN place log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Killer setting log.</returns>
#if SUPPORT_GENERATED_REGEX
        [GeneratedRegex(TonPlacePattern, Options)]
        public static partial Regex GetTonPlaceRegex();
#else
        public static Regex GetTonPlaceRegex() => new Regex(TonPlacePattern, Options);
#endif

        /// <summary>
        /// Get <see cref="Regex"/> instance to detect Killer setting log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Killer setting log.</returns>
#if SUPPORT_GENERATED_REGEX
        [GeneratedRegex(TonKillerSettingPattern, Options)]
        public static partial Regex GetTonKillerSettingRegex();
#else
        public static Regex GetTonKillerSettingRegex() => new Regex(TonWinPattern, Options);
#endif

        /// <summary>
        /// Get <see cref="Regex"/> instance to detect Killer enrage level changed log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Killer enrage level changed log.</returns>
#if SUPPORT_GENERATED_REGEX
        [GeneratedRegex(TonKillerEnragePattern, Options)]
        public static partial Regex GetTonKillerEnrageRegex();
#else
        public static Regex GetTonKillerEnrageRegex() => new Regex(TonKillerEnragePattern, Options);
#endif

        /// <summary>
        /// Get <see cref="Regex"/> instance to detect Terros of Nowhere winning log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Idle Home save log.</returns>
#if SUPPORT_GENERATED_REGEX
        [GeneratedRegex(TonWinPattern, Options)]
        public static partial Regex GetTonWinRegex();
#else
        public static Regex GetTonWinRegex() => new Regex(TonWinPattern, Options);
#endif
    }
}
