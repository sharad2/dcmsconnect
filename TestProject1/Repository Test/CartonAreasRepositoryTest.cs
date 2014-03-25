using DcmsMobile.CartonAreas.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using EclipseLibrary.Oracle;
using System.Web;
using DcmsMobile.CartonAreas.Models;
using System.Collections.Generic;
using System.Data.Common;

namespace TestProject1
{


    /// <summary>
    ///This is a test class for CartonAreasRepositoryTest and is intended
    ///to contain all CartonAreasRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CartonAreasRepositoryTest
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
            _db.CreateConnection("Data Source=w8singapore/mfdev;Proxy User Id=dcms8;Proxy Password=dcms8", "");
        }


        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            _db.Dispose();
        }
        private CartonAreasRepository target;
        private DbTransaction _trans;

        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            target = new CartonAreasRepository(_db);
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
        /// This function is verifying repositories function GetCartonAreas() that accepts a carton Area Id and returns its 
        /// information.
        /// Here first we fetch a random carton area id from tab inventory table and then query information against that area.
        /// After this we called repositories function GetCartonArea() and verified values returned by my query and repositories 
        /// function. 
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void GetCartonAreaTest()
        {
            var cartonArea = SqlBinder.Create(@"
                <![CDATA[
                WITH Q AS
                    (SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA
                    FROM TAB_INVENTORY_AREA TIA
                    WHERE TIA.STORES_WHAT = 'CTN'
                    ORDER BY DBMS_RANDOM.VALUE)
                SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                ", row => new
                    {
                        Id = row.GetValue<string>("INVENTORY_STORAGE_AREA")
                    }).ExecuteSingle(_db);

            if (cartonArea == null)
            {
                Assert.Inconclusive("No carton Area found");
            }


            var expectedAreaInfo = SqlBinder.Create(@"
                <![CDATA[
                SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                       MAX(TIA.DESCRIPTION) AS DESCRIPTION,
                       MAX(TIA.LOCATION_NUMBERING_FLAG) AS LOCATION_NUMBERING_FLAG,
                       MAX(TIA.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
                       MAX(TIA.SHORT_NAME) AS SHORT_NAME,
                       COUNT(MSL.LOCATION_ID) AS NUMBER_OF_LOCATIONS
                  FROM TAB_INVENTORY_AREA TIA
                 LEFT OUTER JOIN MASTER_STORAGE_LOCATION MSL
                    ON MSL.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
                 WHERE TIA.INVENTORY_STORAGE_AREA = :INVENTORY_STORAGE_AREA
                 GROUP BY TIA.INVENTORY_STORAGE_AREA
                ]]>
                ", row => new
                 {
                     Id = row.GetValue<string>("INVENTORY_STORAGE_AREA"),
                     Description = row.GetValue<string>("DESCRIPTION"),
                     LocationNumbering = row.GetValue<string>("LOCATION_NUMBERING_FLAG"),
                     Building = row.GetValue<string>("WAREHOUSE_LOCATION_ID"),
                     ShortName = row.GetValue<string>("SHORT_NAME"),
                     LocationsCount = row.GetValue<int>("NUMBER_OF_LOCATIONS")
                 }).Parameter("INVENTORY_STORAGE_AREA", cartonArea.Id)
                 .ExecuteSingle(_db);

            Assert.IsNotNull(expectedAreaInfo, "No Carton area info found");


            var actualAreaInfo = (IEnumerable<CartonArea>)target.GetCartonAreas(cartonArea.Id);

            Assert.IsNotNull(actualAreaInfo, "No Carton area info found by repository function");



            foreach (var item in actualAreaInfo)
            {
                Assert.AreEqual(expectedAreaInfo.Building, item.BuildingId, "Building Id");
                Assert.AreEqual(expectedAreaInfo.Description, item.Description, "Description");
                Assert.AreEqual(expectedAreaInfo.Id, item.AreaId, "Area Id");
                Assert.AreEqual(string.Equals(expectedAreaInfo.LocationNumbering, "Y"), item.LocationNumberingFlag, "Location Numbering Flag");
                Assert.AreEqual(expectedAreaInfo.LocationsCount, item.TotalLocations, "Locations Count");
                Assert.AreEqual(expectedAreaInfo.ShortName, item.ShortName, "Short Name");
            }

        }


        /// <summary>
        /// This function is verifying repositories function GetLocations() that accepts a Locationfilter model and returns 
        /// information against filter data.
        /// Here first we fetched a random Location Id from a random numbered carton area and then popullated this information 
        /// in LocationFilter Model
        /// Then we query database against fetched location id.After this we called repositories function GetLocations and 
        /// passed LocationFilter model's object as parameter.
        /// Then we verified values returned by my query and repositories function.
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void GetLocationTest()
        {
            var cartonArea = SqlBinder.Create(@"
                <![CDATA[
               WITH Q AS
             (SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                     MSL.LOCATION_ID            AS LOCATION_ID
                FROM TAB_INVENTORY_AREA TIA
               INNER JOIN MASTER_STORAGE_LOCATION MSL
                  ON MSL.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG = 'Y'
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                ", row => new
                    {
                        Id = row.GetValue<string>("INVENTORY_STORAGE_AREA"),
                        LocationId = row.GetValue<string>("LOCATION_ID")
                    }).ExecuteSingle(_db);

            if (cartonArea == null)
            {
                Assert.Inconclusive("No carton area or location found "); 
            }

            var filterData = new LocationFilter
            {
                CartonAreaId = cartonArea.Id,
                LocationId = cartonArea.LocationId                
            };


            var expectedLocationInfo = SqlBinder.Create(@"
                        <![CDATA[
                        SELECT MSL.LOCATION_ID AS LOCATION_ID,
                               MAX(MSL.ASSIGNED_MAX_CARTONS) AS ASSIGNED_MAX_CARTONS,
                               MAX(MSKU.SKU_ID) AS SKU_ID,
                               MAX(MSKU.STYLE) AS STYLE,
                               MAX(MSKU.COLOR) AS COLOR,
                               MAX(MSKU.DIMENSION) AS DIMENSION,
                               MAX(MSKU.SKU_SIZE) AS SKU_SIZE,
                               MAX(MSKU.UPC_CODE) AS UPC_CODE,
                               COUNT(CTN.CARTON_ID) AS CARTON_COUNT,
                               COUNT(CTN.PALLET_ID) AS PALLET_COUNT,
                               SUM(CTNDET.QUANTITY) AS TOTAL_PIECES
                          FROM MASTER_STORAGE_LOCATION MSL
                          LEFT OUTER JOIN MASTER_SKU MSKU
                            ON MSL.ASSIGNED_SKU_ID = MSKU.SKU_ID
                          LEFT OUTER JOIN SRC_CARTON CTN
                            ON CTN.LOCATION_ID = MSL.LOCATION_ID
                           AND CTN.CARTON_STORAGE_AREA = MSL.STORAGE_AREA
                          LEFT OUTER JOIN SRC_CARTON_DETAIL CTNDET
                            ON CTN.CARTON_ID = CTNDET.CARTON_ID
                         WHERE MSL.LOCATION_ID = :LOCATION_ID
                           AND MSL.STORAGE_AREA = :STORAGE_AREA
                         GROUP BY MSL.LOCATION_ID
                        ]]>
                        ", row => new
                         {
                             LocationId = row.GetValue<string>("LOCATION_ID"),
                             MaxCartons = row.GetValue<int?>("ASSIGNED_MAX_CARTONS"),
                             SkuId = row.GetValue<int?>("SKU_ID"),
                             Style = row.GetValue<string>("STYLE"),
                             Color = row.GetValue<string>("COLOR"),
                             Dimension = row.GetValue<string>("DIMENSION"),
                             Size = row.GetValue<string>("SKU_SIZE"),
                             UPC = row.GetValue<string>("UPC_CODE"),
                             CartonCount = row.GetValue<int>("CARTON_COUNT"),
                             PalletCount = row.GetValue<int>("PALLET_COUNT"),
                             Quantity = row.GetValue<int>("TOTAL_PIECES")
                         }).Parameter("LOCATION_ID",cartonArea.LocationId)
                         .Parameter("STORAGE_AREA",cartonArea.Id)
                         .ExecuteSingle(_db);


            Assert.IsNotNull(expectedLocationInfo, "No info found for location");

            var actualLocationInfo = (IEnumerable<Location>)target.GetLocations(filterData);
            Assert.IsNotNull(actualLocationInfo, "No info found for location by repositories function");

            foreach (var item in actualLocationInfo)
            {
                Assert.AreEqual(expectedLocationInfo.CartonCount, item.CartonCount);
                Assert.AreEqual(expectedLocationInfo.LocationId, item.LocationId);
                Assert.AreEqual(expectedLocationInfo.MaxCartons, item.MaxAssignedCarton);
                Assert.AreEqual(expectedLocationInfo.PalletCount, item.PalletCount);
                Assert.AreEqual(expectedLocationInfo.Quantity, item.TotalPieces);
                if (expectedLocationInfo.SkuId != null)
                {
                    Assert.AreEqual(expectedLocationInfo.SkuId, item.AssignedSku.SkuId);
                    Assert.AreEqual(expectedLocationInfo.Style, item.AssignedSku.Style);
                    Assert.AreEqual(expectedLocationInfo.Color, item.AssignedSku.Color);
                    Assert.AreEqual(expectedLocationInfo.Dimension, item.AssignedSku.Dimension);
                    Assert.AreEqual(expectedLocationInfo.Size, item.AssignedSku.SkuSize);
                    Assert.AreEqual(expectedLocationInfo.UPC, item.AssignedSku.UpcCode);
                }
            }
        }


        /// <summary>
        /// This function is verifying repositories function AssignSkuToLocation() that accepts a LocationId ,Sku ID and Max Cartons 
        /// as parameter and update information against that locationId.
        /// Here we fetched a random locationId from master_storage_location and then a random sku from master_sku table.
        /// then we called a repository function AssignSkuToLocation() and pass fetched location , fetched sku and max carton as parameter.
        /// Then we query repository for tfetched location and checked updation made for that location Id         
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void AssignSkuToLocationTest()
        {
            var expectedlocation = SqlBinder.Create(@"
                <![CDATA[
               WITH Q AS
             (SELECT MSL.LOCATION_ID            AS LOCATION_ID
                FROM MASTER_STORAGE_LOCATION MSL
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON MSL.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG = 'Y'
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                ", row => new
                 {
                     Id = row.GetValue<string>("LOCATION_ID")
                 }).ExecuteSingle(_db);


            if (expectedlocation == null)
            {
                Assert.Inconclusive("No Location Found");
            }



             var expectedSku = SqlBinder.Create(@"
                <![CDATA[
                    WITH Q AS
                     (SELECT MSKU.SKU_ID AS SKU_ID
                        FROM MASTER_SKU MSKU
                       ORDER BY DBMS_RANDOM.VALUE)
                    SELECT * FROM Q WHERE ROWNUM < 2
                        ]]>
                ", row => new
                 {
                     SkuId = row.GetValue<int>("SKU_ID")
                 }).ExecuteSingle(_db);

             if (expectedSku == null)
             {
                 Assert.Inconclusive("No Sku Found");
             }

             var expectedVwh = SqlBinder.Create(@"
                <![CDATA[
                    WITH Q AS
                     (SELECT TVW.VWH_ID AS VWH_ID
                        FROM TAB_VIRTUAL_WAREHOUSE TVW
                       ORDER BY DBMS_RANDOM.VALUE)
                    SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                    ",
                      row => new
                          {
                            Vwh = row.GetValue<string>("VWH_ID")
                          }).ExecuteSingle(_db);

             target.AssignSkuToLocation(expectedlocation.Id,expectedSku.SkuId,5,expectedVwh.Vwh);

             var location = SqlBinder.Create(@"
                                <![CDATA[
                                SELECT MSL.ASSIGNED_SKU_ID      AS ASSIGNED_SKU_ID,
                                       MSL.ASSIGNED_MAX_CARTONS AS ASSIGNED_MAX_CARTONS,
                                       MSL.ASSIGNED_VWH_ID AS ASSIGNED_VWH_ID
                                  FROM MASTER_STORAGE_LOCATION MSL
                                 WHERE MSL.LOCATION_ID = :LOCATION_ID
                                ]]>
                                ", row => new
                                  {
                                      AssignedSku = row.GetValue<int>("ASSIGNED_SKU_ID"),
                                      AssignedMaxCarton = row.GetValue<int>("ASSIGNED_MAX_CARTONS"),
                                      AssignedVwh = row.GetValue<string>("ASSIGNED_VWH_ID")

                                  }).Parameter("LOCATION_ID",expectedlocation.Id)
                                  .ExecuteSingle(_db);

             Assert.AreEqual(expectedSku.SkuId, location.AssignedSku,"Assigned Sku");
             Assert.AreEqual(5, location.AssignedMaxCarton,"Assigned max carton");
             Assert.AreEqual(expectedVwh.Vwh, location.AssignedVwh, "Assigned Vwh");

        }


        /// <summary>
        /// This function is verifying repositories function AssignSkuToLocation() that accepts a LocationId ,Sku ID ,Max Cartons 
        /// and Vwh Id as parameter and update information against that locationId.
        /// Here we fetched a random locationId from master_storage_location.
        /// Then we called a repository function AssignSkuToLocation() and pass fetched location ,null as sku and max carton as parameter.
        /// Then we query repository for fetched location and checked updation made for that location Id        
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void AssignNullSkuToLocationTest()
        {
            var expectedlocation = SqlBinder.Create(@"
                <![CDATA[
               WITH Q AS
             (SELECT MSL.LOCATION_ID            AS LOCATION_ID
                FROM MASTER_STORAGE_LOCATION MSL
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON MSL.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG = 'Y'
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                ", row => new
                 {
                     Id = row.GetValue<string>("LOCATION_ID")
                 }).ExecuteSingle(_db);

            if (expectedlocation == null)
            {
                Assert.Inconclusive("No location Found");
            }

             var expectedVwh = SqlBinder.Create(@"
                <![CDATA[
                    WITH Q AS
                     (SELECT TVW.VWH_ID AS VWH_ID
                        FROM TAB_VIRTUAL_WAREHOUSE TVW
                       ORDER BY DBMS_RANDOM.VALUE)
                    SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                    ",
                      row => new
                          {
                            Vwh = row.GetValue<string>("VWH_ID")
                          }).ExecuteSingle(_db);
            
            target.AssignSkuToLocation(expectedlocation.Id, null, 5,expectedVwh.Vwh);

            var location = SqlBinder.Create(@"
                                <![CDATA[
                                SELECT MSL.ASSIGNED_SKU_ID      AS ASSIGNED_SKU_ID,
                                       MSL.ASSIGNED_MAX_CARTONS AS ASSIGNED_MAX_CARTONS,
                                       MSL.ASSIGNED_VWH_ID AS ASSIGNED_VWH_ID
                                  FROM MASTER_STORAGE_LOCATION MSL
                                 WHERE MSL.LOCATION_ID = :LOCATION_ID
                                ]]>
                                ", row => new
                                 {
                                     AssignedSku = row.GetValue<int?>("ASSIGNED_SKU_ID"),
                                     AssignedMaxCarton = row.GetValue<int>("ASSIGNED_MAX_CARTONS"),
                                     AssignedVwh = row.GetValue<string>("ASSIGNED_VWH_ID")
                                 }).Parameter("LOCATION_ID", expectedlocation.Id)
                                 .ExecuteSingle(_db);

            Assert.IsNull(location.AssignedSku, "Assigned Sku");
            Assert.AreEqual(5, location.AssignedMaxCarton, "Assigned max carton");
            Assert.AreEqual(expectedVwh.Vwh, location.AssignedVwh, "Assigned Vwh");
        }

     
        
        /// <summary>
        /// This function is verifying repositories function AssignSkuToLocation() that accepts a LocationId ,Sku ID,Max Cartons 
        /// and VwhId as parameter and update information against that locationId.
        /// Here we fetched a random locationId from master_storage_location and then a random sku from master_sku table.
        /// then we called a repository function AssignSkuToLocation() and fetched location , fetched sku and null as max carton as parameter.
        /// Then we query repository for fetched location and checked updation made for that location Id        
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void AssignNullCartonToLocationTest()
        {
            var expectedlocation = SqlBinder.Create(@"
                <![CDATA[
               WITH Q AS
             (SELECT MSL.LOCATION_ID            AS LOCATION_ID
                FROM MASTER_STORAGE_LOCATION MSL
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON MSL.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG = 'Y'
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                ", row => new
                 {
                     Id = row.GetValue<string>("LOCATION_ID")
                 }).ExecuteSingle(_db);
            
            if (expectedlocation == null)
            {
                Assert.Inconclusive("No location Found");
            }


            var expectedSku = SqlBinder.Create(@"
                <![CDATA[
                    WITH Q AS
                     (SELECT MSKU.SKU_ID AS SKU_ID
                        FROM MASTER_SKU MSKU
                       ORDER BY DBMS_RANDOM.VALUE)
                    SELECT * FROM Q WHERE ROWNUM < 2
                        ]]>
                ", row => new
                 {
                     SkuId = row.GetValue<int>("SKU_ID")
                 }).ExecuteSingle(_db);

            if (expectedSku == null)
            {
                Assert.Inconclusive("No Sku Found");
            }

            var expectedVwh = SqlBinder.Create(@"
                <![CDATA[
                    WITH Q AS
                     (SELECT TVW.VWH_ID AS VWH_ID
                        FROM TAB_VIRTUAL_WAREHOUSE TVW
                       ORDER BY DBMS_RANDOM.VALUE)
                    SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                    ",
                     row => new
                     {
                         Vwh = row.GetValue<string>("VWH_ID")
                     }).ExecuteSingle(_db);

            target.AssignSkuToLocation(expectedlocation.Id, expectedSku.SkuId, null,expectedVwh.Vwh);

            var location = SqlBinder.Create(@"
                                <![CDATA[
                                SELECT MSL.ASSIGNED_SKU_ID      AS ASSIGNED_SKU_ID,
                                       MSL.ASSIGNED_MAX_CARTONS AS ASSIGNED_MAX_CARTONS,
                                       MSL.ASSIGNED_VWH_ID      AS ASSIGNED_VWH_ID
                                  FROM MASTER_STORAGE_LOCATION MSL
                                 WHERE MSL.LOCATION_ID = :LOCATION_ID
                                ]]>
                                ", row => new
                                 {
                                     AssignedSku = row.GetValue<int>("ASSIGNED_SKU_ID"),
                                     AssignedMaxCarton = row.GetValue<int?>("ASSIGNED_MAX_CARTONS"),
                                     AssignedVwh = row.GetValue<string>("ASSIGNED_VWH_ID")

                                 }).Parameter("LOCATION_ID", expectedlocation.Id)
                                 .ExecuteSingle(_db);

            Assert.AreEqual(expectedSku.SkuId, location.AssignedSku, "Assigned Sku");
            Assert.IsNull(location.AssignedMaxCarton, "Assigned max carton");
            Assert.AreEqual(expectedVwh.Vwh,location.AssignedVwh , "Assigned Vwh");
            
        }

        /// <summary>
        /// This function is verifying repositories function AssignSkuToLocation() that accepts a LocationId ,Sku ID Max Cartons 
        /// and Vwh Id as parameter and update information against that locationId.
        /// Here we fetched a random locationId from master_storage_location .
        /// then we called a repository function AssignSkuToLocation() and fetched location ,null as sku and null as max carton as parameter.
        /// Then we query repository for fetched location and checked updation made for that location Id        
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")]
        public void AssignNullToLocationTest()
        {
            var expectedlocation = SqlBinder.Create(@"
                <![CDATA[
               WITH Q AS
             (SELECT MSL.LOCATION_ID            AS LOCATION_ID
                FROM MASTER_STORAGE_LOCATION MSL
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON MSL.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG = 'Y'
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                ", row => new
                 {
                     Id = row.GetValue<string>("LOCATION_ID")
                 }).ExecuteSingle(_db);

            if (expectedlocation == null)
            {
                Assert.Inconclusive("No location Found");
            }

            target.AssignSkuToLocation(expectedlocation.Id, null , null,null);

            var location = SqlBinder.Create(@"
                                <![CDATA[
                                SELECT MSL.ASSIGNED_SKU_ID      AS ASSIGNED_SKU_ID,
                                       MSL.ASSIGNED_MAX_CARTONS AS ASSIGNED_MAX_CARTONS,
                                       MSL.ASSIGNED_VWH_ID      AS ASSIGNED_VWH_ID
                                  FROM MASTER_STORAGE_LOCATION MSL
                                 WHERE MSL.LOCATION_ID = :LOCATION_ID
                                ]]>
                                ", row => new
                                 {
                                     AssignedSku = row.GetValue<int?>("ASSIGNED_SKU_ID"),
                                     AssignedMaxCarton = row.GetValue<int?>("ASSIGNED_MAX_CARTONS"),
                                     AssignedVwh = row.GetValue<string>("ASSIGNED_VWH_ID")
                                 }).Parameter("LOCATION_ID", expectedlocation.Id)
                                 .ExecuteSingle(_db);

            Assert.IsNull(location.AssignedSku, "Assigned Sku");
            Assert.IsNull(location.AssignedMaxCarton, "Assigned max carton");
            Assert.IsTrue(string.IsNullOrEmpty(location.AssignedVwh), "Assigned Vwh");

        }


        //invalid

        /// <summary>
        /// This function is verifying repositories function AssignSkuToLocation() that accepts a LocationId ,Sku ID ,Max Cartons 
        /// and Vwh Id as parameter and update information against that locationId.
        /// Here we fetched a random locationId from master_storage_location .
        /// then we called a repository function AssignSkuToLocation() and fetched location ,an invalid number skuId and null as max carton as parameter.
        /// Then we query repository for fetched location and checked updation made for that location Id        
        /// </summary>
        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("Database")] 
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void AssignInvalidSkuToLocationTest()
        {
            var expectedlocation = SqlBinder.Create(@"
                <![CDATA[
               WITH Q AS
             (SELECT MSL.LOCATION_ID            AS LOCATION_ID
                FROM MASTER_STORAGE_LOCATION MSL
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON MSL.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG = 'Y'
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                ", row => new
                 {
                     Id = row.GetValue<string>("LOCATION_ID")
                 }).ExecuteSingle(_db);

            if (expectedlocation == null)
            {
                Assert.Inconclusive("No location Found");
            }


            var expectedVwh = SqlBinder.Create(@"
                <![CDATA[
                    WITH Q AS
                     (SELECT TVW.VWH_ID AS VWH_ID
                        FROM TAB_VIRTUAL_WAREHOUSE TVW
                       ORDER BY DBMS_RANDOM.VALUE)
                    SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                    ",
                     row => new
                     {
                         Vwh = row.GetValue<string>("VWH_ID")
                     }).ExecuteSingle(_db);

            target.AssignSkuToLocation(expectedlocation.Id, -1, 5,expectedVwh.Vwh);  
            
        }


        [TestMethod]
        [Owner("Ankit")]
        [TestCategory("DataBase")]
        public void AssignInvalidVwhToLocationTest()
        {
            var expectedlocation = SqlBinder.Create(@"
                <![CDATA[
               WITH Q AS
             (SELECT MSL.LOCATION_ID            AS LOCATION_ID
                FROM MASTER_STORAGE_LOCATION MSL
               INNER JOIN TAB_INVENTORY_AREA TIA
                  ON MSL.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
               WHERE TIA.STORES_WHAT = 'CTN'
                 AND TIA.LOCATION_NUMBERING_FLAG = 'Y'
               ORDER BY DBMS_RANDOM.VALUE)
            SELECT * FROM Q WHERE ROWNUM < 2
                ]]>
                ", row => new
                 {
                     Id = row.GetValue<string>("LOCATION_ID")
                 }).ExecuteSingle(_db);

            if (expectedlocation == null)
            {
                Assert.Inconclusive("No location Found");
            }


            var expectedSku = SqlBinder.Create(@"
                <![CDATA[
                    WITH Q AS
                     (SELECT MSKU.SKU_ID AS SKU_ID
                        FROM MASTER_SKU MSKU
                       ORDER BY DBMS_RANDOM.VALUE)
                    SELECT * FROM Q WHERE ROWNUM < 2
                        ]]>
                ", row => new
                 {
                     SkuId = row.GetValue<int>("SKU_ID")
                 }).ExecuteSingle(_db);

            if (expectedSku == null)
            {
                Assert.Inconclusive("No Sku Found");
            }


            target.AssignSkuToLocation(expectedlocation.Id, expectedSku.SkuId, 5,"abxyz");

        }
    }

}
