using System;


namespace Koturn.VRChat.Log.Internals
{
    /// <summary>
    /// User information.
    /// </summary>
    internal sealed class UserInfo
    {
        /// <summary>
        /// User name.
        /// </summary>
        public string UserName { get; }
        /// <summary>
        /// User ID.
        /// </summary>
        public string? UserId { get; }
        /// <summary>
        /// Join timestamp.
        /// </summary>
        public DateTime JoinAt { get; }

        /// <summary>
        /// Initialize <see cref="UserName"/>, <see cref="UserId"/> and <see cref="JoinAt"/>.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userId"></param>
        /// <param name="joinAt"></param>
        public UserInfo(string userName, string? userId, DateTime joinAt)
        {
            UserName = userName;
            UserId = userId;
            JoinAt = joinAt;
        }
    }
}
