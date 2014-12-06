using System;
using System.Web.Security;
using System.Collections.Generic;

namespace EclipseLibrary.Oracle.Web.Security
{

    public class OracleMembershipUserAudit
    {
        /// <summary>
        /// What action was performed on this user
        /// </summary>
        /// <remarks>
        /// One of 'CREATE USER', 'DROP USER', 'ALTER USER', 'GRANT ROLE', 'REVOKE ROLE'
        /// </remarks>
        public string ActionName { get; set; }

        /// <summary>
        /// Which role was granted or revoked. This is always NULL for non role related actions.
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Action was sucess and failure?
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// When was this action performed
        /// </summary>
        public DateTime ActionTime { get; set; }

        /// <summary>
        /// Which oracle user performed this action
        /// </summary>
        public string ByOracleUserName { get; set; }

        /// <summary>
        /// Which OS User performed this action
        /// </summary>
        public string ByOsUserName { get; set; }

        /// <summary>
        /// Which windows terminal was used to perform this action
        /// </summary>
        public string TerminalName { get; set; }

    }

    public class OracleMembershipUserSession
    {
        public int SessionId { get; set; }

        public int SerialNumber { get; set; }
        
        ///// <summary>
        ///// This property is use for user name who loged in.
        ///// </summary>
        //public string UserName { get; set; }
        
        /// <summary>
        /// Which program is used to log in.
        /// </summary>
        public string OsExecutableName { get; set; }

        /// <summary>
        /// This property is use for give the status of loged in user.
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Which OS User performed this action.
        /// </summary>
        public string OsUserName { get; set; }
        
        /// <summary>
        /// Which machine was used to perform this action.
        /// </summary>
        public string MachineName { get; set; }
        
        /// <summary>
        /// Which module was used by user. 
        /// </summary>
        public string Module { get; set; }
        
        /// <summary>
        /// What action was performed on this user.
        /// </summary>
        public string ActionName { get; set; }
        
        /// <summary>
        /// This property is use to give client information.
        /// </summary>
        public string ClientInfo { get; set; }

        /// <summary>
        /// When user loged in.
        /// </summary>
        public DateTime LogonTime { get; set; }
        
        /// <summary>
        /// This property shows state of logged in user
        /// </summary>
        public string State { get; set; }
    }

    public class OracleMembershipUser : MembershipUser
    {
        public DateTime PasswordExpiryDate { get; private set; }


        private readonly string _userName = string.Empty;

        public OracleMembershipUser(string userName, string providerUserKey, DateTime lastLockoutDate, DateTime createDate,
            DateTime passwordExpiryDate)
        {
            _userName = userName;
            _lastLockoutDate = lastLockoutDate;
            _creationDate = createDate;
            _providerUserKey = providerUserKey;
            PasswordExpiryDate = passwordExpiryDate;
        }


        public override string UserName
        {
            get
            {
                return _userName;
            }
        }

        private readonly DateTime _lastLockoutDate;
        public override DateTime LastLockoutDate
        {
            get
            {
                return _lastLockoutDate;
            }
        }

        private readonly DateTime _creationDate;
        public override DateTime CreationDate
        {
            get
            {
                return _creationDate;
            }
        }

        /// <summary>
        /// User Id
        /// </summary>
        private readonly string _providerUserKey;
        public override object ProviderUserKey
        {
            get
            {
                return _providerUserKey;
            }
        }

        public IEnumerable<OracleMembershipUserAudit> AuditLog
        {
            get;
            internal set;
        }

        public IEnumerable<OracleMembershipUserSession> Sessions
        {
            get;
            internal set;
        }
    }

}
