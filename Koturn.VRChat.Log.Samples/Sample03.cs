using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Koturn.VRChat.Log;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log.Samples
{
    internal class Sample03
    {
        /// <summary>
        /// The entry point of this program.
        /// </summary>
        public static void Test()
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Parse all existing VRChat log files.
            foreach (var filePath in VRCBaseLogParser.GetLogFilePaths())
            {
                using (var logParser = new FullCustomedVRCLogParser(filePath))
                {
                    logParser.Parse();
                }
            }
        }
    }

    /// <summary>
    /// Full customed VRChat log parser.
    /// </summary>
    internal class FullCustomedVRCLogParser : VRCBaseLogParser
    {
        /// <summary>
        /// <para>Regex to extract joined or lefted player's name.</para>
        /// <para><c>\[Behaviour\] OnPlayer(Joined|Left) (.+)$</c></para>
        /// </summary>
        private static readonly Regex _regexJoinLeave = new Regex(
            @"\[Behaviour\] OnPlayer(Joined|Left) (.+) \((usr_[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12})\)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Create <see cref="VRCLogParser"/> instance.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        public FullCustomedVRCLogParser(string filePath)
            : base(filePath)
        {
        }

        /// <summary>
        /// Process detected log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines (First line does not contain timestamp and level part, just message only).</param>
        /// <returns>True if any of the log parsing defined in this class succeeds, otherwise false.</returns>
        protected override bool OnLogDetected(DateTime logAt, VRCLogLevel level, List<string> logLines)
        {
            var firstLine = logLines[0];
            return ParseAsUserJoinLeaveLog(logAt, firstLine);
        }

        /// <summary>
        /// Parse first log line as user joined or left log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private static bool ParseAsUserJoinLeaveLog(DateTime logAt, string firstLine)
        {
            var match = _regexJoinLeave.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;
            var joinLeaveKind = groups[1].Value;
            var userName = groups[2].Value;
            var userId = groups[3].Value;
            switch (joinLeaveKind)
            {
                case "Joined":
                    Console.WriteLine($@"[{logAt:yyyy-MM-dd HH\:mm\:ss}] Joined user: [{userName}][{userId}]");
                    return true;
                case "Left":
                    Console.WriteLine($@"[{logAt:yyyy-MM-dd HH\:mm\:ss}] Left user: [{userName}][{userId}]");
                    return true;
                default:
                    return false;
            }
        }
    }
}
