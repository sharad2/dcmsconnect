using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Helpers
{
    /// <summary>
    /// Null display text is displayed in the drop down for the inventory area list
    /// </summary>
    public enum BucketActivityType
    {
        [Display(Name = "")]  //For NotSet type must return Empty String
        NotSet,

        [Display(Name = "Pitching")]
        [DisplayFormat(NullDisplayText = "(Undecided)")]
        Pitching,

        [Display(Name = "Pulling")]
        [DisplayFormat(NullDisplayText = "(No Pulling)")]
        Pulling
    }
}