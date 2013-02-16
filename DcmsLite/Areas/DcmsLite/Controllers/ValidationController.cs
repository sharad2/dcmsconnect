using DcmsMobile.DcmsLite.Repository.Validation;
using DcmsMobile.DcmsLite.ViewModels.Validation;
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Web.Mvc;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers
{
    public partial class ValidationController : DcmsLiteControllerBase<ValidationService>
    {
        [AuthorizeEx("Box validation with DCMS Lite requires role {0}", Roles = ROLE_DCMS_LITE)]
        public virtual ActionResult Index(string lastScan = null, char sound = '\0')
        {
            var model = new IndexViewModel
                {
                    PostVerifyArea = _service.GetPostVerificationArea(),
                    BadVerifyArea = _service.GetBadVerificationArea(),
                    LastScan = lastScan,
                    Sound = sound
                };
            return View(Views.Index, model);
        }

        [HttpPost]
        [AuthorizeEx("Box validation with DCMS Lite requires role {0}", Roles = ROLE_DCMS_LITE)]
        public virtual ActionResult ValidateBox(IndexViewModel model)
        {
            //Probably garbage box was scanned, return back
            if (!ModelState.IsValid)
            {
                return RedirectToAction(Actions.Index(model.UccId, 'W'));
            }

            try
            {
                //Validating box
                _service.ValidateBox(model.UccId);
                AddStatusMessage(string.Format("Box {0} is verified.", model.UccId));
                model.Sound = 'S';
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                model.Sound = 'E';
            }
            return RedirectToAction(Actions.Index(model.UccId, model.Sound));
        }

    }
}
