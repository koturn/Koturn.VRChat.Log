using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.ObjectDropped"/> event.
    /// </summary>
    public class ObjectDroppedEventArgs : LogEventArgs
    {
        /// <summary>
        /// Pickuped object name.
        /// </summary>
        public string ObjectName { get; }
        /// <summary>
        /// True if the object was equipped.
        /// </summary>
        public bool IsEquipped { get; }
        /// <summary>
        /// Reason for dropping the object.
        /// </summary>
        public string DropReason { get; }
        /// <summary>
        /// Last input method name.
        /// </summary>
        public string LastInputMethod { get; }

        /// <summary>
        /// Create instance with specified information about dropped object and timestamps.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="objectName">Pickuped object name.</param>
        /// <param name="isEquipped">True if the object was equipped.</param>
        /// <param name="dropReason">Reason for dropping the object.</param>
        /// <param name="lastInputMethod">Last input method name.</param>
        public ObjectDroppedEventArgs(DateTime logAt, string objectName, bool isEquipped, string dropReason, string lastInputMethod)
            : base(logAt)
        {
            ObjectName = objectName;
            IsEquipped = isEquipped;
            DropReason = dropReason;
            LastInputMethod = lastInputMethod;
        }
    }
}
