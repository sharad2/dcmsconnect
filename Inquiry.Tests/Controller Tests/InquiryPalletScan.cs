using System.Linq;
using System.Web.Mvc;
using DcmsMobile.Areas.Inquiry.Controllers;
using DcmsMobile.Inquiry.Models;
using EclipseLibrary.Mvc.Helpers;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AutoMapper;
using System.Collections.Generic;
using DcmsMobile.Inquiry.Repositories;

namespace Inquiry.Tests
{


    /// <summary>
    ///This is a test class for Pallet Controller Test and is intended
    ///to contain all ControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PalletTest
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

        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
            Mapper.CreateMap<InquiryViewModel, InquiryViewModel>();
            Mapper.CreateMap<Pallet, Pallet>();
            Mapper.CreateMap<SkuWithPieces, Sku>();
            Mapper.CreateMap<ScanInfo, ScanInfo>();

        }


        /// <summary>
        /// A test whether Pallet Test Return Expected scan type
        ///</summary>
        [TestMethod()]
        [Owner("Rajesh Kandari")]
        public void InquiryPalletScanTest_Home()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            // GetScanType
            var scaninfo = new ScanInfo[]{ new ScanInfo
            {
                ScanType = "BPLT"
            }
            };
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                .Returns(() => Mapper.Map<ScanInfo[], ScanInfo[]>(scaninfo));

            var target = new HomeController(); //Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);
            var expectedScan = "123456";
            var result = target.InquiryIndex(expectedScan);
            
            // Act
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");
            // Assert For View Type
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var rr = (RedirectToRouteResult)result;
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.ActionNames.HandleBoxPalletScan, rr.RouteValues["action"], "Action");
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.Name, rr.RouteValues["controller"], "Controller");
            Assert.AreEqual(expectedScan, rr.RouteValues["id"], "id");
            Assert.AreEqual(scaninfo[0].PrimaryKey1, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey1)], "PrimaryKey1");
            Assert.AreEqual(scaninfo[0].PrimaryKey2, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey2)], "PrimaryKey2");
        }

        /// <summary>
        /// Tests whether Pallet scan returns proper information in the Valid model
        ///</summary>
        [TestMethod()]
        [Owner("Ankit")]
        public void InquiryPalletScanTest_Detail_Valid()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            // Providing values for properties

            var expectedPallet = new Pallet
            { 
                Building = "building",
                Customer = new Customer
                {
                },
                InventoryArea = "inventoryArea",
                PalletId = "Pallet1234",
                PickedBoxes = 54,
                TotalBoxes = 60,
                VwhId = "vwhid",
                Scan="122345", 
                AllSku = new SkuWithPieces[]{
                    new SkuWithPieces
                    {
                        Color = "wh",
                        Style = "style",
                        Pieces = 10,
                        Dimension = "L",
                        SkuId = 125,
                        SkuSize = "skuSize",
                        Upc = "012345678901",
                        QualityCode="ASFF"
                        
                    }
                }

            };
            //Mocking Classes
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Pallet>>()))
               .Returns(() => Mapper.Map<Pallet, Pallet>(expectedPallet)).Verifiable();
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()))
            .Returns(() => Mapper.Map<IList<SkuWithPieces>,IList<SkuWithPieces>>(expectedPallet.AllSku)).Verifiable();
            var target = new DetailsController();
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);

            // Act
            var result = target.HandleBoxPalletScan(expectedPallet.PalletId);
            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Pallet>>()), Times.Once(), "Pallet should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()), Times.Once(), "SkuWithPieces should be queried exactly once");
            
           
            //Assert For Model State Validation
            Assert.IsTrue(target.ModelState.IsValid, "Model State Should Be Valid");
            // Assert for Values
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            //Assert.AreEqual(vr.ViewName, "Pallet");
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(Pallet));
            var pvm = (Pallet)vr.Model;
            Assert.AreEqual(expectedPallet.Building, pvm.Building);
            Assert.AreEqual(expectedPallet.Customer, pvm.Customer);
            Assert.AreEqual(expectedPallet.InventoryArea, pvm.InventoryArea);
            Assert.AreEqual(expectedPallet.PalletId, pvm.PalletId);
            Assert.AreEqual(expectedPallet.PercentCompleted, pvm.PercentCompleted);
            Assert.AreEqual(expectedPallet.PickedBoxes, pvm.PickedBoxes);
            Assert.AreEqual(expectedPallet.TotalBoxes, pvm.TotalBoxes);
            Assert.AreEqual(expectedPallet.VwhId, pvm.VwhId);
            for (int i = 0; i < expectedPallet.AllSku.Count(); ++i)
            {
                Assert.AreEqual(expectedPallet.AllSku[i].Color, pvm.AllSku[i].Color);
                Assert.AreEqual(expectedPallet.AllSku[i].Pieces, pvm.AllSku[i].Pieces);
                Assert.AreEqual(expectedPallet.AllSku[i].Dimension, pvm.AllSku[i].Dimension);
                Assert.AreEqual(expectedPallet.AllSku[i].SkuId, pvm.AllSku[i].SkuId);
                Assert.AreEqual(expectedPallet.AllSku[i].SkuSize, pvm.AllSku[i].SkuSize);
                Assert.AreEqual(expectedPallet.AllSku[i].Style, pvm.AllSku[i].Style);
                Assert.AreEqual(expectedPallet.AllSku[i].Upc, pvm.AllSku[i].Upc);
            }

        }

        [TestMethod]
        [Owner("Ankit")]
        public void InquiryPalletScanTest_Detail_Invalid()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            // Providing values for properties

            var expectedInvalidPallet = new Pallet
            {
                Building = "building",
                Customer = new Customer
                {
                    
                },
                
                InventoryArea = "inventoryArea",
                // Invalid. Does not begin with P
                PalletId = "allet1234",
                PickedBoxes = 54,
                TotalBoxes = 60,
                VwhId = "vwhid",

                AllSku = new SkuWithPieces[]{
                    new SkuWithPieces
                    {
                        Color = "wh",
                        Style = "style",
                        // Invalid negative pieces
                        Pieces = -10,
                        Dimension = "L",
                        SkuId = 125,
                        SkuSize = "skuSize",
                        Upc = "upc"

                        
                    }
                }

            };
            //Mocking Classes
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Pallet>>()))
               .Returns(() => Mapper.Map<Pallet, Pallet>(expectedInvalidPallet)).Verifiable();
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()))
            .Returns(() => Mapper.Map<IList<SkuWithPieces>, IList<SkuWithPieces>>(expectedInvalidPallet.AllSku)).Verifiable();

            var target = new DetailsController();
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);

            // Act
            var result = target.HandleBoxPalletScan(expectedInvalidPallet.PalletId);
            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Pallet>>()), Times.Once(), "Pallet should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()), Times.Once(), "SkuWithPieces should be queried exactly once");


            //Assert For Model State Validation
            Assert.IsFalse(target.ModelState.IsValid, "Model State Should Be InValid");
            // Assert for Values
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(Pallet));
            var pvm = (Pallet)vr.Model;

            //Assert Invalid Fields
          
            Assert.IsFalse(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((Pallet p) => p.PalletId)),
                 "Pallet Id must be invalid"); 
            // Discuss About The Different Behavior Of Assert
            Assert.IsFalse(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((Pallet p) => p.AllSku[0].Pieces)),
               "CurrentPieces must be invalid");  
        }
    }
}
