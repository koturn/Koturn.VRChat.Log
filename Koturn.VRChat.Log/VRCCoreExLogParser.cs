using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Koturn.VRChat.Log.Enums;
using Koturn.VRChat.Log.Internals;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// VRChat log file parser, which parses normal log and log in specific world.
    /// </summary>
    public abstract class VRCCoreExLogParser : VRCCoreLogParser
    {
        /// <summary>
        /// ToN save data preamble log line.
        /// </summary>
        public const string TonSaveDataPreamble = "[TERRORS SAVE CODE CREATED. PLEASE MAKE SURE YOU COPY THE ENTIRE THING. DO NOT INCLUDE [START] or [END]]";
        /// <summary>
        /// Rhapsody save data preamble log line.
        /// </summary>
        public const string RhapsodySaveDataPreamble = "セーブが実行されました";

        /// <summary>
        /// Regex to extract Idle Home save data.
        /// </summary>
        private static readonly Regex _regexIdleHomeSave;

        /// <summary>
        /// Initialize regexes.
        /// </summary>
        static VRCCoreExLogParser()
        {
            _regexIdleHomeSave = RegexHelper.GetIdleHomeSaveRegex();
        }


        /// <summary>
        /// World kind.
        /// </summary>
        private WorldKind _worldKind;
        /// <summary>
        /// Indicate next log line is ToN save data.
        /// </summary>
        private bool _isTonSaveData;
        /// <summary>
        /// Indicate next log line is Rhapsody save data.
        /// </summary>
        private bool _isRhapsodySaveData;


        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for <see cref="FileStream"/> and <see cref="StreamReader"/>.</param>
        public VRCCoreExLogParser(string filePath, int bufferSize = 65536)
            : base(filePath, bufferSize)
        {
            _worldKind = WorldKind.NoSpecificWorld;
            _isTonSaveData = false;
            _isRhapsodySaveData = false;
        }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> of VRChat log file.</param>
        /// <param name="bufferSize">Buffer size for <see cref="StreamReader"/>.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="stream"/> open
        /// after the <see cref="VRCCoreLogParser"/> object is disposed; otherwise, false.</param>
        public VRCCoreExLogParser(Stream stream, int bufferSize = 65536, bool leaveOpen = false)
            : base(stream, bufferSize, leaveOpen)
        {
            _worldKind = WorldKind.NoSpecificWorld;
            _isTonSaveData = false;
            _isRhapsodySaveData = false;
        }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="reader"><see cref="TextReader"/> of VRChat log file.</param>
        /// <param name="leaveOpen">true to leave the <paramref name="reader"/> open
        /// after the <see cref="VRCCoreLogParser"/> object is disposed; otherwise, false.</param>
        public VRCCoreExLogParser(TextReader reader, bool leaveOpen = false)
            : base(reader, leaveOpen)
        {
            _worldKind = WorldKind.NoSpecificWorld;
            _isTonSaveData = false;
            _isRhapsodySaveData = false;
        }


        /// <summary>
        /// Load one line of log file and parse it, and fire each event as needed.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="level">Log level.</param>
        /// <param name="logLines">Log lines.</param>
        /// <returns>True if any of the log parsing defined in this class succeeds, otherwise false.</returns>
        protected override bool OnLogDetected(DateTime logAt, LogLevel level, List<string> logLines)
        {
            if (base.OnLogDetected(logAt, level, logLines))
            {
                return true;
            }

            if (_worldKind == WorldKind.NoSpecificWorld)
            {
                return false;
            }

            var firstLine = logLines[0];

            return _worldKind switch
            {
                WorldKind.IdleHome => ParseAsIdleHomeSaveData(logAt, firstLine),
                WorldKind.IdleDefense => ParseAsIdleDefenseSaveData(logAt, logLines),
                WorldKind.TerrorsOfNowhere => ParseAsTonSaveDataPreamble(firstLine) || ParseAsTonSaveData(logAt, firstLine),
                WorldKind.Rhapsody => ParseAsRhapsodySaveDataPreamble(firstLine) || ParseAsRhapsodySaveData(logAt, firstLine),
                _ => false
            };
        }

        /// <summary>
        /// This method is called when join log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="instanceInfo">Instance information.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsJoinedLog(DateTime, string)"/></para>
        /// </remarks>
        protected override void OnJoinedToInstance(DateTime logAt, InstanceInfo instanceInfo)
        {
            _worldKind = instanceInfo.WorldId switch
            {
                WorldIds.IdleHome => WorldKind.IdleHome,
                WorldIds.IdleDefense => WorldKind.IdleDefense,
                WorldIds.TerrorsOfNowhere => WorldKind.TerrorsOfNowhere,
                WorldIds.RhapsodyEp1 => WorldKind.Rhapsody,
                _ => WorldKind.NoSpecificWorld
            };
        }


        /// <summary>
        /// This method is called when Idle Home save data log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="saveText">Save data text.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsIdleHomeSaveData(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnIdleHomeSaved(DateTime logAt, string saveText)
        {
        }

        /// <summary>
        /// This method is called when Idle Defense save data log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="saveText">Save data text.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsIdleDefenseSaveData(DateTime, List{string})"/></para>
        /// </remarks>
        protected virtual void OnIdleDefenseSaved(DateTime logAt, string saveText)
        {
        }

        /// <summary>
        /// This method is called when Terrors of nowhere save data log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="saveText">Save data text.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsTonSaveData(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnTerrorsOfNowhereSaved(DateTime logAt, string saveText)
        {
        }

        /// <summary>
        /// This method is called when Rhapsody save data log is detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="saveText">Save data text.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsTonSaveData(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnRhapsodySaved(DateTime logAt, string saveText)
        {
        }


        /// <summary>
        /// Parse first log line as Idle Home save data log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsIdleHomeSaveData(DateTime logAt, string firstLine)
        {
            var match = _regexIdleHomeSave.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            OnIdleHomeSaved(logAt, match.Groups[1].Value);

            return true;
        }

        /// <summary>
        /// Parse log lines as Idle Defense save data log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="logLines">Log lines.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsIdleDefenseSaveData(DateTime logAt, List<string> logLines)
        {
            if (logLines.Count != 2
                || logLines[0] != "Saving data complete! "
                || !logLines[1].EndsWith(" IDLEDEFENSE", StringComparison.Ordinal))
            {
                return false;
            }

            OnIdleDefenseSaved(logAt, logLines[1].Substring(0, logLines[1].Length - 12));

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere save data preamble log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonSaveDataPreamble(string firstLine)
        {
            if (firstLine != TonSaveDataPreamble)
            {
                return false;
            }

            _isTonSaveData = true;

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere save data log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonSaveData(DateTime logAt, string firstLine)
        {
            if (!_isTonSaveData)
            {
                return false;
            }

            _isTonSaveData = false;

            if (!firstLine.StartsWith("[START]", StringComparison.Ordinal)
                || !firstLine.EndsWith("[END]", StringComparison.Ordinal))
            {
                return false;
            }

            OnTerrorsOfNowhereSaved(logAt, firstLine.Substring(7, firstLine.Length - 12));

            return true;
        }

        /// <summary>
        /// Parse first log line as Rhapsody save data preamble log.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsRhapsodySaveDataPreamble(string firstLine)
        {
            if (firstLine != RhapsodySaveDataPreamble)
            {
                return false;
            }

            _isRhapsodySaveData = true;

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere save data log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsRhapsodySaveData(DateTime logAt, string firstLine)
        {
            if (!_isRhapsodySaveData)
            {
                return false;
            }

            _isRhapsodySaveData = false;

            OnRhapsodySaved(logAt, firstLine);

            return true;
        }
    }
}
