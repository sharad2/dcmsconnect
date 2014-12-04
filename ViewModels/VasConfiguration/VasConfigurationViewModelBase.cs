
namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    /// <summary>
    /// Properties which control whether editing is permissible
    /// </summary>
    public class VasConfigurationViewModelBase
    {
        public bool IsEditable { get; set; }

        public string EditableRoleName { get; set; }
    }
}