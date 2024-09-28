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
        public const string PickupObjectPattern = @"^Pickup object: '([^']+)' equipped = (True|False), is equippable = (True|False), last input method = (.+), is auto equip controller = (True|False)$";
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect drop object log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        public const string DropObjectPattern = @"^Drop object: '([^']+), was equipped = (True|False)' (.+), last input method = (.+)$";
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect Idle Home save log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        public const string IdleHomeSavePattern = @"^\[🦀 Idle Home 🦀\] Saved \d{2}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}: (.+)$";


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
    }
}
