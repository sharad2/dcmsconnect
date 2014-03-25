using System;
using System.Net;
using System.Web.Mvc;
using DcmsMobile.BoxPick.Areas.BoxPick.Controllers;
using DcmsMobile.BoxPick.Models;
using DcmsMobile.BoxPick.Repositories;
using DcmsMobile.BoxPick.Tests.Fakes;
using DcmsMobile.BoxPick.ViewModels;
using EclipseLibrary.Mvc.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DcmsMobile.BoxPick.Tests.Confirm
{
    /// <summary>
    /// Call PartialPickPallet action using GET and POST in confirm controller.
    /// Error case: Redirected to carton View
    /// 1. When scan for ConfirmPalletid does not match MasterModelWithPallet.CurrentPalletId
    /// Success case: Redirected to pallet View
    /// 1. When CurrentPalletId matches ConfirmPalletId 
    /// </summary>
    [TestClass]
    public class PartialPickPalletTests
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

        private ControllerTestEnvironment<ConfirmController> _env;
        private Mock<IBoxPickRepository> _db;

        const string ACTION_NAME = "PartialPickPallet";

        static Pallet __pallet;
        static string __fieldNameConfirmPalletId;

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            __fieldNameConfirmPalletId = ReflectionHelpers.FieldNameFor((PartialPickPalletViewModel m) => m.ConfirmPalletId);
            __pallet = new Pallet
            {
                PalletId = "P12345",
                QueryTime = DateTime.Now,
                BoxToPick = new Box
                {
                    UccId = "00001234567890123456",
                    SkuInBox = new Sku
                    {
                        SkuId = 123
                    },
                    Pieces = 6,
                    QualityCode = "01",
                    VwhId = "C15",
                    AssociatedCarton = new Carton
                    {
                        CartonId = "VCJ00441671",
                        LocationId = "FFDC282008"
                    }
                },
                CartonSourceArea = "BIR",
                TotalBoxCount = 5,
                PickedBoxCount = 2,
                PickModeText = "ADREPPWSS"
            };
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            _env = new ControllerTestEnvironment<ConfirmController>();
            _env.Headers.Add(HttpRequestHeader.UserAgent, "windows ce");
            _db = new Mock<IBoxPickRepository>(MockBehavior.Strict);
            _env.Controller.Repository = _db.Object;

            _env.UserName = "Sharad";
            _env.Role = "DCMS8_BOXPICK";

            var mm = new MasterModelWithPallet(_env.Session);
            mm.Map(__pallet);
        }


        /// <summary>
        /// Prompts for confirming pallet view after redirection
        /// </summary>
        [TestMethod]
        public void PartialPickPallet_After_Redirection()
        {
            //Arrange
            _env.RequestMethod = "GET";

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "PartialPickPallet");
            
            //Arrange
            Assert.AreEqual("", vr.ViewName, "PartialPickPallet view to be displayed");
            Assert.IsInstanceOfType(vr.Model, typeof(PartialPickPalletViewModel), "Unexpected model type");
            Assert.IsTrue(vr.ViewData.ModelState.IsValid, "No pallet does not invalidate model state");
        }

        /// <summary>
        /// Empty Scan, Redirect to Carton view it is assumed that user wants to cancel partial picking
        /// </summary>
        [TestMethod]
        public void PartialPickPallet_Empty_Scan()
        {
            //Arrange
            _env.RequestMethod = "POST";

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "PartialPickPalletConfirm");
            
            //Assert
            Assert.IsNotNull(rr);
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "Because the scanned pallet id is empty");
            Assert.AreEqual("Carton", rr.RouteValues["action"], "Redirection to Carton Action expected");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Redirection to Home controller expected");
            //_env.Controller.TempData.AddErrorsToModelState(_env.Controller.ViewData.ModelState);
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "ModelState should remain valid because no errors expected in TempData");
        }


        /// <summary>
        /// Invalid pattern scanned in confirmation
        /// </summary>
        [TestMethod]
        public void PartialPickPallet_Invalid_Pattern_Scan()
        {
            AssertConfirmScanDoesNotMatch("X98765");
        }

        /// <summary>
        /// Valid pattern but different pallet id scanned
        /// </summary>
        [TestMethod]
        public void PartialPickPallet_Invalid_Pallet_Scan()
        {
            //Arrange
            AssertConfirmScanDoesNotMatch("P98765");
        }

        private void AssertConfirmScanDoesNotMatch(string confirmScan)
        {
            _env.RequestMethod = "POST";
            _env.Form[__fieldNameConfirmPalletId] = confirmScan;

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "PartialPickPalletConfirm");

            //Assert
            Assert.IsNotNull(rr);
            Assert.IsFalse(_env.Controller.ViewData.ModelState.IsValid, "Because the scanned pallet id is invalid");
            Assert.AreEqual("Carton", rr.RouteValues["action"], "Redirection to Carton Action expected");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Redirection to Home controller expected");
            //_env.Controller.TempData.AddErrorsToModelState(_env.Controller.ViewData.ModelState);
            Assert.IsFalse(_env.Controller.ViewData.ModelState.IsValid, "ModelState should become invalid because errors expected in TempData");
        }

        /// <summary>
        /// Prompts for confirming pallet view after redirection
        /// </summary>
        [TestMethod]
        public void PartialPickPallet_Valid_Pallet_Scan()
        {
            //Arrange
            _env.RequestMethod = "POST";
            _env.Form[__fieldNameConfirmPalletId] = __pallet.PalletId;

            _db.Setup(p => p.RemoveRemainingBoxesFromPallet(__pallet.PalletId)).Returns(1);

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "PartialPickPalletConfirm");

            //Assert
            _db.Verify(p => p.RemoveRemainingBoxesFromPallet(__pallet.PalletId), Times.Once());

            Assert.IsNotNull(rr);
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "Because the scanned pallet matched current pallet");
            Assert.AreEqual("Pallet", rr.RouteValues["action"], "Redirection to Pallet Action expected");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Redirection to Home controller expected");
            //_env.Controller.TempData.AddErrorsToModelState(_env.Controller.ViewData.ModelState);
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "ModelState should remain valid because no errors expected in TempData");
        }
    }
}
