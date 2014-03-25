using System;
using System.Web;
using System.Linq;
using System.Data.Common;
using DcmsMobile.PalletLocating.Models;
using EclipseLibrary.Oracle;
using System.Text;
using System.Collections.Generic;
using DcmsMobile.PalletLocating.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace PalletLocating.test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
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

        private static OracleDatastore _db;

        //You can use the following additional attributes as you write your tests:

        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            _db = new OracleDatastore(null);
            _db.CreateConnection("Data Source=w8singapore/mfdev;Proxy User Id=dcms8;Proxy Password=dcms8", "");
        }


        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            _db.Dispose();
        }
        private PalletLocatingRepository target;
        private DbTransaction _trans;

        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            target = new PalletLocatingRepository(_db);
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
        /// This function is validating repositories function GetCartonAreas() this accept
        /// area Id , building Id and short name as parameters.
        /// In this function first we fetch number of carton areas that are numbered and where pallet is required, 
        /// and then call repositories function GetCartonAreas() with providing null as all parameters. 
        /// Then we asserted the number of rows returned by our query is same as returned by repositories function.
        /// </summary>
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        public void GetCartonAreasTest()
        {
            //fetching all the numbered carton areas where pallet is required 
            var expectedCartonAreas = SqlBinder.Create(
                        @"
                        <![CDATA[
                        SELECT COUNT(*) AS NUMBER_OF_AREAS
                          FROM TAB_INVENTORY_AREA TIA
                         WHERE TIA.STORES_WHAT = 'CTN'                          
                           AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                        ]]>
                ",
                 row => new
                 {
                     NumberOfAreas = row.GetValue<int>("NUMBER_OF_AREAS")
                 }).ExecuteSingle(_db);


            //calling repositories function
            var actualArea = target.GetCartonAreas(null, null, null);

            //asserting
            Assert.AreEqual(expectedCartonAreas.NumberOfAreas, actualArea.Count(), "Number Of Carton Area Mismatch");
        }

        /// <summary>
        /// This function is validating repositories function GetCartonAreas() this accept
        /// area Id , building Id and short name as parameters.
        /// In this function first we fetch a numbered carton areas where pallet is required and have a replenishment area, 
        /// and then call repositories function GetCartonAreas() with providing fetched carton area as first parameter and null as other parameters. 
        /// Then we asserted that the information returned by our query is same as returned by repositories function.
        /// </summary>
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        public void GetCartonAreaWithaPassingAreaId()
        {
            //fetching a RANDOM numbered carton areas where pallet is required and have a replenishment area 
            var expectedCartonArea = SqlBinder.Create(
                @"
            <![CDATA[
            WITH Q AS
             (SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                     TIA.WAREHOUSE_LOCATION_ID  AS WAREHOUSE_LOCATION_ID,
                     TIA.SHORT_NAME             AS SHORT_NAME,
                     TIA.DESCRIPTION            AS DESCRIPTION,
                     TIA.REPLENISHMENT_AREA_ID  AS REPLENISHMENT_AREA_ID,
                     TIA2.SHORT_NAME            AS REPLENISHMENT_AREA_SHORT_NAME
                FROM TAB_INVENTORY_AREA TIA
                LEFT OUTER JOIN TAB_INVENTORY_AREA TIA2
                  ON TIA2.INVENTORY_STORAGE_AREA = TIA.REPLENISHMENT_AREA_ID
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
                {
                    Area = row.GetValue<string>("INVENTORY_STORAGE_AREA"),
                    WareHouse = row.GetValue<string>("WAREHOUSE_LOCATION_ID"),
                    ShortName = row.GetValue<string>("SHORT_NAME"),
                    Description = row.GetValue<string>("DESCRIPTION"),
                    ReplinishmentArea = row.GetValue<string>("REPLENISHMENT_AREA_ID"),
                    ReplinishmentAreaShortName = row.GetValue<string>("REPLENISHMENT_AREA_SHORT_NAME")
                }).ExecuteSingle(_db);


            //calling repositories function
            var actualCartonArea = target.GetCartonAreas(expectedCartonArea.Area, null, null);

            //asserting
            foreach (var item in actualCartonArea)
            {
                Assert.AreEqual(expectedCartonArea.Area, item.AreaId, "Area Id");
                Assert.AreEqual(expectedCartonArea.Description, item.Description, "Description");
                Assert.AreEqual(expectedCartonArea.WareHouse, item.BuildingId, "Area Id");
                Assert.AreEqual(expectedCartonArea.ReplinishmentArea, item.ReplenishAreaId, "Replenish Area Id");
                Assert.AreEqual(expectedCartonArea.ReplinishmentAreaShortName, item.ReplenishAreaShortName, "Replenish Area Short Name");
                Assert.AreEqual(expectedCartonArea.ShortName, item.ShortName, "Short Name");
            }


        }


        /// <summary>
        /// This function is validating repositories function GetCartonAreas() this accept
        /// area Id , building Id and short name as parameters.
        /// In this function first we fetch a numbered carton areas where pallet is required and have a replenishment area, 
        /// and then call repositories function GetCartonAreas() with providing fetched carton area as first parameter,its building Id as second parameter and null as short name. 
        /// Then we asserted that the information returned by our query is same as returned by repositories function.
        /// </summary>     
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        public void GetCartonAreaWithaPassingWhId()
        {
             //fetching a numbered carton areas where pallet is required and have a replenishment area
            var expectedCartonArea = SqlBinder.Create(
                @"
            <![CDATA[
            WITH Q AS
             (SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                     TIA.WAREHOUSE_LOCATION_ID  AS WAREHOUSE_LOCATION_ID,
                     TIA.SHORT_NAME             AS SHORT_NAME,
                     TIA.DESCRIPTION            AS DESCRIPTION,
                     TIA.REPLENISHMENT_AREA_ID  AS REPLENISHMENT_AREA_ID,
                     TIA2.SHORT_NAME            AS REPLENISHMENT_AREA_SHORT_NAME
                FROM TAB_INVENTORY_AREA TIA
                LEFT OUTER JOIN TAB_INVENTORY_AREA TIA2
                  ON TIA2.INVENTORY_STORAGE_AREA = TIA.REPLENISHMENT_AREA_ID
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 Area = row.GetValue<string>("INVENTORY_STORAGE_AREA"),
                 WareHouse = row.GetValue<string>("WAREHOUSE_LOCATION_ID"),
                 ShortName = row.GetValue<string>("SHORT_NAME"),
                 Description = row.GetValue<string>("DESCRIPTION"),
                 ReplinishmentArea = row.GetValue<string>("REPLENISHMENT_AREA_ID"),
                 ReplinishmentAreaShortName = row.GetValue<string>("REPLENISHMENT_AREA_SHORT_NAME")
             }).ExecuteSingle(_db);

            //calling repositories function
            var actualCartonArea = target.GetCartonAreas(expectedCartonArea.Area, expectedCartonArea.WareHouse,null);

            //asserting
            foreach (var item in actualCartonArea)
            {
                Assert.AreEqual(expectedCartonArea.Area, item.AreaId, "Area Id");
                Assert.AreEqual(expectedCartonArea.Description, item.Description, "Description");
                Assert.AreEqual(expectedCartonArea.WareHouse, item.BuildingId, "Area Id");
                Assert.AreEqual(expectedCartonArea.ReplinishmentArea, item.ReplenishAreaId, "Replenish Area Id");
                Assert.AreEqual(expectedCartonArea.ReplinishmentAreaShortName, item.ReplenishAreaShortName, "Replenish Area Short Name");
                Assert.AreEqual(expectedCartonArea.ShortName, item.ShortName, "Short Name");
            }


        }


        /// <summary>
        /// This function is validating repositories function GetCartonAreas() this accept
        /// area Id , building Id and short name as parameters.
        /// In this function first we fetch a numbered carton areas where pallet is required and have a replenishment area, 
        /// and then call repositories function GetCartonAreas() with providing null as carton area Id,null as building Id and fetched  short name as short name. 
        /// Then we asserted that the information returned by our query is same as returned by repositories function.
        /// </summary>     
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        public void GetCartonAreaWithPassingShortName()
        {
            //fetching a numbered carton areas where pallet is required and have a replenishment area
            var expectedCartonArea = SqlBinder.Create(
                @"
            <![CDATA[
            WITH Q AS
             (SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                     TIA.WAREHOUSE_LOCATION_ID  AS WAREHOUSE_LOCATION_ID,
                     TIA.SHORT_NAME             AS SHORT_NAME,
                     TIA.DESCRIPTION            AS DESCRIPTION,
                     TIA.REPLENISHMENT_AREA_ID  AS REPLENISHMENT_AREA_ID,
                     TIA2.SHORT_NAME            AS REPLENISHMENT_AREA_SHORT_NAME
                FROM TAB_INVENTORY_AREA TIA
                LEFT OUTER JOIN TAB_INVENTORY_AREA TIA2
                  ON TIA2.INVENTORY_STORAGE_AREA = TIA.REPLENISHMENT_AREA_ID
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 Area = row.GetValue<string>("INVENTORY_STORAGE_AREA"),
                 WareHouse = row.GetValue<string>("WAREHOUSE_LOCATION_ID"),
                 ShortName = row.GetValue<string>("SHORT_NAME"),
                 Description = row.GetValue<string>("DESCRIPTION"),
                 ReplinishmentArea = row.GetValue<string>("REPLENISHMENT_AREA_ID"),
                 ReplinishmentAreaShortName = row.GetValue<string>("REPLENISHMENT_AREA_SHORT_NAME")
             }).ExecuteSingle(_db);

            //calling repositories function
            var actualCartonArea = target.GetCartonAreas(null,null,expectedCartonArea.ShortName);

            foreach (var item in actualCartonArea)
            {
                Assert.AreEqual(expectedCartonArea.Area, item.AreaId, "Area Id");
                Assert.AreEqual(expectedCartonArea.Description, item.Description, "Description");
                Assert.AreEqual(expectedCartonArea.WareHouse, item.BuildingId, "Area Id");
                Assert.AreEqual(expectedCartonArea.ReplinishmentArea, item.ReplenishAreaId, "Replenish Area Id");
                Assert.AreEqual(expectedCartonArea.ReplinishmentAreaShortName, item.ReplenishAreaShortName, "Replenish Area Short Name");
                Assert.AreEqual(expectedCartonArea.ShortName, item.ShortName, "Short Name");
            }


        }

        /// <summary>
        /// This function is validating repositories function GetPallet() this accept
        /// pallet Id and carton Id as parameters.
        /// In this function first we fetch a random pallet info from src_carton,src_carton_detail and master_sku table,
        /// and then call repositories function GetPallet() with providing fetched  Pallet Id as pallet Id and null as carton Id. 
        /// Then we asserted that the information returned by our query is same as returned by repositories function.
        /// </summary>
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        public void GetPalletTest()
        {
            // fetching a random pallet info 
            var expectedPalletInfo = SqlBinder.Create(
                @"
                       <![CDATA[
                        WITH Q AS
             (SELECT CTN.PALLET_ID                           AS PALLET_ID,
                     COUNT(CTN.CARTON_ID)                    AS NUMBER_OF_CARTONS,
                     COUNT(DISTINCT CTN.LOCATION_ID)         AS LOCATION_COUNT,
                     COUNT(DISTINCT CTN.CARTON_STORAGE_AREA) AS NUMBER_OF_CARTON_AREA,
                     COUNT(DISTINCT CTN.VWH_ID)              AS NUMBER_OF_VWH,
                     MIN(MSKU.STYLE)                         AS STYLE,
                     MIN(MSKU.COLOR)                         AS COLOR,
                     MIN(MSKU.DIMENSION)                     AS DIMENSION,
                     MIN(MSKU.SKU_SIZE)                      AS SKU_SIZE,
                     MIN(CTNDET.SKU_ID)                      AS SKU_ID
                FROM SRC_CARTON CTN
               INNER JOIN SRC_CARTON_DETAIL CTNDET
                  ON CTN.CARTON_ID = CTNDET.CARTON_ID
               INNER JOIN MASTER_SKU MSKU
                  ON MSKU.SKU_ID = CTNDET.SKU_ID
               GROUP BY CTN.PALLET_ID
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ", row => new
             {
                 PalletId = row.GetValue<string>("PALLET_ID"),
                 CartonCount = row.GetValue<int>("NUMBER_OF_CARTONS"),
                 LocationCount = row.GetValue<int>("LOCATION_COUNT"),
                 CartonAreaCount = row.GetValue<int>("NUMBER_OF_CARTON_AREA"),
                 VwhCount = row.GetValue<int>("NUMBER_OF_VWH"),
                 Style = row.GetValue<string>("STYLE"),
                 Color = row.GetValue<string>("COLOR"),
                 Dimension = row.GetValue<string>("DIMENSION"),
                 Size = row.GetValue<string>("SKU_SIZE"),
                 SkuId = row.GetValue<int>("SKU_ID")             
             }).ExecuteSingle(_db);

            //calling repositories function
            var actualPalletInfo = target.GetPallet(expectedPalletInfo.PalletId,null);

            //asserting
                Assert.AreEqual(expectedPalletInfo.CartonAreaCount, actualPalletInfo.AreaCount, "Area Count");
                Assert.AreEqual(expectedPalletInfo.CartonCount, actualPalletInfo.CartonCount, "Carton Count");
                Assert.AreEqual(expectedPalletInfo.Style, actualPalletInfo.PalletSku.Style, "Style"); 
                Assert.AreEqual(expectedPalletInfo.Color, actualPalletInfo.PalletSku.Color, "Color");
                Assert.AreEqual(expectedPalletInfo.Dimension, actualPalletInfo.PalletSku.Dimension, "Dimension");
                Assert.AreEqual(expectedPalletInfo.Size, actualPalletInfo.PalletSku.SkuSize, "SkuSize");
                Assert.AreEqual(expectedPalletInfo.LocationCount, actualPalletInfo.LocationCount, "Location Count");
                Assert.AreEqual(expectedPalletInfo.PalletId, actualPalletInfo.PalletId, "PalletId");
                Assert.AreEqual(expectedPalletInfo.SkuId, actualPalletInfo.PalletSku.SkuId, "Sku Id");
                Assert.AreEqual(expectedPalletInfo.VwhCount, actualPalletInfo.CartonVwhCount, "Carton Vwh Count");              

        }


        /// <summary>
        /// This function is validating repositories function GetPallet() this accept
        /// pallet Id and carton Id as parameters.
        ///In this function first we fetched a random carton Id where pallet Id is not null and then   
       /// fetched pallet info from src_carton,src_carton_detail and master_sku table while passing that carton Id as parameter,
        /// and then call repositories function GetPallet() with providing null as pallet Id and fetched cartonId as carton Id. 
        /// Then we asserted that the information returned by our query is same as returned by repositories function.
        /// </summary>
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        public void GetPalletInfoFromCartonIdTest()
        {
            //fetching random carton Id 
            var expectedCartonId = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT S.CARTON_ID AS CARTON_ID
                FROM SRC_CARTON S
               WHERE S.PALLET_ID IS NOT NULL
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 CartonId = row.GetValue<string>("CARTON_ID")
             }).ExecuteSingle(_db);

            //fetched information of pallet containg fetched carton
            var expectedPalletInfo = SqlBinder.Create(
                @"
        <![CDATA[
                WITH Q AS
                (SELECT CTN.PALLET_ID                           AS PALLET_ID,
                        COUNT(CTN.CARTON_ID)                    AS NUMBER_OF_CARTONS,
                        COUNT(DISTINCT CTN.LOCATION_ID)         AS LOCATION_COUNT,
                        COUNT(DISTINCT CTN.CARTON_STORAGE_AREA) AS NUMBER_OF_CARTON_AREA,
                        COUNT(DISTINCT CTN.VWH_ID)              AS NUMBER_OF_VWH,
                        MIN(MSKU.STYLE)                         AS STYLE,
                        MIN(MSKU.COLOR)                         AS COLOR,
                        MIN(MSKU.DIMENSION)                     AS DIMENSION,
                        MIN(MSKU.SKU_SIZE)                      AS SKU_SIZE,
                        MIN(CTNDET.SKU_ID)                      AS SKU_ID
                    FROM SRC_CARTON CTN
                    INNER JOIN SRC_CARTON_DETAIL CTNDET
                    ON CTN.CARTON_ID = CTNDET.CARTON_ID
                    INNER JOIN MASTER_SKU MSKU
                    ON MSKU.SKU_ID = CTNDET.SKU_ID
                    WHERE CTN.PALLET_ID IN
                        (SELECT S.PALLET_ID FROM SRC_CARTON S WHERE S.CARTON_ID = :CARTON_ID)
                    GROUP BY CTN.PALLET_ID
                ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
        ", row => new
            {
                PalletId = row.GetValue<string>("PALLET_ID"),
                CartonCount = row.GetValue<int>("NUMBER_OF_CARTONS"),
                LocationCount = row.GetValue<int>("LOCATION_COUNT"),
                CartonAreaCount = row.GetValue<int>("NUMBER_OF_CARTON_AREA"),
                VwhCount = row.GetValue<int>("NUMBER_OF_VWH"),
                Style = row.GetValue<string>("STYLE"),
                Color = row.GetValue<string>("COLOR"),
                Dimension = row.GetValue<string>("DIMENSION"),
                Size = row.GetValue<string>("SKU_SIZE"),
                SkuId = row.GetValue<int>("SKU_ID")
            }).Parameter("CARTON_ID",expectedCartonId.CartonId).ExecuteSingle(_db);

            //calling repositories function
            var actualPalletInfo = target.GetPallet(null, expectedCartonId.CartonId);

            //asserting
            Assert.AreEqual(expectedPalletInfo.CartonAreaCount, actualPalletInfo.AreaCount, "Area Count");
            Assert.AreEqual(expectedPalletInfo.CartonCount, actualPalletInfo.CartonCount, "Carton Count");
            Assert.AreEqual(expectedPalletInfo.Style, actualPalletInfo.PalletSku.Style, "Style");
            Assert.AreEqual(expectedPalletInfo.Color, actualPalletInfo.PalletSku.Color, "Color");
            Assert.AreEqual(expectedPalletInfo.Dimension, actualPalletInfo.PalletSku.Dimension, "Dimension");
            Assert.AreEqual(expectedPalletInfo.Size, actualPalletInfo.PalletSku.SkuSize, "SkuSize");
            Assert.AreEqual(expectedPalletInfo.LocationCount, actualPalletInfo.LocationCount, "Location Count");
            Assert.AreEqual(expectedPalletInfo.PalletId, actualPalletInfo.PalletId, "PalletId");
            Assert.AreEqual(expectedPalletInfo.SkuId, actualPalletInfo.PalletSku.SkuId, "Sku Id");
            Assert.AreEqual(expectedPalletInfo.VwhCount, actualPalletInfo.CartonVwhCount, "Carton Vwh Count");

        }


        /// <summary>
        /// This function is validating repositories function locatePallet() this accept
        /// Location Id,pallet Id ,area Id and merge pallet Id as parameter.
        /// In this function first we fetch a random pallet its location and area Id from a numbered area
        /// where pallet is required,
        /// Then we fetched a random different location from same carton area, 
        /// Then we called repositories function locate pallet while providing new location as location Id
        /// ,fetched pallet Id and carton area Id as pallet Id and carton area ID.
        /// Then we queryed that pallet information in src_carton Table
        /// and at last asserted that the pallet is relocated to new location in same area
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        public void UpdateSkusLocationTest()
        {
            //fetching a random pallet its location and area Id from a numbered area where pallet is required,
            var expectedPalletInfo = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT CTN.PALLET_ID           AS PALLET_ID,
                     CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                     CTN.LOCATION_ID         AS LOCATION_ID
                FROM SRC_CARTON CTN
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON CTN.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND CTN.PALLET_ID IS NOT NULL
                 AND CTN.LOCATION_ID IS NOT NULL
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 PalletId = row.GetValue<string>("PALLET_ID"),
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 Location_id = row.GetValue<string>("LOCATION_ID")
             }).ExecuteSingle(_db);

            if (expectedPalletInfo == null)
            {
                Assert.Inconclusive("No Pallet Found");
            }


            //fetching a new location from same area
            var newLocation = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT MSL.LOCATION_ID AS LOCATION_ID
                FROM MASTER_STORAGE_LOCATION MSL
               WHERE MSL.STORAGE_AREA = :STORAGE_AREA
                 AND MSL.LOCATION_ID != :LOCATION_ID
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 LocationId = row.GetValue<string>("LOCATION_ID")
             }).Parameter("STORAGE_AREA", expectedPalletInfo.Area)
             .Parameter("LOCATION_ID", expectedPalletInfo.Location_id)
             .ExecuteSingle(_db);

            if (newLocation == null)
            {
                Assert.Inconclusive("No Location Found");
            }


            //calling repositories function
            target.LocatePallet(newLocation.LocationId, expectedPalletInfo.PalletId, expectedPalletInfo.Area,null);

            //querying for pallet information
            var actualPalletInfo = SqlBinder.Create(@"
            <![CDATA[
        SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
               CTN.LOCATION_ID         AS LOCATION_ID
          FROM SRC_CARTON CTN
         WHERE CTN.PALLET_ID = :PALLET_ID
         GROUP BY CTN.CARTON_STORAGE_AREA, CTN.LOCATION_ID

            ]]>
            ",
             row => new
             {
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 LocationId = row.GetValue<string>("LOCATION_ID")
             }).Parameter("PALLET_ID", expectedPalletInfo.PalletId)
             .ExecuteSingle(_db);

            //asserting
            Assert.AreEqual(newLocation.LocationId, actualPalletInfo.LocationId, "LocationId");
            Assert.AreEqual(expectedPalletInfo.Area, actualPalletInfo.Area, "Area");

        }

        /// <summary>
        /// This function is validating repositories function locatePallet() this accept
        /// Location Id,pallet Id ,area Id and merge pallet Id as parameter.
        /// In this function first we fetch a random pallet its location and area Id from a numbered area
        /// where pallet is required,
        /// Then we fetch another random pallet its location and area Id from a numbered area
        /// where pallet is required and area is same as previous pallet area but different location, 
        /// Then we called repositories function locate pallet while providing new pallets location as location Id
        /// ,fetched pallet Id and carton area Id as pallet Id and carton area ID and new palletId as mergeronpalletId.
        /// Then we queryed that pallet information in src_carton Table
        /// and at last asserted that the pallet is merged with new pallet on its location Id in same area.
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("DataBAse")]
        [Owner("Ankit")]
        public void MergePalletsSameAreaTest()
        {
             //fetching a random pallet its location and area Id from a numbered area where pallet is required,
            var expectedPalletInfo = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT CTN.PALLET_ID                AS PALLET_ID,
                     MAX(CTN.CARTON_STORAGE_AREA) AS CARTON_STORAGE_AREA,
                     MAX(CTN.LOCATION_ID)         AS LOCATION_ID,
                     COUNT(CTN.CARTON_ID)         AS CARTON_COUNT,
                     MAX(CTNDET.SKU_ID)           AS SKU_ID                     
                FROM SRC_CARTON CTN
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON CTN.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
                  INNER JOIN src_carton_detail ctndet
                  ON ctn.carton_id = ctndet.carton_id
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND CTN.PALLET_ID IS NOT NULL
                 AND CTN.LOCATION_ID IS NOT NULL
               GROUP BY CTN.PALLET_ID
               ORDER BY DBMS_RANDOM.VALUE
                )
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 PalletId = row.GetValue<string>("PALLET_ID"),
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 Location_id = row.GetValue<string>("LOCATION_ID"),
                 CartonCount = row.GetValue<int>("CARTON_COUNT"),
                 SkuID = row.GetValue<int>("SKU_ID"),
             }).ExecuteSingle(_db);

            if (expectedPalletInfo == null)
            {
                Assert.Inconclusive("No Pallet Found");
            }


            //fetching a random pallet its location and area Id from a numbered area where pallet is required for merging,
            var expectedMergePalletInfo = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT CTN.PALLET_ID                AS PALLET_ID,
                     MAX(CTN.CARTON_STORAGE_AREA) AS CARTON_STORAGE_AREA,
                     MAX(CTN.LOCATION_ID)         AS LOCATION_ID,
                     COUNT(CTN.CARTON_ID)         AS CARTON_COUNT,
                     MAX(CTNDET.SKU_ID)           AS SKU_ID
                FROM SRC_CARTON CTN
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON CTN.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
                  INNER JOIN SRC_CARTON_DETAIL CTNDET
                  ON CTN.CARTON_ID = CTNDET.CARTON_ID
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND CTN.PALLET_ID IS NOT NULL
                 AND CTN.LOCATION_ID IS NOT NULL
                 AND CTN.PALLET_ID != :PALLET_ID
                 AND CTN.LOCATION_ID != :LOCATION_ID
                 AND CTN.CARTON_STORAGE_AREA = :CARTON_STORAGE_AREA
                 AND CTNDET.SKU_ID = :SKU_ID
                GROUP BY CTN.PALLET_ID
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 PalletId = row.GetValue<string>("PALLET_ID"),
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 Location_id = row.GetValue<string>("LOCATION_ID"),
                 CartonCount = row.GetValue<int>("CARTON_COUNT"),
                 SkuID = row.GetValue<int>("SKU_ID")
             }).Parameter("PALLET_ID",expectedPalletInfo.PalletId)
             .Parameter("LOCATION_ID",expectedPalletInfo.Location_id)
             .Parameter("CARTON_STORAGE_AREA",expectedPalletInfo.Area).Parameter("SKU_ID",expectedPalletInfo.SkuID).ExecuteSingle(_db);
            if (expectedMergePalletInfo == null)
            {
                Assert.Inconclusive("No Pallet for merge Found");
            }


            //calling repositories function
            target.LocatePallet(expectedMergePalletInfo.Location_id, expectedPalletInfo.PalletId, expectedPalletInfo.Area,expectedMergePalletInfo.PalletId);

            var actualMergedPalletInfo = SqlBinder.Create(@"
            <![CDATA[
        SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
               CTN.LOCATION_ID         AS LOCATION_ID,
               COUNT(CTN.CARTON_ID)    AS CARTON_COUNT
          FROM SRC_CARTON CTN
         WHERE CTN.PALLET_ID = :PALLET_ID
          AND CTN.CARTON_STORAGE_AREA = :CARTON_STORAGE_AREA
         GROUP BY CTN.PALLET_ID,CTN.CARTON_STORAGE_AREA, CTN.LOCATION_ID

            ]]>
            ",
          row => new
          {
              Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
              LocationId = row.GetValue<string>("LOCATION_ID"),
              CartonCount = row.GetValue<int>("CARTON_COUNT")
          }).Parameter("PALLET_ID", expectedMergePalletInfo.PalletId).Parameter("CARTON_STORAGE_AREA",expectedPalletInfo.Area)
          .ExecuteSingle(_db);

            //asserting
            Assert.AreEqual(expectedMergePalletInfo.Location_id, actualMergedPalletInfo.LocationId, "LocationId");
            Assert.AreEqual(expectedPalletInfo.Area, actualMergedPalletInfo.Area, "Area");
            Assert.AreEqual((expectedPalletInfo.CartonCount + expectedMergePalletInfo.CartonCount), actualMergedPalletInfo.CartonCount, "Carton Count");

            var actualPalletInfo = SqlBinder.Create(@"
            <![CDATA[
        SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
               CTN.LOCATION_ID         AS LOCATION_ID,
               COUNT(CTN.CARTON_ID)    AS CARTON_COUNT
          FROM SRC_CARTON CTN
         WHERE CTN.PALLET_ID = :PALLET_ID
         GROUP BY CTN.PALLET_ID,CTN.CARTON_STORAGE_AREA, CTN.LOCATION_ID

            ]]>
            ",
          row => new
          {
              Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
              LocationId = row.GetValue<string>("LOCATION_ID"),
              CartonCount = row.GetValue<int>("CARTON_COUNT")
          }).Parameter("PALLET_ID", expectedPalletInfo.PalletId)
          .ExecuteSingle(_db);

            //asserting
            Assert.IsNull(actualPalletInfo, "LocationId");
           

        }


        /// <summary>
        /// This function is validating repositories function locatePallet() this accept
        /// Location Id,pallet Id ,area Id and merge pallet Id as parameter.
        /// In this function first we fetch a random pallet its location and area Id from a numbered area
        /// where pallet is required,
        /// Then we fetch another random pallet its location and area Id from a numbered area
        /// where pallet is required and area is not same as previous pallet area.
        /// Then we called repositories function locate pallet while providing new pallets location as location Id
        /// ,fetched pallet Id and carton area Id as pallet Id and carton area ID and new palletId as mergeronpalletId.
        /// Then we queryed that pallet information in src_carton Table
        /// and at last asserted that the pallet is merged with new pallet on its location Id in same area.
        /// 
        /// </summary>
        [TestMethod]
        [TestCategory("DataBAse")]
        [Owner("Ankit")]
        public void MergePalletsDiffAreaTest()
        {
            //fetching a random pallet its location and area Id from a numbered area where pallet is required,
            var expectedPalletInfo = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT CTN.PALLET_ID                AS PALLET_ID,
                     MAX(CTN.CARTON_STORAGE_AREA) AS CARTON_STORAGE_AREA,
                     MAX(CTN.LOCATION_ID)         AS LOCATION_ID,
                     COUNT(CTN.CARTON_ID)         AS CARTON_COUNT,
                     MAX(CTNDET.SKU_ID)           AS SKU_ID                     
                FROM SRC_CARTON CTN
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON CTN.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
                  INNER JOIN src_carton_detail ctndet
                  ON ctn.carton_id = ctndet.carton_id
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND CTN.PALLET_ID IS NOT NULL
                 AND CTN.LOCATION_ID IS NOT NULL
               GROUP BY CTN.PALLET_ID
               ORDER BY DBMS_RANDOM.VALUE
                )
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 PalletId = row.GetValue<string>("PALLET_ID"),
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 Location_id = row.GetValue<string>("LOCATION_ID"),
                 CartonCount = row.GetValue<int>("CARTON_COUNT"),
                 SkuID = row.GetValue<int>("SKU_ID"),
             }).ExecuteSingle(_db);

            if (expectedPalletInfo == null)
            {
                Assert.Inconclusive("No Pallet Found");
            }


            //fetching a random pallet its location and area Id from a numbered area where pallet is required for merging,
            var expectedMergePalletInfo = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT CTN.PALLET_ID                AS PALLET_ID,
                     MAX(CTN.CARTON_STORAGE_AREA) AS CARTON_STORAGE_AREA,
                     MAX(CTN.LOCATION_ID)         AS LOCATION_ID,
                     COUNT(CTN.CARTON_ID)         AS CARTON_COUNT,
                     MAX(CTNDET.SKU_ID)           AS SKU_ID
                FROM SRC_CARTON CTN
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON CTN.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
                  INNER JOIN SRC_CARTON_DETAIL CTNDET
                  ON CTN.CARTON_ID = CTNDET.CARTON_ID
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND CTN.PALLET_ID IS NOT NULL
                 AND CTN.LOCATION_ID IS NOT NULL
                 AND CTN.PALLET_ID != :PALLET_ID
                 AND CTN.LOCATION_ID != :LOCATION_ID
                 AND CTN.CARTON_STORAGE_AREA != :CARTON_STORAGE_AREA
                 AND CTNDET.SKU_ID = :SKU_ID
                GROUP BY CTN.PALLET_ID
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 PalletId = row.GetValue<string>("PALLET_ID"),
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 Location_id = row.GetValue<string>("LOCATION_ID"),
                 CartonCount = row.GetValue<int>("CARTON_COUNT"),
                 SkuID = row.GetValue<int>("SKU_ID")
             }).Parameter("PALLET_ID", expectedPalletInfo.PalletId)
             .Parameter("LOCATION_ID", expectedPalletInfo.Location_id)
             .Parameter("CARTON_STORAGE_AREA", expectedPalletInfo.Area).Parameter("SKU_ID", expectedPalletInfo.SkuID).ExecuteSingle(_db);
            if (expectedMergePalletInfo == null)
            {
                Assert.Inconclusive("No Pallet for merge Found");
            }


            //calling repositories function
            target.LocatePallet(expectedMergePalletInfo.Location_id, expectedPalletInfo.PalletId, expectedMergePalletInfo.Area, expectedMergePalletInfo.PalletId);

            var actualMergedPalletInfo = SqlBinder.Create(@"
            <![CDATA[
        SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
               CTN.LOCATION_ID         AS LOCATION_ID,
               COUNT(CTN.CARTON_ID)    AS CARTON_COUNT
          FROM SRC_CARTON CTN
         WHERE CTN.PALLET_ID = :PALLET_ID
          AND CTN.CARTON_STORAGE_AREA = :CARTON_STORAGE_AREA
         GROUP BY CTN.PALLET_ID,CTN.CARTON_STORAGE_AREA, CTN.LOCATION_ID

            ]]>
            ",
          row => new
          {
              Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
              LocationId = row.GetValue<string>("LOCATION_ID"),
              CartonCount = row.GetValue<int>("CARTON_COUNT")
          }).Parameter("PALLET_ID", expectedMergePalletInfo.PalletId).Parameter("CARTON_STORAGE_AREA", expectedMergePalletInfo.Area)
          .ExecuteSingle(_db);

            //asserting
            Assert.AreEqual(expectedMergePalletInfo.Location_id, actualMergedPalletInfo.LocationId, "LocationId");
            Assert.AreEqual(expectedMergePalletInfo.Area, actualMergedPalletInfo.Area, "Area");
            Assert.AreEqual((expectedPalletInfo.CartonCount + expectedMergePalletInfo.CartonCount), actualMergedPalletInfo.CartonCount, "Carton Count");

            var actualPalletInfo = SqlBinder.Create(@"
            <![CDATA[
        SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
               CTN.LOCATION_ID         AS LOCATION_ID,
               COUNT(CTN.CARTON_ID)    AS CARTON_COUNT
          FROM SRC_CARTON CTN
         WHERE CTN.PALLET_ID = :PALLET_ID
         GROUP BY CTN.PALLET_ID,CTN.CARTON_STORAGE_AREA, CTN.LOCATION_ID

            ]]>
            ",
          row => new
          {
              Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
              LocationId = row.GetValue<string>("LOCATION_ID"),
              CartonCount = row.GetValue<int>("CARTON_COUNT")
          }).Parameter("PALLET_ID", expectedPalletInfo.PalletId)
          .ExecuteSingle(_db);

            //asserting
            Assert.IsNull(actualPalletInfo, "LocationId");


        }


        /// <summary>
        /// This function is validating repositories function locatePallet() this accept
        /// Location Id,pallet Id ,area Id and merge pallet Id as parameter.
        /// In this function first we fetch a random pallet and its area Id from a numbered area
        /// where pallet is required and location of pallet is not provided,
        /// Then we fetched a random location from same carton area, 
        /// Then we called repositories function locate pallet while providing location as location Id
        /// ,fetched pallet Id and carton area Id as pallet Id and carton area ID.
        /// Then we queryed that pallet information in src_carton Table
        /// and at last asserted that the pallet is located to location provided.    
        /// </summary>
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        public void PutSkuOnLocationTest()
        {
            //fetch a random pallet  and its area Id from a numbered area
            /// where pallet is required and location of pallet in not provided,
            var expectedPalletInfo = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT CTN.PALLET_ID           AS PALLET_ID,
                     CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA
                FROM SRC_CARTON CTN
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON CTN.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND CTN.PALLET_ID IS NOT NULL
                 AND CTN.LOCATION_ID IS NULL
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 PalletId = row.GetValue<string>("PALLET_ID"),
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA")
             }).ExecuteSingle(_db);

            if (expectedPalletInfo == null)
            {
                Assert.Inconclusive("No Pallet Found");
            }

            //fetching a location from ftched carton area
            var newLocation = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT MSL.LOCATION_ID AS LOCATION_ID
                FROM MASTER_STORAGE_LOCATION MSL
               WHERE MSL.STORAGE_AREA = :STORAGE_AREA                 
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 LocationId = row.GetValue<string>("LOCATION_ID")
             }).Parameter("STORAGE_AREA", expectedPalletInfo.Area)
             .ExecuteSingle(_db);

            if (newLocation == null)
            {
                Assert.Inconclusive("No Location Found");
            }

            //calling repositories function
            target.LocatePallet(newLocation.LocationId, expectedPalletInfo.PalletId, expectedPalletInfo.Area, null);

            //querying pallet information 
            var actualPalletInfo = SqlBinder.Create(@"
            <![CDATA[
        SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
               CTN.LOCATION_ID         AS LOCATION_ID
          FROM SRC_CARTON CTN
         WHERE CTN.PALLET_ID = :PALLET_ID
         GROUP BY CTN.CARTON_STORAGE_AREA, CTN.LOCATION_ID

            ]]>
            ",
             row => new
             {
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 LocationId = row.GetValue<string>("LOCATION_ID")
             }).Parameter("PALLET_ID", expectedPalletInfo.PalletId)
             .ExecuteSingle(_db);

            //asserting
            Assert.AreEqual(newLocation.LocationId, actualPalletInfo.LocationId, "LocationId");
            Assert.AreEqual(expectedPalletInfo.Area, actualPalletInfo.Area, "Area");

        }


        /// <summary>
        /// This function is validating repositories function locatePallet() this accept
        /// Location Id,pallet Id ,area Id and merge pallet Id as parameter.
        /// In this function first we fetch a random pallet its location and area Id from a numbered area
        /// where pallet is required,
        /// Then we fetched a random different location from different numbered carton area where pallet is required, 
        /// Then we called repositories function locate pallet while providing new location as location Id
        /// ,fetched pallet Id as pallet Id and new carton area Id as carton area ID.
        /// Then we queryed that pallet information in src_carton Table
        /// and at last asserted that the pallet is relocated to new location in new area      
        /// </summary>
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        public void PutSkuOnDifferentAreaTest()
        {
            //fetching a random pallet its location and area Id from a numbered area
            /// where pallet is required
            var expectedPalletInfo = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT CTN.PALLET_ID           AS PALLET_ID,
                     CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                     CTN.LOCATION_ID         AS LOCATION_ID
                FROM SRC_CARTON CTN
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON CTN.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND CTN.PALLET_ID IS NOT NULL
                 ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 PalletId = row.GetValue<string>("PALLET_ID"),
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 Location_id = row.GetValue<string>("LOCATION_ID")
             }).ExecuteSingle(_db);

            if (expectedPalletInfo == null)
            {
                Assert.Inconclusive("No Pallet Found");
            }


            //fetched a random location from different numbered carton area where pallet is required, 
            var newLocation = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT MSL.LOCATION_ID AS LOCATION_ID,
                     MSL.STORAGE_AREA AS STORAGE_AREA
                FROM MASTER_STORAGE_LOCATION MSL
                INNER JOIN TAB_INVENTORY_AREA TIA 
                ON TIA.INVENTORY_STORAGE_AREA = MSL.STORAGE_AREA
               WHERE MSL.STORAGE_AREA != :STORAGE_AREA   
                 AND TIA.STORES_WHAT = 'CTN'                                  
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 LocationId = row.GetValue<string>("LOCATION_ID"),
                 Area = row.GetValue<string>("STORAGE_AREA")
             }).Parameter("STORAGE_AREA", expectedPalletInfo.Area)
             .ExecuteSingle(_db);

            if (newLocation == null)
            {
                Assert.Inconclusive("No Location Found");
            }


            //calling repositories function
            target.LocatePallet(newLocation.LocationId, expectedPalletInfo.PalletId, newLocation.Area,null);

            //querying repositories function
            var actualPalletInfo = SqlBinder.Create(@"
            <![CDATA[
        SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
               CTN.LOCATION_ID         AS LOCATION_ID
          FROM SRC_CARTON CTN
         WHERE CTN.PALLET_ID = :PALLET_ID
         GROUP BY CTN.CARTON_STORAGE_AREA, CTN.LOCATION_ID

            ]]>
            ",
             row => new
             {
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 LocationId = row.GetValue<string>("LOCATION_ID")
             }).Parameter("PALLET_ID", expectedPalletInfo.PalletId)
             .ExecuteSingle(_db);

            //asserting
            Assert.AreEqual(newLocation.LocationId, actualPalletInfo.LocationId, "LocationId");
            Assert.AreEqual(newLocation.Area, actualPalletInfo.Area, "Area");

        }


        /// <summary>
        /// This function is validating repositories function GetLocation() this accept
        /// Location Id as parameters.
        /// In this first we fetched a random location information and then call repositories function 
        /// GetLocations() while passing fetched location Id as parameter
        /// and the we asserted that the information fetched by our query is same as information
        /// returned by repositories function
        ///</summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        public void GetLocationsTest()
        {
            // fetched a random location information 
            var expectedLocationInfo = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT MSL.LOCATION_ID               AS LOCATION_ID,
                     MAX(MSL.ASSIGNED_SKU_ID)      AS ASSIGNED_SKU_ID,
                     MAX(MSL.ASSIGNED_MAX_CARTONS) AS ASSIGNED_MAX_CARTONS,
                     COUNT(DISTINCT CTN.PALLET_ID) AS NUMBER_OF_PALLET,
                     COUNT(DISTINCT CTN.CARTON_ID) AS NUMBER_OF_CARTONS,
                     MAX(MSL.STORAGE_AREA)         AS STORAGE_AREA,
                     MAX(TIA.SHORT_NAME)           AS SHORT_NAME,
                     MAX(MSL.UNAVAILABLE_FLAG)     AS UNAVAILABLE_FLAG,
                     COUNT(DISTINCT CTNDET.SKU_ID) AS NUMBER_OF_SKU
                FROM MASTER_STORAGE_LOCATION MSL
               INNER JOIN SRC_CARTON CTN
                  ON MSL.LOCATION_ID = CTN.LOCATION_ID
               INNER JOIN SRC_CARTON_DETAIL CTNDET
                  ON CTN.CARTON_ID = CTNDET.CARTON_ID
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON TIA.INVENTORY_STORAGE_AREA = MSL.STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
               GROUP BY MSL.LOCATION_ID
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
                 row => new
                 {
                     LocationId = row.GetValue<string>("LOCATION_ID"),
                     AssignedSku = row.GetValue<int?>("ASSIGNED_SKU_ID"),
                     AssignedMaxCartons = row.GetValue<int?>("ASSIGNED_MAX_CARTONS"),
                     CartonCount = row.GetValue<int>("NUMBER_OF_CARTONS"),
                     Area = row.GetValue<string>("STORAGE_AREA"),
                     ShortName = row.GetValue<string>("SHORT_NAME"),
                     UnavailableFlag = row.GetValue<string>("UNAVAILABLE_FLAG"),
                     SkuCount = row.GetValue<int>("NUMBER_OF_SKU")
                 }).ExecuteSingle(_db);

            if (expectedLocationInfo == null)
            {
                Assert.Inconclusive("No Location Found");
            }

            //calling repositories function
            var actualLocationInfo = target.GetLocation(expectedLocationInfo.LocationId);
            
            //asserting
            Assert.AreEqual(expectedLocationInfo.LocationId, actualLocationInfo.LocationId, "Location Id");
            Assert.AreEqual(expectedLocationInfo.Area, actualLocationInfo.Area.AreaId, "Area");
            Assert.AreEqual(expectedLocationInfo.ShortName, actualLocationInfo.Area.ShortName, "Short Name");
            Assert.AreEqual(expectedLocationInfo.AssignedMaxCartons, actualLocationInfo.MaxCartons, "Max Cartons");
            if (actualLocationInfo.AssignedSku != null)
            {
                Assert.AreEqual(expectedLocationInfo.AssignedSku, actualLocationInfo.AssignedSku.SkuId, "Assigned Sku");
            }
            else
            {
                Assert.IsNull(expectedLocationInfo.AssignedSku, "AssignedSku");
            }
            Assert.AreEqual(expectedLocationInfo.CartonCount, actualLocationInfo.CartonCount, "Carton Count");
            Assert.AreEqual((!string.IsNullOrEmpty(expectedLocationInfo.UnavailableFlag)), actualLocationInfo.UnavailableFlag, "Unavailable Flag"); 
            Assert.AreEqual(expectedLocationInfo.SkuCount, actualLocationInfo.SkuCount, "Location Id");
        }



        [TestMethod]
        [TestCategory("Database")]
        [Owner("Ankit")]
        public void GetCartonTest()
        {
            var expectedCarton = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT CTN.CARTON_ID           AS CARTON_ID,
                     CTN.QUALITY_CODE        AS QUALITY_CODE,
                     CTN.VWH_ID              AS VWH_ID,
                     CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                     CTNDET.SKU_ID           AS SKU_ID,
                     CTNDET.STYLE            AS STYLE,
                     CTNDET.COLOR            AS COLOR,
                     CTNDET.DIMENSION        AS DIMENSION,
                     CTNDET.SKU_SIZE         AS SKU_SIZE,
                     CTNDET.QUANTITY         AS QUANTITY,
                     TIA.SHORT_NAME          AS SHORT_NAME
                FROM SRC_CARTON CTN
               INNER JOIN SRC_CARTON_DETAIL CTNDET
                  ON CTN.CARTON_ID = CTNDET.CARTON_ID
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON TIA.INVENTORY_STORAGE_AREA = CTN.CARTON_STORAGE_AREA
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 CartinId = row.GetValue<string>("CARTON_ID"),
                 Quality = row.GetValue<string>("QUALITY_CODE"),
                 VwhId = row.GetValue<string>("VWH_ID"),
                 CartonArea = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 SkuID = row.GetValue<int>("SKU_ID"),
                 Style = row.GetValue<string>("STYLE"),
                 Color = row.GetValue<string>("COLOR"),
                 Dimension = row.GetValue<string>("DIMENSION"),
                 Size = row.GetValue<string>("SKU_SIZE"),
                 Quantity = row.GetValue<int>("QUANTITY"),
                 ShortName = row.GetValue<string>("SHORT_NAME")
             }).ExecuteSingle(_db);

            if (expectedCarton == null)
            {
                Assert.Inconclusive("No carton Found");
            }


            var actualCartonInfo = target.GetCarton(expectedCarton.CartinId);

            Assert.AreEqual(expectedCarton.CartonArea, actualCartonInfo.Area.AreaId, "area");
            Assert.AreEqual(expectedCarton.Style, actualCartonInfo.Sku.Style, "Style");
            Assert.AreEqual(expectedCarton.Color, actualCartonInfo.Sku.Color, "Color");
            Assert.AreEqual(expectedCarton.Dimension, actualCartonInfo.Sku.Dimension, "Dimension");
            Assert.AreEqual(expectedCarton.Size, actualCartonInfo.Sku.SkuSize, "Size");
            Assert.AreEqual(expectedCarton.Quality, actualCartonInfo.QualityCode, "Quality Code");
            Assert.AreEqual(expectedCarton.Quantity, actualCartonInfo.Pieces, "Pieces");
            Assert.AreEqual(expectedCarton.ShortName, actualCartonInfo.Area.ShortName, "Short NAme");
            Assert.AreEqual(expectedCarton.SkuID, actualCartonInfo.Sku.SkuId, "Sku ID");
            Assert.AreEqual(expectedCarton.VwhId, actualCartonInfo.VwhId, "VwhId");

            
        }


        //invalid test cases

        /// <summary>
        /// This function is validating repositories function locatePallet() this accept
        /// Location Id,pallet Id ,area Id and merge pallet Id as parameter.
        /// In this function first we fetch a random pallet and its area Id from a numbered area
        /// where pallet is required. 
        ///and then called repositories function locatePallet() while providing an invalid locationId
        ///and fetched pallet id and area Id as pallet Id and area Id parameter
        ///In this we expect the function voilates exception
        /// </summary>
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void UpdatePalletToInvalidLocationTest()
        {
            //fetch a random pallet and its area Id from a numbered area
            /// where pallet is required. 
            var expectedPalletInfo = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT CTN.PALLET_ID           AS PALLET_ID,
                     CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA
                FROM SRC_CARTON CTN
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON CTN.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND CTN.PALLET_ID IS NOT NULL
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 PalletId = row.GetValue<string>("PALLET_ID"),
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA")
             }).ExecuteSingle(_db);

            if (expectedPalletInfo == null)
            {
                Assert.Inconclusive("No Pallet Found");
            }

            //calling repositories function
            target.LocatePallet("asdf", expectedPalletInfo.PalletId, expectedPalletInfo.Area,null);

        }

        /// <summary>
        /// This function is validating repositories function locatePallet() this accept
        /// Location Id,pallet Id ,area Id and merge pallet Id as parameter.
        /// In this function first we fetch a random pallet and its location Id from a numbered area
        /// where pallet is required. 
        ///and then called repositories function locatePallet() while providing an invalid area Id
        ///and fetched pallet id and area Id as pallet Id and area Id parameter
        ///In this we expect the function voilates exception
        /// </summary>
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void UpdatePalletToInvalidAreaTest()
        {
            //fetching a random pallet and its location Id from a numbered area
            /// where pallet is required. 
            var expectedPalletInfo = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT CTN.PALLET_ID           AS PALLET_ID,
                     CTN.LOCATION_ID         AS LOCATION_ID
                FROM SRC_CARTON CTN
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON CTN.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND CTN.PALLET_ID IS NOT NULL
                 AND CTN.LOCATION_ID IS NOT NULL
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 PalletId = row.GetValue<string>("PALLET_ID"),
                 Location_id = row.GetValue<string>("LOCATION_ID")
             }).ExecuteSingle(_db);

            if (expectedPalletInfo == null)
            {
                Assert.Inconclusive("No Pallet Found");
            }

            //calling repositories function
            target.LocatePallet(expectedPalletInfo.Location_id, expectedPalletInfo.PalletId, "ank",null);

        }
        

        /// <summary>
        /// This function is validating repositories function locatePallet() this accept
        /// Location Id,pallet Id ,area Id as parameters and merge pallet Id as parameter.
        /// In this function first we fetch a random pallet,its area Id and its location Id from a numbered area
        /// where pallet is required. 
        /// and then fetched a different location Id from same area
        ///and then called repositories function locatePallet() while providing an invalid pallet Id,fetched carton area Id
        ///and new location Id
        ///At last we asserted that the information against that pallet is null.
        /// </summary>
        [TestMethod]
        [TestCategory("DataBase")]
        [Owner("Ankit")]
        public void LocateInvalidPalletTest()
        {
            //fetching a random pallet ,its area id and its location Id from a numbered area
            /// where pallet is required. 
            var expectedPalletInfo = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT CTN.PALLET_ID           AS PALLET_ID,
                     CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                     CTN.LOCATION_ID         AS LOCATION_ID
                FROM SRC_CARTON CTN
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON CTN.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                 AND TIA.IS_PALLET_REQUIRED IS NOT NULL
                 AND CTN.PALLET_ID IS NOT NULL
                 AND CTN.LOCATION_ID IS NOT NULL
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 PalletId = row.GetValue<string>("PALLET_ID"),
                 Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
                 Location_id = row.GetValue<string>("LOCATION_ID")
             }).ExecuteSingle(_db);

            if (expectedPalletInfo == null)
            {
                Assert.Inconclusive("No Pallet Found");
            }

            //fetching a new location in fetched area
            var newLocation = SqlBinder.Create(@"
            <![CDATA[
            WITH Q AS
             (SELECT MSL.LOCATION_ID AS LOCATION_ID
                FROM MASTER_STORAGE_LOCATION MSL
               WHERE MSL.STORAGE_AREA = :STORAGE_AREA
                 AND MSL.LOCATION_ID != :LOCATION_ID
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
            ]]>
            ",
             row => new
             {
                 LocationId = row.GetValue<string>("LOCATION_ID")
             }).Parameter("STORAGE_AREA", expectedPalletInfo.Area)
             .Parameter("LOCATION_ID", expectedPalletInfo.Location_id)
             .ExecuteSingle(_db);

            if (newLocation == null)
            {
                Assert.Inconclusive("No Location Found");
            }

            //calling repositories function
            target.LocatePallet(newLocation.LocationId, "amela", expectedPalletInfo.Area,null);

            //querying against pallet
            var actualPalletInfo = SqlBinder.Create(@"
            <![CDATA[
        SELECT CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
               CTN.LOCATION_ID         AS LOCATION_ID
          FROM SRC_CARTON CTN
         WHERE CTN.PALLET_ID = 'amela'
         GROUP BY CTN.CARTON_STORAGE_AREA, CTN.LOCATION_ID
            ]]>
            ",
        row => new
        {
            Area = row.GetValue<string>("CARTON_STORAGE_AREA"),
            LocationId = row.GetValue<string>("LOCATION_ID")
        })
        .ExecuteSingle(_db);

            //asserting
            Assert.IsNull(actualPalletInfo, "Updation is happening for invalid pallet");

        }

        
    }
}
