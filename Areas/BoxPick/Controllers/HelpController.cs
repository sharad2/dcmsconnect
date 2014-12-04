using System.Web.Mvc;
using DcmsMobile.BoxPick.ViewModels;

namespace DcmsMobile.BoxPick.Areas.BoxPick.Controllers
{
    /// <summary>
    /// All actions normally expect to get invoked via GET. None of the methods take any parameters which means that
    /// these will get invoked regardless of the state of the context.
    /// </summary>
    [Route("help/{action}")]
    public partial class HelpController : BoxPickControllerBase
    {
        /// <summary>
        /// Always emit the success sound
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (this.ViewData.ModelMetadata != null)
            {
                var mm = this.ViewData.ModelMetadata.Model as MasterModel;
                if (mm != null)
                {
                    mm.Sound = 'S';
                }
            }

            base.OnActionExecuted(filterContext);
        }

        /// <summary>
        /// Click on Help link on pallet screen
        /// </summary>
        /// <returns></returns>
        [ActionName("Pallet")]
        public virtual ActionResult ShowPalletHelp()
        {
            return View(Views.PalletHelp, new PalletViewModel(this.Session));
        }

        /// <summary>
        /// Click on Help link on Carton screen
        /// </summary>
        /// <returns></returns>
        [ActionName("Carton")]
        public virtual ActionResult ShowCartonHelp()
        {
            var model = new CartonViewModel(this.Session);
            model.AlternateLocations = _repos.Value.GetAlternateLocations(model.CartonIdToPick);
            return View(Views.CartonHelp, model);
        }

        /// <summary>
        /// Click on Help link on UCC screen
        /// </summary>
        /// <returns></returns>
        [ActionName("Ucc")]
        public virtual ActionResult ShowUccHelp()
        {
            return View(Views.UccHelp, new UccViewModel(this.Session));
        }

        /// <summary>
        /// Click on Help link on SkipUCC screen
        /// </summary>
        /// <returns></returns>
        [ActionName("SkipUcc")]
        public virtual ActionResult ShowSkipUccHelp()
        {
            return View(Views.SkipUccHelp, new SkipUccViewModel(this.Session));
        }

        /// <summary>
        /// Click on Help link on Pallet Confirmation screen
        /// </summary>
        /// <returns></returns>
        [ActionName("PartialPickPallet")]
        public virtual ActionResult ShowPartialPickPalletHelp()
        {
            return View(Views.PartialPickPalletHelp, new PartialPickPalletViewModel(this.Session));
        }
    }
}




//$Id$