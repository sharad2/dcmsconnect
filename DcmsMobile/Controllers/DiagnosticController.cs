﻿using System;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.WebPages;
using DcmsMobile.Models;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.Controllers
{
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

        [HttpPost]
        public virtual ActionResult Index(DiagnosticModel model)
        {
            if (model.Choice == null)
            {
                return RedirectToAction(MVC_DcmsMobile.Home.Index());
                //return RedirectToAction("Index", "Home");
            }
            model.LastScan = model.Choice;
            switch (model.Choice.Length)
            {
                case 1:
                    model.Sound = 'E';
                    break;
                case 2:
                    model.Sound = 'W';
                    break;
                default:
                    model.Sound = 'S';
                    break;
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

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var vr = filterContext.Result as ViewResult;
            if (vr != null)
            {
                var model = vr.Model as ViewModelBase;
                if (model != null)
                {
                    model.Init(this.ControllerContext, this.Url);
                }
            }
            base.OnActionExecuted(filterContext);
        }
    }
}




//$Id$