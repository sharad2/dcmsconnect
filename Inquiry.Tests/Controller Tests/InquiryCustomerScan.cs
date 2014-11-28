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
    ///This is a test class for Inquiry Customer Scan and is intended
    ///to contain all Customer Scan Test Unit Tests
    ///</summary>
    [TestClass()]
    public class InquiryCustomerScan
    {


        private TestContext testContextInstance2;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance2;
            }
            set
            {
                testContextInstance2 = value;
            }
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
            Mapper.CreateMap<Customer, Customer>();
            Mapper.CreateMap<InquiryViewModel, InquiryViewModel>();
            Mapper.CreateMap<ScanInfo, ScanInfo>();
        }


         /// <summary>
        /// Tests whether Customer scan returns proper information in the model
        ///</summary>
        [TestMethod()]
        [Owner("Ankit")]
        public void InquiryCustomerScanTest_Home()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            // GetScanType
            var expectedScanInfo = new ScanInfo[] { new ScanInfo
                        {
                             ScanType = "CST",
                        }
                    };
            // GetScanType
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                .Returns(() => Mapper.Map<ScanInfo[], ScanInfo[]>(expectedScanInfo)).Verifiable();
            var target = new HomeController(); // Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);
            var expectedScan = "12345";
            var result = target.InquiryIndex(expectedScan);
            // Assert
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var rr = (RedirectToRouteResult)result;
            Assert.AreEqual(MVC_Inquiry.Inquiry.Details.ActionNames.HandleCustomerScan, rr.RouteValues["action"], "Action");
            Assert.AreEqual("Details", rr.RouteValues["controller"], "Controller");
            Assert.AreEqual(expectedScan, rr.RouteValues["id"], "Id");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey1,rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m)=>m.PrimaryKey1)], "PrimaryKey1");
            Assert.AreEqual(expectedScanInfo[0].PrimaryKey2, rr.RouteValues[ReflectionHelpers.FieldNameFor((ScanInfo m) => m.PrimaryKey2)], "PrimaryKey2");
        }
        /// <summary>
        /// Tests whether CST scan returns proper information in the model
        ///</summary>
        [TestMethod()]
        public void InquiryCustomerScanTest_Detail_ValidModel()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            //  Provide reasonable values for most properties

            var expectedCustomer = new Customer
           {
               asn_flag = "asnflag",
               CarrierId = "carriedId",
               Category = "category",
               CustomerId = "12425",
               CustomerName = "custname",
               DefaultPickMode = "defaultPickMode",
               MaxPiecesPerBox = 100,
               MinPiecesPerBox = 10,

           };
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Customer>>()))
                .Returns(() =>Mapper.Map<Customer,Customer>(expectedCustomer)).Verifiable();

            /// <summary>
            ///A test for HandleCustomerScan
            ///</summary>

            var target = new DetailsController(); // TODO: Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);
            
            // Act
            var result = target.HandleCustomerScan(expectedCustomer.CustomerId);
            // Assert
            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Customer>>()), Times.Once(), "Customer should be queried exactly once");
            //Assert For Maodel State
            Assert.IsTrue(target.ModelState.IsValid, "Model state must be valid");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            //Assert.AreEqual(vr.ViewName, "Customer");
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(Customer));
            var bvm = (Customer)vr.Model;
            Assert.AreEqual(expectedCustomer.asn_flag, bvm.asn_flag);
            Assert.AreEqual(expectedCustomer.CarrierId, bvm.CarrierId);
            Assert.AreEqual(expectedCustomer.Category, bvm.Category);
            Assert.AreEqual(expectedCustomer.CustomerId, bvm.CustomerId);
            Assert.AreEqual(expectedCustomer.CustomerName, bvm.CustomerName);
            Assert.AreEqual(expectedCustomer.DefaultPickMode, bvm.DefaultPickMode);
            Assert.AreEqual(expectedCustomer.MaxPiecesPerBox, bvm.MaxPiecesPerBox);
            Assert.AreEqual(expectedCustomer.MinPiecesPerBox, bvm.MinPiecesPerBox);

        }

        [TestMethod()]
        public void InquiryCustomerScanTest_Detail_InValidModel()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            // TODO: Provide reasonable values for most properties

            var expectedCustomer = new Customer
            {
                asn_flag = "asnflag",
                CarrierId = "carriedId",
                Category = "category",
                
                CustomerName = "custname",
                DefaultPickMode = "defaultPickMode",
                MaxPiecesPerBox = 100,
                MinPiecesPerBox = 1,
             };
            //mocking classes
            db.Setup(p => p.ExecuteSingle(It.IsAny<SqlBinder<Customer>>()))
                .Returns(() => Mapper.Map<Customer, Customer>(expectedCustomer)).Verifiable();

            /// <summary>
            ///A test for HandleCustomerScan
            ///</summary>

            var target = new DetailsController(); // TODO: Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new DetailsRepository(db.Object);
            // Act
            var result = target.HandleCustomerScan(expectedCustomer.CustomerId);
            // Assert
            db.Verify(p => p.ExecuteSingle(It.IsAny<SqlBinder<Customer>>()), Times.Once(), "Customer should be queried exactly once");
            //Assert for model state
            Assert.IsFalse(target.ModelState.IsValid, "Model state must be Invalid");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsNotNull(vr.Model);
            Assert.IsInstanceOfType(vr.Model, typeof(Customer));
            var bvm = (Customer)vr.Model;
            Assert.IsFalse(target.ModelState.IsValidField(ReflectionHelpers.FieldNameFor((Customer m) => m.CustomerId)),
             "First Customer Id must be Invalid");
            Assert.AreEqual(1, target.ModelState.Sum(p => p.Value.Errors.Count), "Number of validation errors");
        } 
    }
}
