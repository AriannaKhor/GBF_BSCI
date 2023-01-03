using System.ComponentModel;

namespace GreatechApp.Core.Enums
{
    public sealed class ACL
    {
        public enum UserLevel
        {
            [Description("None")]
            None = -1,
            [Description("Admin")]
            Admin,
            [Description("Engineer")]
            Engineer,
            [Description("Technician")]
            Technician,
            [Description("Operator")]
            Operator,
        }

        // Note: this listing must match database table - tblAccessControl
        public const string Setting = "Setting";
        public const string IO = "IO";
        public const string Communication = "Communication";
        public const string Motion = "Motion";
        public const string User_Management = "User_Management";
    }
}
