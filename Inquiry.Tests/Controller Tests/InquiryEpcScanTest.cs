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
using System;
namespace Inquiry.Tests
{


    /// <summary>
    ///This is a test class for InquiryControllerTest and is intended
    ///to contain all InquiryControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BoxEpcTest
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
            Mapper.CreateMap<Box, Box>();
            Mapper.CreateMap<SkuWithEpc, BoxSku>();
            Mapper.CreateMap<ScanInfo, ScanInfo>();
        }



        /// <summary>
        /// A test whether EPC Test Return Expected scan type
        ///</summary>
        [TestMethod()]
        [Owner("Ankit")]
        public void InquiryEpcScanTest_Home()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            // GetScanType
            var expectedScanInfo = new ScanInfo[] { new ScanInfo
                        {
                             ScanType = "EPC",
                        }
                    };
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                 .Returns(() => Mapper.Map<ScanInfo[], ScanInfo[]>(expectedScanInfo)).Verifiable();

            var target = new HomeController();
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);

            var expectedScan = "123456";
            // Act
            var result = target.InquiryIndex(expectedScan);

            // Assert
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");
          
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var vr = (RedirectToRouteResult)result;
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.ActionNames.HandleEpcScan, vr.RouteValues["action"], "Action");
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.Name, vr.RouteValues["controller"], "Controller");
            Assert.AreEqual(expectedScan,vr.RouteValues["id"],"id");
            Assert.IsNull(expectedScanInfo[0].PrimaryKey1, ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey1), "PrimaryKey1");
            Assert.IsNull(expectedScanInfo[0].PrimaryKey2, ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey2), "PrimaryKey2");

        }
        /// <summary>
        /// Tests whether EPC scan returns proper information in the Valid model
        ///</summary>
        [TestMethod()]
        public void InquiryEPCScanTest_Detail_Valid()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            var expectedBox = new Box
           {
               Building = "AS",             
               IaId = "IaId",
               Scan = "112",
               Pickslip = new Pickslip
               {
                   PickslipId = 123,
                   PO = new PurchaseOrder
                   {
                       PoId = "123",
                      
                   }
               },
               Ucc128Id = "12345678900123456789",
               Area = "Area",
               CountSku = 5,
               CurrentPieces = 456,
               ExpectedPieces = 5,
               PalletId = "P123456",
               PitchingEndDate = DateTime.Now,
               QcDate = DateTime.Now,
               RfidTagsRequired = "123",
               VerificationDate = DateTime.Now,
               //cant determine the need for FirstSku Property
               VwhId = "123",
               SkuWithEpc = new SkuWithEpc[] { new SkuWithEpc
                   {
                       Color = "A",
                       Dimension = "AS",
                       SkuId = 1234,
                       SkuSize = "123",
                       Style = "as",
                       Upc = "123456789012",
                       Pieces=4,
                       ExpectedPieces=4,
                       ExtendedPrice = 25,
                                            
                       AllEpc = new string[] {
                           "EPC1",
                           "EPC2"
                       }                       
                   }
               }

           };

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Box>>()))
                .Returns(() => Mapper.Map<IEnumerable<Box>, IList<Box>>(Enumerable.Repeat(expectedBox, 1))).Verifiable();

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<BoxSku>>()))
                .Returns(() => Mapper.Map<IEnumerable<SkuWithEpc>, IList<BoxSku>>(expectedBox.SkuWithEpc)).Verifiable();

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Epc>>()))
                .Returns(() => expectedBox.SkuWithEpc.SelectMany(p => p.AllEpc.Select(q => new Epc
            {
                EpcCode = q,
                SkuId = p.SkuId
            })).ToList()).Verifiable();


            var target = new DetailsController(); 
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);

            // Act
            var result = target.HandleEpcScan(expectedBox.Ucc128Id);
            // Assert                   

            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<Box>>()), Times.Once(), "Box should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<Epc>>()), Times.Once(), "SkuWithEpc should be queried exactly once");
            //Assert For Model State
            Assert.IsTrue(target.ModelState.IsValid, "Model state must be valid");

            //Assert For Model Values
            Assert.IsInstanceOfType(result, typeof(ViewResult),"View Is Not Matching");
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model,"Model Is Null");
            Assert.IsInstanceOfType(vr.Model, typeof(Box),"Model Is not Matching");
            var bepc = (Box)vr.Model;
            Assert.AreEqual(expectedBox.Building, bepc.Building, "Building");
            Assert.AreEqual(expectedBox.Pickslip.PickslipId, bepc.Pickslip.PickslipId, "PickSlipId");
            Assert.AreEqual(expectedBox.Pickslip.PO.PoId, bepc.Pickslip.PO.PoId, "PoId");
            Assert.AreEqual(expectedBox.Ucc128Id, bepc.Ucc128Id, "UccId");
            Assert.AreEqual(expectedBox.VwhId, bepc.VwhId, "VwhId");
            Assert.AreEqual(expectedBox.Area, bepc.Area, "Area");
            Assert.AreEqual(expectedBox.CountSku, bepc.CountSku, "Count Sku");
            Assert.AreEqual(expectedBox.CurrentPieces, bepc.CurrentPieces, "Current Pieces");
            Assert.AreEqual(expectedBox.ExpectedPieces, bepc.ExpectedPieces, "Expected Pieces");
            Assert.AreEqual(expectedBox.IaId, bepc.IaId, "IaId");
            Assert.AreEqual(expectedBox.PalletId, bepc.PalletId, "PalletId");
            Assert.AreEqual(expectedBox.PitchingEndDate, bepc.PitchingEndDate, "Pitching End Date");
            //Assert.AreEqual(expectedBox.PitchingStatus, bepc.PitchingStatus, "Pitching Status");
            Assert.AreEqual(expectedBox.QcDate, bepc.QcDate, "Qc Date");
            Assert.AreEqual(expectedBox.QCStatus, bepc.QCStatus, "Qc Status");
            Assert.AreEqual(expectedBox.RfidTagsRequired, bepc.RfidTagsRequired, "RFID");
            for (int i = 0; i < expectedBox.SkuWithEpc.Count(); ++i)
            {

                Assert.AreEqual(expectedBox.SkuWithEpc[i].Color, bepc.SkuWithEpc[i].Color, "Color should be matched");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].Dimension, bepc.SkuWithEpc[i].Dimension, "Dimension");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].SkuId, bepc.SkuWithEpc[i].SkuId, "Sku Id");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].SkuSize, bepc.SkuWithEpc[i].SkuSize, "Sku Size");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].Style, bepc.SkuWithEpc[i].Style, "Style");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].Upc, bepc.SkuWithEpc[i].Upc, "UPC");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].Pieces, bepc.SkuWithEpc[i].Pieces, "Current Piece");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].ExpectedPieces, bepc.SkuWithEpc[i].ExpectedPieces, "Expected Price");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].ExtendedPrice, bepc.SkuWithEpc[i].ExtendedPrice, "Extended Price");
                CollectionAssert.AreEqual(expectedBox.SkuWithEpc[i].AllEpc.ToList(), bepc.SkuWithEpc[i].AllEpc.ToList());

            }
        }

        /// <summary>        
        /// Tests whether EPC scan returns proper information in the InValid model 
        /// Ensure that invalid UPC, UCC and PaleltID are caught by the target. Other fields are not checked for validity.
        /// </summary>
        [TestMethod]
        public void InquiryEPCScanTest_Detail_InValidModel()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            var expectedInvalidBox = new Box
            {
                //Building = "AS",
                //Customer = "GH",
                //PickslipId = 123,
                //PoId = "123",
                //IaId = "IaId",
                //Scan = "112",
                Ucc128Id = "5678900123456789",
                //Area = "Area",
                //CountSku = 5,
                //CurrentPieces = 456,
                //ExpectedPieces = 5,
                // Pallet invalid because it does not start with P
                PalletId = "123456",
                //PitchingEndDate = DateTime.Now,
                //QcDate = DateTime.Now,
                //RfidTagsRequired = "123",
                //VerificationDate = DateTime.Now,
                //VwhId = "123",
                SkuWithEpc = new SkuWithEpc[] { new SkuWithEpc
                   {
                       //Color = "A",
                       //Dimension = "AS",
                       //SkuId = 1234,
                       //SkuSize = "123",
                       //Style = "as",
                       // UPC is invalid != 12 digits
                       //Upc = "123456789012",
                       Upc = "12345",
                       //CurrentPieces=4,
                       //ExpectedPieces=4,
                       //ExtendedPrice = 25,
                                            
                       AllEpc = new string[] {
                       }                       
                   }
               }
               
            };

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Box>>()))
                .Returns(() => Mapper.Map<IEnumerable<Box>, IList<Box>>(Enumerable.Repeat(expectedInvalidBox, 1))).Verifiable();

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<BoxSku>>()))
                .Returns(() => Mapper.Map<IEnumerable<SkuWithEpc>, IList<BoxSku>>(expectedInvalidBox.SkuWithEpc)).Verifiable();

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Epc>>()))
                .Returns(() => expectedInvalidBox.SkuWithEpc.SelectMany(p => p.AllEpc.Select(q => new Epc
                {
                    EpcCode = q,
                    SkuId = p.SkuId
                })).ToList()).Verifiable();



            var target = new DetailsController(); 
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);

            // Act
            var result = target.HandleEpcScan(expectedInvalidBox.Ucc128Id);
            // Assert                   

            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<Box>>()), Times.Once(), "Box should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<Epc>>()), Times.Once(), "SkuWithEpc should be queried exactly once");
            //Assert For Model State
            Assert.IsFalse(target.ModelState.IsValid, "Model state must be Invalid");

            //Assert For Model Values
            Assert.IsInstanceOfType(result, typeof(ViewResult),"View is Not Matching");
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model,"Model Is Null");
            Assert.IsInstanceOfType(vr.Model, typeof(Box),"Model is Not Matching");
            var cvm = (Box)vr.Model;

            // Assert invalid fields
            Assert.IsFalse(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((Box m) => m.SkuWithEpc[0].Upc)),
                "UPC must be invalid");
            Assert.IsFalse(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((Box m) => m.PalletId)),
                "Pallet must be invalid");
            Assert.IsFalse(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((Box m) => m.Ucc128Id)),
                "UPC must be invalid");
        }


    }
}
