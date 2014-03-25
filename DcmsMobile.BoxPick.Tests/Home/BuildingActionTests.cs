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
    /// Call the Index action using GET and POST. In some cases, put something in Session.
    /// The outcome expected is the Building view with a clear session.
    /// Post bad building id (> 5 characters or not in DB) and expect the same view to get redisplayed.
    /// Good Building id should lead to pallet view.
    /// </summary>
    [TestClass]
    public class BuildingActionTests
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
        string _fieldName;

        const string ACTION_NAME = "Index";

        [TestInitialize()]
        public void MyTestInitialize()
        {
            _env = new ControllerTestEnvironment<HomeController>();
            _env.Headers.Add(HttpRequestHeader.UserAgent, "windows ce");
            _db = new Mock<IBoxPickRepository>(MockBehavior.Strict);
            _env.Controller.Repository = _db.Object;

            _env.UserName = "Sharad";
            _env.Role = "DCMS8_BOXPICK";
            //_env.RequestMethod = "POST";
            _fieldName = ReflectionHelpers.FieldNameFor((BuildingViewModel m) => m.ScannedBuildingOrArea);
        }

        /// <summary>
        /// Normal case. The action invoked via GET.
        /// </summary>
        [TestMethod]
        [TestCategory("Primary Path")]
        public void Get_Building_NoBuilding_CleanSession()
        {
            //Arrange 
            _env.RequestMethod = "GET";
            CommonErrorActAndAsserts(null);
        }

        [TestMethod]
        [TestCategory("Primary Path")]
        public void Post_Building_NoBuilding_CleanSession()
        {
            //Arrange 
            _env.RequestMethod = "POST";
            CommonErrorActAndAsserts(null);
        }

        /// <summary>
        /// Adding some junk to session and ensuring it gets cleared
        /// </summary>
        [TestMethod]
        public void Get_Building_NoBuilding_PopulatedSession()
        {
            //Arrange 
            _env.RequestMethod = "GET";
            MasterModel mm = new MasterModel(_env.Session);
            mm.SetLastCartonAndLocation("JUNK", null);
            CommonErrorActAndAsserts(null);
        }

        [TestMethod]
        public void Post_Building_NoBuilding_PopulatedSession()
        {
            //Arrange 
            _env.RequestMethod = "POST";
            MasterModel mm = new MasterModel(_env.Session);
            mm.SetLastCartonAndLocation("JUNK", null);
            CommonErrorActAndAsserts(null);
        }

        /// <summary>
        /// Ensure that everything in model is empty
        /// </summary>
        /// <param name="vr"></param>
        BuildingViewModel CommonErrorActAndAsserts(string buildingId)
        {
            
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptBuilding");
            Assert.AreEqual("Building", vr.ViewName, "Redisplay same view");
            Assert.IsInstanceOfType(vr.Model, typeof(BuildingViewModel), "Unexpected model type");
            var model = (BuildingViewModel)vr.Model;
            Assert.IsNull(model.LastCartonId, "Session must get cleared");
            Assert.IsTrue(string.IsNullOrEmpty(model.CurrentBuildingId), "Invalid building should never become current");
            Assert.IsNull(model.LastLocation, "Everything in model must be null");
            //Assert.AreEqual(buildingId, model.LastScan, "Last scan must be same as the passed building");
            Assert.AreEqual(buildingId, model.ScannedBuildingOrArea, "Scanned Building Id must be same as the passed building");
            Assert.IsNull(model.LastUccId, "Everything in model must be null");
            if (string.IsNullOrEmpty(buildingId))
            {
                Assert.IsTrue(vr.ViewData.ModelState.IsValid, "No building must not invalidate model state");
            }
            else
            {
                Assert.IsFalse(vr.ViewData.ModelState.IsValid, "Bad building must invalidate model state");
                Assert.IsFalse(vr.ViewData.ModelState.IsValidField(_fieldName), "Posted building not in DB");
            }
            return model;
        }


        /// <summary>
        /// Building is 6 characters. No query is expected.
        /// </summary>
        [TestMethod]
        public void Get_Building_BadBuilding()
        {
            //Arrange 
            const string BUILDING_ID = "ABCDEF";
            _env.RequestMethod = "GET";
            _env.QueryString[_fieldName] = BUILDING_ID;
            _db.Setup(p => p.GetPickContext(BUILDING_ID)).Returns((PickContext)null);
            //Assert
            CommonErrorActAndAsserts(BUILDING_ID);
        }

        /// <summary>
        /// Building is 6 characters. No query is expected.
        /// </summary>
        [TestMethod]
        public void Post_Building_BadBuilding()
        {
            //Arrange 
            const string BUILDING_ID = "ABCDEF";
            _env.RequestMethod = "POST";
            _env.Form[_fieldName] = BUILDING_ID;

            _db.Setup(p => p.GetPickContext(BUILDING_ID)).Returns((PickContext)null);

            // Act
            CommonErrorActAndAsserts(BUILDING_ID);
        }

        [TestMethod]
        public void Get_Building_BuildingNotInDb()
        {
            //Arrange 
            const string BUILDING_ID = "ABCDE";
            _env.RequestMethod = "GET";
            _env.QueryString[_fieldName] = BUILDING_ID;

            _db.Setup(p => p.GetPickContext(BUILDING_ID)).Returns((PickContext)null);

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptBuilding");

            //Assert
            _db.Verify(p => p.GetPickContext(BUILDING_ID), Times.Once());
            CommonErrorActAndAsserts(BUILDING_ID);
        }

        /// <summary>
        /// Building of reasonable length is posted. The database will claim that building does not exist
        /// </summary>
        [TestMethod]
        public void Post_Building_BuildingNotInDb()
        {
            //Arrange 
            const string BUILDING_ID = "ABCDE";
            _env.RequestMethod = "POST";
            _env.Form[_fieldName] = BUILDING_ID;

            _db.Setup(p => p.GetPickContext(BUILDING_ID)).Returns((PickContext)null);

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptBuilding");

            //Assert
            _db.Verify(p => p.GetPickContext(BUILDING_ID), Times.Once());
            CommonErrorActAndAsserts(BUILDING_ID);
        }




        /// <summary>
        /// Building of reasonable length is posted. The database will claim that building exist
        /// </summary>
        [TestMethod]
        public void Get_Building_BuildingInDb_Success()
        {
            //Arrange 
            const string BUILDING_ID = "ABCDE";
            _env.RequestMethod = "GET";
            _env.QueryString[_fieldName] = BUILDING_ID;

            CommonSuccessAsserts(BUILDING_ID);
        } 

        [TestMethod]
        [TestCategory("Primary Path")]
        public void Post_Building_BuildingInDb_Success()
        {
            //Arrange 
            const string BUILDING_ID = "ABCDE";
            _env.RequestMethod = "POST";
            _env.Form[_fieldName] = BUILDING_ID;

            CommonSuccessAsserts(BUILDING_ID);
        }

        private void CommonSuccessAsserts(string BUILDING_ID)
        {
            _db.Setup(p => p.GetPickContext(BUILDING_ID)).Returns(new PickContext {  BuildingId =BUILDING_ID , DestinationArea="ADR"});

            // Act
            var vr = _env.InvokeAction<ViewResult>(ACTION_NAME, "AcceptBuilding");

            //Assert
            _db.Verify(p => p.GetPickContext(BUILDING_ID), Times.Once());
            Assert.AreEqual("Pallet", vr.ViewName, "Pallet view displayed");
            Assert.IsTrue(vr.ViewData.ModelState.IsValid);
            Assert.IsTrue(vr.ViewData.ModelState.IsValidField(_fieldName));

            Assert.IsInstanceOfType(vr.Model, typeof(PalletViewModel), "Unexpected model type");
            var model = (PalletViewModel)vr.Model;
            Assert.AreEqual(BUILDING_ID, model.CurrentBuildingId, "Building must be same as passed");
            //Assert.AreEqual(BUILDING_ID, model.LastScan, "Last scan must be same as the passed building");
            Assert.IsNull(model.ScannedPalletId);
            Assert.IsNull(model.LastCartonId);
            Assert.IsNull(model.LastLocation);
            Assert.IsNull(model.LastUccId);
        }
    }
}
