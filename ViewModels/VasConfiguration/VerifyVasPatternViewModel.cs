using System.Collections.Generic;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    public class VasSettingChangeModel
    {
        public string PoId { get; set; }

        public bool IsNew { get; set; }
    }

    public class VerifyVasPatternViewModel : EditVasModelBase
    {

        /// <summary>
        /// VAS did not apply and still does not apply applies
        /// </summary>
        public IList<VasSettingChangeModel> ListWillNotApply { get; set; }

        /// <summary>
        /// VAS used to apply and still applies
        /// </summary>
        public IList<VasSettingChangeModel> ListWillApply { get; set; }

    }
}