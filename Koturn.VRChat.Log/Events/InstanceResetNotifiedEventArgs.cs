using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.InstanceResetNotified"/>.
    /// </summary>
    public class InstanceResetNotifiedEventArgs : LogEventArgs
    {
        /// <summary>
        /// Time until instance is closed (minutes).
        /// </summary>
        public int CloseMinutes { get; }

        /// <summary>
        /// Create instance with specified log timestamp and time until instance is closed.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="closeMinutes">Time until instance is closed (minutes).</param>
        public InstanceResetNotifiedEventArgs(DateTime logAt, int closeMinutes)
            : base(logAt)
        {
            CloseMinutes = closeMinutes;
        }
    }
}
