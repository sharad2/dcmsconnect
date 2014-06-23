using System.Data.Common;
using DcmsMobile.Receiving.Models.Rad;
using DcmsMobile.Receiving.Repository;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Receiving.Tests.Repository_Tests
{


    /// <summary>
    ///This is a test class for RadRepositoryTest and is intended
    ///to contain all RadRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RadRepositoryTest
    {
        public RadRepositoryTest()
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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
        }
        //
        private RadRepository _target;
        private DbTransaction _trans;
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            var dbuser = new OracleDatastore(null);
            dbuser.CreateConnection("Data Source=w8bhutan/mfdev;Persist Security Info=True;User Id=rad;Password=rad", "rad");
            _target = new RadRepository(dbuser);
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
        ///Valid Test:Test for assertion of count of SKU disposition in list. 
        ///</summary>
        [TestMethod]
        [TestCategory("Database")]
        public void Repository_GetValidSkuDisposition()
        {
            const string QUERY_EXPECTED = @"
            select count(*) As SkuCount
                  from TAB_STYLE_DISPOSITION t
                where t.disposition is not null
";
            var binderget = new SqlBinder<int>("Repository_GetValidSkuDispos");
            binderget.CreateMapper(QUERY_EXPECTED);
            var skucount = binderget.ExecuteSingle(_target.Db);
            //act
            var actual = _target.GetSkuDispositions();
            Assert.AreEqual(skucount, actual.Count, "Must Be same");
        }


        /// <summary>
        ///Valid Test:passed a valid sku disposition details to  SetSkuDisposition function and assert it succesfully set.
        ///sku disposition is set successfully Only if disposition is passed to the function. 
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void Repository_SetValidSkuDisposition()
        {
            var binder = new SqlBinder<SkuDisposition>("Repository_SetValidDispos");
            binder.CreateMapper(@"
                 with q1 AS
                     (
                    select t.style       As style,
                           t.color       As color,
                           t.dimension   As dimension,
                           t.sku_size    As sku_size,
                           t.disposition As disposition
                      from TAB_STYLE_DISPOSITION t
                        where t.disposition is not null
                    order by dbms_random.value)
                    select * from q1 where rownum = 1
                    ",
config =>
{
    config.CreateMap<SkuDisposition>()
      .MapField("style", dest => dest.Style)
      .MapField("color", dest => dest.Color)
      .MapField("dimension", dest => dest.Dimension)
      .MapField("sku_size", dest => dest.SkuSize)
      .MapField("disposition", dest => dest.Disposition);
});
            var ExpectedDisposition = _target.Db.ExecuteSingle(binder);

            if (ExpectedDisposition == null)
            {
                Assert.Inconclusive("No Data Found");
            }
            //Act
            _target.SetSkuDisposition(ExpectedDisposition);

            //assert
            var actualdisposition = SqlBinder.Create(@"
            select t.style       As skustyle,
                   t.color       As skucolor,
                   t.dimension   As skudimension,
                   t.sku_size    As skusku_size,
                   t.disposition As skudisposition
              from TAB_STYLE_DISPOSITION t
             where t.disposition = :disposition
               and t.style = :style
            <if>and t.color = :color</if>
            <else>and t.color is null</else>
            <if>and t.dimension = :dimension</if>
            <else>and t.dimension is null</else>
            <if>and t.sku_size = :sku_size</if>
            <else>and t.sku_size is null</else>
", row => new
 {
     Style = row.GetValue<string>("skustyle"),
     Color = row.GetValue<string>("skucolor"),
     Dimension = row.GetValue<string>("skudimension"),
     SkuSize = row.GetValue<string>("skusku_size"),
     disposition = row.GetValue<string>("skudisposition")
 }).Parameter("style", ExpectedDisposition.Style)
 .Parameter("color", ExpectedDisposition.Color)
 .Parameter("dimension", ExpectedDisposition.Dimension)
 .Parameter("sku_size", ExpectedDisposition.SkuSize)
 .Parameter("disposition", ExpectedDisposition.Disposition)
 .ExecuteSingle(_target.Db);
            Assert.AreEqual(ExpectedDisposition.Style, actualdisposition.Style, "Style");
            Assert.AreEqual(ExpectedDisposition.Color, actualdisposition.Color, "Color");
            Assert.AreEqual(ExpectedDisposition.Dimension, actualdisposition.Dimension, "Dimension");
            Assert.AreEqual(ExpectedDisposition.SkuSize, actualdisposition.SkuSize, "SkuSize");
            Assert.AreEqual(ExpectedDisposition.Disposition, actualdisposition.disposition, "Disposition");
        }


        /// <summary>
        /// TestValid : Testing the update operation on sku disposition.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void Repository_EditSkuDisposition()
        {
            var binder = new SqlBinder<SkuDisposition>("Repository_EditSkuDisposition");
            binder.CreateMapper(@"
                     with q1 AS
                     (
                    select t.style       As style,
                           t.color       As color,
                           t.dimension   As dimension,
                           t.sku_size    As sku_size
                      from TAB_STYLE_DISPOSITION t
                    order by dbms_random.value)
                    select * from q1 where rownum = 1
",
             config =>
             {
                 config.CreateMap<SkuDisposition>()
                   .MapField("style", dest => dest.Style)
                   .MapField("color", dest => dest.Color)
                   .MapField("dimension", dest => dest.Dimension)
                   .MapField("sku_size", dest => dest.SkuSize)
                   ;
             });
            var ExpectedDisposition = _target.Db.ExecuteSingle(binder);

            if (ExpectedDisposition == null)
            {
                Assert.Inconclusive("No Data Found");
            }

            ExpectedDisposition.Disposition = "AS";
            _target.SetSkuDisposition(ExpectedDisposition);
            var actualupdated = SqlBinder.Create(@"
                select t.style       As Astyle,
                       t.color       As Acolor,
                       t.dimension   As Adimension,
                       t.sku_size    As Askusize,
                       t.disposition As Adisposition
                  from tab_style_disposition t
               where t.style = :style
            <if>and t.color = :color</if>
            <else>and t.color is null</else>
            <if>and t.dimension = :dimension</if>
            <else>and t.dimension is null</else>
            <if>and t.sku_size = :sku_size</if>
            <else>and t.sku_size is null</else>
                ", row => new
                    {
                        Style = row.GetValue<string>("Astyle"),
                        Color = row.GetValue<string>("Acolor"),
                        Dimension = row.GetValue<string>("Adimension"),
                        SkuSize = row.GetValue<string>("Askusize"),
                        Disposition = row.GetValue<string>("Adisposition")
                    }).Parameter("style", ExpectedDisposition.Style)
                      .Parameter("color", ExpectedDisposition.Color)
                      .Parameter("dimension", ExpectedDisposition.Dimension)
                      .Parameter("sku_size", ExpectedDisposition.SkuSize)
                      .ExecuteSingle(_target.Db);
            Assert.AreEqual(ExpectedDisposition.Style, actualupdated.Style, "Style");
            Assert.AreEqual(ExpectedDisposition.Color, actualupdated.Color, "Color");
            Assert.AreEqual(ExpectedDisposition.Dimension, actualupdated.Dimension, "Dimension");
            Assert.AreEqual(ExpectedDisposition.SkuSize, actualupdated.SkuSize, "SkuSize");
            Assert.AreEqual(ExpectedDisposition.Disposition, actualupdated.Disposition, "Disposition");

        }

        /// <summary>
        /// Valid Test: Delete a valid passed disposition. 
        /// Sku disposition must be deleted only if passed disposition is null.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void Repository_DeleteDisposition()
        {
            var binder = new SqlBinder<SkuDisposition>("Repository_DeleteDisposition");
            binder.CreateMapper(@"
                          with q1 AS
                            (
                    select t.style       As style,
                           t.color       As color,
                           t.dimension   As dimension,
                           t.sku_size    As sku_size
                      from TAB_STYLE_DISPOSITION t
                    order by dbms_random.value)
                    select * from q1 where rownum = 1
",
             config => config.CreateMap<SkuDisposition>()
                           .MapField("style", dest => dest.Style)
                           .MapField("color", dest => dest.Color)
                           .MapField("dimension", dest => dest.Dimension)
                           .MapField("sku_size", dest => dest.SkuSize));
            var expectedDisposition = _target.Db.ExecuteSingle(binder);
            if (expectedDisposition == null)
            {
                Assert.Inconclusive("No Data found");
            }
            _target.SetSkuDisposition(expectedDisposition);

            const string QUERY_DELETED = @"
               select count(*) As Count
                 from tab_style_disposition t
                  where t.style = :style
            <if>and t.color = :color</if>
            <else>and t.color is null</else>
            <if>and t.dimension = :dimension</if>
            <else>and t.dimension is null</else>
            <if>and t.sku_size = :sku_size</if>
            <else>and t.sku_size is null</else>
";
            var deletedBinder = new SqlBinder<int>("Repository_DeleteDisposition");
            deletedBinder.CreateMapper(QUERY_DELETED);
            deletedBinder.Parameter("style", expectedDisposition.Style)
             .Parameter("color", expectedDisposition.Color)
             .Parameter("dimension", expectedDisposition.Dimension)
             .Parameter("sku_size", expectedDisposition.SkuSize);
           var deletedCount = deletedBinder.ExecuteSingle(_target.Db);

           Assert.IsTrue(deletedCount == 0, "Disposition must be Zero");
        }

        /// <summary>
        ///ValidTest: Test for Getting the valid list of spot Check Percentage.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void Repository_GetSpotCheck()
        {
            var binder = new SqlBinder<int>("Repository_GetSpotCheck");
            binder.CreateMapper("Select Count(*) As Count from master_sewingplant_style ms");
            var expectedspotchecklist = _target.Db.ExecuteSingle(binder);
            var actualspotchecklist = _target.GetSpotCheckList();
            Assert.AreEqual(expectedspotchecklist, actualspotchecklist.Count, "Must be same");
        }

        /// <summary>
        /// Valid Test:passed a valid Spot check details to SetSpotCheckPercentage function and assert it succesfully set.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void Repository_AddSpotCheck()
        {
            var binder = new SqlBinder<SpotCheckConfiguration>("Repository_AddSpotCheck");
            binder.CreateMapper(@"
                                    with q1 AS
                   (
                select ms.style            As style,
                       ms.spotcheck_percent As spotcheck_percent,
                       tb.sewing_plant_name As sewing_plant_name,
                       ms.sewing_plant_code As sewing_plant_code
                  from master_sewingplant_style ms
                 inner join tab_sewingplant tb
                    on tb.sewing_plant_code = ms.sewing_plant_code
                 order by dbms_random.value)
                 select * from q1 where rownum = 1
                ",
                    config =>
                    {
                        config.CreateMap<SpotCheckConfiguration>()
                       .MapField("style", dest => dest.Style)
                       .MapField("spotcheck_percent", dest => dest.SpotCheckPercent)
                       .MapField("sewing_plant_name", dest => dest.PlantName)
                       .MapField("sewing_plant_code", dest => dest.SewingPlantId)
                       ;
                    });
            var expectedSpotCheckPercent = _target.Db.ExecuteSingle(binder);
            //Act
            _target.SetSpotCheckPercentage(expectedSpotCheckPercent);
            //Assert
            var actualspotcheck = SqlBinder.Create(@"
                    select ms.style             As style,
                           ms.spotcheck_percent As spotcheck_percent,
                           tb.sewing_plant_name As sewing_plant_name
                      from master_sewingplant_style ms
                     inner join tab_sewingplant tb
                        on tb.sewing_plant_code = ms.sewing_plant_code
                     where ms.style = :style
                       and tb.sewing_plant_name = :sewing_plant_name
                       and ms.spotcheck_percent = :spotcheck_percent
            ", row => new
                {
                    Style = row.GetValue<string>("style"),
                    SpotCheckPercent = row.GetValue<int>("spotcheck_percent"),
                    SewingPlantName = row.GetValue<string>("sewing_plant_name")
                }).Parameter("style", expectedSpotCheckPercent.Style)
                .Parameter("sewing_plant_name", expectedSpotCheckPercent.PlantName)
                .Parameter("spotcheck_percent", expectedSpotCheckPercent.SpotCheckPercent)
                .ExecuteSingle(_target.Db);
            Assert.AreEqual(expectedSpotCheckPercent.Style, actualspotcheck.Style, "Style");
            Assert.AreEqual(expectedSpotCheckPercent.SpotCheckPercent, actualspotcheck.SpotCheckPercent, "SpotCheckPercent");
            Assert.AreEqual(expectedSpotCheckPercent.PlantName, actualspotcheck.SewingPlantName, "Plant Name");
        }


        /// <summary>
        ///  TestValid : Test after changing Spot check percentage,check details related to that sku is same or not
        /// Assert Successfully edit Spotcheck percentage.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void Repository_EditSpotCheckPercentage()
        {
            var binder = new SqlBinder<SpotCheckConfiguration>("EditSpotCheckPercentage");
            binder.CreateMapper(@"
                                        with q1 AS
                    (
                        select ms.style         As style,
                           ms.spotcheck_percent As spotcheck_percent,
                           tb.sewing_plant_name As sewing_plant_name,
                           ms.sewing_plant_code As sewing_plant_code
                      from master_sewingplant_style ms
                     inner join tab_sewingplant tb
                        on tb.sewing_plant_code = ms.sewing_plant_code
                      order by dbms_random.value)
                      select * from q1 where rownum = 1
                ",
                    config =>
                    {
                        config.CreateMap<SpotCheckConfiguration>()
                       .MapField("style", dest => dest.Style)
                       .MapField("spotcheck_percent", dest => dest.SpotCheckPercent)
                       .MapField("sewing_plant_name", dest => dest.PlantName)
                       .MapField("sewing_plant_code", dest => dest.SewingPlantId)
                       ;
                    });
            var ExpectedSpotCheckPercent = _target.Db.ExecuteSingle(binder);
            ExpectedSpotCheckPercent.SpotCheckPercent = 45;
            _target.SetSpotCheckPercentage(ExpectedSpotCheckPercent);
            //Assert
            var actualspotcheckpercent = SqlBinder.Create(@"
                       select ms.style      As style,
                       ms.spotcheck_percent As spotcheck_percent,
                       tb.sewing_plant_name As sewing_plant_name
                  from master_sewingplant_style ms
                 inner join tab_sewingplant tb
                    on tb.sewing_plant_code = ms.sewing_plant_code
                 where ms.style = :style
                   and ms.spotcheck_percent = :spotcheck_percent
                   and tb.sewing_plant_name = :sewing_plant_name
                ", row => new
                 {
                     Style = row.GetValue<string>("style"),
                     SpotCheckPercent = row.GetValue<int>("spotcheck_percent"),
                     SewingPlantName = row.GetValue<string>("sewing_plant_name"),
                 }).Parameter("style", ExpectedSpotCheckPercent.Style)
                      .Parameter("sewing_plant_name", ExpectedSpotCheckPercent.PlantName)
                      .Parameter("spotcheck_percent", ExpectedSpotCheckPercent.SpotCheckPercent)
                      .ExecuteSingle(_target.Db);
            Assert.AreEqual(ExpectedSpotCheckPercent.Style, actualspotcheckpercent.Style, "Style");
            Assert.AreEqual(ExpectedSpotCheckPercent.PlantName, actualspotcheckpercent.SewingPlantName, "SewingPlantName");
            Assert.AreEqual(ExpectedSpotCheckPercent.SpotCheckPercent, actualspotcheckpercent.SpotCheckPercent, "SpotCheckPercent");
        }

        /// <summary>
        /// Valid Test:If passed spot check percentage is null then Delete spot check. 
        /// Assert Passed Sewing Plant must be deleted from master_sewingplant_style.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void Repository_DeleteSpotCheck()
        {
            var binder = new SqlBinder<SpotCheckConfiguration>("Repository_DeleteSpotCheck");
            binder.CreateMapper(@"
                          with q1 AS
                            (
                             select ms.style As style,
                        ms.sewing_plant_code As sewing_plant_code
                    from master_sewingplant_style ms
                   order by dbms_random.value)
                select * from q1 where rownum = 1
",
             config =>
             {
                 config.CreateMap<SpotCheckConfiguration>()
                   .MapField("style", dest => dest.Style)
                   .MapField("sewing_plant_code", dest => dest.SewingPlantId)
                   ;
             });
            var expectedSpotCheckPercent = _target.Db.ExecuteSingle(binder);
            if (expectedSpotCheckPercent == null)
            {
                Assert.Inconclusive("No Data Found");
            }
            
            expectedSpotCheckPercent.SpotCheckPercent = null;
           
            _target.SetSpotCheckPercentage(expectedSpotCheckPercent);
            
            const string QUERY_DELETE_SPOT_CHECK = @"
                    select count(*) As CountSewingPlant 
                     from MASTER_SEWINGPLANT_STYLE ms
                   where ms.style= :style
                      and ms.sewing_plant_code= :sewing_plant_code
";
            var deletedBinder = new SqlBinder<int>("Repository_DeleteSpotCheck");
            deletedBinder.CreateMapper(QUERY_DELETE_SPOT_CHECK);
            deletedBinder.Parameter("style", expectedSpotCheckPercent.Style)
                         .Parameter("sewing_plant_code", expectedSpotCheckPercent.SewingPlantId);
            
            var deletedCount = deletedBinder.ExecuteSingle(_target.Db);

            Assert.IsTrue(deletedCount == 0, "Disposition must be Zero");
        }


        /// <summary>
        /// Valid Test:Test for Get pallet list and assert its count should be same as per expected count.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void Repository_GetPalletLimit()
        {
            const string QUERY_EXPECTED = @"
            select count(*) As PalletCount
              from TAB_WAREHOUSE_LOCATION
";
            var binder = new SqlBinder<int>("Repository_GetPalletLimit");
            binder.CreateMapper(QUERY_EXPECTED);
            var expectedCount = binder.ExecuteSingle(_target.Db);
            //act
            var actual = _target.GetPalletLimitList();
            Assert.AreEqual(expectedCount, actual.Count, "Must Be same");
        }

        /// <summary>
        ///ValidTest : Test change pallet limit.
        /// Assert must be change from one pallet limit to other pallet limit .
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        public void Repository_EditPalletLimit()
        {
            var binder = new SqlBinder<ReceivingPalletLimit>("Repository_EditPalletLimit");
            binder.CreateMapper(@"
                  with q1 AS
                 (
                  SELECT TWL.WAREHOUSE_LOCATION_ID  AS WAREHOUSE_LOCATION_ID,
                         TWL.DESCRIPTION            AS DESCRIPTION,
                         TWL.RECEIVING_PALLET_LIMIT AS RECEIVING_PALLET_LIMIT
                    FROM TAB_WAREHOUSE_LOCATION TWL
                   ORDER BY DBMS_RANDOM.VALUE)
                select * from q1 where rownum = 1
                ",
                    config =>
                    {
                        config.CreateMap<ReceivingPalletLimit>()
                       .MapField("WAREHOUSE_LOCATION_ID", dest => dest.Building)
                       .MapField("DESCRIPTION", dest => dest.Description)
                       .MapField("RECEIVING_PALLET_LIMIT", dest => dest.PalletLimit)
                       ;
                    });
            var expectedSpotCheckPercent = _target.Db.ExecuteSingle(binder);
            if (expectedSpotCheckPercent == null)
            {
                Assert.Inconclusive("No data found");
            }
            expectedSpotCheckPercent.PalletLimit = 10;
            _target.SetPalletLimit(expectedSpotCheckPercent);
            //Assert
            var actualspotcheckpercent = SqlBinder.Create(@"
                        SELECT TWL.WAREHOUSE_LOCATION_ID  AS WAREHOUSE_LOCATION_ID,
                               TWL.DESCRIPTION            AS DESCRIPTION,
                               TWL.RECEIVING_PALLET_LIMIT AS RECEIVING_PALLET_LIMIT
                          FROM TAB_WAREHOUSE_LOCATION TWL
                         WHERE TWL.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID
                           AND TWL.DESCRIPTION = :DESCRIPTION

                ", row => new
                 {
                     LocationId = row.GetValue<string>("WAREHOUSE_LOCATION_ID"),
                     Description = row.GetValue<string>("DESCRIPTION"),
                     NewPalletLimit = row.GetValue<int>("RECEIVING_PALLET_LIMIT")
                 }).Parameter("WAREHOUSE_LOCATION_ID", expectedSpotCheckPercent.Building)
                      .Parameter("DESCRIPTION", expectedSpotCheckPercent.Description)
                      .ExecuteSingle(_target.Db);
            Assert.AreEqual(expectedSpotCheckPercent.Building, actualspotcheckpercent.LocationId, "Building");
            Assert.AreEqual(expectedSpotCheckPercent.Description, actualspotcheckpercent.Description, "Description");
            Assert.AreEqual(expectedSpotCheckPercent.PalletLimit, actualspotcheckpercent.NewPalletLimit, "SpotCheckPercent");
        }


        //Invalid Test
        /// <summary>
        ///Invalid test: passed invalid sku details. 
        ///Assert throw "integrity constraint violated" (parent key not found) 
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Repository_AddInvalidSkuDisposition()
        {
            var disposition = new SkuDisposition
            {
                Style = "1234",
                Color = "AS",
                Dimension = "FD",
                Disposition = "A",
                SkuSize = "38"
            };
            _target.SetSkuDisposition(disposition);
        }


        /// <summary>
        /// Invalid Test: Passed an Invalid  disposition length and assert throw an exception "value too large for column" 
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        [Owner("Rajesh")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Repository_InvalidDisposition()
        {
            var binder = new SqlBinder<SkuDisposition>("Repository_InvalidDisposition");
            binder.CreateMapper(@"
            select t.style       As style,
                   t.color       As color,
                   t.dimension   As dimension,
                   t.sku_size    As sku_size
            from TAB_STYLE_DISPOSITION t
           where rownum &lt;2
           order by dbms_random.value
",
             config =>
             {
                 config.CreateMap<SkuDisposition>()
                   .MapField("style", dest => dest.Style)
                   .MapField("color", dest => dest.Color)
                   .MapField("dimension", dest => dest.Dimension)
                   .MapField("sku_size", dest => dest.SkuSize);
             });
            var ExpectedDisposition = _target.Db.ExecuteSingle(binder);
            if (ExpectedDisposition == null)
            {
                Assert.Inconclusive("No data found");
            }
            ExpectedDisposition.Disposition = "AASGFGHH";
            _target.SetSkuDisposition(ExpectedDisposition);
        }

    }
}
