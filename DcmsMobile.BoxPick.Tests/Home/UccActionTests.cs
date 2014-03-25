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
    /// The only success scan is the UCC to pick in the master model.
    /// Failure Cases: Any other valid pattern UCC.
    /// Bad Pattern UCC: Presume previous is being ignored and this becomes new current carton.
    /// Pallet Pattern scan: Offer to partially pick pallet.
    /// </summary>
    [TestClass]
    public class UccActionTests
    {
        #region Initialization
        private ControllerTestEnvironment<HomeController> _env;
        private Mock<IBoxPickRepository> _db;
        const string ACTION_NAME = "Ucc";
        const string CARTON_ID = "000245";
        string _fieldNameScannedUccId;

        /// <summary>
        /// Minimally valid pallet must exist in environment. The LastCartonId is set to indicate which carton was scanned.
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _env = new ControllerTestEnvironment<HomeController>();
            _env.Headers.Add(HttpRequestHeader.UserAgent, "windows ce");
            _db = new Mock<IBoxPickRepository>(MockBehavior.Strict);
            _env.Controller.Repository = _db.Object;

            _env.UserName = "Deepak";
            _env.Role = "DCMS8_BOXPICK";
            _env.RequestMethod = "POST";

            var mm = new MasterModelWithPallet(_env.Session);
            mm.SetLastCartonAndLocation("000245", null);
            _palletBeforePick = CreatePallet();
            mm.Map(_palletBeforePick);
            _fieldNameScannedUccId = ReflectionHelpers.FieldNameFor((UccViewModel m) => m.ScannedUccId);
        }

        Pallet _palletBeforePick;

        /// <summary>
        /// Creates a minimally valid pallet
        /// </summary>
        /// <returns></returns>
        private Pallet CreatePallet()
        {
            var rand = new Random();
            var __pallet = new Pallet
            {
                PalletId = "P12345",
                QueryTime = DateTime.Now - TimeSpan.FromMinutes(rand.Next(30)),
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
                PickModeText = "ADREPPWSS"
            };
            return __pallet;
        }

        private void AssertPickablePallet(ViewResult vr, Pallet palletAfterQuery)
        {
            Assert.AreEqual("Carton", vr.ViewName);
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(CartonViewModel));
            var mm = (CartonViewModel)vr.Model;
            Helpers.AssertPalletMapping(mm, palletAfterQuery);
        }

        private void AssertFullPallet(Pallet palletAfterQuery)
        {
            var mm = new MasterModelWithPallet(_env.Session);
            Helpers.AssertPalletMapping(mm, palletAfterQuery);
        }

        #endregion

        #region Ucc Scan

        [TestMethod]
        [TestCategory("Primary Path")]
        public void Post_Ucc_Pallet_StillPickable()
        {
            // Arrange
            
            _env.Form[_fieldNameScannedUccId] = _palletBeforePick.BoxToPick.UccId;
            _db.Setup(p => p.PickCarton(_palletBeforePick.BoxToPick.UccId, CARTON_ID, _palletBeforePick.QueryTime.Value));

            var palletAfterQuery = CreatePallet();
            palletAfterQuery.BoxToPick.UccId = "00001234567890654321";
            _db.Setup(p => p.RetrievePalletInfo(_palletBeforePick.PalletId)).Returns(palletAfterQuery);

            // Act
            var vr = _env.InvokeAction<ViewResult>("Ucc", "AcceptUcc");
            AssertPickablePallet(vr, palletAfterQuery);
        }

        [TestMethod]
        [TestCategory("Primary Path")]
        public void Post_Ucc_Pallet_FullyPicked()
        {
            // Arrange
            _env.Form[_fieldNameScannedUccId] = _palletBeforePick.BoxToPick.UccId;

            var palletAfterQuery = CreatePallet();
            palletAfterQuery.BoxToPick = null;

            _db.Setup(p => p.PickCarton(_palletBeforePick.BoxToPick.UccId, CARTON_ID, _palletBeforePick.QueryTime.Value));
            _db.Setup(p => p.RetrievePalletInfo(_palletBeforePick.PalletId)).Returns(palletAfterQuery);

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>("Ucc", "AcceptUcc");
            Assert.AreEqual("Pallet", rr.RouteValues["action"], "Must reprompt for pallet");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Must redirect to pallet in home controller");
            AssertFullPallet(null);
        }


        /// <summary>
        /// Valid pattern but different UCC is scanned.
        /// </summary>
        [TestMethod]
        public void Post_Ucc_Same_Pattern_Ucc()
        {
            string BADUCC = "00001234567890123003";
            _env.Form[_fieldNameScannedUccId] = BADUCC;

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptUcc");

            //Assert
            Assert.AreEqual("Ucc", vr.ViewName, "Redisplay same view");
            Assert.IsInstanceOfType(vr.Model, typeof(UccViewModel), "Unexpected model type");
            Assert.IsFalse(vr.ViewData.ModelState.IsValid, "Bad Ucc");
            var model = (UccViewModel)vr.Model;
            Assert.IsNotNull(model, "Model should never be null");
            Helpers.AssertPalletMapping(model, _palletBeforePick);
        }

        #endregion

        #region Presumed Carton Scan

        /// <summary>
        /// Nothing posted. Should redisplay UCC view. 
        /// UCC Pattern is bad. System should presume that this is a carton scan so AcceptCartonInUcc method is invoked
        /// </summary>
        [TestMethod]
        public void AcceptCartonInUcc_EmptyScan()
        {
            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptCartonInUcc");

            //Assert
            Assert.AreEqual("Ucc", vr.ViewName, "Redisplay same view");
            Assert.IsInstanceOfType(vr.Model, typeof(UccViewModel), "Unexpected model type");
            Assert.IsTrue(vr.ViewData.ModelState.IsValid, "Nothing passed, it doesn't invalidate ModelState");
            var model = (UccViewModel)vr.Model;
            Assert.IsNotNull(model, "Model should never be null");
        }

        /// <summary>
        /// Carton posted is not in database. Reprompts for carton
        /// </summary>
        [TestMethod]
        public void AcceptCartonInUcc_SomeCarton()
        {
            // Arrange
            _env.Form[_fieldNameScannedUccId] = CARTON_ID;

           // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "AcceptCartonInUcc");

            //Assert
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "Because the scanned pallet matched current pallet");
            Assert.AreEqual("Carton", rr.RouteValues["action"], "Redirection to Carton Action is expected");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Redirection to Home controller expected");
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "ModelState should remain valid because no errors expected in TempData");
            MasterModelWithPallet model = new MasterModelWithPallet(_env.Session);
            Helpers.AssertPalletMapping(model, _palletBeforePick);
        }
        #endregion

        #region Pallet Scan test

        /// <summary>
        /// User trying to start a new pallet. We refuse.
        /// </summary>
        [TestMethod]
        public void Ucc_NonCurrent_Pallet_Scanned()
        {
            // Arrange
            const string BAD_PALLET_ID = "P895656";
            _env.Form[_fieldNameScannedUccId] = BAD_PALLET_ID;

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptPalletInUcc");

            //Assert
            Assert.AreEqual("Ucc", vr.ViewName, "Redisplay same view");
            Assert.IsInstanceOfType(vr.Model, typeof(UccViewModel), "Unexpected model type");
            Assert.IsFalse(vr.ViewData.ModelState.IsValid, "Pallet should not qualify for partial picking");
            var model = (UccViewModel)vr.Model;
            Assert.IsNotNull(model, "Model should never be null");
            Assert.IsNull(model.CartonIdToPick, "CartonIdToPick should be null: No carton has been scanned so far");
            Assert.IsNull(model.LastLocation);
            Assert.IsNull(model.LastUccId);
            Helpers.AssertPalletMapping(model, _palletBeforePick);
        }

        /// <summary>
        /// Verify redirection to confirmation scan view
        /// </summary>
        [TestMethod]
        public void Ucc_Current_Pallet_Scanned()
        {
            // Arrange
            _env.Form[_fieldNameScannedUccId] = _palletBeforePick.PalletId;

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>(ACTION_NAME, "AcceptPalletInUcc");

            //Assert
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "Because the scanned pallet matched current pallet");
            Assert.AreEqual("PartialPickPallet", rr.RouteValues["action"], "Redirection to PartialPickPallet Action is expected");
            Assert.AreEqual("Confirm", rr.RouteValues["controller"], "Redirection to Confirm controller expected");
            Assert.IsTrue(_env.Controller.ViewData.ModelState.IsValid, "ModelState should remain valid because no errors expected in TempData");
        }

        #endregion
    }
}
