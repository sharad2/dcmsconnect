using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.BoxPick.ViewModels
{
    /// <summary>
    /// For accepting pallet confirmation.
    /// </summary>
    [ModelBinder(typeof(MasterModelBinder))]
    public class PartialPickPalletViewModel : MasterModelWithPallet
    {
        public PartialPickPalletViewModel(HttpSessionStateBase session)
            : base(session)
        {
        }

        [Display(Name = "Pallet")]
        [UIHint("scan")]
        public string ConfirmPalletId { get; set; }
    }
}



//$Id$