
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Diagnostics.Contracts;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace DcmsMobile.MainArea.Logon
{
    /// <summary>
    /// Default action for this controller is Index
    /// </summary>
    [RoutePrefix(LogonController.NameConst)]
    //[Route("{action=" + LogonController.ActionNameConstants.Index + "}")]
    public partial class LogonController : EclipseController
    {
        private const string OLD_PASSWORD = "LogonController_OldPassword";

        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        [Route("Logon", Name=DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Logon)]
        public virtual ActionResult Index(string returnUrl)
        {
            var model = new LogonModel
            {
                 ReturnUrl = returnUrl
            };
            return View(this.Views.Index, model);
        }

        /// <summary>
        /// Called from the ring scanner view. Simply remembers the user name entered by the user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("user")]
        public virtual ActionResult SetUser(LogonModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                return RedirectToAction(MVC_DcmsMobile.Home.Index());
            }
            return View(this.Views.Index, model);
        }

        /// <summary>
        /// Validates user name and password
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If the controller uses the recommended <c>AuthorizeEx</c> attribute, then reason is set in status messages.
        /// It helps to communicate to the user why login is being requested. The reason is displayed only if the
        /// user is already logged in.
        /// </remarks>
        [HttpPost]
        [Route("mobilelogin")]
        public virtual ActionResult Login(LogonModel model)
        {
            Contract.Requires(model != null, "The passed model should never be null");

            // If both user name empty, redirect to home page. Supports mobile behavior
            if (string.IsNullOrWhiteSpace(model.UserName) || string.IsNullOrWhiteSpace(model.Password))
            {
                this.AddStatusMessage("Login cancelled");
                return RedirectToAction(MVC_DcmsMobile.Home.Index());
            }

            try
            {
                bool b = MembershipService.ValidateUser(model.UserName, model.Password);
                if (b)
                {
                    FormsService.SignIn(model.UserName, false);
                    AddStatusMessage(string.Format("Logged in as {0}", model.UserName));
                    if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return RedirectToAction(MVC_DcmsMobile.Home.Index());
                    }
                    else
                    {
                        return Redirect(model.ReturnUrl);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username/password. Please try again.");
                    // Empty model forces restart of login
                    return View(this.Views.Index, new LogonModel());
                }
            }
            catch (MembershipPasswordException)
            {
                var cpmodel = new ChangeExpiredPasswordModel
                {
                    UserName = model.UserName,
                    ReturnUrl = model.ReturnUrl,
                    //Password = model.Password
                };
                this.TempData[OLD_PASSWORD] = model.Password;
                ModelState.AddModelError("", "Your password has expired. Please change it now.");
                return View(this.Views.ChangeExpiredPassword, cpmodel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(this.Views.Index, model);
            }
        }

        /// <summary>
        /// This action method implements voluntary change of passwords. The view asks for the old password.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("password", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ChangePassword)]
        public virtual ActionResult GetNewPassword()
        {
            var cpmodel = new GetNewPasswordViewModel();
            return View(this.Views.GetNewPassword, cpmodel);
        }

        /// <summary>
        /// The old and new passwords are being posted. The password will be changed.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("changepassword")]
        public virtual ActionResult ChangePassword(GetNewPasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.OldPassword))
            {
                // Old Password not entered. Just navigate to home page.
                ModelState.Clear();
                AddStatusMessage("Password not changed");
                return RedirectToAction(MVC_DcmsMobile.Home.Index());
            }

            if (!ModelState.IsValid)
            {
                // The passwords do not match. Ask for new password again.
                return View(this.Views.GetNewPassword, model);
            }

            try
            {
                if (MembershipService.ChangePassword(this.HttpContext.User.Identity.Name, model.OldPassword, model.NewPassword))
                {
                    // This is the success case
                    this.AddStatusMessage("Your password has been changed");
                    return RedirectToAction(MVC_DcmsMobile.Home.Index());
                }

                // We should never get here
                this.ModelState.AddModelError("", "Your password could not be changed");
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError("", ex.Message);
            }

            // This is the error case
            return View(this.Views.GetNewPassword, model);
        }

        /// <summary>
        /// If password successfully changed, redirects to return Url or home page. Otherwise redirects to login page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ChangeExpiredPassword")]
        public virtual ActionResult ChangeExpiredPassword(ChangeExpiredPasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewPassword) && string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                // Password not entered. Just navigate to home page.
                return RedirectToAction(MVC_DcmsMobile.Home.Index());
            }
            if (!ModelState.IsValid)
            {
                // The passwords do not match. Ask for new password again.
                return View(this.Views.ChangeExpiredPassword, model);
            }

            try
            {
                var oldPassword = this.TempData[OLD_PASSWORD] as string;
                if (!string.IsNullOrWhiteSpace(oldPassword) && MembershipService.ChangePassword(model.UserName,
                    oldPassword, model.NewPassword))
                {
                    // Password has successfully change. Authenticate the user.
                    FormsService.SignIn(model.UserName, false);
                    this.AddStatusMessage(string.Format("Password for {0} successfully changed", model.UserName));
                    if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return RedirectToAction(MVC_DcmsMobile.Home.Index());
                    }
                    else
                    {
                        return Redirect(model.ReturnUrl);
                    }
                }
                else
                {
                    // The only possible reason is that the old password did not match. Apologize and request login again.
                    this.ModelState.AddModelError("", "Old password was not correct. Please try logging in again.");
                    //return RedirectToAction("Index", new
                    //{
                    //    ReturnUrl = model.ReturnUrl
                    //});
                    return RedirectToAction(MVC_DcmsMobile.Logon.Index(model.ReturnUrl));
                }
            }
            catch (Exception ex)
            {
                // We will need the old password after next post
                this.TempData.Keep(OLD_PASSWORD);
                this.ModelState.AddModelError("", ex.Message);
                return View(this.Views.ChangeExpiredPassword, model);
            }
        }

        [Route("Logoff", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Logoff)]
        public virtual ActionResult Logoff()
        {
            if (this.Session != null)
            {
                //Clean session if it exists
                Session.Abandon();
            }
            FormsService.SignOut();
            return RedirectToAction(MVC_DcmsMobile.Home.Index());
        }
    }
}




//$Id$