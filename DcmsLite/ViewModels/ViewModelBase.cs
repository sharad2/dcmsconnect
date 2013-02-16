using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.DcmsLite.ViewModels
{
    public class ViewModelBase
    {
        public string BuildingId { get; set; }

        [DisplayFormat(NullDisplayText="Invalid Building Configuration")]
        public string BuildingDescription { get; set; }

        /// <summary>
        /// Role name required for the DCMS Lite application
        /// </summary>
        public string DcmsLiteRoleName { get; set; }

        /// <summary>
        /// Is user authorized with the required role?
        /// </summary>
        public bool IsEditable { get; set; }
    }
}