using System.ComponentModel;

namespace BuildingManager.Enums
{
    public enum PaymentRequestStatus
    {
        [Description("For a payment request that is still Pending")]
        Pending = 1,
        [Description("For a payment request to be confirmed by the PM")]
        AwaitingConfirmation = 2,
        [Description("For a payment request approved by the PM")]
        Confirmed = 3,
        [Description("For a payment request rejected by the PM")]
        Rejected = 4,
    }

    public enum PaymentRequestType
    {
        [Description("For a single payment request")]
        Single = 1,
        [Description("For a group payment request")]
        Group = 2,
    }
}
