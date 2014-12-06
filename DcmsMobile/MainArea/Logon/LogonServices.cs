using System;
using System.Web.Security;

namespace DcmsMobile.MainArea.Logon
{
    #region FormsAuthenticationService
    public interface IFormsAuthenticationService
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
            if (String.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException("userName");
            }

            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
    #endregion

    #region AccountMembershipService
    public interface IMembershipService
    {
        bool ValidateUser(string userName, string password);
        bool ChangePassword(string userName, string oldPassword, string newPassword);
        MembershipUser GetUser(string userName);
    }

    public class AccountMembershipService : IMembershipService
    {
        public bool ValidateUser(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentNullException("userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentNullException("password");

            return Membership.Provider.ValidateUser(userName, password);
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            return Membership.Provider.ChangePassword(userName, oldPassword, newPassword);
        }

        public MembershipUser GetUser(string userName)
        {
            return Membership.Provider.GetUser(userName, false);
        }
    }
    #endregion
}



//<!--$Id$-->