using System.Collections.Generic;
using EclipseLibrary.Oracle;

namespace DcmsMobile.DcmsLite.Repository.Receive
{
    public class ReceiveRepository : DcmsLiteRepositoryBase
    {
        internal IEnumerable<InventoryArea> GetRestockAreaList(string buildingId)
        {
            const string QUERY = @"
                SELECT IALOC.RESTOCK_IA_ID           AS RESTOCK_IA_ID,
                       MAX(IA.SHORT_DESCRIPTION)     AS SHORT_DESCRIPTION,
                       MAX(IA.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID
                  FROM <proxy />IALOC IALOC
                 INNER JOIN <proxy />IA IA
                    ON IA.IA_ID = IALOC.IA_ID
                   AND IA.PICKING_AREA_FLAG IS NOT NULL
                 WHERE IALOC.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID
                   AND IALOC.RESTOCK_IA_ID IS NOT NULL
                 GROUP BY IALOC.RESTOCK_IA_ID
                ";
            var binder = SqlBinder.Create(row => new InventoryArea
            {
                AreaId = row.GetString("RESTOCK_IA_ID"),
                Discription = row.GetString("SHORT_DESCRIPTION"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID")
            }).Parameter("WAREHOUSE_LOCATION_ID", buildingId);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// The list is retrieved ordered by receive date desc, vwh_id, shipment_id, instransit_id
        /// </summary>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        internal IEnumerable<AsnSummary> GetAsnListToReceive(string buildingId)
        {
            const string QUERY = @"
                SELECT SCI.INTRANSIT_ID         AS INTRANSIT_ID,
                       SCI.SHIPMENT_ID          AS SHIPMENT_ID,
                       SCI.VWH_ID          AS VWH_ID,
                       SUM(SCI.QUANTITY)        AS PIECES,
                       COUNT(UNIQUE SCI.CARTON_ID)     AS CARTON_COUNT,
                       MIN(SCI.RECEIVED_DATE)   AS RECEIVED_DATE
                  FROM <proxy />SRC_CARTON_INTRANSIT SCI
                  left outer join <proxy />master_style ms
                    on ms.style = sci.style
                 inner join <proxy />tab_label_group tlg
                    on tlg.label_id = ms.label_id
                   and tlg.vwh_id = sci.vwh_id
                 inner join <proxy />tab_warehouse_location twl
                    on twl.label_group = tlg.label_group
                 WHERE twl.warehouse_location_id = :warehouse_location_id
                 GROUP BY SCI.INTRANSIT_ID, SCI.SHIPMENT_ID , ROUND(SCI.RECEIVED_DATE, 'HH'), SCI.VWH_ID  
                 order by MIN(SCI.RECEIVED_DATE) desc nulls first, SCI.VWH_ID, SCI.SHIPMENT_ID, SCI.INTRANSIT_ID nulls last
        ";
            var binder = SqlBinder.Create(row => new AsnSummary
            {
                IntransitId = row.GetDecimal("INTRANSIT_ID").ToString(),
                ShipmentId = row.GetString("SHIPMENT_ID"),
                VwhId = row.GetString("VWH_ID"),
                Pieces = row.GetInteger("PIECES") ?? 0,
                CartonCount = row.GetInteger("CARTON_COUNT") ?? 0,
                ReceivedDate = row.GetDate("RECEIVED_DATE")
            }).Parameter("warehouse_location_id", buildingId);
            return _db.ExecuteReader(QUERY, binder, 200);
        }

        internal int ReceiveCartonsOfAsn(string intransitId, string shipmentId, string restockArea)
        {
            var rowCount = 0;
            const string QUERY = @"
            begin  
                :result := <proxy />pkg_receiving_lite.receive_cartons(areceivingarea_id =&gt; :areceivingarea_id,
                                                                        aintransit_id =&gt; :aintransit_id,
                                                                        ashipment_id =&gt; :ashipment_id);
            end;            
            ";
            var binder = SqlBinder.Create()
                .Parameter("ashipment_id", shipmentId)
                .Parameter("aintransit_id", intransitId)
                .Parameter("areceivingarea_id", restockArea)
                .OutParameter("result", val => rowCount = val ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return rowCount;
        }

        internal int RestockCartons(string areaId)
        {
            var rowCount = 0;
            const string QUERY = @"
            begin             
              :result := <proxy />pkg_receiving_lite.restock_cartons(arestockarea_id =&gt; :arestockarea_id);
            end; 
            ";
            var binder = SqlBinder.Create()
                .Parameter("arestockarea_id", areaId)
                 .OutParameter("result", val => rowCount = val ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return rowCount;
        }
    }
}