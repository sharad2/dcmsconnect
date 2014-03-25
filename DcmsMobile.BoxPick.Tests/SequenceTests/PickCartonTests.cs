using System;
using System.Collections.Generic;
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

namespace DcmsMobile.BoxPick.Tests.SequenceTests
{
    /// <summary>
    /// All the tests in this class must be run in the sequence they are defined. Individual tests are not expected to succeed in isolation.
    /// </summary>
    [TestClass]
    public class PickCartonTests
    {
        public PickCartonTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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
        //private static Queue<string> __qUccToPick;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _env = new ControllerTestEnvironment<HomeController>();
            _env.Headers.Add(HttpRequestHeader.UserAgent, "windows ce");
            _env.UserName = "Sharad";
            _env.Role = "DCMS8_BOXPICK";
        }

        private Mock<IBoxPickRepository> _db;

        /// <summary>
        /// Recreate the repository and clear forms.
        /// </summary>
        private void InitializeRequest()
        {
            _db = new Mock<IBoxPickRepository>(MockBehavior.Strict);
            _env.Controller.Repository = _db.Object;
            _env.Form.Clear();
        }

        private Pallet CreatePallet()
        {
            var rand = new Random();

            //As IsFull Property is based on TotolBoxCount and PickedBoxCount so it needs to be defined

            return new Pallet
            {
                PalletId = "P12343",
                QueryTime = DateTime.Now - TimeSpan.FromMinutes(rand.Next(60)),
                BoxToPick = new Box
                {
                    //UccId = uccToPick,
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
                DestinationArea = "ADR",
                BuildingId = "FDC",
                PickModeText = "ADREPPWSS"
            };
        }

        /// <summary>
        /// Posts, building, pallet, [Carton, UCC]... until the pallet gets fully picked.
        /// Asserts that PickCarton was called for each carton, UCC combination.
        /// </summary>
        [TestMethod]
        [TestCategory("Primary Path")]
        public void Building_Pallet_Carton_Ucc()
        {
            InitializeRequest();
            Post_Building();

            var qUccToPick = new Queue<KeyValuePair<string, string>>();
            qUccToPick.Enqueue(new KeyValuePair<string, string>("00001234567890123451", "1234561"));
            qUccToPick.Enqueue(new KeyValuePair<string, string>("00001234567890123452", "1234562"));
            qUccToPick.Enqueue(new KeyValuePair<string, string>("00001234567890123453", "1234563"));
            qUccToPick.Enqueue(new KeyValuePair<string, string>("00001234567890123454", "1234564"));
            qUccToPick.Enqueue(new KeyValuePair<string, string>("00001234567890123455", "1234565"));
            qUccToPick.Enqueue(new KeyValuePair<string, string>("00001234567890123456", "1234566"));
            qUccToPick.Enqueue(new KeyValuePair<string, string>("00001234567890123457", "1234567"));

            InitializeRequest();
            var pallet = CreatePallet();
            pallet.BoxToPick.UccId = qUccToPick.Peek().Key;

            Post_Pallet(pallet);

            while (qUccToPick.Count > 0)
            {
                InitializeRequest();
                var carton = new Carton
                {
                    CartonId = qUccToPick.Peek().Value,
                    SkuInCarton = new Sku
                    {
                        SkuId = pallet.BoxToPick.SkuInBox.SkuId
                    },
                    Pieces = pallet.BoxToPick.Pieces,
                    QualityCode = pallet.BoxToPick.QualityCode,
                    VwhId = pallet.BoxToPick.VwhId,
                    StorageArea = pallet.CartonSourceArea
                };

                Post_Good_Carton(carton, pallet);

                InitializeRequest();
                var uccId = qUccToPick.Dequeue();
                var palletAfterPicking = CreatePallet();
                if (qUccToPick.Count > 0)
                {
                    palletAfterPicking.BoxToPick.UccId = qUccToPick.Peek().Key;
                    Post_Ucc_Pallet_StillPickable(uccId.Key, uccId.Value, pallet.QueryTime.Value, palletAfterPicking);
                }
                else
                {
                    palletAfterPicking.BoxToPick = null;
                    Post_Ucc_PalletComplete(uccId.Key, uccId.Value, pallet.QueryTime.Value, palletAfterPicking);
                }
                pallet = palletAfterPicking;
            }
        }

        /// <summary>
        /// Posts the last UCC of the pallet. Asserts redirection to Pallet view
        /// </summary>
        /// <param name="uccId"></param>
        /// <param name="cartonIdToPick"></param>
        private void Post_Ucc_PalletComplete(string uccId, string cartonIdToPick, DateTime productivityStartTime, Pallet palletToRetrieve)
        {
            // Arrange
            var fieldName = ReflectionHelpers.FieldNameFor((UccViewModel m) => m.ScannedUccId);
            _env.Form[fieldName] = uccId;
            _db.Setup(p => p.PickCarton(uccId, cartonIdToPick, productivityStartTime));
            _db.Setup(p => p.RetrievePalletInfo(palletToRetrieve.PalletId)).Returns(palletToRetrieve);

            // Act
            var rr = _env.InvokeAction<RedirectToRouteResult>("Ucc", "AcceptUcc");
            Assert.AreEqual("Pallet", rr.RouteValues["action"], "Must reprompt for pallet");
            Assert.AreEqual("Home", rr.RouteValues["controller"], "Must redirect to pallet in home controller");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uccId">UCC To post</param>
        /// <param name="cartonIdToPick">Carton which will be picked against this UCC</param>
        /// <param name="productivityStartTime">The time at which the pallet for this UCC was proposed</param>
        /// <param name="palletToRetrieve">The pallet to retrieve after UCC has been picked</param>
        private void Post_Ucc_Pallet_StillPickable(string uccId, string cartonIdToPick, DateTime productivityStartTime, Pallet palletToRetrieve)
        {
            // Arrange
            var fieldName = ReflectionHelpers.FieldNameFor((UccViewModel m) => m.ScannedUccId);
            _env.Form[fieldName] = uccId;
            _db.Setup(p => p.PickCarton(uccId, cartonIdToPick, productivityStartTime));
            _db.Setup(p => p.RetrievePalletInfo(palletToRetrieve.PalletId)).Returns(palletToRetrieve);

            // Act
            var vr = _env.InvokeAction<ViewResult>("Ucc", "AcceptUcc");
            Assert.AreEqual("Carton", vr.ViewName);
        }

        private void Post_Good_Carton(Carton cartonToPost, Pallet currentPallet)
        {
            var fieldName = ReflectionHelpers.FieldNameFor((CartonViewModel m) => m.ScannedCartonId);
            _env.Form[fieldName] = cartonToPost.CartonId;


            _db.Setup(p => p.GetCartonDetails(cartonToPost.CartonId)).Returns(cartonToPost);

            // Act
            var vr = _env.InvokeAction<ViewResult>("Carton", "AcceptCarton");

            //Assert
            _db.Verify(p => p.GetCartonDetails(cartonToPost.CartonId), Times.Once());
            Assert.AreEqual("Ucc", vr.ViewName, "Display UCC view");
        }

        public void Post_Building()
        {
            // Arrange
            const string BUILDING_ID = "FDC";
            _env.RequestMethod = "POST";
            var fieldName = ReflectionHelpers.FieldNameFor((BuildingViewModel m) => m.ScannedBuildingOrArea);
            _env.Form[fieldName] = BUILDING_ID;
            _db.Setup(p => p.GetPickContext(BUILDING_ID)).Returns(new PickContext { BuildingId=BUILDING_ID, DestinationArea="ADR"});

            // Act
            var vr = _env.InvokeAction<ViewResult>("Index", "AcceptBuilding");
            Assert.AreEqual("Pallet", vr.ViewName, "Pallet view displayed");
        }

        public void Post_Pallet(Pallet palletToPost)
        {
            var fieldName = ReflectionHelpers.FieldNameFor((PalletViewModel m) => m.ScannedPalletId);
            _env.Form[fieldName] = palletToPost.PalletId;

            _db.Setup(p => p.RetrievePalletInfo(palletToPost.PalletId)).Returns(palletToPost);

            // Act
            var vr = _env.InvokeAction<ViewResult>("Pallet", "AcceptPallet");
            Assert.AreEqual("Carton", vr.ViewName, "Carton view displayed");
        }
    }
}
