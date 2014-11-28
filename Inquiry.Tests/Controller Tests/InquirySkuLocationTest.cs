using System.Linq;
using System.Web.Mvc;
using DcmsMobile.Areas.Inquiry.Controllers;
using DcmsMobile.Inquiry.Models;
using EclipseLibrary.Mvc.Helpers;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using AutoMapper;
using System;
using DcmsMobile.Inquiry.Repositories;


namespace Inquiry.Tests
{
    /// <summary>
    ///This is a test class for InquirySkuLocationTest and is intended
    ///to contain all InquirySkuLocationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InquirySkuLocationTest
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
            Mapper.CreateMap<SkuLocation, SkuLocation>();
            Mapper.CreateMap<InquiryViewModel, InquiryViewModel>();
            Mapper.CreateMap<SkuWithPieces, SkuWithPieces>();
            Mapper.CreateMap<ScanInfo, ScanInfo>();
        }

        
        /// <summary>
        /// Tests whether SA scan returns proper information in the model
        ///</summary>
        [TestMethod()]
        [Owner("Rajesh Kandari")]
        public void InquirySkuLocationTest_Home()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            // GetScanType
            var expectedscaninfo = new ScanInfo[]{
                new ScanInfo
                {
                    ScanType = "SA"
                }
            };

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                .Returns(() => Mapper.Map<ScanInfo[],ScanInfo[]>(expectedscaninfo)); 

            var target = new HomeController();
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);
            var expectedscan = "2324324";
            // Act
            var result = target.InquiryIndex(expectedscan);

            // Assert For SKU Location Scan Type
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var rr = (RedirectToRouteResult)result;
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.ActionNames.HandleSKuLocationScan, rr.RouteValues["action"], "Action");
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.Name, rr.RouteValues["controller"], "Controller");
            Assert.AreEqual(expectedscan, rr.RouteValues["id"], "id");
            Assert.AreEqual(expectedscaninfo[0].PrimaryKey1,rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey1)], "PrimaryKey1");
            Assert.AreEqual(expectedscaninfo[0].PrimaryKey2, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey2)], "PrimaryKey2");
         }


        //Checking That Sku Location Passes Valid Values
         /// <summary>
        [TestMethod()]
        public void InquirySkuLocationTest_Detail_Valid()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            var expectedskulocation = new SkuLocation
            {
                AllSku = new SkuWithPieces[]{
                new SkuWithPieces
                {
                    Color = "color",
                    Pieces = 12,
                    Dimension = "m",
                    SkuId = 125,
                    SkuSize = "l",
                    Style = "style",
                    Upc = "123456789012"
                    
                }
                },
                Aisle = "ASDh",
                CycDate = DateTime.Now,
                CycFlag = "AS",
                FreezeFlag = "AER",
                IaId = "123334",
                LocationId = "12457",
                MaxPieces = 12,
                VwhId = "NGD",
                WhlId = "EDF"
            };
            //Mockin Classes 
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<SkuLocation>>()))
                .Returns(() => Mapper.Map<SkuLocation, SkuLocation>(expectedskulocation)).Verifiable();

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()))
              .Returns(() => Mapper.Map<IList<SkuWithPieces>, IList<SkuWithPieces>>(expectedskulocation.AllSku)).Verifiable();
            
            
          
            
            var target = new DetailsController();
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);
            var result = target.HandleSKuLocationScan("12345");

            //Assert
            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<SkuLocation>>()), Times.Once(), "Sku Location should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()), Times.Once(), "SkuWithPieces should be queried exactly once");


            // Assert for Model State
            Assert.IsTrue(target.ModelState.IsValid, "Model State Should Be Valid");

            //Assert for Model Values
            Assert.IsInstanceOfType(result, typeof(ViewResult),"View is Not Matching");
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model,"Model Is Null");
            Assert.IsInstanceOfType(vr.Model, typeof(SkuLocation),"Model Is Not Matching");
            var slo = (SkuLocation)vr.Model;
            //Assert For Sku Location properties
            Assert.AreEqual(expectedskulocation.Aisle,slo.Aisle,"Aisle");
            Assert.AreEqual(expectedskulocation.CycDate, slo.CycDate,"Cyc Date");
            Assert.AreEqual(expectedskulocation.CycFlag, slo.CycFlag,"Cyc Flag");
            Assert.AreEqual(expectedskulocation.FreezeFlag, slo.FreezeFlag,"Freeze Flag");
            Assert.AreEqual(expectedskulocation.IaId, slo.IaId,"Ia ID");
            Assert.AreEqual(expectedskulocation.LocationId, slo.LocationId,"Location Id");
            Assert.AreEqual(expectedskulocation.MaxPieces, slo.MaxPieces,"Max Pieces");
            Assert.AreEqual(expectedskulocation.VwhId, slo.VwhId,"VwhID");
            Assert.AreEqual(expectedskulocation.WhlId, slo.WhlId,"WhID");

            //Assert For Sku With Pieces In Sku Location
            for (int i = 0; i < expectedskulocation.AllSku.Count; ++i)
            {
                Assert.AreEqual(expectedskulocation.AllSku[i].Color, slo.AllSku[0].Color,"Color");
                Assert.AreEqual(expectedskulocation.AllSku[i].Pieces, slo.AllSku[0].Pieces,"Current Pieces");
                Assert.AreEqual(expectedskulocation.AllSku[i].Dimension, slo.AllSku[0].Dimension,"Dimension");
                Assert.AreEqual(expectedskulocation.AllSku[i].SkuId, slo.AllSku[0].SkuId,"Sku Id");
                Assert.AreEqual(expectedskulocation.AllSku[i].SkuSize, slo.AllSku[0].SkuSize,"Sku Size");
                Assert.AreEqual(expectedskulocation.AllSku[i].Style, slo.AllSku[0].Style,"Style");
                Assert.AreEqual(expectedskulocation.AllSku[i].Upc, slo.AllSku[0].Upc,"UPC");
            }
            
        }

        /// <summary>
        /// Tests whether Sku Location scan catches validation errors
        ///</summary>
        
        [TestMethod]
        public void InquirySkuLocationTest_Detail_InValid()
        {


            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            var expectedskulocationInvalid = new SkuLocation
            {
                AllSku = new SkuWithPieces[]{
                new SkuWithPieces
                {
                    Color = "color",
                    //current pieces should not positive
                    Pieces = -12,
                    Dimension = "m",
                    SkuId = 125,
                    SkuSize = "l",
                    Style = "style",
                    //Ups must not 12 digit
                    Upc = "12345678"
                    
                }
                },
                Aisle = "ASDh",
                CycDate = DateTime.Now,
                CycFlag = "AS",
                FreezeFlag = "AER",
                IaId = "123334",
                LocationId = "1376",
                MaxPieces = 12,
                VwhId = "NGD",
                WhlId = "EDF"
            };
            //Mockin Classes 
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<SkuLocation>>()))
                .Returns(() => Mapper.Map<SkuLocation, SkuLocation>(expectedskulocationInvalid)).Verifiable();

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()))
              .Returns(() => Mapper.Map<IList<SkuWithPieces>, IList<SkuWithPieces>>(expectedskulocationInvalid.AllSku)).Verifiable();




            var target = new DetailsController();
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);
            var result = target.HandleSKuLocationScan("12345");

            //Assert
            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<SkuLocation>>()), Times.Once(), "Sku Location should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()), Times.Once(), "SkuWithPieces should be queried exactly once");


            // Assert for Model State
            Assert.IsFalse(target.ModelState.IsValid, "Model State Should Be Valid");

            //Assert for Model Values
            Assert.IsInstanceOfType(result, typeof(ViewResult),"View Is Not Matching");
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model,"View Is Null");
            Assert.IsInstanceOfType(vr.Model, typeof(SkuLocation),"Model Is Not Matching");
            var slo = (SkuLocation)vr.Model;

            //want to ask about assert
            Assert.IsFalse(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((SkuLocation m) => m.AllSku[0].Upc)),
              "UPC must be invalid");
            Assert.IsFalse(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((SkuLocation m) => m.AllSku[0].Pieces )),
              "Current Pieces must be invalid");

        }

    }
}
