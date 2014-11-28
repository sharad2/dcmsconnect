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
using System;
using DcmsMobile.Inquiry.Repositories;

namespace Inquiry.Tests
{
    
    
    /// <summary>
    ///This is a test class for HomeControllerTest and is intended
    ///to contain all HomeControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InquiryPOScan
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

        public static void ClassInitialize(TestContext ctx)
        {

            Mapper.CreateMap<InquiryViewModel, InquiryViewModel>();
            Mapper.CreateMap<Pickslip, Pickslip>();
            Mapper.CreateMap<PurchaseOrder, PurchaseOrder>();
            Mapper.CreateMap<Customer, Customer>();
            Mapper.CreateMap<ScanInfo, ScanInfo>();
        }


        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test whether PO Scan Test Return Expected View
        ///</summary>

        [TestMethod()]
        [Owner("Ankit")]
         public void InquiryPOScanTest_Home()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            // GetScanType
            var expectedScanInfo = new ScanInfo[] { new ScanInfo
                        {
                             ScanType = "PO",
                        }
                    };
            // GetScanType
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                .Returns(() => Mapper.Map<ScanInfo[], ScanInfo[]>(expectedScanInfo)).Verifiable();
            var target = new HomeController(); //Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);
            var expectedScan = "123456";
            var result = target.InquiryIndex(expectedScan);
            // Assert
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var rr = (RedirectToRouteResult)result;
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.ActionNames.HandlePoScan, rr.RouteValues["action"], "Action");
            Assert.AreEqual("Details", rr.RouteValues["controller"], "Controller");
            Assert.AreEqual(expectedScan, rr.RouteValues["id"], "id");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey1, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey1)], "PrimaryKey1");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey2, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey2)], "PrimaryKey2");
        }

         /// <summary>
        /// Tests whether PO scan returns proper information in the model
        ///</summary>
        [TestMethod()]
        public void InquiryPOScanTest_Detail_ValidModel()
        {
            //Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
             var expectedPO = new PurchaseOrder
            {
                CancelDate = DateTime.Now,
                CurrencyCode = "currencycode",
                DcCancelDate = DateTime.Now,
                OrderDate = DateTime.Now,
                PoId = "poid",
                PSCount = 100,
                StartDate = DateTime.Now,
                Customer=new Customer
                {
                    CustomerId = "ADFF",
                    asn_flag = "DFF",
                    CarrierId = "ADF",
                    Category = "AAD",
                    CustomerName = "AMAN",
                    DefaultPickMode = "ADF",
                    MaxPiecesPerBox = 15,
                    MinPiecesPerBox = 6,
                    Scan = "ADFG"
                },
                
                AllPickslips=new Pickslip[]{
                    new Pickslip{
                        Building="SDF",
                        CreateDate=DateTime.Now,
                        CustomerDC="ADD",
                        CustomerStore="ADH",
                        ExportFlag="ADF",
                        ImportDate=DateTime.Now,
                        Label="ADF",
                        ModelTitle="SMHK",
                        PickslipCancelDate=DateTime.Now,
                        PickslipId=1242,
                        Scan="ADF",
                        ShippingId="aAS",
                        TotalQuantityOrdered=12,
                        TotalSKUOrdered=13,
                        //TransferDate=DateTime.Now,
                        //UrlPickslip="www.gmail.com",
                        VwhId="113445",
                        PO=new PurchaseOrder
                        {
                          
                        }
                    }

                   }
                };
            //Mocking Classes

             db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<PurchaseOrder>>()))
                      .Returns(() => Mapper.Map<PurchaseOrder, PurchaseOrder>(expectedPO));
             db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Customer>>()))
                      .Returns(() => Mapper.Map<Customer, Customer>(expectedPO.Customer)).Verifiable();
             db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Pickslip>>()))
               .Returns(() => Mapper.Map<IList<Pickslip>, IList<Pickslip>>(expectedPO.AllPickslips)).Verifiable();;
             var target = new DetailsController(); //Initialize to an appropriate value
             target.ControllerContext = new ControllerContext();
             target.Db = new DetailsRepository(db.Object);
            
             var result = target.HandlePoScan(expectedPO.PoId,"pk1",3);

             // Assert
             db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<Pickslip>>()), Times.Once(), "Pick slip should be queried exactly once");
             db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<PurchaseOrder>>()), Times.Once(), "Purchase Order should be queried exactly once");
             //Assert for Model State Validation
             Assert.IsTrue(target.ModelState.IsValid, "Model Should Have A valid State");
            //Assert For Values
            Assert.IsInstanceOfType(result, typeof(ViewResult));
                var vr = (ViewResult)result;
                Assert.IsNotNull(vr.Model);
                Assert.IsInstanceOfType(vr.Model, typeof(PurchaseOrder));
                var povm1 = (PurchaseOrder)vr.Model;
                Assert.AreEqual(expectedPO.CancelDate, povm1.CancelDate, "Cancel Date");
                Assert.AreEqual(expectedPO.CurrencyCode, povm1.CurrencyCode, "Currency Code");
                Assert.AreEqual(expectedPO.DcCancelDate, povm1.DcCancelDate, "DcCancelDate");
                Assert.AreEqual(expectedPO.OrderDate, povm1.OrderDate, "OrderDate");
                Assert.AreEqual(expectedPO.PoId, povm1.PoId, "PO_Id");
                Assert.AreEqual(expectedPO.PSCount, povm1.PSCount, "PSCount");
                Assert.AreEqual(expectedPO.StartDate, povm1.StartDate, "Start Date");
                Assert.AreEqual(expectedPO.Customer.asn_flag,povm1.Customer.asn_flag,"ASn Flag");
                Assert.AreEqual(expectedPO.Customer.CarrierId, povm1.Customer.CarrierId, "Carrier Id");
                Assert.AreEqual(expectedPO.Customer.Category, povm1.Customer.Category, "Category");
                Assert.AreEqual(expectedPO.Customer.CustomerId, povm1.Customer.CustomerId, "Customer Id");
                Assert.AreEqual(expectedPO.Customer.CustomerName, povm1.Customer.CustomerName, "Customer Name");
                Assert.AreEqual(expectedPO.Customer.DefaultPickMode, povm1.Customer.DefaultPickMode, "Default Pick Mode");
                Assert.AreEqual(expectedPO.Customer.MaxPiecesPerBox, povm1.Customer.MaxPiecesPerBox, "Max Pieces");
                Assert.AreEqual(expectedPO.Customer.MinPiecesPerBox, povm1.Customer.MinPiecesPerBox, "Min pieces");
                Assert.AreEqual(expectedPO.Customer.Scan, povm1.Customer.Scan, "Scan");
                for (int i = 0; i < expectedPO.AllPickslips.Count; ++i)
                {
                    Assert.AreEqual(expectedPO.AllPickslips[i].Building, povm1.AllPickslips[i].Building, "Building");
                    Assert.AreEqual(expectedPO.AllPickslips[i].CreateDate, povm1.AllPickslips[i].CreateDate, "Create Date");
                    Assert.AreEqual(expectedPO.AllPickslips[i].CustomerDC, povm1.AllPickslips[i].CustomerDC, "Customer DC");
                    Assert.AreEqual(expectedPO.AllPickslips[i].CustomerStore, povm1.AllPickslips[i].CustomerStore, "Customer Store");
                    Assert.AreEqual(expectedPO.AllPickslips[i].ExportFlag, povm1.AllPickslips[i].ExportFlag, "Export Flag");
                    Assert.AreEqual(expectedPO.AllPickslips[i].ImportDate, povm1.AllPickslips[i].ImportDate, "Import Date");
                    Assert.AreEqual(expectedPO.AllPickslips[i].Label, povm1.AllPickslips[i].Label, "Label");
                    Assert.AreEqual(expectedPO.AllPickslips[i].ModelTitle, povm1.AllPickslips[i].ModelTitle, "Model Title");
                    Assert.AreEqual(expectedPO.AllPickslips[i].PickslipCancelDate, povm1.AllPickslips[i].PickslipCancelDate, "Pick Slip Cancel Date");
                    Assert.AreEqual(expectedPO.AllPickslips[i].PickslipId, povm1.AllPickslips[i].PickslipId, "Pick Slip Id");
                    Assert.AreEqual(expectedPO.AllPickslips[i].ReportingStatus, povm1.AllPickslips[i].ReportingStatus, "Reporting Status");
                    Assert.AreEqual(expectedPO.AllPickslips[i].Scan, povm1.AllPickslips[i].Scan, "Scan");
                    Assert.AreEqual(expectedPO.AllPickslips[i].ShippingId, povm1.AllPickslips[i].ShippingId, "Shipping Id");
                    Assert.AreEqual(expectedPO.AllPickslips[i].TotalQuantityOrdered, povm1.AllPickslips[i].TotalQuantityOrdered, "Total Quantity Ordered");
                    Assert.AreEqual(expectedPO.AllPickslips[i].TotalSKUOrdered, povm1.AllPickslips[i].TotalSKUOrdered, "Total Sku Oredered");
                    //Assert.AreEqual(expectedPO.AllPickslips[i].TransferDate, povm1.AllPickslips[i].TransferDate, "Transfer Date");
                    //Assert.AreEqual(expectedPO.AllPickslips[i].UrlPickslip, povm1.AllPickslips[i].UrlPickslip, "Url Pick Slip");
                    Assert.AreEqual(expectedPO.AllPickslips[i].VwhId, povm1.AllPickslips[i].VwhId, "VwHid");
                }
         }
          /// <summary>
        /// Tests whether PO scan returns proper information in the model
        ///</summary>
        [TestMethod()]
        public void InquiryPOScanTest_Detail_InValidModel()
        {
            //Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            var expectedPO = new PurchaseOrder
           {
               CancelDate = DateTime.Now,
               CurrencyCode = "currencycode",
               DcCancelDate = DateTime.Now,
               OrderDate = DateTime.Now,
               //Po Id is null.Thus invalid
               PoId = "",
               PSCount = 100,
               StartDate = DateTime.Now,
               Customer = new Customer
               {
                   CustomerId = "ADFF",
                   asn_flag = "DFF",
                   CarrierId = "ADF",
                   Category = "AAD",
                   CustomerName = "AMAN",
                   DefaultPickMode = "ADF",
                   MaxPiecesPerBox = 15,
                   MinPiecesPerBox = 6,
                   Scan = "ADFG"
               },

               AllPickslips = new Pickslip[]{
                    new Pickslip{
                        Building="SDF",
                        CreateDate=DateTime.Now,
                        CustomerDC="ADD",
                        CustomerStore="ADH",
                        ExportFlag="ADF",
                        ImportDate=DateTime.Now,
                        Label="ADF",
                        ModelTitle="SMHK",
                        PickslipCancelDate=DateTime.Now,
                        PickslipId=1242,
                        Scan="ADF",
                        ShippingId="aAS",
                        TotalQuantityOrdered=12,
                        TotalSKUOrdered=13,
                        //TransferDate=DateTime.Now,
                        //UrlPickslip="www.gmail.com",
                        VwhId="113445"
                    }

                   }
           };
            //Mocking Classes

            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<PurchaseOrder>>()))
                     .Returns(() => Mapper.Map<PurchaseOrder, PurchaseOrder>(expectedPO));
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Customer>>()))
                     .Returns(() => Mapper.Map<Customer, Customer>(expectedPO.Customer)).Verifiable();
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<Pickslip>>()))
              .Returns(() => Mapper.Map<IList<Pickslip>, IList<Pickslip>>(expectedPO.AllPickslips)).Verifiable(); ;
            var target = new DetailsController(); //Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);

            var result = target.HandlePoScan(expectedPO.PoId,"pk1", 3);

            // Assert

            //db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Customer>>()), Times.Once(), "Customer should be queried exactly once");
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<Pickslip>>()), Times.Once(), "Pick slip should be queried exactly once");
            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<PurchaseOrder>>()), Times.Once(), "Purchase Order should be queried exactly once");
            //Assert for Model State Validation
            Assert.IsFalse(target.ModelState.IsValid, "Model Should Be Invalid State");
            //Assert For Values
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(PurchaseOrder));
            var povm1 = (PurchaseOrder)vr.Model;
            Assert.IsFalse(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((PurchaseOrder m) => m.PoId)),
                 "Po Id must be invalid");
        }

    }
}
