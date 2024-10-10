namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Authenticated user information.
    /// </summary>
    public class AuthUserInfo
    {
        /// <summary>
        /// User name.
        /// </summary>
        public string UserName { get; }
        /// <summary>
        /// User ID.
        /// </summary>
        public string UserId { get; }
        /// <summary>
        /// True if authed user has email, otherwise false.
        /// </summary>
        public bool HasEmail { get; }
        /// <summary>
        /// True if authed user has birthday, otherwise false.
        /// </summary>
        public bool HasBirthday { get; }
        /// <summary>
        /// tos value.
        /// </summary>
        public int Tos { get; }

        /// <summary>
        /// Initialze all members.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="userId">User ID.</param>
        /// <param name="hasEmail">True if authed user has email, otherwise false.</param>
        /// <param name="hasBirthday">True if authed user has birthday, otherwise false.</param>
        /// <param name="tos">tos value.</param>
        public AuthUserInfo(string userName, string userId, bool hasEmail, bool hasBirthday, int tos)
        {
            UserName = userName;
            UserId = userId;
            HasEmail = hasEmail;
            HasBirthday = hasBirthday;
            Tos = tos;
        }
    }
}
