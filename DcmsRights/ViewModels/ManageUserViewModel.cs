using System.Collections.Generic;
using System.Linq;

namespace DcmsMobile.DcmsRights.ViewModels
{
    /// <summary>
    /// For a given role, the purpose within the area
    /// </summary>
    public class ManageUserRoleArea
    {
        public string AreaName { get; set; }

        public string Purpose { get; set; }
    }

    public class ManageUserRole
    {
        public string RoleName { get; set; }

        public IEnumerable<ManageUserRoleArea> Areas { get; set; }
    }

    public class ManageUserViewModel : ViewModelBase
    {
        public OracleUserModel User { get; set; }

        private IEnumerable<ManageUserRole> _assignedRoles;
        public IEnumerable<ManageUserRole> AssignedRoles
        {
            get
            {
                return _assignedRoles ?? Enumerable.Empty<ManageUserRole>();
            }
            set
            {
                _assignedRoles = value;
            }
        }

        private IEnumerable<ManageUserRole> _grantableRoles;
        public IEnumerable<ManageUserRole> GrantableRoles
        {
            get
            {
                return _grantableRoles ?? Enumerable.Empty<ManageUserRole>();
            }
            set
            {
                _grantableRoles = value;
            }
        }

        public string[] SelectedRoles { get; set; }
    }
}