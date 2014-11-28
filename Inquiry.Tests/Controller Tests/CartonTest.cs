using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using DcmsMobile.Areas.Inquiry.Controllers;
using DcmsMobile.Inquiry.Models;
using EclipseLibrary.Mvc.Helpers;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DcmsMobile.Inquiry.Repositories;

namespace Inquiry.Tests
{


    /// <summary>
    ///This is a test class for CartonTest and is intended
    ///to contain all Carton Test  Unit Tests
    ///</summary>
    [TestClass()]
    public class CartonControllerTest
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

        //This Is Used To Initialize  Classes For Providing Only Copied Data.
        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
            Mapper.CreateMap<InquiryViewModel, InquiryViewModel>();
            Mapper.CreateMap<Carton, Carton>();
            Mapper.CreateMap<SkuWithPieces, Sku>();
            Mapper.CreateMap<ScanInfo, ScanInfo>();
        }

        /// <summary>
        ///A test whether Carton Test Return Expected scan type
        ///</summary>
        [TestMethod()]
        public void InquiryCartonScanTest_Home()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            var expectedScanInfo = new ScanInfo[] { new ScanInfo
                        {
                             ScanType = "CTN",
                        }
                    };
            // GetScanType
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                .Returns(() => Mapper.Map<ScanInfo[], ScanInfo[]>(expectedScanInfo)).Verifiable();

            var target = new HomeController(); // TODO: Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);

            var expectedScan = "123456";
            var result = target.InquiryIndex(expectedScan);

            // Assert
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var rr = (RedirectToRouteResult)result;
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.ActionNames.HandleCartonScan, rr.RouteValues["action"], "Action");
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.Name, rr.RouteValues["controller"], "Controller");
            Assert.AreEqual(expectedScan, rr.RouteValues["id"], "id");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey1, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey1)], "PrimaryKey1");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey2, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey2)], "PrimaryKey2");
          
        }

        /// <summary>
        /// Tests whether Carton scan returns proper information in the valid model
        ///</summary>
        [TestMethod()]
        public void InquiryCartonTest_Detail_ValidModel()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            //Providing Values 
            var expectedCarton = new Carton
            {
                Building = "building",
                CartonId = "CtnId",
                CartonStorageArea = "StorageArea",
                DamageCode = "damageCode",
                LocationId = "LOCATIONID",
                PalletId = "PALLETiD",
                PriceSeasonCode = "seasoncode",
                QualityCode = "qltycode",
                RemarkWorkNeeded = "remarks",
                SewingPlant = "sweing",
                SuspenseDate = DateTime.Now,
                UnmatchComment = "comment",
                UnmatchReason = "reason",
                VwhId = "vwhid",
                AllSku = new SkuWithPieces[]{
                    new SkuWithPieces
                        {
                            Color = "color",
                            Pieces = 100,
                            Dimension = "m",
                            SkuId = 125,
                            SkuSize = "l",
                            Style = "style",
                            // 12 digit UPC
                            Upc = "123456789012"
                        }
                    }

            };

            //Mocking Classes
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Carton>>()))
             .Returns(() => Mapper.Map<Carton, Carton>(expectedCarton)).Verifiable();
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()))
               .Returns(() => Mapper.Map<IList<SkuWithPieces>, IList<SkuWithPieces>>(expectedCarton.AllSku)).Verifiable();


            var target = new DetailsController();
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);

            // Act
            var result = target.HandleCartonScan(expectedCarton.CartonId);

            // Assert

            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Carton>>()), Times.Once(), "Carton should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()), Times.Once(), "SkuWithPieces should be queried exactly once");

            // Assert For Model State
            Assert.IsTrue(target.ModelState.IsValid, "Model State Should Be Valid");

            //Assert For Model Values
            Assert.IsInstanceOfType(result, typeof(ViewResult),"View Is Not Matching");
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model,"Model Is Null");
            Assert.IsInstanceOfType(vr.Model, typeof(Carton),"Model IS Not Matching");
            var cvm = (Carton)vr.Model;

            Assert.AreEqual(expectedCarton.Building, cvm.Building, "Carton Building");
            Assert.AreEqual(expectedCarton.CartonId, cvm.CartonId, "Carton Id");
            Assert.AreEqual(expectedCarton.CartonStorageArea, cvm.CartonStorageArea, "CartonStorage Area");
            Assert.AreEqual(expectedCarton.DamageCode, cvm.DamageCode, "Damage Code");
            Assert.AreEqual(expectedCarton.LocationId, cvm.LocationId, "Location Id");
            Assert.AreEqual(expectedCarton.PalletId, cvm.PalletId, "Pallet Id");
            Assert.AreEqual(expectedCarton.PriceSeasonCode, cvm.PriceSeasonCode, "Price Season Code");
            Assert.AreEqual(expectedCarton.QualityCode, cvm.QualityCode, "Quality Code");
            Assert.AreEqual(expectedCarton.RemarkWorkNeeded, cvm.RemarkWorkNeeded, "Remark Work Needed");
            Assert.AreEqual(expectedCarton.SewingPlant, cvm.SewingPlant, "sweing plant");
            Assert.AreEqual(expectedCarton.SuspenseDate, cvm.SuspenseDate, "Suspended Date");
            Assert.AreEqual(expectedCarton.UnmatchComment, cvm.UnmatchComment, "UnmatchComment");
            Assert.AreEqual(expectedCarton.UnmatchReason, cvm.UnmatchReason, "UnmatchReason");
            Assert.AreEqual(expectedCarton.VwhId, cvm.VwhId, "VwhId");

            var query = expectedCarton.AllSku.Zip(cvm.AllSku, (expected, actual) =>
            {
                //Assert.Inconclusive();
                Assert.AreEqual(expected.Color, actual.Color, "Color");
                Assert.AreEqual(expected.Pieces, actual.Pieces, "Current Pieces");
                Assert.AreEqual(expected.Dimension, actual.Dimension, "Dimensions");
                Assert.AreEqual(expected.SkuId, actual.SkuId, "Sku Id");
                Assert.AreEqual(expected.SkuSize, actual.SkuSize, "Sku Size");
                Assert.AreEqual(expected.Style, actual.Style, "Style");
                Assert.AreEqual(expected.Upc, actual.Upc, "Upc");
                return string.Empty;
            }).ToList();
        }




        /// <summary>
        /// Tests whether Carton scan catches validation errors
        ///</summary>
        [TestMethod()]
        public void InquiryCartonTest_Detail_InValidModel()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            //Providing Values 
            var expectedCartonInvalid = new Carton
            {
                Building = "building",
                CartonId = "CtnId",
                CartonStorageArea = "StorageArea",
                DamageCode = "damageCode",
                LocationId = "LOCATIONID",
                PalletId = "PALLETiD",
                PriceSeasonCode = "seasoncode",
                QualityCode = "qltycode",
                RemarkWorkNeeded = "remarks",
                SewingPlant = "sweing",
                SuspenseDate = DateTime.Now,
                UnmatchComment = "comment",
                UnmatchReason = "reason",
                VwhId = "vwhid",
                AllSku = new SkuWithPieces[]{
                    new SkuWithPieces
                        {
                            Color = "color",
                            Pieces = 100,
                            Dimension = "m",
                            SkuId = 125,
                            SkuSize = "l",
                            Style = "style",
                            // UPC is 12 characters and valid
                            Upc = "123456789012"
                        },

                    new SkuWithPieces
                        {
                            Color = "color2",
                            Pieces = 200,
                            Dimension = "m2",
                            SkuId = 126,
                            SkuSize = "l2",
                            Style = "style2",
                            // UPC is not 12 characters and thus invalid
                            Upc = "12345678901234556"
                        }

                    }
            };

            //Mocking Classes
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Carton>>()))
             .Returns(() => Mapper.Map<Carton, Carton>(expectedCartonInvalid)).Verifiable();
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()))
               .Returns(() => Mapper.Map<IList<SkuWithPieces>, IList<SkuWithPieces>>(expectedCartonInvalid.AllSku)).Verifiable();


            var target = new DetailsController();
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);

            // Act
            var result = target.HandleCartonScan(expectedCartonInvalid.CartonId);

            // Assert

            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Carton>>()), Times.Once(), "Carton should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()), Times.Once(), "SkuWithPieces should be queried exactly once");

            // Assert For Model State
            Assert.IsFalse(target.ModelState.IsValid, "Model State Should Be Invalid");

            //Assert For Model Values
            Assert.IsInstanceOfType(result, typeof(ViewResult),"View Is Not MAtched");
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model,"Model Is Null");
            Assert.IsInstanceOfType(vr.Model, typeof(Carton), "Model Is Not Matching");
            var cvm = (Carton)vr.Model;
            Assert.IsTrue(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((Carton m) => m.AllSku[0].Upc)),
                "First UPC must be valid");
            Assert.IsFalse(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((Carton m) => m.AllSku[1].Upc)),
                "Second UPC must be invalid");
         
        }
    }
}