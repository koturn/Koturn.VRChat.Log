using System;
using System.Text;
using Koturn.VRChat.Log.Samples;


namespace Koturn.VRChat.Log.Samples
{
    internal class Sample02
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
        /// <param name="userId">User ID.</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnUserJoined(DateTime logAt, string userName, string? userId, DateTime stayFrom, InstanceInfo instanceInfo)
        {
            // Since the base.OnUserJoined() is empty, there is no need to call it.
            Console.WriteLine($@"[{logAt:yyyy-MM-dd HH\:mm\:ss}] Joined user: [{userName}][{userId}]");
        }

        /// <summary>
        /// Output user leave log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID.</param>
        /// <param name="stayFrom">A timestamp the user joined.</param>
        /// <param name="instanceInfo">Instance information.</param>
        protected override void OnUserLeft(DateTime logAt, string userName, string? userId, DateTime stayFrom, DateTime? stayUntil, InstanceInfo instanceInfo)
        {
            // Since the base.OnUserLeft() is empty, there is no need to call it.
            Console.WriteLine($@"[{logAt:yyyy-MM-dd HH\:mm\:ss}] Left user: [{userName}][{userId}]");
        }
    }
}
