using System;
using System.Collections.Generic;
using System.Web;
using DcmsMobile.Inquiry.Models;
using DcmsMobile.Inquiry.Repositories;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.DataAccess.Client;
using System.Linq;
using System.Collections;


namespace Inquiry.Tests
{


    /// <summary>
    ///This is a test class for DetailsRepositoryTest and is intended
    ///to contain all DetailsRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DetailsRepositoryTest
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
        ///A test for DetailsRepository Constructor
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        //[TestMethod()]
        //[Owner("Rajesh Kandari")]
        //[ExpectedException(typeof(ArgumentNullException))]
        /// <summary>
        ///A test for GetScanType
        ///</summary>
        //[Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_BoxPalletInfo()
        {
            //var binder = new SqlBinder<string>(" Details_BoxPalletInfo");
            const string QUERY = "select i.pallet_id, i.VWH_ID from box i where i.pallet_id IS NOT NULL AND rownum &lt; 2";
            var binder = SqlBinder.CreateAnonymous(QUERY, row => new
            {
                PalletId = row.GetValue<string>("pallet_id"),
                VwhId = row.GetValue<string>("VWH_ID")
            });
            //binder.CreateMapper("select i.pallet_id, i.VWH_ID from pallet i where rownum &lt; 2");
            var expected = _db.ExecuteSingle(binder);
            if (expected == null)
            {
                Assert.Inconclusive("No BPLT found for test");
            }
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetBoxPalletInfo(expected.PalletId);
            Assert.IsNotNull(actual, "Query must return atleast one row");
            Assert.AreEqual(expected.PalletId, actual.PalletId, "Pallet Id");
            Assert.AreEqual(expected.VwhId, actual.VwhId, "VwhId");
            Assert.Inconclusive("Query needs to be reviewed");
        }

        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_BoxPalletInventory()
        {
            const string QUERY = @"
WITH Q1 AS
 (SELECT max(B.Pallet_Id) AS Pallet_Id, COUNT(BD.UPC_CODE) AS SKU_COUNT
    FROM BOX B
    inner join boxdet bd
    on b.ucc128_id=bd.ucc128_id 
    where b.pallet_id is not null
   GROUP BY B.UCC128_ID
   ORDER BY SKU_COUNT DESC)
SELECT * FROM Q1  WHERE ROWNUM &lt; 2
";
            //var binder = new SqlBinder<object>("Repository_BoxPalletInventory");
            var binder = SqlBinder.CreateAnonymous(QUERY, row => new {
                         PalletId = row.GetValue<string>("Pallet_id"),
                         SkuCount = row.GetValue<int>("sku_count")
            });
            //string pallet_id = string.Empty;
            //int sku_count = -1;
            //binder.CreateMapper(,
            // config =>
            // {
            //     config.CreateMap<object>()
            //         .BeforeMap((src, dest) =>
            //         {
            //             pallet_id = src.GetValue<string>("Pallet_id");
            //             sku_count = src.GetValue<int>("sku_count");
            //         });
            // });
            var expected = _db.ExecuteSingle(binder);
            DetailsRepository target = new DetailsRepository(_db);

            // Act
            var actual = target.GetBoxPalletInventory(expected.PalletId);

            // Assert
            Assert.AreEqual(expected.SkuCount, actual.Count, "Count");
        }



        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetUPCInfo()
        {
            var binder = new SqlBinder<string>("  Details_GetUPCInfo");
            binder.CreateMapper("select i.UPC_CODE from master_sku i where rownum &lt; 2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No UPC_CODE found for test");
            }
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetUPCInfo(scan);
            Assert.AreEqual(scan, actual.Upc, "UPC Code Must be match");
            Assert.IsInstanceOfType(actual, typeof(Sku), "One of the scan types must be UPC_CODE");
            Assert.IsNotNull(actual, "Query must return atleast some info against valid UPC_Code");
        }



        //Class for GetSkuInventory Test
        private class AreaInventoryTest
        {
            public int SkuId { get; set; }
            public string CartonStorageArea { get; set; }
        }
        /// <summary>
        /// Select an SKU and carton area from src_carton_detail and assert that the target retrieves at least one row for the carton area.
        /// </summary>
        /// <remarks>
        /// select ctndet.sku_id, ctn.carton_storage_area from src_carton_detail ctndet
        /// inner join src_carton ctn on ctn.carton_id = ctndet.carton_id
        /// </remarks>
        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_SkuInventory()
        {
            var binder = new SqlBinder<AreaInventoryTest>("Details_SkuInventory");
            binder.CreateMapper(@"
            SELECT ctndet.sku_id as sku_id , ctn.carton_storage_area as CartonStorageArea
            FROM SRC_CARTON_DETAIL ctndet
            INNER JOIN src_carton ctn
            ON ctn.carton_id = ctndet.carton_id
            where rownum &lt; 2
",
             config =>
             {
                 config.CreateMap<AreaInventoryTest>()
                   .MapField("sku_id", dest => dest.SkuId)
                   .MapField("CartonStorageArea", dest => dest.CartonStorageArea);
             });
            var expected = _db.ExecuteSingle(binder);
            if (expected == null)
            {
                Assert.Inconclusive("No Sku-Id found for test");
            }
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetSkuInventory(expected.SkuId);
            Assert.IsNotNull(actual, "Query must return atleast some info against valid sku_id");
        }


        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetSkuLocationInfo()
        {
            var binder = new SqlBinder<string>("Details_GetSkuLocationInfo");
            binder.CreateMapper("select i.LOCATION_ID from ialoc_content i where rownum &lt; 2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No LOCATION_ID found for test");
            }
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetSkuLocationInfo(scan);
            Assert.AreEqual(scan, actual.LocationId, "Location Id Must be match");
            Assert.IsInstanceOfType(actual, typeof(SkuLocation), "One of the scan types must be LOCATION_ID");
            Assert.IsNotNull(actual, "Query must return atleast some info against valid Location_ID");
        }



        /// <summary>
        /// The target never retrieves more than 5 rows.
        /// </summary>
        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetSkuLocationInventory()
        {
            var binder = new SqlBinder<object>("Details_GetSkuLocationInventory");
            var scan = string.Empty;
            var skucount = -1;
            binder.CreateMapper(@"select ia.location_id as loc_id ,count(distinct ia.sku_id) as SkuCount
  from ialoc_content ia
 where ia.location_id =ia.location_id
 group by location_id 
",config =>
 {
     config.CreateMap<object>()
         .BeforeMap((src, dest) => {
             scan = src.GetValue<string>("loc_id");
             skucount = src.GetValue<int>("skucount");
         });
 });
            _db.ExecuteSingle(binder);
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetSkuLocationInventory(scan);
            Assert.AreEqual(skucount, actual.Count, "count doesnot match");
            Assert.AreEqual(actual.Count, Math.Min(skucount, 5), "Count");
        }
    

        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetBoxOfUcc()
        {
            var binder = new SqlBinder<string>("Details_GetBoxOfUcc");
            binder.CreateMapper("select i.ucc128_id  from box i where rownum &lt; 2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No ucc128_id found for test");
            }
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetBoxOfUcc(scan);
            Assert.IsInstanceOfType(actual, typeof(Box), "One of the scan types must be UCC_ID");
        }



        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetBoxOfEpc()
        {
            var binder = new SqlBinder<string>("Details_GetBoxOfEpc");
            binder.CreateMapper("select i.epc from boxdet_epc i where rownum &lt; 2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No ucc128_id found for test");
            }
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetBoxOfEpc(scan);
            Assert.IsInstanceOfType(actual, typeof(Box), "One of the scan types must be ucc128_id");
        }



        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetBoxesOfPickslip()
        {
            var binder = new SqlBinder<int>("Details_ GetBoxesOfPickslip");
            binder.CreateMapper("select i.PICKSLIP_ID from box_audit i where rownum &lt; 2");
            int scan = _db.ExecuteSingle(binder);
            if (scan < 0)
            {
                Assert.Inconclusive("No pickslip_id found for test");
            }
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetBoxesOfPickslip(scan);
            Assert.IsInstanceOfType(actual, typeof(IList<Box>), "One of the scan types must be PickSlip_id");
        }



        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetBoxSkuDetails()
        {
            var binder = new SqlBinder<object>("Repository_GetBoxSkuDetails");
            string UCC128_id = string.Empty;
            int sku_count = -1;
            binder.CreateMapper(@"
            WITH Q1 AS
             (SELECT BD.UCC128_ID AS UCC128_ID, COUNT(distinct(BD.UPC_CODE)) AS SKU_COUNT
                FROM BOXDET BD
               GROUP BY BD.UCC128_ID
               ORDER BY SKU_COUNT DESC)
            SELECT * FROM Q1 WHERE ROWNUM &lt; 2
        ",
             config =>
             {
                 config.CreateMap<object>()
                     .BeforeMap((src, dest) =>
                     {
                         UCC128_id = src.GetValue<string>("UCC128_id");
                         sku_count = src.GetValue<int>("sku_count");
                     });
             });
            _db.ExecuteSingle(binder);
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetBoxSkuDetails(UCC128_id, null);
            Assert.AreEqual(sku_count, actual.Count);
        }



        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetCartonInfo()
        {
            var binder = new SqlBinder<string>("Details_GetCartonInfo");
            binder.CreateMapper("select t.carton_id from src_carton t where rownum &lt; 2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No Carton_id found for test");
            }
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetCartonInfo(scan);
            Assert.IsNotNull(actual, "Query must return atleast some info against valid carton id");
            Assert.IsInstanceOfType(actual, typeof(Carton), "One of the scan types must be Carton_id");
            Assert.AreEqual(scan, actual.CartonId, "Carton Id");
        }




        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetCartonInventory()
        {
            var carton_id = string.Empty;
            int sku_count = -1;
            var binder = new SqlBinder<object>("Repository_GetCartonInventory"); ;
            binder.CreateMapper(@"
            WITH Q1 AS( SELECT PSD.Carton_Id, COUNT(*) AS NUM_SKU
            FROM SRC_CARTON_DETAIL PSD
            INNER JOIN MASTER_SKU PS
            ON psd.sku_id = ps.sku_id
            GROUP BY PSD.Carton_Id
            ORDER BY 2 DESC)
            SELECT * FROM Q1 WHERE ROWNUM &lt; 2
        ",
             config =>
             {
                 config.CreateMap<object>()
                     .BeforeMap((src, dest) =>
                     {
                         carton_id = src.GetValue<string>("carton_id");
                         sku_count = src.GetValue<int>("num_sku");
                     });
             });
            _db.ExecuteSingle(binder);
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetCartonInventory(carton_id);
            Assert.AreEqual(sku_count, actual.Count);
        }




        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetCustomerInfo()
        {
            var binder = new SqlBinder<string>("Details_GetCustomerInfo");
            binder.CreateMapper("select t.customer_id from cust t where rownum &lt; 2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No Customer_id found for test");
            }
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetCustomerInfo(scan);
            Assert.IsNotNull(actual, "Query must return atleast some info against valid customer id");
            Assert.IsInstanceOfType(actual, typeof(Customer), "One of the scan types must be Customer_id");
            Assert.AreEqual(scan, actual.CustomerId, "Customer Id");
        }




        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetPOInfo()
        {
            var binder = new SqlBinder<object>("Repository_PickSlipBoxesScan");
            string cst_id = "";
            string po_id = "";
            int iteration = -1;
            binder.CreateMapper(@"select t.po_id,t.customer_id,t.iteration from po t where rownum &lt; 2",
            config =>
            {
                config.CreateMap<object>()
                    .BeforeMap((src, dest) =>
                    {
                        cst_id = src.GetValue<string>("customer_id");
                        po_id = src.GetValue<string>("po_id");
                        iteration = src.GetValue<int>("iteration");
                    });
            });
            _db.ExecuteSingle(binder);
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetPOInfo(po_id, cst_id, iteration);
            Assert.AreEqual(cst_id, actual.Customer.CustomerId, "Customer_ID does'nt match");
            Assert.AreEqual(po_id, actual.PoId, "Po_id must be same");
        }





        [Owner("Rajesh Kandari")]
        [TestMethod]
        public void Details_GetWaveInfo()
        {
            var binder = new SqlBinder<int>("Details_GetWaveInfo");
            binder.CreateMapper("select i.bucket_ID from bucket i where rownum &lt; 2");
            int scan = _db.ExecuteSingle(binder);
            if (scan < 0)
            {
                Assert.Inconclusive("No bucket_id found for test");
            }
            DetailsRepository target = new DetailsRepository(_db);
            var actual = target.GetWaveInfo(scan);
            Assert.IsInstanceOfType(actual, typeof(Wave), "One of the scan types must be bucket_id");
            Assert.IsNotNull(actual, "Query must return atleast some info against valid bucket_id");
            Assert.AreEqual(scan, actual.BucketId, "Bucket Id must be valid");
         }
    }
}
