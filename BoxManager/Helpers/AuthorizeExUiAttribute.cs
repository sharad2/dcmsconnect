using System;
using System.Web;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.BoxManager.Helpers
{
    public enum UiType
    {
        ScanToPallet,       // Default
        Vas
    }

    /// <summary>
    /// This attribute will perform authorization only when the posted UI matches the passed UI.
    /// </summary>
    public class AuthorizeExUiAttribute : AuthorizeExAttribute
    {
        /// <summary>
        /// A value must always be posted for this name
        /// </summary>
        public const string NAME_UITYPE = "UiType";

        private readonly UiType _ui;
        public AuthorizeExUiAttribute(string reasonFormatString, UiType ui)
            : base(reasonFormatString)
        {
            _ui = ui;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            UiType postedUi;
            Enum.TryParse(httpContext.Request.Params[NAME_UITYPE], true, out postedUi);
            if (postedUi != _ui)
            {
                // This attribute does not apply
                return true;
            }
            return base.AuthorizeCore(httpContext);
        }
    }
}