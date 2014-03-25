using System.Collections.Generic;

namespace DcmsMobile.DcmsRights.ViewModels
{
    public class UserListViewModel
    {
        public IEnumerable<OracleUserModel> UsersList { get; set; }

        /// <summary>
        /// This property is used for autocomplete 
        /// to give all info of any perticular user.
        /// </summary>
        public OracleUserModel Users { get; set; }
    }
}