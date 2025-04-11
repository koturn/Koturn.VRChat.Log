using System;


namespace Koturn.VRChat.Log.Internals
{
    /// <summary>
    /// User information.
    /// </summary>
    /// <remarks>
    /// Initialize <see cref="UserName"/>, <see cref="UserId"/> and <see cref="JoinAt"/>.
    /// </remarks>
    /// <param name="userName">User name.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="joinAt">Join timestamp.</param>
    internal sealed class UserInfo(string userName, string? userId, DateTime joinAt)
    {
        /// <summary>
        /// User name.
        /// </summary>
        public string UserName { get; } = userName;
        /// <summary>
        /// User ID.
        /// </summary>
        public string? UserId { get; } = userId;
        /// <summary>
        /// Join timestamp.
        /// </summary>
        public DateTime JoinAt { get; } = joinAt;
    }
}
