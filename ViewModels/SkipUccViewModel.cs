using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.BoxPick.ViewModels
{
    [ModelBinder(typeof(MasterModelBinder))]
    public class SkipUccViewModel : MasterModelWithPallet
    {
        public SkipUccViewModel(HttpSessionStateBase session)
            : base(session)
        {

        }

        [Display(Name = "Box")]
        [UIHint("scan")]
        public string ConfirmScan { get; set; }
    }
}



//$Id$