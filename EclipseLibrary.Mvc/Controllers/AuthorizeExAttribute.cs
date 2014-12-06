using System.Web.Mvc;
using System.Web;

namespace EclipseLibrary.Mvc.Controllers
{
    /// <summary>
    /// This class adds the ability to specify a reason for the login request.
    /// You specify the reason in the constructor. The format string can use the place holder {0}
    /// which will be replaced by the value of <see cref="AuthorizeAttribute.Roles"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The functionality is available only if all controllers involved are derived from <see cref="EclipseController"/>.
    /// </para>
    /// </remarks>
    public class AuthorizeExAttribute:AuthorizeAttribute
    {
        private readonly string _reasonFormatString;
        public AuthorizeExAttribute(string reasonFormatString)
        {
            _reasonFormatString = reasonFormatString;
        }

        private string _purpose;

        /// <summary>
        /// What will this role allow the user to accomplish. Defaults to the reason
        /// </summary>
        public string Purpose
        {
            get
            {
                return _purpose ?? string.Format(_reasonFormatString, this.Roles);
            }
            set
            {
                _purpose = value;
            }
        
        }

        /// <summary>
        /// Sharad 30 Sep 2011: If authorization skipping has been requested, claim that the request is authorized
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Sharad 12 Dec 2011: EclipseLibrary.Oracle.OracleRoleProvider returns WEB_PROXYUSER as the only role for the proxy user.
        /// We consider proxy users to be super users and they are always authorized.
        /// </para>
        /// </remarks>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return IsSuperUser(httpContext) || base.AuthorizeCore(httpContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var eclController = filterContext.Controller as EclipseController;
            if (eclController != null && !string.IsNullOrEmpty(_reasonFormatString))
            {
                string reason = string.Format(_reasonFormatString, this.Roles);
                eclController.AddStatusMessage(reason);
                // Not sure why this is required, but it is. Otherwise the reason gets lost
                eclController.TempData.Keep();
            }
            base.HandleUnauthorizedRequest(filterContext);
        }

        /// <summary>
        /// Returns true if authorization has been skipped, or if the user has the super user role WEB_PROXYUSER
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// Use this static function if you are writing authorization code and wish to emulate the behavior provided by this attribute.
        /// This attribute contains logic to allow full access to anyone who is logged in as the proxy user.
        /// </remarks>
        public static bool IsSuperUser(HttpContextBase httpContext)
        {
            return httpContext.SkipAuthorization || (httpContext.User.Identity.IsAuthenticated && httpContext.User.IsInRole("WEB_PROXYUSER"));
        }
    }
}
