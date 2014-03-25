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
    /// Call SkipUcc action using GET and POST in confirm controller.
    /// Error case: Redirected to carton View
    /// 1. When invalid pattern for ConfirmScan is scanned
    /// 2. When ConfirmScan with valid pattern but other that UccIdToPick is scanned
    /// Success case: Redirected to pallet View
    /// 1. When UccIdToPick matches ConfirmScan 
    /// </summary>
    [TestClass]
    public class SkipUccActionTests
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

        const string ACTION_NAME = "SkipUcc";

        static Pallet __pallet;
        static string __fieldNameConfirmScan;

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            __fieldNameConfirmScan = ReflectionHelpers.FieldNameFor((SkipUccViewModel m) => m.ConfirmScan);
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
                PickModeText="ADREPPWSS"
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
        /// Prompts for confirming ucc view after redirection
        /// </summary>
        [TestMethod]
        public void SkipUcc_After_Redirection()
        {
            //Arrange
            _env.RequestMethod = "GET";

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "StartSkipUcc");

            //Arrange
            Assert.AreEqual("", vr.ViewName, "SkipUcc view to be displayed");
            Assert.IsInstanceOfType(vr.Model, typeof(SkipUccViewModel), "Unexpected model type");
            Assert.IsTrue(vr.ViewData.ModelState.IsValid, "Confirm scan does not invalidate model state");
        }

        /// <summary>
        /// Empty Scan, Redirect to Carton view it is assumed that user wants to cancel skipping
        /// </summary>
        [TestMethod]
        public void SkipUcc_Empty_Scan()
        {
            //Arrange
            _env.RequestMethod = "POST";

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "SkipUcc");

            //Assert
            Assert.IsNotNull(rr);
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "Because the confirm scan is empty");
            Assert.AreEqual("Carton", rr.RouteValues["action"], "Redirection to Carton Action expected");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Redirection to Home controller expected");
            //_env.Controller.TempData.AddErrorsToModelState(_env.Controller.ViewData.ModelState);
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "ModelState should remain valid because no errors expected in TempData");
        }

        /// <summary>
        /// Bad UCC, Invalid pattern
        /// </summary>
        [TestMethod]
        public void SkipUcc_Invalid_Pattern_Ucc()
        {
            //Arrange
            _env.RequestMethod = "POST";
            _env.Form[__fieldNameConfirmScan] = "ABC121212";

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "SkipUcc");

            //Assert
            Assert.IsNotNull(rr);
            Assert.IsFalse(_env.Controller.ViewData.ModelState.IsValid, "Because the confirm scan is invalid");
            Assert.AreEqual("Carton", rr.RouteValues["action"], "Redirection to Carton Action expected");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Redirection to Home controller expected");
            //_env.Controller.TempData.AddErrorsToModelState(_env.Controller.ViewData.ModelState);
            Assert.IsFalse(_env.Controller.ViewData.ModelState.IsValid, "ModelState should become invalid because errors expected in TempData");
        }

        /// <summary>
        /// Bad UCC, Valid pattern but Ucc is different that UccToPick
        /// </summary>
        [TestMethod]
        public void SkipUcc_Valid_Pattern_Different_Ucc()
        {
            //Arrange
            _env.RequestMethod = "POST";
            _env.Form[__fieldNameConfirmScan] = "00001234567890129999";

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "SkipUcc");

            //Assert
            Assert.IsNotNull(rr);
            Assert.IsFalse(_env.Controller.ViewData.ModelState.IsValid, "Because the confirm scan is invalid");
            Assert.AreEqual("Carton", rr.RouteValues["action"], "Redirection to Carton Action expected");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Redirection to Home controller expected");
            //_env.Controller.TempData.AddErrorsToModelState(_env.Controller.ViewData.ModelState);
            Assert.IsFalse(_env.Controller.ViewData.ModelState.IsValid, "ModelState should become invalid because errors expected in TempData");
        }

        /// <summary>
        /// Bad UCC, Valid pattern but Ucc is different that UccToPick
        /// </summary>
        [TestMethod]
        public void SkipUcc_GoodUcc_GoodPallet()
        {
            //Arrange
            _env.RequestMethod = "POST";
            _env.Form[__fieldNameConfirmScan] = __pallet.BoxToPick.UccId;

            _db.Setup(p => p.RemoveBoxFromPallet(__pallet.BoxToPick.UccId, __pallet.PalletId));
            _db.Setup(p => p.RetrievePalletInfo(__pallet.PalletId)).Returns(__pallet);

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "SkipUcc");

            //Assert
            _db.Verify(p => p.RemoveBoxFromPallet(__pallet.BoxToPick.UccId, __pallet.PalletId), Times.Once());
            _db.Verify(p => p.RetrievePalletInfo(__pallet.PalletId), Times.Once());

            Assert.IsNotNull(rr);
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "Because the confirm scan is valid");
            Assert.AreEqual("Carton", rr.RouteValues["action"], "Redirection to Carton Action expected");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Redirection to Home controller expected");
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "ModelState should become invalid because errors expected in TempData");
        }

        /// <summary>
        /// Bad UCC, Valid pattern but Ucc is different that UccToPick
        /// </summary>
        [TestMethod]
        public void SkipUcc_GoodUcc_BadPallet()
        {
            //Arrange
            _env.RequestMethod = "POST";
            _env.Form[__fieldNameConfirmScan] = __pallet.BoxToPick.UccId;

            _db.Setup(p => p.RemoveBoxFromPallet(__pallet.BoxToPick.UccId, __pallet.PalletId));

            var pallet = new Pallet
            {
                // Incomplete pallet
                PalletId = "P12345",
                QueryTime = DateTime.Now,
                TotalBoxCount = 5,
                PickedBoxCount =2
            };

            _db.Setup(p => p.RetrievePalletInfo(__pallet.PalletId)).Returns(pallet);
            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "SkipUcc");

            //Assert

            _db.Verify(p => p.RemoveBoxFromPallet(__pallet.BoxToPick.UccId, __pallet.PalletId), Times.Once());
            _db.Verify(p => p.RetrievePalletInfo(__pallet.PalletId), Times.Once());
            Assert.IsNotNull(rr);
            Assert.IsFalse(_env.Controller.ViewData.ModelState.IsValid, "ModelState must be invalid");
            Assert.AreEqual("Pallet", rr.RouteValues["action"], "Redirection to Carton Action expected");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Redirection to Home controller expected");
            Assert.IsFalse(_env.Controller.ViewData.ModelState.IsValid, "ModelState should become invalid because errors expected in TempData");
        }
    }
}
