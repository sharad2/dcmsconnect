
using System.Collections.Generic;

namespace DcmsMobile.BoxPick.ViewModels.MainContent
{
    public class BuildingViewModel
    {
        public IList<ActivityModel> PendingActivities { get; set; }
    }

    public class ActivityModel
    {
        public string BuildingId { get; set; }

        public string DestinationArea { get; set; }

        public string PickModeText { get; set; }

        public int CountPallets { get; set; }

        public int CountBoxes { get; set; }

        public int PickableBoxCount { get; set; }

        public string AreaShortName { get; set; }
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