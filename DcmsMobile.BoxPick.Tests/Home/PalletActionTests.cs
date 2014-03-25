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
    /// Tests all bad pallet scenarios. A bad pallet should never become the current pallet.
    /// </summary>
    /// <remarks>
    /// A pallet is bad if:
    /// 1) It does not begin P
    /// 2) It has no box to pick
    /// 3) The box to pick has no associated carton.
    /// 
    /// We only check for valid building cases. SessionExpiry tests check for bad building cases.
    /// </remarks>
    [TestClass]
    public class PalletActionTests
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
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        private ControllerTestEnvironment<HomeController> _env;
        private Mock<IBoxPickRepository> _db;
        
        string _fieldNameScannedPalletId;
        const string BUILDING_ID = "FDC";
        const string ACTION_NAME = "Pallet";

        const string GOOD_PALLET_ID = "P12343";
        const string BAD_PALLET_ID = "X12343";

        private Pallet CreatePallet()
        {
            return new Pallet
            {
                PalletId = "P12343",
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
                PickedBoxCount =2,
                BuildingId="FDC",
                DestinationArea = "ADR",
                PickModeText = "ADREPPWSS"
            };
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            _env = new ControllerTestEnvironment<HomeController>();
            _env.UserName = "Sharad";
            _env.Role = "DCMS8_BOXPICK";
            _env.Headers.Add(HttpRequestHeader.UserAgent, "windows ce");

            _db = new Mock<IBoxPickRepository>(MockBehavior.Strict);
            _env.Controller.Repository = _db.Object;

            var bvm = new BuildingViewModel(_env.Session);
            bvm.CurrentBuildingId = BUILDING_ID;

            _fieldNameScannedPalletId = ReflectionHelpers.FieldNameFor((PalletViewModel m) => m.ScannedPalletId);
        }

        /// <summary>
        /// Verify that the building in environment has not changed
        /// </summary>
        [TestCleanup]
        public void MyTestCleanup()
        {
            var bvm = new BuildingViewModel(_env.Session);
            //Assert.AreEqual(BUILDING_ID, bvm.CurrentBuildingId);
        }

        /// <summary>
        /// Pallet not passed. Same view should get redisplayed.
        /// </summary>
        [TestMethod]
        public void Get_AcceptPallet_NoPallet()
        {
            _env.RequestMethod = "GET";
            //Act And Assert
            //CommonErrorActAndAsserts(null);
            var vr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "AcceptPallet");
        }

        [TestMethod]
        public void Post_AcceptPallet_NoPallet()
        {
            _env.RequestMethod = "POST";

            //Act And Assert
            //CommonErrorActAndAsserts(null);
            var vr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "AcceptPallet");
        }

        /// <summary>
        /// Get behaves like POST with empty pallet scanned. The existing current pallet should get cleared.
        /// </summary>
        [TestMethod]
        public void Get_AcceptPallet_NoPallet_PopulatedSession()
        {
            _env.RequestMethod = "GET";

            var mm = new MasterModelWithPallet(_env.Session);
            mm.Map(CreatePallet());

            //Act And Assert
            //CommonErrorActAndAsserts(null);
            var vr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "AcceptPallet");

        }

        [TestMethod]
        public void Post_AcceptPallet_NoPallet_PopulatedSession()
        {
            _env.RequestMethod = "POST";
            var mm = new MasterModelWithPallet(_env.Session);
            mm.Map(CreatePallet());

            //Act And Assert
            //CommonErrorActAndAsserts(null);

            var vr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "AcceptPallet");
        }

        /// <summary>
        /// Pass pallet which does not begin with a P. The same view should get redisplayed
        /// </summary>
        [TestMethod]
        public void Get_AcceptPallet_Pallet_BadPattern()
        {
            // Arrange
            _env.RequestMethod = "GET";
            _env.QueryString[_fieldNameScannedPalletId] = BAD_PALLET_ID;

            //Act And Assert
            CommonErrorActAndAsserts(BAD_PALLET_ID);
        }

        [TestMethod]
        public void Post_AcceptPallet_Pallet_BadPattern()
        {
            // Arrange
            _env.RequestMethod = "POST";
            _env.Form[_fieldNameScannedPalletId] = BAD_PALLET_ID;

            //Act And Assert
            CommonErrorActAndAsserts(BAD_PALLET_ID);
        }

        /// <summary>
        /// Pass pallet which begins a P but is not in DB. The same view should get redisplayed
        /// </summary>
        [TestMethod]
        public void Get_AcceptPallet_Pallet_NotInDb()
        {
            // Arrange
            _env.RequestMethod = "GET";
            _env.QueryString[_fieldNameScannedPalletId] = GOOD_PALLET_ID;
            _db.Setup(p => p.RetrievePalletInfo(GOOD_PALLET_ID)).Returns<Pallet>(null);

            //Act And Assert
            CommonErrorActAndAssertsWhenPalletIsNull(GOOD_PALLET_ID);
            _db.Verify(p => p.RetrievePalletInfo(GOOD_PALLET_ID), Times.Once());
        } 

        [TestMethod]
        public void Post_AcceptPallet_Pallet_NotInDb()
        {
            // Arrange
            _env.RequestMethod = "POST";
            _env.Form[_fieldNameScannedPalletId] = GOOD_PALLET_ID;
            _db.Setup(p => p.RetrievePalletInfo(GOOD_PALLET_ID)).Returns<Pallet>(null);

            //Act And Assert
            CommonErrorActAndAssertsWhenPalletIsNull(GOOD_PALLET_ID);
            _db.Verify(p => p.RetrievePalletInfo(GOOD_PALLET_ID), Times.Once());
        }



        /// <summary>
        /// Pass pallet which begins a P. The pallet is in DB but is not pickable. The same view should get redisplayed
        /// </summary>
        [TestMethod]
        public void Get_AcceptPallet_Pallet_BadInDb()
        {
            // Arrange
            _env.RequestMethod = "GET";

            //Making pallet bad 
            var pallet = CreatePallet();
            pallet.BoxToPick = null;
            _env.QueryString[_fieldNameScannedPalletId] = GOOD_PALLET_ID;
            _db.Setup(p => p.RetrievePalletInfo(GOOD_PALLET_ID)).Returns(pallet);

            //Act And Assert
            CommonErrorActAndAsserts(GOOD_PALLET_ID);
            _db.Verify(p => p.RetrievePalletInfo(GOOD_PALLET_ID), Times.Once());
        }

        [TestMethod]
        public void Post_AcceptPallet_Pallet_BadInDb()
        {
            // Arrange
            _env.RequestMethod = "POST";

            //Making pallet bad 
            var pallet = CreatePallet();
            pallet.BoxToPick = null;
            _env.Form[_fieldNameScannedPalletId] = GOOD_PALLET_ID;
            _db.Setup(p => p.RetrievePalletInfo(GOOD_PALLET_ID)).Returns(pallet);

            //Act And Assert
            CommonErrorActAndAsserts(GOOD_PALLET_ID);
            _db.Verify(p => p.RetrievePalletInfo(GOOD_PALLET_ID), Times.Once());
        }

        private void CommonErrorActAndAsserts(string palletId)
        {
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptPallet");
            Assert.AreEqual("Pallet", vr.ViewName, "Pallet view should be redisplayed");

            var model = (PalletViewModel)vr.Model;
            Assert.IsNull(model.LastCartonId);
            Assert.IsNull(model.LastLocation);
            Assert.IsNull(model.LastUccId);

            Assert.AreEqual(palletId, model.ScannedPalletId);
            //Assert.AreEqual(palletId, model.LastScan, "Last scan must be same as the passed pallet");

            if (string.IsNullOrEmpty(palletId))
            {
                Assert.IsTrue(_env.Controller.ModelState.IsValid, "Empty pallet id should not invalidate model");
            }
            else
            {
                Assert.IsFalse(_env.Controller.ModelState.IsValid);
                Assert.IsFalse(_env.Controller.ModelState.IsValidField(_fieldNameScannedPalletId), "Field invalid");
            }

            Assert.AreEqual(BUILDING_ID, model.CurrentBuildingId);
            Assert.AreEqual('\0', model.Sound);

            var mmp = new MasterModelWithPallet(_env.Session);
            Assert.IsNull(mmp.CurrentPalletId, "Invalid pallet should never be current pallet");
        }

        private void CommonErrorActAndAssertsWhenPalletIsNull(string palletId)
        {
            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "AcceptPallet");
            
            //Assert
            Assert.IsNotNull(rr);
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "Because the scanned pallet id is empty");
            Assert.AreEqual("ADRPallet", rr.RouteValues["action"], "Redirection to Carton Action expected");
            Assert.AreEqual("Confirm", rr.RouteValues["controller"], "Redirection to Home controller expected");

            var model = new MasterModel(_env.Session);
            Assert.IsNull(model.LastCartonId);
            Assert.IsNull(model.LastLocation);
            Assert.IsNull(model.LastUccId);
            Assert.AreEqual(BUILDING_ID, model.CurrentBuildingId);
            Assert.AreEqual('\0', model.Sound);

            var mmp = new MasterModelWithPallet(_env.Session);
            Assert.IsNull(mmp.CurrentPalletId, "Invalid pallet should never be current pallet");


            if (string.IsNullOrEmpty(palletId))
            {
                Assert.IsTrue(_env.Controller.ModelState.IsValid, "Empty pallet id should not invalidate model");
            }
            else
            {
                Assert.IsTrue(_env.Controller.ModelState.IsValid, "Pallet doesn't exists is not an error");
                Assert.IsTrue(_env.Controller.ModelState.IsValidField(_fieldNameScannedPalletId), "Field should not be invalid");
            }
        }

        /// <summary>
        /// Pass pallet which begins a P. The pallet is in DB and is pickable. The carton view should get displayed
        /// </summary>
        [TestMethod]
        [TestCategory("Primary Path")]
        public void Get_AcceptPallet_Pallet_GoodInDb()
        {
            // Arrange
            _env.RequestMethod = "GET";
            _env.QueryString[_fieldNameScannedPalletId] = GOOD_PALLET_ID;
            CommonSuccessArrangeActAndAsserts(GOOD_PALLET_ID);
        }

        [TestMethod]
        [TestCategory("Primary Path")]
        public void Post_AcceptPallet_Pallet_GoodInDb()
        {
            // Arrange
            _env.RequestMethod = "POST";
            _env.Form[_fieldNameScannedPalletId] = GOOD_PALLET_ID;
            CommonSuccessArrangeActAndAsserts(GOOD_PALLET_ID);
        }

        private void CommonSuccessArrangeActAndAsserts(string palletId)
        {
            var pallet = CreatePallet();
            _db.Setup(p => p.RetrievePalletInfo(palletId)).Returns(pallet);

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptPallet");
            _db.Verify(p => p.RetrievePalletInfo(palletId), Times.Once());
            Assert.AreEqual("Carton", vr.ViewName, "Same view should be redisplayed");

            var model = (CartonViewModel)vr.Model;
            Assert.IsTrue(_env.Controller.ModelState.IsValid);
            Assert.AreEqual(palletId, model.CurrentPalletId);
            //Assert.AreEqual(palletId, model.LastScan);
            Assert.AreEqual('\0', model.Sound);
            Assert.IsTrue(_env.Controller.ModelState.IsValidField(_fieldNameScannedPalletId), "pallet id meet pattern");
            Assert.IsNull(model.LastCartonId);
            Assert.IsNull(model.LastLocation);
            Assert.IsNull(model.LastUccId);
            Helpers.AssertPalletMapping(model, pallet);
        }
    }
}
