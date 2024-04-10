using System.ComponentModel;

namespace BuildingManager.Enums
{
    public enum InviteNotificationStatus
    {
        [Description("For an invite notification that is still Pending")]
        Pending = 1,
        [Description("For an invite notification Accepted by the user")]
        Accepted = 2,
        [Description("For an invite notification Rejected by the user")]
        Rejected = 3,
    }
}
