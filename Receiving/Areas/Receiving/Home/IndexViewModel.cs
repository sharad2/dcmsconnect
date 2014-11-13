using DcmsMobile.Receiving.Areas.Receiving.Home;
using DcmsMobile.Receiving.Areas.Receiving.Home.Repository;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.Receiving.Areas.Receiving.Home
{

    public class ReceivingProcessModel
    {
        public ReceivingProcessModel()
        {

        }

        public ReceivingProcessModel(ReceivingProcess entity)
        {
            ProDate = entity.ProDate;
            ProNumber = entity.ProNumber;
            CarrierId = entity.CarrierId;
            CarrierDescription = entity.CarrierName;
            OperatorName = entity.OperatorName;
            ReceivingStartDate = entity.StartDate;
            ReceivingEndDate = entity.ReceivingEndDate;
            CartonCount = entity.CartonCount;
            PalletCount = entity.PalletCount;
            //ReceivingAreaId = src.ReceivingAreaId,
            ProcessId = entity.ProcessId;
            ExpectedCartons = entity.ExpectedCartons;
        }

        [DisplayFormat(DataFormatString="{0:d}")]
        public System.DateTime? ProDate { get; set; }

        public string ProNumber { get; set; }

        public string CarrierId { get; set; }

        public string OperatorName { get; set; }

        public System.DateTime? ReceivingStartDate { get; set; }

        public string CarrierDescription { get; set; }

        public System.DateTime? ReceivingEndDate { get; set; }

        public int CartonCount { get; set; }

        public int PalletCount { get; set; }

        public int ProcessId { get; set; }

        public int? ExpectedCartons { get; set; }

        /// <summary>
        /// Elapsed time of Current receiving process
        /// </summary>
        [Display(Name = "Elapsed Time")]
        public string ElapsedTime
        {
            get
            {
                if (ReceivingEndDate != null && ReceivingStartDate != null)
                {
                    var interval = ReceivingEndDate.Value.Subtract(ReceivingStartDate.Value).Duration();
                    if (interval.TotalDays > 1)
                    {
                        return string.Format("{0:N0} days", interval.TotalDays);
                    }
                    else
                    {
                        return string.Format("{0:N0} hrs", interval.TotalHours);
                    }
                }
                return string.Empty;
            }
        }

        [Display(Name = "Carrier")]
        [DisplayFormat(NullDisplayText = "Unknown Carrier")]
        public string CarrierDisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.CarrierId))
                {
                    return null;
                }
                return string.Format("{0}: {1}", this.CarrierId, this.CarrierDescription);
            }
        }
    }

    /// <summary>
    /// Info for selecting or creating a process. Also recent processes.
    /// </summary>
    public class IndexViewModel
    {
        public IndexViewModel()
        {
            this.CreateProcess = new ProcessEditorViewModel();
        }

        private IList<ReceivingProcessModel> _recentProcesses;

        /// <summary>
        /// This will never be null
        /// </summary>
        public IList<ReceivingProcessModel> RecentProcesses
        {
            get
            {
                return _recentProcesses ?? new ReceivingProcessModel[0];
            }
            set
            {
                _recentProcesses = value;
            }
        }

        /// <summary>
        /// Form input for creating a process
        /// </summary>
        public ProcessEditorViewModel CreateProcess { get; set; }

    }
}




//$Id$