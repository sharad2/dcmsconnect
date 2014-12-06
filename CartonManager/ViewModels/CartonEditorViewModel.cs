using DcmsMobile.CartonManager.Models;
using DcmsMobile.CartonManager.Repository;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.CartonManager.ViewModels
{
    /// <summary>
    /// What kind of pieces needed to be removed
    /// </summary>
    public enum PiecesRemoveFlag
    {

        Irregular,
        Samples
    }


    public class CartonEditorViewModel : AdvancedUiViewModel
    {
        private SelectListItem Map(SkuArea src)
        {
            return new SelectListItem
            {
                Text = src.ShortName + ": " + src.Description,
                Value = src.AreaId
            };
        }

        public int SelectedTab { get; set; }

        public string SkuDescription { get; set; }

        /// <summary>
        /// Property required by function removing irregular and samples
        /// </summary>
        public string BundleId { get; set; }

        /// <summary>
        /// How should the pieces change be handled
        /// </summary>
        public PiecesRemoveFlag PiecesFlag { get; set; }

        /// <summary>
        /// Number of Irregular pieces to transfer.
        /// </summary>      
        public int? IrregularPieces { get; set; }


        /// <summary>
        /// Number of sample pieces to transfer.
        /// </summary>
        public int? SamplePieces { get; set; }

        public IEnumerable<SelectListItem> IrregularAreaList { get; set; }
        public IEnumerable<SelectListItem> SamplesAreaList { get; set; }

        public string IrregularAreaId { get; set; }

        public string SamplesAreaId { get; set; }

        public bool EmptyCarton { get; set; }

        /// <summary>
        /// This property is used to carton area
        /// </summary>
        [Display(Name = "Area")]
        public string ShortName { get; set; }

        private string _destinationPalletId;

        //[BindUpperCase]
        [RegularExpression(@"^([P|p]\S{1,7})", ErrorMessage = "Pallet Id must begin with P and max length should be less then 9.")]
        public string DestinationPalletId
        {
            get
            {
                return _destinationPalletId;
            }
            set
            {
                _destinationPalletId = (value ?? string.Empty).ToUpper();
            }
        }


        public override void OnModelUpdated()
        {
            this.UpdatingRules.PalletId = this.DestinationPalletId;
        }
        /// <summary>
        /// Get info of carton
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        public override void OnViewExecuting(CartonManagerService service, ControllerContext context)
        {
            base.OnViewExecuting(service, context);
            var carton = service.GetCarton(this.ScanText);
            if (carton == null)
            {
                this.StatusMessages.Add(string.Format("Carton {0} has been deleted", this.ScanText));
                this.ScanText = string.Empty;
            }
            else if (string.IsNullOrEmpty(carton.BundleId))
            {
                if (this.EmptyCarton)
                {
                    service.DeleteEmptyCarton(this.ScanText);
                }
                this.StatusMessages.Add(string.Format("Carton {0} is empty and cannot be modified", this.ScanText));
                this.ScanText = string.Empty;
            }
            else
            {

                IrregularAreaList = service.GetTransferAreas(PiecesRemoveFlag.Irregular).Select(p => Map(p));
                SamplesAreaList = service.GetTransferAreas(PiecesRemoveFlag.Samples).Select(p => Map(p));
                this.UpdatingRules.Pieces = carton.Pieces;
                this.UpdatingRules.PriceSeasonCode = carton.PriceSeasonCode;
                this.UpdatingRules.QualityCode = carton.QualityCode;
                this.UpdatingRules.VwhId = carton.VwhId;
                this.QualificationRules.Rework = carton.RemarkWorkNeeded ? ReworkStatus.NeedsRework : ReworkStatus.DoesNotNeedRework;
                this.UpdatingRules.SkuBarCode = carton.SkuInCarton.UpcCode;
                this.UpdatingRules.SkuId = carton.SkuInCarton.SkuId;
                this.QualificationRules.IsReserved = carton.IsReserved;
                this.SkuDescription = carton.SkuInCarton.ToString();
                this.BundleId = carton.BundleId;
                this.ShortName = carton.CartonArea.ShortName;
                this.UpdatingRules.LocationID = carton.LocationId;
                this.DestinationPalletId = carton.PalletId;
            }
            var helper = new UrlHelper(context.RequestContext);
            if (helper.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchCarton1] != null)
            {
                this.UrlInquiryCarton = helper.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchCarton1, new
              {
                  id = carton.CartonId
              });
            }
        }
        /// <summary>
        /// We do not care about any base validations
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.UpdatingRules.Pieces <= 0 || this.UpdatingRules.Pieces > 999)
            {
                yield return new ValidationResult("Pieces in carton must be between 1 and 999");
            }
        }

        /// <summary>
        /// No need to validate reason code
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override IEnumerable<ValidationResult> ValidateReasonCode(ValidationContext validationContext)
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public string UrlInquiryCarton
        {
            get;
            set;
        }
    }

}