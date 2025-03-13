using System;
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
        event EventHandler<SaveEventArgs>? BulletTimeAgentSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Idle Cube is generated.
        /// </summary>
        event EventHandler<SaveEventArgs>? IdleCubeSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Idle Home is generated.
        /// </summary>
        event EventHandler<SaveEventArgs>? IdleHomeSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Idle Defense is generated.
        /// </summary>
        event EventHandler<SaveEventArgs>? IdleDefenseSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Magical Cursed Land is generated.
        /// </summary>
        event EventHandler<SaveEventArgs>? MagicalCursedLandSaved;
        /// <summary>
        /// Occurs when detect a log that save data text of Rhapsody is generated.
        /// </summary>
        event EventHandler<SaveEventArgs>? RhapsodySaved;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere target of killer changed.
        /// </summary>
        event EventHandler<TonKillerNameEventArgs>? TonKillerTargetChanged;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere player dead .
        /// </summary>
        event EventHandler<TonPlayerDeadEventArgs>? TonPlayerDead;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere player damaged.
        /// </summary>
        event EventHandler<TonPlayerDamagedEventArgs>? TonPlayerDamaged;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere killer stunned.
        /// </summary>
        event EventHandler<TonKillerNameEventArgs>? TonKillerStunned;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere killer enrage level changed.
        /// </summary>
        event EventHandler<TonKillerEnragedEventArgs>? TonKillerEnraged;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere killer set.
        /// </summary>
        event EventHandler<TonKillerSetEventArgs>? TonKillerSet;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere killer unlocked.
        /// </summary>
        event EventHandler<TonKillerUnlockedEventArgs>? TonKillerUnlocked;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere item equipped.
        /// </summary>
        event EventHandler<TonEquipEventArgs>? TonEquipped;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere round started.
        /// </summary>
        event EventHandler<TonRoundStartedEventArgs>? TonRoundStarted;
        /// <summary>
        /// Occurs when detect a log that Terrors of Nowhere player won or lost.
        /// </summary>
        event EventHandler<TonRoundFinishedEventArgs>? TonRoundFinished;
        /// <summary>
        /// Occurs when detect a log that save data text of Terrors of Nowhere is generated.
        /// </summary>
        event EventHandler<SaveEventArgs>? TerrorsOfNowhereSaved;
    }
}
