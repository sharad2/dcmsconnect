using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.PieceReplenish.ViewModels
{
    internal class GenerateStatsEventArgs : EventArgs
    {
        public ICollection<AisleReplenishmentModel> AisleReplenishmentStats { get; set; }

        public DateTime? QueryTime { get; set; }

        public bool IsRefreshingNow { get; set; }

        public DateTime? NextRunDate { get; set; }
    }

    /// <summary>
    /// Some properties are expensive to recreate and they are recreated only if they are accessed.
    /// The recreation is done by raising the GenerateStats event
    /// </summary>
    /// <remarks>
    /// BUG: When this model is posted, all properties are potentially accessed by the validation framework. This defeats the event based recreation of expensive properties.
    /// Luckily, there will be no associated event during model binding. If the event is associated after model binding, it will not work as expected.
    /// </remarks>
    public class PalletViewModel : ViewModelBase
    {
        private string _palletId;
        [Display(Name = "Scan Pallet")]
        [RegularExpression(@"^([P|p]\S{1,7})", ErrorMessage = "Pallet must start with P and should not exceed 8 characters.")]
        [ReadOnly(false)]
        public string PalletId
        {
            get
            {
                return _palletId;
            }
            set
            {
                _palletId = value != null ? value.Trim().ToUpper() : value;
            }
        }

        [ReadOnly(false)]
        [Required]
        public ContextModel Context { get; set; }

        /// <summary>
        /// This event is raised ONCE when replenishment statistics are requested by the desktip view
        /// </summary>
        internal event EventHandler<GenerateStatsEventArgs> GenerateStats;

#if DEBUG
        // This bool is being used to protect us from recursive cals
        private bool _inGenerateStats;
#endif
        private void OnGenerateStats()
        {
#if DEBUG
            if (_inGenerateStats)
            {
                throw new InvalidOperationException("Recursive call to OnGenerateStats() detected");
            }
            _inGenerateStats = true;
#endif
            if (_aisleReplenishmentStats == null)
            {
                if (GenerateStats != null)
                {
                    var args = new GenerateStatsEventArgs();
                    GenerateStats(this, args);
                    _aisleReplenishmentStats = args.AisleReplenishmentStats;
                    _queryTime = args.QueryTime;
                    _totalPiecesToPull = _aisleReplenishmentStats.Sum(p => p.TotalPiecesToPull);
                    _totalCartonsToPull = _aisleReplenishmentStats.Sum(p => p.CartonsToPull);
                    _isRefreshingNow = args.IsRefreshingNow;
                    _nextRunDate = args.NextRunDate;
                }
                _aisleReplenishmentStats = _aisleReplenishmentStats ?? Enumerable.Empty<AisleReplenishmentModel>();
            }
#if DEBUG
            _inGenerateStats = false;
#endif
        }

        private IEnumerable<AisleReplenishmentModel> _aisleReplenishmentStats;

        public IEnumerable<AisleReplenishmentModel> AisleReplenishmentStats
        {
            get
            {
                OnGenerateStats();
                return _aisleReplenishmentStats;
            }
        }

        private DateTime? _queryTime;

        [DisplayFormat(DataFormatString = "{0:t}", NullDisplayText = "Never updated")]
        public DateTime? QueryTime
        {
            get
            {
                OnGenerateStats();
                return _queryTime;
            }
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

        private int _totalPiecesToPull;

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalPiecesToPull
        {
            get
            {
                OnGenerateStats();
                return _totalPiecesToPull;
            }
        }

        private int _totalCartonsToPull;

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalCartonsToPull
        {
            get
            {
                OnGenerateStats();
                return _totalCartonsToPull;
            }
        }

        private DateTime? _nextRunDate;

        [DisplayFormat(NullDisplayText = "(Not Scheduled)")]
        public DateTime? NextRunDate
        {
            get
            {
                OnGenerateStats();
                return _nextRunDate;
            }
        }

        public static string InventoryShortageReportUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_130/R130_28.aspx";
            }
        }
    }

}


/*
    $Id: PalletViewModel.cs 17727 2012-07-26 08:19:52Z bkumar $
    $Revision: 17727 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/ViewModels/PalletViewModel.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/ViewModels/PalletViewModel.cs 17727 2012-07-26 08:19:52Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:49:52 +0530 (Thu, 26 Jul 2012) $
*/
