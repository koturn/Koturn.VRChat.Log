using System;
using System.Text;
using Koturn.VRChat.Log;


namespace Koturn.VRChat.Log.Samples
{
    internal class Sample04
    {
        /// <summary>
        /// The entry point of this program.
        /// </summary>
        public static void Test()
        {
            Console.OutputEncoding = Encoding.UTF8;

            using (var logWatcher = new VRCLogWatcher())
            {
                logWatcher.FileOpened += (sender, e) => Console.WriteLine($"Start watching: [{e.FilePath}]");
                logWatcher.FileClosed += (sender, e) => Console.WriteLine($"End watching: [{e.FilePath}]");
                logWatcher.Start();  // Start watching and read to end of latest log file.

                logWatcher.UserJoined += (sender, e) => Console.WriteLine($@"[{e.LogAt:yyyy-MM-dd HH\:mm\:ss}] Joined user: [{e.UserName}][{e.UserId}]");
                logWatcher.UserLeft += (sender, e) => Console.WriteLine($@"[{e.LogAt:yyyy-MM-dd HH\:mm\:ss}] Left user: [{e.UserName}][{e.UserId}]");

                string? read;
                while ((read = Console.ReadLine()) != "exit")
                {
                    // Do nothing.
                }

                // Stop() is called in Dispose().
                // logWatcher.Stop();
            }
        }
    }
}
