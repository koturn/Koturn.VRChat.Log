using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.ApplicationQuitted"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Initialize all members.
    /// </remarks>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="activeTime">Active time (in seconds).</param>
    public class ApplicationQuittedEventArgs(DateTime logAt, double activeTime)
        : VRCLogEventArgs(logAt)
    {
        /// <summary>
        /// Active time (in seconds).
        /// </summary>
        public double ActiveTime { get; } = activeTime;
    }
}
