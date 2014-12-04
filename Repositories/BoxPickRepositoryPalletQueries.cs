using DcmsMobile.BoxPick.Models;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Routing;

namespace DcmsMobile.BoxPick.Repositories
{
    public partial class BoxPickRepository : IDisposable
    {
        #region Initialize

        protected readonly OracleDatastore _db;
        public OracleDatastore Db
        {
            get
            {
                return _db;
            }
        }
        public BoxPickRepository(RequestContext requestContext)
        {
            Contract.Assert(ConfigurationManager.ConnectionStrings["dcms8"] != null);
            var store = new OracleDatastore(requestContext.HttpContext.Trace);
            store.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString, requestContext.HttpContext.User.Identity.Name);
            store.ModuleName = "BOXPICK";
            store.ClientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                requestContext.HttpContext.Request.UserHostName;
            _db = store;
        }

        public BoxPickRepository(OracleDatastore db)
        {
            _db = db;
        }

        public void Dispose()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
        }
        #endregion

        /// <summary>
        /// Retrieves current pallet info, if pallet data is found then it will also populates carton location
        /// </summary>
        public Pallet RetrievePalletInfo(string palletId)
        {
            if (string.IsNullOrEmpty(palletId))
            {
                throw new ArgumentNullException("PalletId is null");
            }

            const string QUERY = @"
                begin
                  :result := <proxy />pkg_boxpick.get_pallet_info(apallet_id => :apallet_id);
                end;
                ";
            var binder = SqlBinder.Create(row => new Pallet
            {
                PalletId = row.GetString("PALLET_ID"),
                TotalBoxCount = row.GetInteger("BOX_COUNT") ?? 0,
                PickableBoxCount = row.GetInteger("PICKABLE_BOX_COUNT") ?? 0,
                PickedBoxCount = row.GetInteger("PICKED_BOX_COUNT") ?? 0,
                ReservedCartonCount = row.GetInteger("RESERVED_CARTON_COUNT") ?? 0,
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                DestinationArea = row.GetString("DESTINATION_AREA"),
                DestAreaShortName = row.GetString("DEST_AREA_SHORT_NAME"),
                PickModeText = row.GetString("PICK_MODE"),
                QueryTime = row.GetDate("CARTONPICKSTARTDATE"),
                CartonSourceArea = row.GetString("CARTON_SOURCE_AREA"),
                SourceAreaShortName = row.GetString("SOURCE_AREA_SHORT_NAME"),
                CountRequiredVAS = row.GetInteger("REQUIRED_VAS_COUNT").Value,
                BoxToPick = new Box
                {
                    UccId = row.GetString("BOX_UCC128_ID"),
                    IaId = row.GetString("BOX_IA_ID"),
                    VwhId = row.GetString("BOX_VWHID"),
                    Pieces = row.GetInteger("BOX_EXPECTED_PIECES") ?? 0,
                    QualityCode = row.GetString("BOX_QUALITY_CODE"),
                    AssociatedCarton = new Carton
                    {
                        CartonId = row.GetString("BOX_CARTON_CARTON_ID"),
                        LocationId = row.GetString("BOX_CARTON_LOCATION_ID"),
                        VwhId = row.GetString("BOX_CARTON_VWHID"),
                        QualityCode = row.GetString("BOX_CARTON_QUALITYCODE"),
                        AssociatedPalletId = row.GetString("CARTON_PALLET"),
                        SkuInCarton = new Sku
                        {
                            Color = row.GetString("BOX_CARTON_COLOR"),
                            Dimension = row.GetString("BOX_CARTON_DIMENSION"),
                            SkuSize = row.GetString("BOX_CARTON_SKU_SIZE"),
                            Style = row.GetString("BOX_CARTON_STYLE")
                        }
                    },
                    SkuInBox = new Sku
                    {
                        Color = row.GetString("BOX_COLOR"),
                        Dimension = row.GetString("BOX_DIMENSION"),
                        SkuId = row.GetInteger("BOX_SKU_ID").Value,
                        SkuSize = row.GetString("BOX_SKU_SIZE"),
                        Style = row.GetString("BOX_STYLE")
                    }
                }
            }).Parameter("apallet_id", palletId).OutRefCursorParameter("result");

            var pallet = _db.ExecuteSingle(QUERY, binder);
            if (pallet != null)
            {
                pallet.CartonLocations = this.RetrieveCartonLocationsForPallet(palletId).ToArray();
            }
            return pallet;
        }

        public IEnumerable<CartonLocation> RetrieveCartonLocationsForPallet(string palletId)
        {
            if (string.IsNullOrEmpty(palletId))
            {
                throw new ArgumentNullException("PalletId is null");
            }

            const string QUERY = @"
                begin
                  :result := <proxy />pkg_boxpick.get_pallet_carton_locations(apallet_id => :apallet_id);
                end;
                ";

            var binder = SqlBinder.Create(row => new CartonLocation
            {
                CartonLocationId = row.GetString("carton_location_id"),
                CartonStorageArea = row.GetString("carton_storage_area"),
                CountCartonsToPick = row.GetInteger("count_cartons_to_pick").Value,
                PiecesPerCarton = row.GetInteger("pieces_per_carton").Value,
                SkuToPick = new Sku
                {
                    Style = row.GetString("style"),
                    Color = row.GetString("color"),
                    Dimension = row.GetString("dimension"),
                    SkuSize = row.GetString("sku_size"),
                    SkuId = row.GetInteger("sku_id").Value
                }
            }).Parameter("apallet_id", palletId)
                .OutRefCursorParameter("result");
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Returns Box if the passed carton can be placed on passed pallet. 
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="palletId"></param>
        /// <param name="ucc128Id"> If this is passed we prefer this ucc.</param>
        /// <returns>Box</returns>

        internal Box GetBoxForCarton(string cartonId, string palletId, string ucc128Id)
        {

            if (string.IsNullOrEmpty(cartonId))
            {
                throw new ArgumentNullException("Carton Id is null");
            }

            if (string.IsNullOrEmpty(palletId))
            {
                throw new ArgumentNullException("Pallet Id is null");
            }

            const string QUERY = @"
                            WITH BOX_DETAIL AS
                                 (SELECT B.UCC128_ID,
                                         BD.PICKSLIP_ID,
                                         BD.SKU_ID,
                                         BD.EXPECTED_PIECES,
                                         B.VWH_ID
                                    FROM <proxy />BOXDET BD
                                   INNER JOIN <proxy />BOX B
                                      ON B.PICKSLIP_ID = BD.PICKSLIP_ID
                                     AND B.UCC128_ID = BD.UCC128_ID
                                   WHERE B.PALLET_ID = :PALLET_ID
                                     AND B.STOP_PROCESS_DATE IS NULL
                                     AND B.PITCHING_END_DATE IS NULL
                                   ORDER BY CASE
                                              WHEN BD.UCC128_ID = :UCC128_ID THEN
                                               1
                                            END),
                                SKU_DETAIL AS
                                 (SELECT PD.SKU_ID          AS SKU_ID,
                                         PD.QUALITY_CODE    AS QUALITY_CODE,
                                         BT.EXPECTED_PIECES AS EXPECTED_PIECES,
                                         BT.VWH_ID          AS VWH_ID,
                                         BT.UCC128_ID       AS UCC128_ID
                                    FROM <proxy />PSDET PD
                                   INNER JOIN <proxy />PS
                                      ON PS.PICKSLIP_ID = PD.PICKSLIP_ID
                                   INNER JOIN BOX_DETAIL BT
                                      ON PD.PICKSLIP_ID = BT.PICKSLIP_ID
                                     AND PD.SKU_ID = BT.SKU_ID)
                                SELECT SD.UCC128_ID, CTN.CARTON_ID, CTN.LOCATION_ID
                                  FROM <proxy />SRC_CARTON CTN
                                 INNER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
                                    ON CTN.CARTON_ID = CTNDET.CARTON_ID
                                 INNER JOIN SKU_DETAIL SD
                                    ON CTNDET.SKU_ID = SD.SKU_ID
                                   AND CTN.VWH_ID = SD.VWH_ID
                                   AND CTNDET.QUANTITY = SD.EXPECTED_PIECES
                                   AND CTN.QUALITY_CODE = SD.QUALITY_CODE
                                 WHERE CTN.CARTON_ID = :CARTON_ID
                                   AND CTN.WORK_NEEDED_XML IS NULL
                                   AND ROWNUM = 1";

            var binder = SqlBinder.Create(row => new Box
            {
                UccId = row.GetString("UCC128_ID"),
                AssociatedCarton = new Carton
                {
                    CartonId = row.GetString("CARTON_ID"),
                    LocationId = row.GetString("LOCATION_ID")
                }
            })
                .Parameter("CARTON_ID", cartonId)
                .Parameter("PALLET_ID", palletId)
                .Parameter("UCC128_ID", ucc128Id);
            return _db.ExecuteSingle(QUERY, binder);

        }
    }
}




//$Id$