using System;
using Koturn.VRChat.Log.Enums;


namespace Koturn.VRChat.Log
{
    /// <summary>
    /// Represents instance join/leave log item.
    /// </summary>
    public class InstanceInfo
    {
        /// <summary>
        /// Timestamp your joined to the instance.
        /// </summary>
        public DateTime StayFrom { get; set; }
        /// <summary>
        /// Timestamp your left from the instance.
        /// </summary>
        public DateTime? StayUntil { get; set; }
        /// <summary>
        /// World name of the instance.
        /// </summary>
        public string? WorldName { get; set; }
        /// <summary>
        /// Instance string.
        /// </summary>
        public string? InstanceString { get; set; }
        /// <summary>
        /// World ID.
        /// </summary>
        public string? WorldId { get; set; }
        /// <summary>
        /// Instance ID.
        /// </summary>
        public string? InstanceId { get; set; }
        /// <summary>
        /// Instance type.
        /// </summary>
        public InstanceType InstanceType { get; set; }
        /// <summary>
        /// User ID or groud ID.
        /// </summary>
        public string? UserOrGroupId { get; set; }
        /// <summary>
        /// Instance server region.
        /// </summary>
        public Region Region { get; set; }
        /// <summary>
        /// Nonce value.
        /// </summary>
        public string? Nonce { get; set; }
        /// <summary>
        /// Log file timestamp.
        /// </summary>
        public DateTime? LogFrom { get; set; }
        /// <summary>
        /// This log is emitted or not.
        /// </summary>
        public bool IsEmitted { get; set; }

        /// <summary>
        /// Initialize <see cref="StayFrom"/> and <see cref="IsEmitted"/>.
        /// </summary>
        /// <param name="stayFrom">Timestamp your joined to the instance.</param>
        public InstanceInfo(DateTime stayFrom)
        {
            StayFrom = stayFrom;
            IsEmitted = false;
        }
    }
}