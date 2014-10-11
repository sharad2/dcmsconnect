namespace DcmsMobile.BoxManager.ViewModels
{
    public abstract class ViewModelBase
    {
        /// <summary>
        /// Number of query executions. Displayed during debugging
        /// </summary>
        public int QueryCount { get; set; }

        /// <summary>
        /// Sound file to play on page load.
        /// </summary>
        public char Sound { get; set; }
    }
}



//$Id: ViewModelBase.cs 10760 2011-12-15 11:50:26Z rkandari $
