namespace DcmsMobile.PickWaves.ViewModels.Config
{
    public class IndexViewModel : ViewModelBase
    {
        /// <summary>
        /// Max Id of Customer who have overwritten splh
        /// </summary>
        public string MaxCaseId { get; set; }

        /// <summary>
        /// This property stores short description for case
        /// </summary>
        public string MaxCaseDescription { get; set; }

        public decimal? MaxContentVolume { get; set; }

        /// <summary>
        /// Min Id of Customer who have overwritten splh
        /// </summary>
        public string MinCaseId { get; set; }

        /// <summary>
        /// This property stores short description for case
        /// </summary>
        public string MinCaseDescription { get; set; }

        public decimal? MinContentVolume { get; set; }
    }
}
