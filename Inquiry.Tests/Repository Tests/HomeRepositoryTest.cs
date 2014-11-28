using System;
using System.Collections.Generic;
using System.Web;
using DcmsMobile.Inquiry.Models;
using DcmsMobile.Inquiry.Repositories;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.DataAccess.Client;
using System.Linq;

namespace Inquiry.Tests
{


    /// <summary>
    ///This is a test class for HomeRepositoryTest and is intended
    ///to contain all HomeRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class HomeRepositoryTest
    {

        const string CONNECTION_STRING = "Data Source=w8bhutan/mfdev;Persist Security Info=True;User Id=dcms8;Password=dcms8";
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

        private OracleDatastore _db;

        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            var trace = new TraceContext(HttpContext.Current);
            _db = new OracleDatastore(trace);
            _db.CreateConnection(CONNECTION_STRING, "");
        }
        //
        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            _db.Dispose();
        }
        //
        #endregion


        /// <summary>
        ///A test for GetScanType
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [Owner("Sharad Singhal")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetScanTypeTest_EmptyScan()
        {
            HomeRepository target = new HomeRepository(_db); // TODO: Initialize to an appropriate value
            string scan = string.Empty; // TODO: Initialize to an appropriate value
            var actual = target.GetScanType(scan);

        }

        /// <summary>
        ///A test for GetScanType
        ///</summary>
        [Owner("Sharad Singhal")]
        [TestMethod]
        public void HomeRepository_CartonScan()
        {
            var binder = new SqlBinder<string>("HomeRepository_CartonScan");
            binder.CreateMapper("select t.carton_id from src_carton t where rownum &lt; 2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No carton found for test");
            }

            HomeRepository target = new HomeRepository(_db); // TODO: Initialize to an appropriate value

            var actual = target.GetScanType(scan);
            Assert.IsTrue(actual.Any(p => p.ScanType == "CTN"), "One of the scan types must be CTN");
        }

        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void HomeRepository_POScan()
        {
            var binder = new SqlBinder<string>("HomeRepository_POScan");
            binder.CreateMapper("select i.PO_ID from PO i where rownum &lt;2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No PO found for test");
            }

            HomeRepository target = new HomeRepository(_db);

            var actual = target.GetScanType(scan);
            Assert.IsTrue(actual.Any(p => p.ScanType == "PO"), "One of the scan types must be PO");
        }

        [Owner("Ankit")]
        [TestMethod]
        public void HomeRepository_UCCScan()
        {
            var binder = new SqlBinder<string>("HomeRepository_UCCScan");
            binder.CreateMapper("select y.ucc128_id  from box y where rownum &lt; 2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No UCC found for test");
            }

            HomeRepository target = new HomeRepository(_db);
            var actual = target.GetScanType(scan);
            Assert.IsTrue(actual.Any(p => p.ScanType == "UCC"), "One of the scan types must be UCC");
        }

        [Owner("Ankit")]
        [TestMethod]
        public void HomeRepository_EpcScan()
        {
            var binder = new SqlBinder<string>("HomeRepository_EPCScan");
            binder.CreateMapper("select t.epc from boxdet_epc t where rownum &lt; 2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No EPC found for test");
            }

            HomeRepository target = new HomeRepository(_db);
            var actual = target.GetScanType(scan);
            Assert.IsTrue(actual.Any(p => p.ScanType == "EPC"), "One of the scan types must be EPC");
        }

        [Owner("Ankit")]
        [TestMethod]
        public void HomeRepository_UPCScan()
        {
            var binder = new SqlBinder<string>("HomeRepository_UPCScan");
            binder.CreateMapper("select t.upc_code from master_sku t where rownum &lt;2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No UPC found for test");
            }
            HomeRepository target = new HomeRepository(_db);
            var actual = target.GetScanType(scan);
            Assert.IsTrue(actual.Any(p => p.ScanType == "UPC"), "One of the scan types must be UPC");
        }

        [Owner("Ankit")]
        [TestMethod]
        public void HomeRepository_PickSlipScan()
        {
            var binder = new SqlBinder<int?>("HomeRepository_PickSlip");
            binder.CreateMapper("select t.pickslip_id from ps t where rownum &lt;2");
            var scan = _db.ExecuteSingle(binder);
            if (scan == null)
            {
                Assert.Inconclusive("No PicSlip found for test");
            }
            HomeRepository target = new HomeRepository(_db);
            var actual = target.GetScanType(scan.ToString());
            Assert.IsTrue(actual.Any(p => p.ScanType == "PS"), "One of the scan types must be PickSlip");
        }
        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void HomeRepository_CustomerScan()
        {
            var binder = new SqlBinder<string>("HomeRepository_CustomerScan");
            binder.CreateMapper("select i.CUSTOMER_ID from cust i where rownum &lt;2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No Customer found for test");
            }
            HomeRepository target = new HomeRepository(_db);

            var actual = target.GetScanType(scan);
            Assert.IsTrue(actual.Any(p => p.ScanType == "CST"), "One of the scan types must be CST");
        }
        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void HomeRepository_PalletScan()
        {
            var binder = new SqlBinder<string>("HomeRepository_PalletScan");
            binder.CreateMapper("select i.PALLET_ID from pallet i where rownum &lt;2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No Pallet found for test");
            }
            HomeRepository target = new HomeRepository(_db);
            var actual = target.GetScanType(scan);
            Assert.IsTrue(actual.Any(p => p.ScanType == "BPLT"), "One of the scan types must be BPLT");
        }
        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void HomeRepository_SkuLocationScan()
        {
            var binder = new SqlBinder<string>("HomeRepository_SkuLocationScan");
            binder.CreateMapper("select i.LOCATION_ID from ialoc_content i where rownum &lt;2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No SkuLocation found for test");
            }
            HomeRepository target = new HomeRepository(_db);
            var actual = target.GetScanType(scan);
            Assert.IsTrue(actual.Any(p => p.ScanType == "SA"), "One of the scan types must be SA");
        }
        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void HomeRepository_WaveScan()
        {
            var binder = new SqlBinder<int?>("HomeRepository_WaveScan");
            binder.CreateMapper("select i.BUCKET_ID from bucket i where rownum &lt;2");
            var scan = _db.ExecuteSingle(binder);
            if (scan == null)
            {
                Assert.Inconclusive("No Wave found for test");
            }
            HomeRepository target = new HomeRepository(_db);
            var actual = target.GetScanType(scan.ToString());
            Assert.IsTrue(actual.Any(p => p.ScanType == "WAV"), "One of the scan types must be SA");
        }
    }
}
