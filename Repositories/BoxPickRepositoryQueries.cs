using System;
using System.Collections.Generic;
using DcmsMobile.BoxPick.Models;
using EclipseLibrary.Oracle;

namespace DcmsMobile.BoxPick.Repositories
{
    /// <summary>
    /// Contains static methods to create domain entities
    /// </summary>
    public partial class BoxPickRepository
    {
        /// <summary>
        /// Returns the details of carton
        /// </summary>
        /// <remarks>
        /// Called when validating the currently scanned carton
        /// </remarks>
        /// <param name="cartonId"></param>
        public Carton GetCartonDetails(string cartonId)
        {
            if (string.IsNullOrEmpty(cartonId))
            {
                throw new ArgumentNullException("CartonId is null");
            }

            const string QUERY = @"
                SELECT ctn.carton_id    as carton_id,
                       CTNDET.SKU_ID    as SKU_ID,
                       m.style          as style_,
                       m.color          as color_,
                       m.dimension      as dimension_,
                       m.sku_size       as sku_size_,
                       CTN.QUALITY_CODE as QUALITY_CODE,
                       CTNDET.QUANTITY  as QUANTITY,
                       CTN.VWH_ID       as VWH_ID,
                       ctn.location_id  as location_id,
                       ctn.carton_storage_area as carton_storage_area
                  FROM <proxy />SRC_CARTON CTN
                  left outer JOIN <proxy />SRC_CARTON_DETAIL CTNDET
                    ON CTNDET.CARTON_ID = CTN.CARTON_ID
                 left outer JOIN <proxy />MASTER_SKU M
                    ON M.SKU_ID = CTNDET.SKU_ID
                WHERE ctn.carton_id = :carton_id
            ";
            var binder = SqlBinder.Create(row => new Carton
            {
                CartonId = row.GetString("carton_id"),
                QualityCode = row.GetString("QUALITY_CODE"),
                Pieces = row.GetInteger("QUANTITY") ?? 0,
                VwhId = row.GetString("VWH_ID"),
                LocationId = row.GetString("location_id"),
                StorageArea = row.GetString("carton_storage_area"),
                SkuInCarton = row.GetInteger("SKU_ID") == null ? null : new Sku
                {
                    Style = row.GetString("style_"),
                    Color = row.GetString("color_"),
                    Dimension = row.GetString("dimension_"),
                    SkuSize = row.GetString("sku_size_"),
                    SkuId = row.GetInteger("SKU_ID").Value
                }
            }).Parameter("carton_id", cartonId);

            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// Based on the passed string, returns the context in which picking must happen
        /// </summary>
        /// <param name="scan">Must be either building or area</param>
        /// <returns></returns>
        public PickContext GetPickContext(string scan)
        {
            if (string.IsNullOrEmpty(scan))
            {
                throw new ArgumentNullException("BuildingId is null");
            }

            const string QUERY = @"
                SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                       TIA.WAREHOUSE_LOCATION_ID  AS BUILDING_ID,
                       TIA.SHORT_NAME             AS SHORT_NAME
                  FROM <proxy />TAB_INVENTORY_AREA TIA
                 WHERE TIA.SHORT_NAME = :SCAN
                UNION ALL
                SELECT  NULL                        AS INVENTORY_STORAGE_AREA, 
                        TBL.WAREHOUSE_LOCATION_ID   AS BUILDING_ID,
                        NULL                        AS SHORT_NAME
                  FROM <proxy />TAB_WAREHOUSE_LOCATION TBL
                 WHERE TBL.WAREHOUSE_LOCATION_ID = :SCAN
                ORDER BY INVENTORY_STORAGE_AREA NULLS LAST
            ";
            var binder = SqlBinder.Create(row => new PickContext
            {
                BuildingId = row.GetString("BUILDING_ID"),
                SourceArea = row.GetString("INVENTORY_STORAGE_AREA"),
                SourceAreaShortName = row.GetString("SHORT_NAME")
            }).Parameter("SCAN", scan);
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// This method returns the list of alternate Locations which contain the same sku
        /// when a carton Id passed to it
        /// </summary>
        /// <param name="cartonId"></param>
        /// <returns></returns>
        public IEnumerable<string> GetAlternateLocations(string cartonId)
        {
            const string QUERY = @"
                        WITH SCANNED_CARTON_DETAIL AS
                            (SELECT SCD.SKU_ID,
                                     SCD.QUANTITY,
                                     SC.VWH_ID,
                                     SC.CARTON_STORAGE_AREA,
                                     MSL.WAREHOUSE_LOCATION_ID
                                FROM <proxy />SRC_CARTON SC
                               INNER JOIN <proxy />SRC_CARTON_DETAIL SCD
                                  ON SCD.CARTON_ID = SC.CARTON_ID
                               INNER JOIN <proxy />MASTER_STORAGE_LOCATION MSL
                                  ON MSL.LOCATION_ID = SC.LOCATION_ID
                                 AND MSL.STORAGE_AREA = SC.CARTON_STORAGE_AREA
                               WHERE SC.CARTON_ID = :CARTON_ID)
                            SELECT DISTINCT(SC.LOCATION_ID)
                              FROM <proxy />SRC_CARTON SC
                             INNER JOIN <proxy />SRC_CARTON_DETAIL SCD
                                ON SCD.CARTON_ID = SC.CARTON_ID
                             INNER JOIN SCANNED_CARTON_DETAIL Q1
                                ON SCD.SKU_ID = Q1.SKU_ID
                               AND SCD.QUANTITY = Q1.QUANTITY
                               AND SC.VWH_ID = Q1.VWH_ID
                               AND SC.CARTON_STORAGE_AREA = Q1.CARTON_STORAGE_AREA
                             INNER JOIN <proxy />MASTER_STORAGE_LOCATION MSL
                                ON MSL.LOCATION_ID = SC.LOCATION_ID
                               AND MSL.STORAGE_AREA = SC.CARTON_STORAGE_AREA
                               AND MSL.WAREHOUSE_LOCATION_ID = Q1.WAREHOUSE_LOCATION_ID
                             WHERE SC.LOCATION_ID NOT IN (SELECT LOCATION_ID FROM <proxy />SRC_CARTON WHERE CARTON_ID = :CARTON_ID)
                ";
            var binder = SqlBinder.Create(row => row.GetString("LOCATION_ID"))
                                .Parameter("CARTON_ID", cartonId);
            return _db.ExecuteReader(QUERY, binder);
        }
    }
}



//$Id$