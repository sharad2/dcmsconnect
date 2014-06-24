using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.Receiving.ViewModels.Home
{
    /// <summary>
    /// Communicaets with the SelectProcess id form within the view
    /// </summary>
    public class SelectProcessModel
    {
        [Required]
        [Range(1, 999999999, ErrorMessageResourceType = typeof(Resources.Receiving), ErrorMessageResourceName = "RangeMinMaxErrorMessage")]
        public int? ProcessId { get; set; }
    }

    /// <summary>
    /// Info for selecting or creating a process. Also recent processes.
    /// </summary>
    public class IndexViewModel: ViewModelBase
    {
        public IndexViewModel()
        {
            this.CreateProcess = new ReceivingProcessModel();
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
        public ReceivingProcessModel CreateProcess { get; set; }

        /// <summary>
        /// Form input for selecting a process
        /// </summary>
        public SelectProcessModel SelectProcess { get; set; }

    }
}




//$Id$