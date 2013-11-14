using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.Areas.PickWaves
{
    [MetadataType(typeof(SubAreas))]
    public class PickWavesAreaRegistration : AreaRegistration
    {
        [Display(Description = "Create and Manage Pick Waves", Name = "Manage Pick Waves", Order = 500, ShortName = "PWAV")]
        [UIHint("desktop", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "PickWaves";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PickWaves_default",
                "PickWaves/{controller}/{action}/{id}",
                new
                {
                    controller = MVC_PickWaves.PickWaves.Home.Name,
                    action = MVC_PickWaves.PickWaves.Home.ActionNames.Index,
                    id = UrlParameter.Optional
                },
                new[] { typeof(Controllers.HomeController).Namespace }
            );
            ModelUnbinderHelpers.ModelUnbinders.Add(new DcmsMobile.PickWaves.ViewModels.ManageWaves.IndexViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(DcmsMobile.PickWaves.ViewModels.CreateWave.IndexViewModel), 
                new DcmsMobile.PickWaves.ViewModels.CreateWave.IndexViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(new DcmsMobile.PickWaves.ViewModels.CreateWave.PickslipListSelectorViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(DcmsMobile.PickWaves.ViewModels.CreateWave.PickslipListViewModel),
                new DcmsMobile.PickWaves.ViewModels.CreateWave.PickslipListViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(new DcmsMobile.PickWaves.ViewModels.ManageWaves.WaveViewModelUnbinder());
        }
    }
}



/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/