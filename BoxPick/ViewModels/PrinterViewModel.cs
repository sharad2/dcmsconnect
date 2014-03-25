using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.BoxPick.ViewModels
{
    [ModelBinder(typeof(MasterModelBinder))]
    public class PrinterViewModel : MasterModel
    {
        public PrinterViewModel(HttpSessionStateBase session)
            : base(session)
        {

        }

        private const string SESSION_KEY_SCAN = "PrinterViewModel_Scan";

        [UIHint("scan")]
        [DisplayName("Scan")]
        public string Scan { get; set; }
        

        [RegularExpression(PalletViewModel.REGEX_PALLET, ErrorMessage = "Pallet Id must begin with a P.")]
        [DisplayName("Pallet")]
        public string PalletToPrint
        {
            get
            {
                return _session[SESSION_KEY_SCAN] as string ?? string.Empty;
            }
            set
            {
                _session[SESSION_KEY_SCAN] = value;
            }
        }
    }
}



//$Id$