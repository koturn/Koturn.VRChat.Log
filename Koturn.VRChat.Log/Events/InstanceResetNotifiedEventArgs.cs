using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.InstanceResetNotified"/>.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with specified log timestamp and time until instance is closed.
    /// </remarks>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="closeMinutes">Time until instance is closed (minutes).</param>
    public class InstanceResetNotifiedEventArgs(DateTime logAt, int closeMinutes)
        : LogEventArgs(logAt)
    {
        /// <summary>
        /// Time until instance is closed (minutes).
        /// </summary>
        public int CloseMinutes { get; } = closeMinutes;
    }
}
