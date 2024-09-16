using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;


namespace Koturn.VRChat.Log.Internals
{
#if NET7_0_OR_GREATER
    internal static partial class RegexHelper
#else
    internal static class RegexHelper
#endif
    {
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect first log line.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        public const string LogLinePattern = @"^(\d{4})\.(\d{2})\.(\d{2}) (\d{2}):(\d{2}):(\d{2}) (\w+)\s+-  (.+)$";
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect video resolved log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        public const string VideoResolvedPattern = @"^URL '(.+)' resolved to '(.+)'$";
        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> to detect Idle Home save log.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Regex)]
        public const string IdleHomeSavePattern = @"^\[🦀 Idle Home 🦀\] Saved \d{2}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}: (.+)$";
        /// <summary>
        /// Options for <see cref="Regex"/> instances.
        /// </summary>
        private const RegexOptions Options = RegexOptions.Compiled | RegexOptions.CultureInvariant;

        /// <summary>
        /// Get <see cref="Regex"/> instance to detect first log line.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect first log line.</returns>
#if NET7_0_OR_GREATER
        [GeneratedRegex(LogLinePattern, Options)]
        public static partial Regex GetLogLineRegex();
#else
        public static Regex GetLogLineRegex() => new Regex(LogLinePattern, Options);
#endif

        /// <summary>
        /// Get <see cref="Regex"/> instance to detect video resolved log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect video resolved log.</returns>
#if NET7_0_OR_GREATER
        [GeneratedRegex(VideoResolvedPattern, Options)]
        public static partial Regex GetVideoResolvedRegex();
#else
        public static Regex GetVideoResolvedRegex() => new Regex(VideoResolvedPattern, Options);
#endif

        /// <summary>
        /// Get <see cref="Regex"/> instance to detect Idle Home save log.
        /// </summary>
        /// <returns><see cref="Regex"/> instance to detect Idle Home save log.</returns>
#if NET7_0_OR_GREATER
        [GeneratedRegex(IdleHomeSavePattern, Options)]
        public static partial Regex GetIdleHomeSaveRegex();
#else
        public static Regex GetIdleHomeSaveRegex() => new Regex(IdleHomeSavePattern, Options);
#endif
    }
}
