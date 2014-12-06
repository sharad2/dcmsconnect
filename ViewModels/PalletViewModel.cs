using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.BoxPick.ViewModels
{
    /// <summary>
    /// For accepting pallet.
    /// Validations:
    /// 1. Building required (inherited)
    /// </summary>
    [ModelBinder(typeof(MasterModelBinder))]
    public class PalletViewModel : MasterModel
    {
        public PalletViewModel(HttpSessionStateBase session)
            : base(session)
        {
            //_mainContentAction = MVC_BoxPick.BoxPick.MainContent.Pallet();
        }
        public const string REGEX_PALLET = @"^P\S{1,}";

        [Display(Name = "Pallet")]
        [RegularExpression(REGEX_PALLET, ErrorMessage = "Pallet Id must begin with a P.")]
        [UIHint("scan")]
        [DataType("scan")]
        public string ScannedPalletId { get; set; }

        [Required(ErrorMessage = "Building is required")]
        public override string CurrentBuildingId
        {
            get
            {
                return base.CurrentBuildingId;
            }
            set
            {
                base.CurrentBuildingId = value;
            }
        }
    }
}



//$Id$