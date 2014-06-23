using System;
using System.Data.Common;
using DcmsMobile.Receiving.Models;
using DcmsMobile.Receiving.Repository;
using EclipseLibrary.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Receiving.Tests.Repository_Tests
{
    public class ReceivableCarton : CartonBase
    {
        public int Quantity { get; set; }

        public string ShipmentId { get; set; }

        public string SewingPlantCode { get; set; }

        public static ReceivableCarton Create(IOracleDatastore2 db)
        {
            var intransitCarton = SqlBinder.Create(
                @"
<![CDATA[
with q1 AS
 (select t.style As style,
    t.color As color,
    t.quantity As quantity,
    t.dimension As dimension,
    t.sku_size As sku_size,
    t.carton_id AS carton_id,
    t.vwh_id AS vwh_id,
    t.shipment_id As shipment_id,
    t.sewing_plant_code As sewing_plant_code
    from src_carton_intransit t
   where t.received_date is null
    and rownum < 100
   order by dbms_random.value)
select * from q1 where rownum = 1
]]>
",
                row => new ReceivableCarton()
                {
                    CartonId = row.GetValue<string>("carton_id"),
                    VwhId = row.GetValue<string>("vwh_id"),
                    Sku = new Sku
                    {
                        Style = row.GetValue<string>("style"),
                        Color = row.GetValue<string>("color"),
                        Dimension = row.GetValue<string>("dimension"),
                        SkuSize = row.GetValue<string>("sku_size")
                    },
                    Quantity = row.GetValue<int>("quantity"),
                    ShipmentId = row.GetValue<string>("shipment_id"),
                    SewingPlantCode = row.GetValue<string>("sewing_plant_code")
                }).ExecuteSingle(db);
            if (intransitCarton == null)
            {
                Assert.Inconclusive("Receivable cartons not available");
            }
            return intransitCarton;
        }
    }


    /// <summary>
    /// Summary description for ReceivintRepository_Tests
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tests in this class cover all possible database scenarios.
    /// </para>
    /// <para>
    /// A valid unreceived carton (Required) in a valid area (Required), with valid process id (required) gets received. PalletId is not part of success criteria.
    /// Transactions are added to src_transaction* tables. src_carton_process* tables get proper entries.
    /// Inventory gets updated in master_raw_inventory.
    /// </para>
    /// <para>
    /// Unreceived carton transfers any carton in src_carton to src_carton_intransit and updates the same audit tables.
    /// </para>
    /// <para>
    /// Failure cases revolve around null carton, carton not in intransit, pallet not matching pattern, receiving already received carton, unreciving unreceived carton,
    /// invalid process id, invalid area. Null tests exist for all required input.
    /// </para>
    /// <para>
    /// TODO: Write a single success case for REC user. If no exception is thrown, the test passes.
    /// </para>
    /// <para>
    /// BUG: Not testing updates in IFR_ISI_INTRANSIT* tables because the philosophy is not clear.
    /// </para>
    /// </remarks>
    [TestClass]
    public class ReceivingRepository_Tests
    {
        #region Helpers
        private static int? GetRandomProcessId(IOracleDatastore2 db)
        {
            //select a random process id
            var processId = SqlBinder.Create(@"
                <![CDATA[  
                        with q1 as (
        select a.process_id As process_id
          from src_carton_process a
         where a.module_code = :MODULE_CODE
           and rownum < 100
         order by dbms_random.value
        )
        select * from q1 where rownum < 2
               
        ]]>
", row => row.GetValue<int?>("process_id")).Parameter("MODULE_CODE", Module_Code)
    .ExecuteSingle(db);
            if (processId == null)
            {
                Assert.Inconclusive("No Procees Id defined");
            }
            return processId;
        }
        #endregion

        public ReceivingRepository_Tests()
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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            //_db = new OracleDatastore(null);
            //_db.CreateConnection("Data Source=w8bhutan/mfdev;Persist Security Info=True;Proxy User Id=dcms4;Proxy Password=dcms4", "rec");

        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            //_dbSuperUser.Dispose();
        }
        //

        private ReceivingRepository _target;
        private ReceivingRepository _target1;
        private DbTransaction _trans;

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            var dbSuperUser = new OracleDatastore(null);
            dbSuperUser.CreateConnection("Data Source=w8singapore/mfdev;Persist Security Info=True;User Id=dcms8;Password=dcms8", "");
            _target = new ReceivingRepository(dbSuperUser);
            _trans = _target.Db.BeginTransaction();
            var dbUser = new OracleDatastore(null);
            dbUser.CreateConnection("Data Source=w8singapore/mfdev;Persist Security Info=True;Proxy User Id=dcms4;Proxy Password=dcms4", "dcms4");
            _target1 = new ReceivingRepository(dbUser);
            _trans = _target1.Db.BeginTransaction();
        }

        //
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
        /// Receive successfully. Select a random receivable carton from src_carton_intransit and receive it
        /// </summary>
        /// <remarks>
        /// <para>
        /// Destination area must exist but it can be anything. Pallet must begin with a P.
        /// </para>
        /// <para>
        /// Asserts: Entries in src_carton, src_carton_detail, src_carton_process_detail, src_transaction, src_transaction_detail.
        /// 
        /// </para>
        /// </remarks>
        /// 
        const string Module_Code = "REC";
        const string PALLET_ID = "P123";
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        public void Repository_ReceiveCarton_Success()
        {
            // Select a random receivable carton
            // Must begin with P
            var intransitCarton = ReceivableCarton.Create(_target.Db);

            //select Quality code
                    var quality = SqlBinder.Create(@"
        <![CDATA[
        select s.quality_code from tab_quality_code s 
        where s.default_receiving_quality='Y'
        ]]>
", row => new
 {
     QualityCode = row.GetValue<string>("quality_code")
 }).ExecuteSingle(_target1.Db);
            if (quality == null)
            {
                Assert.Inconclusive("No Quality Code assigned");
            }

            //select a random process id
            var processId = GetRandomProcessId(_target.Db);

            // Select any random area as destination of carton
            var expectedDestArea = SqlBinder.Create(@"
                <![CDATA[
                select t.inventory_storage_area AS inventory_storage_area
                  from tab_inventory_area sample(50) t
                 where rownum = 1
                ]]>
", row => new
            {
                InventoryStorageArea = row.GetValue<string>("inventory_storage_area")
            }).ExecuteSingle(_target1.Db);
            if (expectedDestArea == null)
            {
                Assert.Inconclusive("No inventory areas defined");
            }

            // Act
            _target1.ReceiveCarton(PALLET_ID, intransitCarton.CartonId, expectedDestArea.InventoryStorageArea, processId);

            // Assert
            // Retrieve the carton which was received 
            var receivedCarton = SqlBinder.Create(@"
                <![CDATA[
                select t.carton_storage_area AS carton_storage_area,
                t.pallet_id AS pallet_id,
                t.vwh_id AS vwh_id,
                t.quality_code as quality_code
                  from src_carton t
                 where t.carton_id = :carton_id
                ]]>
", row => new
            {
                CartonArea = row.GetValue<string>("carton_storage_area"),
                PalletId = row.GetValue<string>("pallet_id"),
                VwhId = row.GetValue<string>("vwh_id"),
                QualityCode = row.GetValue<string>("quality_code")
            }).Parameter("carton_id", intransitCarton.CartonId)
                .ExecuteSingle(_target1.Db);
            Assert.IsNotNull(receivedCarton, "Carton {0} was not received", intransitCarton.CartonId);
            Assert.AreEqual(expectedDestArea.InventoryStorageArea, receivedCarton.CartonArea, "Storage Area doesn't match");
            Assert.AreEqual(PALLET_ID, receivedCarton.PalletId, "Pallet Id doesn't match");
            Assert.AreEqual(intransitCarton.VwhId, receivedCarton.VwhId, "VWH_ID doesn't match");
            Assert.AreEqual(quality.QualityCode, receivedCarton.QualityCode, "QualityCode");


            //Check whether received Sku and intransit Sku are same or not in src_carton_detail and master_sku. 
            var receivedSku = SqlBinder.Create(@"
                                <![CDATA[
                select msku.style        As sku_style,
                       msku.color        As sku_color,
                       msku.dimension    As sku_dimension,
                       msku.sku_size     As sku_sku_size,
                       sc.style          As carton_style,
                       sc.color          As carton_color,
                       sc.dimension      As carton_dimension,
                       sc.sku_size       As carton_sku_size,
                       r.shipment_id      As shipment_id,
                       r.sewing_plant_code As sewing_plant_code,
                       sc.req_line_number As req_line_number,
                       sc.quantity       As quantity,
                       sc.req_module_code As req_module_code,
                       sc.req_process_id AS req_process_id
                  from src_carton_detail sc
                 inner join master_sku msku
                    on sc.sku_id = msku.sku_id
                 inner join src_carton r
                    on sc.carton_id=r.carton_id
                 where sc.carton_id = :carton_id
                                ]]>
            ", row => new
            {
                SkuStyle = row.GetValue<string>("sku_style"),
                SkuColor = row.GetValue<string>("sku_color"),
                SkuDimension = row.GetValue<string>("sku_dimension"),
                SkuSize = row.GetValue<string>("sku_sku_size"),
                CartonStyle = row.GetValue<string>("carton_style"),
                CartonColor = row.GetValue<string>("carton_color"),
                CartonDimension = row.GetValue<string>("carton_dimension"),
                CartonSkuSize = row.GetValue<string>("carton_sku_size"),
                SkuQuantity = row.GetValue<int>("quantity"),
                ReqProcessId = row.GetValue<int?>("req_process_id"),
                ReqLineNumber = row.GetValue<int?>("req_line_number"),
                ReqModuleCode = row.GetValue<string>("req_module_code"),
                ShipmentId = row.GetValue<string>("shipment_id"),
                SewingPlantCode = row.GetValue<string>("sewing_plant_code")
            }).Parameter("carton_id", intransitCarton.CartonId)
                .ExecuteSingle(_target1.Db);
            Assert.AreEqual(intransitCarton.Sku.Style, receivedSku.SkuStyle, "Style");
            Assert.AreEqual(intransitCarton.Sku.Color, receivedSku.SkuColor, "Color");
            Assert.AreEqual(intransitCarton.Sku.Dimension, receivedSku.SkuDimension, "Dimension");
            Assert.AreEqual(intransitCarton.Sku.SkuSize, receivedSku.SkuSize, "Size");
            Assert.AreEqual(intransitCarton.Sku.Style, receivedSku.CartonStyle, "Style");
            Assert.AreEqual(intransitCarton.Sku.Color, receivedSku.CartonColor, "Color");
            Assert.AreEqual(intransitCarton.Sku.Dimension, receivedSku.CartonDimension, "Dimension");
            Assert.AreEqual(intransitCarton.Sku.SkuSize, receivedSku.CartonSkuSize, "SkuSize");
            Assert.AreEqual(intransitCarton.Quantity, receivedSku.SkuQuantity, "Quantity");
            Assert.IsNull(receivedSku.ReqProcessId, "ReqProcessId");
            Assert.IsNull(receivedSku.ReqLineNumber, "ReqLineNumber");
            Assert.IsTrue(receivedSku.ReqModuleCode == "", "ReqModuleCode");
            Assert.AreEqual(intransitCarton.SewingPlantCode, receivedSku.SewingPlantCode, "SewingPlantCode");
            Assert.AreEqual(intransitCarton.ShipmentId, receivedSku.ShipmentId, "ShipmentId");

            // Check if rows are inserted in src_carton_process_detail for the received carton.
            var actualProcess = SqlBinder.Create(@"
            <![CDATA[
            select ps.to_carton_area As to_carton_area,
                  ps.to_pallet_id As to_pallet_id,
                  ps.new_carton_qty As new_carton_qty,
                  ps.vwh_id As vwh_id           
              from src_carton_process_detail ps
             where ps.carton_id = :carton_id
             and ps.process_id= :process_id
            ]]>
            ", row => new
            {
                CartonArea = row.GetValue<string>("to_carton_area"),
                ToPalletId = row.GetValue<string>("to_pallet_id"),
                NewQuantity = row.GetValue<int>("new_carton_qty"),
                VwhId = row.GetValue<string>("vwh_id")
            }).Parameter("carton_id", intransitCarton.CartonId)
            .Parameter("process_id", processId)
                .ExecuteSingle(_target1.Db);
            Assert.IsNotNull(actualProcess, "Entry in src_carton_process_detail");
            Assert.AreEqual(expectedDestArea.InventoryStorageArea, actualProcess.CartonArea, "Carton Area in src_carton_process_detail");
            Assert.AreEqual(intransitCarton.Quantity, actualProcess.NewQuantity, "NewCartonQuantity in src_carton_process_detail");
            Assert.AreEqual(PALLET_ID, actualProcess.ToPalletId, "ToPalletId in src_carton_process_detail");
            Assert.AreEqual(intransitCarton.VwhId, actualProcess.VwhId, "VwhId in src_carton_process_detail");

            //Test the trasaction in src_trasaction and src_trasaction_detail
            var actualTransaction = SqlBinder.Create(@"
                        <![CDATA[
                        WITH q1 AS (
                        select t.style As style,
                               t.color As color,
                               t.dimension As dimension,
                               t.sku_size As sku_size,
                               t.module_code As module_code,
                               t.vwh_id As vwh_id,
                               td.transaction_pieces As transaction_pieces
                          from src_transaction t
                        inner join src_transaction_detail td
                            on t.transaction_id = td.transaction_id
                        where t.carton_id = :carton_id
                        AND td.transaction_id IS NOT NULL
                        AND TD.TRANSACTION_PIECES IS NOT NULL
                        ORDER BY t.insert_date DESC)
                        SELECT * FROM q1 WHERE ROWNUM < 2
                        ]]>
                        ", row => new
                {
                    Style = row.GetValue<string>("style"),
                    Color = row.GetValue<string>("color"),
                    Dimension = row.GetValue<string>("dimension"),
                    SkuSize = row.GetValue<string>("sku_size"),
                    TransactionPieces = row.GetValue<int>("transaction_pieces"),
                    ModuleCode = row.GetValue<string>("module_code")
                }).Parameter("carton_id", intransitCarton.CartonId)
                   .ExecuteSingle(_target1.Db);
            if (actualTransaction == null)
            {
                Assert.Inconclusive("No Transaction Recieved in src_trasaction ");
            }
            Assert.AreEqual(intransitCarton.Sku.Style, actualTransaction.Style, "Style");
            Assert.AreEqual(intransitCarton.Sku.Color, actualTransaction.Color, "Color");
            Assert.AreEqual(intransitCarton.Sku.Dimension, actualTransaction.Dimension, "dimension");
            Assert.AreEqual(intransitCarton.Sku.SkuSize, actualTransaction.SkuSize, "Size");
            Assert.AreEqual(intransitCarton.Quantity, actualTransaction.TransactionPieces, "Quantity doesn't match for carton {0}", intransitCarton.CartonId);
            Assert.AreEqual(Module_Code, actualTransaction.ModuleCode, "Module Code");
        }



        /// <summary>
        ///Valid Test:Receive carton can have null pallet ID.  We will pass all the parameters except pallet id
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        public void Repository_ReceiveWithNullPallet()
        {
            // Selecting a valid random carton from src_carton_intransit which can be received.
            var intransitCarton = ReceivableCarton.Create(_target.Db);
            //select Quality code
            var quality = SqlBinder.Create(@"
            <![CDATA[
            select s.quality_code from tab_quality_code s 
            where s.default_receiving_quality='Y'
            ]]>
", row => new
 {
     QualityCode = row.GetValue<string>("quality_code")
 }).ExecuteSingle(_target.Db);
            if (quality == null)
            {
                Assert.Inconclusive("No Quality Code assigned");
            }
            // Select any random area as destination of carton
            var expectedDestArea = SqlBinder.Create(@"
            <![CDATA[
            select t.inventory_storage_area AS inventory_storage_area
              from tab_inventory_area sample(50) t
             where rownum = 1
            ]]>
", row => new
 {
     InventoryStorageArea = row.GetValue<string>("inventory_storage_area")
 }).ExecuteSingle(_target.Db);
            if (expectedDestArea == null)
            {
                Assert.Inconclusive("No inventory areas defined");
            }
            //select a random proess id
            var processId = GetRandomProcessId(_target1.Db);

            //Act
            _target1.ReceiveCarton(null, intransitCarton.CartonId, expectedDestArea.InventoryStorageArea, processId);

            //Assert
            // Retrieve the carton in src_carton which was received 
            var receivedCarton = SqlBinder.Create(@"
            <![CDATA[
            select t.carton_storage_area AS carton_storage_area,
            t.vwh_id AS vwh_id,
            t.pallet_id As pallet_id,
            t.quality_code as quality_code
              from src_carton t
             where t.carton_id = :carton_id
            ]]>
", row => new
 {
     CartonArea = row.GetValue<string>("carton_storage_area"),
     VwhId = row.GetValue<string>("vwh_id"),
     QualityCode = row.GetValue<string>("quality_code"),
     Pallet_id = row.GetValue<string>("pallet_id")
 }).Parameter("carton_id", intransitCarton.CartonId)
               .ExecuteSingle(_target1.Db);
            Assert.IsNotNull(receivedCarton, "Carton {0} was not received", intransitCarton.CartonId);
            Assert.AreEqual(expectedDestArea.InventoryStorageArea, receivedCarton.CartonArea, "Storage Area doesn't match");
            Assert.AreEqual(intransitCarton.VwhId, receivedCarton.VwhId, "VWH_ID doesn't match");
            Assert.AreEqual(quality.QualityCode, receivedCarton.QualityCode, "QualityCode");
            Assert.IsTrue(string.IsNullOrEmpty(receivedCarton.Pallet_id), "Pallet_Id");
            //Check whether received Sku and intransit Sku are same or not in src_carton_detail and master_sku. 
            var receivedSku = SqlBinder.Create(@"
                            <![CDATA[
            select msku.style        As sku_style,
                   msku.color        As sku_color,
                   msku.dimension    As sku_dimension,
                   msku.sku_size     As sku_sku_size,
                   sc.style          As carton_style,
                   sc.color          As carton_color,
                   sc.dimension      As carton_dimension,
                   sc.sku_size       As carton_sku_size,
                   r.shipment_id      As shipment_id,
                   r.sewing_plant_code As sewing_plant_code,
                   sc.req_line_number As req_line_number,
                   sc.quantity       As quantity,
                   sc.req_module_code As req_module_code,
                   sc.req_process_id AS req_process_id
              from src_carton_detail sc
             inner join master_sku msku
                on sc.sku_id = msku.sku_id
             inner join src_carton r
                on sc.carton_id=r.carton_id
             where sc.carton_id = :carton_id
                            ]]>
            ", row => new
             {
                 SkuStyle = row.GetValue<string>("sku_style"),
                 SkuColor = row.GetValue<string>("sku_color"),
                 SkuDimension = row.GetValue<string>("sku_dimension"),
                 SkuSize = row.GetValue<string>("sku_sku_size"),
                 CartonStyle = row.GetValue<string>("carton_style"),
                 CartonColor = row.GetValue<string>("carton_color"),
                 CartonDimension = row.GetValue<string>("carton_dimension"),
                 CartonSkuSize = row.GetValue<string>("carton_sku_size"),
                 SkuQuantity = row.GetValue<int>("quantity"),
                 ReqProcessId = row.GetValue<int?>("req_process_id"),
                 ReqLineNumber = row.GetValue<int?>("req_line_number"),
                 ReqModuleCode = row.GetValue<string>("req_module_code"),
                 ShipmentId = row.GetValue<string>("shipment_id"),
                 SewingPlantCode = row.GetValue<string>("sewing_plant_code")
             }).Parameter("carton_id", intransitCarton.CartonId)
                .ExecuteSingle(_target1.Db);
            Assert.AreEqual(intransitCarton.Sku.Style, receivedSku.SkuStyle, "Style");
            Assert.AreEqual(intransitCarton.Sku.Color, receivedSku.SkuColor, "Color");
            Assert.AreEqual(intransitCarton.Sku.Dimension, receivedSku.SkuDimension, "Dimension");
            Assert.AreEqual(intransitCarton.Sku.SkuSize, receivedSku.SkuSize, "Size");
            Assert.AreEqual(intransitCarton.Sku.Style, receivedSku.CartonStyle, "Style");
            Assert.AreEqual(intransitCarton.Sku.Color, receivedSku.CartonColor, "Color");
            Assert.AreEqual(intransitCarton.Sku.Dimension, receivedSku.CartonDimension, "Dimension");
            Assert.AreEqual(intransitCarton.Sku.SkuSize, receivedSku.CartonSkuSize, "SkuSize");
            Assert.AreEqual(intransitCarton.Quantity, receivedSku.SkuQuantity, "Quantity");
            Assert.IsNull(receivedSku.ReqProcessId, "ReqProcessId");
            Assert.IsNull(receivedSku.ReqLineNumber, "ReqLineNumber");
            Assert.IsTrue(receivedSku.ReqModuleCode == "", "ReqModuleCode");
            Assert.AreEqual(intransitCarton.SewingPlantCode, receivedSku.SewingPlantCode, "SewingPlantCode");
            Assert.AreEqual(intransitCarton.ShipmentId, receivedSku.ShipmentId, "ShipmentId");


            // Check if rows are inserted in src_carton_process_detail for the received carton.
            var actualProcess = SqlBinder.Create(@"
            <![CDATA[
            select ps.to_carton_area As to_carton_area,
                  ps.to_pallet_id As to_pallet_id,
                  ps.new_carton_qty As new_carton_qty,
                  ps.vwh_id As vwh_id           
              from src_carton_process_detail ps
             where ps.carton_id = :carton_id
             and ps.process_id= :process_id
            ]]>
            ", row => new
             {
                 CartonArea = row.GetValue<string>("to_carton_area"),
                 ToPalletId = row.GetValue<string>("to_pallet_id"),
                 NewQuantity = row.GetValue<int>("new_carton_qty"),
                 VwhId = row.GetValue<string>("vwh_id")
             }).Parameter("carton_id", intransitCarton.CartonId)
            .Parameter("process_id", processId)
                .ExecuteSingle(_target1.Db);
            Assert.IsNotNull(actualProcess, "Entry in src_carton_process_detail");
            Assert.AreEqual(expectedDestArea.InventoryStorageArea, actualProcess.CartonArea, "Carton Area in src_carton_process_detail");
            Assert.AreEqual(intransitCarton.Quantity, actualProcess.NewQuantity, "NewCartonQuantity in src_carton_process_detail");
            Assert.IsTrue(string.IsNullOrEmpty(actualProcess.ToPalletId), "Pallet_id");
            Assert.AreEqual(intransitCarton.VwhId, actualProcess.VwhId, "VwhId in src_carton_process_detail");

            //Test the trasaction in src_trasaction and src_trasaction_detail
            var actualTransaction = SqlBinder.Create(@"
                        <![CDATA[
                        WITH q1 AS (
                        select t.style As style,
                               t.color As color,
                               t.dimension As dimension,
                               t.sku_size As sku_size,
                               t.module_code As module_code,
                               t.vwh_id As vwh_id,
                               td.transaction_pieces As transaction_pieces
                          from src_transaction t
                        inner join src_transaction_detail td
                            on t.transaction_id = td.transaction_id
                        where t.carton_id = :carton_id
                        AND td.transaction_id IS NOT NULL
                        AND TD.TRANSACTION_PIECES IS NOT NULL
                        ORDER BY t.insert_date DESC)
                        SELECT * FROM q1 WHERE ROWNUM < 2
                        ]]>
                        ", row => new
                         {
                             Style = row.GetValue<string>("style"),
                             Color = row.GetValue<string>("color"),
                             Dimension = row.GetValue<string>("dimension"),
                             SkuSize = row.GetValue<string>("sku_size"),
                             TransactionPieces = row.GetValue<int>("transaction_pieces"),
                             ModuleCode = row.GetValue<string>("module_code")
                         }).Parameter("carton_id", intransitCarton.CartonId)
                   .ExecuteSingle(_target1.Db);
            if (actualTransaction == null)
            {
                Assert.Inconclusive("No Transaction Recieved in src_trasaction ");
            }
            Assert.AreEqual(intransitCarton.Sku.Style, actualTransaction.Style, "Style");
            Assert.AreEqual(intransitCarton.Sku.Color, actualTransaction.Color, "Color");
            Assert.AreEqual(intransitCarton.Sku.Dimension, actualTransaction.Dimension, "dimension");
            Assert.AreEqual(intransitCarton.Sku.SkuSize, actualTransaction.SkuSize, "Size");
            Assert.AreEqual(intransitCarton.Quantity, actualTransaction.TransactionPieces, "Quantity doesn't match for carton {0}", intransitCarton.CartonId);
            Assert.AreEqual(Module_Code, actualTransaction.ModuleCode, "Module Code");
        }

        /// <summary>
        /// Valid Test : If passed storage area is not a carton area and it can be any valid sku area.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        public void Repository_ReceiveWithSkuArea()
        {
            // Selecting a valid random carton from src_carton_intransit which can be received.
            var intransitCarton = ReceivableCarton.Create(_target.Db);

            //select Quality code
            var quality = SqlBinder.Create(@"
                <![CDATA[
                select s.quality_code from tab_quality_code s 
                where s.default_receiving_quality='Y'
                ]]>
", row => new
 {
     QualityCode = row.GetValue<string>("quality_code")
 }).ExecuteSingle(_target1.Db);
            if (quality == null)
            {
                Assert.Inconclusive("No Quality Code assigned");
            }

            //select a random proess id
            var processId = GetRandomProcessId(_target.Db);

            //select destination area from master_row inventory 
            var destinationarea = SqlBinder.Create(@"
            <![CDATA[
            select t.sku_storage_area As sku_storage_area
            from master_raw_inventory sample(50) t
             where rownum =1
            ]]>
", row => new
 {
     Sku_Storage_Area = row.GetValue<string>("sku_storage_area")
 }).ExecuteSingle(_target1.Db);

            //Act
            _target1.ReceiveCarton(PALLET_ID, intransitCarton.CartonId, destinationarea.Sku_Storage_Area, processId);

            // Assert
            // Retrieve the carton which was received 
            var receivedCarton = SqlBinder.Create(@"
            <![CDATA[
            select t.carton_storage_area AS carton_storage_area,
            t.pallet_id AS pallet_id,
            t.vwh_id AS vwh_id,
            t.quality_code as quality_code
              from src_carton t
             where t.carton_id = :carton_id
            ]]>
", row => new
 {
     CartonArea = row.GetValue<string>("carton_storage_area"),
     PalletId = row.GetValue<string>("pallet_id"),
     VwhId = row.GetValue<string>("vwh_id"),
     QualityCode = row.GetValue<string>("quality_code")
 }).Parameter("carton_id", intransitCarton.CartonId)
                .ExecuteSingle(_target1.Db);
            Assert.IsNotNull(receivedCarton, "Carton {0} was not received", intransitCarton.CartonId);
            Assert.AreEqual(destinationarea.Sku_Storage_Area, receivedCarton.CartonArea, "Storage Area doesn't match");
            Assert.AreEqual(PALLET_ID, receivedCarton.PalletId, "Pallet Id doesn't match");
            Assert.AreEqual(intransitCarton.VwhId, receivedCarton.VwhId, "VWH_ID doesn't match");
            Assert.AreEqual(quality.QualityCode, receivedCarton.QualityCode, "QualityCode");

            //Check whether received Sku and intransit Sku are same or not in src_carton_detail and master_sku. 
            var receivedSku = SqlBinder.Create(@"
                            <![CDATA[
            select msku.style        As sku_style,
                   msku.color        As sku_color,
                   msku.dimension    As sku_dimension,
                   msku.sku_size     As sku_sku_size,
                   sc.style          As carton_style,
                   sc.color          As carton_color,
                   sc.dimension      As carton_dimension,
                   sc.sku_size       As carton_sku_size,
                   r.shipment_id      As shipment_id,
                   r.sewing_plant_code As sewing_plant_code,
                   sc.req_line_number As req_line_number,
                   sc.quantity       As quantity,
                   sc.req_module_code As req_module_code,
                   sc.req_process_id AS req_process_id
              from src_carton_detail sc
             inner join master_sku msku
                on sc.sku_id = msku.sku_id
             inner join src_carton r
                on sc.carton_id=r.carton_id
             where sc.carton_id = :carton_id
                            ]]>
            ", row => new
             {
                 SkuStyle = row.GetValue<string>("sku_style"),
                 SkuColor = row.GetValue<string>("sku_color"),
                 SkuDimension = row.GetValue<string>("sku_dimension"),
                 SkuSize = row.GetValue<string>("sku_sku_size"),
                 CartonStyle = row.GetValue<string>("carton_style"),
                 CartonColor = row.GetValue<string>("carton_color"),
                 CartonDimension = row.GetValue<string>("carton_dimension"),
                 CartonSkuSize = row.GetValue<string>("carton_sku_size"),
                 SkuQuantity = row.GetValue<int>("quantity"),
                 ReqProcessId = row.GetValue<int?>("req_process_id"),
                 ReqLineNumber = row.GetValue<int?>("req_line_number"),
                 ReqModuleCode = row.GetValue<string>("req_module_code"),
                 ShipmentId = row.GetValue<string>("shipment_id"),
                 SewingPlantCode = row.GetValue<string>("sewing_plant_code")
             }).Parameter("carton_id", intransitCarton.CartonId)
                .ExecuteSingle(_target1.Db);
            Assert.AreEqual(intransitCarton.Sku.Style, receivedSku.SkuStyle, "Style");
            Assert.AreEqual(intransitCarton.Sku.Color, receivedSku.SkuColor, "Color");
            Assert.AreEqual(intransitCarton.Sku.Dimension, receivedSku.SkuDimension, "Dimension");
            Assert.AreEqual(intransitCarton.Sku.SkuSize, receivedSku.SkuSize, "Size");
            Assert.AreEqual(intransitCarton.Sku.Style, receivedSku.CartonStyle, "Style");
            Assert.AreEqual(intransitCarton.Sku.Color, receivedSku.CartonColor, "Color");
            Assert.AreEqual(intransitCarton.Sku.Dimension, receivedSku.CartonDimension, "Dimension");
            Assert.AreEqual(intransitCarton.Sku.SkuSize, receivedSku.CartonSkuSize, "SkuSize");
            Assert.AreEqual(intransitCarton.Quantity, receivedSku.SkuQuantity, "Quantity");
            Assert.IsNull(receivedSku.ReqProcessId, "ReqProcessId");
            Assert.IsNull(receivedSku.ReqLineNumber, "ReqLineNumber");
            Assert.IsTrue(receivedSku.ReqModuleCode == "", "ReqModuleCode");
            Assert.AreEqual(intransitCarton.SewingPlantCode, receivedSku.SewingPlantCode, "SewingPlantCode");
            Assert.AreEqual(intransitCarton.ShipmentId, receivedSku.ShipmentId, "ShipmentId");

            // Check if rows are inserted in src_carton_process_detail for the received carton.
            var actualProcess = SqlBinder.Create(@"
            <![CDATA[
            select ps.to_carton_area As to_carton_area,
                  ps.to_pallet_id As to_pallet_id,
                  ps.new_carton_qty As new_carton_qty,
                  ps.vwh_id As vwh_id           
              from src_carton_process_detail ps
             where ps.carton_id = :carton_id
             and ps.process_id= :process_id
            ]]>
            ", row => new
             {
                 CartonArea = row.GetValue<string>("to_carton_area"),
                 ToPalletId = row.GetValue<string>("to_pallet_id"),
                 NewQuantity = row.GetValue<int>("new_carton_qty"),
                 VwhId = row.GetValue<string>("vwh_id")
             }).Parameter("carton_id", intransitCarton.CartonId)
            .Parameter("process_id", processId)
                .ExecuteSingle(_target1.Db);
            Assert.IsNotNull(actualProcess, "Entry in src_carton_process_detail");
            Assert.AreEqual(destinationarea.Sku_Storage_Area, actualProcess.CartonArea, "Carton Area in src_carton_process_detail");
            Assert.AreEqual(intransitCarton.Quantity, actualProcess.NewQuantity, "NewCartonQuantity in src_carton_process_detail");
            Assert.AreEqual(PALLET_ID, actualProcess.ToPalletId, "ToPalletId in src_carton_process_detail");
            Assert.AreEqual(intransitCarton.VwhId, actualProcess.VwhId, "VwhId in src_carton_process_detail");

            //Test the trasaction in src_trasaction and src_trasaction_detail
            var actualTransaction = SqlBinder.Create(@"
                        <![CDATA[
                        WITH q1 AS (
                        select t.style As style,
                               t.color As color,
                               t.dimension As dimension,
                               t.sku_size As sku_size,
                               t.module_code As module_code,
                               t.vwh_id As vwh_id,
                               td.transaction_pieces As transaction_pieces
                          from src_transaction t
                        inner join src_transaction_detail td
                            on t.transaction_id = td.transaction_id
                        where t.carton_id = :carton_id
                        AND td.transaction_id IS NOT NULL
                        AND TD.TRANSACTION_PIECES IS NOT NULL
                        ORDER BY t.insert_date DESC)
                        SELECT * FROM q1 WHERE ROWNUM < 2
                        ]]>
                        ", row => new
                         {
                             Style = row.GetValue<string>("style"),
                             Color = row.GetValue<string>("color"),
                             Dimension = row.GetValue<string>("dimension"),
                             SkuSize = row.GetValue<string>("sku_size"),
                             TransactionPieces = row.GetValue<int>("transaction_pieces"),
                             ModuleCode = row.GetValue<string>("module_code")
                         }).Parameter("carton_id", intransitCarton.CartonId)
                   .ExecuteSingle(_target1.Db);
            if (actualTransaction == null)
            {
                Assert.Inconclusive("No Transaction Recieved in src_trasaction ");
            }
            Assert.AreEqual(intransitCarton.Sku.Style, actualTransaction.Style, "Style");
            Assert.AreEqual(intransitCarton.Sku.Color, actualTransaction.Color, "Color");
            Assert.AreEqual(intransitCarton.Sku.Dimension, actualTransaction.Dimension, "dimension");
            Assert.AreEqual(intransitCarton.Sku.SkuSize, actualTransaction.SkuSize, "Size");
            Assert.AreEqual(intransitCarton.Quantity, actualTransaction.TransactionPieces, "Quantity doesn't match for carton {0}", intransitCarton.CartonId);
            Assert.AreEqual(Module_Code, actualTransaction.ModuleCode, "Module Code");
        }


        /// <summary>
        /// UnReceive successfully. Select a random recived carton from src_carton and unreceive it.
        /// Asserts:Check Entries in src_carton, src_carton_detail, src_carton_intrasit, src_transaction, src_transaction_detail.
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        public void Repository_UnReceiveCarton()
        {
            // Select a random Received carton
            var RecievedCarton = SqlBinder.Create(@"
            <![CDATA[
            with q1 AS
             (
            select t.carton_id AS carton_id,
                   s.style     As style,
                   s.color     As color,
                   s.dimension As dimension,
                   s.sku_size  As sku_size,
                   s.quantity  As quantity
              from src_carton t
             inner join src_carton_detail s
                on s.carton_id = t.carton_id
             where t.carton_storage_area = 'RCV'
             order by dbms_random.value
            )
            select * from q1 where rownum = 1
            ]]>
", row => new
 {
     CartonId = row.GetValue<string>("carton_id"),
     Style = row.GetValue<string>("style"),
     Color = row.GetValue<string>("color"),
     Dimension = row.GetValue<string>("dimension"),
     SkuSize = row.GetValue<string>("sku_size"),
     Quantity = row.GetValue<int>("quantity")
 }).ExecuteSingle(_target1.Db);

            if (RecievedCarton == null)
            {
                Assert.Inconclusive("Receivable cartons not available");
            }

            //select a Random Process
            var processId = GetRandomProcessId(_target1.Db);

            // Act
            _target1.UnreceiveCarton(RecievedCarton.CartonId, processId);

            // Assert
            // Retrieve the carton which was Unreceived with tight WHERE clauses
            const string QUERY_EXPECTED = @"
                <![CDATA[
                select count(*) As count
                from src_carton sc 
                where
                sc.carton_id = :carton_id
                ]]>
                ";
            var binder = new SqlBinder<int>("UnReceiveCarton");
            binder.CreateMapper(QUERY_EXPECTED);
            binder.Parameter("carton_id", RecievedCarton.CartonId);
            var actualcount = binder.ExecuteSingle(_target1.Db);
            Assert.IsTrue(actualcount == 0, "Count should be 0");

            //Unrecieve the carton from Src_Carton_Detail
            const string QUERY_EXPECTED_DETAIL = @"
                <![CDATA[
                select count(*) As count
                from src_carton_detail sc 
                where
                sc.carton_id = :carton_id
                ]]>
";
            var binderdetail = new SqlBinder<int>("UnReceiveCarton");
            binderdetail.CreateMapper(QUERY_EXPECTED_DETAIL);
            binderdetail.Parameter("carton_id", RecievedCarton.CartonId);
            var actualcountdetail = binderdetail.ExecuteSingle(_target1.Db);
            Assert.IsTrue(actualcountdetail == 0, "Count should be 0");

            //Test the trasaction in src_trasaction and Scr_transaction_detail
            var unrecievetrans = SqlBinder.Create(@"
              <![CDATA[
                                   WITH Q1 AS
             (SELECT T.STYLE               AS STYLE,
                     T.COLOR               AS COLOR,
                     T.DIMENSION           AS DIMENSION,
                     T.SKU_SIZE            AS SKU_SIZE,
                     T.MODULE_CODE         AS MODULE_CODE,
                     TD.TRANSACTION_PIECES AS TRANSACTION_PIECES
                FROM SRC_TRANSACTION T
               INNER JOIN SRC_TRANSACTION_DETAIL TD
                  ON T.TRANSACTION_ID = TD.TRANSACTION_ID
               WHERE T.CARTON_ID = :CARTON_ID
                 AND TD.TRANSACTION_PIECES IS NOT NULL
                 AND td.transaction_id IS NOT NULL
               ORDER BY T.INSERT_DATE DESC)
            SELECT * FROM Q1 WHERE ROWNUM < 2

                                    ]]>
                    ", row => new
         {
             Style = row.GetValue<string>("STYLE"),
             Color = row.GetValue<string>("COLOR"),
             Dimension = row.GetValue<string>("DIMENSION"),
             SkuSize = row.GetValue<string>("SKU_SIZE"),
             TransactionPieces = row.GetValue<int>("TRANSACTION_PIECES"),
             ModuleCode = row.GetValue<string>("MODULE_CODE")
         }).Parameter("carton_id", RecievedCarton.CartonId)
                   .ExecuteSingle(_target1.Db);
            if (unrecievetrans == null)
            {
                Assert.Inconclusive("No Trasaction Recieved in src_trasaction and src_trasaction_details ");
            }
            Assert.AreEqual(RecievedCarton.Color, unrecievetrans.Color, "Color");
            Assert.AreEqual(RecievedCarton.Dimension, unrecievetrans.Dimension, "Dimension");
            Assert.AreEqual(RecievedCarton.SkuSize, unrecievetrans.SkuSize, "Size");
            Assert.AreEqual(RecievedCarton.Style, unrecievetrans.Style, "Style");
            Assert.AreEqual(Module_Code, unrecievetrans.ModuleCode, "Module Code");
            Assert.IsTrue(RecievedCarton.Quantity + unrecievetrans.TransactionPieces == 0, "Transaction Piesces Must be 0");

            //check recived_date in intrasit table should be null.
            var recieveddate = SqlBinder.Create(@"
            <![CDATA[
            select si.received_date As received_date
            from src_carton_intransit si 
            where si.carton_id= :carton_id
            ]]>
", row => new
 {
     RecievedDate = row.GetValue<System.DateTime?>("received_date")
 }).Parameter("carton_id", RecievedCarton.CartonId)
 .ExecuteSingle(_target1.Db);
            Assert.IsNull(recieveddate.RecievedDate, "Recieved Date should be null");
        }


        /// <summary>
        ///Valid Test:Recieve Carton with any process id.
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        public void Repository_RecieveWithAnyProcessId()
        {
            // Selecting a valid random carton from src_carton_intransit which can be received.
            var intransitCarton = ReceivableCarton.Create(_target1.Db);

            // Select any random area as destination of carton
            var expectedDestArea = SqlBinder.Create(@"
            <![CDATA[
            select t.inventory_storage_area AS inventory_storage_area
              from tab_inventory_area sample(50) t
             where rownum = 1
            ]]>
", row => new
 {
     InventoryStorageArea = row.GetValue<string>("inventory_storage_area")
 }).ExecuteSingle(_target1.Db);
            if (expectedDestArea == null)
            {
                Assert.Inconclusive("No inventory areas defined");
            }
            _target1.ReceiveCarton(PALLET_ID, intransitCarton.CartonId, expectedDestArea.InventoryStorageArea, -10);
        }

        /// <summary>
        /// Valid Test : Try to Unrecieve Carton with any process_id.
        /// </summary>
        [TestMethod]
        [TestCategory("Datbaase")]
        public void UnrecieveCartonWithAnyProcessId()
        {
            //Select a random recieved carton from src_carton
            var receivedCarton = SqlBinder.Create(@"
            <![CDATA[
            with q1 AS
             (
            select s.carton_id As carton_id 
            from src_carton s 
            order by dbms_random.value)
            select * from q1 where rownum = 1
            ]]>
", row => new
 {
     CartonId = row.GetValue<string>("carton_id")
 }).ExecuteSingle(_target1.Db);

            _target1.UnreceiveCarton(receivedCarton.CartonId, -333300);
        }

        //Invalid Test

        /// <summary>
        //Test if Passed Carton_id is null then throw exception "Carton_id must be passed"
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Repository_RecieveWithNullCartonId()
        {

            // Select any random area as destination of carton
            var expectedDestArea = SqlBinder.Create(@"
            <![CDATA[
            select t.inventory_storage_area AS inventory_storage_area
              from tab_inventory_area sample(50) t
             where rownum = 1
            ]]>
", row => new
 {
     InventoryStorageArea = row.GetValue<string>("inventory_storage_area")
 }).ExecuteSingle(_target1.Db);
            if (expectedDestArea == null)
            {
                Assert.Inconclusive("No inventory areas defined");
            }
            //select a random proess id
            var processId = GetRandomProcessId(_target.Db);

            _target1.ReceiveCarton(PALLET_ID, null, expectedDestArea.InventoryStorageArea, processId);
        }

        /// <summary>
        /// Invalid Test: If Passed Carton_id is invalid (does not exists in src_carton_intransit table) then throw exception "no data found"
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Repository_RecieveWithInvalidCartonId()
        {
            // Select any random area as destination of carton
            var expectedDestArea = SqlBinder.Create(@"
            <![CDATA[
            select t.inventory_storage_area AS inventory_storage_area
              from tab_inventory_area sample(50) t
             where rownum = 1
            ]]>
", row => new
 {
     InventoryStorageArea = row.GetValue<string>("inventory_storage_area")
 }).ExecuteSingle(_target1.Db);
            if (expectedDestArea == null)
            {
                Assert.Inconclusive("No inventory areas defined");
            }
            //select a random proess id
            var processId = GetRandomProcessId(_target.Db);

            _target1.ReceiveCarton(PALLET_ID, "123245", expectedDestArea.InventoryStorageArea, processId);
        }


        /// <summary>
        // Invalid Test: if Passed StorageArea is null then throw exception "Destination area must be passed"
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Repository_RecieveWithNullStorageArea()
        {
            // Selecting a valid random carton from src_carton_intransit which can be received.
            var intransitCarton = ReceivableCarton.Create(_target1.Db);

            //select a random proess id
            var processId = GetRandomProcessId(_target.Db);

            _target1.ReceiveCarton(PALLET_ID, intransitCarton.CartonId, null, processId);
        }

        /// <summary>
        /// Invalid Test : If passed storage area is invalid (does not exist in tab_inventory_area)then throw
        /// exception "Primary Key violation exception".
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Repository_ReceiveWithInvalidStorageArea()
        {
            // Selecting a valid random carton from src_carton_intransit which can be received.
            var intransitCarton = ReceivableCarton.Create(_target.Db);

            //select a random proess id
            var processId = GetRandomProcessId(_target1.Db);

            _target1.ReceiveCarton(PALLET_ID, intransitCarton.CartonId, "WID", processId);
        }



        /// <summary>
        //Invalid Test:If Passed ProcessId is null then throw exception "processid must be passed"
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Repository_RecieveWithNullProcessId()
        {
            // Selecting a valid random carton from src_carton_intransit which can be received.
            var intransitCarton = ReceivableCarton.Create(_target1.Db);

            // Select any random area as destination of carton
            var expectedDestArea = SqlBinder.Create(@"
            <![CDATA[
            select t.inventory_storage_area AS inventory_storage_area
              from tab_inventory_area sample(50) t
             where rownum = 1
            ]]>
", row => new
 {
     InventoryStorageArea = row.GetValue<string>("inventory_storage_area")
 }).ExecuteSingle(_target1.Db);
            if (expectedDestArea == null)
            {
                Assert.Inconclusive("No inventory areas defined");
            }
            _target1.ReceiveCarton(PALLET_ID, intransitCarton.CartonId, expectedDestArea.InventoryStorageArea, null);
        }


   
        /// <summary>
        /// Invalid Test :Recieve a carton which is already received. it should throw Primary Key violation exception.
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Repository_RecieveAlreadyReceivedCarton()
        {
            var receivedCarton = SqlBinder.Create(@"
            <![CDATA[
            with q1 AS
             (select s.carton_id As carton_id 
            from src_carton s 
            order by dbms_random.value)
            select * from q1 where rownum = 1
            ]]>
", row => new
 {
     CartonId = row.GetValue<string>("carton_id")
 }).ExecuteSingle(_target1.Db);
            if (receivedCarton == null)
            {
                Assert.Inconclusive("Receivable cartons not available");
            }
            var expectedDestArea = SqlBinder.Create(@"
            <![CDATA[
            select t.inventory_storage_area AS inventory_storage_area
              from tab_inventory_area sample(50) t
             where rownum = 1
            ]]>
", row => new
{
    InventoryStorageArea = row.GetValue<string>("inventory_storage_area")
}).ExecuteSingle(_target1.Db);
            if (expectedDestArea == null)
            {
                Assert.Inconclusive("No Storage Area is defined");
            }

            //select a random proess id
            var processId = GetRandomProcessId(_target.Db);
            _target1.ReceiveCarton(PALLET_ID, receivedCarton.CartonId, expectedDestArea.InventoryStorageArea, processId);
        }


        /// <summary>
        ///Invalid Test:Unrecieve a carton with null carton_id and throw exception "Carton must be passed".
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Repository_UnRecievedCartonWithNullCartonId()
        {
            //select a random proess id
            var processId = GetRandomProcessId(_target1.Db);

            _target1.UnreceiveCarton(null, processId);
        }


        /// <summary>
        /// Invalid Test : Try to unrecieve an invalid carton it should throw an exception "no data found" 
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Repository_UnrecieveCartonWithInvalidCartonId()
        {
            //select a random proess id
            var processId = GetRandomProcessId(_target1.Db);

            _target1.UnreceiveCarton("123324", processId);
        }

        /// <summary>
        /// Invalid Test : Try to execute UnRecieveCarton with null process id.  It should throw an exceptionn "process_id must be passed".
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Repository_UnrecieveCartonWithNullProcessId()
        {
            //Select a random recieved carton from src_carton
            var receivedCarton = SqlBinder.Create(@"
            <![CDATA[
            with q1 AS
             (
            select s.carton_id As carton_id 
            from src_carton s 
            order by dbms_random.value)
            select * from q1 where rownum = 1
            ]]>
", row => new
 {
     CartonId = row.GetValue<string>("carton_id")
 }).ExecuteSingle(_target1.Db);

            _target1.UnreceiveCarton(receivedCarton.CartonId, null);

        }

    
        /// <summary>
        /// Invalid Test: Try to execute UnRecieveCarton with a carton which never received and throw exception "no data found".
        /// </summary>
        [TestMethod]
        [Owner("Rajesh")]
        [TestCategory("Database")]
        [ExpectedException(typeof(Oracle.DataAccess.Client.OracleException))]
        public void Unrecieve_Invalid()
        {
            //select an unrecieve carton from src_carton_intransit.
            var intransitCarton = ReceivableCarton.Create(_target1.Db);

            //select a random proess id
            var processId = GetRandomProcessId(_target.Db);

            _target1.UnreceiveCarton(intransitCarton.CartonId, processId);
        }


//        /// <summary>
//        /// Test : Try to Recieve a carton under db user and throw exception "table or view does not exist"
//        /// </summary>
//        [TestMethod]
//        [TestCategory("Database")]
//        public void Repository_RecieveCartonUnderDbUser()
//        {
//            // Select a random receivable carton
//            // Must begin with P
//            var intransitCarton = ReceivableCarton.Create(_target.Db);

//            //select Quality code
//            var quality = SqlBinder.Create(@"
//            <![CDATA[
//            select s.quality_code from tab_quality_code s 
//            where s.default_receiving_quality='Y'
//            ]]>
//", row => new
// {
//     QualityCode = row.GetValue<string>("quality_code")
// }).ExecuteSingle(_target.Db);
//            if (quality == null)
//            {
//                Assert.Inconclusive("No Quality Code assigned");
//            }

//            //select a random process id
//            var processId = GetRandomProcessId(_target.Db);

//            // Select any random area as destination of carton
//            var expectedDestArea = SqlBinder.Create(@"
//            <![CDATA[
//            select t.inventory_storage_area AS inventory_storage_area
//              from tab_inventory_area sample(50) t
//             where rownum = 1
//            ]]>
//", row => new
//            {
//                InventoryStorageArea = row.GetValue<string>("inventory_storage_area")
//            }).ExecuteSingle(_target.Db);
//            if (expectedDestArea == null)
//            {
//                Assert.Inconclusive("No inventory areas defined");
//            }

//            // Act
//            _target.ReceiveCarton(PALLET_ID, intransitCarton.CartonId, expectedDestArea.InventoryStorageArea, processId);
//        }

        /// <summary>
        ///  Test : Try to UnRecieve a carton under db user. Succesfully unrecieved.
        /// </summary>
        [TestMethod]
        [TestCategory("Database")]
        public void Repository_UnRecieveCartonUnderDbUser()
        {
            // Select a random Received carton
            var RecievedCarton = SqlBinder.Create(@"
            <![CDATA[
            with q1 AS
             (
            select t.carton_id AS carton_id,
                   s.style     As style,
                   s.color     As color,
                   s.dimension As dimension,
                   s.sku_size  As sku_size,
                   s.quantity  As quantity
              from src_carton t
             inner join src_carton_detail s
                on s.carton_id = t.carton_id
             where t.carton_storage_area = 'RCV'
             order by dbms_random.value
)
select * from q1 where rownum = 1
]]>
", row => new
 {
     CartonId = row.GetValue<string>("carton_id"),
     Style = row.GetValue<string>("style"),
     Color = row.GetValue<string>("color"),
     Dimension = row.GetValue<string>("dimension"),
     SkuSize = row.GetValue<string>("sku_size"),
     Quantity = row.GetValue<int>("quantity")
 }).ExecuteSingle(_target1.Db);

            if (RecievedCarton == null)
            {
                Assert.Inconclusive("Receivable cartons not available");
            }

            //select a Random Process
            var processId = GetRandomProcessId(_target1.Db);

            // Act
            _target1.UnreceiveCarton(RecievedCarton.CartonId, processId);
        }
    }

}
