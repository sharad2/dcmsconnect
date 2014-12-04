using DcmsMobile.BoxPick.Models;
using DcmsMobile.BoxPick.Models.MainContent;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;

namespace DcmsMobile.BoxPick.Repositories
{
    public class MainContentRepository : IDisposable
    {
        #region Intialization

        private readonly OracleDatastore _db;

        public OracleDatastore Db
        {
            get
            {
                return _db;
            }
        }

        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="moduleName"></param>
        /// <param name="clientInfo"></param>
        /// <param name="trace"></param>
        public MainContentRepository(string userName, string moduleName, string clientInfo, TraceContext trace)
        {
            Contract.Assert(ConfigurationManager.ConnectionStrings["dcms8"] != null);
            var store = new OracleDatastore(trace);
            store.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString,
                userName);
            store.ModuleName = moduleName;
            store.ClientInfo = clientInfo;
            _db = store;
        }

        /// <summary>
        /// For use in unit tests
        /// </summary>
        /// <param name="db"></param>
        public MainContentRepository(OracleDatastore db)
        {
            _db = db;
        }

        public void Dispose()
        {
            var dis = _db as IDisposable;
            if (dis != null)
            {
                dis.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// Returns all possible pallets which can be picked
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Activity> GetPendingActivity()
        {
            var QUERY = @"
            SELECT PS.WAREHOUSE_LOCATION_ID     AS WAREHOUSE_LOCATION_ID,
                   BKT.SHIP_IA_ID               AS SHIP_IA_ID,
                   BKT.PICK_MODE                AS PICK_MODE,
                   COUNT(DISTINCT B.PALLET_ID)  AS COUNT_PALLETS,
                   COUNT(DISTINCT B.UCC128_ID)  AS PICKABLE_BOX_COUNT,
                   MAX(IA.SHORT_NAME)           AS SHORT_NAME
              FROM <proxy />BOX B
             INNER JOIN <proxy />PS PS
                ON PS.PICKSLIP_ID = B.PICKSLIP_ID
             INNER JOIN <proxy />BUCKET BKT
                ON PS.BUCKET_ID = BKT.BUCKET_ID
             INNER JOIN <proxy />IA IA
                ON IA.IA_ID = BKT.SHIP_IA_ID
             WHERE B.STOP_PROCESS_DATE IS NULL
               AND B.PALLET_ID IS NOT NULL
               AND B.STOP_PROCESS_DATE IS NULL
               AND B.IA_ID IS NULL
               AND BKT.AVAILABLE_FOR_PITCHING = 'Y'
               AND BKT.PICK_MODE IN ('PITCHING', 'ADRE', 'ADREPPWSS')
               AND BKT.FREEZE IS NULL
               AND BKT.PULL_TO_DOCK = 'Y'
             GROUP BY PS.WAREHOUSE_LOCATION_ID, BKT.PICK_MODE, SHIP_IA_ID
             ORDER BY PS.WAREHOUSE_LOCATION_ID, SHIP_IA_ID, BKT.PICK_MODE
        ";
            var binder = SqlBinder.Create(row => new Activity
            {
                PickableBoxCount = row.GetInteger("PICKABLE_BOX_COUNT").Value,
                PickModeText = row.GetString("PICK_MODE"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                DestinationArea = row.GetString("SHIP_IA_ID"),
                AreaShortName = row.GetString("SHORT_NAME"),
                CountPallets = row.GetInteger("COUNT_PALLETS").Value
            });
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Get the details of boxes on the pallet
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns>List of boxes on the pallet.</returns>
        public IEnumerable<Box> GetBoxesOnPallet(string palletId)
        {
            if (string.IsNullOrEmpty(palletId))
            {
                throw new ArgumentNullException("PalletId is null");
            }

            const string QUERY = @"
            select b.ucc128_id,
                   b.ia_id            as ia_id,
                   b.vwh_id           as vwh_id,
                   ctn.location_id    as carton_location_id,
                   bd.expected_pieces AS expected_pieces,
                   ctndet.carton_id   AS carton_carton_id,
                   msku.style         as style,
                   msku.SKU_ID        as SKU_ID,
                   msku.color         as color,
                   msku.dimension     as dimension,
                   msku.sku_size      as sku_size,
                   mskuctn.style      as carton_style_,
                   mskuctn.color      as carton_color_,
                   mskuctn.dimension  as carton_dimension_,
                   mskuctn.sku_size   as carton_sku_size_,
                   mskuctn.sku_id     as carton_sku_id,       
                   ctn.quality_code   as quality_code
              from <proxy />box b
             inner join <proxy />boxdet bd
                on b.ucc128_id = bd.ucc128_id
               and b.pickslip_id = bd.pickslip_id
             inner join <proxy />master_sku msku
                on bd.upc_code = msku.upc_code
             left outer join <proxy />src_carton_detail ctndet
                on ctndet.req_process_id = TO_NUMBER(substr(b.ucc128_id, 11, 9))
               and ctndet.req_module_code = 'BOXPICK'
              left outer join <proxy />src_carton ctn
                on ctn.carton_id = ctndet.carton_id
              left outer join <proxy />master_sku mskuctn
                on mskuctn.sku_id = ctndet.sku_id
             where b.pallet_id = :pallet_id
         ";
            var binder = SqlBinder.Create(row => new Box
            {
                UccId = row.GetString("ucc128_id"),
                IaId = row.GetString("ia_id"),
                VwhId = row.GetString("vwh_id"),
                Pieces = row.GetInteger("expected_pieces") ?? 0,
                QualityCode = row.GetString("quality_code"),
                AssociatedCarton = new Carton
                {
                    CartonId = row.GetString("carton_carton_id"),
                    LocationId = row.GetString("carton_location_id"),
                    SkuInCarton = row.GetInteger("carton_sku_id") == null ? null : new Sku
                    {
                        Style = row.GetString("carton_style_"),
                        Color = row.GetString("carton_color_"),
                        Dimension = row.GetString("carton_dimension_"),
                        SkuSize = row.GetString("carton_sku_size_"),
                        SkuId = row.GetInteger("carton_sku_id").Value
                    }
                },
                SkuInBox = row.GetInteger("SKU_ID") == null ? null : new Sku
                {
                    Style = row.GetString("style"),
                    Color = row.GetString("color"),
                    Dimension = row.GetString("dimension"),
                    SkuSize = row.GetString("sku_size"),
                    SkuId = row.GetInteger("SKU_ID").Value
                }
            }).Parameter("pallet_id", palletId);
            return _db.ExecuteReader(QUERY, binder).ToArray();
        }
    }
}
/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/