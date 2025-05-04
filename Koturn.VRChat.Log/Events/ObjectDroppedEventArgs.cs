using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.ObjectDropped"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with specified information about dropped object and timestamps.
    /// </remarks>
    /// <param name="logFilePath">Log file path.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="objectName">Pickuped object name.</param>
    /// <param name="isEquipped">True if the object was equipped.</param>
    /// <param name="dropReason">Reason for dropping the object.</param>
    /// <param name="lastInputMethod">Last input method name.</param>
    public class ObjectDroppedEventArgs(string? logFilePath, DateTime logAt, string objectName, bool isEquipped, string dropReason, string lastInputMethod)
        : VRCLogEventArgs(logFilePath, logAt)
    {
        /// <summary>
        /// Pickuped object name.
        /// </summary>
        public string ObjectName { get; } = objectName;
        /// <summary>
        /// True if the object was equipped.
        /// </summary>
        public bool IsEquipped { get; } = isEquipped;
        /// <summary>
        /// Reason for dropping the object.
        /// </summary>
        public string DropReason { get; } = dropReason;
        /// <summary>
        /// Last input method name.
        /// </summary>
        public string LastInputMethod { get; } = lastInputMethod;
    }
}
