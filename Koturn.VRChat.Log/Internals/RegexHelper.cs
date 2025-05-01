#if NET7_0_OR_GREATER
#    define SUPPORT_GENERATED_REGEX
#endif
#if NET9_0_OR_GREATER
#    define SUPPORT_GENERATED_REGEX_PROPERTY
#endif

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;


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
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect instance close notification log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        internal const string InstanceResetNotificationPattern = @"^\[ModerationManager\] This instance will be reset in (\d+) minutes due to its age.$";
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
        internal const string IdleHomeSavePattern = @"^\[(?:<color=#[0-9A-Fa-f]{6}>)?🦀 Idle Home 🦀(?:</color>)?\] Saved \d{2}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}: (.+)$";
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
        /// <see cref="Regex"/> instance to detect pickup object log.
        /// </summary>
#if !SUPPORT_GENERATED_REGEX
        public static Regex InstanceResetNotificationRegex => _instanceResetNotificationRegex ??= new Regex(InstanceResetNotificationPattern, Options);
        /// <summary>
        /// Cache field of <see cref="InstanceResetNotificationRegex"/>.
        /// </summary>
        private static Regex? _instanceResetNotificationRegex;
#elif SUPPORT_GENERATED_REGEX_PROPERTY
        [GeneratedRegex(InstanceResetNotificationPattern, Options)]
        public static partial Regex InstanceResetNotificationRegex { get; }
#else
        public static Regex InstanceResetNotificationRegex => GetInstanceResetNotificationRegex();
        /// <summary>
        /// Get <see cref="Regex"/> instance to detect pickup object log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect pickup object log.</returns>
        [GeneratedRegex(InstanceResetNotificationPattern, Options)]
        private static partial Regex GetInstanceResetNotificationRegex();
#endif

        /// <summary>
        /// <see cref="Regex"/> instance to detect pickup object log.
        /// </summary>
#if !SUPPORT_GENERATED_REGEX
        public static Regex PickupObjectRegex => _pickupObjectRegex ??= new Regex(PickupObjectPattern, Options);
        /// <summary>
        /// Cache field of <see cref="PickupObjectRegex"/>.
        /// </summary>
        private static Regex? _pickupObjectRegex;
#elif SUPPORT_GENERATED_REGEX_PROPERTY
        [GeneratedRegex(PickupObjectPattern, Options)]
        public static partial Regex PickupObjectRegex { get; }
#else
        public static Regex PickupObjectRegex => GetPickupObjectRegex();
        /// <summary>
        /// Get <see cref="Regex"/> instance to detect pickup object log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect pickup object log.</returns>
        [GeneratedRegex(PickupObjectPattern, Options)]
        private static partial Regex GetPickupObjectRegex();
#endif

        /// <summary>
        /// <see cref="Regex"/> instance to detect drop object log.
        /// </summary>
#if !SUPPORT_GENERATED_REGEX
        public static Regex DropObjectRegex => _dropObjectRegex ??= new Regex(DropObjectPattern, Options);
        /// <summary>
        /// Cache field of <see cref="DropObjectRegex"/>.
        /// </summary>
        private static Regex? _dropObjectRegex;
#elif SUPPORT_GENERATED_REGEX_PROPERTY
        [GeneratedRegex(DropObjectPattern, Options)]
        public static partial Regex DropObjectRegex { get; }
#else
        public static Regex DropObjectRegex => GetDropObjectRegex();
        /// <summary>
        /// Get <see cref="Regex"/> instance to detect drop object log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect drop object log.</returns>
        [GeneratedRegex(DropObjectPattern, Options)]
        private static partial Regex GetDropObjectRegex();
#endif

        /// <summary>
        /// <see cref="Regex"/> instance to detect Idle Home save log.
        /// </summary>
#if !SUPPORT_GENERATED_REGEX
        public static Regex IdleHomeSaveRegex => _idleHomeSaveRegex ??= new Regex(IdleHomeSavePattern, Options);
        /// <summary>
        /// Cache field of <see cref="IdleHomeSaveRegex"/>.
        /// </summary>
        private static Regex? _idleHomeSaveRegex;
#elif SUPPORT_GENERATED_REGEX_PROPERTY
        [GeneratedRegex(IdleHomeSavePattern, Options)]
        public static partial Regex IdleHomeSaveRegex { get; }
#else
        public static Regex IdleHomeSaveRegex => GetIdleHomeSaveRegex();
        /// <summary>
        /// Get <see cref="Regex"/> instance to detect Idle Home save log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Idle Home save log.</returns>
        [GeneratedRegex(IdleHomeSavePattern, Options)]
        private static partial Regex GetIdleHomeSaveRegex();
#endif

        /// <summary>
        /// <see cref="Regex"/> instance to detect Terros of Nowhere equipping item log.
        /// </summary>
#if !SUPPORT_GENERATED_REGEX
        public static Regex TonEquipRegex => _tonEquipRegex ??= new Regex(TonEquipPattern, Options);
        /// <summary>
        /// Cache field of <see cref="TonEquipRegex"/>.
        /// </summary>
        private static Regex? _tonEquipRegex;
#elif SUPPORT_GENERATED_REGEX_PROPERTY
        [GeneratedRegex(TonEquipPattern, Options)]
        public static partial Regex TonEquipRegex { get; }
#else
        public static Regex TonEquipRegex => GetTonEquipRegex();
        /// <summary>
        /// Get <see cref="Regex"/> instance to detect Terros of Nowhere equipping item log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Terros of Nowhere equipping item log.</returns>
        [GeneratedRegex(TonEquipPattern, Options)]
        private static partial Regex GetTonEquipRegex();
#endif

        /// <summary>
        /// <see cref="Regex"/> instance to detect ToN place log.
        /// </summary>
#if !SUPPORT_GENERATED_REGEX
        public static Regex TonPlaceRegex => _tonPlaceRegex ??= new Regex(TonPlacePattern, Options);
        /// <summary>
        /// Cache field of <see cref="TonPlaceRegex"/>.
        /// </summary>
        private static Regex? _tonPlaceRegex;
#elif SUPPORT_GENERATED_REGEX_PROPERTY
        [GeneratedRegex(TonPlacePattern, Options)]
        public static partial Regex TonPlaceRegex { get; }
#else
        public static Regex TonPlaceRegex => GetTonPlaceRegex();
        /// <summary>
        /// Get <see cref="Regex"/> instance to detect ToN place log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Killer setting log.</returns>
        [GeneratedRegex(TonPlacePattern, Options)]
        private static partial Regex GetTonPlaceRegex();
#endif

        /// <summary>
        /// <see cref="Regex"/> instance to detect Killer setting log.
        /// </summary>
#if !SUPPORT_GENERATED_REGEX
        public static Regex TonKillerSettingRegex => _tonKillerSettingRegex ??= new Regex(TonWinPattern, Options);
        /// <summary>
        /// Cache field of <see cref="TonKillerSettingRegex"/>.
        /// </summary>
        private static Regex? _tonKillerSettingRegex;
#elif SUPPORT_GENERATED_REGEX_PROPERTY
        [GeneratedRegex(TonKillerSettingPattern, Options)]
        public static partial Regex TonKillerSettingRegex { get; }
#else
        public static Regex TonKillerSettingRegex => GetTonKillerSettingRegex();
        /// <summary>
        /// Get <see cref="Regex"/> instance to detect Killer setting log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Killer setting log.</returns>
        [GeneratedRegex(TonKillerSettingPattern, Options)]
        private static partial Regex GetTonKillerSettingRegex();
#endif

        /// <summary>
        /// <see cref="Regex"/> instance to detect Killer enrage level changed log.
        /// </summary>
#if !SUPPORT_GENERATED_REGEX
        public static Regex TonKillerEnrageRegex => _tonKillerEnrageRegex ??= new Regex(TonKillerEnragePattern, Options);
        /// <summary>
        /// Cache field of <see cref="TonKillerEnrageRegex"/>.
        /// </summary>
        private static Regex? _tonKillerEnrageRegex;
#elif SUPPORT_GENERATED_REGEX_PROPERTY
        [GeneratedRegex(TonKillerEnragePattern, Options)]
        public static partial Regex TonKillerEnrageRegex { get; }
#else
        public static Regex TonKillerEnrageRegex => GetTonKillerEnrageRegex();
        /// <summary>
        /// Get <see cref="Regex"/> instance to detect Killer enrage level changed log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Killer enrage level changed log.</returns>
        [GeneratedRegex(TonKillerEnragePattern, Options)]
        private static partial Regex GetTonKillerEnrageRegex();
#endif

        /// <summary>
        /// <see cref="Regex"/> instance to detect Terros of Nowhere winning log.
        /// </summary>
#if !SUPPORT_GENERATED_REGEX
        public static Regex TonWinRegex => _tonWinRegex ??= new Regex(TonWinPattern, Options);
        /// <summary>
        /// Cache field of <see cref="TonWinRegex"/>.
        /// </summary>
        private static Regex? _tonWinRegex;
#elif SUPPORT_GENERATED_REGEX_PROPERTY
        [GeneratedRegex(TonWinPattern, Options)]
        public static partial Regex TonWinRegex { get; }
#else
        public static Regex TonWinRegex => GetTonWinRegex();
        /// <summary>
        /// Get <see cref="Regex"/> instance to detect Terros of Nowhere winning log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Idle Home save log.</returns>
        [GeneratedRegex(TonWinPattern, Options)]
        private static partial Regex GetTonWinRegex();
#endif
    }
}
