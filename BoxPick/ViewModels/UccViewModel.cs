using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.BoxPick.ViewModels
{
    /// <summary>
    /// Inherited <see cref="LastCartonId"/> is required. ScannedUccId must match UccIdToPick
    /// </summary>
    [ModelBinder(typeof(MasterModelBinder))]
    public class UccViewModel : MasterModelWithPallet
    {
        public UccViewModel(HttpSessionStateBase session)
            : base(session)
        {
            //_mainContentAction = MVC_BoxPick.BoxPick.MainContent.Ucc();
        }

        [Display(Name = "Box")]
        [UIHint("scan")]
        [DataType("scan")]
        public string ScannedUccId { get; set; }

        /// <summary>
        /// The scanned carton id which was successfully accepted regardless of whether it was later picked or not.
        /// </summary>
        [Required]
        public override string LastCartonId
        {
            get
            {
                return base.LastCartonId;
            }
       }
    }
}




//$Id$