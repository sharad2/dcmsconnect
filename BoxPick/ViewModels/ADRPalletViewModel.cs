using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.BoxPick.ViewModels
{
    [ModelBinder(typeof(MasterModelBinder))]
    public class ADRPalletViewModel : MasterModel
    {
        public ADRPalletViewModel(HttpSessionStateBase session)
            : base(session)
        {

        }

        [UIHint("scan")]
        [DisplayName("Scan")]
        public string Scan { get; set; }

        [Display(Name = "Pallet")]
        [RegularExpression(PalletViewModel.REGEX_PALLET, ErrorMessage = "Pallet Id must begin with a P.")]
        public string ConfirmPalletId { get; set; }
    }
}



//$Id$