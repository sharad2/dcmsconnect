
namespace DcmsMobile.PalletLocating.ViewModels
{
    /// <summary>
    /// Base class of all view models
    /// </summary>
    public abstract class ViewModelBase
    {
        /// <summary>
        /// Number of queries executed during this request
        /// </summary>
        public int QueryCount { get; set; }

        /// <summary>
        /// This is true only if mobile screens are being displayed on the desktop browser
        /// </summary>
        public bool EmulatingMobile { get; set; }

        /// <summary>
        /// Sound file to play on page load.
        /// </summary>
        public char Sound
        {
            get;
            set;
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