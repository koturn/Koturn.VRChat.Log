Koturn.VRChat.Log
=================

[![.NET](https://github.com/koturn/Koturn.VRChat.Log/actions/workflows/dotnet.yml/badge.svg)](https://github.com/koturn/Koturn.VRChat.Log/actions/workflows/dotnet.yml)

[![Build status](https://ci.appveyor.com/api/projects/status/jxk0tr4g5oamyflk/branch/main?svg=true)](https://ci.appveyor.com/project/koturn/koturn-vrchat-log/branch/main)

Simple VRChat Log Parser and Watcher library.

## Supported Frameworks

- .NET Standard 2.0
- .NET 8.0
- .NET 9.0
- .NET 10.0

## Usage (Sample code)

All of the following is sample code that notifies a user of a Join/Leave.

### Log Parser

`VRCLogParser` provides basic log detection events.

```cs
using System;
using System.Text;
using Koturn.VRChat.Log;


namespace VRCLogParserSample
{
    /// <summary>
    /// Sample program for <see cref="VRCLogParser"/>, which is event style.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The entry point of this program.
        /// </summary>
        static void Main()
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
```

### Log Parser (Inheritance style)

`VRCCoreLogParser` provides callback methods that are called when basic logging is detected.
By overriding those callbacks, you can implement the behavior when each log is detected.

```cs
using System;
using System.Text;
using Koturn.VRChat.Log;


namespace VRCCoreLogParserSample
{
    /// <summary>
    /// Sample program for <see cref="VRCCoreLogParser"/>, which is inheritance style.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The entry point of this program.
        /// </summary>
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Parse all existing VRChat log files.
            foreach (var filePath in VRCBaseLogParser.GetLogFilePaths())
            {
                using (var logParser = new MyVRCLogParser(filePath))
                {
                    logParser.Parse();
                }
            }
        }
    }

    /// <summary>
    /// VRChat log parser for inheritance style.
    /// </summary>
    internal class MyVRCLogParser : VRCCoreLogParser
    {
        /// <summary>
        /// Create <see cref="MyVRCLogParser"/> instance.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        public MyVRCLogParser(string filePath)
            : base(filePath)
        {
        }

        /// <summary>
        /// Output user join log.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID.</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnUserJoined(string userName, string? userId, DateTime stayFrom, InstanceInfo instanceInfo)
        {
            // Since the base.OnUserJoined() is empty, there is no need to call it.
            Console.WriteLine($@"[{LogAt:yyyy-MM-dd HH\:mm\:ss}] Joined user: [{userName}][{userId}]");
        }

        /// <summary>
        /// Output user leave log.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID.</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="stayUntil">A timestamp the user left.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnUserLeft(string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
        {
            // Since the base.OnUserLeft() is empty, there is no need to call it.
            Console.WriteLine($@"[{LogAt:yyyy-MM-dd HH\:mm\:ss}] Left user: [{userName}][{userId}]");
        }
    }
}
```

### Log Parser (Full customed style)

`VRCBaseLogParser` splits each message in the log file and calls `OnLogDetected()`.
By overriding `OnLogDetected()`, you can implement behavior for arbitrary logs.

```cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Koturn.VRChat.Log;
using Koturn.VRChat.Log.Enums;


namespace VRCBaseLogParserSample
{
    /// <summary>
    /// Sample program for <see cref="VRCBaseLogParser"/>, which is inheritance style.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The entry point of this program.
        /// </summary>
        static void Main()
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
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines (First line does not contain timestamp and level part, just message only).</param>
        /// <returns>True if any of the log parsing defined in this class succeeds, otherwise false.</returns>
        protected override bool OnLogDetected(VRCLogLevel level, List<string> logLines)
        {
            var firstLine = logLines[0];
            return ParseAsUserJoinLeaveLog(LogAt, firstLine);
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
```

### Log Parser (using Log Reader)

```cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Koturn.VRChat.Log.Samples;


namespace VRCLogReaderSample
{
    /// <summary>
    /// Sample program for <see cref="VRCLogReader"/>.
    /// </summary>
    internal class Program
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
        static void Main()
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
```

### Log Watcher

`VRCLogWatcher` is a class for monitoring VRChat logs in real time.
This class provides basic log detection events only.

```cs
using System;
using System.Text;
using Koturn.VRChat.Log;


namespace VRCLogWatcherSample
{
    /// <summary>
    /// Sample program for <see cref="VRCLogWatcher"/>.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The entry point of this program.
        /// </summary>
        static void Main()
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
```

## LICENSE

This software is released under the MIT License, see [LICENSE](LICENSE "LICENSE").
