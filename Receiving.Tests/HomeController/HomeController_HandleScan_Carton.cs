using System;
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
    /// Summary description for HomeController_HandleScan_Carton
    /// </summary>
    [TestClass]
    public class HomeController_HandleScan_Carton
    {
        public HomeController_HandleScan_Carton()
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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Mapper.CreateMap<IntransitCarton, IntransitCarton>();
        }

        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //


        HomeController _target;
        Mock<HttpResponseBase> _response;
        Mock<IReceivingService> _service;

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _response = new Mock<HttpResponseBase>(MockBehavior.Loose) { CallBase = false };

            var httpContext = new Mock<HttpContextBase>(MockBehavior.Strict) { CallBase = false };
            httpContext.SetupGet(r => r.Response).Returns(_response.Object);

            var routeData = new RouteData();
            _target = new HomeController();
            var controllerContext = new ControllerContext(httpContext.Object, routeData, _target);
            _target.ControllerContext = controllerContext;

            _service = new Mock<IReceivingService>(MockBehavior.Loose);
            _target.Service = _service.Object;
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //


        #endregion


        /// <summary>
        /// Null model raises ArgumentNullException
        /// </summary>
        [Owner("Rajesh Kandari")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HomeController_HandleScan_NullModel()
        {
            //arrange

            // act
            var actual = _target.HandleScan(null);
        }

        ///// <summary>
        ///// Null Process Id raises ArgumentNullException
        ///// </summary>
        //[Owner("Rajesh Kandari")]
        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void HomeController_HandleScan_NullProcessId()
        //{
        //    //arrange

        //    // act
        //    var actual = _target.HandleScan();
        //}

        /// <summary>
        /// Empty scan returns empty result
        /// </summary>
        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void HomeController_HandleScan_EmptyScan()
        {
            //arrange

            // act
            var actual = _target.HandleScan(new ReceivingViewModel());

            // Assert          
            Assert.IsInstanceOfType(actual, typeof(ContentResult));
        }

        /// <summary>
        /// Service claims Pallet Scan. Status code 202 returned.
        /// </summary>
        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void HomeController_HandleScan_PalletScan()
        {
            //arrange
            var expectedRvm = new ReceivingViewModel
            {
                ScanModel = new ScanViewModel
                {
                    ScanText = "P123",
                },
                ProcessId = 123
            };

            _service.Setup(p => p.HandleScan(expectedRvm.ScanModel.ScanText, It.IsAny<ScanContext>()))
                .Callback((string scan, ScanContext ctx) => {
                    Assert.AreEqual(expectedRvm.ProcessId, ctx.ProcessId);
                    ctx.Result = ScanResult.PalletScan;
                }).Returns(new Pallet
            {
                
            });

            // act
            var actual = _target.HandleScan(expectedRvm);

            // Assert
            Assert.IsInstanceOfType(actual, typeof(PartialViewResult));
            _response.Verify(p => p.AppendHeader("PalletId", expectedRvm.ScanModel.PalletId), Times.Never());
            _response.Verify(p => p.AppendHeader("Disposition", It.IsAny<string>()), Times.Never());
            _response.VerifySet(p => p.StatusCode = 202, Times.Once());
        }

        /// <summary>
        /// Service claims CartonReceived. Status code 201 returned.
        /// </summary>
        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void HomeController_HandleScan_CartonReceived()
        {
            //arrange
            var expectedRvm = new ReceivingViewModel
            {
                ScanModel = new ScanViewModel
                {
                    ScanText = "C123",
                },
                ProcessId = 123
            };

            _service.Setup(p => p.HandleScan(expectedRvm.ScanModel.ScanText, It.IsAny<ScanContext>()))
                .Callback((string scan, ScanContext ctx) =>
                {
                    Assert.AreEqual(expectedRvm.ProcessId, ctx.ProcessId);
                    ctx.Result = ScanResult.CartonReceived;
                }).Returns(new Pallet
                {

                });

            // act
            var actual = _target.HandleScan(expectedRvm);

            // Assert
            Assert.IsInstanceOfType(actual, typeof(PartialViewResult));
            _response.Verify(p => p.AppendHeader("CartonId", expectedRvm.ScanModel.ScanText), Times.AtMostOnce());
            _response.Verify(p => p.AppendHeader("Disposition", ""), Times.AtMostOnce());
            _response.VerifySet(p => p.StatusCode = 201, Times.Once());
        }

        /// <summary>
        /// Service claims DispositionMismatch. Status code 250 returned.
        /// </summary>
        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void HomeController_HandleScan_DispositionMismatch()
        {
            //arrange
            var expectedRvm = new ReceivingViewModel
            {
                ScanModel = new ScanViewModel
                {
                    ScanText = "C123",
                },
                ProcessId = 123
            };

            _service.Setup(p => p.HandleScan(expectedRvm.ScanModel.ScanText, It.IsAny<ScanContext>()))
                .Callback((string scan, ScanContext ctx) =>
                {
                    Assert.AreEqual(expectedRvm.ProcessId, ctx.ProcessId);
                    ctx.Result = ScanResult.CartonReceived;
                }).Throws<DispositionMismatchException>();

            // act
            var actual = _target.HandleScan(expectedRvm);

            // Assert
            Assert.IsInstanceOfType(actual, typeof(ContentResult));
            _response.VerifySet(p => p.StatusCode = 250, Times.Once());
        }

        ///// <summary>
        ///// Scenario: Valid carton (i.e. it exists in intransit) passed to HandleScan and it gets received.
        ///// Input:
        /////   service.IsPallet must claim that this is not a pallet
        /////   service.GetIntransitCarton() must claim that this carton exists
        /////   service.ReceiveCarton must return carton which now exist on pallet
        ///// </summary>
        //[Owner("Ankit")]
        //[TestMethod]
        //public void HomeController_HandleScan_ValidCarton_Received()
        //{
        //    //arrange
        //    var inputModel = new ReceivingViewModel
        //    {
        //        ProcessId = 12345,

        //        ScanModel = new ScanViewModel
        //        {
        //            ScanText = "123456",
        //            PalletId = "P123",
        //            PalletDispos = "palletDisp"
        //        }
        //    };


        //    var service = new Mock<IReceivingService>(MockBehavior.Strict);
        //    IntransitCarton cartonToReceive = new IntransitCarton
        //    {
        //        CartonId = "123456",
        //        //DestinationArea = "BIR"
        //    };
        //    //service.Setup(p => p.IsPallet(inputModel.ScanModel.ScanText)).Returns(false);
        //    var expectedCartonsOnPallet = new ReceivedCarton[]{
        //        new ReceivedCarton{
        //            Building = "FKC",
        //            CartonId = "6625413",
        //            DestinationArea = "destin",
        //            DispositionId = "dipId",
        //            PalletId = "P1234",
        //            ReceivedDate = DateTime.Now,
        //            SingleSkuPerPallet = true,
        //            Sku = new Sku
        //            {
        //                Color = "wh",
        //                CurrentPieces = 25,
        //                Dimension = "l",
        //                SkuId = 125,
        //                SkuSize = "l",
        //               Style = "style",
        //               Upc = "12345678901"
        //             },
        //             SkuDisposition = "skuDisposition",
        //             VwhId = "VwHid"
        //          }
        //    };
        //    string outcome;
        //    service.Setup(p => p.HandleScan(cartonToReceive.CartonId, inputModel.ScanModel.PalletId, inputModel.ScanModel.PalletDispos, inputModel.ProcessId.Value, out outcome))
        //        .Callback((string palletId, IntransitCarton ctn, int processId, string palletDispo) => {
        //            // The in transint carton returned by the mock must get passed to the service
        //            Assert.AreEqual(cartonToReceive.CartonId, ctn.CartonId, "CartonId");
        //        }).Returns(expectedCartonsOnPallet.ToList());

        //    service.SetupGet(p => p.PalletLimit).Returns(5);

        //    _target.Service = service.Object;

        //    // act
        //    var actual = _target.HandleScan(inputModel);


        //    // assert
        //    // Assert the response object
        //    _response.VerifySet(p => p.StatusCode = 201, Times.Once(), "StatusCode was not set");
        //    _response.Verify(p => p.AppendHeader("CartonId", inputModel.ScanModel.ScanText), Times.Once());
        //    _response.Verify(p => p.AppendHeader("Disposition", It.IsAny<string>()), Times.Once());

        //    Assert.IsInstanceOfType(actual, typeof(PartialViewResult), "Content result");
        //    Assert.IsInstanceOfType(actual, typeof(PartialViewResult), "partial view result");
        //    var vr = (PartialViewResult)actual;
        //    var model = vr.Model;
        //    Assert.IsInstanceOfType(model, typeof(PalletViewModel), "Pallet View Result");
        //    var actualPalletModel = (PalletViewModel)model;
        //    Assert.IsNotNull(actualPalletModel, "PalletViewModel");

        //    Assert.AreEqual(expectedCartonsOnPallet.Length, actualPalletModel.Cartons.Count);
        //    expectedCartonsOnPallet.Zip(actualPalletModel.Cartons, (expectedvalue, actualvalue) =>
        //    {

        //        Assert.AreEqual(expectedvalue.Building, actualvalue.Building);
        //        Assert.AreEqual(expectedvalue.CartonId, actualvalue.CartonId, "Carton ID");
        //        Assert.AreEqual(expectedvalue.DestinationArea, actualvalue.DestinationArea, "Destination Area");
        //        Assert.AreEqual(expectedvalue.DispositionId, actualvalue.DispositionId, "Disposition Id");
        //        Assert.AreEqual(expectedvalue.PalletId, actualvalue.PalletId, "Pallet Id");
        //        Assert.AreEqual(expectedvalue.ReceivedDate, actualvalue.ReceivedDate, "Recieve Date");
        //        Assert.AreEqual(expectedvalue.SingleSkuPerPallet, actualvalue.SingleSkuPerPallet, "Single Sku Per Pallet");
        //        Assert.AreEqual(expectedvalue.Sku, actualvalue.Sku, "Sku");
        //        Assert.AreEqual(expectedvalue.SkuDisposition, actualvalue.SkuDisposition, "Sku Disposition");
        //        Assert.AreEqual(expectedvalue.VwhId, actualvalue.VwhId, "VWh Id");


        //        Assert.AreEqual(expectedvalue.Sku.Color, actualvalue.Sku.Color, "Sku Color");
        //        Assert.AreEqual(expectedvalue.Sku.CurrentPieces, actualvalue.Sku.CurrentPieces, "Sku CurrentPieces");
        //        Assert.AreEqual(expectedvalue.Sku.Dimension, actualvalue.Sku.Dimension, "Sku Dimension");
        //        Assert.AreEqual(expectedvalue.Sku.SkuId, actualvalue.Sku.SkuId, "Sku Id");
        //        Assert.AreEqual(expectedvalue.Sku.SkuSize, actualvalue.Sku.SkuSize, "SkuSize");
        //        Assert.AreEqual(expectedvalue.Sku.Style, actualvalue.Sku.Style, "Sku style");
        //        Assert.AreEqual(expectedvalue.Sku.Upc, actualvalue.Sku.Upc, "Upc");
        //        return string.Empty;
        //    }).ToList();



        //}

        /// <summary>
        /// 
        /// </summary>
        [Owner("Ankit")]
        [TestMethod]
        public void HomeController_PrintCarton()
        {
            ////arrange
            //var expectedmodel = new ReceivingViewModel
            //{
            //    ScanModel = new ScanViewModel
            //       {
            //           ScanText = "123456"
            //       }
            //};
            //var CartonToReceive = new IntransitCarton
            //{
            //    CartonId = "14324",
            //    DispositionId = "AASD"
            //};

            //var service = new Mock<IReceivingService>(MockBehavior.Strict);
            //service.Setup(p => p.IsPallet(expectedmodel.ScanModel.ScanText)).Returns(false);
            //// act
            //var actual = _target.HandleScan(expectedmodel);
            //Assert.IsInstanceOfType(actual, typeof(PartialViewResult), "Content result");
        }



        //    /// <summary>
        //    /// Valid carton passed, but its disposition did not match pallet disposition.
        //    /// </summary>
        //    /// <remarks>
        //    /// service.ReceiveCarton will return null to indicate disposition mismatch.
        //    /// </remarks>
        //    [Owner("Rajesh Knadari")]
        //    [TestMethod]
        //    public void HomeController_HandleScan_ValidCarton_Not_received()
        //    {
        //        //arrange
        //        var expectedmodel = new ReceivingViewModel
        //        {
        //            ProcessId = 123456,
        //            ScanModel = new ScanViewModel
        //            {
        //                ScanText = "123456",
        //                PalletId = "P123",
        //                PalletDispos = "D1"
        //            }
        //        };
        //        var CartonToReceive = new IntransitCarton
        //        {
        //            CartonId = "14324",

        //        };
        //        var service = new Mock<IReceivingService>(MockBehavior.Strict);
        //        //service.Setup(p => p.IsPallet(expectedmodel.ScanModel.ScanText)).Returns(false);
        //        //service.Setup(p => p.GetIntransitCarton(expectedmodel.ScanModel.ScanText)).Returns(CartonToReceive);
        //        string outcome;
        //        service.Setup(p => p.HandleScan(expectedmodel.ScanModel.ScanText, expectedmodel.ScanModel.PalletId, expectedmodel.ScanModel.PalletDispos,
        //            expectedmodel.ProcessId.Value, out outcome)).Returns((ReceivedCarton[])null);
        //        _target.Service = service.Object;

        //        //act
        //        var actual = _target.HandleScan(expectedmodel);

        //        //assert
        //        _response.VerifySet(p => p.StatusCode = 250, Times.Once(), "StatusCode was not set");
        //        Assert.IsInstanceOfType(actual, typeof(ContentResult));
        //        var cr = (ContentResult)actual;
        //        Assert.AreEqual(CartonToReceive.DispositionId, cr.Content, "CartonToReceive.DispositionId");
        //    }
    }

}

