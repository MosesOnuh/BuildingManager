using System.ComponentModel;

namespace BuildingManager.Enums
{
    public enum UserRoles
    {
        [Description("Project Manager Role")]
        PM = 1,
        [Description("Role for other professionals")]
        OtherPro = 2,
        [Description("Role for other Client")]
        Client = 3
            
    }
}
