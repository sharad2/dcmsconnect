using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using DcmsMobile.Areas.Inquiry.Controllers;
using DcmsMobile.Inquiry.Models;
using DcmsMobile.Inquiry.Repositories;
using EclipseLibrary.Mvc.Helpers;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Inquiry.Tests
{
    
    
    /// <summary>
    ///This is a test class for ArchiveControllerTest and is intended
    ///to contain all ArchiveControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ArchiveControllerTest
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
            Mapper.CreateMap<Box, Box>().ForMember(dest => dest.SkuWithEpc, opt => opt.Ignore());
            Mapper.CreateMap<SkuWithEpc, BoxSku>();
            Mapper.CreateMap<ScanInfo, ScanInfo>();
        }

        /// <summary>
        /// Tests whether UCC scan returns proper information in the model
        ///</summary>
        [TestMethod()]
        [Owner("Ankit")]
        public void InquiryUccScanTest_Home()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            // GetScanType
            var expectedScaninfo = new ScanInfo[]{
                new ScanInfo{
                             ScanType = "UCC"
                }
            };
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                .Returns(() => Mapper.Map<ScanInfo[], ScanInfo[]>(expectedScaninfo)).Verifiable();

            var target = new HomeController();
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);
            var expectedScan = "123456";

            // Act
            var result = target.InquiryIndex(expectedScan);

            // Assert
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");

            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var rr = (RedirectToRouteResult)result;
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.ActionNames.HandleUccScan, rr.RouteValues["action"], "Action");
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.Name, rr.RouteValues["controller"], "Controller");
            Assert.AreEqual(expectedScan, rr.RouteValues["id"], "id");
            Assert.AreEqual(expectedScaninfo[0].PrimaryKey1, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey1)], "PrimaryKey1");
            Assert.AreEqual(expectedScaninfo[0].PrimaryKey2, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey2)], "PrimaryKey2");
        }

        /// <summary>
        /// Tests whether UCC scan returns proper information in the model.
        ///</summary>
        [TestMethod()]
        [Owner("Ankit")]
        public void InquiryUccScanTest_Detail_ValidModel()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            var expectedBox = new Box
            {
                Building = "fuh",
                //Customer = "cust",
                IaId = "iaId",
                PalletId = "PalletId",
                CurrentPieces = 100,
                PitchingEndDate = DateTime.Now,

                Pickslip = new Pickslip
                {
                    PickslipId = 456,
                    PO = new PurchaseOrder
                    {
                        PoId = "poId",
                    }
                },
                QcDate = DateTime.Now,
                RfidTagsRequired = "rfid",
                Ucc128Id = "01234567890123456789",
                VerificationDate = DateTime.Now,
                VwhId = "vwhId",

                SkuWithEpc = new SkuWithEpc[] {
                     new SkuWithEpc {
                    Color = "wh",
                    Pieces = 100,
                    Style = "style",
                    Dimension = "m",
                    ExpectedPieces = 150,
                    ExtendedPrice = 125,
                    SkuId = 125,
                    SkuSize = "l",
                    Upc = "123456789012",
                     AllEpc = new string[] {
                         "EPC1",
                         "EPC2"
                     }
                     }
                 }
            };

            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Box>>()))
               .Returns(() => Mapper.Map<Box, Box>(expectedBox)).Verifiable();

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<BoxSku>>()))
                .Returns(() => Mapper.Map<IList<SkuWithEpc>, IList<BoxSku>>(expectedBox.SkuWithEpc));

            var query = expectedBox.SkuWithEpc.SelectMany(p => p.AllEpc.Select(q => new Epc
            {
                SkuId = p.SkuId,
                EpcCode = q
            }));
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Epc>>()))
               .Returns(() => Mapper.Map<IEnumerable<Epc>, IList<Epc>>(query));


            var target = new ArchiveController(); // TODO: Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new ArchiveRepository(db.Object);

            // Act
            var result = target.HandleUccScan(expectedBox.Ucc128Id);

            // Assert
            Assert.IsTrue(target.ModelState.IsValid, "Model state must be valid");

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(Box));
            var bvm = (Box)vr.Model;
            Assert.AreEqual(expectedBox.Building, bvm.Building, "Building");
            Assert.AreEqual(expectedBox.IaId, bvm.IaId, "IaId");
            Assert.AreEqual(expectedBox.PalletId, bvm.PalletId, "PalletId");
            Assert.AreEqual(expectedBox.Pickslip.PickslipId, bvm.Pickslip.PickslipId, "PickSlipId");
            //Assert.AreEqual(expectedBox.PitchingStatus, bvm.PitchingStatus, "Pitching Status");
            Assert.AreEqual(expectedBox.Pickslip.PO.PoId, bvm.Pickslip.PO.PoId, "PoId");
            Assert.AreEqual(expectedBox.QCStatus, bvm.QCStatus, "QCStatus");
            Assert.AreEqual(expectedBox.RfidTagsRequired, bvm.RfidTagsRequired, "rfidTagsRequired");
            Assert.AreEqual(expectedBox.Ucc128Id, bvm.Ucc128Id, "Ucc128Id");
            //Assert.AreEqual(expectedBox.Verification, bvm.Verification, "Verification");
            Assert.AreEqual(expectedBox.VwhId, bvm.VwhId, "VwhId");

            for (int i = 0; i < expectedBox.SkuWithEpc.Count; ++i)
            {
                Assert.AreEqual(expectedBox.SkuWithEpc[i].Color, bvm.SkuWithEpc[i].Color, "Color");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].Pieces, bvm.SkuWithEpc[i].Pieces, "CurrentPieces");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].Dimension, bvm.SkuWithEpc[i].Dimension, "Dimension");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].ExpectedPieces, bvm.SkuWithEpc[i].ExpectedPieces, "Expected Price");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].ExtendedPrice, bvm.SkuWithEpc[i].ExtendedPrice, "Extended Price");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].SkuId, bvm.SkuWithEpc[i].SkuId, "SkuId");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].SkuSize, bvm.SkuWithEpc[i].SkuSize, "SkuSize");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].Style, bvm.SkuWithEpc[i].Style, "Style");
                Assert.AreEqual(expectedBox.SkuWithEpc[i].Upc, bvm.SkuWithEpc[i].Upc, "Upc");
                CollectionAssert.AreEqual(expectedBox.SkuWithEpc[i].AllEpc.ToList(), bvm.SkuWithEpc[i].AllEpc.ToList());
            }

        }
    }
}
