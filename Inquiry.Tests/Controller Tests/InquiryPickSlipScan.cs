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
    public class PickSlipControllerTest
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
            Mapper.CreateMap<Pickslip, Pickslip>();
            Mapper.CreateMap<Box, Box>();
            Mapper.CreateMap<SkuWithPieces, SkuWithPieces>();
            Mapper.CreateMap<PurchaseOrder,PurchaseOrder >();
            Mapper.CreateMap<ScanInfo, ScanInfo>();
        }


        /// <summary>
        ///A test whether PiclSlip Test Return Expected scan type
        ///</summary>
        [TestMethod()]
        [Owner("Ankit")]
        public void InquiryPickSlipScanTest_Home()
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

            var target = new HomeController(); // TODO: Initialize to an appropriate value
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
                
                Building = "building",
                CreateDate = DateTime.Now,
                CustomerDC = "customerDc",
                CustomerStore = "customer Store",
                ExportFlag = "export Flag",
                ImportDate = DateTime.Now,
                Label = "label",
                ModelTitle = "Pick Slip",
                PickslipCancelDate = DateTime.Now,
                PickslipId = 123456,
                ShippingId = "shipping Id",
                TotalQuantityOrdered = 1235,
                TotalSKUOrdered = 1204,
                //TransferDate = DateTime.Now,
                VwhId = "Vwh Id",
                AllBoxes = new Box[]{
                    new Box
                    {
                        Area = "Area",
                        Building = "building",
                        CountSku = 12,
                        CurrentPieces = 1245,
                        ExpectedPieces = 35,
                        IaId = "IaId",
                        ModelTitle = "box",
                        PalletId = "P1234556",
                        PitchingEndDate = DateTime.Now,
                        QcDate = DateTime.Now,
                        RfidTagsRequired = "Rfid",
                        Ucc128Id = "12345678901234567890",
                        VerificationDate = DateTime.Now,
                        VwhId = "VwhId",                   
                        Pickslip = new Pickslip()
                    }
                
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


                PO = new PurchaseOrder
                {
                    CancelDate = DateTime.Now,
                    CurrencyCode = "Currency",
                    Customer = new Customer
                    {
                         asn_flag = "asn_flag",
                         CarrierId = "carrierId",
                         CustomerName = "cust",
                         Category = "catagory",
                         CustomerId = "custID",
                         DefaultPickMode = "Pick  Mode",
                         MaxPiecesPerBox = 45,
                         MinPiecesPerBox = 1
                    },
                    DcCancelDate = DateTime.Now,
                    Iteration = 12,
                    OrderDate = DateTime.Now,
                    PoId = "poId",
                    PSCount = 1245,
                    StartDate = DateTime.Now,
                    
                }



            };

             //Mocking Classes
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Pickslip>>()))
                .Returns(() => Mapper.Map<IEnumerable<Pickslip>, IList<Pickslip>>(Enumerable.Repeat(expectedPickSlip, 1))).Verifiable();

             db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Box>>()))
               .Returns(() => Mapper.Map<IList<Box>, IList<Box>>(expectedPickSlip.AllBoxes)).Verifiable();

             db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()))
               .Returns(() => Mapper.Map<IList<SkuWithPieces>, IList<SkuWithPieces>>(expectedPickSlip.AllSku)).Verifiable();
             //db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<PurchaseOrder>>()))
             //  .Returns(() => Mapper.Map<PurchaseOrder,PurchaseOrder>(expectedPickSlip.PO)).Verifiable();

                var target = new DetailsController();
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);

            // Act
            var result = target.HandlePickslipScan(expectedPickSlip.PickslipId);

            // Assert

            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<Pickslip>>()), Times.Once(), "PickSlip should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<SkuWithPieces>>()), Times.Once(), "SkuWithPieces should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<Box>>()), Times.Once(), "Box should be queried exactly once");
            //db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<PurchaseOrder>>()), Times.Once(), "PO should be queried exactly once");

            // Assert For Model State
            Assert.IsTrue(target.ModelState.IsValid, "Model State Should Be Valid");


                  //Assert For Model Values
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(Pickslip));
            var psvm = (Pickslip)vr.Model;

               Assert.AreEqual(expectedPickSlip.Building,psvm.Building,"Building");
               Assert.AreEqual(expectedPickSlip.CreateDate,psvm.CreateDate,"Cretae date");
               Assert.AreEqual(expectedPickSlip.CustomerDC,psvm.CustomerDC,"Customer DC");
               Assert.AreEqual(expectedPickSlip.CustomerStore,psvm.CustomerStore,"Customer Store");
               Assert.AreEqual(expectedPickSlip.ExportFlag,psvm.ExportFlag,"Export Flag");
               Assert.AreEqual(expectedPickSlip.ImportDate,psvm.ImportDate,"Import Flag");
               Assert.AreEqual(expectedPickSlip.Label,psvm.Label,"Label");
               //Assert.AreEqual(expectedPickSlip.ModelTitle,psvm.ModelTitle,"Model Title");
               Assert.AreEqual(expectedPickSlip.PickslipCancelDate,psvm.PickslipCancelDate,"Cancel Date");
               Assert.AreEqual(expectedPickSlip.PickslipId,psvm.PickslipId,"Pick Slip Id");
               Assert.AreEqual(expectedPickSlip.ReportingStatus,psvm.ReportingStatus,"Reporting Status");
               Assert.AreEqual(expectedPickSlip.ShippingId,psvm.ShippingId,"Shippong Id");
               Assert.AreEqual(expectedPickSlip.TotalQuantityOrdered,psvm.TotalQuantityOrdered,"Quantity");
               Assert.AreEqual(expectedPickSlip.TotalSKUOrdered,psvm.TotalSKUOrdered,"Sku Ordered");
               //Assert.AreEqual(expectedPickSlip.TransferDate,psvm.TransferDate,"Transfer Date");
               //Assert.AreEqual(expectedPickSlip.UrlPickslip,psvm.UrlPickslip,"URL");
               Assert.AreEqual(expectedPickSlip.VwhId,psvm.VwhId,"Vwh ID");


            //Asserting Po Properties In PickSlip
               Assert.AreEqual(expectedPickSlip.PO.CancelDate, psvm.PO.CancelDate, "Cancel Date");
               Assert.AreEqual(expectedPickSlip.PO.CurrencyCode, psvm.PO.CurrencyCode, "Currency Code");
               Assert.AreEqual(expectedPickSlip.PO.DcCancelDate, psvm.PO.DcCancelDate, "DC Cancel Date");
               Assert.AreEqual(expectedPickSlip.PO.Iteration, psvm.PO.Iteration, "Iteration");
               Assert.AreEqual(expectedPickSlip.PO.OrderDate, psvm.PO.OrderDate, "Order Date");
               Assert.AreEqual(expectedPickSlip.PO.PoId, psvm.PO.PoId, "Po Id");
               Assert.AreEqual(expectedPickSlip.PO.PSCount, psvm.PO.PSCount, "PS Count");
               Assert.AreEqual(expectedPickSlip.PO.StartDate, psvm.PO.StartDate, "Start Date");
              // Assert.AreEqual(expectedPickSlip.PO.UrlPo, psvm.PO.UrlPo, "Url PO");
            //Asserting Customer Properties in Po
               Assert.AreEqual(expectedPickSlip.PO.Customer.asn_flag, psvm.PO.Customer.asn_flag, "asn flag");
               Assert.AreEqual(expectedPickSlip.PO.Customer.CarrierId, psvm.PO.Customer.CarrierId, "CarrierId");
               Assert.AreEqual(expectedPickSlip.PO.Customer.Category, psvm.PO.Customer.Category, "Category");
               Assert.AreEqual(expectedPickSlip.PO.Customer.CustomerId, psvm.PO.Customer.CustomerId, "CustomerId");
               Assert.AreEqual(expectedPickSlip.PO.Customer.CustomerName, psvm.PO.Customer.CustomerName, "CustomerName");
               Assert.AreEqual(expectedPickSlip.PO.Customer.DefaultPickMode, psvm.PO.Customer.DefaultPickMode, "DefaultPickMode");
               Assert.AreEqual(expectedPickSlip.PO.Customer.MaxPiecesPerBox, psvm.PO.Customer.MaxPiecesPerBox, "MaxPiecesPerBox");
               Assert.AreEqual(expectedPickSlip.PO.Customer.MinPiecesPerBox, psvm.PO.Customer.MinPiecesPerBox, "MinPiecesPerBox");
            
            
            //Asserting BOX Properties in PickSlip 
            for(int i=0; i <expectedPickSlip.AllBoxes.Count() ; ++i)
            {
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].Area,psvm.AllBoxes[i].Area,"Area");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].Building,psvm.AllBoxes[i].Building,"BUILDING");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].ModelTitle,psvm.AllBoxes[i].ModelTitle,"Model Title");
                //Assert.AreEqual(expectedPickSlip.AllBoxes[i].UrlBox,psvm.AllBoxes[i].UrlBox,"URL");
                //Assert.AreEqual(expectedPickSlip.AllBoxes[i].Verification,psvm.AllBoxes[i].Verification,"Verification");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].CountSku, psvm.AllBoxes[i].CountSku, "Count Sku");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].CurrentPieces, psvm.AllBoxes[i].CurrentPieces, "Current Pieces");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].ExpectedPieces, psvm.AllBoxes[i].ExpectedPieces, "Expected Pieces");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].IaId, psvm.AllBoxes[i].IaId, "IaId");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].PalletId, psvm.AllBoxes[i].PalletId, "PalletId");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].PitchingEndDate, psvm.AllBoxes[i].PitchingEndDate, "Pitching End Date");
                //Assert.AreEqual(expectedPickSlip.AllBoxes[i].PitchingStatus, psvm.AllBoxes[i].PitchingStatus, "Pitching Status");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].QcDate, psvm.AllBoxes[i].QcDate, "Qc Date");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].QCStatus, psvm.AllBoxes[i].QCStatus, "Qc Status");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].RfidTagsRequired, psvm.AllBoxes[i].RfidTagsRequired, "RFID");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].Ucc128Id, psvm.AllBoxes[i].Ucc128Id, "UccId");
                Assert.AreEqual(expectedPickSlip.AllBoxes[i].VwhId, psvm.AllBoxes[i].VwhId, "VwhId");
            }

             for(int i=0; i <expectedPickSlip.AllSku.Count() ; ++i)
            {
                Assert.AreEqual(expectedPickSlip.AllSku[i].Color, psvm.AllSku[i].Color, "Color");
                Assert.AreEqual(expectedPickSlip.AllSku[i].Pieces, psvm.AllSku[i].Pieces, "Current Pieces");
                Assert.AreEqual(expectedPickSlip.AllSku[i].Dimension, psvm.AllSku[i].Dimension, "Dimensions");
                Assert.AreEqual(expectedPickSlip.AllSku[i].SkuId, psvm.AllSku[i].SkuId, "Sku Id");
                Assert.AreEqual(expectedPickSlip.AllSku[i].SkuSize, psvm.AllSku[i].SkuSize, "Sku Size");
                Assert.AreEqual(expectedPickSlip.AllSku[i].Style, psvm.AllSku[i].Style, "Style");
                Assert.AreEqual(expectedPickSlip.AllSku[i].Upc, psvm.AllSku[i].Upc, "Upc");
            }
        }





    }
}