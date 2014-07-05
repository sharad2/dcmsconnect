using EclipseLibrary.Oracle.Web.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.DcmsRights.ViewModels
{
    /// <summary>
    /// User to whom the role has been or can be granted
    /// </summary>
    public class OracleUserModel
    {

        public OracleUserModel()
        {

        }
        public OracleUserModel(OracleMembershipUser entity)
        {
            this.UserName = entity.UserName;
            this.CreateDate = entity.CreationDate;
            //this.IsLocked = entity.IsLockedOut;
            this.LastLockoutDate = entity.LastLockoutDate == DateTime.MinValue ? (DateTime?)null : entity.LastLockoutDate;
            this.PasswordExpiryDate = entity.PasswordExpiryDate == DateTime.MinValue ? (DateTime?)null : entity.PasswordExpiryDate;            
        }


        [Display(Name = "User Name")]
        [Required]
        public string UserName { get; set; }

        [Display(Name = "Locked ?")]
        [DisplayFormat(NullDisplayText = "Not Locked")]
        public DateTime? LastLockoutDate { get; set; }

        [Display(Name = "Create Date")]
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// The date on which password expires. If this date is in the past, then the password has already expired.
        /// </summary>
        [Display(Name = "Password Expiring on")]
        [DisplayFormat(NullDisplayText = "Does not Expire")]
        public DateTime? PasswordExpiryDate { get; set; }

        /// <summary>
        /// Whether the password has already expired
        /// </summary>
        public bool PasswordExpired
        {
            get
            {
                return PasswordExpiryDate != DateTime.MinValue && PasswordExpiryDate < DateTime.Now;
            }
        }

        public bool IsLocked
        {
            get
            {
                return LastLockoutDate != DateTime.MinValue && LastLockoutDate != null;
            }
        }

        private IEnumerable<UserAudit> _userAuditLog;

        public IEnumerable<UserAudit> UserAuditLog
        {
            get
            {
                return _userAuditLog ?? Enumerable.Empty<UserAudit>();
            }
            set
            {
                _userAuditLog = value;
            }
        }

        private IEnumerable<UserSession> _userSessionLog;

        public IEnumerable<UserSession> UserSessionLog
        {
            get
            {
                return _userSessionLog ?? Enumerable.Empty<UserSession>();
            }
            set
            {
                _userSessionLog = value;
            }
        }
    }

    /// <summary>
    /// For showing all information of user by using Audit.
    /// </summary>
    public class UserAudit
    {
        /// <summary>
        /// What action was performed on this user
        /// </summary>
        /// <remarks>
        /// One of 'CREATE USER', 'DROP USER', 'ALTER USER', 'GRANT ROLE', 'REVOKE ROLE'
        /// </remarks>
        [Display(Name = "Action")]
        public string ActionName { get; set; }

        /// <summary>
        /// Which role was granted or revoked. This is always NULL for non role related actions.
        /// </summary>
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }

        /// <summary>
        /// Action was sucess and failure?
        /// </summary>
        [Display(Name="Result")]
        public string Result { get; set; }

        public bool IsSuccess { get { return Result.CompareTo("SUCCESS") == 0; } }

        /// <summary>
        /// When was this action performed
        /// </summary>
        [Display(Name = "Time")]
        public DateTime ActionTime { get; set; }

        /// <summary>
        /// Which oracle user performed this action
        /// </summary>
        [Display(Name = "Oracle User")]
        public string ByOracleUserName { get; set; }

        /// <summary>
        /// Which OS User performed this action
        /// </summary>
        [Display(Name = "OS User")]
        public string ByOsUserName { get; set; }

        /// <summary>
        /// Which windows terminal was used to perform this action
        /// </summary>
        [Display(Name = "Terminal")]
        public string TerminalName { get; set; }
    }

    /// <summary>
    ///  For showing all information of user loged in.
    /// </summary>
    public class UserSession
    {
        /// <summary>
        /// This value is only used for posting by KillSession
        /// </summary>
        public string UserName { get; set; }

        [Display(Name = "Session Id")]
        public int SessionId { get; set; }

        [Display(Name = "Serial Number")]
        public int SerialNumber { get; set; }

        /// <summary>
        /// Which program is use to loged in.
        /// </summary>
        [Display(Name = "OS Executable Name")]
        public string OsExecutableName { get; set; }

        /// <summary>
        /// This property is use for give the status of loged in user.
        /// </summary>
        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Which OS User performed this action.
        /// </summary>
        [Display(Name = "OS User")]
        public string OSUserName { get; set; }

        /// <summary>
        /// Which machine was used to perform this action.
        /// </summary>
        [Display(Name = "Machine Name")]
        public string MachineName { get; set; }

        /// <summary>
        /// Which module was used by user. 
        /// </summary>
        [Display(Name = "Module")]
        public string Module { get; set; }

        /// <summary>
        /// What action was performed on this user.
        /// </summary>
        [Display(Name = "Action Name")]
        public string ActionName { get; set; }

        /// <summary>
        /// This property is use to give client information.
        /// </summary>
        [Display(Name = "Client Info")]
        public string ClientInfo { get; set; }

        /// <summary>
        /// When user loged in.
        /// </summary>
        [Display(Name = "Logon Time")]
        public DateTime LogonTime { get; set; }

        /// <summary>
        /// This property shows state of loged in user
        /// </summary>
        [Display(Name = "State")]
        public string State { get; set; }
    }

}