using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.ObjectPickedup"/> event.
    /// </summary>
    public class ObjectPickedupEventArgs : LogEventArgs
    {
        /// <summary>
        /// Pickedup object name.
        /// </summary>
        public string ObjectName { get; }
        /// <summary>
        /// True if equipped.
        /// </summary>
        public bool IsEquipped { get; }
        /// <summary>
        /// True if the object is equippable.
        /// </summary>
        public bool IsEquippable { get; }
        /// <summary>
        /// Last input method name.
        /// </summary>
        public string LastInputMethod { get; }
        /// <summary>
        /// True if the object is auto equip controller.
        /// </summary>
        public bool IsAutoEquipController { get; }

        /// <summary>
        /// Create instance with specified information about pickedup object and timestamps.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="objectName">Pickedup object name.</param>
        /// <param name="isEquipped">True if equipped.</param>
        /// <param name="isEquippable">True if the object is equippable.</param>
        /// <param name="lastInputMethod">Last input method name.</param>
        /// <param name="isAutoEquipController">True if the object is auto equip controller.</param>
        public ObjectPickedupEventArgs(DateTime logAt, string objectName, bool isEquipped, bool isEquippable, string lastInputMethod, bool isAutoEquipController)
            : base(logAt)
        {
            ObjectName = objectName;
            IsEquipped = isEquipped;
            IsEquippable = isEquippable;
            LastInputMethod = lastInputMethod;
            IsAutoEquipController = isAutoEquipController;
        }
    }
}
