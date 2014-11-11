
using System.Collections.Generic;

namespace DcmsMobile.Receiving.ViewModels.Rad
{

    public interface ISpotCheckListPartialViewModel
    {
        bool EnableEditing { get; }

        IList<SpotCheckViewModel> SpotCheckList { get; set; }
    }

    public class SpotCheckAreaModel
    {
        /// <summary>
        /// Spot check area
        /// </summary>
        public string AreaId { get; set; }

        /// <summary>
        /// Building of Spot check area
        /// </summary>
        public string BuildingId { get; set; }
    }


    public class RadViewModel : ISpotCheckListPartialViewModel
    {
        public bool EnableEditing { get; set; }

        public IList<SpotCheckViewModel> SpotCheckList { get; set; }

        public IList<SpotCheckAreaModel> SpotCheckAreaList { get; set; }
    }
}



//$Id$