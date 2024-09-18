Koturn.VRChat.Log
=================

Simple VRChat Log Parser and Watcher library.

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
                    logParser.UserJoined += (sender, e) => Console.WriteLine($@"[{e.LogAt:yyyy-MM-dd HH\:mm\:ss}] Joined user: {e.UserName}");
                    logParser.UserLeft += (sender, e) => Console.WriteLine($@"[{e.LogAt:yyyy-MM-dd HH\:mm\:ss}] Left user: {e.UserName}");
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
    /// VRChat log parser for inherit style.
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
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="userName">User name.</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnUserJoined(DateTime logAt, string userName, DateTime stayFrom, InstanceInfo instanceInfo)
        {
            // Since the base.OnUserJoined() is empty, there is no need to call it.
            Console.WriteLine($@"[{logAt:yyyy-MM-dd HH\:mm\:ss}] Joined user: {userName}");
        }

        /// <summary>
        /// Output user leave log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="userName">User name.</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnUserLeft(DateTime logAt, string userName, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
        {
            // Since the base.OnUserLeft() is empty, there is no need to call it.
            Console.WriteLine($@"[{logAt:yyyy-MM-dd HH\:mm\:ss}] Left user: {userName}");
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
        private static readonly Regex _regexJoinLeave;

        /// <summary>
        /// Initialize <see cref="_regexJoinLeave"/>.
        /// </summary>
        static FullCustomedVRCLogParser()
        {
            _regexJoinLeave = new Regex(@"\[Behaviour\] OnPlayer(Joined|Left) (.+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }


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
        protected override bool OnLogDetected(DateTime logAt, LogLevel level, List<string> logLines)
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
        private bool ParseAsUserJoinLeaveLog(DateTime logAt, string firstLine)
        {
            var match = _regexJoinLeave.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;
            var joinLeaveKind = groups[1].Value;
            var userName = groups[2].Value;
            switch (joinLeaveKind)
            {
                case "Joined":
                    Console.WriteLine($@"[{logAt:yyyy-MM-dd HH\:mm\:ss}] Joined user: {userName}");
                    return true;
                case "Left":
                    Console.WriteLine($@"[{logAt:yyyy-MM-dd HH\:mm\:ss}] Left user: {userName}");
                    return true;
                default:
                    return false;
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
                logWatcher.FileOpened += (sender, e) => Console.WriteLine($"Start watching: {e.FilePath}");
                logWatcher.FileClosed += (sender, e) => Console.WriteLine($"End watching: {e.FilePath}");
                logWatcher.Start();  // Start watching and read to end of latest log file.

                logWatcher.UserJoined += (sender, e) => Console.WriteLine($@"[{e.LogAt:yyyy-MM-dd HH\:mm\:ss}] Joined user: {e.UserName}");
                logWatcher.UserLeft += (sender, e) => Console.WriteLine($@"[{e.LogAt:yyyy-MM-dd HH\:mm\:ss}] Left user: {e.UserName}");

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
