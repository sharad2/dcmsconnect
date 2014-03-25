using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Models
{
    public class LogonModel:ViewModelBase
    {
        [Display(Name = "User Name")]
        [UIHint("scan")]
        public string UserName { get; set; }

        [UIHint("scan")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Url)]
        public string ReturnUrl { get; set; }
    }

    public class ChangeExpiredPasswordModel:ViewModelBase
    {
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [DataType(DataType.Url)]
        public string ReturnUrl { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Retype Password")]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The passwords do not match. Please try again")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// To change the password of the logged in user
    /// </summary>
    public class GetNewPasswordViewModel:ViewModelBase
    {
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Required]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Required]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Retype Password")]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The passwords do not match. Please try again")]
        [Required]
        public string ConfirmPassword { get; set; }
    }

}




//<!--$Id$-->
