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

    public enum ProjectOwner
    {
        [Description("Owner of Project")]
        Owner = 1,
        [Description("Not Owner of Project")]
        NotOwner = 2,
    }

    public enum ProjectUserAccess
    {
        [Description("For a User that is not blocked")]
        Allowed = 1,
        [Description("For a User that is blocked")]
        Blocked = 2,
    }
}
