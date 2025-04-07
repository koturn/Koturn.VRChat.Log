using Koturn.VRChat.Log.Events;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Provides some events that occures when detect specific logs.
    /// </summary>
    public interface IVRCExLogEvent
    {
        /// <summary>
        /// Occurs when detect a log that save data text of Bullet Time Agent is generated.
        /// </summary>
        event VRCLogEventHandler<SaveEventArgs>? BulletTimeAgentSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Idle Cube is generated.
        /// </summary>
        event VRCLogEventHandler<SaveEventArgs>? IdleCubeSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Idle Home is generated.
        /// </summary>
        event VRCLogEventHandler<SaveEventArgs>? IdleHomeSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Idle Defense is generated.
        /// </summary>
        event VRCLogEventHandler<SaveEventArgs>? IdleDefenseSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Magical Cursed Land is generated.
        /// </summary>
        event VRCLogEventHandler<SaveEventArgs>? MagicalCursedLandSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Rhapsody is generated.
        /// </summary>
        event VRCLogEventHandler<SaveEventArgs>? RhapsodySaved;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere target of killer changed.
        /// </summary>
        event VRCLogEventHandler<TonKillerNameEventArgs>? TonKillerTargetChanged;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere player dead .
        /// </summary>
        event VRCLogEventHandler<TonPlayerDeadEventArgs>? TonPlayerDead;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere player damaged.
        /// </summary>
        event VRCLogEventHandler<TonPlayerDamagedEventArgs>? TonPlayerDamaged;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere killer stunned.
        /// </summary>
        event VRCLogEventHandler<TonKillerNameEventArgs>? TonKillerStunned;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere killer enrage level changed.
        /// </summary>
        event VRCLogEventHandler<TonKillerEnragedEventArgs>? TonKillerEnraged;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere killer set.
        /// </summary>
        event VRCLogEventHandler<TonKillerSetEventArgs>? TonKillerSet;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere killer unlocked.
        /// </summary>
        event VRCLogEventHandler<TonKillerUnlockedEventArgs>? TonKillerUnlocked;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere item equipped.
        /// </summary>
        event VRCLogEventHandler<TonEquipEventArgs>? TonEquipped;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere round started.
        /// </summary>
        event VRCLogEventHandler<TonRoundStartedEventArgs>? TonRoundStarted;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere player won or lost.
        /// </summary>
        event VRCLogEventHandler<TonRoundFinishedEventArgs>? TonRoundFinished;
        /// <summary>
        /// Occurs when detect a log that save data text of Terrors of Nowhere is generated.
        /// </summary>
        event VRCLogEventHandler<SaveEventArgs>? TerrorsOfNowhereSaved;
    }
}
