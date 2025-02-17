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
        /// Rhapsody save data preamble log line.
        /// </summary>
        public const string RhapsodySaveDataPreamble = "セーブが実行されました";
        /// <summary>
        /// Terros of Nowhere save data preamble log line.
        /// </summary>
        public const string TonSaveDataPreamble = "[TERRORS SAVE CODE CREATED. PLEASE MAKE SURE YOU COPY THE ENTIRE THING. DO NOT INCLUDE [START] or [END]]";

        /// <summary>
        /// Regex to extract Idle Home save data.
        /// </summary>
        private static readonly Regex _regexIdleHomeSave;
        /// <summary>
        /// Regex to extract Terros of Nowhere equipping item log.
        /// </summary>
        private static readonly Regex _regexTonEquip;
        /// <summary>
        /// Regex to extract Terros of Nowhere place log.
        /// </summary>
        private static readonly Regex _regexTonPlace;
        /// <summary>
        /// Regex to extract Terros of Nowhere winning information log.
        /// </summary>
        private static readonly Regex _regexTonWin;
        /// <summary>
        /// Regex to extract Terros of Nowhere killer set log.
        /// </summary>
        private static readonly Regex _regexTonKillerSetting;
        /// <summary>
        /// Regex to extract Terros of Nowhere killer enrage level changed log.
        /// </summary>
        private static readonly Regex _regexTonKillerEnrage;

        /// <summary>
        /// Initialize regexes.
        /// </summary>
        static VRCCoreExLogParser()
        {
            _regexIdleHomeSave = RegexHelper.GetIdleHomeSaveRegex();
            _regexTonEquip = RegexHelper.GetTonEquipRegex();
            _regexTonPlace = RegexHelper.GetTonPlaceRegex();
            _regexTonWin = RegexHelper.GetTonWinRegex();
            _regexTonKillerSetting = RegexHelper.GetTonKillerSettingRegex();
            _regexTonKillerEnrage = RegexHelper.GetTonKillerEnrageRegex();
        }


        /// <summary>
        /// Instance information.
        /// </summary>
        public TonRoundInfo? TonRoundInfo { get; private set; }
        /// <summary>
        /// <see cref="string"/> <see cref="HashSet{T}"/> of terror name.
        /// </summary>
        public HashSet<string> TerrorNameSet { get; }

        /// <summary>
        /// World kind.
        /// </summary>
        private WorldKind _worldKind;
        /// <summary>
        /// Indicate next log line is Rhapsody save data.
        /// </summary>
        private bool _isRhapsodySaveData;
        /// <summary>
        /// Terros of Nowhere Round start timestamp.
        /// </summary>
        private DateTime _tonRoundAt;
        /// <summary>
        /// Indicate next log line is Terros of Nowhere save data.
        /// </summary>
        private bool _isTonSaveData;
        /// <summary>
        /// Equipped item index.
        /// </summary>
        private int _tonEquippedItemIndex;
        /// <summary>
        /// Round place name.
        /// </summary>
        private string? _tonPlaceName;
        /// <summary>
        /// Round place index.
        /// </summary>
        private int _tonPlaceIndex;


        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="filePath">VRChat log file path.</param>
        /// <param name="bufferSize">Buffer size for <see cref="FileStream"/> and <see cref="StreamReader"/>.</param>
        public VRCCoreExLogParser(string filePath, int bufferSize = 65536)
            : base(filePath, bufferSize)
        {
            TonRoundInfo = null;
            TerrorNameSet = new HashSet<string>();
            _worldKind = WorldKind.NoSpecificWorld;
            _isRhapsodySaveData = false;
            _tonPlaceName = null;
            _tonPlaceIndex = -1;
            _isTonSaveData = false;
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
            TonRoundInfo = null;
            TerrorNameSet = new HashSet<string>();
            _worldKind = WorldKind.NoSpecificWorld;
            _isRhapsodySaveData = false;
            _tonPlaceName = null;
            _tonPlaceIndex = -1;
            _isTonSaveData = false;
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
                WorldKind.TerrorsOfNowhere => ParseAsTonLog(logAt, firstLine),
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
        /// <para><see cref="VRCCoreLogParser.ParseAsJoinedLog(DateTime, string)"/></para>
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
        /// This method is called when Terrors of Nowhere target of killer changed.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="terrorName">Terror name.</param>
        protected virtual void OnTonKillerTargetChanged(DateTime logAt, string terrorName)
        {
        }

        /// <summary>
        /// This method is called when Terrors of Nowhere any player dead.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="playerName">Player name.</param>
        /// <param name="message">message.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsTonPlayerDead(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnTonPlayerDead(DateTime logAt, string playerName, string message)
        {
        }

        /// <summary>
        /// This method is called when Terrors of Nowhere player damaged.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="damage">Damage point.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsTonPlayerDamaged(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnTonPlayerDamaged(DateTime logAt, int damage)
        {
        }

        /// <summary>
        /// This method is called when Terrors of Nowhere killer stunned.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="terrorName">Terror name.</param>
        protected virtual void OnTonKillerStunned(DateTime logAt, string terrorName)
        {
        }

        /// <summary>
        /// This method is called when Terrors of Nowhere killer's enrage level changed.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="terrorName">Terror name.</param>
        /// <param name="enragteLevel">Enrage level.</param>
        protected virtual void OnTonKillerEnraged(DateTime logAt, string terrorName, int enragteLevel)
        {
        }

        /// <summary>
        /// This method is called when Terrors of Nowhere killer set.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="terrorIndex1">First terror index.</param>
        /// <param name="terrorIndex2">Second terror index.</param>
        /// <param name="terrorIndex3">Third terror index.</param>
        /// <param name="roundName">Round name.</param>
        protected virtual void OnTonKillerSet(DateTime logAt, int terrorIndex1, int terrorIndex2, int terrorIndex3, string roundName)
        {
        }

        /// <summary>
        /// This method is called when Terrors of Nowhere killer unlocked.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="indexType">Terror index type.</param>
        /// <param name="terrorIndex">Terror (Killer) index.</param>
        protected virtual void OnTonKillerUnlocked(DateTime logAt, TonTerrorIndexType indexType, int terrorIndex)
        {
        }

        /// <summary>
        /// This method is called when Terrors of Nowhere item equipping log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="itemIndex">Equipped item index.</param>
        /// <param name="lastItemIndex">Last equipped item index.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsTonEquipped(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnTonEquipped(DateTime logAt, int itemIndex, int lastItemIndex)
        {
        }

        /// <summary>
        /// This method is called when Terrors of Nowhere round started.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="placeName">Place name.</param>
        /// <param name="placeIndex">Place index.</param>
        /// <param name="roundName">Round name.</param>
        /// <remarks>
        /// <para>Called from following method.</para>
        /// <para><see cref="ParseAsTonPlace(DateTime, string)"/></para>
        /// </remarks>
        protected virtual void OnTonRoundStart(DateTime logAt, string placeName, int placeIndex, string roundName)
        {
        }

        /// <summary>
        /// This method is called when Terrors of Nowhere round finished.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="result">Round result.</param>
        protected virtual void OnTonRoundFinished(DateTime logAt, TonRoundResult result)
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

        /// <summary>
        /// Parse first log line as Terrors of Nowhere save log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonLog(DateTime logAt, string firstLine)
        {
            return ParseAsTonTargetChanged(logAt, firstLine)
                || ParseAsTonPlayerDead(logAt, firstLine)
                || ParseAsTonPlayerDamaged(logAt, firstLine)
                || ParseAsTonKillerStunned(logAt, firstLine)
                || ParseAsTonTriggerEnrage(logAt, firstLine)
                || ParseAsTonKillerSet(logAt, firstLine)
                || ParseAsTonUnlockingEntry(logAt, firstLine)
                || ParseAsTonEquipped(logAt, firstLine)
                || ParseAsTonRoundStart(firstLine)
                || ParseAsTonPlace(logAt, firstLine)
                || ParseAsTonPlayerLost(logAt, firstLine)
                || ParseAsTonPlayerWon(logAt, firstLine)
                || ParseAsTonRespawn(firstLine)
                || ParseAsTonSaveDataPreamble(firstLine)
                || ParseAsTonSaveData(logAt, firstLine);
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere target of killer changed.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonTargetChanged(DateTime logAt, string firstLine)
        {
            // 2024.09.28 23:18:42 Log        -  Detection : New Target : BFF
            // 2024.09.28 23:18:42 Log        -  Detection : Initial Target : BFF

            // if (!firstLine.StartsWith("Detection : ", StringComparison.Ordinal))
            // {
            //     return false;
            // }
            //
            // const int offset = 12;
            // if (SubstringEquals(firstLine, offset, "New Target : "))
            // {
            //     Console.WriteLine($@"  [{logAt:yyyy-MM-dd HH\:mm\:ss}] New Target [{firstLine.Substring(offset + 13)}]");
            //     return true;
            // }
            // else if (SubstringEquals(firstLine, offset, "Initial Target : "))
            // {
            //     Console.WriteLine($@"  [{logAt:yyyy-MM-dd HH\:mm\:ss}] Initial Target [{firstLine.Substring(offset + 17)}]");
            //     return true;
            // }

            if (!firstLine.StartsWith("Detection : New Target : ", StringComparison.Ordinal))
            {
                return false;
            }

            OnTonKillerTargetChanged(logAt, firstLine.Substring(25));

            return false;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere player lost.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonPlayerDead(DateTime logAt, string firstLine)
        {
            // 2024.10.05 23:30:45 Log - [DEATH][XXXXXXXX] XXXXXXXX zzzzzzzz...
            if (!firstLine.StartsWith("[DEATH][", StringComparison.Ordinal))
            {
                return false;
            }

            const int offset = 8;
            var index = firstLine.IndexOf(']', offset);
            if (index == -1)
            {
                return false;
            }

            var playerName = firstLine.Substring(offset, index - offset);
            var message = firstLine.Substring(index + 2);

            OnTonPlayerDead(logAt, playerName, message);

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere player damaged.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonPlayerDamaged(DateTime logAt, string firstLine)
        {
            const int offset = 6;

            if (!firstLine.StartsWith("Hit - ", StringComparison.Ordinal) || firstLine.Length <= offset)
            {
                return false;
            }

            int damage;
            unsafe
            {
                fixed (char* pcFirstLine = firstLine)
                {
                    damage = ParseIntSimple(pcFirstLine + offset, firstLine.Length - offset);
                }
            }

            OnTonPlayerDamaged(logAt, damage);

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere killer stunned.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonKillerStunned(DateTime logAt, string firstLine)
        {
            // 2024.09.28 23:18:44 Log - BFF was stunned.
            if (!firstLine.EndsWith(" was stunned.", StringComparison.Ordinal))
            {
                return false;
            }

            var killerName = firstLine.Substring(0, firstLine.Length - 13);
            if (killerName.Length != 0)
            {
                TerrorNameSet.Add(killerName);
            }

            OnTonKillerStunned(logAt, killerName);

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere killer's enrage level changing.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonTriggerEnrage(DateTime logAt, string firstLine)
        {
            // 2024.09.28 23:18:27 Log        -  BFFtriggered an Enrage State!
            var match = _regexTonKillerEnrage.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;

            var terrorName = groups[1].Value;

            var enrageLevel = 1;
            if (groups[2].Success)
            {
                enrageLevel = int.Parse(groups[2].Value);
            }

            if (terrorName.Length != 0)
            {
                TerrorNameSet.Add(terrorName);
            }

            OnTonKillerEnraged(logAt, terrorName, enrageLevel);

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere killer set.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonKillerSet(DateTime logAt, string firstLine)
        {
            var match = _regexTonKillerSetting.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;
            var terrorIndex1 = int.Parse(groups[1].Value);
            var terrorIndex2 = int.Parse(groups[2].Value);
            var terrorIndex3 = int.Parse(groups[3].Value);
            var roundName = groups[4].Value;

            TonRoundInfo tonRoundInfo;
            if (terrorIndex3 > 0)
            {
                tonRoundInfo = new TonRoundInfo(_tonRoundAt, roundName, terrorIndex1, terrorIndex2, terrorIndex3);
            }
            else if (terrorIndex2 > 0)
            {
                tonRoundInfo = new TonRoundInfo(_tonRoundAt, roundName, terrorIndex1, terrorIndex2);
            }
            else
            {
                tonRoundInfo = new TonRoundInfo(_tonRoundAt, roundName, terrorIndex1);
            }
            tonRoundInfo.PlaceName = _tonPlaceName;
            if (_tonPlaceIndex > 0)
            {
                tonRoundInfo.PlaceIndex = _tonPlaceIndex;
            }
            TonRoundInfo = tonRoundInfo;

            if (_tonEquippedItemIndex > 0)
            {
                tonRoundInfo.ItemIndex = _tonEquippedItemIndex;
            }

            OnTonKillerSet(logAt, terrorIndex1, terrorIndex2, terrorIndex3, roundName);

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere killer unlocked.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonUnlockingEntry(DateTime logAt, string firstLine)
        {
            if (!firstLine.StartsWith("Unlocking ", StringComparison.Ordinal))
            {
                return false;
            }

            const int offset = 10;
            if (IsSubstringAt("Entry ", firstLine, offset))
            {
#if NET7_0_OR_GREATER
                OnTonKillerUnlocked(logAt, TonTerrorIndexType.Normal, int.Parse(firstLine.AsSpan(offset + 6)));
#else
                OnTonKillerUnlocked(logAt, TonTerrorIndexType.Normal, int.Parse(firstLine.Substring(offset + 6)));
#endif
                return true;
            }
            if (IsSubstringAt("Alt Entry ", firstLine, offset))
            {
#if NET7_0_OR_GREATER
                OnTonKillerUnlocked(logAt, TonTerrorIndexType.Alternate, int.Parse(firstLine.AsSpan(offset + 10)));
#else
                OnTonKillerUnlocked(logAt, TonTerrorIndexType.Alternate, int.Parse(firstLine.Substring(offset + 10)));
#endif
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere item equipping log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonEquipped(DateTime logAt, string firstLine)
        {
            var match = _regexTonEquip.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;
            var itemIndex = int.Parse(groups[1].Value);
            var lastItemIndex = int.Parse(groups[2].Value);

            _tonEquippedItemIndex = itemIndex;

            OnTonEquipped(logAt, itemIndex, lastItemIndex);

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere round start.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonRoundStart(string firstLine)
        {
            // 2024.09.29 17:11:01 Log        -  Everything recieved!
            // 2024.10.06 00:37:15 Log        -  Everything recieved, looks good to meee~!
            if (!firstLine.StartsWith("Everything recieved", StringComparison.Ordinal))
            {
                return false;
            }

            TonRoundInfo = null;
            _tonRoundAt = default;
            _tonPlaceName = null;
            _tonPlaceIndex = -1;

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere place detected.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonPlace(DateTime logAt, string firstLine)
        {
            var match = _regexTonPlace.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            var tonPlaceName = match.Groups[1].Value;
            var tonPlaceIndex = int.Parse(match.Groups[2].Value);
            _tonPlaceName = tonPlaceName;
            _tonPlaceIndex = tonPlaceIndex;

            OnTonRoundStart(logAt, tonPlaceName, tonPlaceIndex, match.Groups[3].Value);

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere player lost.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonPlayerLost(DateTime logAt, string firstLine)
        {
            // 2024.09.22 17:10:05 Log        -  Died in round.
            // 2024.09.22 17:10:06 Log        -  You Died iN the Round
            // 2024.09.22 17:10:06 Log        -  Player lost, not killer
            if (firstLine != "Died in round.")
            {
                return false;
            }

            if (TonRoundInfo != null)
            {
                TonRoundInfo.Result = TonRoundResult.Lose;
            }

            OnTonRoundFinished(logAt, TonRoundResult.Lose);
            TonRoundInfo = null;
            TerrorNameSet.Clear();

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere player winning log.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        /// <exception cref="NullReferenceException">Thrown when <see cref="TonRoundInfo"/> is null.</exception>
        /// <exception cref="Exception">Thrown when terror index mismatched.</exception>
        private bool ParseAsTonPlayerWon(DateTime logAt, string firstLine)
        {
            // 2024.10.05 22:04:10 Log - WE WON
            if (!firstLine.StartsWith("WE WON", StringComparison.Ordinal))
            {
                return false;
            }

            if (firstLine.Length == 6)
            {
                if (TonRoundInfo != null)
                {
                    TonRoundInfo.Result = TonRoundResult.Win;
                }

                OnTonRoundFinished(logAt, TonRoundResult.Win);

                return true;
            }

            //
            // for legacy win log.
            //
            var match = _regexTonWin.Match(firstLine);
            if (!match.Success)
            {
                return false;
            }

            var groups = match.Groups;
            var roundIndex = int.Parse(groups[1].Value);
            var terrorIndex1 = int.Parse(groups[2].Value);
            var terrorIndex2 = int.Parse(groups[3].Value);
            var terrorIndex3 = int.Parse(groups[4].Value);
            var itemIndex = int.Parse(groups[5].Value);

            var roundInfo = TonRoundInfo;
            if (roundInfo == null)
            {
                throw new NullReferenceException("Round info is null");
            }

            if (roundInfo.RoundIndex != roundIndex)
            {
                // throw new Exception($"Round index different; expected=[{roundIndex}] actual=[{roundInfo.RoundIndex}]");
                // Fix round index.
                roundInfo.RoundIndex = roundIndex;
            }
            if (roundInfo.TerrorIndex1 != terrorIndex1)
            {
                throw new Exception("Terror index1 different");
            }
            if (roundInfo.TerrorIndex2.GetValueOrDefault(0) != terrorIndex2)
            {
                throw new Exception("Terror index2 different");
            }
            if (roundInfo.TerrorIndex3.GetValueOrDefault(0) != terrorIndex3)
            {
                throw new Exception("Terror index3 different");
            }

            roundInfo.ItemIndex = itemIndex;
            roundInfo.Result = TonRoundResult.Win;

            return true;
        }

        /// <summary>
        /// Parse first log line as Terrors of Nowhere player respawn.
        /// </summary>
        /// <param name="firstLine">First log line.</param>
        /// <returns>True if parsed successfully, false otherwise.</returns>
        private bool ParseAsTonRespawn(string firstLine)
        {
            // 2024.09.22 18:06:16 Log        -  Respawned?
            // 2024.09.22 18:06:16 Log        -  Player respawned, opted out!
            if (firstLine != "Player respawned, opted out!")
            {
                return false;
            }

            if (TonRoundInfo != null)
            {
                TonRoundInfo.Result = TonRoundResult.Default;
            }
            TerrorNameSet.Clear();
            _tonEquippedItemIndex = 0;

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
    }
}
