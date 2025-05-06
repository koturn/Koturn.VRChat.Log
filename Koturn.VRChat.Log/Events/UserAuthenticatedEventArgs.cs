using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.UserAuthenticated"/> event.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Create instance with log timestamp and authenticated user information.
    /// </remarks>
    /// <param name="logFileName">Log file name.</param>
    /// <param name="logAt">Log timestamp.</param>
    /// <param name="authUserInfo">Authenticated user information.</param>
    public class UserAuthenticatedEventArgs(string? logFileName, DateTime logAt, AuthUserInfo authUserInfo)
        : VRCLogEventArgs(logFileName, logAt)
    {
        /// <summary>
        /// Instance information.
        /// </summary>
        public AuthUserInfo AuthUserInfo { get; } = authUserInfo;
    }
}
