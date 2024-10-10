using System;


namespace Koturn.VRChat.Log.Events
{
    /// <summary>
    /// Provides data for <see cref="IVRCCoreLogEvent.UserAuthenticated"/> event.
    /// </summary>
    public class UserAuthenticatedEventArgs : LogEventArgs
    {
        /// <summary>
        /// Instance information.
        /// </summary>
        public AuthUserInfo AuthUserInfo { get; }

        /// <summary>
        /// Create instance with log timestamp and authenticated user information.
        /// </summary>
        /// <param name="logAt">Log timestamp.</param>
        /// <param name="authUserInfo">Authenticated user information.</param>
        public UserAuthenticatedEventArgs(DateTime logAt, AuthUserInfo authUserInfo)
            : base(logAt)
        {
            AuthUserInfo = authUserInfo;
        }
    }
}
