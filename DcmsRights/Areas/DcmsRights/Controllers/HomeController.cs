using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Common;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using DcmsMobile.DcmsRights.Helpers;
using DcmsMobile.DcmsRights.ViewModels;
using EclipseLibrary.Mvc.Controllers;
using EclipseLibrary.Oracle;
using EclipseLibrary.Oracle.Web.Security;

namespace DcmsMobile.DcmsRights.Areas.DcmsRights.Controllers
{

    public partial class HomeController : EclipseController
    {
        static HomeController()
        {
            //For all info of any user.
            //Mapper.CreateMap<OracleMembershipUser, OracleUserModel>()
            //       .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            //       .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreationDate))
            //       .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsLockedOut))
            //       .ForMember(dest => dest.LastLockoutDate, opt => opt.MapFrom(src => src.LastLockoutDate == DateTime.MinValue ? (DateTime?)null : src.LastLockoutDate))
            //       .ForMember(dest => dest.PasswordExpiryDate, opt => opt.MapFrom(src => src.PasswordExpiryDate == DateTime.MinValue ? (DateTime?)null : src.PasswordExpiryDate))
            //       ;

            //For all audit info of any perticular user.
            //Mapper.CreateMap<OracleMembershipUserAudit, UserAudit>()
            //       .ForMember(dest => dest.ActionName, opt => opt.MapFrom(src => src.ActionName))
            //       .ForMember(dest => dest.ActionTime, opt => opt.MapFrom(src => src.ActionTime))
            //       .ForMember(dest => dest.ByOracleUserName, opt => opt.MapFrom(src => src.ByOracleUserName))
            //       .ForMember(dest => dest.ByOsUserName, opt => opt.MapFrom(src => src.ByOsUserName))
            //       .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
            //       .ForMember(dest => dest.TerminalName, opt => opt.MapFrom(src => src.TerminalName))
            //       .ForMember(dest => dest.Result, opt => opt.MapFrom(src => src.Result))
            //       ;

            //For all info of any logged in user.
            //Mapper.CreateMap<OracleMembershipUserSession, UserSession>()
            //       .ForMember(dest => dest.ActionName, opt => opt.MapFrom(src => src.ActionName))
            //       .ForMember(dest => dest.ClientInfo, opt => opt.MapFrom(src => src.ClientInfo))
            //       .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            //       .ForMember(dest => dest.LogonTime, opt => opt.MapFrom(src => src.LogonTime))
            //       .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.MachineName))
            //       .ForMember(dest => dest.Module, opt => opt.MapFrom(src => src.Module))
            //       .ForMember(dest => dest.OsExecutableName, opt => opt.MapFrom(src => src.OsExecutableName))
            //       .ForMember(dest => dest.OSUserName, opt => opt.MapFrom(src => src.OsUserName))
            //       .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => src.SerialNumber))
            //       .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId))
            //       .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            //       ;
        }

        /// <summary>
        /// Required for T4MVC.
        /// </summary>
        public HomeController()
        {
        }

        private const string ROLE_SECURITY_ADMIN = "DCMS_MANAGER";

        /// <summary>
        /// Set Editable properties on the base view model
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var vr = filterContext.Result as ViewResult;
            if (vr != null)
            {
                var model = vr.Model as ViewModelBase;
                if (model != null)
                {
                    if (model.IsEditable == null || model.IsEditable.Value)
                    {
                        // Action has not specified any value, or it has permitted editing.
                        // In this case we ensure that the logged in user has proper privileges.
                        // Super users and people having the correct role are always alloed to edit.
                        model.IsEditable = AuthorizeExAttribute.IsSuperUser(this.HttpContext) ||
                                this.HttpContext.User.IsInRole(ROLE_SECURITY_ADMIN);

                    }
                    else
                    {
                        // The action does not want this page to be editable. Honor its desire.
                    }
                    model.EditableRoleName = ROLE_SECURITY_ADMIN;
                }
            }
            base.OnActionExecuted(filterContext);
        }

        /// <summary>
        /// Displays a list of areas and the roles required to access them
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public virtual ActionResult Index()
        {
            return View(Views.Index, new IndexViewModel());
        }

        /// <summary>
        /// This method is used to create new user with roles.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// Creates the user, grants roles to the user, grants connect through to the user.
        /// </remarks>
        [HttpPost]
        [AuthorizeEx("Creating users requires role {0}", Roles = ROLE_SECURITY_ADMIN)]
        public virtual ActionResult CreateUsers(IndexViewModel model)
        {
            //TC1 : Do not enter anything in either UserName or Intial Password textbox on Index Page.
            if (!ModelState.IsValid)
            {
                return View(Views.Index, model);
            }
            //TC2 : Do not select any Role while creating new USER
            if (model.RoleNames.Count() <= 0)
            {
                ModelState.AddModelError("", "Select at least one role to grant this user");
                return View(Views.Index, model);
            }
            var users = model.UserNames.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                var rolesToAdd = model.RoleNames.Concat(new[] { ViewModelBase.DcmsUserRole }).ToArray();

                foreach (var user in users)
                {
                    Membership.CreateUser(user, model.Password);
                    Roles.AddUserToRoles(user, rolesToAdd);
                    AddStatusMessage(string.Format("User <a href=\"{0}\">{1}</a> created successfully", Url.Action(this.Actions.ManageUser(user)), user));
                }
                return RedirectToAction(Actions.Index());
            }
            catch (ProviderException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ModelState.AddModelError("", "Not all users have been created");
                return View(Views.Index, model);
            }
            //TC3: Provide initial Password of less than 9 digits or enter alphabates as intial password or try to create exisiting user again.
            catch (MembershipCreateUserException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ModelState.AddModelError("", "Not all users have been created");
                return View(Views.Index, model);
            }
        }

        /// <summary>
        /// This method is used to show list of all users. 
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult UserList()
        {
            var model = new UserListViewModel();
            //var dcmsUsers = Roles.GetUsersInRole(ViewModelBase.DcmsUserRole);
            var allUsers = Membership.GetAllUsers();
            //model.UsersList = Mapper.Map<IEnumerable<OracleUserModel>>(allUsers);
            model.UsersList = allUsers.Cast<OracleMembershipUser>().Select(p => new OracleUserModel(p));
            
            return View(Views.UserList, model);
        }

        /// <summary>
        /// This method is used for showing only those users whose account is locked.
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult LockedUserList()
        {
            var model = new UserListViewModel();
            var allUsers = Membership.GetAllUsers().Cast<OracleMembershipUser>().Where(p => p.LastLockoutDate != DateTime.MinValue);
            //model.UsersList = Mapper.Map<IEnumerable<OracleUserModel>>(allUsers);
            model.UsersList = allUsers.Cast<OracleMembershipUser>().Select(p => new OracleUserModel(p));
            return View(Views.UserList, model);
        }

        /// <summary>
        /// This method is used to grant roles from any specific user on ManageUser page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Granting roles to users requires role {0}", Roles = ROLE_SECURITY_ADMIN)]
        public virtual ActionResult GrantRolesToUser(ManageUserViewModel model)
        {
            //TC4 : Donot select any role to grant to user and click Grant button.
            if (model.SelectedRoles == null || model.SelectedRoles.Length == 0)
            {
                ModelState.AddModelError("", string.Format("Select some roles to grant to user {0}", model.User.UserName));
            }
            //TC5 : Try to give role to logged in user.
            if (model.User.UserName.ToUpper() == this.HttpContext.User.Identity.Name.ToUpper())
            {
                ModelState.AddModelError("", string.Format("You cannot grant role to yourself. you are logged in as {0}.", model.User.UserName));
            }
            if (ModelState.IsValid)
            {
                try
                {
                    Roles.AddUserToRoles(model.User.UserName, model.SelectedRoles);
                }
                catch (ProviderException ex)
                {
                    // Probably the role was unreasonable
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return RedirectToAction(Actions.ManageUser(model.User.UserName));
        }

        /// <summary>
        ///  This method is used to revoke roles from any specific user on ManageUser page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Revoking roles from users requires role {0}", Roles = ROLE_SECURITY_ADMIN)]
        public virtual ActionResult RevokeRolesFromUser(ManageUserViewModel model)
        {
            //TC6 : Do not select any role to revoke from user and click Revoke button
            if (model.SelectedRoles == null || model.SelectedRoles.Length == 0)
            {
                ModelState.AddModelError("Role Names", string.Format("Select some roles to revoke from user {0}", model.User.UserName));
            }
            else
            {
                try
                {
                    Roles.RemoveUserFromRoles(model.User.UserName, model.SelectedRoles);
                }
                catch (ProviderException ex)
                {
                    // Probably the role was unreasonable
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return RedirectToAction(this.Actions.ManageUser(model.User.UserName));
        }

        /// <summary>
        /// This method gives the list of User having grant on passed role. 
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public virtual ActionResult UsersInRole(string roleName)
        {
            var query2 = from capability in RoleAreaCapability.Capabilities
                         where capability.RoleName == roleName
                         orderby capability.AreaName
                         select new UsersInRoleArea
                         {
                             AreaName = capability.AreaName,
                             Capability = capability.Purpose
                         };

            var model = new UsersInRoleViewModel
            {
                RoleName = roleName,
                Areas = query2,
                Users = from name in Roles.GetUsersInRole(roleName)
                        orderby name
                        //let user = Membership.Provider.GetUser(name, false) as OracleMembershipUser
                        select new OracleUserModel
                        {
                            UserName = name
                        }
            };
            return View(Views.UsersInRole, model);
        }

        /// <summary>
        /// This method is used to remove users from specific role on UserInRole page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Revoking user roles requires role {0}", Roles = ROLE_SECURITY_ADMIN)]
        public virtual ActionResult RemoveUsersFromRole(UsersInRoleViewModel model)
        {
            //TC7 : Do not select any user from list and click remove button on Users in role view
            if (model.SelectedUsers == null || model.SelectedUsers.Length == 0)
            {
                ModelState.AddModelError("", string.Format("Select some users to revoke from role {0}", model.RoleName));
            }
            else
            {
                var roles = new[] { model.RoleName };
                try
                {
                    Roles.Provider.RemoveUsersFromRoles(model.SelectedUsers, roles);
                    AddStatusMessage(string.Format("{0} users removed successfully",model.SelectedUsers.Count()));
                }
                catch (ProviderException ex)
                {
                    // Probably the role was unreasonable
                    ModelState.AddModelError("", ex);
                }
            }
            return RedirectToAction(Actions.UsersInRole(model.RoleName));
        }

        #region Manage Users

        /// <summary>
        /// This displays information about a specific user. If the user does not exist, it redirects to the user list page.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public virtual ActionResult ManageUser(string userName)
        {
            //TC8 : Do not enter user name on textbox and click GO button
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction(Actions.UserList());
            }
            var model = new ManageUserViewModel();
            var userRoles = Roles.GetRolesForUser(userName);
            var query = from capability in RoleAreaCapability.Capabilities
                        group capability by capability.RoleName into g
                        orderby g.Key
                        select new ManageUserRole
                        {
                            RoleName = g.Key,
                            Areas = from area in g
                                    orderby area.AreaName
                                    select new ManageUserRoleArea
                                    {
                                        AreaName = area.AreaName,
                                        Purpose = area.Purpose
                                    }
                        };
            OracleMembershipUser user;
            try
            {
                model.AssignedRoles = query.Where(p => userRoles.Contains(p.RoleName));

                model.GrantableRoles = query.Where(p => !userRoles.Contains(p.RoleName));

                user = Membership.Provider.GetUser(userName, false) as OracleMembershipUser;
            }
            catch (ProviderException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(Actions.UserList());
            }
            //TC 9 :Enter any invalid user and click GO button
            if (user == null)
            {
                ModelState.AddModelError("", string.Format("User {0} does not exist.", userName));
                return RedirectToAction(Actions.UserList());
            }

            model.User = new OracleUserModel
            {
                UserName = userName,
                PasswordExpiryDate = user.PasswordExpiryDate == DateTime.MinValue ? (DateTime?)null : user.PasswordExpiryDate,
                LastLockoutDate = user.LastLockoutDate == DateTime.MinValue ? (DateTime?)null : user.LastLockoutDate,
                CreateDate = user.CreationDate,
                //UserAuditLog = Mapper.Map<IEnumerable<UserAudit>>(user.AuditLog),                                
                UserAuditLog = user.AuditLog.Select(p => new UserAudit
                {
                    ActionName = p.ActionName,
                    ActionTime = p.ActionTime,
                    ByOracleUserName = p.ByOracleUserName,
                    ByOsUserName = p.ByOsUserName,
                    Result = p.Result,
                    RoleName = p.RoleName,
                    TerminalName = p.TerminalName
                }),
                //UserSessionLog = Mapper.Map<IEnumerable<UserSession>>(user.Sessions)
                UserSessionLog = user.Sessions.Select(p => new UserSession
                {
                    ActionName = p.ActionName,
                    ClientInfo = p.ClientInfo,
                    IsActive = p.IsActive,
                    LogonTime = p.LogonTime,
                    MachineName = p.MachineName,
                    Module = p.Module,
                    OsExecutableName = p.OsExecutableName,
                    OSUserName = p.OsUserName,
                    SerialNumber = p.SerialNumber,
                    SessionId = p.SessionId,
                    State = p.State
                })
            };

            // The user we are trying to edit must be a DCMS user. We do not want to risk editing system users such as sys and dcms8.
            // We ensure this by checking whether the user to edit has the DCMS8_USER role.
            //model.IsEditable = userRoles.Contains(ViewModelBase.DcmsUserRole);

            return View(Views.ManageUser, model);
        }

        /// <summary>
        /// Unlocks the passed user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Unlocking users requires role {0}", Roles = ROLE_SECURITY_ADMIN)]
        [ButtonAction]
        public virtual ActionResult UnlockUser(ManageUserViewModel model)
        {
            try
            {
                Membership.Provider.UnlockUser(model.User.UserName);
                AddStatusMessage(string.Format("User {0} unlock successfully.", model.User.UserName));
            }
            catch (ProviderException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(Actions.ManageUser(model.User.UserName));
        }

        /// <summary>
        /// Locked the account of any perticular user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Locking users requires role {0}", Roles = ROLE_SECURITY_ADMIN)]
        [ButtonAction]
        public virtual ActionResult LockedUser(ManageUserViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.User.UserName))
            {
                throw new ArgumentNullException("userName");
            }
            OracleDatastore db = null;
            try
            {
                db = new OracleDatastore(this.HttpContext.Trace);
                DbConnectionStringBuilder dcms8 = new DbConnectionStringBuilder();
                dcms8.ConnectionString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;

                // Creating the connection as super user
                db.CreateConnection(dcms8.ConnectionString, string.Empty);

                const string QUERY_ALTER_USER = "ALTER USER {0} ACCOUNT LOCK";

                var sql = string.Format(QUERY_ALTER_USER, model.User.UserName);
                db.ExecuteNonQuery(sql, null);
                AddStatusMessage(string.Format("{0} user account has been locked", model.User.UserName));
            }
            catch (ProviderException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
            return RedirectToAction(Actions.ManageUser(model.User.UserName));
        }

        /// <summary>
        /// This method is use to reset expiry password of any user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Resetting password requires role {0}", Roles = ROLE_SECURITY_ADMIN)]
        [ButtonAction]
        public virtual ActionResult ResetPassword(ManageUserViewModel model)
        {
            try
            {
                var passwd = Membership.Provider.ResetPassword(model.User.UserName, null);
                this.AddStatusMessage(string.Format("The temporary password for user {0} is {1}", model.User.UserName, passwd));
            }
            catch (ProviderException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(Actions.ManageUser(model.User.UserName));
        }

        /// <summary>
        /// This method is used for delete any user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Deleting users requires role {0}", Roles = ROLE_SECURITY_ADMIN)]
        [ButtonAction]
        public virtual ActionResult DeleteUser(ManageUserViewModel model)
        {
            try
            {
                Membership.DeleteUser(model.User.UserName);
                this.AddStatusMessage(string.Format("User {0} has been deleted", model.User.UserName));
                return RedirectToAction(Actions.ManageUser());
            }
            catch (ProviderException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(Actions.ManageUser(model.User.UserName));
            }
        }

        /// <summary>
        /// This method is used for kill session.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AuthorizeEx("Kill session for users requires role {0}", Roles = ROLE_SECURITY_ADMIN)]
        [HttpPost]
        public virtual ActionResult KillSession(UserSession model)
        {
            OracleDatastore db = null;
            try
            {
                db = new OracleDatastore(this.HttpContext.Trace);
                DbConnectionStringBuilder dcms8 = new DbConnectionStringBuilder();
                dcms8.ConnectionString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;

                // Creating the connection as super user
                db.CreateConnection(dcms8.ConnectionString, string.Empty);

                const string QUERY_ALTER_USER = "ALTER SYSTEM KILL SESSION '{0},{1}' IMMEDIATE";

                var sql = string.Format(QUERY_ALTER_USER, model.SessionId, model.SerialNumber);
                db.ExecuteNonQuery(sql, null);
                AddStatusMessage(string.Format("Session of user {0} kill successfully", model.UserName));
            }
            catch (ProviderException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
            return RedirectToAction(Actions.ManageUser(model.UserName));
        }

        #endregion
    }
}
