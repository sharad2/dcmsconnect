using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.PieceReplenish.ViewModels
{

    public class BuildingAreaModel : ContextModel
    {
        [Display(Name = "# Cartons")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CartonCount { get; set; }
    }

    internal class BuildingStatsEventArgs : EventArgs
    {
        public ICollection<PullerActivityModel> PullerActivities { get; set; }

        public bool IsRefreshingNow;
    }

    public class BuildingViewModel : ViewModelBase
    {
        internal event EventHandler<BuildingStatsEventArgs> GenerateStats;

        private IEnumerable<PullerActivityModel> _pullerActivities;

        internal void OnGenerateStats()
        {
            if (_pullerActivities != null)
                return;

            if (GenerateStats != null)
            {
                var args = new BuildingStatsEventArgs();
                GenerateStats(this, args);
                _pullerActivities = args.PullerActivities;
                _isRefreshingNow = args.IsRefreshingNow;
            }
            _pullerActivities = _pullerActivities ?? Enumerable.Empty<PullerActivityModel>();
        }

        public IEnumerable<PullerActivityModel> PullerActivities
        {
            get
            {
                OnGenerateStats();
                return _pullerActivities;
            }
        }

        private IEnumerable<BuildingAreaModel> _buildingAreaChoiceList;

        public IEnumerable<BuildingAreaModel> BuildingAreaChoiceList
        {
            get
            {
                if (_buildingAreaChoiceList == null)
                {
                    return Enumerable.Empty<BuildingAreaModel>();
                }
                return _buildingAreaChoiceList;
            }
            set { _buildingAreaChoiceList = value; }
        }

        private bool _isRefreshingNow { get; set; }

        public bool IsRefreshingNow
        {
            get
            {
                OnGenerateStats();
                return _isRefreshingNow;
            }
        }
    }
}



/*
    $Id: IndexViewModel.cs 17727 2012-07-26 08:19:52Z bkumar $
    $Revision: 17727 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/ViewModels/IndexViewModel.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/ViewModels/IndexViewModel.cs 17727 2012-07-26 08:19:52Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:49:52 +0530 (Thu, 26 Jul 2012) $
*/
