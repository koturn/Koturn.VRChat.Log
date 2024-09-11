Koturn.VRChat.Log
=================

Simple VRChat Log Parser and Watcher library.

## Usage

### Log Parser

```cs
using System;
using System.Text;
using Koturn.VRChat.Log;


namespace VRCLogParserSample
{
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            var logParser = new VRCLogParser();
            logParser.UserJoined += (sender, e) =>
            {
                Console.WriteLine($"[{e.LogAt:yyyy-MM-dd HH\\:mm\\:ss}] Joined user: {e.UserName}");
            };
            logParser.UserLeft += (sender, e) =>
            {
                Console.WriteLine($"[{e.LogAt:yyyy-MM-dd HH\\:mm\\:ss}] Left user: {e.UserName}");
            };

            foreach (var filePath in VRCLogParser.GetLogFilePaths())
            {
                logParser.Parse(filePath);
                logParser.Terminate();
            }
        }
    }
}
```

### Log Watcher

```cs
using System;
using System.Text;
using Koturn.VRChat.Log;


namespace VRCLogWatcherSample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            using (var logWatcher = new VRCLogWatcher())
            {
                logWatcher.FileOpened += (sender, e) => Console.WriteLine($"Start watching: {e.FilePath}");
                logWatcher.FileClosed += (sender, e) => Console.WriteLine($"End watching: {e.FilePath}");
                logWatcher.StartWatching();  // Start watching and Read to end of latest log file.

                logWatcher.UserJoined += (sender, e) =>
                {
                    Console.WriteLine($"[{e.LogAt:yyyy-MM-dd HH\\:mm\\:ss}] Joined user: {e.UserName}");
                };
                logWatcher.UserLeft += (sender, e) =>
                {
                    Console.WriteLine($"[{e.LogAt:yyyy-MM-dd HH\\:mm\\:ss}] Left user: {e.UserName}");
                };

                string? read;
                while ((read = Console.ReadLine()) != "exit")
                {
                    // Do nothing.
                }

                logWatcher.StopWatching();
            }
        }
    }
}
```

## LICENSE

This software is released under the MIT License, see [LICENSE](LICENSE "LICENSE").
