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
    ///This is a test class for ArchiveControllerPickSlipTest and is intended
    ///to contain all ArchiveControllerPickSlipTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ArchiveControllerPickSlipTest
    {


        private TestContext testContextInstance;


        /// <summary>
        ///This is a test class for PickSlipTest and is intended
        ///to contain all Pick Slip Test  Unit Tests
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
            Mapper.CreateMap<Pickslip, Pickslip>();
            Mapper.CreateMap<Customer, Customer>();
            Mapper.CreateMap<PurchaseOrder, PurchaseOrder>();
            Mapper.CreateMap<ScanInfo, ScanInfo>();
            Mapper.CreateMap<SkuWithPieces, SkuWithPieces>();
        }

        /// <summary>
        ///A test whether PiclSlip Test Return Expected scan type
        ///</summary>
        [TestMethod()]
        [Owner("Ankit")]
        public void InquiryArchivePickSlipScanTest_Home()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            var expectedScanInfo = new ScanInfo[] { new ScanInfo
                        {
                             ScanType = "PS",
                        }
                    };
            // GetScanType
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                .Returns(() => Mapper.Map<ScanInfo[], ScanInfo[]>(expectedScanInfo)).Verifiable();

            var target = new HomeController();
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);

            var expectedScan = "123456";
            var result = target.InquiryIndex(expectedScan);

            // Assert
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");


            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var rr = (RedirectToRouteResult)result;
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.ActionNames.HandlePickslipScan, rr.RouteValues["action"], "Action");
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.Name, rr.RouteValues["controller"], "Controller");
            Assert.AreEqual(expectedScan, rr.RouteValues["id"], "id");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey1, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey1)], "PrimaryKey1");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey2, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey2)], "PrimaryKey2");
        }
        /// <summary>
        /// Tests whether Carton scan returns proper information in the valid model
        ///</summary>
        [TestMethod()]
        public void InquiryPickSlipTest_Detail_ValidModel()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            //Providing Values 
            var expectedPickSlip = new Pickslip
            {
                Building = "AS",
                CreateDate = DateTime.Now,
                CustomerDC = "ADDD",
                CustomerStore = "ADFF",
                ExportFlag = "SFF",
                ImportDate = DateTime.Now,
                PickslipCancelDate = DateTime.Now,
                Label = "ASD",
                ModelTitle = "ADF",
                PickslipId = 1234,
                VwhId = "FDC",
                PO = new PurchaseOrder
                {
                    CancelDate = DateTime.Now,
                    CurrencyCode = "AFF",
                    Customer = new Customer
                    {
                        asn_flag = "QWER",
                        CarrierId = "SDFG",
                        Category = "ADF",
                        CustomerId = "ADFG",
                        CustomerName = "AMAN",
                        DefaultPickMode = "ADSD",
                        MaxPiecesPerBox = 12,
                        MinPiecesPerBox = 5,
                        Scan = "31335"
                    },
                    DcCancelDate = DateTime.Now,
                    Iteration = 12,
                    OrderDate = DateTime.Now,
                    PoId = "ASFG",
                    PSCount = 12,
                    Scan = "123234",
                    StartDate = DateTime.Now,
                    //UrlPo = "http://www.gmail.com",
                },
                AllSku = new SkuWithPieces[]{
                new SkuWithPieces
                {
                    Color = "color",
                    Pieces = 1245,
                    Dimension = "m",
                    SkuId = 125,
                    SkuSize = "m",
                    Upc = "123456789012",
                    Style = "style"
                }
                },
                AllBoxes = new Box[]{
                    new Box
                    {
                        Area="ASD",
                        Building="ASD",
                        CountSku=12,
                        CurrentPieces=12,
                        ExpectedPieces=12,
                        IaId="ASD",
                        ModelTitle="ASDF",
                        PalletId="PASD",
                        PitchingEndDate=DateTime.Now,
                        QcDate=DateTime.Now,
                        RfidTagsRequired="QDF",
                        Scan="134545",
                        Ucc128Id="00000146710004855780",
                        VerificationDate=DateTime.Now,
                        VwhId="121122",
                        Pickslip = new Pickslip {
                            PickslipId = 1234
                        }
                        
                    }
                }
            };
            //Mocking Classes
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Pickslip>>()))
               .Returns(() => Mapper.Map<Pickslip, Pickslip>(expectedPickSlip)).Verifiable();
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()))
               .Returns(() => Mapper.Map<IList<SkuWithPieces>, IList<SkuWithPieces>>(expectedPickSlip.AllSku)).Verifiable();
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Box>>()))
               .Returns(() => Mapper.Map<IList<Box>, IList<Box>>(expectedPickSlip.AllBoxes)).Verifiable();
            var target = new ArchiveController();
            target.ControllerContext = new ControllerContext();
            target.Db = new ArchiveRepository(db.Object);

            // Act
            var result = target.HandlePickslipScan(expectedPickSlip.PickslipId);

            // Assert

            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Pickslip>>()), Times.Once(), "PickSlip should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()), Times.Once(), "PickSlip SKU should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<Box>>()), Times.Once(), "PickSlip Boxes should be queried exactly once");

            // Assert For Model State
            Assert.IsTrue(target.ModelState.IsValid, "Model State Should Be Valid");


            //Assert For Model Values
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(Pickslip));
            var psvm = (Pickslip)vr.Model;

            Assert.AreEqual(expectedPickSlip.Building, psvm.Building, "Building");
            Assert.AreEqual(expectedPickSlip.CreateDate, psvm.CreateDate, "Cretae date");
            Assert.AreEqual(expectedPickSlip.CustomerDC, psvm.CustomerDC, "Customer DC");
            Assert.AreEqual(expectedPickSlip.CustomerStore, psvm.CustomerStore, "Customer Store");
            Assert.AreEqual(expectedPickSlip.ExportFlag, psvm.ExportFlag, "Export Flag");
            Assert.AreEqual(expectedPickSlip.ImportDate, psvm.ImportDate, "Import Flag");
            Assert.AreEqual(expectedPickSlip.Label, psvm.Label, "Label");
            //Assert.AreEqual(expectedPickSlip.ModelTitle, psvm.ModelTitle, "Model Title");
            Assert.AreEqual(expectedPickSlip.PickslipCancelDate, psvm.PickslipCancelDate, "Cancel Date");
            Assert.AreEqual(expectedPickSlip.PickslipId, psvm.PickslipId, "Pick Slip Id");
            Assert.AreEqual(expectedPickSlip.ReportingStatus, psvm.ReportingStatus, "Reporting Status");
            Assert.AreEqual(expectedPickSlip.ShippingId, psvm.ShippingId, "Shipping Id");
            Assert.AreEqual(expectedPickSlip.TotalQuantityOrdered, psvm.TotalQuantityOrdered, "Quantity");
            Assert.AreEqual(expectedPickSlip.TotalSKUOrdered, psvm.TotalSKUOrdered, "Sku Ordered");
            //Assert.AreEqual(expectedPickSlip.TransferDate, psvm.TransferDate, "Transfer Date");
            //Assert.AreEqual(expectedPickSlip.UrlPickslip, psvm.UrlPickslip, "URL");
            Assert.AreEqual(expectedPickSlip.VwhId, psvm.VwhId, "Vwh ID");
            Assert.AreEqual(expectedPickSlip.PO.CancelDate, psvm.PO.CancelDate, "Cancel Date");
            Assert.AreEqual(expectedPickSlip.PO.CurrencyCode, psvm.PO.CurrencyCode, "Currency Date");
            Assert.AreEqual(expectedPickSlip.PO.DcCancelDate, psvm.PO.DcCancelDate, "DC Date");
            Assert.AreEqual(expectedPickSlip.PO.Iteration, psvm.PO.Iteration, "Iteration");
            Assert.AreEqual(expectedPickSlip.PO.OrderDate, psvm.PO.OrderDate, "Order Date");
            Assert.AreEqual(expectedPickSlip.PO.PoId, psvm.PO.PoId, "PoId");
            Assert.AreEqual(expectedPickSlip.PO.PSCount, psvm.PO.PSCount, "Ps Count");
            Assert.AreEqual(expectedPickSlip.PO.Scan, psvm.PO.Scan, "Scan");
            Assert.AreEqual(expectedPickSlip.PO.StartDate, psvm.PO.StartDate, "Start Date");
            //Assert.AreEqual(expectedPickSlip.PO.UrlPo, psvm.PO.UrlPo, "Url Pro");
            Assert.AreEqual(expectedPickSlip.PO.Customer.asn_flag, psvm.PO.Customer.asn_flag, "Asn Flag");
            Assert.AreEqual(expectedPickSlip.PO.Customer.Category, psvm.PO.Customer.Category, "Categry");
            Assert.AreEqual(expectedPickSlip.PO.Customer.CustomerId, psvm.PO.Customer.CustomerId, "Customer Id");
            Assert.AreEqual(expectedPickSlip.PO.Customer.CustomerName, psvm.PO.Customer.CustomerName, "Custome Name");
            Assert.AreEqual(expectedPickSlip.PO.Customer.DefaultPickMode, psvm.PO.Customer.DefaultPickMode, "Default Pick Slip Mode");
        }
    }
}
