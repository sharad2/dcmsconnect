using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using DcmsMobile.Receiving.Areas.Receiving.Controllers;
using DcmsMobile.Receiving.Models;
using DcmsMobile.Receiving.Repository;
using DcmsMobile.Receiving.ViewModels;
using DcmsMobile.Receiving.ViewModels.Home;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Receiving.Tests
{


    /// <summary>
    ///This is a test class for HomeController_Receiving and is intended
    ///to contain all HomeController_Receiving Unit Tests
    ///</summary>
    [TestClass()]
    public class HomeController_Receiving
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes

        HomeController _target;
        Mock<HttpResponseBase> _response;

        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Mapper.CreateMap<ProcessModel, ReceivingViewModel>();
            Mapper.CreateMap<ProcessModel, ProcessViewModel>();
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _response = new Mock<HttpResponseBase>(MockBehavior.Loose) { CallBase = false };
            var httpContext = new Mock<HttpContextBase>(MockBehavior.Loose) { CallBase = false };
            httpContext.SetupGet(r => r.Response).Returns(_response.Object);

            var principal = new Mock<IPrincipal>(MockBehavior.Loose);
            var identity = new Mock<IIdentity>(MockBehavior.Loose);
            principal.SetupGet(r => r.Identity).Returns(identity.Object);

            httpContext.SetupGet(r => r.User).Returns(principal.Object);

            var routeData = new RouteData();
            _target = new HomeController();
            var controllerContext = new ControllerContext(httpContext.Object, routeData, _target);
            _target.ControllerContext = controllerContext;

            Mapper.CreateMap<ProcessModel, ReceivingViewModel>();

        }

        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        /// Receiving action redirects if process id is not passed
        ///</summary>
        [TestMethod()]
        public void HomeController_Receiving_NullProcessId()
        {
            //HomeController target = new HomeController();

            // Act
            var actual = _target.Receiving(null);
            Assert.IsInstanceOfType(actual, typeof(RedirectToRouteResult));
            var rr = (RedirectToRouteResult)actual;
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Controller");
            Assert.AreEqual("Index", rr.RouteValues["action"], "Action");
            Assert.AreEqual("Receiving", rr.RouteValues["area"], "Area");
        }

        /// <summary>
        /// Receving action redirects if process id is invalid
        ///</summary>
        [Owner("Rajesh Kandari")]
        [TestMethod()]
        public void HomeController_Receiving_ProcessId()
        {
            int? processId = 65757348;
            ReceivingProcess pm = null;
            var service = new Mock<IReceivingService>(MockBehavior.Strict);
            _target.Service = service.Object;
            //service.Setup(p => p.GetProcessInfo(processId.Value)).Returns(pm);
            service.Setup(p => p.GetProcessInfo(processId.Value)).Returns(pm);
            var actual = _target.Receiving(processId);

            Assert.IsInstanceOfType(actual, typeof(RedirectToRouteResult));
            var rr = (RedirectToRouteResult)actual;
            Assert.IsNotNull(actual, "Process Id");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Controller");
            Assert.AreEqual("Index", rr.RouteValues["action"], "Action");
            Assert.AreEqual("Receiving", rr.RouteValues["area"], "Area");
        }


        /// <summary>
        // Receiving function with proper process id displays receiving view
        /// </summary>
        ///test is breaking as we can not mock the usermismatch propoerty
        [TestMethod()]
        public void HomeController_Receiving_ProcessId_Mapping()
        {
            var service = new Mock<IReceivingService>(MockBehavior.Strict);
            _target.Service = service.Object;

            var pm = new ReceivingProcess
            {
                Carrier = new Carrier
                {
                    CarrierId = "AWER",
                    Description = "VYTTU"
                },
                OperatorName = "Rajesh",
                ProcessId = 123,
                ProDate = DateTime.Now,
                ProNumber = "ASADD",
                StartDate = DateTime.Now
            };
            service.Setup(p => p.GetPalletsOfProcess(pm.ProcessId)).Returns(new List<Pallet>());
            service.Setup(p => p.GetProcessInfo(pm.ProcessId)).Returns(Mapper.Map <ReceivingProcess, ReceivingProcess>(pm));

            //act
            var actualActionResult = _target.Receiving(pm.ProcessId);

            //assert
            Assert.IsInstanceOfType(actualActionResult, typeof(ViewResult), "View Result");
            var vr = (ViewResult)actualActionResult;
            Assert.AreEqual("Receiving", vr.ViewName, "View name");
            var model = vr.Model;
            Assert.IsInstanceOfType(model, typeof(ReceivingViewModel));
            var rvm = (ReceivingViewModel)model;
            Assert.AreEqual(pm.Carrier.Description, rvm.CarrierDescription);
            Assert.AreEqual(pm.ProcessId, rvm.ProcessId.Value);
            Assert.AreEqual(pm.Carrier.CarrierId, rvm.CarrierId);
            Assert.AreEqual(pm.OperatorName, rvm.OperatorName);
            Assert.AreEqual(pm.ProDate, rvm.ProDate);
            Assert.AreEqual(pm.ProNumber, rvm.ProNumber);

        }


        /// <summary>
        /// CreateProcess action inserts a new process and redirects to Receving action with the newly created process id
        /// </summary>
        [TestMethod()]
        public void HomeController_CreateProcess_ValidProcessModel()
        {
            //arrange
            var service = new Mock<IReceivingService>(MockBehavior.Strict);
            _target.Service = service.Object;
            Mapper.CreateMap<ReceivingProcess, ReceivingProcessModel>();
            ReceivingProcess expectedProcessModel = new ReceivingProcess
            {
                OperatorName = "umesh",
                ProNumber = "PRO1",
                Carrier = new Carrier
                {
                    CarrierId="123343",
                    Description="ASD"
                },
                ProcessId=123,
                ProDate=DateTime.Now,
                StartDate=DateTime.Now
            };
            //int processid = 5;
            service.Setup(p => p.InsertProcess(It.IsAny<ReceivingProcess>()))
                .Callback((ReceivingProcess p) =>
                {
                    Assert.AreEqual(p.Carrier.CarrierId, expectedProcessModel.Carrier.CarrierId, "CarrierId");
                    Assert.AreEqual(p.Carrier.Description, expectedProcessModel.Carrier.Description, "Description");
                    Assert.AreEqual(p.OperatorName, expectedProcessModel.OperatorName, "OperatorName");
                    Assert.AreEqual(p.ProcessId, expectedProcessModel.ProcessId, "ProcessId");
                    Assert.AreEqual(p.ProDate, expectedProcessModel.ProDate, "ProDate");
                    Assert.AreEqual(p.StartDate, expectedProcessModel.StartDate, "StartDate");
                })
                .Returns(expectedProcessModel.ProcessId);


            //act
            var actionResult = _target.CreateProcess(Mapper.Map<ReceivingProcess, ReceivingProcessModel>(expectedProcessModel));

            // assert
            Assert.IsNotNull(actionResult, "Process Id");
            Assert.IsInstanceOfType(actionResult, typeof(RedirectToRouteResult), "Redirection Result");
            var rr = (RedirectToRouteResult)actionResult;
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Controller");
            Assert.AreEqual("Receiving", rr.RouteValues["action"], "Action");
            Assert.AreEqual("Receiving", rr.RouteValues["area"], "Area");
            Assert.AreEqual(expectedProcessModel.ProcessId, rr.RouteValues["processId"], "processId");
        }
    }
}
