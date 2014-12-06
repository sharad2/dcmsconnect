using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.BoxPick.ViewModels
{
    /// <summary>
    /// For accepting building.
    /// Validations:
    /// 1. Building required and must be of 5 characters.
    /// </summary>
    [ModelBinder(typeof(MasterModelBinder))]
    public class BuildingViewModel : MasterModel
    {
        public BuildingViewModel(HttpSessionStateBase session)
            : base(session)
        {
            _mainContentAction = MVC_BoxPick.BoxPick.MainContent.Building();
        }

        [Display(Name = "Building/Area")]
        [UIHint("scan")]
        [StringLength(6, MinimumLength=3)]
        public string ScannedBuildingOrArea
        {
            get;
            set;
        }
    }
}


//$Id$
