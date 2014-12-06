using System;
using System.Configuration;

namespace DcmsMobile.DcmsRights.ViewModels
{
    public abstract class ViewModelBase
    {
        /// <summary>
        /// True if the logged in user has the rights to modify DCMS user properties
        /// </summary>
        /// <remarks>
        /// If the action does not set this value, then OnActionExecuting will set it.
        /// </remarks>
        public bool? IsEditable { get; set; }

        /// <summary>
        /// The role needed to edit uers
        /// </summary>
        public string EditableRoleName { get; set; }

        /// <summary>
        /// When user have DCMS8_USER Role.
        /// </summary>
        public static string DcmsUserRole
        {
            get
            {
                var defaultRole = ConfigurationManager.AppSettings["DcmsUserRole"];
                if (string.IsNullOrEmpty(defaultRole))
                {
                    throw new ApplicationException("web.config must contain an appSetting for DcmsUserRole. The value must be the role required by all DCMS users, e.g. DCMS8_USER");
                }
                return defaultRole;
            }
        }

        ///// <summary>
        ///// When user have SO_DCMS_SINGLE Profile.
        ///// </summary>
        //public static string DcmsUserProfile
        //{
        //    get
        //    {
        //        var defaultProfile = ConfigurationManager.AppSettings["DcmsUserProfile"];
        //        if (string.IsNullOrEmpty(defaultProfile))
        //        {
        //            throw new ApplicationException("web.config must contain an appSetting for DcmsUserProfile. The value must be the profile required by all DCMS users, e.g. SO_DCMS_SINGLE");
        //        }
        //        return defaultProfile;
        //    }   
        //}
    }
}