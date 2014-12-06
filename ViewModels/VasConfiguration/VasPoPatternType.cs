using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    /// <summary>
    /// Defines the various types of PO patterns which can be manipulated by Vas configuration
    /// </summary>
    /// <remarks>
    /// Display Attribute is used to construct pattern description. Group Name is the regular expression needed to enforce the associated constraint. {0} is replaced
    /// by the text entered by user.
    /// </remarks>
    public enum VasPoPatternType
    {
        [Display(Name = "starting with", GroupName = "^{0}.*")]
        StartsWith = 1,

        [Display(Name = "ending with", GroupName = ".*{0}")]
        EndsWith = 2,

        [Display(Name = "containing", GroupName = ".*{0}.*")]
        Contains = 3
    }

    /// <summary>
    /// GroupName represents the regular expression which refers to the allowable characters
    /// </summary>
    public enum VasPoTextType
    {
        [Display(Name = "Alphabet", GroupName = "[[:alpha:]]")]
        Alphabet = 1,

        [Display(Name = "Numeric", GroupName = "[[:digit:]]")]
        Numeric = 2,

        [Display(Name = "Alpha Numeric", GroupName = "[[:alnum:]]")]
        AlphaNumeric = 3
    }
}