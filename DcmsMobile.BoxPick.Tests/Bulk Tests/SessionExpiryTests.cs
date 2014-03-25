using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Mvc;
using DcmsMobile.BoxPick.Areas.BoxPick.Controllers;
using DcmsMobile.BoxPick.Tests.Fakes;
using DcmsMobile.BoxPick.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DcmsMobile.BoxPick.Tests.Controllers
{
    /// <summary>
    /// Expired session is passed to each action of each controller with POST and GET. We ensure that it redirects to home page
    /// along with an error message in TempData.
    /// </summary>
    [TestClass]
    public class SessionExpiredTests
    {
        /// <summary>
        /// Since we are posting, all actions, except Index, must redirect to home page.
        /// </summary>
        [TestMethod]
        public void Session_Expired_Get_Home()
        {
            AssertRedirection<HomeController>("GET", new[] { "Index", "PalletList"});
        }

        /// <summary>
        /// Index action should not get redirected
        /// </summary>
        [TestMethod]
        public void Session_Expired_Get_Home_Index()
        {
            // For Index Get, the method should get invoked
            var env = InitializeEnvironment<HomeController>();
            env.RequestMethod = "GET";
            var vr = env.InvokeAction<ViewResult>("Index", "AcceptBuilding");
            Assert.IsNotNull(vr);
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(MasterModel));
            var model = (MasterModel)vr.Model;
            Assert.AreEqual('\0', model.Sound);
        }

        /// <summary>
        /// Since we are posting, all actions must redirect to home page.
        /// </summary>
        [TestMethod]
        public void Session_Expired_Post_Home()
        {
            AssertRedirection<HomeController>("POST", new[] { "Index", "PalletList"});
        }

        /// <summary>
        /// HelpController does not care about expired sessions. The action should get invoked.
        /// </summary>
        [TestMethod]
        public void Session_Expired_Post_Help()
        {
            foreach (var action in GetActions<HelpController>())
            {
                var env = InitializeEnvironment<HelpController>();
                env.RequestMethod = "POST";
                var vr = env.InvokeAction<ViewResult>(action.Value, action.Key.Name);
                Assert.IsNotNull(vr);
                Assert.IsNotNull(vr.Model);
                Assert.IsInstanceOfType(vr.Model, typeof(MasterModel));
                var model = (MasterModel)vr.Model;
                Assert.AreEqual('S', model.Sound);
            }
        }

        /// <summary>
        /// HelpController does not care about expired sessions. The action should get invoked.
        /// </summary>
        [TestMethod]
        public void Session_Expired_Get_Help()
        {
            foreach (var action in GetActions<HelpController>())
            {
                var env = InitializeEnvironment<HelpController>();
                env.RequestMethod = "GET";
                var vr = env.InvokeAction<ViewResult>(action.Value, action.Key.Name);
                Assert.IsNotNull(vr);
                Assert.IsNotNull(vr.Model);
                Assert.IsInstanceOfType(vr.Model, typeof(MasterModel));
                var model = (MasterModel)vr.Model;
                Assert.AreEqual('S', model.Sound);
            }
        }

        /// <summary>
        /// All actions must get redirected
        /// </summary>
        [TestMethod]
        public void Session_Expired_Get_Confirm()
        {
            AssertRedirection<ConfirmController>("GET", new[] { "ADRPallet", "Print" });
        }

        /// <summary>
        /// All actions must get redirected
        /// </summary>
        [TestMethod]
        public void Session_Expired_Post_Confirm()
        {
            AssertRedirection<ConfirmController>("POST", new[] { "ADRPallet", "Print" });
        }

        /// <summary>
        /// Any public method which returns an ActionResult is considered to be an action
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<MethodInfo, string>> GetActions<TController>() where TController : Controller
        {
            var query = typeof(TController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => p.ReturnType == typeof(ActionResult) || p.ReturnType.IsSubclassOf(typeof(ActionResult)))
                .Where(p => !p.GetCustomAttributes(typeof(NonActionAttribute), true).Any());
            foreach (var method in query)
            {
                var actionAttr = method.GetCustomAttributes(typeof(ActionNameAttribute), true).Cast<ActionNameAttribute>().SingleOrDefault();
                //Assert.IsNotNull(actionAttr, "Method {0}.{1} does not have an action name attribute", typeof(TController), method.Name);
                yield return new KeyValuePair<MethodInfo, string>(method, actionAttr.Name);
            }
        }

        private void AssertRedirection<TController>(string requestMethod, IEnumerable<string> excludeActions = null) where TController : Controller, new()
        {
            foreach (var action in GetActions<TController>())
            {
                if (excludeActions != null && excludeActions.Contains(action.Value))
                {
                    continue;
                }
                var env = InitializeEnvironment<TController>();
                env.RequestMethod = requestMethod;
                var rr = env.InvokeAction<RedirectResult>(action.Value, null);
                Assert.AreEqual("/BoxPick/Index", rr.Url);
                Assert.IsFalse(env.Controller.ModelState.IsValid, "ModelState must be false");
            }
        }

        /// <summary>
        /// Creates an authenticated environment
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <returns></returns>
        private static ControllerTestEnvironment<TController> InitializeEnvironment<TController>() where TController : Controller, new()
        {
            var env = new ControllerTestEnvironment<TController>();
            env.Headers.Add(HttpRequestHeader.UserAgent, "windows ce");
            env.UserName = "Sharad";
            env.Role = "DCMS8_BOXPICK";
            return env;
        }
    }
}
