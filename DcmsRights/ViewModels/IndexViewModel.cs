using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.DcmsRights.ViewModels
{
    /// <summary>
    /// This represents the purpose of a role for a given area
    /// </summary>
    public class SecurityRoleModel
    {
        public string RoleName { get; set; }

        public string Purpose { get; set; }
    }

    /// <summary>
    /// Represents an area and the roles associated with the area. This represents each row of the grid displayed by the view.
    /// </summary>
    public class SecurityAreaModel
    {
        public string AreaName { get; set; }

        public string AreaDescription { get; set; }

        public IEnumerable<SecurityRoleModel> Roles { get; set; }
    }

    /// <summary>
    /// View model passed to Index.cshtml
    /// </summary>
    public class IndexViewModel : ViewModelBase
    {
        private readonly IEnumerable<SecurityAreaModel> _areas;
        public IndexViewModel()
        {
            _areas = from capability in RoleAreaCapability.Capabilities
                     group capability by capability.AreaName into g
                     orderby g.Key
                     select new SecurityAreaModel
                     {
                         AreaName = g.Key,
                         AreaDescription = g.First().AreaDescription,
                         Roles = from role in g
                                 select new SecurityRoleModel
                                 {
                                     RoleName = role.RoleName,
                                     Purpose = role.Purpose
                                 }
                     };
           this.RoleNames = new string[0];
        }

        public IEnumerable<SecurityAreaModel> Areas
        {
            get
            {
                return _areas;
            }
        }

        [Required]
        [Display(Name = "User Names")]
        public string UserNames { get; set; }

        [Required(ErrorMessage = "New users must be granted at least one role to enable them to use DCMS")]
        [Display(Name = "Roles")]
        public string[] RoleNames { get; set; }

        [Required]
        [Display(Name = "Initial Password")]
        public string Password { get; set; }
    }
}