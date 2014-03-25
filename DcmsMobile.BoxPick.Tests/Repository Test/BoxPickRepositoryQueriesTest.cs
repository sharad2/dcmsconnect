using System;
using System.Data.Common;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DcmsMobile.BoxPick.Repositories;
using DcmsMobile.BoxPick.Models;
using EclipseLibrary.Oracle;
using Oracle.DataAccess.Client;

namespace DcmsMobile.BoxPick.Tests.Repository_Test
{
    /// <summary>
    /// Pick Carton
    /// </summary>
    [TestClass]
    public class BoxPickRepositoryQueriesTest
    {
        public BoxPickRepositoryQueriesTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        
        private static OracleDatastore dbuser;
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) 
        {
            dbuser = new OracleDatastore(null);
            dbuser.CreateConnection("Data Source=w8bhutan/mfdev;Proxy User Id=dcms8;Proxy Password=DDM", "");
        
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }

        private BoxPickRepository _target;
        private DbTransaction _trans;
       
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _target = new BoxPickRepository(dbuser);
            _trans = _target.Db.BeginTransaction();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            _trans.Rollback();
            _trans.Dispose();
            _target.Dispose();
        }
        //
        #endregion



        /// <summary>
        /// Valid Test: 
        /// Pick the carton which is suggested by the system.
        /// Call "PickCarton()" function  of Repository to pick the suggested carton.
        /// This function takes ucc128_id, carton_id and cartonPickStartDate as parameter.
        /// Retreiving a valid pickable Ucc128_id from box table whose ia_id is null, pallet_id and carton id is not null.  This box should belongs to ADREPPWSS bucket.
        /// Retreive carton which is assinged to selected box from src_carton table.
        /// Current date will be passed as cartonPickStartDate.
        /// Validate the following:
        /// 1: Ia_id in box table should changed to Buket.SHIP_IA_ID for passed ucc128_id.
        /// 2: Passed carton should be deleted from src_carton and src_carton_detail tables.
        /// 3: Passed carton, should be inserted in src_open_carton table.
        /// 4: Validate the information(pallet_id, Vwh_id, Pieces, upc_code) in carton_productivity table for the passed carton_id.
        /// 5: Transaction is recorded in src_transaction and src_transaction_detail tables.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void PickSuggestedCaton()
        {
            //select a random box
            var suggestedBox = SqlBinder.Create(@"
               WITH Q1 AS
             (SELECT B.UCC128_ID            AS UCC128_ID,
                     B.PALLET_ID            AS PALLET_ID,
                     B.CARTON_ID            AS CARTON_ID,
                     BD.UPC_CODE            AS UPC_CODE,
                     B.VWH_ID               AS VWH_ID,
                     SCD.QUANTITY           AS QUANTITY,
                     SCD.STYLE              AS STYLE,
                     SCD.COLOR              AS COLOR,
                     SCD.DIMENSION          AS DIMENSION,
                     SCD.SKU_SIZE           AS SKU_SIZE,
                     BK.SHIP_IA_ID          AS SHIP_IA_ID,
                     SC.SHIPMENT_ID         AS SHIPMENT_ID,
                     SCD.BUNDLE_ID           AS BUNDLE_ID,
                     SC.LOCATION_ID         AS CARTON_LOCATION,
                     SC.SEWING_PLANT_CODE   AS SEWING_PLANT_CODE,
                     SC.QUALITY_CODE        AS QUALITY_CODE,
                     SC.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                     SC.PRICE_SEASON_CODE   AS PRICE_SEASON_CODE,
                     P.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID
                FROM BOX B
               INNER JOIN BOXDET BD
                  ON B.UCC128_ID = BD.UCC128_ID
                 AND B.PICKSLIP_ID = BD.PICKSLIP_ID
               INNER JOIN PS P
                  ON B.PICKSLIP_ID = P.PICKSLIP_ID
               INNER JOIN BUCKET BK
                  ON P.BUCKET_ID = BK.BUCKET_ID
               INNER JOIN SRC_CARTON SC
                  ON B.CARTON_ID = SC.CARTON_ID
               INNER JOIN SRC_CARTON_DETAIL SCD
                  ON SC.CARTON_ID = SCD.CARTON_ID
               WHERE B.PALLET_ID IS NOT NULL
                 AND B.IA_ID IS NULL
                 AND BK.PICK_MODE = 'ADREPPWSS'
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
 {
     UccId = row.GetValue<string>("UCC128_ID"),
     PalletId = row.GetValue<string>("PALLET_ID"),
     CartonId = row.GetValue<string>("CARTON_ID"),
     UpcCode = row.GetValue<string>("UPC_CODE"),
     VwhId = row.GetValue<string>("VWH_ID"),
     Pieces = row.GetValue<int>("QUANTITY"),
     ShipIaId = row.GetValue<string>("ship_ia_id"),
     Shipmentid = row.GetValue<string>("SHIPMENT_ID"),
     BundleId = row.GetValue<string>("BUNDLE_ID"),
     CartonLocation = row.GetValue<string>("CARTON_LOCATION"),
     SewingPalntCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     CartonStorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     SkuSize = row.GetValue<string>("SKU_SIZE"),
     BuildingId = row.GetValue<string>("WAREHOUSE_LOCATION_ID")
 }).ExecuteSingle(dbuser);
            if (suggestedBox == null)
            {
                Assert.Inconclusive("no box found");
            }
            _target.PickCarton(suggestedBox.UccId, suggestedBox.CartonId, DateTime.Now);

            //Select ia_id from box and assert it should be same as bucket.ship_ia_id. Also selecting current_pieces of the box and these should be
            //equal to carton quantity.
            var currentBox = SqlBinder.Create(@"
             SELECT max(b.IA_ID) as Area, SUM(bd.current_pieces) as current_pieces
              FROM BOX b
             INNER JOIN boxdet bd
                ON b.ucc128_id = bd.ucc128_id
               AND b.pickslip_id = bd.pickslip_id
             WHERE b.UCC128_ID = :ucc128_id
            GROUP BY bd.upc_code
",
            row => new
            {
                IaId = row.GetValue<string>("Area"),
                CurrentPieces = row.GetValue<int>("current_pieces")

            }).Parameter("ucc128_id", suggestedBox.UccId)
            .ExecuteSingle(dbuser);
            Assert.AreEqual(suggestedBox.ShipIaId, currentBox.IaId, "Area is not expected area");
            Assert.AreEqual(suggestedBox.Pieces, currentBox.CurrentPieces, "Current pieces are not updated");

            //Carton should be deleted from src_carton table.
            var currentCarton = SqlBinder.Create(@"
                SELECT COUNT(*) as count 
                  FROM src_carton 
                 WHERE carton_id = :carton_id
",
          row => new
          {
              Count = row.GetValue<int>("count")
          }).Parameter("carton_id", suggestedBox.CartonId)
          .ExecuteSingle(dbuser);

            Assert.IsTrue(currentCarton.Count == 0, "Carton {0} is not removed from carton table", suggestedBox.CartonId);

            //Carton should be deleted from src_carton_detail table.
            var currentCartonDetail = SqlBinder.Create(@"
                SELECT COUNT(*) as count 
                  FROM src_carton_detail
                 WHERE carton_id = :carton_id
",
            row => new
            {
                Count = row.GetValue<int>("count")
            }).Parameter("carton_id", suggestedBox.CartonId)
            .ExecuteSingle(dbuser);
            Assert.IsTrue(currentCarton.Count == 0, "Carton {0} is not removed from carton detail table", suggestedBox.CartonId);

            //Assert information inserted in carton_productivity table.
            var infoInProductivity = SqlBinder.Create(@"
             SELECT A.PALLET_ID              AS PALLET_ID,
                    A.CARTON_SOURCE_AREA      AS CARTON_SOURCE_AREA,
                    A.CARTON_DESTINATION_AREA AS CARTON_DESTINATION_AREA,
                    A.UPC_CODE                AS UPC_CODE,
                    A.CARTON_QUANTITY         AS CARTON_QUANTITY,
                    A.VWH_ID                  AS VWH_ID,
                    A.WAREHOUSE_LOCATION_ID   AS WAREHOUSE_LOCATION_ID
               FROM CARTON_PRODUCTIVITY A
              WHERE A.MODULE_CODE = 'BOXPICK'
                AND A.CARTON_ID = :carton_id
", row => new
 {
     PalletId = row.GetValue<string>("PALLET_ID"),
     CartonSourceArea = row.GetValue<string>("CARTON_SOURCE_AREA"),
     CartonDestinationArea = row.GetValue<string>("CARTON_DESTINATION_AREA"),
     UpcCode = row.GetValue<string>("UPC_CODE"),
     Pieces = row.GetValue<int>("CARTON_QUANTITY"),
     VwhId = row.GetValue<string>("VWH_ID"),
     BuildingId = row.GetValue<string>("WAREHOUSE_LOCATION_ID")
 }).Parameter("carton_id", suggestedBox.CartonId)
 .ExecuteSingle(dbuser);
            Assert.AreEqual(suggestedBox.PalletId, infoInProductivity.PalletId, "Pallet id does not match");
            Assert.AreEqual(suggestedBox.CartonStorageArea, infoInProductivity.CartonSourceArea, "Carton source area does not match");
            Assert.AreEqual(suggestedBox.ShipIaId, infoInProductivity.CartonDestinationArea, "Carton destination area does not match");
            Assert.AreEqual(suggestedBox.UpcCode, infoInProductivity.UpcCode, "Upc code does not match");
            Assert.AreEqual(suggestedBox.Pieces, infoInProductivity.Pieces, "Pieces does not match");
            Assert.AreEqual(suggestedBox.VwhId, infoInProductivity.VwhId, "Vwh id does not match");
            Assert.AreEqual(suggestedBox.BuildingId, infoInProductivity.BuildingId, "Building id does not match");

            //Assert information inserted in src_open_carton table.
            var infoInOpenCarton = SqlBinder.Create(@"
            SELECT OP.SHIPMENT_ID              AS SHIPMENT_ID,
                   OP.PRICE_SEASON_CODE        AS PRICE_SEASON_CODE,
                   OP.LAST_CARTON_STORAGE_AREA AS LAST_CARTON_STORAGE_AREA,
                   OP.LAST_LOCATION_ID         AS LAST_LOCATION_ID,
                   OP.UPC_CODE                 AS UPC_CODE,
                   OP.TOTAL_CARTON_QUANTITY    AS TOTAL_CARTON_QUANTITY,
                   OP.SEWING_PLANT_CODE        AS SEWING_PLANT_CODE,
                   OP.QUALITY_CODE             AS QUALITY_CODE,
                   OP.VWH_ID                   AS VWH_ID
              FROM SRC_OPEN_CARTON OP
             WHERE OP.CARTON_ID = :carton_id

", row => new
 {
     ShipmentId = row.GetValue<string>("SHIPMENT_ID"),
     PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
     CartonStorageArea = row.GetValue<string>("LAST_CARTON_STORAGE_AREA"),
     LastLocationId = row.GetValue<string>("LAST_LOCATION_ID"),
     UpcCode = row.GetValue<string>("UPC_CODE"),
     Quantity = row.GetValue<int>("TOTAL_CARTON_QUANTITY"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     VwhId = row.GetValue<string>("VWH_ID")
 }).Parameter("carton_id", suggestedBox.CartonId)
    .ExecuteSingle(dbuser);
            Assert.AreEqual(suggestedBox.Shipmentid, infoInOpenCarton.ShipmentId, "Shipment Id does not match");
            Assert.AreEqual(suggestedBox.PriceSeasonCode, infoInOpenCarton.PriceSeasonCode, "PriceSeasonCode does not match");
            Assert.AreEqual(suggestedBox.CartonStorageArea, infoInOpenCarton.CartonStorageArea, "CartonStorageArea does not match");
            Assert.AreEqual(suggestedBox.CartonLocation, infoInOpenCarton.LastLocationId, "Location Id does not match");
            Assert.AreEqual(suggestedBox.UpcCode, infoInOpenCarton.UpcCode, "UpcCode does not match");
            Assert.AreEqual(suggestedBox.Pieces, infoInOpenCarton.Quantity, "carton Quantity does not match");
            Assert.AreEqual(suggestedBox.SewingPalntCode, infoInOpenCarton.SewingPlantCode, "Sewing Plant code does not match");
            Assert.AreEqual(suggestedBox.QualityCode, infoInOpenCarton.QualityCode, "QualityCode does not match");
            Assert.AreEqual(suggestedBox.VwhId, infoInOpenCarton.VwhId, "Vwh Id does not match");

            //Check the Positive transaction in src_transaction family table.
            var positivetrasaction = SqlBinder.Create(@"
            WITH TR AS
             (SELECT T.TRANSACTION_TYPE        AS TRANSACTION_TYPE,
                     T.STYLE                   AS STYLE,
                     T.COLOR                   AS COLOR,
                     T.DIMENSION               AS DIMENSION,
                     T.SKU_SIZE                AS SKU_SIZE,
                     T.BUNDLE_ID               AS BUNDLE_ID,
                     T.SEWING_PLANT_CODE       AS SEWING_PLANT_CODE,
                     T.VWH_ID                  AS VWH_ID,
                     T.QUALITY_CODE            AS QUALITY_CODE,
                     TD.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                     TD.TRANSACTION_PIECES     AS TRANSACTION_PIECES
                FROM SRC_TRANSACTION T
               INNER JOIN SRC_TRANSACTION_DETAIL TD
                  ON T.TRANSACTION_ID = TD.TRANSACTION_ID
               WHERE TD.TRANSACTION_PIECES &gt; 0
                 AND T.CARTON_ID = :carton_id
               ORDER BY T.INSERT_DATE DESC)
            SELECT * FROM TR WHERE ROWNUM = 1



", row => new
 {
     TransactionType = row.GetValue<string>("TRANSACTION_TYPE"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     SkuSize = row.GetValue<string>("SKU_SIZE"),
     BundleId = row.GetValue<string>("BUNDLE_ID"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     VwhId = row.GetValue<string>("VWH_ID"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     InventoryStorageArea = row.GetValue<string>("INVENTORY_STORAGE_AREA"),
     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES")
 }).Parameter("Carton_id", suggestedBox.CartonId)
 .ExecuteSingle(dbuser);

            Assert.AreEqual("BMOV", positivetrasaction.TransactionType, "Transaction type should be BMOV");
            Assert.AreEqual(suggestedBox.Style, positivetrasaction.Style, "Style Does not match");
            Assert.AreEqual(suggestedBox.Color, positivetrasaction.Color, "Color Does not match");
            Assert.AreEqual(suggestedBox.Dimension, positivetrasaction.Dimension, "Dimension Does not match");
            Assert.AreEqual(suggestedBox.SkuSize, positivetrasaction.SkuSize, "SkuSize Does not match");
            Assert.AreEqual(suggestedBox.BundleId, positivetrasaction.BundleId, "Bundle Id Does not match");
            Assert.AreEqual(suggestedBox.SewingPalntCode, positivetrasaction.SewingPlantCode, "Sewing Plant does not match");
            Assert.AreEqual(suggestedBox.VwhId, positivetrasaction.VwhId, "Vwh Id does not match");
            Assert.AreEqual(suggestedBox.QualityCode, positivetrasaction.QualityCode, "Quality code does not match");
            Assert.AreEqual("SHL", positivetrasaction.InventoryStorageArea, "Inventory storage area should be SHL");
            Assert.AreEqual(suggestedBox.Pieces, positivetrasaction.TransactionPieces, "Transaction pieces should be same as carton pieces");

            //Check the Negative transaction in src_transaction family table.
            var negativetrasaction = SqlBinder.Create(@"
            WITH TR AS
             (SELECT T.TRANSACTION_TYPE        AS TRANSACTION_TYPE,
                     T.STYLE                   AS STYLE,
                     T.COLOR                   AS COLOR,
                     T.DIMENSION               AS DIMENSION,
                     T.SKU_SIZE                AS SKU_SIZE,
                     T.BUNDLE_ID               AS BUNDLE_ID,
                     T.SEWING_PLANT_CODE       AS SEWING_PLANT_CODE,
                     T.VWH_ID                  AS VWH_ID,
                     T.QUALITY_CODE            AS QUALITY_CODE,
                     TD.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                     TD.TRANSACTION_PIECES     AS TRANSACTION_PIECES
                FROM SRC_TRANSACTION T
               INNER JOIN SRC_TRANSACTION_DETAIL TD
                  ON T.TRANSACTION_ID = TD.TRANSACTION_ID
               WHERE TD.TRANSACTION_PIECES &lt; 0
                 AND T.CARTON_ID = :carton_id
               ORDER BY T.INSERT_DATE DESC)
            SELECT * FROM TR WHERE ROWNUM = 1
", row => new
 {
     TransactionType = row.GetValue<string>("TRANSACTION_TYPE"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     SkuSize = row.GetValue<string>("SKU_SIZE"),
     BundleId = row.GetValue<string>("BUNDLE_ID"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     VwhId = row.GetValue<string>("VWH_ID"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     InventoryStorageArea = row.GetValue<string>("INVENTORY_STORAGE_AREA"),
     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES")
 }).Parameter("Carton_id", suggestedBox.CartonId)
 .ExecuteSingle(dbuser);

            Assert.AreEqual("BMOV", negativetrasaction.TransactionType, "Transaction type should be BMOV");
            Assert.AreEqual(suggestedBox.Style, negativetrasaction.Style, "Style Does not match");
            Assert.AreEqual(suggestedBox.Color, negativetrasaction.Color, "Color Does not match");
            Assert.AreEqual(suggestedBox.Dimension, negativetrasaction.Dimension, "Dimension Does not match");
            Assert.AreEqual(suggestedBox.SkuSize, negativetrasaction.SkuSize, "SkuSize Does not match");
            Assert.AreEqual(suggestedBox.BundleId, negativetrasaction.BundleId, "Bundle Id Does not match");
            Assert.AreEqual(suggestedBox.SewingPalntCode, negativetrasaction.SewingPlantCode, "Sewing Plant does not match");
            Assert.AreEqual(suggestedBox.VwhId, negativetrasaction.VwhId, "Vwh Id does not match");
            Assert.AreEqual(suggestedBox.QualityCode, negativetrasaction.QualityCode, "Quality code does not match");
            Assert.AreEqual("BIR", negativetrasaction.InventoryStorageArea, "Inventory storage area should be SHL");
            Assert.IsTrue(suggestedBox.Pieces + negativetrasaction.TransactionPieces == 0, "Transaction pieces should be same as carton pieces");

        }



        /// <summary>
        /// valid Test: 
        /// Pick Any carton instead of Suggested carton of same SKU, Vwh and Quantity.
        /// Call "PickCarton()" function  of Repository to pick any carton.
        /// This function takes ucc128_id, carton_id and cartonPickStartDate as parameter.
        /// Retreiving a valid Ucc128_id from box table whose ia_id is null, pallet_id and carton id is not null.  This box should belongs to ADREPPWSS bucket.
        /// Retreive carton which is similar to assinged carton from src_carton table.
        /// Validate the following:
        /// 1: Ia_id in box table should changed to Buket.SHIP_IA_ID for passed ucc128_id.
        /// 2: Passed carton should be deleted from src_carton and src_carton_detail.
        /// 3. Carton_productivity table should be populated for the passed carton_id along with pallet_id, Vwh_id, Pieces, upc_code
        /// 4: "Req_Process_Id, REQ_MODULE_CODE, REQ_LINE_NUMBER" of passed carton should be assign to suggested carton.
        /// 5: Information(ShipmentId, Price_season_code,vwhid, Quality_code, Sewing_plant_code,Carton_storage_area and Quantity) should be inserted in 
        ///    src_open_carton table for the passed carton.
        /// 6: Transaction is recorded in src_transaction and src_transaction_detail tables.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void PickSimilarCarton()
        {
            //select a random box
             var suggestedBox = SqlBinder.Create(@"
                                     WITH Q1 AS
             (SELECT B.UCC128_ID            AS UCC128_ID,
                     B.PALLET_ID            AS PALLET_ID,
                     B.CARTON_ID            AS CARTON_ID,
                     BD.UPC_CODE            AS UPC_CODE,
                     B.VWH_ID               AS VWH_ID,
                     BD.EXPECTED_PIECES     AS EXPECTED_PIECES,
                     BK.SHIP_IA_ID          AS SHIP_IA_ID,
                     SCD.REQ_PROCESS_ID     AS REQ_PROCESS_ID,
                     SCD.REQ_MODULE_CODE    AS REQ_MODULE_CODE,
                     SCD.REQ_LINE_NUMBER    AS REQ_LINE_NUMBER,
                     SC.SHIPMENT_ID         AS SHIPMENT_ID,
                     SC.SEWING_PLANT_CODE   AS SEWING_PLANT_CODE,
                     SC.QUALITY_CODE        AS QUALITY_CODE,
                     BK.PULL_CARTON_AREA    AS PULL_CARTON_AREA,
                     SC.PRICE_SEASON_CODE   AS PRICE_SEASON_CODE,
                     P.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID
                FROM BOX B
               INNER JOIN BOXDET BD
                  ON B.UCC128_ID = BD.UCC128_ID
                 AND B.PICKSLIP_ID = BD.PICKSLIP_ID
               INNER JOIN PS P
                  ON B.PICKSLIP_ID = P.PICKSLIP_ID
               INNER JOIN BUCKET BK
                  ON P.BUCKET_ID = BK.BUCKET_ID
               INNER JOIN SRC_CARTON SC
                  ON B.CARTON_ID = SC.CARTON_ID
               INNER JOIN SRC_CARTON_DETAIL SCD
                  ON SC.CARTON_ID = SCD.CARTON_ID
               WHERE B.PALLET_ID IS NOT NULL
                 AND B.IA_ID IS NULL
                 AND BK.PICK_MODE = 'ADREPPWSS'
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q1 WHERE ROWNUM = 1

", row => new
 {
     Ucc128Id = row.GetValue<string>("UCC128_ID"),
     PalletId = row.GetValue<string>("PALLET_ID"),
     CartonId = row.GetValue<string>("CARTON_ID"),
     UpcCode = row.GetValue<string>("UPC_CODE"),
     VwhId = row.GetValue<string>("VWH_ID"),
     Pieces = row.GetValue<int>("EXPECTED_PIECES"),
     ShipIaId = row.GetValue<string>("ship_ia_id"),
     Shipmentid = row.GetValue<string>("SHIPMENT_ID"),
     SewingPalntCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     PullCartonArea = row.GetValue<string>("PULL_CARTON_AREA"),
     PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
     BuildingId = row.GetValue<string>("WAREHOUSE_LOCATION_ID"),
     Process_id = row.GetValue<int?>("REQ_PROCESS_ID"),
     Module_code = row.GetValue<string>("REQ_MODULE_CODE"),
     Line_number = row.GetValue<int?>("REQ_LINE_NUMBER")
 }).ExecuteSingle(dbuser);

            if (suggestedBox == null)
            {
                Assert.Inconclusive("No box found");
            }

            //select a similar carton(same quantity) from src_carton
            var similarCarton = SqlBinder.Create(@"
            WITH Q1 AS
             (
             SELECT C.CARTON_ID           AS CARTON_ID,
                    C.SHIPMENT_ID         AS SHIPMENT_ID,
                    C.PRICE_SEASON_CODE   AS PRICE_SEASON_CODE,
                    C.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                    C.QUALITY_CODE        AS QUALITY_CODE,
                    C.SEWING_PLANT_CODE   AS SEWING_PLANT_CODE,
                    C.VWH_ID              AS VWH_ID,
                    C.LOCATION_ID         AS LOCATION_ID,
                    CD.BUNDLE_ID          AS BUNDLE_ID,
                    CD.STYLE              AS STYLE,
                    CD.COLOR              AS COLOR,
                    CD.DIMENSION          AS DIMENSION,
                    CD.SKU_SIZE           AS SKU_SIZE,
                    CD.QUANTITY           AS QUANTITY,
                    CD.REQ_PROCESS_ID     AS REQ_PROCESS_ID,
                    CD.REQ_MODULE_CODE    AS REQ_MODULE_CODE,
                    CD.REQ_LINE_NUMBER    AS REQ_LINE_NUMBER
                FROM SRC_CARTON C
               INNER JOIN SRC_CARTON_DETAIL CD
                  ON C.CARTON_ID = CD.CARTON_ID
               INNER JOIN MASTER_SKU MS
                  ON CD.SKU_ID = MS.SKU_ID
               WHERE MS.UPC_CODE = :UPC_CODE
                 AND CD.QUANTITY = :PIECES
                 AND C.VWH_ID    = :VWH_ID
                 AND C.CARTON_STORAGE_AREA = :PULL_CARTON_AREA
                 AND C.CARTON_ID &lt;&gt; :CARTON_ID
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
               {
                   ShipMentId = row.GetValue<string>("SHIPMENT_ID"),
                   PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
                   CartonStorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
                   QualityCode = row.GetValue<string>("QUALITY_CODE"),
                   SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
                   VwhId = row.GetValue<string>("VWH_ID"),
                   LocationId = row.GetValue<string>("LOCATION_ID"),
                   BundleId = row.GetValue<string>("BUNDLE_ID"),
                   CartonQuantity = row.GetValue<int>("QUANTITY"),
                   Carton_id = row.GetValue<string>("CARTON_ID"),
                   Process_id = row.GetValue<int?>("REQ_PROCESS_ID"),
                   Module_code = row.GetValue<string>("REQ_MODULE_CODE"),
                   Line_number = row.GetValue<int?>("REQ_LINE_NUMBER"),
                   Style = row.GetValue<string>("STYLE"),
                   Color = row.GetValue<string>("COLOR"),
                   Dimension = row.GetValue<string>("DIMENSION"),
                   SkuSize = row.GetValue<string>("SKU_SIZE")
               }).Parameter("UPC_CODE", suggestedBox.UpcCode)
               .Parameter("PIECES", suggestedBox.Pieces)
               .Parameter("PULL_CARTON_AREA", suggestedBox.PullCartonArea)
               .Parameter("VWH_ID", suggestedBox.VwhId)
               .Parameter("CARTON_ID", suggestedBox.CartonId)
               .ExecuteSingle(dbuser);
            if (similarCarton == null)
            {
                Assert.Inconclusive("No carton found");
            }

            _target.PickCarton(suggestedBox.Ucc128Id, similarCarton.Carton_id, DateTime.Now);

            //Select ia_id from box and assert it should be same as bucket.ship_ia_id. Also selecting current_pieces of the box and these should be
            //equal to carton quantity.
            var currentBox = SqlBinder.Create(@"
            SELECT max(b.IA_ID) as Area,
                SUM(bd.current_pieces) as current_pieces
              FROM BOX b
             INNER JOIN boxdet bd
                ON b.ucc128_id = bd.ucc128_id
               AND b.pickslip_id = bd.pickslip_id
             WHERE b.UCC128_ID = :UCC128_ID
            GROUP BY bd.upc_code
",
            row => new
            {
                IaId = row.GetValue<string>("Area"),
                CurrentPieces = row.GetValue<int>("current_pieces")
            }).Parameter("UCC128_ID", suggestedBox.Ucc128Id)
            .ExecuteSingle(dbuser);
            Assert.AreEqual(suggestedBox.ShipIaId, currentBox.IaId, "Area is not expected area");
            Assert.AreEqual(similarCarton.CartonQuantity, currentBox.CurrentPieces, "Current pieces are not updated");

            //Assert Carton should be deleted from src_carton table.
            var currentCarton = SqlBinder.Create(@"
                SELECT COUNT(*) as count 
                FROM src_carton 
                WHERE carton_id = :carton_id
",
          row => new
          {
              Count = row.GetValue<int>("count")
          }).Parameter("carton_id", similarCarton.Carton_id)
          .ExecuteSingle(dbuser);
            Assert.IsTrue(currentCarton.Count == 0, "Carton {0} is not removed from carton table", similarCarton.Carton_id);

            //Assert Carton should be deleted from src_carton_detail table.
            var currentCartonDetail = SqlBinder.Create(@"
                SELECT COUNT(*) as count 
                FROM src_carton_detail
                WHERE carton_id = :carton_id
",
        row => new
        {
            Count = row.GetValue<int>("count")
        }).Parameter("carton_id", similarCarton.Carton_id)
        .ExecuteSingle(dbuser);
            Assert.IsTrue(currentCarton.Count == 0, "Carton {0} is not removed from carton detail table", similarCarton.Carton_id);

            //Assert that "REQ_PROCESS_ID, REQ_MODULE_CODE, REQ_LINE_NUMBER" of SimilarCarton should be assign to SuggestedCarton.
            var suggestedCarton = SqlBinder.Create(@"
            SELECT SCD.REQ_PROCESS_ID     AS REQ_PROCESS_ID,
                   SCD.REQ_MODULE_CODE    AS REQ_MODULE_CODE,
                   SCD.REQ_LINE_NUMBER    AS REQ_LINE_NUMBER
              FROM SRC_CARTON SC
             INNER JOIN SRC_CARTON_DETAIL SCD
                ON SC.CARTON_ID = SCD.CARTON_ID
             WHERE SCD.CARTON_ID = :CARTON_ID
",
row => new
{
    Process_id = row.GetValue<int?>("REQ_PROCESS_ID"),
    Module_code = row.GetValue<string>("REQ_MODULE_CODE"),
    Line_number = row.GetValue<int?>("REQ_LINE_NUMBER")
}).Parameter("CARTON_ID", suggestedBox.CartonId)
.ExecuteSingle(dbuser);
            Assert.AreEqual(similarCarton.Process_id, suggestedCarton.Process_id, "Process id is not updated");
            Assert.AreEqual(similarCarton.Module_code, suggestedCarton.Module_code, "Module code is not updated");
            Assert.AreEqual(similarCarton.Line_number, suggestedCarton.Line_number, "Line number is not updated");


            //Assert information inserted in carton_productivity table.
            var infoInProductivity = SqlBinder.Create(@"
            SELECT A.PALLET_ID               AS PALLET_ID,
                   A.ACTION_CODE             AS ACTION_CODE,
                   A.CARTON_SOURCE_AREA      AS CARTON_SOURCE_AREA,
                   A.CARTON_DESTINATION_AREA AS CARTON_DESTINATION_AREA,
                   A.UPC_CODE                AS UPC_CODE,
                   A.CARTON_QUANTITY         AS CARTON_QUANTITY,
                   A.VWH_ID                  AS VWH_ID,
                   A.WAREHOUSE_LOCATION_ID   AS WAREHOUSE_LOCATION_ID
              FROM CARTON_PRODUCTIVITY A
             WHERE A.MODULE_CODE = 'BOXPICK'
               AND A.CARTON_ID = :CARTON_ID
", row => new
{
    PalletId = row.GetValue<string>("PALLET_ID"),
    ActionCode = row.GetValue<string>("ACTION_CODE"),
    CartonSourceArea = row.GetValue<string>("CARTON_SOURCE_AREA"),
    CartonDestinationArea = row.GetValue<string>("CARTON_DESTINATION_AREA"),
    UpcCode = row.GetValue<string>("UPC_CODE"),
    Pieces = row.GetValue<int>("CARTON_QUANTITY"),
    VwhId = row.GetValue<string>("VWH_ID"),
    BuildingId = row.GetValue<string>("WAREHOUSE_LOCATION_ID")
}).Parameter("CARTON_ID", similarCarton.Carton_id)
.ExecuteSingle(dbuser);
            Assert.AreEqual(suggestedBox.PalletId, infoInProductivity.PalletId, "Pallet id does not match");
            Assert.AreEqual(similarCarton.CartonStorageArea, infoInProductivity.CartonSourceArea, "Carton source area does not match");
            Assert.AreEqual(suggestedBox.ShipIaId, infoInProductivity.CartonDestinationArea, "Carton destination area does not match");
            Assert.AreEqual(suggestedBox.UpcCode, infoInProductivity.UpcCode, "Upc code does not match");
            Assert.AreEqual(similarCarton.CartonQuantity, infoInProductivity.Pieces, "Pieces does not match");
            Assert.AreEqual(similarCarton.VwhId, infoInProductivity.VwhId, "Vwh id does not match");
            Assert.AreEqual(suggestedBox.BuildingId, infoInProductivity.BuildingId, "Building id does not match");

            //Assert information inserted in src_open_carton table.
            var infoInOpenCarton = SqlBinder.Create(@"
            SELECT OP.SHIPMENT_ID              AS SHIPMENT_ID,
                   OP.PRICE_SEASON_CODE        AS PRICE_SEASON_CODE,
                   OP.LAST_CARTON_STORAGE_AREA AS LAST_CARTON_STORAGE_AREA,
                   OP.LAST_LOCATION_ID         AS LAST_LOCATION_ID,
                   OP.UPC_CODE                 AS UPC_CODE,
                   OP.TOTAL_CARTON_QUANTITY    AS TOTAL_CARTON_QUANTITY,
                   OP.SEWING_PLANT_CODE        AS SEWING_PLANT_CODE,
                   OP.QUALITY_CODE             AS QUALITY_CODE,
                   OP.VWH_ID                   AS VWH_ID
              FROM SRC_OPEN_CARTON OP
             WHERE OP.CARTON_ID = :CARTON_ID

", row => new
 {
     ShipmentId = row.GetValue<string>("SHIPMENT_ID"),
     PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
     CartonStorageArea = row.GetValue<string>("LAST_CARTON_STORAGE_AREA"),
     LastLocationId = row.GetValue<string>("LAST_LOCATION_ID"),
     UpcCode = row.GetValue<string>("UPC_CODE"),
     Quantity = row.GetValue<int>("TOTAL_CARTON_QUANTITY"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     VwhId = row.GetValue<string>("VWH_ID")
 }).Parameter("CARTON_ID", similarCarton.Carton_id)
    .ExecuteSingle(dbuser);
            Assert.AreEqual(similarCarton.ShipMentId, infoInOpenCarton.ShipmentId, "Shipment Id does not match");
            Assert.AreEqual(similarCarton.PriceSeasonCode, infoInOpenCarton.PriceSeasonCode, "PriceSeasonCode does not match");
            Assert.AreEqual(similarCarton.CartonStorageArea, infoInOpenCarton.CartonStorageArea, "CartonStorageArea does not match");
            Assert.AreEqual(similarCarton.LocationId, infoInOpenCarton.LastLocationId, "Location Id does not match");
            Assert.AreEqual(suggestedBox.UpcCode, infoInOpenCarton.UpcCode, "UpcCode does not match");
            Assert.AreEqual(similarCarton.CartonQuantity, infoInOpenCarton.Quantity, "carton Quantity does not match");
            Assert.AreEqual(similarCarton.SewingPlantCode, infoInOpenCarton.SewingPlantCode, "Sewing Plant code does not match");
            Assert.AreEqual(similarCarton.QualityCode, infoInOpenCarton.QualityCode, "QualityCode does not match");
            Assert.AreEqual(similarCarton.VwhId, infoInOpenCarton.VwhId, "Vwh Id does not match");

            //Check the Positive transaction in src_transaction family table.
            var positiveTrasaction = SqlBinder.Create(@"
            WITH TR AS
             (SELECT T.TRANSACTION_TYPE        AS TRANSACTION_TYPE,
                     T.STYLE                   AS STYLE,
                     T.COLOR                   AS COLOR,
                     T.DIMENSION               AS DIMENSION,
                     T.SKU_SIZE                AS SKU_SIZE,
                     T.BUNDLE_ID               AS BUNDLE_ID,
                     T.SEWING_PLANT_CODE       AS SEWING_PLANT_CODE,
                     T.VWH_ID                  AS VWH_ID,
                     T.QUALITY_CODE            AS QUALITY_CODE,
                     TD.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                     TD.TRANSACTION_PIECES     AS TRANSACTION_PIECES
                FROM SRC_TRANSACTION T
               INNER JOIN SRC_TRANSACTION_DETAIL TD
                  ON T.TRANSACTION_ID = TD.TRANSACTION_ID
               WHERE TD.TRANSACTION_PIECES &gt; 0
                 AND T.CARTON_ID = :carton_id
               ORDER BY T.INSERT_DATE DESC)
            SELECT * FROM TR WHERE ROWNUM = 1
", row => new
 {
     TransactionType = row.GetValue<string>("TRANSACTION_TYPE"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     SkuSize = row.GetValue<string>("SKU_SIZE"),
     BundleId = row.GetValue<string>("BUNDLE_ID"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     VwhId = row.GetValue<string>("VWH_ID"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     InventoryStorageArea = row.GetValue<string>("INVENTORY_STORAGE_AREA"),
     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES")
 }).Parameter("Carton_id", similarCarton.Carton_id)
 .ExecuteSingle(dbuser);

            Assert.AreEqual("BMOV", positiveTrasaction.TransactionType, "Transaction type should be BMOV");
            Assert.AreEqual(similarCarton.Style, positiveTrasaction.Style, "Style Does not match");
            Assert.AreEqual(similarCarton.Color, positiveTrasaction.Color, "Color Does not match");
            Assert.AreEqual(similarCarton.Dimension, positiveTrasaction.Dimension, "Dimension Does not match");
            Assert.AreEqual(similarCarton.SkuSize, positiveTrasaction.SkuSize, "SkuSize Does not match");
            Assert.AreEqual(similarCarton.BundleId, positiveTrasaction.BundleId, "Bundle Id Does not match");
            Assert.AreEqual(similarCarton.SewingPlantCode, positiveTrasaction.SewingPlantCode, "Sewing Plant does not match");
            Assert.AreEqual(similarCarton.VwhId, positiveTrasaction.VwhId, "Vwh Id does not match");
            Assert.AreEqual(similarCarton.QualityCode, positiveTrasaction.QualityCode, "Quality code does not match");
            Assert.AreEqual("SHL", positiveTrasaction.InventoryStorageArea, "Inventory storage area should be SHL");
            Assert.AreEqual(similarCarton.CartonQuantity, positiveTrasaction.TransactionPieces, "Transaction pieces should be same as carton pieces");

            //Check the Negative transaction in src_transaction family table.
            var negativeTrasaction = SqlBinder.Create(@"
            WITH TR AS
             (SELECT T.TRANSACTION_TYPE        AS TRANSACTION_TYPE,
                     T.STYLE                   AS STYLE,
                     T.COLOR                   AS COLOR,
                     T.DIMENSION               AS DIMENSION,
                     T.SKU_SIZE                AS SKU_SIZE,
                     T.BUNDLE_ID               AS BUNDLE_ID,
                     T.SEWING_PLANT_CODE       AS SEWING_PLANT_CODE,
                     T.VWH_ID                  AS VWH_ID,
                     T.QUALITY_CODE            AS QUALITY_CODE,
                     TD.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                     TD.TRANSACTION_PIECES     AS TRANSACTION_PIECES
                FROM SRC_TRANSACTION T
               INNER JOIN SRC_TRANSACTION_DETAIL TD
                  ON T.TRANSACTION_ID = TD.TRANSACTION_ID
               WHERE TD.TRANSACTION_PIECES &lt; 0
                 AND T.CARTON_ID = :carton_id
               ORDER BY T.INSERT_DATE DESC)
            SELECT * FROM TR WHERE ROWNUM = 1
", row => new
 {
     TransactionType = row.GetValue<string>("TRANSACTION_TYPE"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     SkuSize = row.GetValue<string>("SKU_SIZE"),
     BundleId = row.GetValue<string>("BUNDLE_ID"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     VwhId = row.GetValue<string>("VWH_ID"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     InventoryStorageArea = row.GetValue<string>("INVENTORY_STORAGE_AREA"),
     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES")
 }).Parameter("Carton_id", similarCarton.Carton_id)
 .ExecuteSingle(dbuser);

            Assert.AreEqual("BMOV", negativeTrasaction.TransactionType, "Transaction type should be BMOV");
            Assert.AreEqual(similarCarton.Style, negativeTrasaction.Style, "Style Does not match");
            Assert.AreEqual(similarCarton.Color, negativeTrasaction.Color, "Color Does not match");
            Assert.AreEqual(similarCarton.Dimension, negativeTrasaction.Dimension, "Dimension Does not match");
            Assert.AreEqual(similarCarton.SkuSize, negativeTrasaction.SkuSize, "SkuSize Does not match");
            Assert.AreEqual(similarCarton.BundleId, negativeTrasaction.BundleId, "Bundle Id Does not match");
            Assert.AreEqual(similarCarton.SewingPlantCode, negativeTrasaction.SewingPlantCode, "Sewing Plant does not match");
            Assert.AreEqual(similarCarton.VwhId, negativeTrasaction.VwhId, "Vwh Id does not match");
            Assert.AreEqual(similarCarton.QualityCode, negativeTrasaction.QualityCode, "Quality code does not match");
            Assert.AreEqual(similarCarton.CartonStorageArea, negativeTrasaction.InventoryStorageArea, "Inventory storage area should be SHL");
            Assert.IsTrue(similarCarton.CartonQuantity + negativeTrasaction.TransactionPieces == 0, "Transaction pieces should be same as carton pieces");
        }

        /// <summary>
        /// valid Test: 
        /// Select a case where suggested carton is not available. Pick Any carton which is similar to suggested carton of same SKU and quantity.
        /// Call "PickCarton()" function  of Repository to pick any carton.
        /// This function takes ucc128_id, carton_id and cartonPickStartDate as parameter.
        /// Retreiving a valid Ucc128_id from box table whose ia_id is null, pallet_id and carton id is not null and it should not be in src_carton table.
        /// This box should belongs to ADREPPWSS bucket.
        /// Retreive carton which is similar to suggested carton from src_carton table.
        /// Current date will be passed as cartonPickStartDate.
        /// 1: Test ia_id in box table should be update and it will be Buket SHIP_IA_ID.
        /// 2: Passed carton should be deleted from src_carton and src_carton_detail.
        /// 3: Information(pallet_id,Vwh_id,Pieces,upc_code) should be inserted in carton_productivity table.
        /// 4: Information(ShipmentId, priceseasonCode,vwhid,qyalitycode,sewingPlantCode,CartonStorageArea,Quantity) should be inserted in src_open_carton table.
        /// 5: Transaction is recorded in src_transaction and src-transaction_detail tables.
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void PickSimilarCartonSuggestesNotExist()
        {
            //select a random box
            var expectedBox = SqlBinder.Create(@"
             WITH Q1 AS
             (SELECT B.UCC128_ID         AS UCC128_ID,
                     B.PALLET_ID         AS PALLET_ID,
                     BD.UPC_CODE         AS UPC_CODE,
                     B.VWH_ID            AS VWH_ID,
                     B.CARTON_ID         AS CARTON_ID,
                     BK.ship_ia_id       as ship_ia_id,
                     BK.pull_carton_area as pull_carton_area,
                     P.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                     BD.EXPECTED_PIECES  AS EXPECTED_PIECES
                    FROM BOX B
               INNER JOIN BOXDET BD
                  ON B.UCC128_ID = BD.UCC128_ID
                 AND B.PICKSLIP_ID = BD.PICKSLIP_ID
               INNER JOIN PS P
                  ON B.PICKSLIP_ID = P.PICKSLIP_ID
               INNER JOIN BUCKET BK
                  ON P.BUCKET_ID = BK.BUCKET_ID
               WHERE B.PALLET_ID IS NOT NULL
                 AND B.IA_ID IS NULL
                 AND BK.PICK_MODE = 'ADREPPWSS'
                 AND B.CARTON_ID NOT IN(SELECT CT.CARTON_ID FROM SRC_CARTON CT)
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q1 WHERE ROWNUM = 1
        ", row => new
         {
             UccId = row.GetValue<string>("UCC128_ID"),
             Carton_id = row.GetValue<string>("CARTON_ID"),
             Pallet_id = row.GetValue<string>("PALLET_ID"),
             Upc_code = row.GetValue<string>("UPC_CODE"),
             Vwh_id = row.GetValue<string>("VWH_ID"),
             ShipIaId = row.GetValue<string>("ship_ia_id"),
             PullCartonArea = row.GetValue<string>("pull_carton_area"),
             BuildingId = row.GetValue<string>("WAREHOUSE_LOCATION_ID"),
             BoxPieces = row.GetValue<int>("EXPECTED_PIECES")
         }).ExecuteSingle(dbuser);

            if (expectedBox == null)
            {
                Assert.Inconclusive("No box found");
            }

            //select a similar carton(same quantity and upc code) from src_carton

            var similarCarton = SqlBinder.Create(@"
             WITH Q1 AS
             (SELECT C.CARTON_ID           AS CARTON_ID,
                     C.SHIPMENT_ID         AS SHIPMENT_ID,
                     C.PRICE_SEASON_CODE   AS PRICE_SEASON_CODE,
                     C.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                     C.QUALITY_CODE        AS QUALITY_CODE,
                     C.VWH_ID              AS VWH_ID,
                     C.SEWING_PLANT_CODE   AS SEWING_PLANT_CODE,
                     C.LOCATION_ID         AS LOCATION_ID,
                     CD.BUNDLE_ID          AS BUNDLE_ID,
                     CD.STYLE              AS STYLE,
                     CD.COLOR              AS COLOR,
                     CD.DIMENSION          AS DIMENSION,
                     CD.SKU_SIZE           AS SKU_SIZE,
                     CD.QUANTITY           AS QUANTITY,
                     CD.REQ_PROCESS_ID     AS REQ_PROCESS_ID,
                     CD.REQ_MODULE_CODE    AS REQ_MODULE_CODE,
                     CD.REQ_LINE_NUMBER    AS REQ_LINE_NUMBER
                FROM SRC_CARTON C
               INNER JOIN SRC_CARTON_DETAIL CD
                  ON C.CARTON_ID = CD.CARTON_ID
               INNER JOIN MASTER_SKU MS
                  ON CD.SKU_ID = MS.SKU_ID
               WHERE MS.UPC_CODE = :UPC_CODE
                 AND CD.QUANTITY = :quantity
                 AND C.VWH_ID    = :VWH_ID 
                 AND C.CARTON_STORAGE_AREA = :pull_carton_area
                 AND C.CARTON_ID &lt;&gt; :CARTON_ID   
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
    {
        ShipMentId = row.GetValue<string>("SHIPMENT_ID"),
        PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
        CartonStorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
        QualityCode = row.GetValue<string>("QUALITY_CODE"),
        VwhId = row.GetValue<string>("VWH_ID"),
        SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
        LocationId = row.GetValue<string>("LOCATION_ID"),
        BundleId = row.GetValue<string>("BUNDLE_ID"),
        Quantity = row.GetValue<int>("QUANTITY"),
        Carton_id = row.GetValue<string>("CARTON_ID"),
        Process_id = row.GetValue<int?>("REQ_PROCESS_ID"),
        Module_code = row.GetValue<string>("REQ_MODULE_CODE"),
        Line_number = row.GetValue<int?>("REQ_LINE_NUMBER"),
        Style = row.GetValue<string>("STYLE"),
        Color = row.GetValue<string>("COLOR"),
        Dimension = row.GetValue<string>("DIMENSION"),
        SkuSize = row.GetValue<string>("SKU_SIZE")
    }).Parameter("upc_code", expectedBox.Upc_code)
    .Parameter("VWH_ID", expectedBox.Vwh_id)
    .Parameter("quantity", expectedBox.BoxPieces)
    .Parameter("pull_carton_area", expectedBox.PullCartonArea)
    .Parameter("CARTON_ID", expectedBox.Carton_id)
       .ExecuteSingle(dbuser);
            if (similarCarton == null)
            {
                Assert.Inconclusive("Carton Does Not exist");
            }
            _target.PickCarton(expectedBox.UccId, similarCarton.Carton_id, DateTime.Now);

            //Select ia_id from box and assert it should be same as bucket.ship_ia_id. Also selecting current_pieces of the box and these should be
            //equal to carton quantity.
            var currentBox = SqlBinder.Create(@"
            SELECT max(b.IA_ID) as Area,
                SUM(bd.current_pieces) as current_pieces
              FROM BOX b
             INNER JOIN boxdet bd
                ON b.ucc128_id = bd.ucc128_id
               AND b.pickslip_id = bd.pickslip_id
             WHERE b.UCC128_ID = :UCC128_ID
            GROUP BY bd.upc_code
",
           row => new
           {
               IaId = row.GetValue<string>("Area"),
               CurrentPieces = row.GetValue<int>("current_pieces")

           }).Parameter("UCC128_ID", expectedBox.UccId)
           .ExecuteSingle(dbuser);
            Assert.AreEqual(expectedBox.ShipIaId, currentBox.IaId, "Area is not expected area");
            Assert.AreEqual(similarCarton.Quantity, currentBox.CurrentPieces, "Current pieces are not updated");

            //Assert Carton should be deleted from src_carton table.
            var currentCarton = SqlBinder.Create(@"
                SELECT COUNT(*) as count 
                FROM src_carton 
                WHERE carton_id = :carton_id
",
          row => new
          {
              Count = row.GetValue<int>("count")
          }).Parameter("carton_id", similarCarton.Carton_id)
          .ExecuteSingle(dbuser);
            Assert.IsTrue(currentCarton.Count == 0, "Carton {0} is not removed from carton table", similarCarton.Carton_id);

            //Assert Carton should be deleted from src_carton_detail table.
            var currentCartonDetail = SqlBinder.Create(@"
                SELECT COUNT(*) as count 
                FROM src_carton_detail
                WHERE carton_id = :carton_id
",
                         row => new
                         {
                             Count = row.GetValue<int>("count")
                         }).Parameter("carton_id", similarCarton.Carton_id)
                         .ExecuteSingle(dbuser);
            Assert.IsTrue(currentCarton.Count == 0, "Carton {0} is not removed from carton detail table", similarCarton.Carton_id);

            //Check that Suggested Carton has been lost.
            var suggestedCarton = SqlBinder.Create(@"
            SELECT SC.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                   SCD.REQ_PROCESS_ID     AS REQ_PROCESS_ID,
                   SCD.REQ_MODULE_CODE    AS REQ_MODULE_CODE,
                   SCD.REQ_LINE_NUMBER    AS REQ_LINE_NUMBER
              FROM SRC_CARTON SC
             INNER JOIN SRC_CARTON_DETAIL SCD
                ON SC.CARTON_ID = SCD.CARTON_ID
             WHERE SCD.CARTON_ID = :CARTON_ID
",
row => new
{
    Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
    Process_id = row.GetValue<int?>("REQ_PROCESS_ID"),
    Module_code = row.GetValue<string>("REQ_MODULE_CODE"),
    Line_number = row.GetValue<int?>("REQ_LINE_NUMBER")
}).Parameter("CARTON_ID", expectedBox.Carton_id)
.ExecuteSingle(dbuser);

            Assert.IsNull(suggestedCarton, "Must be null");

            //Assert information inserted in carton_productivity table.
            var infoInProductivity = SqlBinder.Create(@"
            SELECT A.PALLET_ID               AS PALLET_ID,
                   A.ACTION_CODE             AS ACTION_CODE,
                   A.CARTON_SOURCE_AREA      AS CARTON_SOURCE_AREA,
                   A.CARTON_DESTINATION_AREA AS CARTON_DESTINATION_AREA,
                   A.UPC_CODE                AS UPC_CODE,
                   A.CARTON_QUANTITY         AS CARTON_QUANTITY,
                   A.VWH_ID                  AS VWH_ID,
                   A.WAREHOUSE_LOCATION_ID   AS WAREHOUSE_LOCATION_ID
              FROM CARTON_PRODUCTIVITY A
             WHERE A.MODULE_CODE = 'BOXPICK'
               AND A.CARTON_ID = :CARTON_ID
", row => new
 {
     PalletId = row.GetValue<string>("PALLET_ID"),
     ActionCode = row.GetValue<string>("ACTION_CODE"),
     CartonSourceArea = row.GetValue<string>("CARTON_SOURCE_AREA"),
     CartonDestinationArea = row.GetValue<string>("CARTON_DESTINATION_AREA"),
     UpcCode = row.GetValue<string>("UPC_CODE"),
     Pieces = row.GetValue<int>("CARTON_QUANTITY"),
     VwhId = row.GetValue<string>("VWH_ID"),
     BuildingId = row.GetValue<string>("WAREHOUSE_LOCATION_ID")
 }).Parameter("carton_id", similarCarton.Carton_id)
 .ExecuteSingle(dbuser);
            Assert.AreEqual(expectedBox.Pallet_id, infoInProductivity.PalletId, "Pallet id does not match");
            Assert.AreEqual(similarCarton.CartonStorageArea, infoInProductivity.CartonSourceArea, "Carton source area does not match");
            Assert.AreEqual(expectedBox.ShipIaId, infoInProductivity.CartonDestinationArea, "Carton destination area does not match");
            Assert.AreEqual(expectedBox.Upc_code, infoInProductivity.UpcCode, "Upc code does not match");
            Assert.AreEqual(similarCarton.Quantity, infoInProductivity.Pieces, "Pieces does not match");
            Assert.AreEqual(expectedBox.Vwh_id, infoInProductivity.VwhId, "Vwh id does not match");
            Assert.AreEqual(expectedBox.BuildingId, infoInProductivity.BuildingId, "Building id does not match");

            //Assert information inserted in src_open_carton table.
            var infoInOpenCarton = SqlBinder.Create(@"
             SELECT OP.SHIPMENT_ID             AS SHIPMENT_ID,
                   OP.PRICE_SEASON_CODE        AS PRICE_SEASON_CODE,
                   OP.LAST_CARTON_STORAGE_AREA AS LAST_CARTON_STORAGE_AREA,
                   OP.LAST_LOCATION_ID         AS LAST_LOCATION_ID,
                   OP.UPC_CODE                 AS UPC_CODE,
                   OP.TOTAL_CARTON_QUANTITY    AS TOTAL_CARTON_QUANTITY,
                   OP.SEWING_PLANT_CODE        AS SEWING_PLANT_CODE,
                   OP.QUALITY_CODE             AS QUALITY_CODE,
                   OP.VWH_ID                   AS VWH_ID
              FROM SRC_OPEN_CARTON OP
             WHERE OP.CARTON_ID = :CARTON_ID
", row => new
 {
     ShipmentId = row.GetValue<string>("SHIPMENT_ID"),
     PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
     CartonStorageArea = row.GetValue<string>("LAST_CARTON_STORAGE_AREA"),
     LastLocationId = row.GetValue<string>("LAST_LOCATION_ID"),
     UpcCode = row.GetValue<string>("UPC_CODE"),
     Quantity = row.GetValue<int>("TOTAL_CARTON_QUANTITY"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     VwhId = row.GetValue<string>("VWH_ID")
 }).Parameter("carton_id", similarCarton.Carton_id)
    .ExecuteSingle(dbuser);
            Assert.AreEqual(similarCarton.ShipMentId, infoInOpenCarton.ShipmentId, "Shipment Id does not match");
            Assert.AreEqual(similarCarton.PriceSeasonCode, infoInOpenCarton.PriceSeasonCode, "PriceSeasonCode does not match");
            Assert.AreEqual(similarCarton.CartonStorageArea, infoInOpenCarton.CartonStorageArea, "CartonStorageArea does not match");
            Assert.AreEqual(similarCarton.LocationId, infoInOpenCarton.LastLocationId, "Location Id does not match");
            Assert.AreEqual(expectedBox.Upc_code, infoInOpenCarton.UpcCode, "UpcCode does not match");
            Assert.AreEqual(similarCarton.Quantity, infoInOpenCarton.Quantity, "carton Quantity does not match");
            Assert.AreEqual(similarCarton.SewingPlantCode, infoInOpenCarton.SewingPlantCode, "Sewing Plant code does not match");
            Assert.AreEqual(similarCarton.QualityCode, infoInOpenCarton.QualityCode, "QualityCode does not match");
            Assert.AreEqual(expectedBox.Vwh_id, infoInOpenCarton.VwhId, "Vwh Id does not match");


            //Check the Positive transaction in src_transaction family table.
            var positiveTrasaction = SqlBinder.Create(@"
            WITH TR AS
             (SELECT T.TRANSACTION_TYPE        AS TRANSACTION_TYPE,
                     T.STYLE                   AS STYLE,
                     T.COLOR                   AS COLOR,
                     T.DIMENSION               AS DIMENSION,
                     T.SKU_SIZE                AS SKU_SIZE,
                     T.BUNDLE_ID               AS BUNDLE_ID,
                     T.SEWING_PLANT_CODE       AS SEWING_PLANT_CODE,
                     T.VWH_ID                  AS VWH_ID,
                     T.QUALITY_CODE            AS QUALITY_CODE,
                     TD.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                     TD.TRANSACTION_PIECES     AS TRANSACTION_PIECES
                FROM SRC_TRANSACTION T
               INNER JOIN SRC_TRANSACTION_DETAIL TD
                  ON T.TRANSACTION_ID = TD.TRANSACTION_ID
               WHERE TD.TRANSACTION_PIECES &gt; 0
                 AND T.CARTON_ID = :carton_id
               ORDER BY T.INSERT_DATE DESC)
            SELECT * FROM TR WHERE ROWNUM = 1
", row => new
 {
     TransactionType = row.GetValue<string>("TRANSACTION_TYPE"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     SkuSize = row.GetValue<string>("SKU_SIZE"),
     BundleId = row.GetValue<string>("BUNDLE_ID"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     VwhId = row.GetValue<string>("VWH_ID"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     InventoryStorageArea = row.GetValue<string>("INVENTORY_STORAGE_AREA"),
     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES")
 }).Parameter("Carton_id", similarCarton.Carton_id)
 .ExecuteSingle(dbuser);

            Assert.AreEqual("BMOV", positiveTrasaction.TransactionType, "Transaction type should be BMOV");
            Assert.AreEqual(similarCarton.Style, positiveTrasaction.Style, "Style Does not match");
            Assert.AreEqual(similarCarton.Color, positiveTrasaction.Color, "Color Does not match");
            Assert.AreEqual(similarCarton.Dimension, positiveTrasaction.Dimension, "Dimension Does not match");
            Assert.AreEqual(similarCarton.SkuSize, positiveTrasaction.SkuSize, "SkuSize Does not match");
            Assert.AreEqual(similarCarton.BundleId, positiveTrasaction.BundleId, "Bundle Id Does not match");
            Assert.AreEqual(similarCarton.SewingPlantCode, positiveTrasaction.SewingPlantCode, "Sewing Plant does not match");
            Assert.AreEqual(expectedBox.Vwh_id, positiveTrasaction.VwhId, "Vwh Id does not match");
            Assert.AreEqual(similarCarton.QualityCode, positiveTrasaction.QualityCode, "Quality code does not match");
            Assert.AreEqual("SHL", positiveTrasaction.InventoryStorageArea, "Inventory storage area should be SHL");
            Assert.AreEqual(similarCarton.Quantity, positiveTrasaction.TransactionPieces, "Transaction pieces should be same as carton pieces");

            //Check the Negative transaction in src_transaction family table.
            var negativeTrasaction = SqlBinder.Create(@"
            WITH TR AS
             (SELECT T.TRANSACTION_TYPE        AS TRANSACTION_TYPE,
                     T.STYLE                   AS STYLE,
                     T.COLOR                   AS COLOR,
                     T.DIMENSION               AS DIMENSION,
                     T.SKU_SIZE                AS SKU_SIZE,
                     T.BUNDLE_ID               AS BUNDLE_ID,
                     T.SEWING_PLANT_CODE       AS SEWING_PLANT_CODE,
                     T.VWH_ID                  AS VWH_ID,
                     T.QUALITY_CODE            AS QUALITY_CODE,
                     TD.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                     TD.TRANSACTION_PIECES     AS TRANSACTION_PIECES
                FROM SRC_TRANSACTION T
               INNER JOIN SRC_TRANSACTION_DETAIL TD
                  ON T.TRANSACTION_ID = TD.TRANSACTION_ID
               WHERE TD.TRANSACTION_PIECES &lt; 0
                 AND T.CARTON_ID = :carton_id
               ORDER BY T.INSERT_DATE DESC)
            SELECT * FROM TR WHERE ROWNUM = 1
", row => new
 {
     TransactionType = row.GetValue<string>("TRANSACTION_TYPE"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     SkuSize = row.GetValue<string>("SKU_SIZE"),
     BundleId = row.GetValue<string>("BUNDLE_ID"),
     SewingPlantCode = row.GetValue<string>("SEWING_PLANT_CODE"),
     VwhId = row.GetValue<string>("VWH_ID"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     InventoryStorageArea = row.GetValue<string>("INVENTORY_STORAGE_AREA"),
     TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES")
 }).Parameter("Carton_id", similarCarton.Carton_id)
 .ExecuteSingle(dbuser);

            Assert.AreEqual("BMOV", negativeTrasaction.TransactionType, "Transaction type should be BMOV");
            Assert.AreEqual(similarCarton.Style, negativeTrasaction.Style, "Style Does not match");
            Assert.AreEqual(similarCarton.Color, negativeTrasaction.Color, "Color Does not match");
            Assert.AreEqual(similarCarton.Dimension, negativeTrasaction.Dimension, "Dimension Does not match");
            Assert.AreEqual(similarCarton.SkuSize, negativeTrasaction.SkuSize, "SkuSize Does not match");
            Assert.AreEqual(similarCarton.BundleId, negativeTrasaction.BundleId, "Bundle Id Does not match");
            Assert.AreEqual(similarCarton.SewingPlantCode, negativeTrasaction.SewingPlantCode, "Sewing Plant does not match");
            Assert.AreEqual(expectedBox.Vwh_id, negativeTrasaction.VwhId, "Vwh Id does not match");
            Assert.AreEqual(similarCarton.QualityCode, negativeTrasaction.QualityCode, "Quality code does not match");
            Assert.AreEqual("BIR", negativeTrasaction.InventoryStorageArea, "Inventory storage area should be SHL");
            Assert.IsTrue(similarCarton.Quantity + negativeTrasaction.TransactionPieces == 0, "Transaction pieces should be same as carton pieces");
        }

        /// <summary>
        /// Valid Test:Test for removing present box from the pallet.
        /// Call "RemoveBoxFromPallet" function of repository.
        /// This function takes valid ucc128_id and pallet_id. 
        /// Test:
        ///Validating, Pallet_id should be null in box table against passed ucc128_id and pallet_id.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void RemoveBoxFromPallet()
        {
            var presentBox = SqlBinder.Create(@"
                WITH Q1 AS
                 (SELECT B.UCC128_ID AS UCC128_ID,
                         B.PALLET_ID AS PALLET_ID
                    FROM BOX B
                   INNER JOIN PS P
                      ON B.PICKSLIP_ID = P.PICKSLIP_ID
                   INNER JOIN BUCKET BK
                      ON P.BUCKET_ID = BK.BUCKET_ID
                   INNER JOIN SRC_CARTON SC
                      ON B.CARTON_ID = SC.CARTON_ID
                   INNER JOIN SRC_CARTON_DETAIL SCD
                      ON SC.CARTON_ID = SCD.CARTON_ID
                   WHERE B.PALLET_ID IS NOT NULL
                     AND B.IA_ID IS NULL
                     AND BK.PICK_MODE = 'ADREPPWSS'
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
 {
     Ucc128Id = row.GetValue<string>("UCC128_ID"),
     PalletId = row.GetValue<string>("PALLET_ID")
 }).ExecuteSingle(dbuser);

            if (presentBox == null)
            {
                Assert.Inconclusive("No data found");
            }
            _target.RemoveBoxFromPallet(presentBox.Ucc128Id, presentBox.PalletId);

            //Assert pallet_id will be null after romoving box from pallet.
            var actualBox = SqlBinder.Create(@"
            SELECT B.PALLET_ID AS PALLET_ID 
            FROM BOX B 
            WHERE B.UCC128_ID= :UCC128_ID
", row => new
 {
     PalletId = row.GetValue<string>("PALLET_ID")
 }).Parameter("UCC128_ID", presentBox.Ucc128Id)
 .ExecuteSingle(dbuser);

            Assert.IsTrue(string.IsNullOrEmpty(actualBox.PalletId), "palletId must be null");
        }

        /// <summary>
        /// Valid Test:Test for removing remaining boxes from the pallet.
        /// Call "RemoveRemainingBoxesFromPallet" function of repository.
        /// This function takes valid pallet_id.
        /// In box table for those carton whose ia_id is null,the pallet_id should be null.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void RemoveRemainingBoxesFromPallet()
        {
            var presentBox = SqlBinder.Create(@"
                WITH Q1 AS
                 (SELECT B.PALLET_ID AS PALLET_ID,
                         B.CARTON_ID AS CARTON_ID
                    FROM BOX B
                   INNER JOIN PS P
                      ON B.PICKSLIP_ID = P.PICKSLIP_ID
                   INNER JOIN BUCKET BK
                      ON P.BUCKET_ID = BK.BUCKET_ID
                   INNER JOIN SRC_CARTON SC
                      ON B.CARTON_ID = SC.CARTON_ID
                   INNER JOIN SRC_CARTON_DETAIL SCD
                      ON SC.CARTON_ID = SCD.CARTON_ID
                   WHERE B.PALLET_ID IS NOT NULL
                     AND B.IA_ID IS NULL
                     AND BK.PICK_MODE = 'ADREPPWSS'
                   ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
 {
     CartonId = row.GetValue<string>("CARTON_ID"),
     PalletId = row.GetValue<string>("PALLET_ID")
 }).ExecuteSingle(dbuser);

            if (presentBox == null)
            {
                Assert.Inconclusive("No data found");
            }

            _target.RemoveRemainingBoxesFromPallet(presentBox.PalletId);

            //Assert pallet_id will be null after romoving box from pallet.
            var actual = SqlBinder.Create(@"
select b.pallet_id as pallet_id
from box b
where b.carton_id= :CARTON_ID
", row => new
  {
      PalletId = row.GetValue<string>("pallet_id")
  }).Parameter("CARTON_ID", presentBox.CartonId)
  .ExecuteSingle(dbuser);
            Assert.IsTrue(string.IsNullOrEmpty(actual.PalletId), "Pallet Id must be null");
        }

        /// <summary>
        /// This function is checking wheather we are getting right number of suggested locations against given carton Id or not..
        /// This test fetch a random cartonId and carton detalis from src_carton table and src_carton_details.
        /// After that this test is calling repository function Getlocation() and pass fetched carton Id as parameter,
        /// then we are querying src_carton to get number of suggested locations of cartons that have same sku,quantity,quality,
        /// virtual_warehouse,warehouse_location and storage area as fetched carton.
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Rajesh")]
        public void GetSkuLocation()
        {
//            //fetching a random cartonId from src_carton
//            var cartonId = SqlBinder.Create(@"
//                WITH Q AS
//                    (SELECT CTN.CARTON_ID AS CARTON_ID
//                    FROM SRC_CARTON CTN
//                    ORDER BY DBMS_RANDOM.VALUE)
//                SELECT * FROM Q WHERE ROWNUM &lt; 2
//                ", row => new
//                    {
//                        CartonId = row.GetValue<string>("CARTON_ID")
//                    }).ExecuteSingle(_target.Db);

            //querying the details against fetched cartonId.
            var cartonDetail = SqlBinder.Create(@"
            WITH Q AS
                   (
                    SELECT CTN.CARTON_ID       AS CARTON_ID,  
                       CTN.LOCATION_ID         AS LOCATION_ID,
                       CTN.QUALITY_CODE        AS QUALITY_CODE,
                       CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                       CTN.VWH_ID              AS VWH_ID,
                       CTNDET.QUANTITY         AS QUANTITY,
                       CTNDET.SKU_ID           AS SKU_ID,
                       MSL.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID
                  FROM SRC_CARTON CTN
                 INNER JOIN SRC_CARTON_DETAIL CTNDET
                    ON CTN.CARTON_ID = CTNDET.CARTON_ID
                    INNER JOIN MASTER_STORAGE_LOCATION MSL 
                    ON MSL.STORAGE_AREA = CTN.CARTON_STORAGE_AREA
                    AND MSL.LOCATION_ID = CTN.LOCATION_ID)
                    SELECT * FROM Q WHERE ROWNUM &lt; 2
            ", row => new
             {
                 CartonId = row.GetValue<string>("CARTON_ID"),
                 LocationId = row.GetValue<string>("LOCATION_ID"),
                 QualityCode = row.GetValue<string>("QUALITY_CODE"),
                 VwhId = row.GetValue<string>("VWH_ID"),
                 Quantity = row.GetValue<int>("QUANTITY"),
                 SkuID = row.GetValue<int>("SKU_ID"),
                 CartonStorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 WhId = row.GetValue<string>("WAREHOUSE_LOCATION_ID")
             }).ExecuteSingle(dbuser);
 

            if (cartonDetail == null)
            {
                Assert.Inconclusive("No data found");
            }

            //calling repositories function
           var actualLocation =  _target.GetAlternateLocations(cartonDetail.CartonId);


            //Querying number of suggested locations against fetched carton details.
           var expectedLocation = SqlBinder.Create(@"
            SELECT COUNT(CTN.LOCATION_ID) AS LocationCount
              FROM SRC_CARTON CTN
                   INNER JOIN SRC_CARTON_DETAIL CTNDET
                ON CTN.CARTON_ID = CTNDET.CARTON_ID 
               inner join MASTER_STORAGE_LOCATION MSL
            on MSL.LOCATION_ID = CTN.LOCATION_ID
               AND MSL.STORAGE_AREA = CTN.CARTON_STORAGE_AREA
             WHERE CTN.QUALITY_CODE = :QUALITY_CODE
               and CTN.LOCATION_ID &lt; &gt; :LOCATION_ID
               AND CTN.VWH_ID = :VWH_ID
               AND CTNDET.QUANTITY = :QUANTITY
               AND CTNDET.SKU_ID = :SKU_ID
               AND CTN.CARTON_STORAGE_AREA = :CARTON_STORAGE_AREA
               AND MSL.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID
            ", row => new 
            {
                CountLocationId = row.GetValue<int>("LocationCount")
            }).Parameter("QUALITY_CODE", cartonDetail.QualityCode)
            .Parameter("VWH_ID", cartonDetail.VwhId)
            .Parameter("QUANTITY", cartonDetail.Quantity)
            .Parameter("SKU_ID", cartonDetail.SkuID)
            .Parameter("CARTON_STORAGE_AREA", cartonDetail.CartonStorageArea)
            .Parameter("WAREHOUSE_LOCATION_ID", cartonDetail.WhId)
            .Parameter("LOCATION_ID", cartonDetail.LocationId)
            .ExecuteSingle(dbuser);

            //assert
           Assert.AreEqual(expectedLocation.CountLocationId, actualLocation.Count(), "Number of locations doesnot matched with desired");
        }



        //Invalid Test Cases:


        /// <summary>
        /// Invalid Test :
        /// Passed invalid ucc128_id, Valid Carton_Id and valid CartonPickStartDate to the function "PickCarton" of repository.
        /// It should be generate an Oracle Exception "no data found".
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void PickCartonWithInvalidUccId()
        {
            var invalidBox = SqlBinder.Create(@"
            WITH Q1 AS
             (SELECT B.CARTON_ID AS CARTON_ID
                FROM BOX B
               INNER JOIN BOXDET BD
                  ON B.UCC128_ID = BD.UCC128_ID
                 AND B.PICKSLIP_ID = BD.PICKSLIP_ID
               INNER JOIN PS P
                  ON B.PICKSLIP_ID = P.PICKSLIP_ID
               INNER JOIN BUCKET BK
                  ON P.BUCKET_ID = BK.BUCKET_ID
               INNER JOIN SRC_CARTON SC
                  ON B.CARTON_ID = SC.CARTON_ID
               INNER JOIN SRC_CARTON_DETAIL SCD
                  ON SC.CARTON_ID = SCD.CARTON_ID
               WHERE B.PALLET_ID IS NOT NULL
                 AND B.IA_ID IS NULL
                 AND BK.PICK_MODE = 'ADREPPWSS'
               ORDER BY DBMS_RANDOM.value)
            SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
 {
     CartonId = row.GetValue<string>("CARTON_ID")
 }).ExecuteSingle(dbuser);

            _target.PickCarton("24353534", invalidBox.CartonId, DateTime.Now);
        }

        /// <summary>
        /// Invalid Test Case:
        /// Passed null Ucc128_id,valid Carton_id and valid CartonPickStartDate to the function "pickcarton" of repository.
        /// It should be generate an "ArgumentNullException"(value can not be null) exception.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PickCartonWithNullUccId()
        {
            var invalidBox = SqlBinder.Create(@"
            WITH Q1 AS
             (SELECT B.CARTON_ID AS CARTON_ID
                FROM BOX B
               INNER JOIN BOXDET BD
                  ON B.UCC128_ID = BD.UCC128_ID
                 AND B.PICKSLIP_ID = BD.PICKSLIP_ID
               INNER JOIN PS P
                  ON B.PICKSLIP_ID = P.PICKSLIP_ID
               INNER JOIN BUCKET BK
                  ON P.BUCKET_ID = BK.BUCKET_ID
               INNER JOIN SRC_CARTON SC
                  ON B.CARTON_ID = SC.CARTON_ID
               INNER JOIN SRC_CARTON_DETAIL SCD
                  ON SC.CARTON_ID = SCD.CARTON_ID
               WHERE B.PALLET_ID IS NOT NULL
                 AND B.IA_ID IS NULL
                 AND BK.PICK_MODE = 'ADREPPWSS'
               ORDER BY DBMS_RANDOM.value)
            SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
  {
      CartonId = row.GetValue<string>("CARTON_ID")
  }).ExecuteSingle(dbuser);

            _target.PickCarton(null, invalidBox.CartonId, DateTime.Now);
        }


        /// <summary>
        /// Invalid Test Case:
        /// Passed a valid Ucc128_Id, invalid Carton_id and valid CartonPickStartDate to the function "PickCarton" of repository.
        /// Its should be generate an Oracle exception "Carton: 'carton_id' does not match with Box: 'ucc_id'. Make sure that you scanned valid carton and Box".
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void PickCartonWithInvalidCartonId()
        {
            var invalidBox = SqlBinder.Create(@"
            WITH Q1 AS
             (SELECT B.UCC128_ID AS UCC128_ID
                FROM BOX B
               INNER JOIN BOXDET BD
                  ON B.UCC128_ID = BD.UCC128_ID
                 AND B.PICKSLIP_ID = BD.PICKSLIP_ID
               INNER JOIN PS P
                  ON B.PICKSLIP_ID = P.PICKSLIP_ID
               INNER JOIN BUCKET BK
                  ON P.BUCKET_ID = BK.BUCKET_ID
               INNER JOIN SRC_CARTON SC
                  ON B.CARTON_ID = SC.CARTON_ID
               INNER JOIN SRC_CARTON_DETAIL SCD
                  ON SC.CARTON_ID = SCD.CARTON_ID
               WHERE B.PALLET_ID IS NOT NULL
                 AND B.IA_ID IS NULL
                 AND BK.PICK_MODE = 'ADREPPWSS'
               ORDER BY DBMS_RANDOM.value)
            SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
 {
     Ucc128Id = row.GetValue<string>("UCC128_ID")
 }).ExecuteSingle(dbuser);

            _target.PickCarton(invalidBox.Ucc128Id, "Carton_id", DateTime.Now);
        }

        /// <summary>
        /// Invalid Test Case:
        /// Passed a valid Ucc128_Id, null Carton_id and valid CartonPickStartDate to the function "PickCarton" of repository.
        /// It should be generate an exception "ArgumentNullException"(value can not be null).
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PickCartonWithNullCartonId()
        {
            var invalidBox = SqlBinder.Create(@"
            WITH Q1 AS
             (SELECT B.UCC128_ID AS UCC128_ID
                FROM BOX B
               INNER JOIN BOXDET BD
                  ON B.UCC128_ID = BD.UCC128_ID
                 AND B.PICKSLIP_ID = BD.PICKSLIP_ID
               INNER JOIN PS P
                  ON B.PICKSLIP_ID = P.PICKSLIP_ID
               INNER JOIN BUCKET BK
                  ON P.BUCKET_ID = BK.BUCKET_ID
               INNER JOIN SRC_CARTON SC
                  ON B.CARTON_ID = SC.CARTON_ID
               INNER JOIN SRC_CARTON_DETAIL SCD
                  ON SC.CARTON_ID = SCD.CARTON_ID
               WHERE B.PALLET_ID IS NOT NULL
                 AND B.IA_ID IS NULL
                 AND BK.PICK_MODE = 'ADREPPWSS'
               ORDER BY DBMS_RANDOM.value)
            SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
 {
     Ucc128Id = row.GetValue<string>("UCC128_ID")
 }).ExecuteSingle(dbuser);

            _target.PickCarton(invalidBox.Ucc128Id, null, DateTime.Now);
        }


        /// <summary>
        /// Inavalid Test Case:
        /// Passed null date as cartonPickStartDate to the function "PickCarton" of repository.
        /// It should be generate an exception.
        /// </summary>
        //        [TestMethod]
        //        [TestCategory("Database")]
        //        [Owner("Rajesh")]
        //        public void PickCartonWithNullDate()
        //        {
        //            var invalidBox = SqlBinder.Create(@"
        //            WITH Q1 AS
        //             (SELECT B.UCC128_ID AS UCC128_ID, 
        //                     B.CARTON_ID AS CARTON_ID
        //                FROM BOX B
        //               INNER JOIN BOXDET BD
        //                  ON B.UCC128_ID = BD.UCC128_ID
        //                 AND B.PICKSLIP_ID = BD.PICKSLIP_ID
        //               INNER JOIN PS P
        //                  ON B.PICKSLIP_ID = P.PICKSLIP_ID
        //               INNER JOIN BUCKET BK
        //                  ON P.BUCKET_ID = BK.BUCKET_ID
        //               INNER JOIN SRC_CARTON SC
        //                  ON B.CARTON_ID = SC.CARTON_ID
        //               INNER JOIN SRC_CARTON_DETAIL SCD
        //                  ON SC.CARTON_ID = SCD.CARTON_ID
        //               WHERE B.PALLET_ID IS NOT NULL
        //                 AND B.IA_ID IS NULL
        //                 AND BK.PICK_MODE = 'ADREPPWSS'
        //               ORDER BY DBMS_RANDOM.value)
        //            SELECT * FROM Q1 WHERE ROWNUM = 1
        //
        //", row => new
        // {
        //     Ucc128Id = row.GetValue<string>("UCC128_ID"),
        //     CartonId = row.GetValue<string>("CARTON_ID")
        // }).ExecuteSingle(_target.Db);

        //            _target.PickCarton(invalidBox.Ucc128Id, invalidBox.CartonId,);
        //}


        /// <summary>
        /// Invalid Test:
        /// Pick a carton which is already picked.
        /// Call "PickCarton" function of repository.
        /// It should be generate an Oracle exception "Carton:'carton_id' does not match with Box:'ucc_id'. Make sure that you scanned valid carton and Box".
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void PickAlreadyPickedCarton()
        {
            var expectedCarton = SqlBinder.Create(@"
            WITH Q1 AS
             (SELECT B.CARTON_ID AS CARTON_ID,
                     B.UCC128_ID AS UCC128_ID
                FROM BOX B
               INNER JOIN PS P
                  ON B.PICKSLIP_ID = P.PICKSLIP_ID
               INNER JOIN BUCKET BK
                  ON P.BUCKET_ID = BK.BUCKET_ID
               WHERE B.PALLET_ID IS NOT NULL
                 AND B.IA_ID IS NOT NULL
                 AND BK.PICK_MODE = 'ADREPPWSS'
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
 {
     UccId = row.GetValue<string>("UCC128_ID"),
     CartonId = row.GetValue<string>("CARTON_ID")
 }).ExecuteSingle(dbuser);

            _target.PickCarton(expectedCarton.UccId, expectedCarton.CartonId, DateTime.Now);
        }



        /// <summary>
        /// Pick any carton instead of suggested carton and it should not be simmilar to suggested carton
        ///  Call "PickCarton()" function  of Repository to pick any carton.
        ///  It should be generate an Oracle exception "Carton:'carton_id' does not match with Box:'ucc_id'. Make sure that you scanned valid carton and Box"..
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void PickSimilarCartonwithDifferentQuantity()
        {
            //select a random box
            var expectedBox = SqlBinder.Create(@"
                                     WITH Q1 AS
             (SELECT B.UCC128_ID  AS UCC128_ID,
                     B.PALLET_ID  AS PALLET_ID,
                     B.CARTON_ID  AS CARTON_ID,
                     BD.UPC_CODE  AS UPC_CODE,
                     B.VWH_ID     AS VWH_ID,
                     BK.ship_ia_id as ship_ia_id,
                     BK.pull_carton_area as pull_carton_area,
                     SCD.QUANTITY AS QUANTITY
                FROM BOX B
               INNER JOIN BOXDET BD
                  ON B.UCC128_ID = BD.UCC128_ID
                 AND B.PICKSLIP_ID = BD.PICKSLIP_ID
               INNER JOIN PS P
                  ON B.PICKSLIP_ID = P.PICKSLIP_ID
               INNER JOIN BUCKET BK
                  ON P.BUCKET_ID = BK.BUCKET_ID
               INNER JOIN SRC_CARTON SC
                  ON B.CARTON_ID = SC.CARTON_ID
               INNER JOIN SRC_CARTON_DETAIL SCD
                  ON SC.CARTON_ID = SCD.CARTON_ID
               WHERE B.PALLET_ID IS NOT NULL
                 AND B.IA_ID IS NULL
                 AND BK.PICK_MODE = 'ADREPPWSS'
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
 {
     UccId = row.GetValue<string>("UCC128_ID"),
     Carton_id = row.GetValue<string>("CARTON_ID"),
     Pallet_id = row.GetValue<string>("PALLET_ID"),
     Upc_code = row.GetValue<string>("UPC_CODE"),
     Vwh_id = row.GetValue<string>("VWH_ID"),
     ShipIaId = row.GetValue<string>("ship_ia_id"),
     PullCartonArea = row.GetValue<string>("pull_carton_area"),
     Pieces = row.GetValue<int>("QUANTITY")
 }).ExecuteSingle(dbuser);

            if (expectedBox == null)
            {
                Assert.Inconclusive("No box found");
            }

            //select a similar carton(with different quantity) from src_carton
            var differentCarton = SqlBinder.Create(@"
            WITH Q1 AS
             (
                SELECT C.CARTON_ID        AS CARTON_ID,
                    C.SHIPMENT_ID         AS SHIPMENT_ID,
                    C.PRICE_SEASON_CODE   AS PRICE_SEASON_CODE,
                    C.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                    C.QUALITY_CODE        AS QUALITY_CODE,
                    C.VWH_ID              AS VWH_ID,
                    CD.STYLE              AS STYLE,
                    CD.COLOR              AS COLOR,
                    CD.DIMENSION          AS DIMENSION,
                    CD.SKU_SIZE           AS SKU_SIZE,
                    CD.QUANTITY           AS QUANTITY,
                    CD.REQ_PROCESS_ID     AS REQ_PROCESS_ID,
                    CD.REQ_MODULE_CODE    AS REQ_MODULE_CODE,
                    CD.REQ_LINE_NUMBER    AS REQ_LINE_NUMBER
                FROM SRC_CARTON C
               INNER JOIN SRC_CARTON_DETAIL CD
                  ON C.CARTON_ID = CD.CARTON_ID
               INNER JOIN MASTER_SKU MS
                  ON CD.SKU_ID = MS.SKU_ID
                 AND C.CARTON_STORAGE_AREA = :pull_carton_area
                 AND C.CARTON_ID &lt;&gt; :CARTON_ID
                 AND CD.QUANTITY &lt;&gt; :QUANTITY
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q1 WHERE ROWNUM = 1
", row => new
 {
     ShipMentId = row.GetValue<string>("SHIPMENT_ID"),
     PriceSeasonCode = row.GetValue<string>("PRICE_SEASON_CODE"),
     CartonStorageArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
     QualityCode = row.GetValue<string>("QUALITY_CODE"),
     VwhId = row.GetValue<string>("VWH_ID"),
     Quantity = row.GetValue<int>("QUANTITY"),
     Carton_id = row.GetValue<string>("CARTON_ID"),
     Process_id = row.GetValue<int?>("REQ_PROCESS_ID"),
     Module_code = row.GetValue<string>("REQ_MODULE_CODE"),
     Line_number = row.GetValue<int?>("REQ_LINE_NUMBER"),
     Style = row.GetValue<string>("STYLE"),
     Color = row.GetValue<string>("COLOR"),
     Dimension = row.GetValue<string>("DIMENSION"),
     SkuSize = row.GetValue<string>("SKU_SIZE")
 }).Parameter("pull_carton_area", expectedBox.PullCartonArea)
 .Parameter("CARTON_ID", expectedBox.Carton_id)
 .Parameter("QUANTITY", expectedBox.Pieces)
               .ExecuteSingle(dbuser);
            if (differentCarton == null)
            {
                Assert.Inconclusive("No carton found");
            }

            _target.PickCarton(expectedBox.UccId, differentCarton.Carton_id, DateTime.Now);
        }
    }
}
