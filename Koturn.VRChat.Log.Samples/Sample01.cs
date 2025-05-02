using System;
using System.Text;
using Koturn.VRChat.Log.Samples;


namespace Koturn.VRChat.Log.Samples
{
    internal class Sample01
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
                using (var logParser = new VRCLogParser(filePath))
                {
                    logParser.UserJoined += (sender, e) => Console.WriteLine($@"[{e.LogAt:yyyy-MM-dd HH\:mm\:ss}] Joined user: [{e.UserName}][{e.UserId}]");
                    logParser.UserLeft += (sender, e) => Console.WriteLine($@"[{e.LogAt:yyyy-MM-dd HH\:mm\:ss}] Left user: [{e.UserName}][{e.UserId}]");
                    logParser.Parse();
                }
            }
        }
    }
}
