using System.ComponentModel;

namespace BuildingManager.Enums
{
    public enum ProjectPhase
    {
        [Description("Project Pre-Construction Phase")]
        PreConstruction = 1,
        [Description("Project Construction Phase")]
        Construction = 2,
        [Description("Project Post-Construction Phase")]
        PostConstruction = 3,
    }
}
