using System;
using System.Reflection;
using System.Web.Mvc;

namespace DcmsMobile.DcmsRights.Helpers
{
    /// <summary>
    /// A form can have multiple buttons. This attribute enables you to specify the action name as the name of the button. The action name of the of the form must be set to
    /// <c>Action</c>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Inspired by http://blog.ashmind.com/2010/03/15/multiple-submit-buttons-with-asp-net-mvc-final-solution/
    /// </para>
    /// </remarks>
    public class ButtonActionAttribute : ActionNameSelectorAttribute
    {
        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
        {
            if (!actionName.Equals("Action", StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (actionName.Equals(methodInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                return true;

            var request = controllerContext.RequestContext.HttpContext.Request;
            return request[methodInfo.Name] != null;
        }
    }
}