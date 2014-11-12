

using System.Collections.Generic;

namespace DcmsMobile.Receiving.Areas.Receiving.Rad
{
    /// <summary>
    /// used by action AddUpdateSpotCheckSetting() to determine what to do
    /// </summary>
    public enum ModifyAction
    {
        Edit,
        Add,
        Delete
    }



    /// <summary>
    /// RadViewModel uses this to show list of spot check areas
    /// </summary>
    public class SpotCheckAreaModel
    {
        public SpotCheckAreaModel()
        {

        }

        public SpotCheckAreaModel(SpotCheckArea entity)
        {
            AreaId = entity.AreaId;
            BuildingId = entity.BuildingId;
        }

        /// <summary>
        /// Spot check area
        /// </summary>
        public string AreaId { get; set; }

        /// <summary>
        /// Building of Spot check area
        /// </summary>
        public string BuildingId { get; set; }
    }




    public class IndexViewModel : ISpotCheckListPartialViewModel
    {
        public bool EnableEditing { get; set; }

        public IList<SpotCheckConfigurationModel> SpotCheckList { get; set; }

        public IList<SpotCheckAreaModel> SpotCheckAreaList { get; set; }

        public IList<SewingPlantGroupModel> SewingPlants { get; set; }
    }
}



//$Id$
