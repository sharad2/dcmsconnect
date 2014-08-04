using System.Data.Common;
using DcmsMobile.Repack.Repository;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DcmsMobile.Repack.Models;




namespace Repack.Test
{


    /// <summary>
    ///This is a test class for RepackRepositoryTest and is intended
    ///to contain all RepackRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RepackRepositoryTest
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
        private static OracleDatastore _db1;

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            _db = new OracleDatastore(null);
           _db1 = new OracleDatastore(null);
           _db.CreateConnection("Data Source=w8bhutan/mfdev;User Id=dcms4;Password=DDM", "rpk");
           _db1.CreateConnection("Data Source=w8bhutan/mfdev;User Id=dcms4;Password=DDM", "");           
            _db.ModuleName = "rpk";
            _db1.ModuleName = "rpk";
            
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            _db.Dispose();
            _db1.Dispose();
        }
        //

        private RepackRepository _target;
        private DbTransaction _trans;

        private RepackRepository _target1;
        private DbTransaction _trans1;

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _target = new RepackRepository(_db);
            _trans = _db.BeginTransaction();

            _target1 = new RepackRepository(_db1);
            _trans1 = _db1.BeginTransaction();
        }

        //
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {

            _trans.Rollback();
            _trans.Dispose();

            _trans1.Rollback();
            _trans1.Dispose();
        }
        

        #endregion


        const string palletId = "P007";


        /// <summary>
        /// This function will verifies the data which was created either by function RepackSingleCarton()
        /// RepackBulkCarton().
        /// This function will validates the data from Src_carton, Src_carton_Detail, Src_carton_process_detail, src_transaction & src_transaction_detail tables.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="CartonId"></param>
        /// <param name="DestinationQuantity"></param>
        /// <param name="SourceQuantity"></param>
        /// <param name="Color"></param>
        /// <param name="Dimension"></param>
        /// <param name="SkuSize"></param>
        /// <param name="Style"></param>
        public void ValidateRepackedCarton(CartonRepackInfo Info,
            string CartonId,
            int DestinationQuantity,
            int SourceQuantity,
            string Color,
            string Dimension,
            string SkuSize,
            string Style)
        {

            var x = _db.Connection.ServerVersion;
            // Retrieves the  repacked carton from src_carton and src_carton_detail tables.
            var actualSrcCarton = SqlBinder.Create(
                @"
<![CDATA[
SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
       CTN.VWH_ID              AS VWH_ID,
       CTN.QUALITY_CODE        AS QUALITY_CODE,      
       CTN.PALLET_ID           AS PALLET_ID,
       CTN.SHIPMENT_ID         AS SHIPMENT_ID, 
       CTN.PRICE_SEASON_CODE   AS PRICE_SEASON_CODE,
       CTN.SEWING_PLANT_CODE   AS SEWING_PLANT_CODE,
       CTN.REMARK_WORK_NEEDED  AS REMARK_WORK_NEEDED,
       CTNDET.SKU_ID           AS SKU_ID,
       CTNDET.QUANTITY         AS QUANTITY,
       CTNDET.STYLE            AS STYLE,
       CTNDET.COLOR            AS COLOR,
       CTNDET.DIMENSION        AS DIMENSION,
       CTNDET.SKU_SIZE         AS SKU_SIZE,
       CTNDET.REQ_PROCESS_ID   AS REQ_PROCESS_ID,
       CTNDET.REQ_MODULE_CODE  AS REQ_MODULE_CODE,
       CTNDET.REQ_LINE_NUMBER AS REQ_LINE_NUMBER       
  FROM SRC_CARTON CTN
 INNER JOIN SRC_CARTON_DETAIL CTNDET
    ON CTN.CARTON_ID = CTNDET.CARTON_ID
 WHERE CTN.CARTON_ID = :CARTON_ID
]]>
", row => new
 {
     CartonStorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     VwhId = row.GetValue<string>("VWH_ID"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     Pieces = row.GetValue<int>("QUANTITY"),
     SkuId = row.GetValue<int>("SKU_ID"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     Size = row.GetValue<string>("SKU_SIZE"),
     ShipmentId = row.GetValue<string>("SHIPMENT_ID"),
     PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
     PalletId = row.GetValue<string>("PALLET_ID"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     ReqModuleCode = row.GetValue<string>("REQ_MODULE_CODE"),
     ReqLineNumber = row.GetValue<int?>("REQ_LINE_NUMBER"),
     ReqProcessId = row.GetValue<int?>("REQ_PROCESS_ID"),
     Remarks = row.GetValue<string>("REMARK_WORK_NEEDED")
 }).Parameter("CARTON_ID", CartonId)
                .ExecuteSingle(_db);
            Assert.IsNotNull(actualSrcCarton, "carton not received");
            Assert.AreEqual(Info.DestinationCartonArea, actualSrcCarton.CartonStorageArea, "Carton Area does not matched in src_carton ");
            Assert.AreEqual(Info.QualityCode, actualSrcCarton.QualityCode, "qualityCode does not matched in src_carton ");
            Assert.AreEqual(Info.VwhId, actualSrcCarton.VwhId, "vwhId does not matched in src_carton ");
            Assert.AreEqual(Info.Pieces, actualSrcCarton.Pieces, "Pieces does not matched in src_carton_detail");
            Assert.AreEqual(Info.SkuId, actualSrcCarton.SkuId, "skuId does not matched in src_carton_detail");
            Assert.AreEqual(Style, actualSrcCarton.Style, "skuId does not matched in src_carton_detail");
            Assert.AreEqual(Color, actualSrcCarton.Color, "skuId does not matched in src_carton_detail");
            Assert.AreEqual(Dimension, actualSrcCarton.Dimension, "skuId does not matched in src_carton_detail");
            Assert.AreEqual(SkuSize, actualSrcCarton.Size, "skuId does not matched in src_carton_detail");            
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.PriceSeasonCode), "PriceSeason Code does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.SewingPlantCode), "sewing Plant Code does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ShipmentId), "Shipment Id does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqProcessId.ToString()), "Process Id does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqModuleCode), "Module Code does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqLineNumber.ToString()), "Line Number does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.Remarks), "Remarks should not br null at the time of repack");
            if (string.IsNullOrEmpty(Info.PalletId))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.PalletId), "PalletId does not matched in src_carton ");
            }

            if (!string.IsNullOrEmpty(Info.PalletId))
            {
                Assert.AreEqual(Info.PalletId,actualSrcCarton.PalletId, "PalletId does not matched in src_carton ");
            }


            // Retrieves the repacked carton from src_carton_process_detail table
            var actualSrcCartonProcess = SqlBinder.Create(
          @"
<![CDATA[
SELECT CPD.MODULE_CODE        AS MODULE_CODE,
       CPD.TO_CARTON_AREA     AS CARTON_STORAGE_AREA,
       CPD.VWH_ID             AS VWH_ID,
       CPD.DATABASE_OPERATION AS DATABASE_OPERATION,
       CPD.NEW_CARTON_QTY     AS QUANTITY,
       CPD.APPLICATION_ACTION AS APPLICATION_ACTION
  FROM SRC_CARTON_PROCESS_DETAIL CPD
 WHERE CPD.CARTON_ID = :CARTON_ID
 ]]>", row => new
     {
         ModuleCode = row.GetValue<string>("MODULE_CODE"),
         DestArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
         VwhId = row.GetValue<string>("VWH_ID"),
         DatabaseOperation = row.GetValue<string>("DATABASE_OPERATION"),
         CartonPieces = row.GetValue<int>("QUANTITY"),
         ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
     }).Parameter("CARTON_ID", CartonId)
          .ExecuteSingle(_db);
            Assert.IsNotNull(actualSrcCartonProcess, "Carton not found in src_carton_process_details");
            Assert.AreEqual(Info.DestinationCartonArea, actualSrcCartonProcess.DestArea, "destArea does not matched in src_carton_process_details");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", actualSrcCartonProcess.ApplicationAction, "ApplicationAction does not matched in src_carton_process_details");
                Assert.AreEqual("rpk", actualSrcCartonProcess.ModuleCode, "Module Code does not matched in src_carton_process_details");
            }
            Assert.AreEqual(Info.VwhId, actualSrcCartonProcess.VwhId, "vwhId does not matched in src_carton_process_details");
            Assert.AreEqual("INSERT", actualSrcCartonProcess.DatabaseOperation, "databaseOperation does not matched in src_carton_process_details");
            Assert.AreEqual(Info.Pieces, actualSrcCartonProcess.CartonPieces, "pieces does not matched in src_carton_process_details");


        
            
            // Retrieves the  -ve trancastion for repacked carton from src_transaction and src_transaction_detail tables
            var negativeTransaction = SqlBinder.Create(
                @"
              <![CDATA[
WITH q1 AS
(SELECT A.TRANSACTION_TYPE       AS TRANSACTION_TYPE,
       A.STYLE                  AS STYLE,
       A.COLOR                  AS COLOR,
       A.DIMENSION              AS DIMENSION,
       A.SKU_SIZE               AS SKU_SIZE,
       A.VWH_ID                 AS VWH_ID,
       A.SEWING_PLANT_CODE      AS SEWING_PLANT_CODE,
       A.QUALITY_CODE           AS QUALITY_CODE,
       A.APPLICATION_ACTION     AS APPLICATION_ACTION,
       B.INVENTORY_STORAGE_AREA AS SKU_STORAGE_AREA,
       B.TRANSACTION_PIECES     AS TRANSACTION_PIECES
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID 
AND B.TRANSACTION_PIECES IS NOT NULL
AND B.TRANSACTION_PIECES < 0   
 ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2 
               ]]>
                ", row => new
                 {
                     Transactiontype = row.GetValue<string>("TRANSACTION_TYPE"),
                     VwhId = row.GetValue<string>("VWH_ID"),
                     QualityCode = row.GetValue<string>("QUALITY_CODE"),
                     Style = row.GetValue<string>("STYLE"),
                     Color = row.GetValue<string>("COLOR"),
                     Dimension = row.GetValue<string>("DIMENSION"),
                     Size = row.GetValue<string>("SKU_SIZE"),
                     StorageArea = row.GetValue<string>("SKU_STORAGE_AREA"),
                     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
                     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
                     ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
                 }).Parameter("CARTON_ID", CartonId)
                  .ExecuteSingle(_db);
            Assert.IsNotNull(negativeTransaction, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(Style, negativeTransaction.Style, "style does not matched in src-tansaction table");
            Assert.AreEqual(Color, negativeTransaction.Color, "color does not matched in src-tansaction table  ");
            Assert.AreEqual(Dimension, negativeTransaction.Dimension, "dimension does not matched in src-tansaction table");
            Assert.AreEqual(SkuSize, negativeTransaction.Size, "size does not matched in src-tansaction table");
            Assert.AreEqual(Info.QualityCode, negativeTransaction.QualityCode, "quality Codedoes not matched in src-tansaction table");
            Assert.AreEqual("BCRE", negativeTransaction.Transactiontype, "transactiontype does not matched in src-tansaction table");
            Assert.AreEqual(Info.VwhId, negativeTransaction.VwhId, "vwhId does not matched in src-tansaction table");
            Assert.AreEqual(0, (negativeTransaction.TransactionPieces + Info.Pieces), "Transaction pieces does not matched in  src_transaction_detail table");
            Assert.AreEqual(Info.SourceSkuArea, negativeTransaction.StorageArea, "carton Area does not matched in src_transaction_detail table");
            Assert.IsTrue(string.IsNullOrEmpty(negativeTransaction.SewingPlantCode), "Sewing Plant does not matched in src_transaction");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", negativeTransaction.ApplicationAction, "ApplicationAction does not matched in src_transaction table");
            }

            // Retrieves the  +ve trancastion for repacked carton from src_transaction and src_transaction_detail tables
            var positiveTransaction = SqlBinder.Create(
                @"
 <![CDATA[
WITH q1 AS
(SELECT A.TRANSACTION_TYPE       AS TRANSACTION_TYPE,
       A.STYLE                  AS STYLE,
       A.COLOR                  AS COLOR,
       A.DIMENSION              AS DIMENSION,
       A.SKU_SIZE               AS SKU_SIZE,
       A.VWH_ID                 AS VWH_ID,
       A.SEWING_PLANT_CODE      AS SEWING_PLANT_CODE,
       A.APPLICATION_ACTION     AS APPLICATION_ACTION,
       B.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
       B.TRANSACTION_PIECES     AS TRANSACTION_PIECES,
       A.QUALITY_CODE           AS QUALITY_CODE
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID 
AND B.TRANSACTION_PIECES IS NOT NULL
AND B.TRANSACTION_PIECES > 0   
 ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2 
               ]]>
                ", row => new
                 {
                     Transactiontype = row.GetValue<string>("TRANSACTION_TYPE"),
                     VwhId = row.GetValue<string>("VWH_ID"),
                     QualityCode = row.GetValue<string>("QUALITY_CODE"),
                     Style = row.GetValue<string>("STYLE"),
                     Color = row.GetValue<string>("COLOR"),
                     Dimension = row.GetValue<string>("DIMENSION"),
                     Size = row.GetValue<string>("SKU_SIZE"),
                     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
                     StorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
                     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
                     ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
                 }).Parameter("CARTON_ID", CartonId)
                  .ExecuteSingle(_db);
            Assert.IsNotNull(positiveTransaction, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(Style, positiveTransaction.Style, "style does not matched in src-tansaction table");
            Assert.AreEqual(Color, positiveTransaction.Color, "color does not matched in src-tansaction table  ");
            Assert.AreEqual(Dimension, positiveTransaction.Dimension, "dimension does not matched in src-tansaction table");
            Assert.AreEqual(SkuSize, positiveTransaction.Size, "size does not matched in src-tansaction table");
            Assert.AreEqual(Info.QualityCode, positiveTransaction.QualityCode, "quality Codedoes not matched in src-tansaction table");
            Assert.AreEqual("BCRE", positiveTransaction.Transactiontype, "transactiontype does not matched in src-tansaction table");
            Assert.AreEqual(Info.VwhId, positiveTransaction.VwhId, "vwhId does not matched in src-tansaction table");
            Assert.AreEqual(Info.Pieces, positiveTransaction.TransactionPieces, "Transaction pieces does not matched in  src_transaction_detail table");
            Assert.AreEqual(Info.DestinationCartonArea, positiveTransaction.StorageArea, "carton Area does not matched in src_transaction_detail table");
            Assert.IsTrue(string.IsNullOrEmpty(negativeTransaction.SewingPlantCode), "Sewing Plant does not matched in src_transaction");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", positiveTransaction.ApplicationAction, "ApplicationAction does not matched in src_transaction table");
            }



            //Querying changed source area quantity in mri_raw_inventory table where Area is Source Sku area,
            //VwhId is Source VwhId,and quality code is source quality code.
            var changedSourceQuantity = SqlBinder.Create(
                @"
<![CDATA[
SELECT MRI.QUANTITY AS QUANTITY
  FROM MASTER_RAW_INVENTORY MRI
 WHERE MRI.SKU_STORAGE_AREA = :SKU_STORAGE_AREA
   AND MRI.VWH_ID = :SOURCE_VWH_ID
   AND MRI.SKU_ID = :SOURCE_SKU_ID
   AND MRI.QUALITY_CODE = :SOURCE_QUALITY_CODE
]]>
", row => new
 {
     ChangedSourceQuantity = row.GetValue<int>("QUANTITY")
 }).Parameter("SKU_STORAGE_AREA", Info.SourceSkuArea)
 .Parameter("SOURCE_VWH_ID", Info.VwhId)
 .Parameter("SOURCE_SKU_ID", Info.SkuId)
 .Parameter("SOURCE_QUALITY_CODE", Info.QualityCode)
 .ExecuteSingle(_db);

            //Asserting changed source quantity in master_raw_inventory .
            Assert.AreEqual((SourceQuantity - Info.NumberOfCartons * (Info.Pieces)), changedSourceQuantity.ChangedSourceQuantity, "quantity ");


            //Querying changed destination area quantity in mri_raw_inventory table  where Area is carton Sku area
            //VwhId is Source VwhId,and quality code is source quality code.
            var changedDestQuantity = SqlBinder.Create(
                @"
<![CDATA[
SELECT MRI.QUANTITY AS QUANTITY
  FROM MASTER_RAW_INVENTORY MRI
 WHERE MRI.SKU_STORAGE_AREA = :CARTON_STORAGE_AREA
   AND MRI.VWH_ID = :SOURCE_VWH_ID
   AND MRI.SKU_ID = :SOURCE_SKU_ID
   AND MRI.QUALITY_CODE = :SOURCE_QUALITY_CODE
]]>
", row => new
 {
     ChangedDestinationQuantity = row.GetValue<int>("QUANTITY")
 }).Parameter("CARTON_STORAGE_AREA", Info.DestinationCartonArea)
 .Parameter("SOURCE_VWH_ID", Info.VwhId)
 .Parameter("SOURCE_SKU_ID", Info.SkuId)
 .Parameter("SOURCE_QUALITY_CODE", Info.QualityCode)
 .ExecuteSingle(_db);

            //Asserting changed destination quantity in master_raw_inventory
            Assert.AreEqual((DestinationQuantity + Info.NumberOfCartons * (Info.Pieces)), changedDestQuantity.ChangedDestinationQuantity, "quantity ");


        }


        /// <summary>
        /// This function will verifies the data which was created by function ReceiveCarton().
        /// This function will validates the data from Src_carton, Src_carton_Detail, Src_carton_process_detail, src_transaction & src_transaction_detail tables.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="CartonId"></param>
        /// <param name="DestinationQuantity"></param>
        /// <param name="Color"></param>
        /// <param name="Dimension"></param>
        /// <param name="SkuSize"></param>
        /// <param name="Style"></param>
        public void ValidateReceivedCarton(CartonRepackInfo Info,
         string CartonId,
         int DestinationQuantity,
         int SourceQuantity,
         string Color,
         string Dimension,
         string SkuSize,
         string Style)
        {
            var x = _db.Connection.ServerVersion;
            // Retrieves the repacked carton from src_carton and src_carton_detail tables.
            var actualSrcCarton = SqlBinder.Create(
                @"
<![CDATA[
SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
       CTN.VWH_ID              AS VWH_ID,
       CTN.QUALITY_CODE        AS QUALITY_CODE,      
       CTN.PALLET_ID           AS PALLET_ID,
       CTN.SHIPMENT_ID         AS SHIPMENT_ID, 
       CTN.PRICE_SEASON_CODE   AS PRICE_SEASON_CODE,
       CTN.SEWING_PLANT_CODE   AS SEWING_PLANT_CODE,
       CTN.REMARK_WORK_NEEDED  AS REMARK_WORK_NEEDED,
       CTNDET.SKU_ID           AS SKU_ID,
       CTNDET.QUANTITY         AS QUANTITY,
       CTNDET.STYLE            AS STYLE,
       CTNDET.COLOR            AS COLOR,
       CTNDET.DIMENSION        AS DIMENSION,
       CTNDET.SKU_SIZE         AS SKU_SIZE,
       CTNDET.REQ_PROCESS_ID   AS REQ_PROCESS_ID,
       CTNDET.REQ_MODULE_CODE  AS REQ_MODULE_CODE,
       CTNDET.REQ_LINE_NUMBER AS REQ_LINE_NUMBER       
  FROM SRC_CARTON CTN
 INNER JOIN SRC_CARTON_DETAIL CTNDET
    ON CTN.CARTON_ID = CTNDET.CARTON_ID
 WHERE CTN.CARTON_ID = :CARTON_ID
]]>
", row => new
 {
     CartonStorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     VwhId = row.GetValue<string>("VWH_ID"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     Pieces = row.GetValue<int>("QUANTITY"),
     SkuId = row.GetValue<int>("SKU_ID"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     Size = row.GetValue<string>("SKU_SIZE"),
     ShipmentId = row.GetValue<string>("SHIPMENT_ID"),
     PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
     PalletId = row.GetValue<string>("PALLET_ID"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     ReqModuleCode = row.GetValue<string>("REQ_MODULE_CODE"),
     ReqLineNumber = row.GetValue<int?>("REQ_LINE_NUMBER"),
     ReqProcessId = row.GetValue<int?>("REQ_PROCESS_ID"),
     Remarks = row.GetValue<string>("REMARK_WORK_NEEDED")
 }).Parameter("CARTON_ID", CartonId)
                .ExecuteSingle(_db);
            Assert.IsNotNull(actualSrcCarton, "carton not received");
            Assert.AreEqual(Info.DestinationCartonArea, actualSrcCarton.CartonStorageArea, "Carton Area does not matched in src_carton ");
            Assert.AreEqual(Info.QualityCode, actualSrcCarton.QualityCode, "qualityCode does not matched in src_carton ");
            Assert.AreEqual(Info.VwhId, actualSrcCarton.VwhId, "vwhId does not matched in src_carton ");
            Assert.AreEqual(Info.Pieces, actualSrcCarton.Pieces, "Pieces does not matched in src_carton_detail");
            Assert.AreEqual(Info.SkuId, actualSrcCarton.SkuId, "skuId does not matched in src_carton_detail");
            Assert.AreEqual(Style, actualSrcCarton.Style, "skuId does not matched in src_carton_detail");
            Assert.AreEqual(Color, actualSrcCarton.Color, "skuId does not matched in src_carton_detail");
            Assert.AreEqual(Dimension, actualSrcCarton.Dimension, "skuId does not matched in src_carton_detail");
            Assert.AreEqual(SkuSize, actualSrcCarton.Size, "skuId does not matched in src_carton_detail");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.PriceSeasonCode), "PriceSeason Code does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.SewingPlantCode), "sewing Plant Code does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ShipmentId), "Shipment Id does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqProcessId.ToString()), "Process Id does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqModuleCode), "Module Code does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqLineNumber.ToString()), "Line Number does not matched in src_carton ");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.Remarks), "Remarks should not br null at the time of repack");
            if (string.IsNullOrEmpty(Info.PalletId))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.PalletId), "PalletId does not matched in src_carton ");
            }

            if (!string.IsNullOrEmpty(Info.PalletId))
            {
                Assert.AreEqual(Info.PalletId, actualSrcCarton.PalletId, "PalletId does not matched in src_carton ");
            }


            //Retrieves the  repacked carton from src_carton_process_detail table .
            var actualSrcCartonProcess = SqlBinder.Create(
          @"
<![CDATA[
SELECT CPD.MODULE_CODE        AS MODULE_CODE,
       CPD.TO_CARTON_AREA     AS CARTON_STORAGE_AREA,
       CPD.VWH_ID             AS VWH_ID,
       CPD.DATABASE_OPERATION AS DATABASE_OPERATION,
       CPD.NEW_CARTON_QTY     AS CARTON_QTY,
       CPD.APPLICATION_ACTION AS APPLICATION_ACTION
  FROM SRC_CARTON_PROCESS_DETAIL CPD
 WHERE CPD.CARTON_ID = :CARTON_ID
 ]]>", row => new
     {
         ModuleCode = row.GetValue<string>("MODULE_CODE"),
         DestArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
         VwhId = row.GetValue<string>("VWH_ID"),
         DatabaseOperation = row.GetValue<string>("DATABASE_OPERATION"),
         Quantity = row.GetValue<int>("CARTON_QTY"),
         ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
     }).Parameter("CARTON_ID", CartonId)
          .ExecuteSingle(_db);
            Assert.IsNotNull(actualSrcCartonProcess, "Carton not found in src_carton_process_details");
            Assert.AreEqual(Info.DestinationCartonArea, actualSrcCartonProcess.DestArea, "Carton area does not matched in  src_carton_process_detail ");
          
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", actualSrcCartonProcess.ApplicationAction, "ApplicationAction does not matched in src_carton_process_detail");
                Assert.AreEqual("rpk", actualSrcCartonProcess.ModuleCode, "moduleCode  does not matched in src_carton_process_detail");
            }
            Assert.AreEqual(Info.VwhId, actualSrcCartonProcess.VwhId, "vwhId  does not matched in src_carton_process_detail");
            Assert.AreEqual("INSERT", actualSrcCartonProcess.DatabaseOperation, "databaseOperation  does not matched in src_carton_process_detail");
            Assert.AreEqual(Info.Pieces, actualSrcCartonProcess.Quantity, "pieces does not matched in src_carton_process_detail");
            


            // Retrieves the  +ve trancastion for repacked carton from src_transaction and src_transaction_detail tables
            var positiveTransaction = SqlBinder.Create(
                @"
 <![CDATA[
WITH q1 AS
(SELECT A.TRANSACTION_TYPE       AS TRANSACTION_TYPE,
       A.STYLE                  AS STYLE,
       A.COLOR                  AS COLOR,
       A.DIMENSION              AS DIMENSION,
       A.SKU_SIZE               AS SKU_SIZE,
       A.VWH_ID                 AS VWH_ID,
       A.SEWING_PLANT_CODE      AS SEWING_PLANT_CODE,
       A.APPLICATION_ACTION     AS APPLICATION_ACTION,
       B.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
       B.TRANSACTION_PIECES     AS TRANSACTION_PIECES,
       A.QUALITY_CODE           AS QUALITY_CODE
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID 
AND B.TRANSACTION_PIECES IS NOT NULL
AND B.TRANSACTION_PIECES > 0   
 ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2 
               ]]>
                ", row => new
                 {
                     Transactiontype = row.GetValue<string>("TRANSACTION_TYPE"),
                     VwhId = row.GetValue<string>("VWH_ID"),
                     QualityCode = row.GetValue<string>("QUALITY_CODE"),
                     Style = row.GetValue<string>("STYLE"),
                     Color = row.GetValue<string>("COLOR"),
                     Dimension = row.GetValue<string>("DIMENSION"),
                     Size = row.GetValue<string>("SKU_SIZE"),
                     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
                     StorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
                     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
                     ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
                 }).Parameter("CARTON_ID", CartonId)
                  .ExecuteSingle(_db);
            Assert.IsNotNull(positiveTransaction, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(Style, positiveTransaction.Style, "style does not matched in src-tansaction table");
            Assert.AreEqual(Color, positiveTransaction.Color, "color does not matched in src-tansaction table  ");
            Assert.AreEqual(Dimension, positiveTransaction.Dimension, "dimension does not matched in src-tansaction table");
            Assert.AreEqual(SkuSize, positiveTransaction.Size, "size does not matched in src-tansaction table");
            Assert.AreEqual(Info.QualityCode, positiveTransaction.QualityCode, "quality Codedoes not matched in src-tansaction table");
            Assert.AreEqual("BCRE", positiveTransaction.Transactiontype, "transactiontype does not matched in src-tansaction table");
            Assert.AreEqual(Info.VwhId, positiveTransaction.VwhId, "vwhId does not matched in src-tansaction table");
            Assert.AreEqual(Info.Pieces, positiveTransaction.TransactionPieces, "Transaction pieces does not matched in  src_transaction_detail table");
            Assert.AreEqual(Info.DestinationCartonArea, positiveTransaction.StorageArea, "carton Area does not matched in src_transaction_detail table");
            Assert.IsTrue(string.IsNullOrEmpty(positiveTransaction.SewingPlantCode), "Sewing Plant does not matched in src_transaction");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", positiveTransaction.ApplicationAction, "ApplicationAction does not matched in src_transaction table");
            }



            //Querying changed source area quantity in mri_raw_inventory table where Area is Source Sku area,
            //VwhId is Source VwhId,and quality code is source quality code.
            var changedSourceQuantity = SqlBinder.Create(
                @"
<![CDATA[
SELECT MRI.QUANTITY AS QUANTITY
  FROM MASTER_RAW_INVENTORY MRI
 WHERE MRI.SKU_STORAGE_AREA = :SKU_STORAGE_AREA
   AND MRI.VWH_ID = :SOURCE_VWH_ID
   AND MRI.SKU_ID = :SOURCE_SKU_ID
   AND MRI.QUALITY_CODE = :SOURCE_QUALITY_CODE
]]>
", row => new
 {
     ChangedSourceQuantity = row.GetValue<int>("QUANTITY")
 }).Parameter("SKU_STORAGE_AREA", Info.SourceSkuArea)
 .Parameter("SOURCE_VWH_ID", Info.VwhId)
 .Parameter("SOURCE_SKU_ID", Info.SkuId)
 .Parameter("SOURCE_QUALITY_CODE", Info.QualityCode)
 .ExecuteSingle(_db);

            //Asserting changed source quantity in master_raw_inventory .
            Assert.AreEqual((SourceQuantity - Info.NumberOfCartons * (Info.Pieces)), changedSourceQuantity.ChangedSourceQuantity, "quantity ");




            //Querying changed destination area quantity in mri_raw_inventory table where Storage area is selected carton area
            //vwhId is selected source vwhId and quality Code is selected Source Quality Code
            var changedDestQuantity = SqlBinder.Create(
                @"
<![CDATA[
SELECT MRI.QUANTITY AS QUANTITY
  FROM MASTER_RAW_INVENTORY MRI
 WHERE MRI.SKU_STORAGE_AREA = :CARTON_STORAGE_AREA
   AND MRI.VWH_ID = :SOURCE_VWH_ID
   AND MRI.SKU_ID = :SOURCE_SKU_ID
   AND MRI.QUALITY_CODE = :SOURCE_QUALITY_CODE
]]>
", row => new
 {
     ChangedDestinationQuantity = row.GetValue<int>("QUANTITY")
 }).Parameter("CARTON_STORAGE_AREA", Info.DestinationCartonArea)
 .Parameter("SOURCE_VWH_ID", Info.VwhId)
 .Parameter("SOURCE_SKU_ID", Info.SkuId)
 .Parameter("SOURCE_QUALITY_CODE", Info.QualityCode)
 .ExecuteSingle(_db);

            //Asserting changed destination quantity in master_raw_inventory
            Assert.AreEqual((DestinationQuantity + Info.NumberOfCartons * (Info.Pieces)), changedDestQuantity.ChangedDestinationQuantity, "quantity ");


        }

        /// <summary>
        /// This function will verifies the data which was created either by the RepackCartonForConversionSameVwh() or RepackBulkCartonForConversionSameVwh() functions.
        /// This function validates the data in Src_carton, Src_carton_Detail, Src_carton_process_detail, src_transaction & src_transaction_detail tables.
        /// </summary>
        /// <param name="Info"></param>
        /// <param name="CartonId"></param>
        /// <param name="SourceStyle"></param>
        /// <param name="SourceColor"></param>
        /// <param name="SourceDimension"></param>
        /// <param name="SourceSize"></param>
        /// <param name="TargetStyle"></param>
        /// <param name="TargetColor"></param>
        /// <param name="TargetDimension"></param>
        /// <param name="TargetSize"></param>
        /// <param name="BestQuality"></param>
        /// <param name="PreConversionQuality"></param>
        /// <param name="DestQuantity"></param>
        /// <param name="SourceQuantity"></param>
        public void ValidateRepackedConvertedCartonSameVwh(CartonRepackInfo Info,
            string CartonId,
            string SourceStyle,
            string SourceColor,
            string SourceDimension,
            string SourceSize,
            string TargetStyle,
            string TargetColor,
            string TargetDimension,
            string TargetSize,
            string BestQuality,
            string PreConversionQuality,
            int DestQuantity,
            int SourceQuantity)
        {
            var x = _db.Connection.ServerVersion;
            // Retrieves the repacked carton from src_carton and src_carton_detail tables.
            var actualSrcCarton = SqlBinder.Create(
                @"
<![CDATA[
SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
       CTN.VWH_ID              AS SOURCE_VWH_ID,
       CTN.PALLET_ID           AS PALLET_ID,
       CTN.PRICE_SEASON_CODE   AS PRICE_SEASON_CODE,
       CTN.SHIPMENT_ID         AS SHIPMENT_ID,
       CTN.SEWING_PLANT_CODE   AS SEWING_PLANT_CODE,
       CTN.QUALITY_CODE        AS QUALITY_CODE,
       CTN.REMARK_WORK_NEEDED  AS REMARK_WORK_NEEDED,
       CTNDET.SKU_ID           AS TARGET_SKU_ID,
       CTNDET.QUANTITY         AS QUANTITY,
       CTNDET.STYLE            AS STYLE,
       CTNDET.COLOR            AS COLOR,
       CTNDET.DIMENSION        AS DIMENSION,
       CTNDET.SKU_SIZE         AS SKU_SIZE,
       CTNDET.REQ_PROCESS_ID   AS REQ_PROCESS_ID,
       CTNDET.REQ_MODULE_CODE  AS REQ_MODULE_CODE,
       CTNDET.REQ_LINE_NUMBER  AS REQ_LINE_NUMBER
       FROM SRC_CARTON CTN
 INNER JOIN SRC_CARTON_DETAIL CTNDET
    ON CTN.CARTON_ID = CTNDET.CARTON_ID
 WHERE CTN.CARTON_ID = :CARTON_ID
]]>
", row => new
 {
     CartonStorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     VwhId = row.GetValue<string>("SOURCE_VWH_ID"),
     SkuId = row.GetValue<int>("TARGET_SKU_ID"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     Pieces = row.GetValue<int>("QUANTITY"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     Size = row.GetValue<string>("SKU_SIZE"),
     ShipmentId = row.GetValue<string>("SHIPMENT_ID"),
     PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
     PalletId = row.GetValue<string>("PALLET_ID"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     ReqModuleCode = row.GetValue<string>("REQ_MODULE_CODE"),
     ReqLineNumber = row.GetValue<int?>("REQ_LINE_NUMBER"),
     ReqProcessId = row.GetValue<int?>("REQ_PROCESS_ID"),
     Remark = row.GetValue<string>("REMARK_WORK_NEEDED")
 }).Parameter("CARTON_ID", CartonId)
                .ExecuteSingle(_db);
            Assert.IsNotNull(actualSrcCarton, "carton not received");
            Assert.AreEqual(Info.DestinationCartonArea, actualSrcCarton.CartonStorageArea, "carton area does not matched in src_carton");
            Assert.AreEqual(Info.VwhId, actualSrcCarton.VwhId, "vwhId does not matched in src_carton");
            Assert.AreEqual(Info.Pieces, actualSrcCarton.Pieces, "Pieces does not matched in src_carton_detail");
            Assert.AreEqual(Info.TartgetSkuId, actualSrcCarton.SkuId, "sku does not matched in src_carton_detal");
            Assert.AreEqual(TargetStyle, actualSrcCarton.Style, "Style does not matched in src_carton_detal");
            Assert.AreEqual(TargetColor, actualSrcCarton.Color, "color does not matched in src_carton_detal");
            Assert.AreEqual(TargetDimension, actualSrcCarton.Dimension, "Dimension does not matched in src_carton_detal");
            Assert.AreEqual(TargetSize, actualSrcCarton.Size, "Size does not matched in src_carton_detal");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqLineNumber.ToString()), "req_Line_number does not matched in src_carton_detal");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqProcessId.ToString()), "req_Process_Id does not matched in src_carton_detal");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqModuleCode.ToString()), "req_module_Code does not matched in src_carton_detal");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.PalletId.ToString()), "Pallet Id does not matched in src_carton");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.PriceSeasonCode.ToString()), "PriceSeasonCode does not matched in src_carton");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.SewingPlantCode.ToString()), "SewingPlantCode does not matched in src_carton");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ShipmentId.ToString()), "ShipmentId does not matched in src_carton");
            Assert.AreEqual(PreConversionQuality, actualSrcCarton.QualityCode, "QualityCode does not matched in src_carton");
            Assert.IsNotNull(actualSrcCarton.Remark, " Remark is null in Src _carton");
            if (string.IsNullOrEmpty(Info.PalletId))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.PalletId), "PalletId does not matched in src_carton ");
            }

            if (!string.IsNullOrEmpty(Info.PalletId))
            {
                Assert.AreEqual(Info.PalletId, actualSrcCarton.PalletId, "PalletId does not matched in src_carton ");
            }



            // Retrieve the repacked carton from src_carton_process_detail table
        var actualSrcCartonProcess = SqlBinder.Create(
          @"
<![CDATA[
SELECT CPD.MODULE_CODE        AS MODULE_CODE,
       CPD.TO_CARTON_AREA     AS CARTON_STORAGE_AREA,
       CPD.VWH_ID             AS VWH_ID,
       CPD.DATABASE_OPERATION AS DATABASE_OPERATION,
       CPD.NEW_CARTON_QTY     AS QUANTITY,
       CPD.APPLICATION_ACTION AS APPLICATION_ACTION
  FROM SRC_CARTON_PROCESS_DETAIL CPD
 WHERE CPD.CARTON_ID = :CARTON_ID
 ]]>", row => new
     {
         ModuleCode = row.GetValue<string>("MODULE_CODE"),
         DestArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
         VwhId = row.GetValue<string>("VWH_ID"),
         DatabaseOperation = row.GetValue<string>("DATABASE_OPERATION"),
         CartonPieces = row.GetValue<int>("QUANTITY"),
         ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
     }).Parameter("CARTON_ID", CartonId)
          .ExecuteSingle(_db);
            Assert.IsNotNull(actualSrcCartonProcess, "Carton not found in src_carton_process_details");
            Assert.AreEqual(Info.DestinationCartonArea, actualSrcCartonProcess.DestArea, "destArea does not matched in src_carton_process_details");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", actualSrcCartonProcess.ApplicationAction, "ApplicationAction does not matched in src_carton_process_details");
                Assert.AreEqual("rpk", actualSrcCartonProcess.ModuleCode, "Module Code does not matched in src_carton_process_details");
            }
            Assert.AreEqual(Info.VwhId, actualSrcCartonProcess.VwhId, "vwhId does not matched in src_carton_process_details");
            Assert.AreEqual("INSERT", actualSrcCartonProcess.DatabaseOperation, "databaseOperation does not matched in src_carton_process_details");
            Assert.AreEqual(Info.Pieces, actualSrcCartonProcess.CartonPieces, "pieces does not matched in src_carton_process_details");
            
            // Retrieves the 'BCRE' -ve trancastion for repacked carton from src_transaction and src_transaction_detail tables
            var negativeTransactionBCRE = SqlBinder.Create(
                @"
              <![CDATA[
WITH q1 AS (
SELECT A.STYLE                  AS STYLE,
       A.COLOR                  AS COLOR,
       A.DIMENSION              AS DIMENSION,
       A.SKU_SIZE               AS SKU_SIZE,
       A.VWH_ID                 AS VWH_ID,
       B.INVENTORY_STORAGE_AREA AS SKU_STORAGE_AREA,
       B.TRANSACTION_PIECES     AS TRANSACTION_PIECES,
       A.APPLICATION_ACTION     AS APPLICATION_ACTION,
       A.QUALITY_CODE           AS QUALITY_CODE
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID
   AND A.TRANSACTION_TYPE = 'BCRE'
   AND B.TRANSACTION_PIECES IS NOT NULL
   AND B.TRANSACTION_PIECES < 0
ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2
               ]]>
                ", row => new
                 {
                     VwhId = row.GetValue<string>("VWH_ID"),
                     QualityCode = row.GetValue<string>("QUALITY_CODE"),
                     Style = row.GetValue<string>("STYLE"),
                     Color = row.GetValue<string>("COLOR"),
                     Dimension = row.GetValue<string>("DIMENSION"),
                     Size = row.GetValue<string>("SKU_SIZE"),
                     StorageArea = row.GetValue<string>("SKU_STORAGE_AREA"),
                     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
                     ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
                 }).Parameter("CARTON_ID", CartonId)
                  .ExecuteSingle(_db);
            Assert.IsNotNull(negativeTransactionBCRE, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(SourceStyle, negativeTransactionBCRE.Style, "style does not matched in src-tansaction table");
            Assert.AreEqual(SourceColor, negativeTransactionBCRE.Color, "color does not matched in src-tansaction table  ");
            Assert.AreEqual(SourceDimension, negativeTransactionBCRE.Dimension, "dimension does not matched in src-tansaction table");
            Assert.AreEqual(SourceSize, negativeTransactionBCRE.Size, "size does not matched in src-tansaction table");
            Assert.AreEqual(BestQuality, negativeTransactionBCRE.QualityCode, "quality Codedoes not matched in src-tansaction table");
            Assert.AreEqual(Info.VwhId, negativeTransactionBCRE.VwhId, "vwhId does not matched in src-tansaction table");
            Assert.AreEqual(0, (negativeTransactionBCRE.TransactionPieces + Info.Pieces), "Transaction pieces does not matched in  src_transaction_detail table");
            Assert.AreEqual(Info.SourceSkuArea, negativeTransactionBCRE.StorageArea, "carton Area does not matched in src_transaction_detail table");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", negativeTransactionBCRE.ApplicationAction, "ApplicationAction does not matched in src_carton_process_details");
            }

            // Verifiying  the 'BCRE' +ve trancastion for repacked carton from src_transaction and src_transaction_detail tables.
            var positiveTransactionBcre = SqlBinder.Create(
                  @"
              <![CDATA[
WITH q1 AS
(SELECT A.STYLE                 AS STYLE,
       A.COLOR                  AS COLOR,
       A.DIMENSION              AS DIMENSION,
       A.SKU_SIZE               AS SKU_SIZE,
       A.VWH_ID                 AS VWH_ID,
       B.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
       B.TRANSACTION_PIECES     AS TRANSACTION_PIECES,
       A.APPLICATION_ACTION     AS APPLICATION_ACTION,
       A.QUALITY_CODE           AS QUALITY_CODE
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID 
AND A.TRANSACTION_TYPE = 'BCRE' 
AND B.TRANSACTION_PIECES IS NOT NULL
AND B.TRANSACTION_PIECES > 0   
ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2
               ]]>
                ", row => new
                   {
                       VwhId = row.GetValue<string>("VWH_ID"),
                       QualityCode = row.GetValue<string>("QUALITY_CODE"),
                       Style = row.GetValue<string>("STYLE"),
                       Color = row.GetValue<string>("COLOR"),
                       Dimension = row.GetValue<string>("DIMENSION"),
                       Size = row.GetValue<string>("SKU_SIZE"),
                       StorageArea1 = row.GetValue<string>("CARTON_STORAGE_AREA"),
                       TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
                       ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
                   }).Parameter("CARTON_ID", CartonId)
                    .ExecuteSingle(_db);
            Assert.IsNotNull(positiveTransactionBcre, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(SourceStyle, positiveTransactionBcre.Style, "style does not matched in src-tansaction table");
            Assert.AreEqual(SourceColor, positiveTransactionBcre.Color, "color does not matched in src-tansaction table  ");
            Assert.AreEqual(SourceDimension, positiveTransactionBcre.Dimension, "dimension does not matched in src-tansaction table");
            Assert.AreEqual(SourceSize, positiveTransactionBcre.Size, "size does not matched in src-tansaction table");
            Assert.AreEqual(BestQuality, positiveTransactionBcre.QualityCode, "quality Codedoes not matched in src-tansaction table");
            Assert.AreEqual(Info.VwhId, positiveTransactionBcre.VwhId, "vwhId does not matched in src-tansaction table");
            Assert.AreEqual(Info.Pieces, positiveTransactionBcre.TransactionPieces, "Transaction pieces does not matched in  src_transaction_detail table");
            Assert.AreEqual(Info.DestinationCartonArea, positiveTransactionBcre.StorageArea1, "carton Area does not matched in src_transaction_detail table");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", positiveTransactionBcre.ApplicationAction, "ApplicationAction does not matched in src_carton_process_details");
            }

            // Verifiying the 'IXFR' -ve trancastion for repacked carton from src_transaction and src_transaction_detail tables.
            var negativeTransactionIxfr = SqlBinder.Create(
                @"
              <![CDATA[
WITH q1 AS
(SELECT A.STYLE                  AS STYLE,
       A.COLOR                  AS COLOR,
       A.DIMENSION              AS DIMENSION,
       A.SKU_SIZE               AS SKU_SIZE,
       A.VWH_ID                 AS VWH_ID,
       B.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
       B.TRANSACTION_PIECES     AS TRANSACTION_PIECES,
       A.APPLICATION_ACTION     AS APPLICATION_ACTION,
       A.QUALITY_CODE           AS QUALITY_CODE
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID
   AND A.TRANSACTION_TYPE = 'IXFR'
   AND B.TRANSACTION_PIECES IS NOT NULL
   AND B.TRANSACTION_PIECES < 0
 ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2
]]>
                ", row => new
                 {
                     VwhId = row.GetValue<string>("VWH_ID"),
                     QualityCode = row.GetValue<string>("QUALITY_CODE"),
                     Style = row.GetValue<string>("STYLE"),
                     Color = row.GetValue<string>("COLOR"),
                     Dimension = row.GetValue<string>("DIMENSION"),
                     Size = row.GetValue<string>("SKU_SIZE"),
                     StorageArea2 = row.GetValue<string>("CARTON_STORAGE_AREA"),
                     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
                     ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
                 }).Parameter("CARTON_ID", CartonId)
                  .ExecuteSingle(_db);
            Assert.IsNotNull(negativeTransactionIxfr, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(SourceStyle, negativeTransactionIxfr.Style, "style does not matched in src-tansaction table");
            Assert.AreEqual(SourceColor, negativeTransactionIxfr.Color, "color does not matched in src-tansaction table  ");
            Assert.AreEqual(SourceDimension, negativeTransactionIxfr.Dimension, "dimension does not matched in src-tansaction table");
            Assert.AreEqual(SourceSize, negativeTransactionIxfr.Size, "size does not matched in src-tansaction table");
            Assert.AreEqual(BestQuality, negativeTransactionIxfr.QualityCode, "quality Codedoes not matched in src-tansaction table");
            Assert.AreEqual(Info.VwhId, negativeTransactionIxfr.VwhId, "vwhId does not matched in src-tansaction table");
            Assert.AreEqual(0, (negativeTransactionIxfr.TransactionPieces + Info.Pieces), "Transaction pieces does not matched in  src_transaction_detail table");
            Assert.AreEqual(Info.DestinationCartonArea, negativeTransactionIxfr.StorageArea2, "carton Area does not matched in src_transaction_detail table");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", negativeTransactionIxfr.ApplicationAction, "ApplicationAction does not matched in src_carton_process_details");
            }
            // Verifiying the 'IXFR' +ve trancastion for repacked carton from src_transaction and src_transaction_detail tables.
            var positiveTransactionIxfr = SqlBinder.Create(
                @"
<![CDATA[
WITH q1 AS
(SELECT A.STYLE                 AS STYLE,
       A.COLOR                  AS COLOR,
       A.DIMENSION              AS DIMENSION,
       A.SKU_SIZE               AS SKU_SIZE,
       A.VWH_ID                 AS VWH_ID,
       B.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
       B.TRANSACTION_PIECES     AS TRANSACTION_PIECES,
       A.APPLICATION_ACTION     AS APPLICATION_ACTION,
       A.QUALITY_CODE           AS QUALITY_CODE
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID
   AND A.TRANSACTION_TYPE = 'IXFR'
   AND B.TRANSACTION_PIECES IS NOT NULL
   AND B.TRANSACTION_PIECES > 0
 ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2
]]>
", row => new
     {
         VwhId = row.GetValue<string>("VWH_ID"),
         QualityCode = row.GetValue<string>("QUALITY_CODE"),
         Style = row.GetValue<string>("STYLE"),
         Color = row.GetValue<string>("COLOR"),
         Dimension = row.GetValue<string>("DIMENSION"),
         Size = row.GetValue<string>("SKU_SIZE"),
         StorageArea3 = row.GetValue<string>("CARTON_STORAGE_AREA"),
         TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
         ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")

     }).Parameter("CARTON_ID", CartonId)
     .ExecuteSingle(_db);


            Assert.IsNotNull(positiveTransactionIxfr, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(SourceStyle, positiveTransactionIxfr.Style, "style does not matched in src-tansaction table");
            Assert.AreEqual(SourceColor, positiveTransactionIxfr.Color, "color does not matched in src-tansaction table  ");
            Assert.AreEqual(SourceDimension, positiveTransactionIxfr.Dimension, "dimension does not matched in src-tansaction table");
            Assert.AreEqual(SourceSize, positiveTransactionIxfr.Size, "size does not matched in src-tansaction table");
            Assert.AreEqual(PreConversionQuality, positiveTransactionIxfr.QualityCode, "quality Codedoes not matched in src-tansaction table");
            Assert.AreEqual(Info.VwhId, positiveTransactionIxfr.VwhId, "vwhId does not matched in src-tansaction table");
            Assert.AreEqual(Info.Pieces, positiveTransactionIxfr.TransactionPieces, "Transaction pieces does not matched in  src_transaction_detail table");
            Assert.AreEqual(Info.DestinationCartonArea, positiveTransactionBcre.StorageArea1, "carton Area does not matched in src_transaction_detail table");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", positiveTransactionIxfr.ApplicationAction, "ApplicationAction does not matched in src_carton_process_details");
            }

            // Verifiying the 'BDSC' -ve trancastion for repacked carton from src_transaction and src_transaction_detail tables.
            var negativeTransactionBdsc = SqlBinder.Create(
                @"
              <![CDATA[
WITH q1 AS
(SELECT A.STYLE                  AS STYLE,
       A.COLOR                  AS COLOR,
       A.DIMENSION              AS DIMENSION,
       A.SKU_SIZE               AS SKU_SIZE,
       A.VWH_ID                 AS VWH_ID,
       B.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
       B.TRANSACTION_PIECES     AS TRANSACTION_PIECES,
       A.APPLICATION_ACTION     AS APPLICATION_ACTION,
       A.QUALITY_CODE           AS QUALITY_CODE
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID
   AND A.TRANSACTION_TYPE = 'BDSC'
   AND B.TRANSACTION_PIECES IS NOT NULL
   AND B.TRANSACTION_PIECES < 0
ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2
               ]]>
                ", row => new
                 {
                     VwhId = row.GetValue<string>("VWH_ID"),
                     QualityCode = row.GetValue<string>("QUALITY_CODE"),
                     Style = row.GetValue<string>("STYLE"),
                     Color = row.GetValue<string>("COLOR"),
                     Dimension = row.GetValue<string>("DIMENSION"),
                     Size = row.GetValue<string>("SKU_SIZE"),
                     StorageArea4 = row.GetValue<string>("CARTON_STORAGE_AREA"),
                     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
                     ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
                 }).Parameter("CARTON_ID", CartonId)
                  .ExecuteSingle(_db);
            Assert.IsNotNull(negativeTransactionBdsc, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(SourceStyle, negativeTransactionBdsc.Style, "style does not matched in src-tansaction table");
            Assert.AreEqual(SourceColor, negativeTransactionBdsc.Color, "color does not matched in src-tansaction table  ");
            Assert.AreEqual(SourceDimension, negativeTransactionBdsc.Dimension, "dimension does not matched in src-tansaction table");
            Assert.AreEqual(SourceSize, negativeTransactionBdsc.Size, "size does not matched in src-tansaction table");
            Assert.AreEqual(PreConversionQuality, negativeTransactionBdsc.QualityCode, "quality Codedoes not matched in src-tansaction table");
            Assert.AreEqual(Info.VwhId, negativeTransactionBdsc.VwhId, "vwhId does not matched in src-tansaction table");
            Assert.AreEqual(0, (negativeTransactionBdsc.TransactionPieces + Info.Pieces), "Transaction pieces does not matched in  src_transaction_detail table");
            Assert.AreEqual(Info.DestinationCartonArea, negativeTransactionBdsc.StorageArea4, "carton Area does not matched in src_transaction_detail table");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", negativeTransactionBdsc.ApplicationAction, "ApplicationAction does not matched in src_carton_process_details");
            }


            // Verifiying the 'BDSC' +ve trancastion for repacked carton from src_transaction and src_transaction_detail tables.
            var positiveTransactionBdsc = SqlBinder.Create(
                  @"
              <![CDATA[
WITH q1 AS
(SELECT A.STYLE                  AS STYLE,
       A.COLOR                  AS COLOR,
       A.DIMENSION              AS DIMENSION,
       A.SKU_SIZE               AS SKU_SIZE,
       A.VWH_ID                 AS VWH_ID,
       B.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
       B.TRANSACTION_PIECES     AS TRANSACTION_PIECES,
       A.APPLICATION_ACTION     AS APPLICATION_ACTION,
       A.QUALITY_CODE           AS QUALITY_CODE
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID 
AND A.TRANSACTION_TYPE = 'BDSC'
AND B.TRANSACTION_PIECES IS NOT NULL
AND B.TRANSACTION_PIECES > 0   
ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2 
]]>
                ", row => new
                 {
                     VwhId = row.GetValue<string>("VWH_ID"),
                     QualityCode = row.GetValue<string>("QUALITY_CODE"),
                     Style = row.GetValue<string>("STYLE"),
                     Color = row.GetValue<string>("COLOR"),
                     Dimension = row.GetValue<string>("DIMENSION"),
                     Size = row.GetValue<string>("SKU_SIZE"),
                     StorageArea5 = row.GetValue<string>("CARTON_STORAGE_AREA"),
                     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
                     ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
                 }).Parameter("CARTON_ID", CartonId)
                    .ExecuteSingle(_db);
            Assert.IsNotNull(positiveTransactionBdsc, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(TargetStyle, positiveTransactionBdsc.Style, "style does not matched in src-tansaction table");
            Assert.AreEqual(TargetColor, positiveTransactionBdsc.Color, "color does not matched in src-tansaction table  ");
            Assert.AreEqual(TargetDimension, positiveTransactionBdsc.Dimension, "dimension does not matched in src-tansaction table");
            Assert.AreEqual(TargetSize, positiveTransactionBdsc.Size, "size does not matched in src-tansaction table");
            Assert.AreEqual(PreConversionQuality, positiveTransactionBdsc.QualityCode, "quality Codedoes not matched in src-tansaction table");
            Assert.AreEqual(Info.VwhId, positiveTransactionBdsc.VwhId, "vwhId does not matched in src-tansaction table");
            Assert.AreEqual(Info.Pieces, positiveTransactionBdsc.TransactionPieces, "Transaction pieces does not matched in  src_transaction_detail table");
            Assert.AreEqual(Info.DestinationCartonArea, positiveTransactionBdsc.StorageArea5, "carton Area does not matched in src_transaction_detail table");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", positiveTransactionBdsc.ApplicationAction, "ApplicationAction does not matched in src_carton_process_details");
            }


            //Querying changed source area quantity in mri_raw_inventory table where storage area is selected SkuStorageArea
            //vwhId is source VwhID,SkuId is source SkuId and quality is BestQuality.
            var changedSourceQuantity = SqlBinder.Create(
                @"
<![CDATA[
SELECT MRI.QUANTITY AS QUANTITY
  FROM MASTER_RAW_INVENTORY MRI
 WHERE MRI.SKU_STORAGE_AREA = :SKU_STORAGE_AREA
   AND MRI.VWH_ID = :SOURCE_VWH_ID
   AND MRI.SKU_ID = :SOURCE_SKU_ID
   AND MRI.QUALITY_CODE = :BEST_QUALITY_CODE
]]>
", row => new
 {
     SourceQuantity = row.GetValue<int>("QUANTITY")
 }).Parameter("SKU_STORAGE_AREA", Info.SourceSkuArea)
 .Parameter("SOURCE_VWH_ID", Info.VwhId)
 .Parameter("SOURCE_SKU_ID", Info.SkuId)
 .Parameter("BEST_QUALITY_CODE", BestQuality)
 .ExecuteSingle(_db);

            //Asserting changed source quantity in master_raw_inventory 
            Assert.AreEqual((SourceQuantity - (Info.NumberOfCartons * Info.Pieces)), changedSourceQuantity.SourceQuantity, "quantity ");


            //Querying changed destination area quantity in mri_raw_inventory table where storage area is selected CartonStorageArea
            //vwhId is source VwhID,SkuId is target SkuId and quality is preconversion Quality.
            var changedDestQuantity = SqlBinder.Create(
                @"
<![CDATA[
SELECT MRI.QUANTITY AS QUANTITY
  FROM MASTER_RAW_INVENTORY MRI
 WHERE MRI.SKU_STORAGE_AREA = :CTN_STORAGE_AREA
   AND MRI.VWH_ID = :SOURCE_VWH_ID
   AND MRI.SKU_ID = :TARGET_SKU_ID
   AND MRI.QUALITY_CODE = :PRE_CONVERSION_QUALITY_CODE
]]>
", row => new
 {
     DestQuantity = row.GetValue<int>("QUANTITY")
 }).Parameter("CTN_STORAGE_AREA", Info.DestinationCartonArea)
 .Parameter("SOURCE_VWH_ID", Info.VwhId)
 .Parameter("TARGET_SKU_ID", Info.TartgetSkuId)
 .Parameter("PRE_CONVERSION_QUALITY_CODE", PreConversionQuality)
 .ExecuteSingle(_db);

            //Asserting changed source quantity in master_raw_inventory
            Assert.AreEqual((DestQuantity + (Info.NumberOfCartons * Info.Pieces)), changedDestQuantity.DestQuantity, "quantity");
        }



        /// <summary>
        /// This function will validate data which was created either by the RepackCartonForConversionDiffVwh() or RepackBulkCartonForConversionDiffVwh() functions.
        /// This function validates the data in Src_carton, Src_carton_Detail, Src_carton_process_detail, src_transaction & src_transaction_detail tables.
        /// </summary>
        /// <param name="Info"></param>
        /// <param name="CartonId"></param>
        /// <param name="SourceStyle"></param>
        /// <param name="SourceColor"></param>
        /// <param name="SourceDimension"></param>
        /// <param name="SourceSize"></param>
        /// <param name="TargetStyle"></param>
        /// <param name="TargetColor"></param>
        /// <param name="TargetDimension"></param>
        /// <param name="TargetSize"></param>
        /// <param name="BestQuality"></param>
        /// <param name="PreConversionQuality"></param>
        /// <param name="DestQuantity"></param>
        /// <param name="SourceQuantity"></param>
        public void ValidateRepackedConvertedCartonDiffVwh(CartonRepackInfo Info,
            string CartonId,
            string SourceStyle,
            string SourceColor,
            string SourceDimension,
            string SourceSize,
            string TargetStyle,
            string TargetColor,
            string TargetDimension,
            string TargetSize,
            string BestQuality,
            string PreConversionQuality,
            int DestQuantity,
            int SourceQuantity)
        {
            var x = _db.Connection.ServerVersion;
            // Retrieves the repacked carton from src_carton and src_carton_detail tables.
            var actualSrcCarton = SqlBinder.Create(
                @"
<![CDATA[
SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
       CTN.VWH_ID              AS TARGET_VWH_ID,
       CTN.PALLET_ID           AS PALLET_ID,
       CTN.PRICE_SEASON_CODE   AS PRICE_SEASON_CODE,
       CTN.SHIPMENT_ID         AS SHIPMENT_ID,
       CTN.SEWING_PLANT_CODE   AS SEWING_PLANT_CODE,
       CTN.QUALITY_CODE        AS QUALITY_CODE,
       CTN.REMARK_WORK_NEEDED  AS REMARK_WORK_NEEDED,
       CTNDET.SKU_ID           AS TARGET_SKU_ID,
       CTNDET.QUANTITY         AS QUANTITY,
       CTNDET.STYLE            AS STYLE,
       CTNDET.COLOR            AS COLOR,
       CTNDET.DIMENSION        AS DIMENSION,
       CTNDET.SKU_SIZE         AS SKU_SIZE,
       CTNDET.REQ_PROCESS_ID   AS REQ_PROCESS_ID,
       CTNDET.REQ_MODULE_CODE  AS REQ_MODULE_CODE,
       CTNDET.REQ_LINE_NUMBER  AS REQ_LINE_NUMBER
  FROM SRC_CARTON CTN
 INNER JOIN SRC_CARTON_DETAIL CTNDET
    ON CTN.CARTON_ID = CTNDET.CARTON_ID
 WHERE CTN.CARTON_ID = :CARTON_ID
]]>
", row => new
 {
     CartonStorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     VwhId = row.GetValue<string>("TARGET_VWH_ID"),
     SkuId = row.GetValue<int>("TARGET_SKU_ID"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     Pieces = row.GetValue<int>("QUANTITY"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     Size = row.GetValue<string>("SKU_SIZE"),
     ShipmentId = row.GetValue<string>("SHIPMENT_ID"),
     PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
     PalletId = row.GetValue<string>("PALLET_ID"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     ReqProcessId = row.GetValue<int?>("REQ_PROCESS_ID"),
     ReqModuleCode = row.GetValue<string>("REQ_MODULE_CODE"),
     ReqLineNumber = row.GetValue<int?>("REQ_LINE_NUMBER"),
     Remark = row.GetValue<string>("REMARK_WORK_NEEDED")
 }).Parameter("CARTON_ID", CartonId)
                .ExecuteSingle(_db);
            Assert.IsNotNull(actualSrcCarton, "carton not received");
            Assert.AreEqual(Info.DestinationCartonArea, actualSrcCarton.CartonStorageArea, "carton area does not matched in src_carton");
            Assert.AreEqual(Info.TargetVWhId, actualSrcCarton.VwhId, "vwhId does not matched in src_carton");
            Assert.AreEqual(Info.Pieces, actualSrcCarton.Pieces, "Pieces does not matched in src_carton_detail");
            Assert.AreEqual(Info.TartgetSkuId, actualSrcCarton.SkuId, "sku does not matched in src_carton_detal");
            Assert.AreEqual(TargetStyle, actualSrcCarton.Style, "Style does not matched in src_carton_detal"); 
            Assert.AreEqual(TargetColor, actualSrcCarton.Color, "color does not matched in src_carton_detal");
            Assert.AreEqual(TargetDimension, actualSrcCarton.Dimension, "Dimension does not matched in src_carton_detal");
            Assert.AreEqual(TargetSize, actualSrcCarton.Size, "Size does not matched in src_carton_detal");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqProcessId.ToString()), "Process Id does not matched in src_carton_detal");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqModuleCode.ToString()), "module Code does not matched in src_carton_detal");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ReqLineNumber.ToString()), "Line number does not matched in src_carton_detal"); 
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.PriceSeasonCode.ToString()), "PriceSeasonCode does not matched in src_carton");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.SewingPlantCode.ToString()), "SewingPlantCode does not matched in src_carton");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.ShipmentId.ToString()), "ShipmentId does not matched in src_carton");
            Assert.AreEqual(PreConversionQuality, actualSrcCarton.QualityCode, "QualityCode does not matched in src_carton");
            Assert.IsNotNull(actualSrcCarton.Remark, " Remark is null in Src _carton");
            if (string.IsNullOrEmpty(Info.PalletId))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualSrcCarton.PalletId), "PalletId does not matched in src_carton ");
            }

            if (!string.IsNullOrEmpty(Info.PalletId))
            {
                Assert.AreEqual(Info.PalletId, actualSrcCarton.PalletId, "PalletId does not matched in src_carton ");
            }



            //Retrieves the repacked carton from src_carton_process_detail table for insert data operation.
            var actualSrcCartonProcessInsert = SqlBinder.Create(
          @"
<![CDATA[
SELECT CPD.MODULE_CODE        AS MODULE_CODE,
       CPD.TO_CARTON_AREA     AS TO_CARTON_AREA,
       CPD.FROM_CARTON_AREA   AS FROM_CARTON_AREA,
       CPD.VWH_ID             AS VWH_ID,
       CPD.NEW_CARTON_QTY     AS CARTON_QTY,
       CPD.APPLICATION_ACTION AS APPLICATION_ACTION
  FROM SRC_CARTON_PROCESS_DETAIL CPD
 WHERE CPD.CARTON_ID = :CARTON_ID 
AND CPD.DATABASE_OPERATION = 'INSERT'
 ]]>", row => new
     {
         ModuleCode = row.GetValue<string>("MODULE_CODE"),
         DestArea = row.GetValue<string>("TO_CARTON_AREA"),
         VwhId = row.GetValue<string>("VWH_ID"),
         Quantity = row.GetValue<int>("CARTON_QTY"),
         ApplicationAction = row.GetValue<string>("APPLICATION_ACTION"),
         SourceArea = row.GetValue<string>("FROM_CARTON_AREA")
     }).Parameter("CARTON_ID", CartonId)
          .ExecuteSingle(_db);
            Assert.IsNotNull(actualSrcCartonProcessInsert, "Carton not found in src_carton_process_details");
            Assert.AreEqual(Info.DestinationCartonArea, actualSrcCartonProcessInsert.DestArea, "Carton area does not matched in  src_carton_process_detail ");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("rpk", actualSrcCartonProcessInsert.ModuleCode, "moduleCode  does not matched in src_carton_process_detail");
                Assert.AreEqual("Repack carton", actualSrcCartonProcessInsert.ApplicationAction, "ApplicationAction does not matched in src_carton_process_detail");
            }
            Assert.AreEqual(Info.VwhId, actualSrcCartonProcessInsert.VwhId, "vwhId  does not matched in src_carton_process_detail");
            Assert.AreEqual(Info.Pieces, actualSrcCartonProcessInsert.Quantity, "pieces does not matched in src_carton_process_detail");
            Assert.IsTrue(string.IsNullOrEmpty(actualSrcCartonProcessInsert.SourceArea), "To carton area should be null at the time of insertion");


            //Retrieves the repacked carton from src_carton_process_detail table for update data operation.
            var actualSrcCartonProcessUpdate = SqlBinder.Create(
          @"
<![CDATA[
SELECT CPD.MODULE_CODE        AS MODULE_CODE,
       CPD.TO_CARTON_AREA     AS TO_CARTON_AREA,
       CPD.FROM_CARTON_AREA   AS FROM_CARTON_AREA,
       CPD.VWH_ID             AS VWH_ID,
       CPD.NEW_CARTON_QTY     AS CARTON_QTY,
       CPD.APPLICATION_ACTION AS APPLICATION_ACTION
  FROM SRC_CARTON_PROCESS_DETAIL CPD
 WHERE CPD.CARTON_ID = :CARTON_ID 
AND CPD.DATABASE_OPERATION = 'UPDATE'
AND CPD.OLD_QUALITY_CODE = CPD.NEW_QUALITY_CODE
AND CPD.NEW_QUALITY_CODE = :PRE_CONVERSION_QUALITY
 ]]>", row => new
     {
         ModuleCode = row.GetValue<string>("MODULE_CODE"),
         DestArea = row.GetValue<string>("TO_CARTON_AREA"),
         VwhId = row.GetValue<string>("VWH_ID"),
         Quantity = row.GetValue<int>("CARTON_QTY"),
         ApplicationAction = row.GetValue<string>("APPLICATION_ACTION"),
         SourceArea = row.GetValue<string>("FROM_CARTON_AREA")
     }).Parameter("CARTON_ID", CartonId)
     .Parameter("PRE_CONVERSION_QUALITY", PreConversionQuality)
          .ExecuteSingle(_db);
            Assert.IsNotNull(actualSrcCartonProcessUpdate, "Carton not found in src_carton_process_details");
            Assert.AreEqual(Info.DestinationCartonArea, actualSrcCartonProcessUpdate.DestArea, "Carton area does not matched in  src_carton_process_detail ");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("rpk", actualSrcCartonProcessUpdate.ModuleCode, "moduleCode  does not matched in src_carton_process_detail");
                Assert.AreEqual("Repack carton", actualSrcCartonProcessUpdate.ApplicationAction, "ApplicationAction does not matched in src_carton_process_detail");
            }
            Assert.AreEqual(Info.TargetVWhId, actualSrcCartonProcessUpdate.VwhId, "vwhId  does not matched in src_carton_process_detail");
            Assert.AreEqual(Info.Pieces, actualSrcCartonProcessUpdate.Quantity, "pieces does not matched in src_carton_process_detail");
            Assert.AreEqual(actualSrcCartonProcessUpdate.DestArea, actualSrcCartonProcessUpdate.SourceArea, "The source and destination area must be same at the time of update operation in src_carton_process_detail table");

    

            // Verifiying the 'BCRE' -ve trancastion for repacked carton from src_transaction and src_transaction_detail tables.
            var negativeTransactionBCRE = SqlBinder.Create(
                @"
              <![CDATA[
WITH q1 AS
(SELECT A.STYLE                  AS STYLE,
       A.COLOR                  AS COLOR,
       A.DIMENSION              AS DIMENSION,
       A.SKU_SIZE               AS SKU_SIZE,
       A.VWH_ID                 AS VWH_ID,
       B.INVENTORY_STORAGE_AREA AS SKU_STORAGE_AREA,
       B.TRANSACTION_PIECES     AS TRANSACTION_PIECES,
       A.APPLICATION_ACTION     AS APPLICATION_ACTION,
       A.QUALITY_CODE           AS QUALITY_CODE
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID
   AND A.TRANSACTION_TYPE = 'BCRE'
   AND B.TRANSACTION_PIECES IS NOT NULL
   AND B.TRANSACTION_PIECES < 0
ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2 
               ]]>
                ", row => new
                 {
                     VwhId = row.GetValue<string>("VWH_ID"),
                     QualityCode = row.GetValue<string>("QUALITY_CODE"),
                     Style = row.GetValue<string>("STYLE"),
                     Color = row.GetValue<string>("COLOR"),
                     Dimension = row.GetValue<string>("DIMENSION"),
                     Size = row.GetValue<string>("SKU_SIZE"),
                     StorageArea = row.GetValue<string>("SKU_STORAGE_AREA"),
                     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
                     ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
                 }).Parameter("CARTON_ID", CartonId)
                  .ExecuteSingle(_db);
            Assert.IsNotNull(negativeTransactionBCRE, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(SourceStyle, negativeTransactionBCRE.Style, "style does not matched in src-tansaction table"); 
            Assert.AreEqual(SourceColor, negativeTransactionBCRE.Color, "color does not matched in src-tansaction table  ");
            Assert.AreEqual(SourceDimension, negativeTransactionBCRE.Dimension, "dimension does not matched in src-tansaction table");
            Assert.AreEqual(SourceSize, negativeTransactionBCRE.Size, "size does not matched in src-tansaction table"); 
            Assert.AreEqual(BestQuality, negativeTransactionBCRE.QualityCode, "quality Codedoes not matched in src-tansaction table");
            Assert.AreEqual(Info.VwhId, negativeTransactionBCRE.VwhId, "vwhId does not matched in src-tansaction table");
            Assert.AreEqual(0, (negativeTransactionBCRE.TransactionPieces + Info.Pieces), "Transaction pieces does not matched in  src_transaction_detail table");
            Assert.AreEqual(Info.SourceSkuArea, negativeTransactionBCRE.StorageArea, "carton Area does not matched in src_transaction_detail table");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", negativeTransactionBCRE.ApplicationAction, "ApplicationAction does not matched in src_carton_process_details");
            }


            // Verifiying the 'BCRE' +ve trancastion for repacked carton from src_transaction and src_transaction_detail tables.
            var positiveTransactionBcre = SqlBinder.Create(
                  @"
              <![CDATA[
WITH q1 AS
(SELECT A.STYLE                  AS STYLE,
       A.COLOR                  AS COLOR,
       A.DIMENSION              AS DIMENSION,
       A.SKU_SIZE               AS SKU_SIZE,
       A.VWH_ID                 AS VWH_ID,
       B.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
       B.TRANSACTION_PIECES     AS TRANSACTION_PIECES,
       A.APPLICATION_ACTION     AS APPLICATION_ACTION,
       A.QUALITY_CODE           AS QUALITY_CODE
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID 
AND A.TRANSACTION_TYPE = 'BCRE' 
AND B.TRANSACTION_PIECES IS NOT NULL
AND B.TRANSACTION_PIECES > 0   
 ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2 
]]>
                ", row => new
                 {
                     VwhId = row.GetValue<string>("VWH_ID"),
                     QualityCode = row.GetValue<string>("QUALITY_CODE"),
                     Style = row.GetValue<string>("STYLE"),
                     Color = row.GetValue<string>("COLOR"),
                     Dimension = row.GetValue<string>("DIMENSION"),
                     Size = row.GetValue<string>("SKU_SIZE"),
                     CartonStorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
                     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
                     ApplicationAction = row.GetValue<string>("APPLICATION_ACTION")
                 }).Parameter("CARTON_ID", CartonId)
                    .ExecuteSingle(_db);
            Assert.IsNotNull(positiveTransactionBcre, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(SourceStyle, positiveTransactionBcre.Style, "style does not matched in src-tansaction table"); 
            Assert.AreEqual(SourceColor, positiveTransactionBcre.Color, "color does not matched in src-tansaction table  ");
            Assert.AreEqual(SourceDimension, positiveTransactionBcre.Dimension, "dimension does not matched in src-tansaction table");
            Assert.AreEqual(SourceSize, positiveTransactionBcre.Size, "size does not matched in src-tansaction table"); 
            Assert.AreEqual(BestQuality, positiveTransactionBcre.QualityCode, "quality Codedoes not matched in src-tansaction table");
            Assert.AreEqual(Info.VwhId, positiveTransactionBcre.VwhId, "vwhId does not matched in src-tansaction table");
            Assert.AreEqual(Info.Pieces, positiveTransactionBcre.TransactionPieces, "Transaction pieces does not matched in  src_transaction_detail table");
            Assert.AreEqual(Info.DestinationCartonArea, positiveTransactionBcre.CartonStorageArea, "carton Area does not matched in src_transaction_detail table");
            if (x.StartsWith("11"))
            {
                Assert.AreEqual("Repack carton", positiveTransactionBcre.ApplicationAction, "ApplicationAction does not matched in src_carton_process_details");
            }

            // Verifiying the 'IXFR' -ve trancastion for repacked carton from src_transaction and src_transaction_detail tables 
            //for quality = best quality, Area = carton area, vwhId = source vwhid.
            var negativeTransactionIxfr = SqlBinder.Create(
                @"
              <![CDATA[
WITH q1 AS
(SELECT COUNT(*) AS COUNT
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID
   AND A.TRANSACTION_TYPE = 'IXFR'
   AND B.TRANSACTION_PIECES IS NOT NULL
   AND B.TRANSACTION_PIECES < 0
   AND A.QUALITY_CODE = :BEST_QUALITY
   AND A.VWH_ID = :SOURCE_VWH_ID
   AND B.INVENTORY_STORAGE_AREA = :CARTON_STORAGE_AREA
 ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2 
]]>
                ", row => new
                 {
                     Count = row.GetValue<int>("COUNT")
                 }).Parameter("CARTON_ID", CartonId)
                 .Parameter("BEST_QUALITY", BestQuality)
                 .Parameter("SOURCE_VWH_ID", Info.VwhId)
                 .Parameter("CARTON_STORAGE_AREA", Info.DestinationCartonArea)
                  .ExecuteSingle(_db);
            Assert.IsNotNull(negativeTransactionIxfr, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(1, negativeTransactionIxfr.Count, "more then one row inserted in src_transaction table");




            // Verifiying the 'IXFR' +ve trancastion for repacked carton from src_transaction and src_transaction_detail tables 
            //for quality = pre conversion quality, Area = carton area, vwhId = source vwhid.
            var positiveTransactionIxfr = SqlBinder.Create(
                  @"
<![CDATA[
WITH q1 AS
(SELECT COUNT(*) AS COUNT
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID
   AND A.TRANSACTION_TYPE = 'IXFR'
   AND B.TRANSACTION_PIECES IS NOT NULL
   AND B.TRANSACTION_PIECES > 0
   AND A.QUALITY_CODE = :PRE_CONVERSION_QUALITY
   AND A.VWH_ID = :SOURCE_VWH_ID
   AND B.INVENTORY_STORAGE_AREA = :CARTON_STORAGE_AREA
 ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2 
]]>
                ", row => new
                 {
                     Count = row.GetValue<int>("COUNT")
                 }).Parameter("CARTON_ID", CartonId)
                 .Parameter("PRE_CONVERSION_QUALITY", PreConversionQuality)
                 .Parameter("SOURCE_VWH_ID", Info.VwhId)
                 .Parameter("CARTON_STORAGE_AREA", Info.DestinationCartonArea)
                  .ExecuteSingle(_db);
            Assert.IsNotNull(positiveTransactionIxfr, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(1, negativeTransactionIxfr.Count, "more then one row inserted in src_transaction table");


            // Verifiying the 'IXFR' +ve trancastion for repacked carton from src_transaction
            //and src_transaction_detail tables for quality = preconversion quality, Area = carton area, 
            //vwhId = target vwhid.
            var positiveTransactionIxfr1 = SqlBinder.Create(
                          @"
              <![CDATA[
WITH q1 AS
(SELECT COUNT(*) AS COUNT
  FROM SRC_TRANSACTION A
 INNER JOIN SRC_TRANSACTION_DETAIL B
    ON A.TRANSACTION_ID = B.TRANSACTION_ID
 WHERE A.CARTON_ID = :CARTON_ID
   AND A.TRANSACTION_TYPE = 'IXFR'
   AND B.TRANSACTION_PIECES IS NOT NULL
   AND B.TRANSACTION_PIECES > 0
   AND A.QUALITY_CODE = :PRE_CONVERSION_QUALITY
   AND A.VWH_ID = :TARGET_VWH_ID
   AND B.INVENTORY_STORAGE_AREA = :CARTON_STORAGE_AREA
 ORDER BY A.insert_date DESC)
SELECT * FROM q1 WHERE ROWNUM < 2 
]]>
                ", row => new
                 {
                     Count = row.GetValue<int>("COUNT")
                 }).Parameter("CARTON_ID", CartonId)
                 .Parameter("PRE_CONVERSION_QUALITY", PreConversionQuality)
                 .Parameter("TARGET_VWH_ID", Info.TargetVWhId)
                 .Parameter("CARTON_STORAGE_AREA", Info.DestinationCartonArea)
                  .ExecuteSingle(_db);
            Assert.IsNotNull(negativeTransactionIxfr, "carton is not inserted in src-tansaction table ");
            Assert.AreEqual(1, negativeTransactionIxfr.Count, "more then one row inserted in src_transaction table");



            //Querying changed source area quantity in mri_raw_inventory table where storage area is SkuStorageArea,
            //vwhId is source VwhId and quality is BestQuality.
            var changedSourceQuantity = SqlBinder.Create(
                @"
<![CDATA[
SELECT MRI.QUANTITY AS QUANTITY
  FROM MASTER_RAW_INVENTORY MRI
 WHERE MRI.SKU_STORAGE_AREA = :SKU_STORAGE_AREA
   AND MRI.VWH_ID = :SOURCE_VWH_ID
   AND MRI.SKU_ID = :SOURCE_SKU_ID
   AND MRI.QUALITY_CODE = :BEST_QUALITY
]]>
", row => new
 {
     SourceQuantity = row.GetValue<int>("QUANTITY")
 }).Parameter("SKU_STORAGE_AREA", Info.SourceSkuArea)
 .Parameter("SOURCE_VWH_ID", Info.VwhId)
 .Parameter("SOURCE_SKU_ID", Info.SkuId)
 .Parameter("BEST_QUALITY", BestQuality)
 .ExecuteSingle(_db);

            //Asserting changed source quantity in master_raw_inventory
            Assert.AreEqual((SourceQuantity - (Info.NumberOfCartons * Info.Pieces)), changedSourceQuantity.SourceQuantity, "quantity ");


            //Querying changed destination area quantity in mri_raw_inventory table  where storage area is CartonStorageArea,
            //vwhId is target VwhId and quality is PreConversionQuality.
            var changedDestQuantity = SqlBinder.Create(
                @"
<![CDATA[
SELECT MRI.QUANTITY AS QUANTITY
  FROM MASTER_RAW_INVENTORY MRI
 WHERE MRI.SKU_STORAGE_AREA = :CARTON_STORAGE_AREA
   AND MRI.VWH_ID = :TARGET_VWH_ID
   AND MRI.SKU_ID = :TARGET_SKU_ID
   AND MRI.QUALITY_CODE = :PRE_CONVERSION_QUALITY
]]>
", row => new
 {
     DestQuantity = row.GetValue<int>("QUANTITY")
 }).Parameter("CARTON_STORAGE_AREA", Info.DestinationCartonArea)
 .Parameter("TARGET_VWH_ID", Info.TargetVWhId)
 .Parameter("TARGET_SKU_ID", Info.TartgetSkuId)
 .Parameter("PRE_CONVERSION_QUALITY", PreConversionQuality)
 .ExecuteSingle(_db);

            //Asserting changed source quantity in master_raw_inventory
            Assert.AreEqual((DestQuantity + (Info.NumberOfCartons * Info.Pieces)), changedDestQuantity.DestQuantity, "quantity");
        }


        /// <summary>
        /// Calling ReoackCarton() function for repacking similar Sku.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object and pass to RepackCarton() function. After calling RepackCarton() function ,
        /// this function calls ValidateRepackedCarton().
        /// 
        /// To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// .
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100 and inventory area is valid Sku storage area
        /// 
        /// To get the quantity of selected sku in Selected Carton Area this function queries master_raw_inventory table where sku is equal to selected sku and area is selected carton area.
        /// 
        /// 
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void RepackSingleCarton()
        {
            //Selecting a valid un-numbered carton area, which will be treated as destination area.
            var cartonArea = SqlBinder.Create(
                @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
            SELECT * FROM Q1 WHERE ROWNUM < 2
]]>
", row => new
                {
                    DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
                    IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
                }).ExecuteSingle(_db);
            if (cartonArea == null)
            {
                Assert.Inconclusive("Carton area is not available");
            }


            //Selecting a valid source Sku to be repacked.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID           AS VWH_ID,
       MRI.SKU_ID           AS SKU_ID,
       MRI.QUALITY_CODE     AS QUALITY_CODE,
       MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA,
       MRI.STYLE            AS STYLE,
       MRI.COLOR            AS COLOR,
       MRI.DIMENSION        AS DIMENSION,
       MRI.SKU_SIZE         AS SKU_SIZE,
       MRI.QUANTITY         AS QUANTITY
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE MRI.QUANTITY > 100
   AND TIA.STORES_WHAT = 'SKU'
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
            {
                SourceSkuId = row.GetValue<int>("SKU_ID"),
                SourceVwhId = row.GetValue<string>("VWH_ID"),
                SourceQualityCode = row.GetValue<string>("QUALITY_CODE"),
                SourceStyle = row.GetValue<string>("STYLE"),
                SourceColor = row.GetValue<string>("COLOR"),
                SourceDimension = row.GetValue<string>("DIMENSION"),
                SourceSize = row.GetValue<string>("SKU_SIZE"),
                SourceArea = row.GetValue<string>("SKU_STORAGE_AREA"),
                SourceQuantity = row.GetValue<int>("QUANTITY")
            }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Does not found");
            }

            //Fetching Quantity from master_raw_inventory table against destination area and selected Sku.
            var destinationAreaPieces = SqlBinder.Create(
                @"
<![CDATA[
WITH R1 AS
 (SELECT MRI.SKU_ID AS SKU_ID, MRI.QUANTITY AS QUANTITY
    FROM MASTER_RAW_INVENTORY MRI
   WHERE MRI.SKU_STORAGE_AREA = :CARTON_STORAGE_AREA
     AND MRI.SKU_ID = :SKU_ID
     AND MRI.VWH_ID = :VWH_ID
     AND MRI.QUALITY_CODE = :QUALITY_CODE
  UNION ALL
  SELECT NULL AS SKU_ID, 0 AS QUANTITY 
    FROM DUAL 
ORDER BY SKU_ID NULLS LAST)
SELECT * FROM R1 WHERE ROWNUM < 2
]]>
", row => new
 {
     Quantity = row.GetValue<int>("QUANTITY")
 }).Parameter("CARTON_STORAGE_AREA", cartonArea.DestinationArea)
  .Parameter("SKU_ID", sourceSku.SourceSkuId)
 .Parameter("VWH_ID", sourceSku.SourceVwhId)
 .Parameter("QUALITY_CODE", sourceSku.SourceQualityCode)
 .ExecuteSingle(_db);


            //Inserting value in model CartonRepackInfo
            var cartonInfo = new CartonRepackInfo

           {
               DestinationCartonArea = cartonArea.DestinationArea,
               SkuId = sourceSku.SourceSkuId,
               VwhId = sourceSku.SourceVwhId,
               SourceSkuArea = sourceSku.SourceArea,
               QualityCode = sourceSku.SourceQualityCode,
               Pieces = 5,
               NumberOfCartons = 1,
                            
           };
            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }

            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);

            // Assert
            Assert.IsNotNull(repackCartonId, "carton not created");
            Assert.AreEqual(repackCartonId[0], repackCartonId[1], "Returned Carton Id's must be same in case of shelf inventory repacking");

            //Calling validation Function
            ValidateRepackedCarton(cartonInfo, repackCartonId[0], destinationAreaPieces.Quantity, sourceSku.SourceQuantity, sourceSku.SourceColor, sourceSku.SourceDimension, sourceSku.SourceSize, sourceSku.SourceStyle);

        }


        ///<summary>
        ///Calling RepackCarton() function for repacking similar sku in bulk.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object and pass to RepackCarton() function. After calling RepackCarton() function 
        /// this function calls ValidateRepackedCarton().
        /// 
        /// To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory is more than 100 and inventory area is a valid Sku storage area.
        /// 
        /// To get the quantity of source sku in Selected Carton Area this function queries master_raw_inventory table where sku is equal to source sku and area is selected carton area.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void RepackBulkCarton()
        {
            ///Selecting valid un-numbered carton area, which will be treated as destination area.
            var cartonArea = SqlBinder.Create(
                @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
SELECT * FROM Q1 WHERE ROWNUM < 2                      
]]>
", row => new
 {
     DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).ExecuteSingle(_db);

            //Assert 
            Assert.IsNotNull(cartonArea, "Carton area not found");

            //Selecting a valid source Sku from master_raw_inventory whose inventory is more than 100 pieces and could be in any Sku area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA,
         MRI.STYLE            AS STYLE,
         MRI.COLOR            AS COLOR,
         MRI.DIMENSION        AS DIMENSION,
         MRI.SKU_SIZE         AS SKU_SIZE,
         MRI.QUANTITY         AS QUANTITY,
         MRI.VWH_ID           AS VWH_ID,
         MRI.QUALITY_CODE     AS QUALITY_CODE,
         MRI.SKU_ID           AS SKU_ID
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE MRI.QUANTITY > 100
   AND TIA.STORES_WHAT = 'SKU'
 ORDER BY DBMS_RANDOM.VALUE
)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA"),
     SourceStyle = row.GetValue<string>("STYLE"),
     SourceColor = row.GetValue<string>("COLOR"),
     SourceDimension = row.GetValue<string>("DIMENSION"),
     SourceSkuSize = row.GetValue<string>("SKU_SIZE"),
     SourceQuantity = row.GetValue<int>("QUANTITY"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceQuality = row.GetValue<string>("QUALITY_CODE"),
     SourceSkuId = row.GetValue<int>("SKU_ID")
 }).ExecuteSingle(_db);

            //Assert
            Assert.IsNotNull(sourceSku, "Sku not found");

            //Fetching Quantity from master_raw_inventory against selected carton area and selected sku.
            var destinationAreaPieces = SqlBinder.Create(
                @"
<![CDATA[
WITH R1 AS
 (SELECT MRI.SKU_ID AS SKU_ID, MRI.QUANTITY AS QUANTITY
    FROM MASTER_RAW_INVENTORY MRI
   WHERE MRI.SKU_STORAGE_AREA = :CARTON_STORAGE_AREA
     AND MRI.SKU_ID = :SKU_ID
     AND MRI.VWH_ID = :VWH_ID
     AND MRI.QUALITY_CODE = :QUALITY_CODE
  UNION ALL
  SELECT NULL AS SKU_ID, 0 AS QUANTITY 
    FROM DUAL 
ORDER BY SKU_ID NULLS LAST)
SELECT * FROM R1 WHERE ROWNUM < 2
]]>
", row => new
 {
     Quantity = row.GetValue<int>("QUANTITY")
 }).Parameter("CARTON_STORAGE_AREA", cartonArea.DestinationArea)
  .Parameter("SKU_ID", sourceSku.SourceSkuId)
 .Parameter("VWH_ID", sourceSku.SourceVwhId)
 .Parameter("QUALITY_CODE", sourceSku.SourceQuality)
 .ExecuteSingle(_db);


            //Inserting values in model Carton repack info
            var cartonInfo = new CartonRepackInfo
            {
                DestinationCartonArea = cartonArea.DestinationArea,
                NumberOfCartons = 2,
                Pieces = 5,
                QualityCode = sourceSku.SourceQuality,
                SkuId = sourceSku.SourceSkuId,
                SourceSkuArea = sourceSku.SourceArea,
                VwhId = sourceSku.SourceVwhId
            };

            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }

            //Calling funtion repackCarton
            var repackCartonId = _target.RepackCarton(cartonInfo);

            //Assert 
            Assert.IsNotNull(repackCartonId, "cartons not created");
            Assert.AreNotEqual(repackCartonId[0], repackCartonId[1], "Created Cartons must be Different in case of bulk ");

            //Loop for calling validation function for various cartons
            for (int i = 0; i < repackCartonId.Length; i++)
            {
                //Calling validation function 
                ValidateRepackedCarton(cartonInfo, repackCartonId[i], destinationAreaPieces.Quantity, sourceSku.SourceQuantity, sourceSku.SourceColor, sourceSku.SourceDimension, sourceSku.SourceSkuSize, sourceSku.SourceStyle);
            }
        }



        ///<summary>
        ///Calling repackCarton() function for repacking Sku while passing dummy CartonId.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object and pass to RepackCarton() function. After calling RepackCarton() function 
        /// this function calls ValidateReceivedCarton().
        /// 
        /// To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory is greater than 100 and inventory area is valid Sku storage area
        /// 
        /// To get the quantity of source sku in Selected Carton Area this function queries master_raw_inventory table where sku is equal selected sku and area is selected carton area.
        ///
        /// In case of receive a carton we also need to pass a dummy carton id to Repackcarton() function.  In our test we are passing 'ctn1' as carton id.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void ReceiveCarton()
        {
            //Select a valid carton area, which will be treated as the destination area for the received carton.
            var cartonArea = SqlBinder.Create(
                @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
SELECT * FROM Q1 WHERE ROWNUM < 2                        

]]>
", row => new
 {
     DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).ExecuteSingle(_db);

            //Assert 
            Assert.IsNotNull(cartonArea, "Carton area not found");


            //Selecting a valid sku that needs to be repacked from never received(NR) area and where inventory pieces are more than 100.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID           AS VWH_ID,
       MRI.SKU_ID           AS SKU_ID,
       MRI.QUALITY_CODE     AS QUALITY_CODE,
       MRI.STYLE            AS STYLE,
       MRI.COLOR            AS COLOR,
       MRI.DIMENSION        AS DIMENSION,
       MRI.SKU_SIZE         AS SKU_SIZE,
       MRI.QUANTITY         AS QUANTITY
  FROM MASTER_RAW_INVENTORY MRI
 WHERE MRI.QUANTITY > 100
 AND MRI.SKU_STORAGE_AREA = 'NR'
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceQualityCode = row.GetValue<string>("QUALITY_CODE"),
     SourceStyle = row.GetValue<string>("STYLE"),
     SourceColor = row.GetValue<string>("COLOR"),
     SourceDimension = row.GetValue<string>("DIMENSION"),
     SourceSize = row.GetValue<string>("SKU_SIZE"),
     SourceQuantity = row.GetValue<int>("QUANTITY")
 }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Does not found");
            }

            //Fetching Quantity from master_raw_inventory table against selected carton area and selected Sku.
            var destinationAreaPieces = SqlBinder.Create(
               @"
<![CDATA[
WITH R1 AS
 (SELECT MRI.SKU_ID AS SKU_ID, MRI.QUANTITY AS QUANTITY
    FROM MASTER_RAW_INVENTORY MRI
   WHERE MRI.SKU_STORAGE_AREA = :CARTON_STORAGE_AREA
     AND MRI.SKU_ID = :SKU_ID
     AND MRI.VWH_ID = :VWH_ID
     AND MRI.QUALITY_CODE = :QUALITY_CODE
  UNION ALL
  SELECT NULL AS SKU_ID, 0 AS QUANTITY 
    FROM DUAL 
ORDER BY SKU_ID NULLS LAST)
SELECT * FROM R1 WHERE ROWNUM < 2
]]>
", row => new
 {
     Quantity = row.GetValue<int>("QUANTITY")
 }).Parameter("CARTON_STORAGE_AREA", cartonArea.DestinationArea)
 .Parameter("SKU_ID", sourceSku.SourceSkuId)
.Parameter("VWH_ID", sourceSku.SourceVwhId)
.Parameter("QUALITY_CODE", sourceSku.SourceQualityCode)
.ExecuteSingle(_db);



            //Inserting value in model CartonRepackInfo
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = cartonArea.DestinationArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = "NR",
                QualityCode = sourceSku.SourceQualityCode,
                Pieces = 10,
                NumberOfCartons = 1,
                CartonId = "ctn1"
            };

            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }

            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);

            // Assert
            Assert.IsNotNull(repackCartonId, "carton not created");
            Assert.AreEqual(cartonInfo.CartonId, repackCartonId[0], "Returned Carton Id is not matching with provided carton Id");
            Assert.AreEqual(repackCartonId[0], repackCartonId[1], "Returned Carton Id's must be same in case of receive carton");

            //Calling validation function
            ValidateReceivedCarton(cartonInfo, repackCartonId[0], destinationAreaPieces.Quantity,sourceSku.SourceQuantity, sourceSku.SourceColor, sourceSku.SourceDimension, sourceSku.SourceSize, sourceSku.SourceStyle);

        }



        /// <summary>
        ///  Calling RepackCarton() function for conversion where VwhId of source and target Sku are same.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object and pass to RepackCarton() function. After calling RepackCarton() function 
        /// this function calls ValidateRepackConvertedCartonsSameVwh().
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory is greater than 100
        /// and quality is best quality.
        /// 
        /// To get the destination Sku it is select again any valid Sku from master_raw_inventory table of preconversion quality whose area is an carton area
        /// Target sku id should not be same as source Sku and Vwh Id of both the Sku should be same.
        /// 
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void RepackCartonForConversionSameVwh()
        {
            //Fetching quality from tab_quality_code against top most quality rank.
            var bestQuality = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT TQC.QUALITY_CODE AS BEST_QUALITY_CODE
    FROM TAB_QUALITY_CODE TQC
   ORDER BY TQC.QUALITY_RANK)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
",
 row => new
 {
     BestQualityCode = row.GetValue<string>("BEST_QUALITY_CODE")
 }).ExecuteSingle(_db);

            Assert.IsNotNull(bestQuality, "Quality is not defined in tab_quality_code");


            //Fetching PreConversionQuality from tab_quality_code table
            var preConversionQuality = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT TQC.QUALITY_CODE AS PRE_CONVERSION_QUALITY
    FROM TAB_QUALITY_CODE TQC
   WHERE TQC.PRECONVERSION_QUALITY IS NOT NULL
   ORDER BY TQC.QUALITY_RANK)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
",
 row => new
 {
     PreConversionQualityCode = row.GetValue<string>("PRE_CONVERSION_QUALITY")
 }).ExecuteSingle(_db);

            Assert.IsNotNull(preConversionQuality, "No Pre-conversion quality is defined");



            // Selecting a valid source Sku of best quality from master_raw_inventory table whose quantity is more than 100 pieces.  
            // This Skus can be in any valid Sku area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT MRI.STYLE            AS STYLE,
         MRI.COLOR            AS COLOR,
         MRI.DIMENSION        AS DIMENSION,
         MRI.SKU_SIZE         AS SKU_SIZE,
         MRI.VWH_ID           AS VWH_ID,
         MRI.SKU_ID           AS SKU_ID,
         MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA,
         MRI.QUANTITY         AS QUANTITY
    FROM MASTER_RAW_INVENTORY MRI
   INNER JOIN TAB_INVENTORY_AREA TIA
      ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
   WHERE MRI.QUANTITY > 100
     AND TIA.STORES_WHAT = 'SKU'
     AND MRI.QUALITY_CODE = :BEST_QUALITY_CODE
   ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {

     SourceStyle = row.GetValue<string>("STYLE"),
     SourceColor = row.GetValue<string>("COLOR"),
     SourceDimension = row.GetValue<string>("DIMENSION"),
     SourceSize = row.GetValue<string>("SKU_SIZE"),
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceQuantity = row.GetValue<int>("QUANTITY"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).Parameter("BEST_QUALITY_CODE", bestQuality.BestQualityCode)
 .ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive(" Sku does not found");
            }


            //Selecting another valid Sku of preconversion quality, from any carton area where location_numbering_flag is null.  
            // We will use this Sku as target Sku.  The Vwh Id of target Sku and Source Sku are same but SkuId must be Different.
 
            var targetSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q1 AS (
SELECT MRI.VWH_ID             AS VWH_ID,
       MRI.SKU_ID             AS SKU_ID,
       MRI.STYLE              AS STYLE,
       MRI.COLOR              AS COLOR,
       MRI.DIMENSION          AS DIMENSION,
       MRI.SKU_SIZE           AS SKU_SIZE,
       MRI.SKU_STORAGE_AREA   AS CARTON_STORAGE_AREA,
       MRI.QUANTITY           AS QUANTITY,
       TIA.IS_PALLET_REQUIRED AS IS_PALLET_REQUIRED
       FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
   AND MRI.SKU_ID != :SOURCE_SKUID
   AND MRI.QUALITY_CODE = :PRE_CONVERSION_QUALITY
   AND MRI.VWH_ID = :SOURCE_VWHID
 ORDER BY DBMS_RANDOM.VALUE
)
SELECT * FROM Q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     TargetSkuId = row.GetValue<int>("SKU_ID"),
     TargetVwhId = row.GetValue<string>("VWH_ID"),
     TargetStyle = row.GetValue<string>("STYLE"),
     TargetColor = row.GetValue<string>("COLOR"),
     TargetDimension = row.GetValue<string>("DIMENSION"),
     TargetSize = row.GetValue<string>("SKU_SIZE"),
     TargetQuantity = row.GetValue<int>("QUANTITY"),
     TargetCartonArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).Parameter("SOURCE_SKUID", sourceSku.SourceSkuId)
 .Parameter("PRE_CONVERSION_QUALITY", preConversionQuality.PreConversionQualityCode)
 .Parameter("SOURCE_VWHID", sourceSku.SourceVwhId)
            .ExecuteSingle(_db);
            if (targetSku == null)
            {
                Assert.Inconclusive("No target Sku found");
            }

            //Inserting value in model CartonRepackInfo
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = targetSku.TargetCartonArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = bestQuality.BestQualityCode,
                Pieces = 10,
                NumberOfCartons = 1,
                TartgetSkuId = targetSku.TargetSkuId
            };
            if (!string.IsNullOrEmpty(targetSku.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }


            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
            // Assert
            Assert.IsNotNull(repackCartonId, "carton not received");
            Assert.AreEqual(repackCartonId[0], repackCartonId[1], "Returned Carton Id's must be same in case of single carton repacking");

            //Calling validation function
            ValidateRepackedConvertedCartonSameVwh(cartonInfo, repackCartonId[0], sourceSku.SourceStyle, sourceSku.SourceColor, sourceSku.SourceDimension, sourceSku.SourceSize, targetSku.TargetStyle, targetSku.TargetColor, targetSku.TargetDimension, targetSku.TargetSize, bestQuality.BestQualityCode, preConversionQuality.PreConversionQualityCode, targetSku.TargetQuantity, sourceSku.SourceQuantity);

        }



        /// <summary>
        /// Calling RepackCarton() function for bulk conversion where VwhId of source and target Sku are same.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object and pass to RepackCarton() function. After calling RepackCarton() function 
        /// this function calls ValidateRepackConvertedCartonsSameVwh().
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory is greater than 100
        /// and quality is best quality.
        /// 
        /// To get the destination Sku it is select again any valid Sku from master_raw_inventory table of preconversion whose area is an carton area
        /// and which should not be same as source Sku and Vwh Id of both the Sku should be same.
        /// 
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void RepackBulkCartonForConversionSameVwh()
        {


            //Fetching quality from tab_quality_code against top most quality rank.
            var bestQuality = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT TQC.QUALITY_CODE AS BEST_QUALITY_CODE
    FROM TAB_QUALITY_CODE TQC
   ORDER BY TQC.QUALITY_RANK)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
",
 row => new
 {
     BestQualityCode = row.GetValue<string>("BEST_QUALITY_CODE")
 }).ExecuteSingle(_db);

            Assert.IsNotNull(bestQuality, "Quality is null");


            //Fetching preConversionQuality from tab_quality_code .
            var preConversionQuality = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT TQC.QUALITY_CODE AS PRE_CONVERSION_QUALITY
    FROM TAB_QUALITY_CODE TQC
   WHERE TQC.PRECONVERSION_QUALITY IS NOT NULL
  ORDER BY TQC.QUALITY_RANK)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
",
 row => new
 {
     PreConversionQualityCode = row.GetValue<string>("PRE_CONVERSION_QUALITY")
 }).ExecuteSingle(_db);

            Assert.IsNotNull(preConversionQuality.PreConversionQualityCode, "Quality is null");


            // Selecting a valid source Sku of best quality from any Sku storage area where quantity is more than 100.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID            AS VWH_ID,
         MRI.SKU_ID           AS SKU_ID,
         MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA,
         MRI.STYLE            AS STYLE,
         MRI.COLOR            AS COLOR,
         MRI.DIMENSION        AS DIMENSION,
         MRI.SKU_SIZE         AS SKU_SIZE,
         MRI.QUANTITY         AS QUANTITY
    FROM MASTER_RAW_INVENTORY MRI
   INNER JOIN TAB_INVENTORY_AREA TIA
      ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
   WHERE MRI.QUANTITY > 100
     AND TIA.STORES_WHAT = 'SKU'
     AND MRI.QUALITY_CODE = :BEST_QUALITY_CODE
   ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceStyle = row.GetValue<string>("STYLE"),
     SourceColor = row.GetValue<string>("COLOR"),
     SourceDimension = row.GetValue<string>("DIMENSION"),
     SourceSize = row.GetValue<string>("SKU_SIZE"),
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceQuantity = row.GetValue<int>("QUANTITY"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).Parameter("BEST_QUALITY_CODE", bestQuality.BestQualityCode)
 .ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("Expected Sku does not found");
            }


            //Selecting valid target Sku of preconversion quality from any carton area where location-nnumbering_flag is null
            //and whose SkuId is Different from selected Source Sku And VwhId is same as selected Source Sku's VwhId.
            var targetSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q1 AS (
SELECT MRI.VWH_ID           AS VWH_ID,
       MRI.SKU_ID           AS SKU_ID,
       MRI.STYLE            AS STYLE,
       MRI.COLOR            AS COLOR,
       MRI.DIMENSION        AS DIMENSION,
       MRI.SKU_SIZE         AS SKU_SIZE,
       MRI.SKU_STORAGE_AREA AS CARTON_STORAGE_AREA,
       MRI.QUANTITY         AS QUANTITY,
       TIA.IS_PALLET_REQUIRED AS IS_PALLET_REQUIRED
      FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
   AND MRI.SKU_ID != :SOURCE_SKUID
   AND MRI.QUALITY_CODE = :PRE_CONVERSION_QUALITY
   AND MRI.VWH_ID = :SOURCE_VWHID
 ORDER BY DBMS_RANDOM.VALUE
)
SELECT * FROM q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     TargetStyle = row.GetValue<string>("STYLE"),
     TargetColor = row.GetValue<string>("COLOR"),
     TargetDimension = row.GetValue<string>("DIMENSION"),
     TargetSize = row.GetValue<string>("SKU_SIZE"),
     TargetSkuId = row.GetValue<int>("SKU_ID"),
     TargetVwhId = row.GetValue<string>("VWH_ID"),
     TargetQuantity = row.GetValue<int>("QUANTITY"),
     TargetCartonArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).Parameter("SOURCE_SKUID", sourceSku.SourceSkuId)
 .Parameter("PRE_CONVERSION_QUALITY", preConversionQuality.PreConversionQualityCode)
 .Parameter("SOURCE_VWHID", sourceSku.SourceVwhId)
            .ExecuteSingle(_db);
            if (targetSku == null)
            {
                Assert.Inconclusive("Target Sku does not found");
            }

            //Inserting value in model CartonRepackInfo
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = targetSku.TargetCartonArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = bestQuality.BestQualityCode,
                Pieces = 10,
                NumberOfCartons = 2,
                TargetVWhId = targetSku.TargetVwhId,
                TartgetSkuId = targetSku.TargetSkuId
            };

            if (!string.IsNullOrEmpty(targetSku.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }

            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);

            // Assert
            Assert.IsNotNull(repackCartonId, "carton not received");
            Assert.AreNotEqual(repackCartonId[0], repackCartonId[1], "Returned Carton Id's must NOT be same in case of bulk conversion");

            //Creating a loop for calling validation function number of times
            for (int i = 0; i < repackCartonId.Length; i++)
            {
                ValidateRepackedConvertedCartonSameVwh(cartonInfo, repackCartonId[i], sourceSku.SourceStyle, sourceSku.SourceColor, sourceSku.SourceDimension, sourceSku.SourceSize, targetSku.TargetStyle, targetSku.TargetColor, targetSku.TargetDimension, targetSku.TargetSize, bestQuality.BestQualityCode, preConversionQuality.PreConversionQualityCode, targetSku.TargetQuantity, sourceSku.SourceQuantity);
            }
        }




        /// <summary>
        /// Calling RepackCarton() function for conversion where VwhId of source and target Sku are different.
        /// 
        ///  This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object and pass to RepackCarton() function. After calling RepackCarton() function 
        /// this function calls ValidateRepackConvertedCartonsDiffVwh().
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory is greater than 100
        /// and quality is best quality.
        /// 
        /// To get the destination Sku it is select again any valid Sku from master_raw_inventory table of preconversion whose area is an carton area
        /// which should not be same as source Sku and Vwh Id of both the Sku should be different.
        /// 
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void RepackCartonForConversionDiffVwh()
        {
            //Fetching quality from tab_quality_code against top most quality rank.
            var bestQuality = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT TQC.QUALITY_CODE AS BEST_QUALITY_CODE
    FROM TAB_QUALITY_CODE TQC
   ORDER BY TQC.QUALITY_RANK)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
",
 row => new
 {
     BestQualityCode = row.GetValue<string>("BEST_QUALITY_CODE")
 }).ExecuteSingle(_db);

            Assert.IsNotNull(bestQuality, "Quality is null");


            //Fetching preconvertionalquality from tab_quality_code.
            var preConversionQuality = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT TQC.QUALITY_CODE AS PRE_CONVERSION_QUALITY
    FROM TAB_QUALITY_CODE TQC
   WHERE TQC.PRECONVERSION_QUALITY IS NOT NULL
  ORDER BY TQC.QUALITY_RANK)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
",
 row => new
 {
     PreConversionQualityCode = row.GetValue<string>("PRE_CONVERSION_QUALITY")
 }).ExecuteSingle(_db);

            Assert.IsNotNull(preConversionQuality, "Quality is null");


            // Selecting a valid source Sku of best quality from any valid sku Storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID            AS VWH_ID,
         MRI.SKU_ID           AS SKU_ID,
         MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA,
         MRI.STYLE            AS STYLE,
         MRI.COLOR            AS COLOR,
         MRI.DIMENSION        AS DIMENSION,
         MRI.SKU_SIZE         AS SKU_SIZE,
         MRI.QUANTITY         AS QUANTITY
    FROM MASTER_RAW_INVENTORY MRI
   INNER JOIN TAB_INVENTORY_AREA TIA
      ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
   WHERE MRI.QUANTITY > 100
     AND TIA.STORES_WHAT = 'SKU'
     AND MRI.QUALITY_CODE = :BEST_QUALITY_CODE
   ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceStyle = row.GetValue<string>("STYLE"),
     SourceColor = row.GetValue<string>("COLOR"),
     SourceDimension = row.GetValue<string>("DIMENSION"),
     SourceSize = row.GetValue<string>("SKU_SIZE"),
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceQuantity = row.GetValue<int>("QUANTITY"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).Parameter("BEST_QUALITY_CODE", bestQuality.BestQualityCode)
 .ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("Expected Sku does not found");
            }



            //selecting valid target Sku of preconversion quality from any valid carton area where location_numbering_flag is null 
            //and where the  SkuId and VwhId are not equal to selected Source SkuId and vwhId.
            var targetSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q1 AS (
SELECT MRI.VWH_ID           AS VWH_ID,
       MRI.SKU_ID           AS SKU_ID,
       MRI.STYLE            AS STYLE,
       MRI.COLOR            AS COLOR,
       MRI.DIMENSION        AS DIMENSION,
       MRI.SKU_SIZE         AS SKU_SIZE,
       MRI.SKU_STORAGE_AREA AS CARTON_STORAGE_AREA,
       MRI.QUANTITY         AS QUANTITY,
       TIA.IS_PALLET_REQUIRED AS IS_PALLET_REQUIRED
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
   AND MRI.SKU_ID != :SOURCE_SKUID
   AND MRI.QUALITY_CODE = :PRE_CONVERSION_QUALITY
   AND MRI.VWH_ID != :SOURCE_VWHID
 ORDER BY DBMS_RANDOM.VALUE
)
SELECT * FROM q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     TargetSkuId = row.GetValue<int>("SKU_ID"),
     TargetVwhId = row.GetValue<string>("VWH_ID"),
     TargetStyle = row.GetValue<string>("STYLE"),
     TargetColor = row.GetValue<string>("COLOR"),
     TargetDimension = row.GetValue<string>("DIMENSION"),
     TargetSize = row.GetValue<string>("SKU_SIZE"),
     TargetQuantity = row.GetValue<int>("QUANTITY"),
     TargetCartonArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).Parameter("SOURCE_SKUID", sourceSku.SourceSkuId)
 .Parameter("PRE_CONVERSION_QUALITY", preConversionQuality.PreConversionQualityCode)
 .Parameter("SOURCE_VWHID", sourceSku.SourceVwhId)
            .ExecuteSingle(_db);
            if (targetSku == null)
            {
                Assert.Inconclusive("Target Sku doesnot found");
            }

            //Inserting value in model CartonRepackInfo
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = targetSku.TargetCartonArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = bestQuality.BestQualityCode,
                Pieces = 10,
                NumberOfCartons = 1,
                TargetVWhId = targetSku.TargetVwhId,
                TartgetSkuId = targetSku.TargetSkuId
            };
            if (!string.IsNullOrEmpty(targetSku.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }


            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
            // Assert
            Assert.IsNotNull(repackCartonId, "carton not received");
            Assert.AreEqual(repackCartonId[0], repackCartonId[1], "Returned Carton Id's must be same in case of conversion");

            //calling validation function
            ValidateRepackedConvertedCartonDiffVwh(cartonInfo, repackCartonId[0], sourceSku.SourceStyle, sourceSku.SourceColor, sourceSku.SourceDimension, sourceSku.SourceSize, targetSku.TargetStyle, targetSku.TargetColor, targetSku.TargetDimension, targetSku.TargetSize, bestQuality.BestQualityCode, preConversionQuality.PreConversionQualityCode, targetSku.TargetQuantity, sourceSku.SourceQuantity);
        }



        /// <summary>
        /// Calling RepackCarton() function for bulk conversion where VwhId of source and target Sku are different.
        /// 
        ///  This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object and pass to RepackCarton() function. After calling RepackCarton() function 
        /// this function calls ValidateRepackConvertedCartonsDiffVwh().
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory is greater than 100
        /// and quality is best quality.
        /// 
        /// To get the destination Sku it is select again any valid Sku from master_raw_inventory table of preconversion whose area is an carton area
        /// which should not be same as source Sku and Vwh Id of both the Sku should be different.
        /// 
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void RepackBulkCartonForConversionDiffVwh()
        {

            //Fetching quality from tab_quality_code against top most quality rank
            var bestQuality = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT TQC.QUALITY_CODE AS BEST_QUALITY_CODE
    FROM TAB_QUALITY_CODE TQC
   ORDER BY TQC.QUALITY_RANK)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
",
 row => new
 {
     BestQualityCode = row.GetValue<string>("BEST_QUALITY_CODE")
 }).ExecuteSingle(_db);
            if (bestQuality == null)
            {
                Assert.Inconclusive("Expected Quality does not found");
            }

            //Fetching preconvertionalQuality from tab_quality_code.
            var preConversionQuality = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
 (SELECT TQC.QUALITY_CODE AS PRE_CONVERSION_QUALITY
    FROM TAB_QUALITY_CODE TQC
   WHERE TQC.PRECONVERSION_QUALITY IS NOT NULL
   ORDER BY TQC.QUALITY_RANK)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
",
 row => new
 {
     PreConversionQualityCode = row.GetValue<string>("PRE_CONVERSION_QUALITY")
 }).ExecuteSingle(_db);
            
            if (preConversionQuality == null)
            {
                Assert.Inconclusive("Expected preconversion Quality does not found");
            }


            // Selecting a valid source Sku of bestQuality from any valid Sku storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID           AS VWH_ID,
         MRI.SKU_ID           AS SKU_ID,
         MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA,
         MRI.STYLE            AS STYLE,
         MRI.COLOR            AS COLOR,
         MRI.DIMENSION        AS DIMENSION,
         MRI.SKU_SIZE         AS SKU_SIZE,
         MRI.QUANTITY         AS QUANTITY
    FROM MASTER_RAW_INVENTORY MRI
   INNER JOIN TAB_INVENTORY_AREA TIA
      ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
   WHERE MRI.QUANTITY > 100
     AND TIA.STORES_WHAT = 'SKU'
     AND MRI.QUALITY_CODE = :BEST_QUALITY_CODE
   ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceStyle = row.GetValue<string>("STYLE"),
     SourceColor = row.GetValue<string>("COLOR"),
     SourceDimension = row.GetValue<string>("DIMENSION"),
     SourceSize = row.GetValue<string>("SKU_SIZE"),
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceQuantity = row.GetValue<int>("QUANTITY"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).Parameter("BEST_QUALITY_CODE", bestQuality.BestQualityCode)
 .ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("Expected Sku does not found");
            }

            //Selecting a valid target Sku of preconversion quality from any Carton area with null location_numbering_flag 
            //where the skuId and vwhId are not same as selected Source Sku's SkuId and VwhId.
            var targetSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q1 AS (
SELECT MRI.VWH_ID           AS VWH_ID,
       MRI.SKU_ID           AS SKU_ID,
       MRI.STYLE            AS STYLE,
       MRI.COLOR            AS COLOR,
       MRI.DIMENSION        AS DIMENSION,
       MRI.SKU_SIZE         AS SKU_SIZE,
       MRI.SKU_STORAGE_AREA AS CARTON_STORAGE_AREA,
       MRI.QUANTITY         AS QUANTITY,
       TIA.IS_PALLET_REQUIRED AS IS_PALLET_REQUIRED
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
   AND MRI.SKU_ID != :SOURCE_SKUID
   AND MRI.QUALITY_CODE = :PRE_CONVERSION_QUALITY
   AND MRI.VWH_ID != :SOURCE_VWHID
 ORDER BY DBMS_RANDOM.VALUE
)
SELECT * FROM q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     TargetSkuId = row.GetValue<int>("SKU_ID"),
     TargetVwhId = row.GetValue<string>("VWH_ID"),
     TargetStyle = row.GetValue<string>("STYLE"),
     TargetColor = row.GetValue<string>("COLOR"),
     TargetDimension = row.GetValue<string>("DIMENSION"),
     TargetSize = row.GetValue<string>("SKU_SIZE"),
     TargetQuantity = row.GetValue<int>("QUANTITY"),
     TargetCartonArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).Parameter("SOURCE_SKUID", sourceSku.SourceSkuId)
 .Parameter("PRE_CONVERSION_QUALITY", preConversionQuality.PreConversionQualityCode)
 .Parameter("SOURCE_VWHID", sourceSku.SourceVwhId)
            .ExecuteSingle(_db);
            if (targetSku == null)
            {
                Assert.Inconclusive("Target Sku does not found");
            }

            //inserting value in model CartonRepackInfo
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = targetSku.TargetCartonArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = bestQuality.BestQualityCode,
                Pieces = 10,
                NumberOfCartons = 2,
                TargetVWhId = targetSku.TargetVwhId,
                TartgetSkuId = targetSku.TargetSkuId
            };

            if (!string.IsNullOrEmpty(targetSku.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;

            }


            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);

            // Assert
            Assert.IsNotNull(repackCartonId, "carton not received");
            Assert.AreNotEqual(repackCartonId[0], repackCartonId[1], "Returned Carton Id's must not same in case of repacking bulk cartons");

            //calling validation function
            for (int i = 0; i < repackCartonId.Length; i++)
            {
                ValidateRepackedConvertedCartonDiffVwh(cartonInfo, repackCartonId[i], sourceSku.SourceStyle, sourceSku.SourceColor, sourceSku.SourceDimension, sourceSku.SourceSize, targetSku.TargetStyle, targetSku.TargetColor, targetSku.TargetDimension, targetSku.TargetSize, bestQuality.BestQualityCode, preConversionQuality.PreConversionQualityCode, targetSku.TargetQuantity, sourceSku.SourceQuantity);
            }
        }



        /// <summary>
        /// Calling RepackCarton() function under RPK user to check if it is voilating any constraint. 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object and pass to RepackCarton() function and calling RepackCarton() function 
        /// 
        /// To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100 and inventory area 
        /// is valid Sku storage area
        ///       
        /// But in this function we are calling RepackCarton() function under "rpk" user.
        /// 
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void RepackSingleCartonRpkUser()
        {
            //Selecting valid un-numbered carton area, which will be treated as destination area.
            var cartonArea = SqlBinder.Create(
                @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
            SELECT * FROM Q1 WHERE ROWNUM < 2
]]>
", row => new
                {
                    DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
                    IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
                }).ExecuteSingle(_db1);
            if (cartonArea == null)
            {
                Assert.Inconclusive("Carton area is not available");
            }


            //Selecting a valid source Sku  from Sku storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID           AS VWH_ID,
       MRI.SKU_ID           AS SKU_ID,
       MRI.QUALITY_CODE     AS QUALITY_CODE,
       MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA,
       MRI.STYLE            AS STYLE,
       MRI.COLOR            AS COLOR,
       MRI.DIMENSION        AS DIMENSION,
       MRI.SKU_SIZE         AS SKU_SIZE      
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE MRI.QUANTITY > 100
   AND TIA.STORES_WHAT = 'SKU'
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM = 1
]]>
", row => new
            {
                SourceSkuId = row.GetValue<int>("SKU_ID"),
                SourceVwhId = row.GetValue<string>("VWH_ID"),
                SourceQualityCode = row.GetValue<string>("QUALITY_CODE"),
                SourceStyle = row.GetValue<string>("STYLE"),
                SourceColor = row.GetValue<string>("COLOR"),
                SourceDimension = row.GetValue<string>("DIMENSION"),
                SourceSize = row.GetValue<string>("SKU_SIZE"),
                SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
            }).ExecuteSingle(_db1);
            if (sourceSku == null)
            {
                Assert.Inconclusive("No SKU found");
            }



            //Inserting value in model CartonRepackInfo
            var cartonInfo = new CartonRepackInfo

           {
               DestinationCartonArea = cartonArea.DestinationArea,
               SkuId = sourceSku.SourceSkuId,
               VwhId = sourceSku.SourceVwhId,
               SourceSkuArea = sourceSku.SourceArea,
               QualityCode = sourceSku.SourceQualityCode,
               Pieces = 10,
               NumberOfCartons = 1

           };

            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }

            //Act
            _target1.RepackCarton(cartonInfo);

        }


        //Invalid cases

        ///<summary>
        ///Calling ReapckCarton() function to check is it voilation any exception for null destination area.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object with passing null value in  mandatory field DestinationCartonArea and pass the object to RepackCarton() function. 
        ///
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100 and inventory area is valid Sku storage area
        /// 
        /// After calling RepackCarton() function this function checks wheater RepackCarton() is generating any exception for for null
        /// DestinationCartonArea or not.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void NullDestinationArea()
        {

            //Selecting a valid source Sku from Sku storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID          AS VWH_ID,
       MRI.SKU_ID           AS SKU_ID,
       MRI.QUALITY_CODE     AS QUALITY_CODE,
       MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE MRI.QUANTITY > 100
   AND TIA.STORES_WHAT = 'SKU'
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceQualityCode = row.GetValue<string>("QUALITY_CODE"),
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Doesnot found");
            }

            //Inserting value in model CartonRepackInfo where DestinationCartonArea is NULL
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = null,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = sourceSku.SourceQualityCode,
                Pieces = 10,
                NumberOfCartons = 1

            };

            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
        }


        ///<summary>
        ///Calling ReapckCarton() function to check is it voilation any exception for invalid destination area.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object with passing invalid value in field DestinationCartonArea and pass the object to RepackCarton() function. 
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100 and inventory area is valid Sku storage area
        /// 
        /// After calling RepackCarton() function this function checks wheater RepackCarton() is generating any exception for invalid
        /// DestinationCartonArea or not.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void InvalidDestinationArea()
        {

            //Selecting a valid source Sku to be repacked from Sku storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID           AS VWH_ID,
       MRI.SKU_ID           AS SKU_ID,
       MRI.QUALITY_CODE     AS QUALITY_CODE,
       MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE MRI.QUANTITY > 100
   AND TIA.STORES_WHAT = 'SKU'
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceQualityCode = row.GetValue<string>("QUALITY_CODE"),
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Doesnot found");
            }

            //Inserting value in model CartonRepackInfo where DestinationCartonArea is NULL
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = sourceSku.SourceArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = sourceSku.SourceQualityCode,
                Pieces = 10,
                NumberOfCartons = 1

            };

            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
        }



        ///<summary>
        ///Calling ReapckCarton() function to check is it voilation any exception for null source area.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object with passing null value in mandatory field SourceSkuArea and pass the object to RepackCarton() function.
        /// 
        /// To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100 .
        /// 
        /// After calling RepackCarton() function this function checks wheater RepackCarton() is generating any exception for null
        /// SourceArea or not.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void NullSourceArea()
        {
            ///Selecting valid un-numbered carton area, which will be treated as destination area.
            var cartonArea = SqlBinder.Create(
                   @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
            SELECT * FROM Q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).ExecuteSingle(_db1);
            //assert
            Assert.IsNotNull(cartonArea, "Carton Area is not available");


            //Selecting a valid source Sku to be repacked from Sku storage area .
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID           AS VWH_ID,
       MRI.SKU_ID           AS SKU_ID,
       MRI.QUALITY_CODE     AS QUALITY_CODE
  FROM MASTER_RAW_INVENTORY MRI
 WHERE MRI.QUANTITY > 100
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceQualityCode = row.GetValue<string>("QUALITY_CODE")
 }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Doesnot found");
            }

            //Inserting value in model CartonRepackInfo where DestinationCartonArea is NULL
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = cartonArea.DestinationArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = null,
                QualityCode = sourceSku.SourceQualityCode,
                Pieces = 10,
                NumberOfCartons = 1

            };
            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }

            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
        }



        ///<summary>
        ///Calling ReapckCarton() function to check is it generate any exception for invalid source area.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object with passing invalid value in mandatory field SourceSkuArea and pass the object to RepackCarton() function.
        /// 
        /// To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100.
        /// 
        /// After calling RepackCarton() function this function checks wheater RepackCarton() is generating any exception for invalid
        /// SourceArea or not.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void InvalidSourceArea()
        {
            ///Selecting valid un-numbered carton area, which will be treated as destination area.
            var cartonArea = SqlBinder.Create(
               @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
            SELECT * FROM Q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).ExecuteSingle(_db1);
            //assert
            Assert.IsNotNull(cartonArea, "Carton Area is not available");

            //Selecting a valid source Sku to be repacked from Sku storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID           AS VWH_ID,
       MRI.SKU_ID           AS SKU_ID,
       MRI.QUALITY_CODE     AS QUALITY_CODE
FROM MASTER_RAW_INVENTORY MRI
 WHERE MRI.QUANTITY > 100
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceQualityCode = row.GetValue<string>("QUALITY_CODE")
 }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Doesnot found");
            }

            //Inserting value in model CartonRepackInfo where DestinationCartonArea is NULL
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = cartonArea.DestinationArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = "soso area",
                QualityCode = sourceSku.SourceQualityCode,
                Pieces = 10,
                NumberOfCartons = 1

            };
            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }
            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
        }


        ///<summary>
        ///Calling ReapckCarton() function to check is it voilation any exception for invalid SkuId.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object with passing invalid value in mandatory field SkuId and pass the object to RepackCarton() function. 
        /// 
        /// To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100 and where area is valid sku area.
        /// 
        /// 
        /// After calling RepackCarton() function this function checks wheater RepackCarton() is generating any exception for invalid
        /// SkuId or not.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void InvalidSkuId()
        {
            ///Selecting valid un-numbered carton area, which will be treated as destination area.
            var cartonArea = SqlBinder.Create(
                @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
            SELECT * FROM Q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).ExecuteSingle(_db1);

            //assert
            Assert.IsNotNull(cartonArea, "Carton Area is not available");

            //Selecting a valid source Sku to be repacked from Sku storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID           AS VWH_ID,
       MRI.QUALITY_CODE     AS QUALITY_CODE,
       MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE MRI.QUANTITY > 100
   AND TIA.STORES_WHAT = 'SKU'
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceQualityCode = row.GetValue<string>("QUALITY_CODE"),
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Doesnot found");
            }

            //Inserting value in model CartonRepackInfo where DestinationCartonArea is NULL
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = cartonArea.DestinationArea,
                SkuId = -102,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = sourceSku.SourceQualityCode,
                Pieces = 10,
                NumberOfCartons = 1

            };
            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }
            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
        }

        ///<summary>
        ///Calling ReapckCarton() function to check is it voilation any exception for null VwhId.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object with passing null value in mandatory field VwhId and pass to RepackCarton() function. 
        /// 
        ///  To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100 and where area is valid sku area.
        /// 
        /// After calling RepackCarton() function this function checks wheater RepackCarton() is generating any exception for 
        /// Null VwhId or not.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void NullVwhId()
        {
            ///Selecting valid un-numbered carton area, which will be treated as destination area.
            var cartonArea = SqlBinder.Create(
              @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
            SELECT * FROM Q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).ExecuteSingle(_db1);

            //assert
            Assert.IsNotNull(cartonArea, "Carton Area is not available");



            //Selecting a valid source Sku to be repacked from Sku storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.SKU_ID           AS SKU_ID,
       MRI.QUALITY_CODE     AS QUALITY_CODE,
       MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE MRI.QUANTITY > 100
   AND TIA.STORES_WHAT = 'SKU'
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceQualityCode = row.GetValue<string>("QUALITY_CODE"),
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Does not found");
            }

            //Inserting value in model CartonRepackInfo where DestinationCartonArea is NULL
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = cartonArea.DestinationArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = null,
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = sourceSku.SourceQualityCode,
                Pieces = 10,
                NumberOfCartons = 1

            };

            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }
            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
        }


        ///<summary>
        ///Calling ReapckCarton() function to check is it voilation any exception for invalid VwhId.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object with passing invalid value in mandatory field VwhId and pass the object to RepackCarton() function. 
        /// 
        ///  To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100 and where area is valid sku area.
        /// 
        /// After calling RepackCarton() function this function checks wheater RepackCarton() is generating any exception for 
        /// invalid VwhId or not.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void InvalidVwhId()
        {
            ///Selecting valid un-numbered carton area, which will be treated as destination area.
            var cartonArea = SqlBinder.Create(
               @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
            SELECT * FROM Q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).ExecuteSingle(_db1);

            //assert
            Assert.IsNotNull(cartonArea, "Carton Area is not available");



            //Selecting a valid source Sku to be repacked from Sku storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.SKU_ID           AS SKU_ID,
       MRI.QUALITY_CODE     AS QUALITY_CODE,
       MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE MRI.QUANTITY > 100
   AND TIA.STORES_WHAT = 'SKU'
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceQualityCode = row.GetValue<string>("QUALITY_CODE"),
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Doesnot found");
            }

            //Inserting value in model CartonRepackInfo where DestinationCartonArea is NULL
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = cartonArea.DestinationArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = "007",
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = sourceSku.SourceQualityCode,
                Pieces = 10,
                NumberOfCartons = 1

            };
            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }

            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
        }


        ///<summary>
        ///Calling ReapckCarton() function to check is it voilation any exception for null quality code.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object with passing null value in mandatory field QualityCode and pass the object to RepackCarton() function. 
        /// 
        /// To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100 and where area is valid sku area.
        /// 
        /// After calling RepackCarton() function this function checks wheater RepackCarton() is generating any exception for null 
        /// QualityCode or not.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void NullQualityCode()
        {
            ///Selecting valid un-numbered carton area, which will be treated as destination area.
            var cartonArea = SqlBinder.Create(
              @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
            SELECT * FROM Q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).ExecuteSingle(_db1);

            //assert
            Assert.IsNotNull(cartonArea, "Carton Area is not available");



            //Selecting a valid source Sku to be repacked from Sku storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.SKU_ID           AS SKU_ID,
        MRI.VWH_ID           AS VWH_ID,
        MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE MRI.QUANTITY > 100
   AND TIA.STORES_WHAT = 'SKU'
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Doesnot found");
            }

            //Inserting value in model CartonRepackInfo where DestinationCartonArea is NULL
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = cartonArea.DestinationArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = null,
                Pieces = 10,
                NumberOfCartons = 1

            };
            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }

            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
        }


        ///<summary>
        ///Calling ReapckCarton() function to check is it generate any exception for invalid quality code.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object with passing invalid value in mandatory field QualityCode and pass to RepackCarton() function.
        /// 
        /// To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100 and where area is valid sku area.
        /// 
        /// After calling RepackCarton() function this function checks wheater RepackCarton() is generating any exception for invalid
        /// QualityCode or not.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void InvalidQualityCode()
        {
            ///Selecting valid un-numbered carton area, which will be treated as destination area.
            var cartonArea = SqlBinder.Create(
            @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
            SELECT * FROM Q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).ExecuteSingle(_db1);

            //assert
            Assert.IsNotNull(cartonArea, "Carton Area is not available");



            //Selecting a valid source Sku to be repacked from Sku storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.SKU_ID           AS SKU_ID,
        MRI.VWH_ID           AS VWH_ID,
        MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE MRI.QUANTITY > 100
   AND TIA.STORES_WHAT = 'SKU'
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM < 2
]]>
", row => new
 {
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),   
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Doesnot found");
            }

            //Inserting value in model CartonRepackInfo where DestinationCartonArea is NULL
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = cartonArea.DestinationArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = "786",
                Pieces = 10,
                NumberOfCartons = 1

            };

            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }
            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
        }


        ///<summary>
        ///Calling ReapckCarton() function to check is it voilation any exception for invalid pieces.
        /// 
        /// This function is calling RepackCarton() function of repository.  This function creates
        /// Carton info object with passing invalid number in mandatory field Pieces and pass to RepackCarton() function.
        /// 
        ///  To get a target Area it is selecting any valid Carton area from tab_inventory_area table where location_numbering flag is null.
        /// 
        /// To get a source Sku it is selecting any valid Sku from master_raw_inventory table where inventory pieces are more than 100 and where area is valid sku area.
        /// 
        /// After calling RepackCarton() function this function checks wheater RepackCarton() is generating any exception 
        /// for pieces less than or equal to zero.
        ///</summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void InvalidPieces()
        {
            //Selecting valid un-numbered carton area, which will be treated as destination area.
            var cartonArea = SqlBinder.Create(
                @"
<![CDATA[
WITH Q1 AS
(SELECT TIA.INVENTORY_STORAGE_AREA AS CARTON_STORAGE_AREA,
        TIA.IS_PALLET_REQUIRED     AS IS_PALLET_REQUIRED
  FROM TAB_INVENTORY_AREA TIA
 WHERE TIA.STORES_WHAT = 'CTN'
   AND TIA.LOCATION_NUMBERING_FLAG IS NULL
 ORDER BY DBMS_RANDOM.VALUE
)
            SELECT * FROM Q1 WHERE ROWNUM < 2
]]>
", row => new
 {
     DestinationArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     IsPalletRequired = row.GetValue<string>("IS_PALLET_REQUIRED")
 }).ExecuteSingle(_db1);

            //assert
            Assert.IsNotNull(cartonArea, "Carton Area is not available");



            //Selecting a valid source Sku to be repacked from Sku storage area.
            var sourceSku = SqlBinder.Create(
                @"
<![CDATA[
WITH Q AS
(SELECT MRI.VWH_ID           AS VWH_ID,
       MRI.SKU_ID           AS SKU_ID,
       MRI.QUALITY_CODE     AS QUALITY_CODE,
       MRI.SKU_STORAGE_AREA AS SKU_STORAGE_AREA
  FROM MASTER_RAW_INVENTORY MRI
 INNER JOIN TAB_INVENTORY_AREA TIA
    ON MRI.SKU_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
 WHERE MRI.QUANTITY > 100
   AND TIA.STORES_WHAT = 'SKU'
 ORDER BY DBMS_RANDOM.VALUE)
SELECT * FROM Q WHERE ROWNUM = 1
]]>
", row => new
 {
     SourceSkuId = row.GetValue<int>("SKU_ID"),
     SourceVwhId = row.GetValue<string>("VWH_ID"),
     SourceQualityCode = row.GetValue<string>("QUALITY_CODE"),    
     SourceArea = row.GetValue<string>("SKU_STORAGE_AREA")
 }).ExecuteSingle(_db);
            if (sourceSku == null)
            {
                Assert.Inconclusive("SKU Doesnot found");
            }



            //Inserting value in model CartonRepackInfo
            var cartonInfo = new CartonRepackInfo

            {
                DestinationCartonArea = cartonArea.DestinationArea,
                SkuId = sourceSku.SourceSkuId,
                VwhId = sourceSku.SourceVwhId,
                SourceSkuArea = sourceSku.SourceArea,
                QualityCode = sourceSku.SourceQualityCode,
                Pieces = -12,
                NumberOfCartons = 1

            };

            if (!string.IsNullOrEmpty(cartonArea.IsPalletRequired))
            {
                cartonInfo.PalletId = palletId;
            }
            //Act
            var repackCartonId = _target.RepackCarton(cartonInfo);
        }





    }
}
