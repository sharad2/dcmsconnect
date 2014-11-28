using System;
using System.Collections.Generic;
using System.Web;
using DcmsMobile.Inquiry.Models;
using DcmsMobile.Inquiry.Repositories;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Dynamic;

namespace Inquiry.Tests
{
    
    
    /// <summary>
    ///This is a test class for ArchiveRepositoryTest and is intended
    ///to contain all ArchiveRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ArchiveRepositoryTest
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
        #endregion
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
   
        /// <summary>
        ///A test for GetScanType
        ///</summary>
        [Owner("Ankit")]
        [TestMethod]
        public void Repository_PickSlipInfoScan()
        {
            var binder = new SqlBinder<int?>("Repository_PickSlipInfoScan");
            binder.CreateMapper("select ps.pickslip_id from dem_pickslip_h ps where rownum &lt; 2");
            var scan = _db.ExecuteSingle(binder);
            if (scan == null)
            {
                Assert.Inconclusive("No PicSlipInfo found for test");
            }
            ArchiveRepository target = new ArchiveRepository(_db);
            //Act
            var actual = target.GetArchivePickslipInfo(Convert.ToInt32(scan));
            //Assert
            Assert.AreEqual(scan, actual.PickslipId,"PickSlip");

        }

        [Owner("Ankit")]
        [TestMethod]  
        public void Repository_PickSlipDetailScan()
        {
            var binder = new SqlBinder<object>("Repository_PickSlipInfoScan");
            var scan = -1;
            var skuCount = -1;
            binder.CreateMapper(@"WITH Q1 AS(
SELECT PSD.PICKSLIP_ID, COUNT(*) AS NUM_SKU
  FROM DEM_PICKSLIP_DETAIL_H PSD
  INNER JOIN DEM_PICKSLIP_H PS
ON PS.PICKSLIP_ID = PSD.PICKSLIP_ID
  GROUP BY PSD.PICKSLIP_ID
  ORDER BY 2 DESC)
SELECT * FROM Q1 WHERE ROWNUM &lt; 2
", config =>
 {
     config.CreateMap<object>()
         .BeforeMap((src, dest) => {
             scan = src.GetValue<int>("pickslip_id");
             skuCount = src.GetValue<int>("num_sku");
         });
 });
           _db.ExecuteSingle(binder);
           
            ArchiveRepository target = new ArchiveRepository(_db);
            var actual = target.GetArchivePickslipDetails(Convert.ToInt32(scan));
            Assert.IsNotNull(actual, "List must not be null");
            Assert.AreEqual(skuCount,actual.Count,"Count Doesnt match");

         
        }

        [Owner("Ankit")]
        [TestMethod]
        public void Repository_PickSlipBoxesScan()
        {
            var binder = new SqlBinder<ExpandoObject>("Repository_PickSlipBoxesScan");
            binder.CreateMapper(@"with q as
 (select count(distinct bh.ucc128_id) PICKSLIP_BOX_COUNT, oh.pickslip_id as PICKSLIP
  from dem_pickslip_detail_h oh
 inner join dem_box_h bh
    on oh.pickslip_id = bh.checking_id
 group by oh.pickslip_id
 order by 1 desc)
select * from q where rownum &lt; 2
", config =>
 {
     config.CreateMap<ExpandoObject>()
         .BeforeMap((src, dest) =>
         {
             dynamic x = dest;
             x.PickslipId = src.GetValue<int>("PICKSLIP");
             x.BoxCount = src.GetValue<int>("PICKSLIP_BOX_COUNT");
         });
 });
            dynamic expected = _db.ExecuteSingle(binder);

            ArchiveRepository target = new ArchiveRepository(_db);
            var actual = target.GetArchivePickslipBoxes(expected.PickslipId);
            Assert.IsNotNull(actual, "Scan cant be NULL");
            Assert.AreEqual(expected.BoxCount, actual.Count, "Number Of Boxes Dont Match for pickslip {0}", expected.PickslipId);
            
        }

        [Owner("Ankit")]
        [TestMethod]
        public void ArchiveRepository_Boxesinfo()
        {
            var binder = new SqlBinder<string>("ArchiveRepository_Boxesinfo");
            binder.CreateMapper("select y.ucc128_id  from dem_box_h y where rownum &lt; 2");
            var scan = _db.ExecuteSingle(binder);
            if (string.IsNullOrEmpty(scan))
            {
                Assert.Inconclusive("No UCC found for test");
            }

            ArchiveRepository target = new ArchiveRepository(_db);

            // Act
            var actual = target.GetArchiveBoxInfo(scan);

            // Assert
            Assert.AreEqual(scan, actual.Ucc128Id);
           
        }
        
        
        
        [Owner("Ankit")]
        [TestMethod]
        public void ArchiveRepository_BoxSkuDetail()
        {
            //var binder = new SqlBinder<object>("ArchiveRepository_BoxSkuDetail");
//            string scan = string.Empty;
//            int skuCount = -1;
//            binder.CreateMapper(@"
//WITH q1 AS
// (select b.ucc128_id, count(*) AS num_sku
//    from dem_box_h b
//   inner join dem_box_detail_h bd
//      on b.checking_id = bd.checking_id
//   group by b.ucc128_id
//   order by 2 desc)
//select * from q1 where rownum &lt; 2
//", config =>
// {
//     config.CreateMap<object>()
//         .BeforeMap((src, dest) => {
//             scan = src.GetValue<string>("ucc128_id");
//             skuCount = src.GetValue<int>("num_sku");
//         });
// });
            var binder = SqlBinder.CreateAnonymous(@"
WITH q1 AS
 (select b.ucc128_id, count(*) AS num_sku
    from dem_box_h b
   inner join dem_box_detail_h bd
      on b.checking_id = bd.checking_id
   group by b.ucc128_id
   order by 2 desc)
select * from q1 where rownum &lt; 2
", row => new
 {
     scan = row.GetValue<string>("ucc128_id"),
     skuCount = row.GetValue<int>("num_sku")
 });
            var expected = _db.ExecuteSingle(binder);
            if (expected == null)
            {
                Assert.Inconclusive("No UCC found for test");
            }

            ArchiveRepository target = new ArchiveRepository(_db);

            // Act
            var actual = target.GetArchiveBoxSkuDetails(expected.scan);
            Assert.IsNotNull(actual, "List must not be null");
            Assert.AreEqual(expected.skuCount, actual.Count, "BoxSku count");
        }

        private class BoxEpcTest
        {
            public string UccId { get; set; }
            public string EpcCode { get; set; }
        }

        [Owner("Ankit")]
        [TestMethod]
        public void ArchiveRepository_BoxEPC()
        {
            var binder = new SqlBinder<BoxEpcTest>("ArchiveRepository_BoxEPC");
            binder.CreateMapper(@"
select b.ucc128_id as ucc128_id, bde.epc as epc
  from dem_box_detail_epc_h bde
 inner join dem_box_detail_h bd
    on bd.boxdet_id = bde.boxdet_id
 inner join dem_box_h b
    on b.checking_id = bd.checking_id
 where rownum &lt; 2
", config => {
                config.CreateMap<BoxEpcTest>()
                    .MapField("ucc128_id", dest => dest.UccId)
                    .MapField("epc", dest => dest.EpcCode);
            });
            var expected = _db.ExecuteSingle(binder);
            if (expected == null)
            {
                Assert.Inconclusive("No UCC found for test");
            }
            ArchiveRepository target = new ArchiveRepository(_db);
            var actual = target.GetArchiveBoxEpc(expected.UccId);
            Assert.IsTrue(actual.Any(p => p.EpcCode == expected.EpcCode), "Box {0} must contain EPC {1}", expected.UccId, expected.EpcCode);
        }

    }
}
