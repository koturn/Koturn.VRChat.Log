Koturn.VRChat.Log
=================

Simple VRChat Log Parser and Watcher library.

## Usage

### Log Parser

```cs
namespace VRCLogParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var logParser = new VRCLogParser();
            logParser.UserJoined += (sender, e) =>
            {
                Console.WriteLine($"Joined user: {e.UserName}");
            };
            logParser.UserLeft += (sender, e) =>
            {
                Console.WriteLine($"Left user: {e.UserName}");
            };

            foreach (var filePath in VRCLogParser.GetLogFilePaths())
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    logParser.Parse(sr);
                }
                logParser.ClearState();
            }
        }
    }
}
```

### Log Watcher

```cs
namespace VRCLogWatcher
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
                    Console.WriteLine($"Joined user: {e.UserName}");
                };
                logWatcher.UserLeft += (sender, e) =>
                {
                    Console.WriteLine($"Left user: {e.UserName}");
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
