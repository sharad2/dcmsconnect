using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Web;
using System.Web.Mvc;
using DcmsMobile.BoxPick.Models;
using EclipseLibrary.Mvc.Helpers;
using System;

namespace DcmsMobile.BoxPick.ViewModels
{
    /// <summary>
    /// Inherits pallet validation logic to ensure that a carton can be picked for the (inherited) current pallet.
    /// </summary>
    [ModelBinder(typeof(MasterModelBinder))]
    public class CartonViewModel : MasterModelWithPallet
    {
        public CartonViewModel(HttpSessionStateBase session)
            : base(session)
        {
            //_mainContentAction = MVC_BoxPick.BoxPick.MainContent.Carton();
        }

        public override ActionResult MainContentAction
        {
            get
            {
                return MVC_BoxPick.BoxPick.MainContent.Carton(this.CurrentPalletId);
            }
        }

        [UIHint("scan")]
        [DataType("scan")]
        [Display(Name = "Carton")]
        public string ScannedCartonId { get; set; }

        [UIHint("scan")]
        [DataType("scan")]
        [Display(Name = "Carton")]
        public string ConfirmCartonId { get; set; }

        public bool SuspenseFlag { get; set; }

        [Display(Name = "Alternate Locations")]
        public IEnumerable<string> AlternateLocations { get; set; }

        //[Obsolete]
        //public void ValidateCarton(Carton carton, ControllerContext ctx)
        //{
        //    Contract.Requires(carton != null);

        //    //var fieldName = ReflectionHelpers.FieldNameFor((CartonViewModel m) => m.ScannedCartonId);
        //    var fieldName = this.NameFor(m => m.ScannedCartonId);
        //    string msg;
        //    if (carton.SkuInCarton == null)
        //    {
        //        ctx.Controller.ViewData.ModelState.AddModelError(fieldName, "This carton does not contain any SKU");
        //        return;
        //    }

        //    if (this.SkuIdToPick != carton.SkuInCarton.SkuId)
        //    {
        //        msg = string.Format("Carton contains {0}. UCC Label is for {1}", carton.SkuInCarton.DisplayName,
        //            this.SkuDisplayNameToPick);
        //        ctx.Controller.ViewData.ModelState.AddModelError(fieldName, msg);
        //        return;
        //    }
        //    else if (this.PiecesToPick != carton.Pieces)
        //    {
        //        msg = string.Format("Carton contains {0:N0} pieces of SKU {1}. UCC Label requires {2:N0} pieces",
        //            carton.Pieces,
        //           carton.SkuInCarton.DisplayName,
        //            this.PiecesToPick);
        //        ctx.Controller.ViewData.ModelState.AddModelError(fieldName, msg);
        //        return;
        //    }

        //    if (this.QualityCodeToPick != carton.QualityCode)
        //    {
        //        msg = string.Format("Carton quality is {0}. UCC Label quality is for {1}", carton.QualityCode,
        //            this.QualityCodeToPick);
        //        ctx.Controller.ViewData.ModelState.AddModelError(fieldName, msg);
        //        return;
        //    }

        //    if (this.VwhIdToPick != carton.VwhId)
        //    {
        //        msg = string.Format("Carton virtual warehouse is {0}. UCC Label is for {1}", carton.VwhId,
        //            this.VwhIdToPick);
        //        ctx.Controller.ViewData.ModelState.AddModelError(fieldName, msg);
        //        return;
        //    }

        //    if (this.CartonSourceAreaToPick != carton.StorageArea)
        //    {
        //        msg = string.Format("Carton source area is {0}. UCC Label is for {1}", carton.StorageArea,
        //            this.CartonSourceAreaToPick);
        //        ctx.Controller.ViewData.ModelState.AddModelError(fieldName, msg);
        //        return;
        //    }
        //}
    }
}



//$Id$