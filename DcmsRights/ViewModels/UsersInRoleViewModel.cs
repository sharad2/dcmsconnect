using System.Collections.Generic;

namespace DcmsMobile.DcmsRights.ViewModels
{
    /// <summary>
    /// For a given role, what capabilies will be enabled for an area
    /// </summary>
    public class UsersInRoleArea
    {
        public string AreaName { get; set; }

        public string Capability { get; set; }
    }


    public class UsersInRoleViewModel : ViewModelBase
    {
        public string RoleName { get; set; }

        public IEnumerable<UsersInRoleArea> Areas { get; set; }

        public IEnumerable<OracleUserModel> Users { get; set; }

        public string[] SelectedUsers { get; set; }

    }
}