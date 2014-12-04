using DcmsMobile.Repack.Models;
using EclipseLibrary.Oracle;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Web.Routing;


namespace DcmsMobile.Repack.Repository
{
    [Flags]
    public enum InventoryAreaFilters
    {
        All = 0x0,

        /// <summary>
        /// tia.track_bundle_flag != 'M' OR tia.track_bundle_flag IS NULL
        /// </summary>
        StoresCarton = 0x2,

        /// <summary>
        /// tia.track_bundle_flag != 'M'
        /// </summary>
        StoresSku = 0x4,

        /// <summary>
        /// tia.unusable_inventory IS NULL
        /// </summary>
        Usable = 0x8,

        /// <summary>
        /// tia.unusable_inventory IS NOT NULL
        /// </summary>
        Unusable = 0x10,

        /// <summary>
        /// Not Exists
        ///(SELECT 1
        ///         FROM master_storage_location msl
        ///        where msl.storage_area = tia.inventory_storage_area)
        /// </summary>
        Unnumbered = 0x20,

        /// <summary>
        /// tia.conversion_area IS NOT NULL
        /// </summary>
        ConversionAreasOnly = 0x40,

        ExcludeConversionAreas = 0x80,

        /// <summary>
        /// tia.is_pallet_required IS NOT NULL
        /// </summary>
        NoPallet = 0x100,

        /// <summary>
        /// Selects tia.unusable_inventory
        /// </summary>
        GroupByUsability = 0x200,

        /// <summary>
        /// Selects tia.is_pallet_required
        /// </summary>
        GroupByPalletRequirement = 0x400,

        /// <summary>
        /// tia.is_repack_area='Y'
        /// </summary>
        RepackForStorage = 0x800,

        /// <summary>
        /// tia.is_receiving_area='Y'
        /// </summary>
        ReceivingAreas = 0x1000,

        ExcludeReceivingAreas = 0x2000,

        /// <summary>
        /// By default SSS is never retrieved. Include this flag if you wish to retrive it.
        /// </summary>
        IncludeConsolidatedUpcAreas = 0x4000,

        /// <summary>
        /// For cancelled area in IA table.
        /// </summary>
        CancelledArea = 0x8000        
    }

    public class RepackRepository
    {
        protected readonly OracleDatastore _db;

        /// <summary>
        /// For use in tests
        /// </summary>
        public OracleDatastore Db
        {
            get
            {
                return _db;
            }
        }

        public RepackRepository(OracleDatastore db)
        {
            _db = db;
        }

        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="requestContext"></param>
        public RepackRepository(RequestContext requestContext)
        {
            _db = new OracleDatastore(requestContext.HttpContext.Trace);
            _db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString,
                requestContext.HttpContext.SkipAuthorization ? string.Empty : requestContext.HttpContext.User.Identity.Name);
            // Sharad 20 Dec 2011: The module code must be RPK because reason code philosophy embedded in 
            // package IFR_ISI special handles reason codes for this module.
            _db.ModuleName = "RPK";
            _db.ClientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                requestContext.HttpContext.Request.UserHostName;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public DbTransaction BeginTransaction()
        {
            return _db.BeginTransaction();
        }

        /// <summary>
        /// Sharad 19 Dec 2011: Using tia.location_numbering_flag to determine whether location is numbered.
        /// Earlier we were checking for existence of locations in master_storage_location.
        /// For cencelled area, we were getting area from Ia table.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        internal IList<InventoryArea> GetInventoryAreas(InventoryAreaFilters filters)
        {
            const string QUERY = @"
                SELECT inventory_storage_area AS Inventory_Storage_Area,
                        short_name AS short_name,
                        description AS description,
                        <if c='$GroupBy = ""P""'>tia.is_pallet_required</if>
                        <else c='$GroupBy = ""U""'>tia.unusable_inventory</else>
                        <else>NULL</else>
                        AS grouping_column,
                        NULL AS  CAN_AREA
                        FROM <proxy />tab_inventory_area tia
                        WHERE 1 = 1
                        <if c='not($IncludeConsolidatedUpcAreas)'>
                        AND tia.consolidated_upc_code IS NULL
                        </if>                           
                        <if c='$RepackForStorage'>
                            And tia.is_repack_area='Y'
                        </if>
                         <if>AND tia.stores_what = :stores_what</if>
                        <if c='$stores_what=""CTN""'>AND (tia.track_bundle_flag != 'M' OR tia.track_bundle_flag IS NULL)</if>
                         <if c='$UsableAreas'>AND tia.unusable_inventory IS NULL</if>
                         <if c='$UnusableAreas'>AND tia.unusable_inventory IS NOT NULL</if>
                         <if c='$NoPallet'>AND tia.is_pallet_required IS NOT NULL</if>
                         <if c='$Unnumbered'>AND tia.location_numbering_flag is null</if>
                         <if c='$Conversion=""Y""'>AND tia.is_conversion_area is not null</if>
                         <else c='$Conversion=""N""'>AND tia.is_conversion_area is null</else>
                         <if c='$ReceivingArea=""Y""'>AND tia.is_receiving_area is not null</if>
                        <else c='$ReceivingArea=""N""'>AND tia.is_receiving_area is null</else>
                         <if c='$CancelledArea'>
                                UNION ALL
                                SELECT T.IA_ID              AS INVENTORY_STORAGE_AREA,
                                       T.IA_ID              AS SHORT_NAME,
                                       T.SHORT_DESCRIPTION  AS DESCRIPTION,
                                       NULL                 AS GROUPING_COLUMN,
                                       'Y'                AS CAN_AREA
                                FROM <proxy />IA T
                                WHERE T.IA_ID IN (SELECT DISTINCT (IA_ID)
                                FROM <proxy />IALOC_CONTENT)
                                    AND T.DEFAULT_IA_LOCATION IS NOT NULL
                          </if>
                        ORDER BY GROUPING_COLUMN DESC, INVENTORY_STORAGE_AREA
";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new InventoryArea
            {
                IaId = row.GetString("Inventory_Storage_Area"),
                ShortName = row.GetString("short_name"),
                Description = row.GetString("description"),
                GroupingColumn = row.GetString("grouping_column"),
                IsCancelArea = !string.IsNullOrEmpty(row.GetString("CAN_AREA"))
            });
            if (filters.HasFlag(InventoryAreaFilters.StoresSku))
            {
                binder.Parameter("stores_what", "SKU");
            }
            // For destination areas pass CTN
            else if (filters.HasFlag(InventoryAreaFilters.StoresCarton))
            {
                binder.Parameter("stores_what", "CTN");
            }
            else
            {
                binder.Parameter("stores_what", "");
            }
            if (filters.HasFlag(InventoryAreaFilters.Usable))
            {
                binder.Parameter("UsableAreas", "1");
            }
            else
            {
                binder.Parameter("UsableAreas", "");
            }
            if (filters.HasFlag(InventoryAreaFilters.Unusable))
            {
                binder.Parameter("UnusableAreas", "1");
            }
            else
            {
                binder.Parameter("UnusableAreas", "");
            }
            if (filters.HasFlag(InventoryAreaFilters.Unnumbered))
            {
                binder.Parameter("Unnumbered", "1");
            }
            else
            {
                binder.Parameter("Unnumbered", "");
            }
            if (filters.HasFlag(InventoryAreaFilters.NoPallet))
            {
                binder.Parameter("NoPallet", "1");
            }
            else
            {
                binder.Parameter("NoPallet", "");
            }
            if (filters.HasFlag(InventoryAreaFilters.GroupByPalletRequirement))
            {
                binder.Parameter("GroupBy", "P");
            }
            else if (filters.HasFlag(InventoryAreaFilters.GroupByUsability))
            {
                binder.Parameter("GroupBy", "U");
            }
            else
            {
                binder.Parameter("GroupBy", "");
            }
            if (filters.HasFlag(InventoryAreaFilters.RepackForStorage))
            {
                binder.Parameter("RepackForStorage", "S");
            }
            else
            {
                binder.Parameter("RepackForStorage", "");
            }
            if (filters.HasFlag(InventoryAreaFilters.IncludeConsolidatedUpcAreas))
            {
                binder.Parameter("IncludeConsolidatedUpcAreas", "Y");
            }
            else
            {
                binder.Parameter("IncludeConsolidatedUpcAreas", "");
            }
            //For cancelled area.
            if (filters.HasFlag(InventoryAreaFilters.CancelledArea))
            {
                binder.Parameter("CancelledArea", "CAN");
            }
            else
            {
                binder.Parameter("CancelledArea", "");
            }
            //for conversion area
            if (filters.HasFlag(InventoryAreaFilters.ConversionAreasOnly))
            {
                binder.Parameter("Conversion", "Y");
            }
            else  if (filters.HasFlag(InventoryAreaFilters.ExcludeConversionAreas))            
            {
                binder.Parameter("Conversion", "N");
            }            
            else
            {
                binder.Parameter("Conversion", "");
            }
            if (filters.HasFlag(InventoryAreaFilters.ReceivingAreas))
            {
                binder.Parameter("ReceivingArea", "Y");
            }
            else if (filters.HasFlag(InventoryAreaFilters.ExcludeReceivingAreas))
            {
                binder.Parameter("ReceivingArea", "N");
            }
            else
            {
                binder.Parameter("ReceivingArea", "");
            }

            var result = _db.ExecuteReader(QUERY, binder);
            return result;
        }

        internal IList<PriceSeason> GetPriceSeasonCodes()
        {
            const string QUERY = @"
                              SELECT tps.price_season_code AS Price_Season_Code,
                                     tps.description       AS Description
                              FROM <proxy />tab_price_season tps
                              ORDER BY tps.price_season_code
                            ";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new PriceSeason
            {
                PriceSeasonCode = row.GetString("Price_Season_Code"),
                Description = row.GetString("Description")
            });
            var result = _db.ExecuteReader(QUERY, binder);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qualityType">R for receiving qualities, O for order qualities, null for all</param>
        /// <param name="maxRows"> </param>
        /// <returns></returns>
        internal IList<Quality> GetQualityCodes(string qualityType, int? maxRows)
        {
            const string QUERY = @"
WITH q1 AS (
   SELECT qc.quality_code AS Quality_Code,
                            NVL(qc.description, qc.quality_code) AS description,
                            qc.quality_rank AS quality_rank
                        FROM <proxy />tab_quality_code qc
                        <if c=""$ui_type = 'R'"">WHERE qc.default_receiving_quality IS NOT NULL</if>
                        <if c=""$ui_type = 'O'"">WHERE qc.order_quality IS NOT NULL</if>
                        <if c=""$ui_type = 'D'"">ORDER BY qc.quality_rank desc, qc.quality_code</if>
                        <else>ORDER BY qc.quality_rank asc, qc.quality_code</else>
)
SELECT * from q1 
<if>
WHERE rownum &lt;= :maxrows
</if>
";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new Quality()
            {
                QualityCode = row.GetString("Quality_Code"),
                Description = row.GetString("description"),
                QualityRank = row.GetInteger("quality_rank")
            }).Parameter("ui_type", qualityType)
            .Parameter("maxrows", maxRows);
            var result = _db.ExecuteReader(QUERY, binder);
            return result;
        }

        internal IEnumerable<SewingPlant> GetSewingPlants()
        {
            const string QUERY = @"
                SELECT sp.sewing_plant_code       AS sewing_plant_code,
                        sp.sewing_plant_name       AS sewing_plant_name,
                        spparent.sewing_plant_name AS parent_sewing_plant_name
                FROM <proxy />tab_sewingplant sp
                LEFT OUTER JOIN <proxy />tab_sewingplant spparent
                        ON spparent.sewing_plant_code = sp.parent_sewing_plant
                        ORDER BY sp.sewing_plant_name
";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new SewingPlant()
            {
                SewingPlantCode = row.GetString("sewing_plant_code"),
                PlantName = row.GetString("sewing_plant_name"),
                GroupingColumn = row.GetString("parent_sewing_plant_name")
            });
            var result = _db.ExecuteReader(QUERY, binder);
            return result;
        }

        internal IList<VirtualWarehouse> GetVirtualWarehouses()
        {
            const string QUERY = @"
  SELECT vwh_id AS vwh_id, description AS description FROM <proxy />tab_virtual_warehouse ORDER BY vwh_id 
";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new VirtualWarehouse
            {
                VwhId = row.GetString("vwh_id"),
                Description = row.GetString("description")
            });
            var result = _db.ExecuteReader(QUERY, binder);
            return result;
        }

        internal IList<Printer> GetPrinters(string printerType)
        {
            const string QUERY = @"
  select
    tabprinter.name AS name,
    tabprinter.description AS description
from <proxy />tab_printer tabprinter 
where 1 = 1
<if>AND upper(printer_type) = :printerType</if>
order by lower(name)
";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new Printer
            {
                PrinterName = row.GetString("name"),
                Description = row.GetString("description")
            }).Parameter("printerType", printerType);
            var result = _db.ExecuteReader(QUERY, binder);
            return result;
        }

//        /// <summary>
//        /// If passed area is cancelled area then return true otherwise false.
//        /// We update the pieces in ialoc_content if source area is cancel area.
//        /// </summary>
//        /// <param name="sourceArea"></param>
//        /// <param name="vwhId"></param>
//        /// <param name="pieces"></param>
//        /// <param name="upcCode"></param>
//        /// <returns>
//        /// Bool: True or False
//        /// </returns>
//        internal void UpdateCancelAreaPieces(string sourceArea, string vwhId, int? pieces, string upcCode)
//        {
//            Contract.Assert(_db != null);

//            const string QUERY = @"
//                    UPDATE <proxy />IALOC_CONTENT I
//                    SET I.NUMBER_OF_UNITS = I.NUMBER_OF_UNITS - :PIECES
//                    WHERE I.LOCATION_ID = (SELECT A.LOCATION_ID
//                                            FROM <proxy />IALOC A
//                                        WHERE A.IA_ID = :SOURCE_AREA
//                                            AND A.VWH_ID = :VWH_ID)
//                    AND I.IACONTENT_ID = :UPC_CODE 
//                ";

//            var binder = SqlBinder.Create()
//                .Parameter("SOURCE_AREA", sourceArea)
//                .Parameter("VWH_ID", vwhId)
//                .Parameter("PIECES", pieces)
//                .Parameter("UPC_CODE", upcCode);
//            _db.ExecuteNonQuery(QUERY, binder);
//        }


//        /// <summary>
//        /// Remove carton from SRC_OPEN_CARTON.
//        /// </summary>
//        /// <param name="cartonId"></param>
//        internal void RemoveCarton(string cartonId)
//        {
//            Contract.Assert(_db != null);
//            const string QUERY = @"
//                                DELETE FROM <proxy />SRC_OPEN_CARTON S WHERE S.CARTON_ID = :CARTON_ID";

//            var binder = SqlBinder.Create()
//                .Parameter("CARTON_ID", cartonId);
//            _db.ExecuteNonQuery(QUERY, binder);
//        }
        /// <summary>
        /// Creates a new carton based on the passed info.
        /// Returns the id of the first and last newly created carton.
        /// </summary>
        /// <param name="info">This will be null if BarCode is invalid</param>
        internal string[] RepackCarton(CartonRepackInfo info)
        {
            const string QUERY = @"
DECLARE
  LRelated_TRansaction_Id   NUMBER(10);
  LLcarton_id               VARCHAR2(255);
  LFcarton_id               VARCHAR2(255);
  LLOCATION_ID              VARCHAR2(13);
  LFLAG_QUALITY             NUMBER := <proxy />PKG_CARTON_WORK_2.PFLAG_QUALITY;

BEGIN
  FOR i IN 1 .. :no_of_cartons LOOP
    insert into <proxy />src_carton
      (carton_id,
       shipment_id,
       pallet_id,
       price_season_code,
       carton_storage_area,
       sewing_plant_code,
       vwh_id,
       quality_code)
    VALUES
      (
<if>:carton_id</if>
<if c='not($carton_id)'>'R'||Carton_Sequence.Nextval</if>,
       :shipment_id,
       :apallet_id,
       :aprice_season_code,
       :adestination_area,
       :asewing_plant_code,
       :vwh_id,
       :quality_code)
    RETURNING carton_id into LLcarton_id;
    IF LFcarton_id is NULL THEN
    LFcarton_id:=LLcarton_id;
    END IF;

    LRelated_TRansaction_Id := <proxy />pkg_inv_3.ADDSKUTOCARTON(acarton_id     =&gt; LLcarton_id,
                                                        asku_id                 =&gt; :sku_id,
                                                        apieces                 =&gt; :apieces,
                                                        asource_area            =&gt; :asource_area,
                                                        arelated_transaction_id =&gt; LRelated_TRansaction_Id);

<if c='$target_sku_id or $target_vwh_id'>  
    LRelated_TRansaction_Id := <proxy />pkg_carton_work_2.mark_carton_for_work(ACARTON_ID     =&gt; LLcarton_id,
                                  ATARGET_SKU_ID =&gt; :target_sku_id,
                                  ATARGET_VWH_ID =&gt; :target_vwh_id,
                                  ATARGET_QUALITY =&gt; :target_QualityCode,
                                  arelated_transaction_id =&gt; LRelated_TRansaction_Id,
                                  acomplete_flags =&gt; LFLAG_QUALITY);
</if>
<if c='$aprinter_name'>
    <proxy />PKG_JF_SRC_2.PKG_JF_SRC_CTN_TKT(ACARTON_ID    =&gt; LLcarton_id,
                                    APRINTER_NAME =&gt; :aprinter_name);
</if>
 END LOOP;
:acarton_id := LFcarton_id;
:acarton_id1 := LLcarton_id;
END;
            ";
            if (string.IsNullOrEmpty(info.QualityCode))
            {
                throw new ArgumentNullException("info.QualityCode");
            }
            if (info.Pieces == null)
            {
                throw new ArgumentNullException("info.Pieces");
            }

            string[] cartonId = new string[2];
            var binder = SqlBinder.Create().Parameter("carton_id", info.CartonId)
                .Parameter("sku_id", info.SkuId)
                .Parameter("target_sku_id", info.TartgetSkuId)
                .OutParameter("acarton_id", row => cartonId[0] = row)
                .OutParameter("acarton_id1", row => cartonId[1] = row)
                .Parameter("vwh_id", info.VwhId)
                .Parameter("adestination_area", info.DestinationCartonArea)
                .Parameter("apallet_id", info.PalletId)
                .Parameter("aprice_season_code", info.PriceSeasonCode)
                .Parameter("quality_code", info.QualityCode)
                .Parameter("asource_area", info.SourceSkuArea)
                .Parameter("apieces", info.Pieces.Value)
                .Parameter("asewing_plant_code", info.SewingPlantCode)
                .Parameter("shipment_id", info.ShipmentId)
                .Parameter("no_of_cartons", info.NumberOfCartons)
                .Parameter("target_vwh_id", info.TargetVWhId)
                .Parameter("target_QualityCode", info.TargetQualityCode)
                .Parameter("aprinter_name", info.PrinterName)
                .Parameter("UPC_CODE", info.UpcCode);
            try
            {
                _db.ExecuteNonQuery(QUERY, binder);
            }
            catch (OracleException ex)
            {
                switch (ex.Number)
                {
                    case 20006:
                        throw new Exception("Not enough inventory in source area. Overdraft is not allowed.", ex);
                    case 00001:
                        throw new Exception("Box is already convert in to carton.", ex);

                    default:
                        throw;
                }
            }
            return cartonId;
        }


        /// <summary>
        /// Used to validated the UPC code
        /// </summary>
        /// <param name="barCode"></param>
        /// <returns></returns>
        public Sku GetSkuFromBarCode(string barCode)
        {
            const string QUERY = @"
            SELECT MS.UPC_CODE  AS UPC_CODE,
                   MS.STYLE     AS STYLE,
                   MS.COLOR     AS COLOR,
                   MS.DIMENSION AS DIMENSION,
                   MS.SKU_SIZE  AS SKU_SIZE,
                   MS.SKU_ID    AS SKU_ID
              FROM <proxy />MASTER_SKU MS
             WHERE MS.UPC_CODE = :UPCCODE
               AND MS.INACTIVE_FLAG IS NULL

            UNION ALL

            SELECT MS.UPC_CODE,
                   MS.STYLE,
                   MS.COLOR,
                   MS.DIMENSION,
                   MS.SKU_SIZE,
                   MS.SKU_ID
              FROM <proxy />MASTER_CUSTOMER_SKU MCS
             INNER JOIN <proxy />MASTER_SKU MS
                ON MS.STYLE = MCS.STYLE
               AND MS.COLOR = MCS.COLOR
               AND MS.DIMENSION = MCS.DIMENSION
               AND MS.SKU_SIZE = MCS.SKU_SIZE
             WHERE MS.INACTIVE_FLAG IS NULL
               AND NVL(MCS.SCANNED_BAR_CODE, MCS.CUSTOMER_SKU_ID) = :UPCCODE
        ";

            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new Sku
            {
                SkuId = row.GetInteger("SKU_ID").Value,
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                UpcCode = row.GetString("UPC_CODE")
            }).Parameter("UPCCODE", barCode);
            return _db.ExecuteSingle(QUERY, binder);
        }
    }
}




//$Id$
