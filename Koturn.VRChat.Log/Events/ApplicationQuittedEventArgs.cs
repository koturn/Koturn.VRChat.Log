using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.ApplicationQuitted"/> event.
    /// </summary>
    public class ApplicationQuittedEventArgs : LogEventArgs
    {
        /// <summary>
        /// Active time (in seconds).
        /// </summary>
        public double ActiveTime { get; }

        /// <summary>
        /// Initialize all members.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="activeTime">Active time (in seconds).</param>
        public ApplicationQuittedEventArgs(DateTime logAt, double activeTime)
            : base(logAt)
        {
            ActiveTime = activeTime;
        }
    }
}
