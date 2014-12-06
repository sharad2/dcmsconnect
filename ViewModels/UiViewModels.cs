using DcmsMobile.CartonManager.Models;
using DcmsMobile.CartonManager.Repository;
using EclipseLibrary.Mvc.Html;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.CartonManager.ViewModels
{
    /// <summary>
    /// Information needed to update carton properties
    /// </summary>
    public class AdvancedUiViewModel : ViewModelBase
    {
        private SelectListItem Map(CodeDescriptionModel src)
        {
            return new SelectListItem
            {
                Text = src.Code + ":" + src.Description,
                Value = src.Code
            };
        }

        private GroupSelectListItem Map(CartonArea src)
        {
            return new GroupSelectListItem
            {
                Text = src.ShortName + " : " + src.Description,
                Value = src.AreaId,
                GroupText = string.IsNullOrEmpty(src.Building) ? "All Building" : src.Building
            };
        }
        public IEnumerable<SelectListItem> QualityList { get; set; }

        public IEnumerable<SelectListItem> PriceSeasonCodeList { get; set; }

        public IEnumerable<SelectListItem> VirutalWareHouseList { get; set; }

        public IEnumerable<SelectListItem> ReasonCodeList { get; set; }

        public IEnumerable<SelectListItem> PrinterList { get; set; }

        public IEnumerable<GroupSelectListItem> AreaList { get; set; }

        public override void OnViewExecuting(CartonManagerService service, ControllerContext context)
        {
            this.QualityList = service.GetQualityCodes().Select(Map);
            this.VirutalWareHouseList = service.GetVwhList().Select(Map);
            this.PriceSeasonCodeList = service.GetPriceSeasonCodes().Select(Map);
            this.ReasonCodeList = service.GetReasonCodes().Select(Map);
            this.PrinterList = service.GetZebraPrinters().Select(Map);
            this.AreaList = Enumerable.Repeat(new GroupSelectListItem { Text = "(Any)", Value = "" }, 1).Concat(service.GetCartonAreas(null, null).Select(Map));
        }

        public override void OnModelUpdated()
        {
            //throw new NotImplementedException();
        }
    }

    public class MarkReworkCompleteViewModel : ViewModelBase // <QualifyRulesNeedsRework, UpdateRuleCompleteRework>
    {
        public MarkReworkCompleteViewModel()
        {
            this.QualificationRules.Rework = ReworkStatus.NeedsRework;
            this.UpdatingRules.Rework = ReworkStatus.CompleteRework;
        }
        public IEnumerable<GroupSelectListItem> AreaList { get; set; }

        public override void OnModelUpdated()
        {
            //throw new NotImplementedException();
        }
    }

    public class PalletizeViewModel : ViewModelBase  //<QualifyRulesNoRework, UpdateRulesPalletArea>
    {
        public PalletizeViewModel()
        {
            this.QualificationRules.Rework = ReworkStatus.DoesNotNeedRework;

        }

        private string _destinationPalletId;
        [Display(Name = "Scan Pallet")]
        [Required(ErrorMessage = "Please provide destination Pallet.")]
        [RegularExpression(@"^([P|p]\S{1,7})", ErrorMessage = "Pallet Id must begin with P and max length should be less then 9.")]
        public string DestinationPalletId
        {
            get
            {
                return this._destinationPalletId;
            }
            set
            {
                _destinationPalletId = value != null ? value.ToUpper() : null;
            }
        }

        [Required(ErrorMessage = "Please provide destination Area")]
        [Display(Name = "Destination Area")]
        public string DestinationAreaId { get; set; }

        public IEnumerable<GroupSelectListItem> AreaList { get; set; }

        public override void OnModelUpdated()
        {
            this.UpdatingRules.PalletId = this.DestinationPalletId;
            this.UpdatingRules.AreaId = this.DestinationAreaId;
        }
    }

    public class AbandonReworkViewModel : ViewModelBase   //<QualifyRulesNeedsRework, UpdateRulesAbandonRework>
    {
        public AbandonReworkViewModel()
        {
            this.QualificationRules.Rework = ReworkStatus.NeedsRework;
            this.UpdatingRules.Rework = ReworkStatus.DoesNotNeedRework;
        }

        public IEnumerable<GroupSelectListItem> AreaList { get; set; }

        public override void OnModelUpdated()
        {
            //throw new System.NotImplementedException();
        }
    }

    public class DestinationPalletForMobileViewModel : SoundModel
    {
        private string _palletId;
        [Display(Name = "Pallet")]
        [RegularExpression(@"^([P|p]\S{1,7})", ErrorMessage = "Pallet Id must begin with P,min length should be greater then 1 and max length should be less then 9.")]
        public string PalletId
        {
            get
            {
                return this._palletId;
            }
            set
            {
                _palletId = value != null ? value.ToUpper() : null;
            }
        }

        public string AreaId { get; set; }

        public string ShortName { get; set; }

        [Display(Name = "Building")]
        public string BuildingId { get; set; }
    }

    public class PalletizeMobileViewModel : ViewModelBase       //<QualifyRulesNoRework, CartonModel>
    {
        public PalletizeMobileViewModel()
        {
            this.QualificationRules.Rework = ReworkStatus.DoesNotNeedRework;
        }

        //public override void PostUpdateCarton(Repository.ICartonManagerService service)
        //{
        //    //throw new NotImplementedException();
        //}

        public override void OnModelUpdated()
        {
            //throw new NotImplementedException();
        }

        [Display(Name = "Building")]
        public string BuidingId { get; set; }

        public string AreaShorName { get; set; }
    }

    public class DestinationAreaForMobileViewModel : SoundModel
    {
        private string _shortName;
        [Display(Name = "Area")]
        [Required(ErrorMessage = "Area must be passed.")]
        public string AreaShortName
        {
            get
            {
                return this._shortName;
            }
            set
            {
                _shortName = value != null ? value.ToUpper() : null;
            }
        }
    }

    public class DestinationBuildingForMobileViewModel : SoundModel
    {
        private string _buildingId;
        [Display(Name = "Building")]
        [Required(ErrorMessage = "Building is required")]
        public string BuildingId
        {
            get
            {
                return this._buildingId;
            }
            set
            {
                _buildingId = value != null ? value.ToUpper() : null;
            }
        }

        //public string AreaId { get; set; }

        public string AreaShortName { get; set; }

        public IEnumerable<string> BuildingList { get; set; }
    }

}

//$Id$
