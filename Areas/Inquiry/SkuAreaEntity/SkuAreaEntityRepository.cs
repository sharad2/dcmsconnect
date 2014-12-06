using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuAreaEntity
{
    internal class SkuAreaEntityRepository : IDisposable
    {
        private readonly OracleDatastore _db;
        public SkuAreaEntityRepository(string userName, string clientInfo)
        {
            _db = new OracleDatastore(HttpContext.Current.Trace);
            _db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString, userName);
            _db.ModuleName = "Inquiry_SkuAreaEntity";
            _db.ClientInfo = clientInfo;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
        /// <summary>
        /// FOR SKU AREA  INFORMATION
        /// </summary>
        /// <param name="iaId"></param>
        /// <returns></returns>
        public SkuArea GetSkuAreaInfo(string iaId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
SELECT IA.IA_ID AS IA_ID,
       MAX(IA.SHORT_NAME) AS SHORT_NAME,
       MAX(IA.SHORT_DESCRIPTION) AS SHORT_DESCRIPTION,
       MAX(IA.DEFAULT_IA_LOCATION) AS DEFAULT_IA_LOCATION,
       MAX(IA.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
       MAX(IA.PICKING_AREA_FLAG) AS PICKING_AREA_FLAG,
       MAX(IA.SHIPPING_AREA_FLAG) AS SHIPPING_AREA_FLAG,
       MAX(IA.PULL_CARTON_LIMIT) AS PULL_CARTON_LIMIT,
       COUNT(DISTINCT IAL.LOCATION_ID) AS NUMBER_OF_LOCATIONS,
       COUNT(IAL.ASSIGNED_UPC_CODE) AS NUMBER_OF_ASSIGNED_LOCATIONS
  FROM <proxy />IA IA
  LEFT OUTER JOIN <proxy />IALOC IAL
    ON IA.IA_ID = IAL.IA_ID
 WHERE IA.IA_ID = :IA_ID
 GROUP BY IA.IA_ID
";
            var binder = SqlBinder.Create(row => new SkuArea
            {
                IaId = row.GetString("IA_ID"),
                Description = row.GetString("SHORT_DESCRIPTION"),
                DefaultLocation = row.GetString("DEFAULT_IA_LOCATION"),
                WhId = row.GetString("WAREHOUSE_LOCATION_ID"),
                PickingAreaFlag = row.GetString("PICKING_AREA_FLAG"),
                ShipingAreaFlag = row.GetString("SHIPPING_AREA_FLAG"),
                PullCartonLimit = row.GetInteger("PULL_CARTON_LIMIT"),
                NumberOfLocations = row.GetInteger("NUMBER_OF_LOCATIONS"),
                AssignedLocations = row.GetInteger("NUMBER_OF_ASSIGNED_LOCATIONS"),
                ShortName = row.GetString("SHORT_NAME")
            }).Parameter("IA_ID", iaId);

            return _db.ExecuteSingle(QUERY, binder);
        }

        public SkuLocation GetSkuLocation2(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new ArgumentNullException("locationId");
            }

            Contract.Assert(_db != null);
            const string QUERY_SKU_LOCATION_INFO = @"
WITH q1 AS (
             SELECT IA.LOCATION_ID             AS LOCATION_ID,
                    IA.IA_ID                   AS IA_ID,
                    I.SHORT_NAME                AS SHORT_NAME,
                    IA.CYC_MARKED_DATE         AS CYC_MARKED_DATE,
                    IA.CYC_FLAG                AS CYC_FLAG,
                    IA.LAST_CYC_START_DATE     AS CYC_START_DATE,
                    IA.LAST_CYC_END_DATE       AS CYC_END_DATE,
                    IA.FREEZE_FLAG             AS FREEZE_FLAG,
                    IA.ASSIGNED_UPC_MAX_PIECES AS ASSIGNED_UPC_MAX_PIECES,
                    IA.PITCH_AISLE_ID          AS PITCH_AISLE_ID,
                    IA.VWH_ID                  AS VWH_ID,
                    IA.WAREHOUSE_LOCATION_ID   AS WAREHOUSE_LOCATION_ID,
                    IA.RESTOCK_AISLE_ID        AS RESTOCK_AISLE_ID,
                    MSKU.STYLE                 AS ASSIGNED_STYLE,
                    MSKU.COLOR                 AS ASSIGNED_COLOR,
                    MSKU.DIMENSION             AS ASSIGNED_DIMENSION,
                    MSKU.SKU_SIZE              AS ASSIGNED_SKU_SIZE,
                    MSKU.SKU_ID                AS ASSIGNED_SKU_ID,
                    IC.NUMBER_OF_UNITS         AS NUMBER_OF_UNITS,
                    MSKUIC.STYLE                AS STYLE,
                    MSKUIC.COLOR                AS COLOR,
                    MSKUIC.DIMENSION            AS DIMENSION,
                    MSKUIC.SKU_SIZE             AS SKU_SIZE,
                    MSKUIC.SKU_ID               AS SKU_ID
            FROM <proxy />IALOC IA
            LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
             ON IA.ASSIGNED_UPC_CODE = MSKU.UPC_CODE
            LEFT OUTER JOIN <proxy />IALOC_CONTENT IC
             ON IA.LOCATION_ID = IC.LOCATION_ID
                and IC.NUMBER_OF_UNITS != 0
            LEFT OUTER JOIN <proxy />MASTER_SKU MSKUIC
             ON IC.IACONTENT_ID = MSKUIC.UPC_CODE
            LEFT OUTER JOIN <proxy />IA I
             ON IA.IA_ID = I.IA_ID   
            WHERE IA.LOCATION_ID = :LOCATION_ID
)
select LOCATION_ID, CYC_MARKED_DATE, CYC_START_DATE, CYC_END_DATE, FREEZE_FLAG, ASSIGNED_UPC_MAX_PIECES, 
PITCH_AISLE_ID, IA_ID, SHORT_NAME, WAREHOUSE_LOCATION_ID, CYC_FLAG, VWH_ID, RESTOCK_AISLE_ID,
ASSIGNED_STYLE, ASSIGNED_COLOR, ASSIGNED_DIMENSION, ASSIGNED_SKU_SIZE, ASSIGNED_SKU_ID,
cast(sku_id_xml as varchar2(4000)) as sku_id_xml
  from q1 pivot xml(
  MAX(style) as style, MAX(color) as color, MAX(DIMENSION) as DIMENSION, MAX(SKU_SIZE) as SKU_SIZE, sum(NUMBER_OF_UNITS) as NUMBER_OF_UNITS for SKU_ID in(ANY))
    ";
            var binder = SqlBinder.Create(row => new SkuLocation
            {
                LocationId = row.GetString("LOCATION_ID"),
                CycDate = row.GetDate("CYC_MARKED_DATE"),
                CycStartDate = row.GetDate("CYC_START_DATE"),
                CycEndDate = row.GetDate("CYC_END_DATE"),
                FreezeFlag = row.GetString("FREEZE_FLAG"),
                MaxPieces = row.GetInteger("ASSIGNED_UPC_MAX_PIECES"),
                PitchAisle = row.GetString("PITCH_AISLE_ID"),
                IaId = row.GetString("IA_ID"),
                AreaShortName = row.GetString("SHORT_NAME"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                CycFlag = row.GetString("CYC_FLAG"),
                VwhId = row.GetString("VWH_ID"),
                RestockAisle = row.GetString("RESTOCK_AISLE_ID"),
                AssignedStyle = row.GetString("ASSIGNED_STYLE"),
                AssignedColor = row.GetString("ASSIGNED_COLOR"),
                AssignedDimension = row.GetString("ASSIGNED_DIMENSION"),
                AssignedSkuSize = row.GetString("ASSIGNED_SKU_SIZE"),
                AssignedSkuId = row.GetInteger("ASSIGNED_SKU_ID"),
                SkusAtLocation = MapSkuLocationXml(row.GetString("sku_id_xml"))
            }).Parameter("LOCATION_ID", locationId);
            var x = _db.ExecuteSingle(QUERY_SKU_LOCATION_INFO, binder);
            return x;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Sample XML which this function parses
        /// <![CDATA[
        /// 
        ///<PivotSet>
        ///  <item>
        ///    <column name="SKU_ID">930</column>
        ///    <column name="STYLE">0455</column>
        ///    <column name="COLOR">CGS</column>
        ///    <column name="DIMENSION">C</column>
        ///    <column name="SKU_SIZE">36</column>
        ///    <column name="NUMBER_OF_UNITS">2</column>
        ///  </item>
        ///  <item>
        ///    <column name="SKU_ID">3910</column>
        ///    <column name="STYLE">07959</column>
        ///    <column name="COLOR">WH</column>
        ///    <column name="DIMENSION">A</column>
        ///    <column name="SKU_SIZE">34</column>
        ///    <column name="NUMBER_OF_UNITS">100</column>
        ///  </item>
        ///</PivotSet>
        /// ]]>
        /// <para>
        /// When there are no SKUs at location, we still get XML with all empty values
        /// </para>
        /// <![CDATA[
        ///<PivotSet>
        ///  <item>
        ///    <column name="SKU_ID"></column>
        ///    <column name="STYLE"></column>
        ///    <column name="COLOR"></column>
        ///    <column name="DIMENSION"></column>
        ///    <column name="SKU_SIZE"></column>
        ///    <column name="NUMBER_OF_UNITS"></column>
        ///  </item>
        ///</PivotSet>
        /// ]]>
        /// </remarks>
        private IList<SkuLocationSku> MapSkuLocationXml(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                // Return empty list
                return new SkuLocationSku[0];
            }
            var elem = XElement.Parse(data);
            var query = from item in elem.Elements("item")
                        let cols = item.Elements("column")
                        where !string.IsNullOrWhiteSpace(cols.First(p => (string)p.Attribute("name") == "SKU_ID").Value)
                        select new SkuLocationSku
                        {
                            SkuId = (int)cols.First(p => (string)p.Attribute("name") == "SKU_ID"),
                            Style = (string)cols.First(p => (string)p.Attribute("name") == "STYLE"),
                            Color = (string)cols.First(p => (string)p.Attribute("name") == "COLOR"),
                            Dimension = (string)cols.First(p => (string)p.Attribute("name") == "DIMENSION"),
                            SkuSize = (string)cols.First(p => (string)p.Attribute("name") == "SKU_SIZE"),
                            Pieces = (int)cols.First(p => (string)p.Attribute("name") == "NUMBER_OF_UNITS")
                        };
            return query.ToList();
        }

        /// <summary>
        /// This function gets number of boxes per pallet on sku location
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public IList<SkuLocationPallet> GetPalletsOfSkuLocation(string locationId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"                               
                 SELECT   B.PALLET_ID AS PALLET_ID,
                 COUNT(DISTINCT B.UCC128_ID) AS TOTALBOXES,
                 C.CUSTOMER_ID AS CUSTOMER_ID,
                 MAX(C.NAME)   AS CUSTOMER_NAME      
                   FROM <proxy />BOX B
                  INNER JOIN <proxy />BOXDET BD
                     ON B.UCC128_ID = BD.UCC128_ID
                     LEFT OUTER JOIN  <proxy />PS P
                     ON B.PICKSLIP_ID = P.PICKSLIP_ID
                     LEFT OUTER JOIN <proxy />CUST C
                     ON P.CUSTOMER_ID = C.CUSTOMER_ID
                  WHERE B.LOCATION_ID = :LOCATION_ID and b.stop_process_date is null
                    AND B.PALLET_ID IS NOT NULL
                  GROUP BY B.PALLET_ID,C.CUSTOMER_ID 
                  ORDER BY B.PALLET_ID
            ";
            var binder = SqlBinder.Create(row => new SkuLocationPallet
            {
                TotalBoxes = row.GetInteger("TOTALBOXES") ?? 0,
                PalletId = row.GetString("PALLET_ID"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME")

            }).Parameter("LOCATION_ID", locationId);
            return _db.ExecuteReader(QUERY, binder);

        }


        /// <summary>
        /// This function returns audit of SKU assignment and unassignment on particular location.
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public IList<LocationAudit> GetLocAssignUnassignAudit(string locationId)
        {
            const string QUERY = @"
            SELECT ACTION_PERFORMED   AS ACTION_PERFORMED,
                                     DATE_CREATED       AS DATE_CREATED,
                                     MODULE_CODE        AS MODULE_CODE,
                                     CREATED_BY         AS CREATED_BY,
                                     IACONTENT_ID       AS IACONTENT_ID
                                FROM <proxy />LOCATION_AUDIT
                               WHERE ACTION_PERFORMED IN
                                     ('SKU_UNASSIGNED','SKU_ASSIGNED', 'SKUCHANGED')
                                 AND LOCATION_ID = :LOCATION_ID
                               ORDER BY DATE_CREATED DESC
            ";
            var binder = SqlBinder.Create(row => new LocationAudit
            {
                CreatedBy = row.GetString("CREATED_BY"),
                ActionPerformed = row.GetString("ACTION_PERFORMED"),
                DateCreated = row.GetDate("DATE_CREATED"),
                ModuleCode = row.GetString("MODULE_CODE"),
                UpcCode = row.GetString("IACONTENT_ID")
            }).Parameter("LOCATION_ID", locationId);

            return _db.ExecuteReader(QUERY, binder, 500);
        }

        /// <summary>
        /// /// This function returns audit of pieces in a particular location.
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public IList<LocationAudit> GetLocationsInventoryAudit(string locationId)
        {
            const string QUERY = @"
           SELECT                    ACTION_PERFORMED   AS ACTION_PERFORMED,
                                     DATE_CREATED       AS DATE_CREATED,
                                     MODULE_CODE        AS MODULE_CODE,
                                     CREATED_BY         AS CREATED_BY,
                                     IACONTENT_ID       AS IACONTENT_ID,
                                     NUMBER_OF_UNITS    AS NUMBER_OF_UNITS,
                                     TRANSACTION_PIECES AS TRANSACTION_PIECES,
                                      MS.SKU_ID AS SKU_ID,
                                     MS.STYLE  AS STYLE,
                                     MS.COLOR AS COLOR,
                                     MS.DIMENSION AS DIMENSION,
                                     MS.SKU_SIZE AS SKU_SIZE
                                FROM <proxy />LOCATION_AUDIT LA
                                LEFT OUTER JOIN MASTER_SKU MS
                                ON  LA.IACONTENT_ID =   MS.UPC_CODE                            
                               WHERE ACTION_PERFORMED IN ('SKUADDED','SKUREMOVED','INSERTED')
                                 AND LOCATION_ID = :LOCATION_ID
                               ORDER BY DATE_CREATED DESC
            ";
            var binder = SqlBinder.Create(row => new LocationAudit
            {
                CreatedBy = row.GetString("CREATED_BY"),
                ActionPerformed = row.GetString("ACTION_PERFORMED"),
                DateCreated = row.GetDate("DATE_CREATED"),
                ModuleCode = row.GetString("MODULE_CODE"),
                UpcCode = row.GetString("IACONTENT_ID"),
                Pieces = row.GetInteger("NUMBER_OF_UNITS"),
                TransactionPieces = row.GetInteger("TRANSACTION_PIECES"),
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                SkuId = row.GetInteger("SKU_ID").Value
            }).Parameter("LOCATION_ID", locationId);

            return _db.ExecuteReader(QUERY, binder, 500);
        }

        public IList<SkuAreaHeadline> GetSkuAreaList()
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                                SELECT IA.IA_ID AS IA_ID,
                                        MAX(IA.SHORT_NAME) as SHORT_NAME,
                                       MAX(IA.SHORT_DESCRIPTION) AS SHORT_DESCRIPTION,
                                       COUNT(DISTINCT IAL.LOCATION_ID) AS NUMBER_OF_LOCATIONS
                                  FROM <proxy />IA IA
                                  LEFT OUTER JOIN <proxy />IALOC IAL
                                    ON IA.IA_ID = IAL.IA_ID
                                 GROUP BY IA.IA_ID
                                    ";
            var binder = SqlBinder.Create(row => new SkuAreaHeadline
            {
                IaId = row.GetString("IA_ID"),
                ShortName = row.GetString("SHORT_NAME"),
                Description = row.GetString("SHORT_DESCRIPTION"),
                NumberOfLocations = row.GetInteger("NUMBER_OF_LOCATIONS")
            });

            return _db.ExecuteReader(QUERY, binder);
        }


        public IList<SkuLocationHeadline> GetSkuLocationList()
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                                 SELECT IL.LOCATION_ID,
                                       MAX(MSKU.STYLE) AS STYLE,
                                       MAX(MSKU.COLOR) AS COLOR,
                                       MAX(MSKU.DIMENSION) AS DIMENSION,
                                       MAX(MSKU.SKU_SIZE) AS SKU_SIZE,
                                       MAX(MSKU.SKU_ID) AS SKU_ID,
                                       MAX(IA.WAREHOUSE_LOCATION_ID) AS BUILDING,
                                       MAX(IL.IA_ID) AS IA_ID,
                                       MAX(IA.SHORT_NAME) AS AREA_SHORT_NAME,
                                       SUM(IC.NUMBER_OF_UNITS) AS ASSIGNED_UPC_MAX_PIECES

                                  FROM IALOC IL
                                  LEFT OUTER JOIN IA
                                    ON IA.IA_ID = IL.IA_ID
                                  LEFT OUTER JOIN MASTER_SKU MSKU
                                    ON IL.ASSIGNED_UPC_CODE = MSKU.UPC_CODE
                                  LEFT OUTER JOIN IALOC_CONTENT IC
                                    ON IL.IA_ID = IC.IA_ID
                                   AND IL.ASSIGNED_UPC_CODE = IC.IACONTENT_ID
                                   AND IL.LOCATION_ID = IC.LOCATION_ID
                                   AND IL.LOCATION_TYPE = 'RAIL'
                                   AND IL.CAN_ASSIGN_SKU = '1'
                                 GROUP BY IL.LOCATION_ID, MSKU.SKU_ID
                                 ORDER BY SUM(IC.NUMBER_OF_UNITS) DESC NULLS LAST";
            var binder = SqlBinder.Create(row => new SkuLocationHeadline
            {
                LocationId = row.GetString("LOCATION_ID"),
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                SkuId = row.GetInteger("SKU_ID") ?? 0,
                BuildingId = row.GetString("BUILDING"),
                IaId = row.GetString("IA_ID"),
                AreaShortName = row.GetString("AREA_SHORT_NAME"),
                MaxPieces = row.GetInteger("ASSIGNED_UPC_MAX_PIECES")             
            });

            return _db.ExecuteReader(QUERY, binder,200);
        }
    }
}