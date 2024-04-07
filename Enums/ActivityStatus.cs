﻿using System.ComponentModel;

namespace BuildingManager.Enums
{
    public enum ActivityStatus
    {
        [Description("For an activity that is still Pending")]
        Pending = 1,
        [Description("For an activity approved by the PM")]
        Approved = 2,
        [Description("For an activity rejected by the PM")]
        Rejected = 3,
        [Description("For an activity that is set to done by OtherPro")]
        Done = 4,
    }
}
