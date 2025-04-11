namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Authenticated user information.
    /// </summary>
    /// <remarks>
    /// Primary ctor: Initialze all members.
    /// </remarks>
    /// <param name="userName">User name.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="hasEmail">True if authed user has email, otherwise false.</param>
    /// <param name="hasBirthday">True if authed user has birthday, otherwise false.</param>
    /// <param name="tos">tos value.</param>
    public class AuthUserInfo(string userName, string userId, bool hasEmail, bool hasBirthday, int tos)
    {
        /// <summary>
        /// User name.
        /// </summary>
        public string UserName { get; } = userName;
        /// <summary>
        /// User ID.
        /// </summary>
        public string UserId { get; } = userId;
        /// <summary>
        /// True if authed user has email, otherwise false.
        /// </summary>
        public bool HasEmail { get; } = hasEmail;
        /// <summary>
        /// True if authed user has birthday, otherwise false.
        /// </summary>
        public bool HasBirthday { get; } = hasBirthday;
        /// <summary>
        /// tos value.
        /// </summary>
        public int Tos { get; } = tos;
    }
}
