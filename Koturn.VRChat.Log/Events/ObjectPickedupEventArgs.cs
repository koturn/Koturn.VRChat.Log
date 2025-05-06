using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.ObjectPickedup"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with specified information about pickedup object and timestamps.
    /// </remarks>
    /// <param name="logFileName">Log file name.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="objectName">Pickedup object name.</param>
    /// <param name="isEquipped">True if equipped.</param>
    /// <param name="isEquippable">True if the object is equippable.</param>
    /// <param name="lastInputMethod">Last input method name.</param>
    /// <param name="isAutoEquipController">True if the object is auto equip controller.</param>
    public class ObjectPickedupEventArgs(string? logFileName, DateTime logAt, string objectName, bool isEquipped, bool isEquippable, string lastInputMethod, bool isAutoEquipController)
        : VRCLogEventArgs(logFileName, logAt)
    {
        /// <summary>
        /// Pickedup object name.
        /// </summary>
        public string ObjectName { get; } = objectName;
        /// <summary>
        /// True if equipped.
        /// </summary>
        public bool IsEquipped { get; } = isEquipped;
        /// <summary>
        /// True if the object is equippable.
        /// </summary>
        public bool IsEquippable { get; } = isEquippable;
        /// <summary>
        /// Last input method name.
        /// </summary>
        public string LastInputMethod { get; } = lastInputMethod;
        /// <summary>
        /// True if the object is auto equip controller.
        /// </summary>
        public bool IsAutoEquipController { get; } = isAutoEquipController;
    }
}
