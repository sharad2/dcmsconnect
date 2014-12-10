using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.Areas.PickWaves
{
    //[MetadataType(typeof(SubAreas))]
    public class PickWavesAreaRegistration : AreaRegistration
    {
        //[Display(Description = "Create and Manage Pick Waves", Name = "Manage Pick Waves", Order = 500, ShortName = "PWAV")]
        //[UIHint("desktop", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "PickWaves";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            //context.MapRoute(
            //    "PickWaves_default",
            //    "PickWaves/{controller}/{action}/{id}",
            //    new
            //    {
            //        controller = MVC_PickWaves.PickWaves.Home.Name,
            //        action = MVC_PickWaves.PickWaves.Home.ActionNames.Index,
            //        id = UrlParameter.Optional
            //    },
            //    new[] { typeof(DcmsMobile.PickWaves.Areas.PickWaves.Home.HomeController).Namespace }
            //);
            ModelUnbinderHelpers.ModelUnbinders.Add(new DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves.IndexViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(DcmsMobile.PickWaves.Areas.PickWaves.CreateWave.IndexViewModel),
                new DcmsMobile.PickWaves.Areas.PickWaves.CreateWave.IndexViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(DcmsMobile.PickWaves.Areas.PickWaves.CreateWave.PickslipListViewModel),
                new DcmsMobile.PickWaves.Areas.PickWaves.CreateWave.PickslipListViewModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(new DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves.WaveViewModelUnbinder());
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