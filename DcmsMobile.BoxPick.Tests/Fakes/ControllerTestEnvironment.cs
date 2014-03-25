using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DcmsMobile.BoxPick.Tests.Fakes
{
    public class ControllerTestEnvironment<TController> where TController : Controller, new()
    {
        private readonly NameValueCollection _queryString;
        private readonly NameValueCollection _form;
        private readonly TController _controller;
        private readonly WebHeaderCollection _headers;
        private readonly FakeSession _session;

        public ControllerTestEnvironment()
        {
            _session = new FakeSession(false);
            _queryString = new NameValueCollection();
            _form = new NameValueCollection();
            _headers = new WebHeaderCollection();
            _controller = new TController();

            Initialize();
        }

        public ControllerTestEnvironment(FakeSession session)
        {
            _session = session;
            _queryString = new NameValueCollection();
            _form = new NameValueCollection();
            _headers = new WebHeaderCollection();
            _controller = new TController();

            Initialize();
        }

        private void Initialize()
        {
            this.RequestMethod = "GET";

            // Construct the Mock object required for the test.
            // Setting CallBase to true can help debug null reference errors
            var request = new Mock<HttpRequestBase>(MockBehavior.Strict) { CallBase = false };
            request.SetupGet(r => r.Headers).Returns(_headers);
            request.SetupGet(r => r.Form).Returns(this.Form);
            request.SetupGet(r => r.QueryString).Returns(this.QueryString);
            const string FAKE_URL = @"http://does.not.matter";
            request.SetupGet(r => r.RawUrl).Returns(FAKE_URL);
            request.SetupGet(r => r.HttpMethod).Returns(() => this.RequestMethod);
            request.SetupGet(r => r.RequestType).Returns(() => this.RequestMethod);
            request.Setup(r => r.ValidateInput());
            request.SetupGet(r => r.UserAgent).Returns(() => _headers[HttpRequestHeader.UserAgent]);
            request.SetupGet(r => r.Url).Returns(() => new Uri(FAKE_URL));
            request.SetupGet(r => r.Cookies).Returns(new HttpCookieCollection());
            request.SetupGet(r => r.ApplicationPath).Returns("/BoxPick/Index");
            request.SetupGet(r => r.ServerVariables).Returns(new NameValueCollection());

            var cachePolicy = new Mock<HttpCachePolicyBase>(MockBehavior.Strict);
            cachePolicy.Setup(p => p.SetProxyMaxAge(It.IsAny<TimeSpan>()));
            cachePolicy.Setup(p => p.AddValidationCallback(It.IsAny<HttpCacheValidateHandler>(), It.IsAny<object>()));

            var response = new Mock<HttpResponseBase>(MockBehavior.Strict) { CallBase = false };
            response.SetupGet(p => p.Cache).Returns(cachePolicy.Object);
            response.Setup(r => r.ApplyAppPathModifier(It.IsAny<string>())).Returns("/BoxPick/Index");

            var identity = new Mock<IIdentity>(MockBehavior.Strict);
            identity.SetupGet(i => i.IsAuthenticated).Returns(() => !string.IsNullOrEmpty(this.UserName));
            identity.SetupGet(i => i.Name).Returns(() => this.UserName);

            var principal = new Mock<IPrincipal>(MockBehavior.Strict);
            principal.SetupGet(p => p.Identity).Returns(identity.Object);
            principal.Setup(p => p.IsInRole(It.IsAny<string>()))
                .Returns((string role) => role == this.Role);

            var httpContext = new Mock<HttpContextBase>(MockBehavior.Strict) { CallBase = false };
            httpContext.SetupGet(r => r.Request).Returns(request.Object);
            httpContext.SetupGet(r => r.Response).Returns(response.Object);
            httpContext.SetupGet(r => r.User).Returns(principal.Object);
            httpContext.SetupGet(r => r.Items).Returns(new Dictionary<object, object>());
            httpContext.SetupGet(r => r.Cache).Returns(HttpRuntime.Cache);

            httpContext.SetupGet(r => r.Session).Returns(_session);

            var routeData = new RouteData();
            var name = typeof(TController).Name;
            var index = name.LastIndexOf("Controller");
            routeData.Values.Add("controller", name.Substring(0, index));
            var controllerContext = new ControllerContext(httpContext.Object, routeData, _controller);
            _controller.ControllerContext = controllerContext;
        }

        /// <summary>
        /// Access the controller to write test assertions
        /// </summary>
        public TController Controller
        {
            get
            {
                return _controller;
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return _headers;
            }
        }

        public FakeSession Session
        {
            get
            {
                return _session;
            }
        }

        /// <summary>
        /// Set up your conditions and then call this to execute the action
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="actionName"></param>
        /// <param name="methodName">Should be null if you are not expecting the method to get called</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public TResult InvokeAction<TResult>(string actionName, string methodName) where TResult : ActionResult
        {
            
            var invoker = new FakeControllerActionInvoker();

            _controller.ActionInvoker = invoker;

            // Create the value provider containing both form and query values
            var nvc = new NameValueCollection(_form);
            foreach (string item in _queryString)
            {
                nvc[item] = _queryString[item];
            }
            _controller.ValueProvider = new NameValueCollectionValueProvider(nvc, CultureInfo.CurrentUICulture);

            bool b = _controller.ActionInvoker.InvokeAction(_controller.ControllerContext, actionName);
            Assert.IsTrue(b, "Action {0} was not found", actionName);

            if (string.IsNullOrEmpty(methodName))
            {
                Assert.IsNull(invoker.InvokedActionDescriptor, "No method should get invoked for action {0}", actionName);
            }
            else
            {
                Assert.IsNotNull(invoker.InvokedActionDescriptor, "Expecting method {0} to get invoked", methodName);
                Assert.AreEqual(actionName, invoker.InvokedActionDescriptor.ActionName, "Unexpected action invoked");
                Assert.IsInstanceOfType(invoker.InvokedActionDescriptor, typeof(ReflectedActionDescriptor), "Unexpected ActionDescriptor");
                var rad = (ReflectedActionDescriptor)invoker.InvokedActionDescriptor;
                Assert.AreEqual(methodName, rad.MethodInfo.Name, "Unexpected method invoked");
            }

            Assert.IsInstanceOfType(invoker.InvokedResult, typeof(TResult), "Unexpected ActionResult returned by the Action method {0}", methodName);
            return (TResult)invoker.InvokedResult;
        }

        /// <summary>
        /// Get/Set the request method
        /// </summary>
        public string RequestMethod { get; set; }

        /// <summary>
        /// Get set query string values
        /// </summary>
        public NameValueCollection QueryString
        {
            get
            {
                return _queryString;
            }
        }

        /// <summary>
        /// Get/set form values
        /// </summary>
        public NameValueCollection Form
        {
            get
            {
                return _form;
            }
        }

        /// <summary>
        /// Setting user name to non empty makes the request authenticated
        /// </summary>
        public string UserName { get; set; }

        public string Role { get; set; }
    }
}
