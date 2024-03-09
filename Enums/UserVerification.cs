using System.ComponentModel;

namespace BuildingManager.Enums
{
    public enum EUserVerification
    {
        [Description("User Not Verified")]
        NotVerified = 1,
        [Description("User Verified")]
        Verified = 2
    }
}
