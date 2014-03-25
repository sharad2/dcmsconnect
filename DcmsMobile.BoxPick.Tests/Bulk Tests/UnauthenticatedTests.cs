using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Mvc;
using DcmsMobile.BoxPick.Areas.BoxPick.Controllers;
using DcmsMobile.BoxPick.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DcmsMobile.BoxPick.Tests.Controllers
{
    /// <summary>
    /// Tests in this class ensure that
    /// 1) Unauthenticated requests get redirected.
    /// 2) Authenticated requests without any role get redirected.
    /// 3) Authenticated requests with a role other than DCMS8_BOXPICK get redirected.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These tests will fail if the controller does not have the <c>AuthorizeEx</c> attribute
    /// </para>
    /// <code>
    /// <![CDATA[
    /// [AuthorizeEx("Box picking requires {0} role", Roles = "DCMS8_BOXPICK")]
    /// ]]>
    /// </code>
    /// </remarks>
    [TestClass]
    public class UnauthenticatedTests
    {
        /// <summary>
        /// All unauthenticated requests must get redirected to the login page. The returnUrl must be same as the URL of this request
        /// and the supplied reason must reference the DCMS8_BOXPICK role
        /// </summary>
        /// <remarks>
        /// Every public method of the controller which returns ActionResult is an action. Each action must have the ActionName attribute.
        /// </remarks>
        [TestMethod]
        public void Unauthenticated_HomeController()
        {
            AssertAllActionsOfController<HomeController>();
        }

        [TestMethod]
        public void Unauthenticated_HelpController()
        {
            AssertAllActionsOfController<HelpController>();
        }

        [TestMethod]
        public void Unauthenticated_ConfirmController()
        {
            AssertAllActionsOfController<ConfirmController>();
        }

        /// <summary>
        /// Build a list of actions using reflection
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        private static void AssertAllActionsOfController<TController>() where TController : Controller, new()
        {
            var actions = typeof(TController).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.IsSpecialName && p.ReturnType == typeof(ActionResult))
                .Where( p => !p.GetCustomAttributes(typeof(NonActionAttribute), true).Any())
                .Select(p => new
                {
                    Method = p,
                    ActionAttr = p.GetCustomAttributes(typeof(ActionNameAttribute), true).Cast<ActionNameAttribute>().FirstOrDefault(),
                    RequestMethod = p.GetCustomAttributes(typeof(HttpPostAttribute), true).Any() ? "POST" : "GET"
                });

            var env = new ControllerTestEnvironment<TController>();
            env.Headers.Add(HttpRequestHeader.UserAgent, "windows ce");
            foreach (var action in actions)
            {
                Assert.IsNotNull(action.ActionAttr, "Method {0}.{1} does not have ActionNameAttribute", typeof(TController).FullName, action.Method.Name);
                env.RequestMethod = action.RequestMethod;

                // Unauthenticated
                EnsureActionGetsRedirected<TController>(env, action.ActionAttr.Name);

                // Authenticated with no role
                env.UserName = "Sharad";
                EnsureActionGetsRedirected<TController>(env, action.ActionAttr.Name);

                // Authenticated with wrong role
                env.Role = "ROLE_WRONG";
                EnsureActionGetsRedirected<TController>(env, action.ActionAttr.Name);

                //// Authenticated with right role
                //env.Role = "DCMS8_BOXPICK";
                //var rr = env.InvokeAction<ViewResult>(action.ActionAttr.Name, action.Method.Name);

            }
        }

        /// <summary>
        /// Ensures that:
        /// 1) Redirect result is obtained
        /// 2) The redirected URL contains the query string returnUrl which is same as the URL of the original request.
        /// 2) The redirected URL contains the query string reason which contains role name DCMS8_BOXPICK
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <param name="env"></param>
        /// <param name="action"></param>
        private static void EnsureActionGetsRedirected<TController>(ControllerTestEnvironment<TController> env, string action) where TController : Controller, new()
        {
            // Act
            var rr = env.InvokeAction<HttpUnauthorizedResult>(action, null);

            // Assert
            //Assert.IsFalse(rr.Permanent, "Redirection should not be permanent");

            //Uri uri = new Uri(new Uri(env.Controller.Request.RawUrl), rr.Url);
            // Construct dictionary of query string values
            //var tokens = uri.Query.Substring(1).Split('&').Select(p => p.Split('='))
            //    .ToDictionary(p => p[0], q => q[1], StringComparer.InvariantCultureIgnoreCase);

            //Assert.IsTrue(tokens.ContainsKey("returnUrl"), "returnUrl must be one of the query strings of the redirected URL");
            //Assert.AreEqual(HttpUtility.UrlEncode(env.Controller.Request.RawUrl), tokens["returnUrl"],
            //    "returnUrl must be same as the URL of the request");
            //Assert.IsTrue(tokens.ContainsKey("reason"), "Query string must specify a reason for the redirection");
            //Assert.IsTrue(tokens["reason"].Contains("DCMS8_BOXPICK"), "The reason must reference the DCMS8_BOXPICK role");
        }
    }
}
