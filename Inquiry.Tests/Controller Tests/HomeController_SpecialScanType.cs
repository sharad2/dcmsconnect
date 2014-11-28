using DcmsMobile.Areas.Inquiry.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Web.Mvc;
using EclipseLibrary.Oracle;
using Moq;
using DcmsMobile.Inquiry.Models;
using System.Linq;
using DcmsMobile.Inquiry.Repositories;

namespace Inquiry.Tests
{


    /// <summary>
    /// Tests the behavior when passed scan is not recognized, or multiple scan types are discoverd.
    ///</summary>
    [TestClass()]
    public class HomeController_SpecialScanTypes
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
        /// Unrecognized scan is passed. Must redirect to Index View of Home controller
        ///</summary>
        [TestMethod()]
        [Owner("Sharad Singhal")]
        public void Home_InvalidScan()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);
            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                .Returns(() => Enumerable.Empty<ScanInfo>().ToList()).Verifiable();
            HomeController target = new HomeController(); // TODO: Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);
            string expectedScan = "12345";

            // Act
            var actual = target.InquiryIndex(expectedScan);

            // Assert
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");

            // TODO: More asserts
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        /// Unrecognized scan is passed. Must redirect to Index View of Home controller
        ///</summary>
        [TestMethod()]
        [Owner("Sharad Singhal")]
        public void Home_AmbiguousScan()
        {
            // Arrange
            Mock<IOracleDatastore> db = new Mock<IOracleDatastore>(MockBehavior.Strict);

            var scanTypes = new ScanInfo[] {
                new ScanInfo {
                    ScanType = "S1"
                },
                new ScanInfo {
                    ScanType = "S2"
                },
                new ScanInfo {
                    ScanType = "S3"
                }
            };

            db.Setup(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()))
                .Returns(() => scanTypes).Verifiable();
            HomeController target = new HomeController(); // TODO: Initialize to an appropriate value
            target.ControllerContext = new ControllerContext();
            target.Db = new HomeRepository(db.Object);
            string expectedScan = "12345";

            // Act
            var actual = target.InquiryIndex(expectedScan);

            // Assert
            db.Verify(p => p.ExecuteReader(It.IsAny<SqlBinder<ScanInfo>>()), Times.Once(), "ScanInfo should be queried exactly once");

            // TODO: More asserts
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
