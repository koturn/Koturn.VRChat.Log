using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Koturn.VRChat.Log.Samples;


namespace Koturn.VRChat.Log.Samples
{
    internal class Sample00
    {
        /// <summary>
        /// <para>Regex to extract joined or lefted player's name.</para>
        /// <para><c>\[Behaviour\] OnPlayer(Joined|Left) (.+)$</c></para>
        /// </summary>
        private static readonly Regex _regexJoinLeave = new Regex(
            @"\[Behaviour\] OnPlayer(Joined|Left) (.+) \((usr_[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12})\)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// The entry point of this program.
        /// </summary>
        public static void Test()
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Parse all existing VRChat log files.
            foreach (var filePath in VRCBaseLogParser.GetLogFilePaths())
            {
                using (var logReader = new VRCLogReader(filePath))
                {
                    var logLines = new List<string>(128);
                    while ((logLines = logReader.ReadLogEntry(out var logAt, out var logLevel)) != null)
                    {
                        var match = _regexJoinLeave.Match(logLines[0]);
                        if (!match.Success)
                        {
                            continue;
                        }

                        var groups = match.Groups;
                        var joinLeaveKind = groups[1].Value;
                        var userName = groups[2].Value;
                        var userId = groups[3].Value;
                        switch (joinLeaveKind)
                        {
                            case "Joined":
                                Console.WriteLine($@"[{logAt:yyyy-MM-dd HH\:mm\:ss}] Joined user: [{userName}][{userId}]");
                                break;
                            case "Left":
                                Console.WriteLine($@"[{logAt:yyyy-MM-dd HH\:mm\:ss}] Left user: [{userName}][{userId}]");
                                break;
                        }
                    }
                }
            }
        }
    }
}
