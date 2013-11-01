using DcmsMobile.REQ2.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EclipseLibrary.Oracle;
using DcmsMobile.REQ2.Models;
using System.Data.Common;
using System.Linq;
using System;

namespace REQ2.Test
{


    /// <summary>
    ///This is a test class for ReqRepositoryTest and is intended
    ///to contain all ReqRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ReqRepositoryTest
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

        private static OracleDatastore _db;

        //You can use the following additional attributes as you write your tests:

        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            _db = new OracleDatastore(null);
            _db.CreateConnection("Data Source=w8bhutan/mfdev;Proxy User Id=dcms8;Proxy Password=DDM", "");
        }


        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            _db.Dispose();
        }
        private ReqRepository target;
        private DbTransaction _trans;

        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            target = new ReqRepository(_db);
            _trans = _db.BeginTransaction();
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            _trans.Rollback();
            _trans.Dispose();
        }

        #endregion

        /// <summary>
        /// This Function Verifies the data returned from repositories function GetRequestInfo().
        /// In this function first we fetch a random carton Reserve Id from CTNRESV table.
        /// Then we query info for that carton request id from CTNRESV table and then call the function GetRequestInfo()
        /// and pass the fetched carton Reserve ID as parameter and the we asserted the values returned 
        /// from our query and repository's function.
        /// 
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void RequestInfoTest()
        {

            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                    WITH Q AS
                     (SELECT C.CTN_RESV_ID AS CTN_RESV_ID                         
                        FROM CTNRESV C 
                            WHERE C.MODULE_CODE = 'REQ2' 
                            ORDER BY DBMS_RANDOM.VALUE)
                    SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
       {
           CartonReserveId = row.GetValue<string>("CTN_RESV_ID")
       }).ExecuteSingle(_db);


            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No Carton Reserve Id Found");
            }

            //querying data for fetched carton reserve Id
            var expectedRequest = SqlBinder.Create(
                @"
                    <![CDATA[
                    SELECT     C.VWH_ID                AS SOURCE_VWH_ID,
                               C.CTN_RESV_ID           AS CTN_RESV_ID,
                               C.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                               C.CONVERSION_VWH_ID     AS CONVERSION_VWH_ID,
                               C.SEWING_PLANT_CODE     AS SEWING_PLANT_CODE,
                               C.SOURCE_AREA           AS SOURCE_AREA,
                               C.DESTINATION_AREA      AS DESTINATION_AREA,
                               C.QUALITY_CODE          AS QUALITY_CODE,
                               C.ASSIGNED_FLAG         AS ASSIGNED_FLAG,
                               C.PRIORITY              AS PRIORITY,
                               C.PACKAGING_PREFERENCE  AS PACKAGING_PREFERENCE,
                               C.PRICE_SEASON_CODE     AS PRICE_SEASON_CODE,
                               C.SALE_TYPE_ID          AS SALE_TYPE_ID,
                               C.RECEIVE_DATE          AS RECEIVE_DATE,
                               C.REMARKS               AS REMARKS,
                               C.PIECES_CONSTRAINT     AS OVERPULLING
                    FROM CTNRESV C
                    WHERE C.CTN_RESV_ID = :CTN_RESV_ID                      
            ]]>
    ", row => new
  {
      ctnresvId = row.GetValue<string>("CTN_RESV_ID"),
      SourceVwh = row.GetValue<string>("SOURCE_VWH_ID"),
      WhId = row.GetValue<string>("WAREHOUSE_LOCATION_ID"),
      ConversionVwhId = row.GetValue<string>("CONVERSION_VWH_ID"),
      SweingPlant = row.GetValue<string>("SEWING_PLANT_CODE"),
      SourceArea = row.GetValue<string>("SOURCE_AREA"),
      DestArea = row.GetValue<string>("DESTINATION_AREA"),
      Quality = row.GetValue<string>("QUALITY_CODE"),
      AssignedFlag = row.GetValue<string>("ASSIGNED_FLAG"),
      Priority = row.GetValue<string>("PRIORITY"),
      OverPulling = row.GetValue<string>("OVERPULLING"),
      PackagingPreference = row.GetValue<string>("PACKAGING_PREFERENCE"),
      PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
      SaleType = row.GetValue<string>("SALE_TYPE_ID"),
      ReceiveDate = row.GetValue<DateTime?>("RECEIVE_DATE"),
      Remarks = row.GetValue<string>("REMARKS")
  }).Parameter("CTN_RESV_ID", expectedCartonReserveId.CartonReserveId)
  .ExecuteSingle(_db);

            if (expectedRequest == null)
            {
                Assert.Inconclusive("No info found for provided carton Reserve Id");
            }

            //calling repository function
            var actualRequest = target.GetRequests(expectedCartonReserveId.CartonReserveId, 1);

            Assert.IsNotNull(actualRequest, "No Info found for id");


            //assert
            foreach (var item in actualRequest)
            {
                Assert.AreEqual(!string.IsNullOrEmpty(expectedRequest.AssignedFlag), item.AssignedFlag, "Assigned Flag");
                Assert.AreEqual(expectedRequest.DestArea, item.DestinationArea, "Destination Area");
                Assert.AreEqual(expectedRequest.OverPulling, item.AllowOverPulling, "Allow Over Pulling");
                Assert.AreEqual(expectedRequest.PackagingPreference, item.PackagingPreferance, "Packaging Preferences");
                Assert.AreEqual(expectedRequest.PriceSeasonCode, item.PriceSeasonCode, "Price Season Code");
                Assert.AreEqual(expectedRequest.Priority, item.Priority, "Priority");
                Assert.AreEqual(expectedRequest.Quality, item.QualityCode, "Quality Code");
                Assert.AreEqual(expectedRequest.ReceiveDate, item.CartonReceivedDate, "Received Date");
                Assert.AreEqual(expectedRequest.Remarks, item.Remarks, "Remarks");
                Assert.AreEqual(expectedRequest.SweingPlant, item.SewingPlantCode, "Swewing Plant");
                Assert.AreEqual(expectedRequest.SourceArea, item.SourceAreaId, "Source Area");
                Assert.AreEqual(expectedRequest.WhId, item.BuildingId, "Building");
                Assert.AreEqual(expectedRequest.SourceVwh, item.SourceVwhId, "Source VwhId");
                Assert.AreEqual(expectedRequest.SaleType, item.SaleTypeId, "SaleType");
                Assert.AreEqual(expectedRequest.ConversionVwhId, item.TargetVwhId, "Target VwhId");
            }

        }


        /// <summary>
        /// This Function Verifies the data returned from repositories function GetRequestInfo().
        /// Here we passed null as ctnresvId in function and checks it does not return null.
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void RequestInfoNullIdTest()
        {

            //calling repository function
            var actualRequest = target.GetRequests(null, 1);

            Assert.IsNotNull(actualRequest, "No Info found for id");

        }




        /// <summary>
        /// This function verifies the data returned from repositories function GetAssignedCartons()s.
        /// In this function first we fetch a random carton reserve id from CTNRESV table where module code is REQ2.
        /// Then we query database for number of cartons assigned for that request and then we call repositories function
        /// GetAssignedCartons() and pass the fetched carton reserve id as parameter.
        /// At last we asserted data returned form our query and repositories function GetAssignedCartons().
        /// 
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Databse")]
        public void AssignedCartonsTest()
        {

            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                    WITH Q AS
                     (SELECT C.CTN_RESV_ID AS CTN_RESV_ID
                        FROM CTNRESV C
                       WHERE C.MODULE_CODE = 'REQ2'
                       ORDER BY DBMS_RANDOM.VALUE)
                    SELECT * FROM Q WHERE ROWNUM < 2
             ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No reserve Id found");
            }



            //Fetching expected cartons info
            var expectedCartonInfo = SqlBinder.Create(
                @"
<![CDATA[
SELECT CTNDET.STYLE AS STYLE,
       CTNDET.COLOR AS COLOR,
       CTNDET.DIMENSION AS DIMENSION,
       CTNDET.SKU_SIZE AS SKU_SIZE,
       CTNDET.SKU_ID AS SKU_ID,
       COUNT(CTN.CARTON_ID) AS NUMBER_OF_CARTON,
       SUM(CTNDET.QUANTITY) AS NUMBER_OF_PIECES,
       COUNT(DECODE(CTN.CARTON_STORAGE_AREA,
                    C.SOURCE_AREA,
                    NULL,
                    CTN.CARTON_ID)) AS NUMBER_OF_PULLEDCARTONS,
       COUNT(DECODE(CTN.CARTON_STORAGE_AREA,
                    C.SOURCE_AREA,
                    0,
                    CTNDET.QUANTITY)) AS NUMBER_OF_PULLEDPIECES
  FROM CTNRESV C
 INNER JOIN SRC_CARTON_DETAIL CTNDET
    ON C.DCMS4_REQ_ID = CTNDET.REQ_PROCESS_ID
 INNER JOIN SRC_CARTON CTN
    ON CTN.CARTON_ID = CTNDET.CARTON_ID
 WHERE C.CTN_RESV_ID = :CTN_RESV_ID
 GROUP BY CTNDET.STYLE,
          CTNDET.COLOR,
          CTNDET.DIMENSION,
          CTNDET.SKU_SIZE,
          CTNDET.SKU_ID
 ORDER BY CTNDET.STYLE,
          CTNDET.COLOR,
          CTNDET.DIMENSION,
          CTNDET.SKU_SIZE,
          CTNDET.SKU_ID
]]>
", row => new
 {
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     Size = row.GetValue<string>("SKU_SIZE"),
     TotalCartons = row.GetValue<int>("NUMBER_OF_CARTON"),
     PulledCartons = row.GetValue<int>("NUMBER_OF_PULLEDCARTONS"),
     TotalQuantity = row.GetValue<int>("NUMBER_OF_PIECES"),
     PulledQuantity = row.GetValue<int>("NUMBER_OF_PULLEDPIECES"),
     SkuID = row.GetValue<int>("SKU_ID")
 }).Parameter("CTN_RESV_ID", expectedCartonReserveId.CartonReserveId)
 .ExecuteSingle(_db);

            if (expectedCartonInfo == null)
            {
                Assert.Inconclusive("No Carton Info Found");
            }


            //calling repositories function
            var actualCartonInfo = target.GetAssignedCartons(expectedCartonReserveId.CartonReserveId);

            Assert.IsNotNull(actualCartonInfo, "No row returned by query");


            //asserting
            foreach (var item in actualCartonInfo)
            {
                Assert.AreEqual(expectedCartonInfo.Style, item.Sku.Style, "Style");
                Assert.AreEqual(expectedCartonInfo.Color, item.Sku.Color, "Color");
                Assert.AreEqual(expectedCartonInfo.Dimension, item.Sku.Dimension, "Dimension");
                Assert.AreEqual(expectedCartonInfo.Size, item.Sku.SkuSize, "SkuSize");
                Assert.AreEqual(expectedCartonInfo.SkuID, item.Sku.SkuId, "SkuId");
                Assert.AreEqual(expectedCartonInfo.TotalCartons, item.TotalCartons, "TotalCartons");
                Assert.AreEqual(expectedCartonInfo.TotalQuantity, item.TotalPieces, "TotalPieces");
                Assert.AreEqual(expectedCartonInfo.PulledCartons, item.PulledCartons, "PulledCartons");
                Assert.AreEqual(expectedCartonInfo.PulledQuantity, item.PulledPieces, "PulledPieces");
            }
        }


        /// <summary>
        /// This function verifies the data returned from repositories function GetRecentSkus().
        /// In this function first we fetch a random carton reserve id from CTNRESV table.
        /// Then we query database for number of sku's against that carton reserve Id and then we call repositories function
        /// GetRecentSkus() and pass the fetched carton reserve id as parameter.
        /// At last we asserted data returned form our query and repositories function GetRecentSkus(). 
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void RecentSkuTest()
        {
            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                    WITH Q AS
                     (SELECT C.CTN_RESV_ID AS CTN_RESV_ID FROM CTNRESV C
                                   WHERE C.MODULE_CODE = 'REQ2' 
                        ORDER BY DBMS_RANDOM.VALUE)
                    SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No carton reservation Id found");
            }

            //fetching sku for fetched carton reserve id
            var expectesSkus = SqlBinder.Create(
                @"
<![CDATA[
   SELECT  MAX(B.STYLE)                 AS STYLE,
           MAX(B.COLOR)                AS COLOR,
           MAX(B.DIMENSION)            AS DIMENSION,
           MAX(B.SKU_SIZE)             AS SKU_SIZE,
           MAX(B.CONVERSION_STYLE)     AS CONVERSION_STYLE,
           MAX(B.CONVERSION_COLOR)     AS CONVERSION_COLOR,
           MAX(B.CONVERSION_DIMENSION) AS CONVERSION_DIMENSION,
           MAX(B.CONVERSION_SKU_SIZE)  AS CONVERSION_SKU_SIZE,
           MAX(MSKU.SKU_ID)            AS SKU_ID,
           MAX(CON_MSKU.SKU_ID)        AS CONVERSION_SKUID
  FROM CTNRESV C
  INNER JOIN SRC_REQ_DETAIL B
    ON C.DCMS4_REQ_ID = B.REQ_PROCESS_ID
    AND C.MODULE_CODE = B.REQ_MODULE_CODE
  LEFT OUTER JOIN MASTER_SKU MSKU
    ON B.STYLE = MSKU.STYLE
   AND B.COLOR = MSKU.COLOR
   AND B.DIMENSION = MSKU.DIMENSION
   AND B.SKU_SIZE = MSKU.SKU_SIZE
  LEFT OUTER JOIN MASTER_SKU CON_MSKU
    ON B.CONVERSION_STYLE = CON_MSKU.STYLE
   AND B.CONVERSION_COLOR = CON_MSKU.COLOR
   AND B.CONVERSION_DIMENSION = CON_MSKU.DIMENSION
   AND B.CONVERSION_SKU_SIZE = CON_MSKU.SKU_SIZE
 WHERE C.CTN_RESV_ID = :CTN_RESV_ID 
GROUP BY B.REQ_PROCESS_ID, B.REQ_LINE_NUMBER
]]>
", row => new
 {
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     Size = row.GetValue<string>("SKU_SIZE"),
     ConversionStyle = row.GetValue<string>("CONVERSION_STYLE"),
     ConversionColor = row.GetValue<string>("CONVERSION_COLOR"),
     ConversionDimension = row.GetValue<string>("CONVERSION_DIMENSION"),
     ConversionSize = row.GetValue<string>("CONVERSION_SKU_SIZE"),
     SkuID = row.GetValue<int?>("SKU_ID"),
     ConversionSkuID = row.GetValue<int?>("CONVERSION_SKUID")
 }).Parameter("CTN_RESV_ID", expectedCartonReserveId.CartonReserveId);

            var x = _db.ExecuteReader(expectesSkus);
            if (x.Count() == 0)
            {
                Assert.Inconclusive("No sku  found");
            }

            //calling repositories function
            var actualSkus = target.GetRequestSkus(expectedCartonReserveId.CartonReserveId);

            Assert.AreNotEqual(actualSkus.Count(), 0, "No row returned by repositories program");


            //asserting
            int i = 0;
            foreach (var item in actualSkus)
            {
                Assert.AreEqual(x[i].Style, item.SourceSku.Style, "Style");
                Assert.AreEqual(x[i].Color, item.SourceSku.Color, "Color");
                Assert.AreEqual(x[i].Dimension, item.SourceSku.Dimension, "Dimension");
                Assert.AreEqual(x[i].Size, item.SourceSku.SkuSize, "Size");
                Assert.AreEqual(x[i].ConversionStyle, item.TargetSku.Style, "Target Style ");
                Assert.AreEqual(x[i].ConversionColor, item.TargetSku.Color, "Target Color");
                Assert.AreEqual(x[i].ConversionDimension, item.TargetSku.Dimension, "Target Dimension ");
                Assert.AreEqual(x[i].ConversionSize, item.TargetSku.SkuSize, "Target Size ");
                Assert.AreEqual(x[i].SkuID, item.SourceSku.SkuId, "Sku ID ");
                Assert.AreEqual(x[i].ConversionSkuID, item.TargetSku.SkuId, "Target Sku Id");


                i++;
            }
        }



        /// <summary>
        /// This function verifies the DeleteCartonRequest() function of repository that deletes the respected carton against 
        /// specified request.
        /// In this function first we fetch a random carton reservation id and Request Id from ctnresv table where assigned flag is null.
        /// Then we call repositories function DeleteCartonRequest() and pass the fetched carton reserve id as parameter.
        ///After all this we query database tables src_req_detail and ctnresv to check the entery of fetched reserved carton is deleted.
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void DeleteCartonTest()
        {
            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                  WITH Q AS
                 (SELECT A.CTN_RESV_ID  AS CTN_RESV_ID,
                         A.DCMS4_REQ_ID AS DCMS4_REQ_ID
                    FROM CTNRESV A
                   WHERE A.ASSIGNED_FLAG IS NULL
                      AND  A.MODULE_CODE = 'REQ2' 
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2
             ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
                 RequestId = row.GetValue<int>("DCMS4_REQ_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No carton reserve id found");
            }



            //calling repositories function
            target.DeleteCartonRequest(expectedCartonReserveId.CartonReserveId);


            //Querying database against fetched req_process_id.
            var srcReqDet = SqlBinder.Create(
                @"
                <![CDATA[   
                SELECT COUNT(*) AS COUNT FROM SRC_REQ_DETAIL C WHERE C.REQ_PROCESS_ID = :REQ_PROCESS_ID
                ]]>
                ", row => new
                 {
                     Count = row.GetValue<int>("COUNT")
                 }).Parameter("REQ_PROCESS_ID", expectedCartonReserveId.RequestId)
                 .ExecuteSingle(_db);


            Assert.AreEqual(srcReqDet.Count, 0, "Does not deleted from src_req_detail table");



            //Querying database against fetched carton reserve id.
            var ctnResv = SqlBinder.Create(
                @"
                <![CDATA[   
               SELECT COUNT(*) AS COUNT FROM CTNRESV C WHERE C.CTN_RESV_ID = :CTN_RESV_ID
                ]]>
                ", row => new
                 {
                     Count = row.GetValue<int>("COUNT")
                 }).Parameter("CTN_RESV_ID", expectedCartonReserveId.CartonReserveId)
                 .ExecuteSingle(_db);

            Assert.AreEqual(ctnResv.Count, 0, "Does not deleted from ctnresv table");

        }


        /// <summary>
        /// This function verifies the DeleteSkuFromRequest() function of repository that deletes the respective sku against 
        /// specified request.
        /// In this function first we fetch a random carton reserve id and Request Id from CTNRESV table where assigned flag is 'Y'.
        /// Then we query the sku details(style,color,dimension,size) from src_req_detail table against fetched Request Id.
        /// Then we query the skuId from master_sku table against fetched sku details.
        /// Then we call repositories function DeleteSkuFromRequestTest() and pass the fetched carton reserve id and skuId as parameter.
        ///After all this we query database tables src_req_detail to check that entery of fetched Sku is deleted.
        /// </summary>
        [TestMethod]
        [Owner("Anit")]
        [TestCategory("Database")]
        public void DeleteSkuFromRequestTest()
        {
            //fetching carton reserve id and request id
            var expectedCartonReserve = SqlBinder.Create(
               @"
            <![CDATA[
                  WITH Q AS
                 (SELECT A.CTN_RESV_ID  AS CTN_RESV_ID,
                         A.DCMS4_REQ_ID AS DCMS4_REQ_ID
                    FROM CTNRESV A
                   WHERE A.ASSIGNED_FLAG = 'Y'                    
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2
             ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
                 RequestId = row.GetValue<int>("DCMS4_REQ_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserve == null)
            {
                Assert.Inconclusive("No Carton Reservatio Id found");
            }


            //fetching sku against fetched request id
            var expectedSku = SqlBinder.Create(
                @"
                <![CDATA[
                WITH Q AS
                 (SELECT B.STYLE     AS STYLE,
                         B.COLOR     AS COLOR,
                         B.DIMENSION AS DIMENSION,
                         B.SKU_SIZE  AS SKU_SIZE
                    FROM SRC_REQ_DETAIL B
                   WHERE B.REQ_PROCESS_ID = :REQ_PROCESS_ID
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2

                ]]>
                ", row => new
                 {
                     Style = row.GetValue<string>("STYLE"),
                     Color = row.GetValue<string>("COLOR"),
                     Dimension = row.GetValue<string>("DIMENSION"),
                     Size = row.GetValue<string>("SKU_SIZE")
                 }).Parameter("REQ_PROCESS_ID", expectedCartonReserve.RequestId).ExecuteSingle(_db);

            if (expectedSku == null)
            {
                Assert.Inconclusive("No sku found against request Id");
            }

            //querying skuId against sku details
            var expectedSkuId = SqlBinder.Create(
                 @"
                <![CDATA[

                SELECT MSKU.SKU_ID AS SKU_ID
                  FROM MASTER_SKU MSKU
                 WHERE MSKU.STYLE = :STYLE
                   AND MSKU.COLOR = :COLOR
                   AND MSKU.DIMENSION = :DIMENSION
                   AND MSKU.SKU_SIZE = :SKU_SIZE
                ]]>
                ", row => new
                 {
                     SkuID = row.GetValue<int>("SKU_ID")
                 }).Parameter("STYLE", expectedSku.Style)
                 .Parameter("COLOR", expectedSku.Color)
                 .Parameter("DIMENSION", expectedSku.Dimension)
                 .Parameter("SKU_SIZE", expectedSku.Size)
                 .ExecuteSingle(_db);

            if (expectedSkuId == null)
            {
                Assert.Inconclusive("No skuid found against sku details");
            }

            //calling repositories function
            target.DeleteSkuFromRequest(expectedSkuId.SkuID, expectedCartonReserve.CartonReserveId);

            //querying the entery of sku is deleted or not
            var srcReq = SqlBinder.Create(
                @"
<![CDATA[
SELECT COUNT(*) AS COUNT
  FROM SRC_REQ_DETAIL B
 WHERE B.REQ_PROCESS_ID = :REQ_PROCESS_ID
   AND B.STYLE = :STYLE
   AND B.COLOR = :COLOR
   AND B.DIMENSION = :DIMENSION
   AND B.SKU_SIZE = :SKU_SIZE
]]>
", row => new
     {
         Count = row.GetValue<int>("COUNT")
     }).Parameter("REQ_PROCESS_ID", expectedCartonReserve.RequestId)
     .Parameter("STYLE", expectedSku.Style)
     .Parameter("COLOR", expectedSku.Color)
     .Parameter("DIMENSION", expectedSku.Dimension)
     .Parameter("SKU_SIZE", expectedSku.Size)
     .ExecuteSingle(_db);


            //asserting that the sku is deleted

            Assert.AreEqual(srcReq.Count, 0, "Does not deleted from src_req_detail");

        }


        /// <summary>
        /// This function verifies the CreateCartonRequest() function of the repository that creates a request 
        /// In this function first we fetch a random carton reservation id from ctnresv table where assigned flag is null
        /// Then we fetch a random Source and destination area from tab_inventory_area table , and then a random virtual warehouse
        /// from tab_virtual_warehouse table to poppulate the RequestModel.
        /// After this we call repositories function CreateCartonRequest() while passing RequestModel as parameter in it and 
        /// this will create a request against provided carton reservation Id.       
        /// Then we verifies the entries in table ctnresv and src_req_detail against fetched carton reservation id and request Id. 
        /// 
        /// </summary>
        [TestMethod]
        [Owner("Anit")]
        [TestCategory("Database")]
        public void CreateRequestWithId()
        {
            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                    WITH Q AS
                 (SELECT A.CTN_RESV_ID  AS CTN_RESV_ID,
                         A.DCMS4_REQ_ID AS DCMS4_REQ_ID
                    FROM CTNRESV A
                   WHERE A.ASSIGNED_FLAG IS NULL
                    AND A.MODULE_CODE = 'REQ2'
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
                 RequestId = row.GetValue<int>("DCMS4_REQ_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No Carton Found");
            }

            //fetching source area
            var expectedSourceArea = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA
    FROM TAB_INVENTORY_AREA TIA  ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     Area = row.GetValue<string>("INVENTORY_STORAGE_AREA")
 }).ExecuteSingle(_db);

            if (expectedSourceArea.Area == null)
            {
                Assert.Inconclusive("No Area Found");
            }

            //fetching destination area
            var expectedDestArea = SqlBinder.Create(
       @"
<![CDATA[
WITH Q AS
 (SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA
    FROM TAB_INVENTORY_AREA TIA  ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     Area = row.GetValue<string>("INVENTORY_STORAGE_AREA")
 }).ExecuteSingle(_db);

            if (expectedDestArea.Area == null)
            {
                Assert.Inconclusive("No Dest Area Found");
            }

            var expectedVirtualWarehouse = SqlBinder.Create(
               @"
<![CDATA[
WITH Q AS
 (SELECT TVW.VWH_ID AS VWH_ID
    FROM TAB_VIRTUAL_WAREHOUSE TVW
   ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     Id = row.GetValue<string>("VWH_ID")
 }).ExecuteSingle(_db);

            if (expectedVirtualWarehouse.Id == null)
            {
                Assert.Inconclusive("No vwh Found");
            }

            var request = new RequestModel
            {
                CtnResvId = expectedCartonReserveId.CartonReserveId,
                SourceAreaId = expectedSourceArea.Area,
                DestinationArea = expectedDestArea.Area,
                Priority = "5",
                AllowOverPulling = "O",
                SourceVwhId = expectedVirtualWarehouse.Id
            };

            target.CreateCartonRequest(request);

            var actualRequest = SqlBinder.Create(@"
<![CDATA[
SELECT C.CTN_RESV_ID       AS CTN_RESV_ID,
       C.PRIORITY          AS PRIORITY,
       C.PIECES_CONSTRAINT AS PIECES_CONSTRAINT,
       C.SOURCE_AREA       AS SOURCE_AREA,
       C.DESTINATION_AREA  AS DESTINATION_AREA,
       C.VWH_ID            AS VWH_ID,
       C.DCMS4_REQ_ID      AS DCMS4_REQ_ID
FROM CTNRESV C
 WHERE C.CTN_RESV_ID = :CTN_RESV_ID
]]>
", row => new
 {
     ReservationId = row.GetValue<string>("CTN_RESV_ID"),
     Priority = row.GetValue<string>("PRIORITY"),
     PiecesConstraints = row.GetValue<string>("PIECES_CONSTRAINT"),
     SourceArea = row.GetValue<string>("SOURCE_AREA"),
     DestArea = row.GetValue<string>("DESTINATION_AREA"),
     VwhID = row.GetValue<string>("VWH_ID"),
     RequestId = row.GetValue<int>("DCMS4_REQ_ID")
 }).Parameter("CTN_RESV_ID", expectedCartonReserveId.CartonReserveId)
 .ExecuteSingle(_db);


            Assert.IsNotNull(actualRequest, "No Request Found");

            Assert.AreEqual(expectedCartonReserveId.CartonReserveId, actualRequest.ReservationId, "Carton Reservation Id");
            Assert.AreEqual(expectedDestArea.Area, actualRequest.DestArea, "Destination Area");
            Assert.AreEqual(request.AllowOverPulling, actualRequest.PiecesConstraints, "OverPulling");
            Assert.AreEqual(request.Priority, actualRequest.Priority, "Priority");
            Assert.AreEqual(expectedSourceArea.Area, actualRequest.SourceArea, "Source Area");
            Assert.AreEqual(expectedVirtualWarehouse.Id, actualRequest.VwhID, "Virtual WareHouse");

        }





        /// <summary>
        /// This function verifies the CreateCartonRequest() function of the repository that creates a  request. 
        /// 
        /// In this we fetch random Source and  destination area from tab_inventory_area table and a random virtual warehouse
        /// from tab_virtual_warehouse table to poppulate the RequestModel.
        /// After this we call repositories function CreateCartonRequest() while passing RequestModel as parameter in it.
        /// 
        /// </summary>
        [TestMethod]
        [Owner("Anit")]
        [TestCategory("Database")]
        public void CreateRequestWithoutId()
        {

            //fetching source area
            var expectedSourceArea = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA
    FROM TAB_INVENTORY_AREA TIA  ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     Area = row.GetValue<string>("INVENTORY_STORAGE_AREA")
 }).ExecuteSingle(_db);

            if (expectedSourceArea == null)
            {
                Assert.Inconclusive("No Area Found");
            }

            //fetching destination area
            var expectedDestArea = SqlBinder.Create(
       @"
<![CDATA[
WITH Q AS
 (SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA
    FROM TAB_INVENTORY_AREA TIA  ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     Area = row.GetValue<string>("INVENTORY_STORAGE_AREA")
 }).ExecuteSingle(_db);

            if (expectedDestArea == null)
            {
                Assert.Inconclusive("No Dest Area Found");
            }

            var expectedVirtualWarehouse = SqlBinder.Create(
               @"
<![CDATA[
WITH Q AS
 (SELECT TVW.VWH_ID AS VWH_ID
    FROM TAB_VIRTUAL_WAREHOUSE TVW
   ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     Id = row.GetValue<string>("VWH_ID")
 }).ExecuteSingle(_db);

            if (expectedVirtualWarehouse == null)
            {
                Assert.Inconclusive("No vwh Found");
            }

            var request = new RequestModel
            {
                SourceAreaId = expectedSourceArea.Area,
                DestinationArea = expectedDestArea.Area,
                Priority = "5",
                AllowOverPulling = "O",
                SourceVwhId = expectedVirtualWarehouse.Id
            };

            target.CreateCartonRequest(request);


            //asserts
            var actualRequest = SqlBinder.Create(@"
<![CDATA[
SELECT C.CTN_RESV_ID       AS CTN_RESV_ID,
       C.PRIORITY          AS PRIORITY,
       C.PIECES_CONSTRAINT AS PIECES_CONSTRAINT,
       C.SOURCE_AREA       AS SOURCE_AREA,
       C.DESTINATION_AREA  AS DESTINATION_AREA,
       C.VWH_ID            AS VWH_ID,
       C.DCMS4_REQ_ID      AS DCMS4_REQ_ID
FROM CTNRESV C
 WHERE C.CTN_RESV_ID = :CTN_RESV_ID
]]>
", row => new
 {
     ReservationId = row.GetValue<string>("CTN_RESV_ID"),
     Priority = row.GetValue<string>("PRIORITY"),
     PiecesConstraints = row.GetValue<string>("PIECES_CONSTRAINT"),
     SourceArea = row.GetValue<string>("SOURCE_AREA"),
     DestArea = row.GetValue<string>("DESTINATION_AREA"),
     VwhID = row.GetValue<string>("VWH_ID"),
     RequestId = row.GetValue<int>("DCMS4_REQ_ID")
 }).Parameter("CTN_RESV_ID", request.CtnResvId)
.ExecuteSingle(_db);

            Assert.IsNotNull(actualRequest, "No Request Found");
            Assert.AreEqual(expectedDestArea.Area, actualRequest.DestArea, "Destination Area");
            Assert.AreEqual(request.AllowOverPulling, actualRequest.PiecesConstraints, "OverPulling");
            Assert.AreEqual(request.Priority, actualRequest.Priority, "Priority");
            Assert.AreEqual(expectedSourceArea.Area, actualRequest.SourceArea, "Source Area");
            Assert.AreEqual(expectedVirtualWarehouse.Id, actualRequest.VwhID, "Virtual WareHouse");

        }


        /// <summary>
        /// This Function verifies reposirotires function AddSkuToRequest() that adds an request against existing carton reservationId.
        /// 
        /// In this we fetch a carton reservation id from ctnresv table and then fetch different  source and target sku from
        /// master_sku table.      
        /// Then we create a request model object and provide mandatory details to them.
        /// Then we called the repositorie's function AddSkuToRequest() while passing RequestModel as parameter.
        /// And then we validate the entry of sku against that carton reservationId in src_req_detail table.
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void AddSkuToRequestWithTargetSkuTest()
        {
            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                    WITH Q AS
                 (SELECT A.CTN_RESV_ID  AS CTN_RESV_ID,
                         A.DCMS4_REQ_ID AS DCMS4_REQ_ID
                    FROM CTNRESV A
                    WHERE A.MODULE_CODE = 'REQ2'
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
                 RequestId = row.GetValue<int>("DCMS4_REQ_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No Carton Found");
            }

            var expectedSourceSku = SqlBinder.Create(
                @"
                <![CDATA[

                WITH Q AS
                    (SELECT MSKU.STYLE     AS STYLE,
                            MSKU.COLOR     AS COLOR,
                            MSKU.DIMENSION AS DIMENSION,
                            MSKU.SKU_SIZE  AS SKU_SIZE,
                            MSKU.SKU_ID    AS SKU_ID
                    FROM MASTER_SKU MSKU
                    ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2

                ]]>
                ", row => new
                    {
                        SourceStyle = row.GetValue<string>("STYLE"),
                        SourceColor = row.GetValue<string>("COLOR"),
                        SourceDimension = row.GetValue<string>("DIMENSION"),
                        SourceSize = row.GetValue<string>("SKU_SIZE"),
                        SourceSkuID = row.GetValue<int>("SKU_ID")
                    }).ExecuteSingle(_db);


            if (expectedSourceSku == null)
            {
                Assert.Inconclusive("No Source Sku Found");
            }


            var expectedTargetSku = SqlBinder.Create(
                @"
                <![CDATA[

                WITH Q AS
                    (SELECT MSKU.STYLE     AS STYLE,
                            MSKU.COLOR     AS COLOR,
                            MSKU.DIMENSION AS DIMENSION,
                            MSKU.SKU_SIZE  AS SKU_SIZE,
                            MSKU.SKU_ID    AS SKU_ID
                    FROM MASTER_SKU MSKU
                    WHERE MSKU.SKU_ID != :SKU_ID
                    ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2

                ]]>
                ", row => new
                 {
                     TargetStyle = row.GetValue<string>("STYLE"),
                     TargetColor = row.GetValue<string>("COLOR"),
                     TargetDimension = row.GetValue<string>("DIMENSION"),
                     TargetSize = row.GetValue<string>("SKU_SIZE"),
                     TargetSkuID = row.GetValue<int>("SKU_ID")
                 }).Parameter("SKU_ID", expectedSourceSku.SourceSkuID).ExecuteSingle(_db);

            if (expectedTargetSku == null)
            {
                Assert.Inconclusive("No Target Sku Found");
            }

            var expectedRequestSku = new RequestSkuModel
            {

                Pieces = 5,
                SourceSku = new SkuModel
               {
                   SkuId = expectedSourceSku.SourceSkuID
               },

                TargetSku = new SkuModel
                {
                    SkuId = expectedTargetSku.TargetSkuID
                }
            };



            //calling repository function
            target.AddSkutoRequest(expectedRequestSku, expectedCartonReserveId.CartonReserveId);


            var actualSku = SqlBinder.Create(@"
                <![CDATA[
                SELECT A.STYLE                AS STYLE,
                       A.COLOR                AS COLOR,
                       A.DIMENSION            AS DIMENSION,
                       A.SKU_SIZE             AS SKU_SIZE,
                       A.QUANTITY_REQUESTED   AS QUANTITY_REQUESTED,
                       A.CONVERSION_STYLE     AS CONVERSION_STYLE,
                       A.CONVERSION_COLOR     AS CONVERSION_COLOR,
                       A.CONVERSION_DIMENSION AS CONVERSION_DIMENSION,
                       A.CONVERSION_SKU_SIZE  AS CONVERSION_SKU_SIZE
                  FROM SRC_REQ_DETAIL A
                 WHERE A.REQ_PROCESS_ID = :REQ_PROCESS_ID
                   AND A.STYLE = :STYLE
                   AND A.COLOR = :COLOR
                   AND A.DIMENSION = :DIMENSION
                   AND A.SKU_SIZE = :SKU_SIZE
                ]]>
                ", row => new
                 {
                     SourceStyle = row.GetValue<string>("STYLE"),
                     SourceColor = row.GetValue<string>("COLOR"),
                     SourceDimension = row.GetValue<string>("DIMENSION"),
                     SourceSize = row.GetValue<string>("SKU_SIZE"),
                     TargetStyle = row.GetValue<string>("CONVERSION_STYLE"),
                     TargetColor = row.GetValue<string>("CONVERSION_COLOR"),
                     TargetDimension = row.GetValue<string>("CONVERSION_DIMENSION"),
                     TargetSize = row.GetValue<string>("CONVERSION_SKU_SIZE"),
                     QuantityRequested = row.GetValue<int>("QUANTITY_REQUESTED")
                 }).Parameter("REQ_PROCESS_ID", expectedCartonReserveId.RequestId)
             .Parameter("STYLE", expectedSourceSku.SourceStyle)
             .Parameter("COLOR", expectedSourceSku.SourceColor)
             .Parameter("DIMENSION", expectedSourceSku.SourceDimension)
             .Parameter("SKU_SIZE", expectedSourceSku.SourceSize)
             .ExecuteSingle(_db);


            Assert.IsNotNull(actualSku, "No entry made in src_req_detail table for ku");
            Assert.AreEqual(expectedSourceSku.SourceStyle, actualSku.SourceStyle, "SourceStyle");
            Assert.AreEqual(expectedSourceSku.SourceColor, actualSku.SourceColor, "SourceColor");
            Assert.AreEqual(expectedSourceSku.SourceDimension, actualSku.SourceDimension, "SourceDimension");
            Assert.AreEqual(expectedSourceSku.SourceSize, actualSku.SourceSize, "SourceSize");
            Assert.AreEqual(expectedTargetSku.TargetStyle, actualSku.TargetStyle, "TargetStyle");
            Assert.AreEqual(expectedTargetSku.TargetColor, actualSku.TargetColor, "SourceStyle");
            Assert.AreEqual(expectedTargetSku.TargetDimension, actualSku.TargetDimension, "TargetDimension");
            Assert.AreEqual(expectedTargetSku.TargetSize, actualSku.TargetSize, "SourceStyle");
            Assert.AreEqual(expectedRequestSku.Pieces, actualSku.QuantityRequested, "QuantityRequested");

        }


        /// <summary>
        /// This Function verifies reposirotires function AddSkuToRequest() that adds an request against existing carton reservationId.
        /// 
        /// In this we fetch a carton reservation id from ctnresv table and then fetch source sku from master_sku table for request.      
        /// then we create a request model object and provide mandatory details to them.
        /// Then we called the repositories function while passing RequestModel as parameter.
        /// And then we validate the entry of sku against that carton reservationId in src_req_detail table.
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void AddSkuToRequestWithoutTargetSkuTest()
        {
            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                    WITH Q AS
                 (SELECT A.CTN_RESV_ID  AS CTN_RESV_ID,
                         A.DCMS4_REQ_ID AS DCMS4_REQ_ID
                    FROM CTNRESV A      
                    WHERE A.MODULE_CODE = 'REQ2'             
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
                 RequestId = row.GetValue<int>("DCMS4_REQ_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No Carton Found");
            }


            //fetching source sku
            var expectedSourceSku = SqlBinder.Create(
                @"
                <![CDATA[

                WITH Q AS
                    (SELECT MSKU.STYLE     AS STYLE,
                            MSKU.COLOR     AS COLOR,
                            MSKU.DIMENSION AS DIMENSION,
                            MSKU.SKU_SIZE  AS SKU_SIZE,
                            MSKU.SKU_ID    AS SKU_ID
                    FROM MASTER_SKU MSKU
                    where msku.inactive_flag is null
                    ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2

                ]]>
                ", row => new
                 {
                     SourceStyle = row.GetValue<string>("STYLE"),
                     SourceColor = row.GetValue<string>("COLOR"),
                     SourceDimension = row.GetValue<string>("DIMENSION"),
                     SourceSize = row.GetValue<string>("SKU_SIZE"),
                     SourceSkuID = row.GetValue<int>("SKU_ID")
                 }).ExecuteSingle(_db);


            if (expectedSourceSku == null)
            {
                Assert.Inconclusive("No Source Sku Found");
            }

            //popullating request model
            var expectedRequestSku = new RequestSkuModel
            {

                Pieces = 5,
                SourceSku = new SkuModel
                {
                    SkuId = expectedSourceSku.SourceSkuID
                }

            };



            //calling repository function
            target.AddSkutoRequest(expectedRequestSku, expectedCartonReserveId.CartonReserveId);


            //querying repository against request
            var actuaSku = SqlBinder.Create(@"
                <![CDATA[
                SELECT A.STYLE                AS STYLE,
                       A.COLOR                AS COLOR,
                       A.DIMENSION            AS DIMENSION,
                       A.SKU_SIZE             AS SKU_SIZE,
                       A.QUANTITY_REQUESTED   AS QUANTITY_REQUESTED,
                       A.CONVERSION_STYLE     AS CONVERSION_STYLE,
                       A.CONVERSION_COLOR     AS CONVERSION_COLOR,
                       A.CONVERSION_DIMENSION AS CONVERSION_DIMENSION,
                       A.CONVERSION_SKU_SIZE  AS CONVERSION_SKU_SIZE
                  FROM SRC_REQ_DETAIL A
                 WHERE A.REQ_PROCESS_ID = :REQ_PROCESS_ID
                   AND A.STYLE = :STYLE
                   AND A.COLOR = :COLOR
                   AND A.DIMENSION = :DIMENSION
                   AND A.SKU_SIZE = :SKU_SIZE
                ]]>
                ", row => new
                 {
                     SourceStyle = row.GetValue<string>("STYLE"),
                     SourceColor = row.GetValue<string>("COLOR"),
                     SourceDimension = row.GetValue<string>("DIMENSION"),
                     SourceSize = row.GetValue<string>("SKU_SIZE"),
                     TargetStyle = row.GetValue<string>("CONVERSION_STYLE"),
                     TargetColor = row.GetValue<string>("CONVERSION_COLOR"),
                     TargetDimension = row.GetValue<string>("CONVERSION_DIMENSION"),
                     TargetSize = row.GetValue<string>("CONVERSION_SKU_SIZE"),
                     QuantityRequested = row.GetValue<int>("QUANTITY_REQUESTED")
                 }).Parameter("REQ_PROCESS_ID", expectedCartonReserveId.RequestId)
                 .Parameter("STYLE", expectedSourceSku.SourceStyle)
                 .Parameter("COLOR", expectedSourceSku.SourceColor)
                 .Parameter("DIMENSION", expectedSourceSku.SourceDimension)
                 .Parameter("SKU_SIZE", expectedSourceSku.SourceSize)
                 .ExecuteSingle(_db);



            Assert.IsNotNull(actuaSku, "No entry made in src_req_detail table for ku");
            Assert.AreEqual(expectedSourceSku.SourceStyle, actuaSku.SourceStyle, "SourceStyle");
            Assert.AreEqual(expectedSourceSku.SourceColor, actuaSku.SourceColor, "SourceColor");
            Assert.AreEqual(expectedSourceSku.SourceDimension, actuaSku.SourceDimension, "SourceDimension");
            Assert.AreEqual(expectedSourceSku.SourceSize, actuaSku.SourceSize, "SourceSize");
            Assert.IsTrue(string.IsNullOrEmpty(actuaSku.TargetStyle), "TargetStyle");
            Assert.IsTrue(string.IsNullOrEmpty(actuaSku.TargetColor), "SourceStyle");
            Assert.IsTrue(string.IsNullOrEmpty(actuaSku.TargetDimension), "TargetDimension");
            Assert.IsTrue(string.IsNullOrEmpty(actuaSku.TargetSize), "Target Size");
            Assert.AreEqual(expectedRequestSku.Pieces, actuaSku.QuantityRequested, "QuantityRequested");


        }



        /// <summary>
        /// This function will verifies the repositories function UnassignCartons() that unassign carton reservation request.
        /// In this we fetch a random carton reservation Id and its request id and its line number fro ctnresv and src_req_detail
        /// where there is not an case of conversion means entery for conversion sku is null and carton is in 'BIR'.
        /// Then we fetch expected carton Id ,its Vwh and and quality from src_carton and src_carton_detail table against
        /// carton reservation details.
        /// And then we call repositorie's function unassignCartons() while passing carton reservation id as parameter.
        /// At last we verifies that the carton is unassigned from src_carton_detail and ctnresv table
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void UnassignCartonsWithoutConversionTest()
        {

            var expectedCartonReserveId = SqlBinder.Create(
              @"
            <![CDATA[
                    WITH Q AS
                 (
                    SELECT A.CTN_RESV_ID     AS CTN_RESV_ID,
                           A.DCMS4_REQ_ID    AS DCMS4_REQ_ID,
                           B.REQ_LINE_NUMBER AS REQ_LINE_NUMBER
                      FROM CTNRESV A
                      INNER JOIN SRC_REQ_DETAIL B
                        ON A.DCMS4_REQ_ID = B.REQ_PROCESS_ID
                        AND A.MODULE_CODE = B.REQ_MODULE_CODE
                     INNER JOIN SRC_CARTON_DETAIL CTNDET
                        ON CTNDET.REQ_PROCESS_ID = B.REQ_PROCESS_ID
                       AND CTNDET.REQ_LINE_NUMBER = B.REQ_LINE_NUMBER
                     INNER JOIN SRC_CARTON CTN
                        ON CTN.CARTON_ID = CTNDET.CARTON_ID
                     WHERE B.CONVERSION_VWH_ID IS NULL
                       AND B.CONVERSION_STYLE IS NULL
                       AND B.CONVERSION_COLOR IS NULL
                       AND B.CONVERSION_DIMENSION IS NULL
                       AND B.CONVERSION_SKU_SIZE IS NULL
                       AND CTN.CARTON_STORAGE_AREA = 'BIR'
                       AND A.ASSIGNED_FLAG = 'Y'
                       AND A.MODULE_CODE = 'REQ2'                        
                     ORDER BY DBMS_RANDOM.VALUE
                 )
                SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
           {
               CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
               RequestId = row.GetValue<int>("DCMS4_REQ_ID"),
               LineNumber = row.GetValue<int>("REQ_LINE_NUMBER")
           }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No Carton Found");
            }



            var expectedCarton = SqlBinder.Create(
                                @"
                <![CDATA[
                        SELECT COUNT(*) AS COUNT
                           FROM SRC_CARTON_DETAIL CTNDET
                           INNER JOIN SRC_REQ_DETAIL REQDET
                           ON CTNDET.REQ_PROCESS_ID = REQDET.REQ_PROCESS_ID
                           WHERE REQDET.REQ_PROCESS_ID = :REQ_PROCESS_ID                           
                ]]>
                ", row => new
                 {
                     Count = row.GetValue<int>("COUNT")
                 }).Parameter("REQ_PROCESS_ID", expectedCartonReserveId.RequestId)
                 .ExecuteSingle(_db);

            if (expectedCarton == null)
            {
                Assert.Inconclusive("No carton Id found for request");
            }

            target.UnAssignCartons(expectedCartonReserveId.CartonReserveId);

            var actualRequest = SqlBinder.Create(@"
                    <![CDATA[
                    SELECT A.ASSIGNED_FLAG AS ASSIGNED_FLAG,
                           A.ASSIGN_DATE   AS ASSIGN_DATE,
                           A.ASSIGN_BY     AS ASSIGN_BY,
                           A.MODIFIED_DATE AS MODIFIED_DATE,
                           A.MODIFIED_BY   AS MODIFIED_BY
                      FROM CTNRESV A
                     WHERE A.DCMS4_REQ_ID = :DCMS4_REQ_ID
                    ]]>
                    ", row => new
                     {
                         AssignedFlag = row.GetValue<string>("ASSIGNED_FLAG"),
                         AssignDate = row.GetValue<DateTime>("ASSIGN_DATE"),
                         AssignedBy = row.GetValue<string>("ASSIGN_BY"),

                     }).Parameter("DCMS4_REQ_ID", expectedCartonReserveId.RequestId).ExecuteSingle(_db);

            Assert.IsNotNull(actualRequest, "no info found in ctnresv table for given reservation Id");
            Assert.IsTrue(string.IsNullOrEmpty(actualRequest.AssignedBy), "Assigned By");
            Assert.IsTrue(string.IsNullOrEmpty(actualRequest.AssignedFlag), "Assigned Flag");





            var actualCartonDetail = SqlBinder.Create(
                @"
                    <![CDATA[
                    SELECT COUNT(*) AS COUNT
                           FROM SRC_CARTON_DETAIL CTNDET
                           INNER JOIN SRC_REQ_DETAIL REQDET
                           ON CTNDET.REQ_PROCESS_ID = REQDET.REQ_PROCESS_ID
                           WHERE REQDET.REQ_PROCESS_ID = :REQ_PROCESS_ID      
                    ]]>
                    ", row => new
                     {
                         Count = row.GetValue<int>("COUNT")
                     }).Parameter("REQ_PROCESS_ID", expectedCartonReserveId.RequestId).ExecuteSingle(_db);



            Assert.AreEqual(actualCartonDetail.Count,0, "Entry found in src_carton_detail table for inserted carton");
        
        }


        /// <summary>
        /// This function will verifies the repositories function UnassignCartons() that unassign carton reservation request.
        /// In this we fetch a random carton reservation Id and its details from ctnresv and src_req_detail
        /// where there is a case of conversion means a entery for convertion_sku is made.
        /// We also fetch best quality from table tab_quality_code
        /// Then we fetch expected carton Id and its details from src_carton and src_carton_detail table against
        /// carton reservation details.
        /// And then we call repositorie's function unassignCartons() while passing carton reservation id as parameter.
        /// At last we verifies that the carton is unassigned from src_carton_detail and ctnresv table
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void UnassignCartonsWithConversionTest()
        {
            var expectedCartonReserveId = SqlBinder.Create(
             @"
            <![CDATA[
                    WITH Q AS
                 (
                    SELECT A.CTN_RESV_ID          AS CTN_RESV_ID,
                           A.DCMS4_REQ_ID         AS DCMS4_REQ_ID,
                           B.REQ_LINE_NUMBER      AS REQ_LINE_NUMBER,
                           B.CONVERSION_VWH_ID    AS CONVERSION_VWH_ID,
                           B.VWH_ID               AS VWH_ID,
                           B.CONVERSION_STYLE     AS CONVERSION_STYLE,
                           B.STYLE                AS STYLE,
                           B.CONVERSION_COLOR     AS CONVERSION_COLOR,
                           B.COLOR                AS COLOR,
                           B.CONVERSION_DIMENSION AS CONVERSION_DIMENSION,
                           B.DIMENSION            AS DIMENSION,
                           B.CONVERSION_SKU_SIZE  AS CONVERSION_SKU_SIZE,
                           B.SKU_SIZE             AS SKU_SIZE
                      FROM CTNRESV A
                      LEFT OUTER JOIN SRC_REQ_DETAIL B
                        ON A.DCMS4_REQ_ID = B.REQ_PROCESS_ID
                     INNER JOIN SRC_CARTON_DETAIL CTNDET
                        ON CTNDET.REQ_PROCESS_ID = A.DCMS4_REQ_ID
                      AND CTNDET.REQ_LINE_NUMBER = B.REQ_LINE_NUMBER
                     INNER JOIN SRC_CARTON CTN
                        ON CTN.CARTON_ID = CTNDET.CARTON_ID
                      WHERE B.CONVERSION_STYLE  != B.STYLE
                        AND B.CONVERSION_COLOR  != B.COLOR
                        AND B.CONVERSION_DIMENSION != B.DIMENSION
                        AND B.CONVERSION_SKU_SIZE != B.SKU_SIZE
                        AND B.CONVERSION_VWH_ID IS  NOT NULL
                        AND B.CONVERSION_STYLE IS NOT NULL
                        AND B.CONVERSION_COLOR IS NOT NULL
                        AND B.CONVERSION_DIMENSION IS NOT NULL
                        AND B.CONVERSION_SKU_SIZE IS NOT NULL
                        AND CTN.CARTON_STORAGE_AREA = 'BIR'
                        AND A.ASSIGNED_FLAG = 'Y'
                       ORDER BY DBMS_RANDOM.VALUE
                  )
                SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
                 RequestId = row.GetValue<int>("DCMS4_REQ_ID"),
                 LineNumber = row.GetValue<int>("REQ_LINE_NUMBER"),
                 Style = row.GetValue<string>("STYLE"),
                 TargetStyle = row.GetValue<string>("CONVERSION_STYLE"),
                 Color = row.GetValue<string>("COLOR"),
                 TargetColor = row.GetValue<string>("CONVERSION_COLOR"),
                 Dimension = row.GetValue<string>("DIMENSION"),
                 TargetDimension = row.GetValue<string>("CONVERSION_DIMENSION"),
                 Size = row.GetValue<string>("SKU_SIZE"),
                 TargetSize = row.GetValue<string>("CONVERSION_SKU_SIZE"),
                 Vwh = row.GetValue<string>("VWH_ID"),
                 TargetVwh = row.GetValue<string>("CONVERSION_VWH_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No Carton Found");
            }

            var bestquality = SqlBinder.Create(
                @"
                    <![CDATA[
                    WITH Q AS
                     (SELECT TQC.QUALITY_CODE AS QUALITY_CODE
                        FROM TAB_QUALITY_CODE TQC  
                       ORDER BY TQC.QUALITY_RANK)
                    SELECT * FROM Q WHERE ROWNUM < 2
                    ]]>
                    ", row => new
                     {
                         Quality = row.GetValue<string>("QUALITY_CODE")
                     }).ExecuteSingle(_db);

            if (bestquality == null)
            {
                Assert.Inconclusive("No Quality Found");
            }




            var expectedCarton = SqlBinder.Create(
                              @"
                <![CDATA[
                         SELECT COUNT(*) AS COUNT
                           FROM SRC_CARTON_DETAIL CTNDET
                           INNER JOIN SRC_REQ_DETAIL REQDET
                           ON CTNDET.REQ_PROCESS_ID = REQDET.REQ_PROCESS_ID
                           WHERE REQDET.REQ_PROCESS_ID = :REQ_PROCESS_ID 
                ]]>
                ", row => new
                 {
                     Count = row.GetValue<int>("COUNT")
                 }).Parameter("REQ_PROCESS_ID", expectedCartonReserveId.RequestId)
               .ExecuteSingle(_db);

            if (expectedCarton == null)
            {
                Assert.Inconclusive("No carton Id found for request");
            }

            target.UnAssignCartons(expectedCartonReserveId.CartonReserveId);


            var actualCartonDetail = SqlBinder.Create(
                @"
                    <![CDATA[
                       SELECT COUNT(*) AS COUNT
                           FROM SRC_CARTON_DETAIL CTNDET
                           INNER JOIN SRC_REQ_DETAIL REQDET
                           ON CTNDET.REQ_PROCESS_ID = REQDET.REQ_PROCESS_ID
                           WHERE REQDET.REQ_PROCESS_ID = :REQ_PROCESS_ID 
                    ]]>
                    ", row => new
                     {
                         Count = row.GetValue<int>("COUNT")
                     }).Parameter("CARTON_ID", expectedCartonReserveId.RequestId).ExecuteSingle(_db);


            Assert.AreEqual(actualCartonDetail.Count,0, "No entry found in src_carton_detail table for inserted carton");
            
            var actualRequest = SqlBinder.Create(@"
                    <![CDATA[
                    SELECT A.ASSIGNED_FLAG AS ASSIGNED_FLAG,
                           A.ASSIGN_DATE   AS ASSIGN_DATE,
                           A.ASSIGN_BY     AS ASSIGN_BY,
                           A.DCMS4_REQ_ID  AS DCMS4_REQ_ID,
                           A.MODIFIED_DATE AS MODIFIED_DATE,
                           A.MODIFIED_BY   AS MODIFIED_BY,
                           A.VWH_ID        AS VWH_ID,
                           A.SOURCE_AREA AS SOURCE_AREA,
                           A.CONVERSION_VWH_ID AS CONVERSION_VWH_ID,
                           A.QUALITY_CODE  AS QUALITY_CODE
                      FROM CTNRESV A
                     WHERE A.DCMS4_REQ_ID = :DCMS4_REQ_ID
                    ]]>
                    ", row => new
                     {
                         AssignedFlag = row.GetValue<string>("ASSIGNED_FLAG"),
                         AssignDate = row.GetValue<DateTime>("ASSIGN_DATE"),
                         AssignedBy = row.GetValue<string>("ASSIGN_BY"),
                         RequestId = row.GetValue<int>("DCMS4_REQ_ID"),
                         ModifeidDate = row.GetValue<DateTime>("MODIFIED_DATE"),
                         VwhId = row.GetValue<string>("VWH_ID"),
                         Quality = row.GetValue<string>("QUALITY_CODE"),
                         SourceArea = row.GetValue<string>("SOURCE_AREA"),
                         TargetVwhId = row.GetValue<string>("CONVERSION_VWH_ID")
                     }).Parameter("DCMS4_REQ_ID", expectedCartonReserveId.RequestId).ExecuteSingle(_db);


            Assert.IsNotNull(actualRequest, "no info found in ctnresv table for given reservation Id");
            //Assert.IsTrue(string.IsNullOrEmpty(actualRequest.AssignDate.ToString()), "Assigned Date");
            Assert.IsTrue(string.IsNullOrEmpty(actualRequest.AssignedBy), "Assigned By");
            Assert.IsTrue(string.IsNullOrEmpty(actualRequest.AssignedFlag), "Assigned Flag");
            Assert.IsTrue(string.IsNullOrEmpty(actualRequest.ModifeidDate.ToString()), "Modified Date");
            Assert.AreEqual(expectedCartonReserveId.RequestId, actualRequest.RequestId, "Request Id");
            Assert.AreEqual(expectedCartonReserveId.Vwh, actualRequest.VwhId, "VwhId");
            Assert.AreEqual(bestquality.Quality, actualRequest.Quality, "Quality");
            Assert.IsTrue(string.IsNullOrEmpty(actualRequest.TargetVwhId), "Convertional VwhId");            
        }




        /// <summary>
        /// This function is verifying repositories function AssignCartons() that assign cartons on a request
        /// In this first we fetch a random carton detail that can be assigned and then we created a request for sku
        /// that exists in that carton.
        /// Then we call repositories function assign cartons to assign carton for a specified request 
        /// and then we verifies the cartons are assigned.
        /// 
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void AssignCartonsTest()
        {
            //fetching a random carton details that can be assigned
            var expectedCarton = SqlBinder.Create(
                @"
                    <![CDATA[
                    WITH Q AS
                     (SELECT CTN.CARTON_ID           AS CARTON_ID,
                             CTN.LOCATION_ID         AS LOCATION_ID,
                             CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                             CTN.VWH_ID              AS VWH_ID,
                             CTNDET.STYLE            AS STYLE,
                             CTNDET.COLOR            AS COLOR,
                             CTNDET.DIMENSION        AS DIMENSION,
                             CTNDET.SKU_SIZE         AS SKU_SIZE,
                             CTNDET.QUANTITY         AS QUANTITY,
                             CTNDET.SKU_ID           AS SKU_ID
                        FROM SRC_CARTON CTN
                       INNER JOIN SRC_CARTON_DETAIL CTNDET
                          ON CTN.CARTON_ID = CTNDET.CARTON_ID
                       WHERE CTN.SUSPENSE_DATE IS NULL
                         AND CTN.CARTON_STORAGE_AREA = 'BIR'
                         AND CTNDET.REQ_PROCESS_ID IS NULL
                         AND CTNDET.REQ_MODULE_CODE IS NULL
                         AND CTNDET.REQ_LINE_NUMBER IS NULL
                         AND CTNDET.REQ_ASSIGN_DATE IS NULL
                       ORDER BY DBMS_RANDOM.VALUE)
                    SELECT * FROM Q WHERE ROWNUM < 2

                    ]]>
                    ", row => new
                     {
                         CartonId = row.GetValue<string>("CARTON_ID"),
                         LocationId = row.GetValue<string>("LOCATION_ID"),
                         Vwh = row.GetValue<string>("VWH_ID"),
                         Style = row.GetValue<string>("STYLE"),
                         Color = row.GetValue<string>("COLOR"),
                         Dimension = row.GetValue<string>("DIMENSION"),
                         Size = row.GetValue<string>("SKU_SIZE"),
                         Quantity = row.GetValue<int>("QUANTITY"),
                         SkuId = row.GetValue<int>("SKU_ID"),
                         Area = row.GetValue<string>("CARTON_STORAGE_AREA")
                     }).ExecuteSingle(_db);
            if (expectedCarton == null)
            {
                Assert.Inconclusive("No Carton Found");
            }

            //fetching destination area
            var expectedDestArea = SqlBinder.Create(
       @"
<![CDATA[
WITH Q AS
 (SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA
    FROM TAB_INVENTORY_AREA TIA  ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     Area = row.GetValue<string>("INVENTORY_STORAGE_AREA")
 }).ExecuteSingle(_db);

            if (expectedDestArea == null)
            {
                Assert.Inconclusive("No Dest Area Found");
            }


            //popullating RequestModel
            var request = new RequestModel
            {
                AllowOverPulling = "O",
                DestinationArea = expectedDestArea.Area,
                Priority = "20",
                SourceAreaId = expectedCarton.Area,
                SourceVwhId = expectedCarton.Vwh
            };

            //creating request
            target.CreateCartonRequest(request);


            //popullating requestSkuModel
            var requestSku = new RequestSkuModel
            {
                Pieces = expectedCarton.Quantity,
                SourceSku = new SkuModel
                {
                    SkuId = expectedCarton.SkuId
                }
            };

            //Adding sku to request
            target.AddSkutoRequest(requestSku, request.CtnResvId);

            //Assigning cartons
            target.AssignCartons(request.CtnResvId);


            //fetyching request and line number against created ctnresvId
            var expectedRequest = SqlBinder.Create(
                       @"
                            <![CDATA[
                            SELECT A.REQ_PROCESS_ID  AS REQ_PROCESS_ID,
                                   A.REQ_LINE_NUMBER AS REQ_LINE_NUMBER
                              FROM SRC_REQ_DETAIL A
                             INNER JOIN CTNRESV B
                                ON A.REQ_PROCESS_ID = B.DCMS4_REQ_ID
                             WHERE A.STYLE = :STYLE
                               AND A.COLOR = :COLOR
                               AND A.DIMENSION = :DIMENSION
                               AND A.SKU_SIZE = :SKU_SIZE
                            ]]>
                            ",
                        row => new
                        {
                            RequestId = row.GetValue<int>("REQ_PROCESS_ID"),
                            LineNumber = row.GetValue<int>("REQ_LINE_NUMBER")
                        }).Parameter("STYLE", expectedCarton.Style)
                        .Parameter("COLOR", expectedCarton.Color)
                        .Parameter("DIMENSION", expectedCarton.Dimension)
                        .Parameter("SKU_SIZE", expectedCarton.Size)
                        .ExecuteSingle(_db);
            if (expectedRequest == null)
            {
            }

            //fetching assigned carton Id
            var cartonId = SqlBinder.Create(@"
<![CDATA[
SELECT CTNDET.CARTON_ID AS CARTON_ID
  FROM SRC_CARTON_DETAIL CTNDET
 INNER JOIN SRC_REQ_DETAIL B
    ON CTNDET.REQ_PROCESS_ID = B.REQ_PROCESS_ID
   AND CTNDET.REQ_LINE_NUMBER = B.REQ_LINE_NUMBER
 WHERE B.REQ_PROCESS_ID = :REQ_PROCESS_ID
   AND B.REQ_LINE_NUMBER = :REQ_LINE_NUMBER
]]>
", row => new
 {
     CartonId = row.GetValue<string>("CARTON_ID")
 }).Parameter("REQ_PROCESS_ID", expectedRequest.RequestId)
.Parameter("REQ_LINE_NUMBER", expectedRequest.LineNumber).ExecuteSingle(_db);

            if (cartonId == null)
            {
                Assert.Inconclusive("No id found");
            }


            //Querying database
            var actualCtnReserve = SqlBinder.Create(
                 @"
                <![CDATA[
                SELECT A.ASSIGNED_FLAG    AS ASSIGNED_FLAG,
                       A.ASSIGN_DATE      AS ASSIGN_DATE,
                       A.ASSIGN_BY        AS ASSIGN_BY,
                       A.MODIFIED_DATE    AS MODIFIED_DATE,
                       A.MODIFIED_BY      AS MODIFIED_BY,
                       A.SOURCE_AREA      AS SOURCE_AREA,
                       A.DESTINATION_AREA AS DESTINATION_AREA,
                       A.QUALITY_CODE     AS QUALITY_CODE
                  FROM CTNRESV A
                  WHERE A.CTN_RESV_ID = :CTN_RESV_ID
                ]]>
                ", row => new
                 {
                     AssignedFlag = row.GetValue<string>("ASSIGNED_FLAG"),
                     AssignedDate = row.GetValue<DateTime>("ASSIGN_DATE"),
                     AssignedBy = row.GetValue<string>("ASSIGN_BY"),
                     ModifiedDate = row.GetValue<DateTime>("MODIFIED_DATE"),
                     ModifiedBy = row.GetValue<string>("MODIFIED_BY"),
                     SourceArea = row.GetValue<string>("SOURCE_AREA"),
                     DestinationArea = row.GetValue<string>("DESTINATION_AREA"),
                     Quality = row.GetValue<string>("QUALITY_CODE")
                 }).Parameter("CTN_RESV_ID", request.CtnResvId).ExecuteSingle(_db);


            Assert.IsNotNull(actualCtnReserve, "No data found in ctnresv to verify");
            Assert.AreEqual("Y", actualCtnReserve.AssignedFlag, "Assigned Flag");
            Assert.IsTrue(!string.IsNullOrEmpty(actualCtnReserve.AssignedBy), "Assigned By");
            Assert.IsTrue(!string.IsNullOrEmpty(actualCtnReserve.AssignedDate.ToString()), "Assigned Date");
            Assert.IsTrue(!string.IsNullOrEmpty(actualCtnReserve.ModifiedBy), "Modified By");
            Assert.IsTrue(!string.IsNullOrEmpty(actualCtnReserve.ModifiedDate.ToString()), "Modified Date");
            Assert.AreEqual(expectedDestArea.Area, actualCtnReserve.DestinationArea, "Destination Area");
            Assert.AreEqual(expectedCarton.Area, actualCtnReserve.SourceArea, "Source Area");
        }



        //invalid Test

        /// <summary>
        /// This test case is verifying repositories function GetRequests().
        /// In this function we passed a invalid ctnResvId to function GetRequests() and check it must return null.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        public void RequestInfoTestInvalidParameter()
        {
            string ctnResvId = "Ankit";

            var actualRequest = target.GetRequests(ctnResvId, 1);

            Assert.AreEqual(actualRequest.Count(), 0, "Rows are returning for invalid arton Reservation Id");
        }


        /// <summary>
        /// This test case is verifying repositories function GetAssignedCartons().
        /// In this function we passed  null as ctnResvId parameter to function GetRequests() and check it must not return 
        /// any row.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        public void GetAssignedCartonsNullId()
        {

            var actualCartons = target.GetAssignedCartons(null);
            Assert.AreEqual(actualCartons.Count().ToString(), "0", "Returning assigned cartons for null carton reservation ID");

        }


        /// <summary>
        /// This test case is verifying repositories function GetAssignedCartons().
        /// In this function we passed  invalid ctnResvId as parameter to function GetRequests() and check it must not return 
        /// any row.
        /// </summary>        
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        public void GetAssignedCartonsInvalidId()
        {

            var actualCartons = target.GetAssignedCartons("Ankit");

            Assert.AreEqual(actualCartons.Count().ToString(), "0", "Returning assigned cartons for invalid carton reservation ID");

        }


        /// <summary>
        /// This test case is verifying repositories function GetRequestSkus().
        /// In this function we passed  invalid ctnResvId as parameter to function GetRequestSkus() and check it must not return 
        /// any row.
        /// </summary>        
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        public void GetRecentSkuInvalidId()
        {

            var actualRequest = target.GetRequestSkus("Ankit");
            Assert.AreEqual(actualRequest.Count().ToString(), "0", "Returning assigned cartons for invalid carton reservation ID");
        }



        /// <summary>
        /// This test case is verifying repositories function GetRequestSkus().
        /// In this function we passed  null as ctnResvId parameter to function GetRequestSkus() and check it must not return 
        /// any row.
        /// </summary>        
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        public void GetRecentSkuNullId()
        {

            var actualRequest = target.GetRequestSkus(null);
            Assert.AreEqual(actualRequest.Count().ToString(), "0", "Returning assigned cartons for null carton reservation ID");
        }


        /// <summary>
        /// This test case is verifying repositories function DeleteCartonRequest().
        /// In this function we passed  null as ctnResvId parameter to function DeleteCartonRequest() and check it must not return 
        /// any row.
        /// </summary>        
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void DeleteCartonRequestNullId()
        {

            target.DeleteCartonRequest(null);

        }


        /// <summary>
        /// This test case is verifying repositories function DeleteCartonRequest().
        /// In this function we passed  an invalid ctnResvId as parameter to function DeleteCartonRequest() and check it must not return 
        /// any row.
        /// </summary>        
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        public void DeleteCartonRequestInvalidId()
        {

            target.DeleteCartonRequest("Ankit");

        }

        /// <summary>
        /// This test case is verifying repositories function DeleteSkuFromRequestRequest().
        /// In this function we passed  null as ctnResvId parameter to function DeleteSkuFromRequestRequest() and check it must 
        /// throw exception.
        /// </summary>  
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void DeleteSkuNullCtnresvId()
        {

            //querying skuId against sku details
            var expectedSkuId = SqlBinder.Create(
                 @"
                <![CDATA[
                   WITH Q AS
                    (SELECT MSKU.SKU_ID AS SKU_ID
                       FROM MASTER_SKU MSKU
                      ORDER BY DBMS_RANDOM.VALUE)
                   SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                ", row => new
                 {
                     SkuID = row.GetValue<int>("SKU_ID")
                 }).ExecuteSingle(_db);

            if (expectedSkuId == null)
            {
                Assert.Inconclusive("No skuid found against sku details");
            }


            //calling repositories function
            target.DeleteSkuFromRequest(expectedSkuId.SkuID, null);

        }

        /// <summary>
        /// This test case is verifying repositories function DeleteSkuFromRequestRequest().
        /// In this function we passed  an invalid ctnResvId as parameter to function DeleteSkuFromRequestRequest() and check it must 
        /// throw exception.
        /// </summary>  
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void DeleteSkuInvalidCtnresvId()
        {


            //querying skuId against sku details
            var expectedSkuId = SqlBinder.Create(
                 @"
                <![CDATA[
                   WITH Q AS
                    (SELECT MSKU.SKU_ID AS SKU_ID
                       FROM MASTER_SKU MSKU
                      ORDER BY DBMS_RANDOM.VALUE)
                   SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                ", row => new
                 {
                     SkuID = row.GetValue<int>("SKU_ID")
                 }).ExecuteSingle(_db);

            if (expectedSkuId == null)
            {
                Assert.Inconclusive("No skuid found against sku details");
            }



            //calling repositories function
            target.DeleteSkuFromRequest(expectedSkuId.SkuID, "aNKIT");

        }

        /// <summary>
        /// This test case is verifying repositories function CreateCartonRequest().
        /// In this function we passed  null as SourceAreaId in RequestModel and passed it as parameter to function CreateCartonRequest() 
        /// and check it must throw exception.
        /// </summary>  
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void CreateRequestNullSourceArea()
        {

            var request = new RequestModel
            {
                SourceAreaId = null,
                DestinationArea = "CON",
                Priority = "5",
                AllowOverPulling = "O",
                SourceVwhId = "15"
            };

            target.CreateCartonRequest(request);

        }

        /// <summary>
        /// This test case is verifying repositories function CreateCartonRequest().
        /// In this function we passed  null as DestinationArea in RequestModel and passed it as parameter to function CreateCartonRequest() 
        /// and check it must throw exception.
        /// </summary>  
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void CreateRequestNullDestinationArea()
        {

            var request = new RequestModel
            {
                SourceAreaId = "BIR",
                DestinationArea = null,
                Priority = "5",
                AllowOverPulling = "O",
                SourceVwhId = "15"
            };

            target.CreateCartonRequest(request);

        }

        /// <summary>
        /// This test case is verifying repositories function CreateCartonRequest().
        /// In this function we passed  null as Priority in RequestModel and passed it as parameter to function CreateCartonRequest() 
        /// and check it must throw exception.
        /// </summary>  
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void CreateRequestNullPriority()
        {

            var request = new RequestModel
            {
                SourceAreaId = "BIR",
                DestinationArea = "CON",
                Priority = null,
                AllowOverPulling = "O",
                SourceVwhId = "15"
            };

            target.CreateCartonRequest(request);

        }



        /// <summary>
        /// This test case is verifying repositories function CreateCartonRequest().
        /// In this function we passed  null as AllowOverPulling in RequestModel and passed it as parameter to function CreateCartonRequest() 
        /// and check it must throw exception.
        /// </summary>  
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void CreateRequestNullPulling()
        {

            var request = new RequestModel
            {
                SourceAreaId = "BIR",
                DestinationArea = "CON",
                Priority = "5",
                AllowOverPulling = null,
                SourceVwhId = "15"
            };

            target.CreateCartonRequest(request);

        }



        /// <summary>
        /// This test case is verifying repositories function AddSkutoRequest().
        /// In this function we passed  null CtnResvId as parameter to function AddSkutoRequest() 
        /// and check it must throw exception.
        /// </summary>  
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void AddSkuToRequestNullCtnresvId()
        {


            //fetching source sku
            var expectedSourceSku = SqlBinder.Create(
                @"
                <![CDATA[

                WITH Q AS
                    (SELECT MSKU.STYLE     AS STYLE,
                            MSKU.COLOR     AS COLOR,
                            MSKU.DIMENSION AS DIMENSION,
                            MSKU.SKU_SIZE  AS SKU_SIZE,
                            MSKU.SKU_ID    AS SKU_ID
                    FROM MASTER_SKU MSKU
                    where msku.inactive_flag is null
                    ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2

                ]]>
                ", row => new
                 {
                     SourceStyle = row.GetValue<string>("STYLE"),
                     SourceColor = row.GetValue<string>("COLOR"),
                     SourceDimension = row.GetValue<string>("DIMENSION"),
                     SourceSize = row.GetValue<string>("SKU_SIZE"),
                     SourceSkuID = row.GetValue<int>("SKU_ID")
                 }).ExecuteSingle(_db);


            if (expectedSourceSku == null)
            {
                Assert.Inconclusive("No Source Sku Found");
            }

            //popullating request model
            var expectedRequestSku = new RequestSkuModel
            {

                Pieces = 5,
                SourceSku = new SkuModel
                {
                    SkuId = expectedSourceSku.SourceSkuID
                }

            };



            //calling repository function
            target.AddSkutoRequest(expectedRequestSku, null);


        }

        /// <summary>
        /// This test case is verifying repositories function AddSkutoRequest().
        /// In this function we passed an invalid CtnResvId as parameter to function AddSkutoRequest() 
        /// and check it must throw exception.
        /// </summary>  
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void AddSkuToRequestInvalidCtnresvId()
        {


            //fetching source sku
            var expectedSourceSku = SqlBinder.Create(
                @"
                <![CDATA[

                WITH Q AS
                    (SELECT MSKU.STYLE     AS STYLE,
                            MSKU.COLOR     AS COLOR,
                            MSKU.DIMENSION AS DIMENSION,
                            MSKU.SKU_SIZE  AS SKU_SIZE,
                            MSKU.SKU_ID    AS SKU_ID
                    FROM MASTER_SKU MSKU
                    where msku.inactive_flag is null
                    ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2

                ]]>
                ", row => new
                 {
                     SourceStyle = row.GetValue<string>("STYLE"),
                     SourceColor = row.GetValue<string>("COLOR"),
                     SourceDimension = row.GetValue<string>("DIMENSION"),
                     SourceSize = row.GetValue<string>("SKU_SIZE"),
                     SourceSkuID = row.GetValue<int>("SKU_ID")
                 }).ExecuteSingle(_db);


            if (expectedSourceSku == null)
            {
                Assert.Inconclusive("No Source Sku Found");
            }

            //popullating request model
            var expectedRequestSku = new RequestSkuModel
            {

                Pieces = 5,
                SourceSku = new SkuModel
                {
                    SkuId = expectedSourceSku.SourceSkuID
                }

            };



            //calling repository function
            target.AddSkutoRequest(expectedRequestSku, "Ankit");


        }


        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void AddSkuToRequestNullRequestSkuModel()
        {
            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                    WITH Q AS
                 (SELECT A.CTN_RESV_ID  AS CTN_RESV_ID,
                         A.DCMS4_REQ_ID AS DCMS4_REQ_ID
                    FROM CTNRESV A      
                    WHERE A.MODULE_CODE = 'REQ2'             
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
                 RequestId = row.GetValue<int>("DCMS4_REQ_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No Carton Found");
            }



            //popullating request model
            var expectedRequestSku = new RequestSkuModel();

            //calling repository function
            target.AddSkutoRequest(expectedRequestSku, expectedCartonReserveId.CartonReserveId);


        }


        /// <summary>
        /// This test case is verifying repositories function AddSkutoRequest().
        /// In this function we passed an invalid pieces in RequestskuModel and pass it as parameter to function AddSkutoRequest() 
        /// and check it must throw exception.
        /// </summary>  
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void AddSkuToRequestInvalidPieces()
        {
            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                    WITH Q AS
                 (SELECT A.CTN_RESV_ID  AS CTN_RESV_ID,
                         A.DCMS4_REQ_ID AS DCMS4_REQ_ID
                    FROM CTNRESV A      
                    WHERE A.MODULE_CODE = 'REQ2'             
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
                 RequestId = row.GetValue<int>("DCMS4_REQ_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No Carton Found");
            }


            //fetching source sku
            var expectedSourceSku = SqlBinder.Create(
                @"
                <![CDATA[

                WITH Q AS
                    (SELECT MSKU.STYLE     AS STYLE,
                            MSKU.COLOR     AS COLOR,
                            MSKU.DIMENSION AS DIMENSION,
                            MSKU.SKU_SIZE  AS SKU_SIZE,
                            MSKU.SKU_ID    AS SKU_ID
                    FROM MASTER_SKU MSKU
                    where msku.inactive_flag is null
                    ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2

                ]]>
                ", row => new
                 {
                     SourceStyle = row.GetValue<string>("STYLE"),
                     SourceColor = row.GetValue<string>("COLOR"),
                     SourceDimension = row.GetValue<string>("DIMENSION"),
                     SourceSize = row.GetValue<string>("SKU_SIZE"),
                     SourceSkuID = row.GetValue<int>("SKU_ID")
                 }).ExecuteSingle(_db);


            if (expectedSourceSku == null)
            {
                Assert.Inconclusive("No Source Sku Found");
            }

            //popullating request model
            var expectedRequestSku = new RequestSkuModel
            {
                Pieces = -1,

                SourceSku = new SkuModel
                {
                    SkuId = expectedSourceSku.SourceSkuID
                }

            };



            //calling repository function
            target.AddSkutoRequest(expectedRequestSku, expectedCartonReserveId.CartonReserveId);


        }


        /// <summary>
        /// This test case is verifying repositories function AddSkutoRequest().
        /// In this function we passed null as source SkuID  in RequestskuModel and pass it as parameter to function AddSkutoRequest() 
        /// and check it must throw exception.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void AddSkuToRequestNullSkuId()
        {
            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                    WITH Q AS
                 (SELECT A.CTN_RESV_ID  AS CTN_RESV_ID,
                         A.DCMS4_REQ_ID AS DCMS4_REQ_ID
                    FROM CTNRESV A      
                    WHERE A.MODULE_CODE = 'REQ2'             
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
                 RequestId = row.GetValue<int>("DCMS4_REQ_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No Carton Found");
            }

            //popullating request model
            var expectedRequestSku = new RequestSkuModel
            {
                Pieces = 5

            };



            //calling repository function
            target.AddSkutoRequest(expectedRequestSku, expectedCartonReserveId.CartonReserveId);


        }


        /// <summary>
        /// This test case is verifying repositories function AddSkutoRequest().
        /// In this function we passed an invalid source SkuID  in RequestskuModel and pass it as parameter to function AddSkutoRequest() 
        /// and check it must throw exception.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void AddSkuToRequestInvalidSkuId()
        {
            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                    WITH Q AS
                 (SELECT A.CTN_RESV_ID  AS CTN_RESV_ID,
                         A.DCMS4_REQ_ID AS DCMS4_REQ_ID
                    FROM CTNRESV A      
                    WHERE A.MODULE_CODE = 'REQ2'             
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
                 RequestId = row.GetValue<int>("DCMS4_REQ_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No Carton Found");
            }


            //popullating request model
            var expectedRequestSku = new RequestSkuModel
            {
                Pieces = -1,

                SourceSku = new SkuModel
                {
                    SkuId = -101
                }

            };



            //calling repository function
            target.AddSkutoRequest(expectedRequestSku, expectedCartonReserveId.CartonReserveId);


        }

        /// <summary>
        /// This test case is verifying repositories function AddSkutoRequest().
        /// In this function we passed an invalid target SkuID  in RequestskuModel and pass it as parameter to function AddSkutoRequest() 
        /// and check it must throw exception.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void AddSkuToRequestInvalidTargetSkuId()
        {
            //fetching random carton reserve Id.
            var expectedCartonReserveId = SqlBinder.Create(
                @"
            <![CDATA[
                    WITH Q AS
                 (SELECT A.CTN_RESV_ID  AS CTN_RESV_ID,
                         A.DCMS4_REQ_ID AS DCMS4_REQ_ID
                    FROM CTNRESV A      
                    WHERE A.MODULE_CODE = 'REQ2'             
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 CartonReserveId = row.GetValue<string>("CTN_RESV_ID"),
                 RequestId = row.GetValue<int>("DCMS4_REQ_ID")
             }).ExecuteSingle(_db);

            if (expectedCartonReserveId == null)
            {
                Assert.Inconclusive("No Carton Found");
            }


            //fetching source sku
            var expectedSourceSku = SqlBinder.Create(
                @"
                <![CDATA[

                WITH Q AS
                    (SELECT MSKU.STYLE     AS STYLE,
                            MSKU.COLOR     AS COLOR,
                            MSKU.DIMENSION AS DIMENSION,
                            MSKU.SKU_SIZE  AS SKU_SIZE,
                            MSKU.SKU_ID    AS SKU_ID
                    FROM MASTER_SKU MSKU
                    where msku.inactive_flag is null
                    ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2

                ]]>
                ", row => new
                 {
                     SourceStyle = row.GetValue<string>("STYLE"),
                     SourceColor = row.GetValue<string>("COLOR"),
                     SourceDimension = row.GetValue<string>("DIMENSION"),
                     SourceSize = row.GetValue<string>("SKU_SIZE"),
                     SourceSkuID = row.GetValue<int>("SKU_ID")
                 }).ExecuteSingle(_db);


            if (expectedSourceSku == null)
            {
                Assert.Inconclusive("No Source Sku Found");
            }

            //popullating request model
            var expectedRequestSku = new RequestSkuModel
            {
                Pieces = -1,

                SourceSku = new SkuModel
                {
                    SkuId = expectedSourceSku.SourceSkuID
                },
                TargetSku = new SkuModel
                {
                    SkuId = -101
                }

            };



            //calling repository function
            target.AddSkutoRequest(expectedRequestSku, expectedCartonReserveId.CartonReserveId);


        }

        /// <summary>
        /// This test case is verifying repositories function UnAssignCartons().
        /// In this function we passed null as CtnResvId as parameter to function UnAssignCartons() 
        /// and check it must throw exception.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void UnassignCartonNullId()
        {
            //calling repository function
            target.UnAssignCartons(null);

        }


        /// <summary>
        /// This test case is verifying repositories function AssignCartons().
        /// In this function we passed null as CtnResvId as parameter to function AssignCartons() 
        /// and check it must throw exception.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void AssignCartonNullId()
        {

            //Calling Assigning cartons function
            target.AssignCartons(null);

        }

        /// <summary>
        /// This test case is verifying repositories function AssignCartons().
        /// In this function we passed an invalid CtnResvId as parameter to function AssignCartons() 
        /// and check it must throw exception.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void AssignCartonInvalidId()
        {

            //Calling Assigning cartons function
            target.AssignCartons("Ankit");

        }


    }
}
