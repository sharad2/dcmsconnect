using System;
using System.Net;
using System.Web.Mvc;
using DcmsMobile.BoxPick.Models;
using DcmsMobile.BoxPick.Repositories;
using DcmsMobile.BoxPick.Tests.Fakes;
using DcmsMobile.BoxPick.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EclipseLibrary.Mvc.Helpers;
using DcmsMobile.BoxPick.Areas.BoxPick.Controllers;

namespace DcmsMobile.BoxPick.Tests.Home
{
    /// <summary>
    /// Creates a valid input environment for the Carton Actions. Posts carton, pallet and UCC to the carton action and asserts results.
    /// Failure cases involve the carton not qualifying for substitution.
    /// TODO: Pallet scanned tests.
    /// </summary>
    [TestClass]
    public class CartonActionTests
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

        private ControllerTestEnvironment<HomeController> _env;
        private Mock<IBoxPickRepository> _db;

        const string ACTION_NAME = "Carton";
        const string CARTON_ID = "12345";

        static string _fieldNameScannedCartonId;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _env = new ControllerTestEnvironment<HomeController>();
            _env.Headers.Add(HttpRequestHeader.UserAgent, "windows ce");
            _db = new Mock<IBoxPickRepository>(MockBehavior.Strict);
            _env.Controller.Repository = _db.Object;

            _env.UserName = "Sharad";
            _env.Role = "DCMS8_BOXPICK";
            _env.RequestMethod = "POST";

            var mm = new MasterModelWithPallet(_env.Session);
            mm.Map(CreatePallet());

            _fieldNameScannedCartonId = ReflectionHelpers.FieldNameFor((CartonViewModel m) => m.ScannedCartonId);
        }

        /// <summary>
        /// The pallet information must stay in session
        /// </summary>
        [TestCleanup]
        public void MyTestCleanup()
        {
            var mm = new MasterModelWithPallet(_env.Session);
            var pallet = CreatePallet();
            Assert.AreEqual(pallet.PickableBoxCount, mm.PalletPickableBoxCount);
            Assert.AreEqual(pallet.PickedBoxCount, mm.PalletPickedBoxCount);
            Assert.AreEqual(pallet.TotalBoxCount, mm.PalletTotalBoxCount);
            Assert.AreEqual(pallet.CartonSourceArea, mm.CartonSourceAreaToPick);
        }


        private Pallet CreatePallet()
        {
            return new Pallet
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
                    VwhId = "C15"
                },
                CartonSourceArea = "BIR",
                TotalBoxCount = 5,
                PickedBoxCount = 2,
                DestinationArea = "ADR",
                BuildingId = "FDC",
                PickModeText = "ADREPPWSS"
            };
        }

        private Carton CreateCarton()
        {
            var pallet = CreatePallet();

            var carton = new Carton
            {
                CartonId = "12345",
                SkuInCarton = new Sku
                {
                    SkuId = pallet.BoxToPick.SkuInBox.SkuId
                },
                Pieces = pallet.BoxToPick.Pieces,
                QualityCode = pallet.BoxToPick.QualityCode,
                VwhId = pallet.BoxToPick.VwhId,
                StorageArea = pallet.CartonSourceArea
            };
            return carton;
        }

        /// <summary>
        /// Calling Action via Get. The current carton should get cleared.
        /// </summary>
        [TestMethod]
        public void AcceptCarton_Get_ValidCurrentCarton()
        {
            var mm = new MasterModel(_env.Session);
           
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptCarton");

            //Assert
            AssertErrorInput(vr, null);
        }

        /// <summary>
        /// Calling Action via Get. The current carton should get cleared.
        /// </summary>
        [TestMethod]
        public void AcceptCarton_Get_CartonInQueryString()
        {
            _env.QueryString[_fieldNameScannedCartonId] = CARTON_ID;
            _db.Setup(p => p.GetCartonDetails(CARTON_ID)).Returns((Carton)null);

            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptCarton");

            //Assert
            AssertErrorInput(vr, CARTON_ID);
        }
        /// <summary>
        /// Nothing posted. Should redisplay carton view.
        /// </summary>
        [TestMethod]
        public void AcceptCarton_NoCarton()
        {
            // Arrange

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptCarton");

            //Assert
            AssertErrorInput(vr, null);
        }

        private void AssertErrorInput(ViewResult vr, string postedCartonId)
        {
            var pallet = CreatePallet();

            Assert.AreEqual("Carton", vr.ViewName, "Redisplay same view");
            Assert.IsInstanceOfType(vr.Model, typeof(CartonViewModel), "Unexpected model type");
            if (string.IsNullOrEmpty(postedCartonId))
            {
                Assert.IsTrue(vr.ViewData.ModelState.IsValid, "No carton does not invalidate model state");
            }
            else
            {
                Assert.IsFalse(vr.ViewData.ModelState.IsValid, "Bad carton should invalidate model state");
                Assert.IsFalse(vr.ViewData.ModelState.IsValidField(_fieldNameScannedCartonId), "Invalid model state because carton was not in database");
            }
            var model = (CartonViewModel)vr.Model;
            Assert.IsNotNull(model, "Model should never be null");
            Assert.IsNull(model.LastCartonId, "LastCartonId should be null: No carton has been scanned so far");
            Assert.IsNull(model.CartonIdToPick, "CartonIdToPick should be null: No carton has been scanned so far");

            Assert.AreEqual(pallet.PalletId, model.CurrentPalletId, "Current Pallet must be same as passed");
            Assert.AreEqual(pallet.CartonSourceArea, model.CartonSourceAreaToPick);
            Assert.IsNull(model.LastLocation);
            Assert.IsNull(model.LastUccId);
        }

        /// <summary>
        /// Carton posted is not in database. Reprompts for carton
        /// </summary>
        [TestMethod]
        public void AcceptCarton_CartonNotInDb()
        {
            // Arrange
            _env.Form[_fieldNameScannedCartonId] = CARTON_ID;

            _db.Setup(p => p.GetCartonDetails(CARTON_ID)).Returns((Carton)null);

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptCarton");

            //Assert
            _db.Verify(p => p.GetCartonDetails(CARTON_ID), Times.Once());
            AssertErrorInput(vr, CARTON_ID);
        }

        /// <summary>
        /// The carton is in DB but is not substitutable for the carton we are looking for
        /// </summary>
        [TestMethod]
        public void AcceptCarton_BadCartonInDb()
        {
            // Arrange
            _env.Form[_fieldNameScannedCartonId] = CARTON_ID;

            //Making carton invalid
            var carton = CreateCarton();
            carton.VwhId = null;

            _db.Setup(p => p.GetCartonDetails(CARTON_ID)).Returns(carton);

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptCarton");

            //Assert
            _db.Verify(p => p.GetCartonDetails(CARTON_ID), Times.Once());
            AssertErrorInput(vr, CARTON_ID);
        }

 

        /// <summary>
        /// The carton is in DB but and is substitutable for the carton we are looking for
        /// </summary>
        [TestMethod]
        [TestCategory("Primary Path")]
        public void Carton_GoodCartonInDb_Success()
        {
            // Arrange
            _env.Form[_fieldNameScannedCartonId] = CARTON_ID;
            var pallet = CreatePallet();

            _db.Setup(p => p.GetCartonDetails(CARTON_ID)).Returns(CreateCarton());

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptCarton");

            //Assert
            _db.Verify(p => p.GetCartonDetails(CARTON_ID), Times.Once());
            Assert.AreEqual("Ucc", vr.ViewName, "Redisplay same view");
            Assert.IsInstanceOfType(vr.Model, typeof(UccViewModel), "Unexpected model type");
            Assert.IsTrue(vr.ViewData.ModelState.IsValid, "Valid model state because carton qualified");
            var model = (UccViewModel)vr.Model;
            Assert.IsNotNull(model, "Model should never be null");
            Assert.AreEqual(CARTON_ID, model.LastCartonId, "LastCartonId must match the scanned carton");
            Assert.IsNull(model.CartonIdToPick, "CartonIdToPick should be null: No carton has been scanned so far");
            Assert.AreEqual(pallet.PalletId, model.CurrentPalletId, "Current Pallet must be same as passed");
            Assert.AreEqual(pallet.CartonSourceArea, model.CartonSourceAreaToPick);
            Assert.IsNull(model.LastLocation);
            Assert.IsNull(model.LastUccId);
        }

        /// <summary>
        /// UCC scanned instead of carton. The UCC scanned matches the UCC Id of box to pick.
        /// Should redirect to confirmation scan view.
        /// </summary>
        [TestMethod]
        public void Carton_SkippableUcc_Scanned()
        {
            // Arrange
            _env.Form[_fieldNameScannedCartonId] = CreatePallet().BoxToPick.UccId;

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "AcceptUccInCarton");

            //Assert
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "Because the scanned UCC matched current UCC");
            Assert.AreEqual("SkipUcc", rr.RouteValues["action"], "Redirection to SkipUcc Action expected");
            Assert.AreEqual("Confirm", rr.RouteValues["controller"], "Redirection to Confirm controller expected");
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "ModelState should remain valid because no errors expected in TempData");
        }

        /// <summary>
        /// The UCC scanned does not match the UCC Id of box to pick
        /// </summary>
        [TestMethod]
        public void Carton_UnskippableUcc_Scanned()
        {
            // Arrange
            const string BAD_UCC_ID = "00001234567890123400";
            _env.Form[_fieldNameScannedCartonId] = BAD_UCC_ID;

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptUccInCarton");

            //Assert
            AssertErrorInput(vr, BAD_UCC_ID);
        }

        /// <summary>
        /// User trying to start a new pallet. We refuse.
        /// </summary>
        [TestMethod]
        public void Carton_NonCurrent_Pallet_Scanned()
        {
            // Arrange
            const string BAD_PALLET_ID = "P895656";
            _env.Form[_fieldNameScannedCartonId] = BAD_PALLET_ID;

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptPalletInCarton");

            //Assert
            AssertErrorInput(vr, BAD_PALLET_ID);
        }

        /// <summary>
        /// Verify redirection to confirmation scan view
        /// </summary>
        [TestMethod]
        public void Carton_Current_Pallet_Scanned()
        {
            // Arrange
            _env.Form[_fieldNameScannedCartonId] = CreatePallet().PalletId;

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "AcceptPalletInCarton");

            //Assert
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "Because the scanned pallet matched current pallet");
            Assert.AreEqual("PartialPickPallet", rr.RouteValues["action"], "Redirection to PartialPickPallet Action expected");
            Assert.AreEqual("Confirm", rr.RouteValues["controller"], "Redirection to Confirm controller expected");
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "ModelState should remain valid because no errors expected in TempData");
        }
    }
}
