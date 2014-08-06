using EclipseLibrary.Mvc.Controllers;
using System;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.WebPages;


namespace DcmsMobile.MainArea.Diagnostic
{
    [RoutePrefix(DiagnosticController.NameConst)]
    [Route("{action=" + DiagnosticController.ActionNameConstants.Index + "}")]
    public partial class DiagnosticController : EclipseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="emulateMobile">If true, mobile views are rendered on the desktop</param>
        /// <param name="redirectUrl">Used only if emulateMobile is passed</param>
        /// <returns></returns>
        /// <remarks>
        /// The parameter names must not be changed. launcher.cshtml calls this action with hardwired parameter names
        /// </remarks>
        [HttpGet]
        [Route(Name="DcmsMobile_Diagnostic")]
        public virtual ActionResult Index(bool? emulateMobile)
        {
            DiagnosticModel model = new DiagnosticModel();
            if (emulateMobile.HasValue)
            {
                if (emulateMobile.Value)
                {
                    HttpContext.SetOverriddenBrowser(BrowserOverride.Mobile);
                }
                else
                {
                    HttpContext.ClearOverriddenBrowser();
                }
                //MobileEmulation.EmulateMobile(this.ControllerContext, emulateMobile.Value);
                return RedirectToAction(MVC_DcmsMobile.Home.Actions.Index());
            }
            return View(model);
        }

        #region Email
        /// <summary>
        /// Displays the e-mail view
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual ActionResult Email(DiagnosticModel model)
        {
            var emailModel = new DiagnosticEmailModel();
            return View(emailModel);
        }

        /// <summary>
        /// Sending mail
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult SendEmail(DiagnosticEmailModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Email", model);
            }

            try
            {
                var msg = new MailMessage
                {
                    Body = model.Body,
                    Subject = model.Subject
                };
                msg.To.Add(model.To);
                using (var smtp = new SmtpClient())
                {
                    smtp.Send(msg);
                }
                this.AddStatusMessage("E-mail sent");
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    ModelState.AddModelError("", ex.Message);
                    ex = ex.InnerException;
                }
            }
            return View("Email", model);
        }
        #endregion

    }
}




//$Id$