using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Web;
using DcmsMobile.CartonManager.Models;
using DcmsMobile.CartonManager.ViewModels;
using EclipseLibrary.Oracle;

namespace DcmsMobile.CartonManager.Repository
{

    public class CartonManagerRepository : IDisposable
    {
        #region Intialization

        private readonly OracleDatastore _db;

        public OracleDatastore Db
        {
            get { return _db; }
        }

        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="moduleName"></param>
        /// <param name="clientInfo"></param>
        /// <param name="trace"></param>
        public CartonManagerRepository(string userName, string moduleName, string clientInfo, TraceContext trace)
        {
            Contract.Assert(ConfigurationManager.ConnectionStrings["dcms4"] != null);
            var store = new OracleDatastore(trace);
            store.CreateConnection(ConfigurationManager.ConnectionStrings["dcms4"].ConnectionString,
                                   userName);
            store.ModuleName = moduleName;
            store.ClientInfo = clientInfo;
            _db = store;
        }

        /// <summary>
        /// For use in unit tests
        /// </summary>
        /// <param name="db"></param>
        public CartonManagerRepository(OracleDatastore db)
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
        /// Gets all carton areas if you do not pass any flag.
        /// If you pass a flag gets area according to passed flag. 
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="shortName"> </param>
        /// <param name="buildingId"> </param>
        /// <returns></returns>
        public IEnumerable<CartonArea> GetCartonAreas(string areaId, string shortName, string buildingId)
        {
            const string QUERY =
                @"
                  SELECT TIA.INVENTORY_STORAGE_AREA  AS INVENTORY_STORAGE_AREA,
                         TIA.DESCRIPTION             AS DESCRIPTION,
                         TIA.WAREHOUSE_LOCATION_ID   AS BUILDING,
				         TWL.RECEIVING_PALLET_LIMIT  AS RECEIVING_PALLET_LIMIT,
                         TIA.LOCATION_NUMBERING_FLAG AS LOCATION_NUMBERING_FLAG,
                         TIA.SHORT_NAME              AS SHORT_NAME
		            FROM <proxy />TAB_INVENTORY_AREA TIA
		            LEFT OUTER JOIN <proxy />TAB_WAREHOUSE_LOCATION TWL
		              ON TWL.WAREHOUSE_LOCATION_ID = TIA.WAREHOUSE_LOCATION_ID
	               WHERE TIA.STORES_WHAT = 'CTN'               
                <if>
                    AND TIA.INVENTORY_STORAGE_AREA = :INVENTORY_STORAGE_AREA
                </if>  
                <if>
                    AND TIA.SHORT_NAME = :SHORT_NAME
                </if> 
                 <if>
                    AND TIA.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID
                  </if>                   
                ORDER BY TIA.INVENTORY_STORAGE_AREA
        ";
            var binder = SqlBinder.Create(row => new CartonArea
                {
                AreaId = row.GetString("INVENTORY_STORAGE_AREA"),
                Description = row.GetString("DESCRIPTION"),
                Building = row.GetString("BUILDING"),
                PalletLimit = row.GetInteger("RECEIVING_PALLET_LIMIT"),
                ShortName = row.GetString("SHORT_NAME"),
                IsNumberedLocationArea = row.GetString("LOCATION_NUMBERING_FLAG") == "Y"
            }).Parameter("INVENTORY_STORAGE_AREA", areaId).Parameter("SHORT_NAME", shortName).Parameter("WAREHOUSE_LOCATION_ID", buildingId);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// getting the list of Zebra printers, for Select Printer Drop down list.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetZebraPrinters()
        {
            const string QUERY =
                @"
            SELECT TABPRINTER.NAME        AS NAME,
                   TABPRINTER.DESCRIPTION AS DESCRIPTION
              FROM <proxy />TAB_PRINTER TABPRINTER
             WHERE UPPER(PRINTER_TYPE) = 'ZEBRA'
             ORDER BY LOWER(NAME)
        ";
            var binder = SqlBinder.Create(row => new CodeDescriptionModel()
                {
                    Code = row.GetString("NAME"),
                    Description = row.GetString("DESCRIPTION")
                });

            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Getting the list of Quality Codes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetQualityCodes()
        {
            const string QUERY =
                @"
               SELECT QC.QUALITY_CODE AS QUALITY_CODE,
                       NVL(QC.DESCRIPTION, QC.QUALITY_CODE) AS DESCRIPTION
                 FROM <proxy />TAB_QUALITY_CODE QC
                ORDER BY QC.QUALITY_RANK ASC, QC.QUALITY_CODE
        ";

            var binder = SqlBinder.Create(row => new CodeDescriptionModel()
                {
                    Code = row.GetString("QUALITY_CODE"),
                    Description = row.GetString("DESCRIPTION")
                });
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Get the list of PriceSeasonCode.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetPriceSeasonCode()
        {
            const string QUERY =
                            @"SELECT TPS.PRICE_SEASON_CODE, TPS.DESCRIPTION
                              FROM <proxy />TAB_PRICE_SEASON TPS
                             ORDER BY TPS.PRICE_SEASON_CODE";
            var binder = SqlBinder.Create(row => new CodeDescriptionModel()
                {
                    Code = row.GetString("PRICE_SEASON_CODE"),
                    Description = row.GetString("DESCRIPTION")
                });

            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Getting the list of Virtual Warehouse
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetVwhList()
        {
            const string QUERY =
                @"
               SELECT VWH_ID AS VWH_ID, 
                       DESCRIPTION AS DESCRIPTION
                 FROM <proxy />TAB_VIRTUAL_WAREHOUSE
                ORDER BY VWH_ID
        ";

            var binder = SqlBinder.Create(row => new CodeDescriptionModel()
            {
                Code = row.GetString("VWH_ID"),
                Description = row.GetString("DESCRIPTION")
            });
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Get the list of Code.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetRichterReasonCodes()
        {
            const string QUERY =
                @"
               SELECT RCODE.RICHTER_REASON_CODE AS RICHTER_REASON_CODE, 
                      RCODE.DESCRIPTION AS DESCRIPTION
                    FROM <proxy />TAB_RICHTERADJ_REASON_CODE RCODE
                     ORDER BY RCODE.RICHTER_REASON_CODE
        ";

            var binder = SqlBinder.Create(row => new CodeDescriptionModel()
                {
                    Code = row.GetString("RICHTER_REASON_CODE"),
                    Description = row.GetString("DESCRIPTION")
                });
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Updates quality,SKU,pieces of the carton.
        /// </summary>
        /// <param name="carton"></param>
        /// <param name="updateFlags"></param>
        /// <param name="reasonCode"></param>
        public void UpdateCarton(Carton carton, CartonUpdateFlags updateFlags, string reasonCode)
        {
            const string QUERY = @"  
                        DECLARE
                            Lsku_rec  <proxy />pkg_inv_3.SKU_REC;
                            LRelated_TRansaction_Id NUMBER(10);
                        BEGIN
                            Lsku_rec.sku_id := :trgSKU;
                            Lsku_rec.vwh_id := :trgVwh_id;
                            Lsku_rec.quality_code := :trgQuality;
                            LRelated_TRansaction_Id := <proxy />pkg_inv_3.editcarton(acarton_id =&gt; :cartonId,
                                                                          atarget_sku =&gt; Lsku_rec,
                                                                          anew_pieces =&gt; :trgPieces,
                                                                          arelated_transaction_id =&gt; NULL,
                                                                          areason_code =&gt; :reasonCode );


                        <if c='$priceseasoncode'>
                               UPDATE <proxy />SRC_CARTON
                                 SET PRICE_SEASON_CODE = :priceseasoncode
                               WHERE CARTON_ID = :cartonId;
                        </if>   
                        <if c='$completeRework'>
                           begin
                              LRelated_TRansaction_Id := <proxy />pkg_carton_work_2.mark_work_complete(acarton_id =&gt; :cartonId,
                                                                  arelated_transaction_id =&gt; NULL);
                           end;
                        </if>
                           <if c='$abandonRework'>
                                    begin
                                     
                                           LRelated_TRansaction_Id := <proxy />pkg_carton_work_2.undo_work(acarton_id =&gt; :cartonId,
                                           arelated_transaction_id =&gt; NULL);
                                    end;
                        </if>
                       
                        UPDATE <proxy />SRC_CARTON_DETAIL SCD
                        SET SCD.REQ_PROCESS_ID = NULL,
                            SCD.REQ_MODULE_CODE= NULL,
                            SCD.REQ_LINE_NUMBER =NULL
                        WHERE SCD.CARTON_ID =:cartonId;
                              
                        UPDATE <proxy />SRC_CARTON SC
                            SET SUSPENSE_DATE = NULL                                 
                        WHERE SC.CARTON_ID = :cartonId;
                        END; ";
            if (updateFlags.HasFlag(CartonUpdateFlags.MarkReworkComplete) && updateFlags.HasFlag(CartonUpdateFlags.AbandonRework))
            {
                throw new ProviderException("Mark rework complete and abandon rework can not be performed on same carton. ");
            }

            var binder = SqlBinder.Create().Parameter("cartonId", carton.CartonId)
                .Parameter("trgVwh_id", updateFlags.HasFlag(CartonUpdateFlags.Vwh) ? carton.VwhId : null)
                .Parameter("trgSKU", updateFlags.HasFlag(CartonUpdateFlags.Sku) ? (int?)carton.SkuInCarton.SkuId : null)
                .Parameter("trgPieces", updateFlags.HasFlag(CartonUpdateFlags.Pieces) ? (int?)carton.Pieces : null)
                .Parameter("reasonCode", reasonCode)
                .Parameter("trgQuality", updateFlags.HasFlag(CartonUpdateFlags.Quality) ? carton.QualityCode : null)
                .Parameter("completeRework", updateFlags.HasFlag(CartonUpdateFlags.MarkReworkComplete) ? carton.CartonId : null)
                .Parameter("abandonRework", updateFlags.HasFlag(CartonUpdateFlags.AbandonRework) ? carton.CartonId : null)
                .Parameter("priceseasoncode", updateFlags.HasFlag(CartonUpdateFlags.PriceSeasonCode) ? carton.PriceSeasonCode : null);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// This method updates the carton storage and pallet id of a carton
        /// </summary>
        /// <param name="carton"></param>
        /// <param name="moveFlags"></param>
        public void MoveCarton(Carton carton, CartonUpdateFlags moveFlags)
        {
            const string QUERY = @"
            BEGIN
            <if c='$carton_area'>
                UPDATE <proxy />SRC_CARTON SC SET SC.SUSPENSE_DATE = NULL, SC.CARTON_STORAGE_AREA = :carton_area ,SC.location_id = :location_id  WHERE SC.CARTON_ID = :carton_id;
            </if>
            <if c='$pallet_id'>
                UPDATE <proxy />SRC_CARTON SC SET SC.SUSPENSE_DATE = NULL, SC.PALLET_ID = :pallet_id WHERE SC.CARTON_ID = :carton_id;   
            </if>
             <if c='$removePallet'>
                UPDATE <proxy />SRC_CARTON SC SET SC.SUSPENSE_DATE = NULL, SC.PALLET_ID = NULL WHERE SC.CARTON_ID = :carton_id;
              </if>
              <if c='$updateLocation'>
                UPDATE <proxy />SRC_CARTON SC SET SC.SUSPENSE_DATE = NULL, SC.location_id = :location_id WHERE SC.CARTON_ID = :carton_id;
               </if>
            END;
            ";
            var binder = SqlBinder.Create().Parameter("carton_id", carton.CartonId)
                .Parameter("carton_area", moveFlags.HasFlag(CartonUpdateFlags.Area) ? carton.CartonArea.AreaId : null)
                .Parameter("pallet_id", moveFlags.HasFlag(CartonUpdateFlags.Pallet) ? carton.PalletId : null)
                .Parameter("location_id", moveFlags.HasFlag(CartonUpdateFlags.Location) ? carton.LocationId : null);
            binder.ParameterXPath("removePallet", moveFlags.HasFlag(CartonUpdateFlags.RemovePallet));
            binder.ParameterXPath("updateLocation", moveFlags.HasFlag(CartonUpdateFlags.Location));

            _db.ExecuteNonQuery(QUERY, binder);
        }


        /// <summary>
        /// This method prints the Carton Ticket
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="printerId"></param>
        public void PrintCartonTicket(string scanText, string printerId)
        {
            const string QUERY = @"            
             DECLARE
                  TYPE CARTON_TABLE_T IS TABLE OF SRC_CARTON.CARTON_ID%TYPE;
                  CARTON_LIST CARTON_TABLE_T;

                BEGIN
                  SELECT CARTON_ID BULK COLLECT
                    INTO CARTON_LIST
                    FROM <proxy />SRC_CARTON S
                   WHERE S.CARTON_ID = :scanText
                      OR S.PALLET_ID = :scanText;
        IF CARTON_LIST.COUNT &gt; 0 THEN
                  FOR I IN 1 .. CARTON_LIST.COUNT LOOP  
                    <proxy />PKG_JF_SRC_2.PKG_JF_SRC_CTN_TKT(ACARTON_ID    =&gt; CARTON_LIST(I),
                                                    APRINTER_NAME =&gt; :aprinter_name);  
                  END LOOP;
                END IF;
                END;";
            var binder = SqlBinder.Create().Parameter("scanText", scanText)
               .Parameter("aprinter_name", printerId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Information of passed carton
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public IEnumerable<Carton> GetCartons(string cartonId, string palletId)
        {
            const string QUERY = @"
            SELECT SC.CARTON_ID           AS CARTON_ID,
                   SC.PALLET_ID           AS PALLET_ID,
                   SC.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                   TIA.SHORT_NAME         AS SHORT_NAME, 
                   SC.VWH_ID              AS VWH_ID,
                   SC.QUALITY_CODE        AS QUALITY_CODE,
                   SC.PRICE_SEASON_CODE   AS PRICE_SEASON_CODE,
                   SC.LOCATION_ID         AS LOCATION_ID,
                   SCD.SKU_ID             AS SKU_ID,
                   SCD.BUNDLE_ID          AS BUNDLE_ID,
                   MSKU.STYLE              AS STYLE_,
                   MSKU.COLOR              AS COLOR_,
                   MSKU.DIMENSION          AS DIMENSION_,
                   MSKU.SKU_SIZE           AS SKU_SIZE_,
                   MSKU.UPC_CODE           AS UPC_CODE_,
                   SCD.QUANTITY           AS QUANTITY,
                   SCD.REQ_PROCESS_ID     AS REQ_PROCESS_ID,                   
                   <proxy/>pkg_carton_work_2.IS_CARTON_MARKED_FOR_WORK(sc.work_needed_xml)  AS REMARK_WORK_NEEDED
              FROM <proxy />SRC_CARTON SC
             LEFT OUTER JOIN <proxy />SRC_CARTON_DETAIL SCD
                ON SCD.CARTON_ID = SC.CARTON_ID
            LEFT OUTER JOIN <proxy />MASTER_SKU MSKU 
                ON SCD.SKU_ID = MSKU.SKU_ID
             LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
                 ON SC.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
             WHERE 1 = 1
            <if>
                AND SC.CARTON_ID = :cartonId
            </if>
            <if>
                AND SC.PALLET_ID = :palletId
            </if>
            ";

            var binder = SqlBinder.Create(row => new Carton()
                {
                    CartonId = row.GetString("CARTON_ID"),
                    PalletId = row.GetString("PALLET_ID"),
                    VwhId = row.GetString("VWH_ID"),
                    QualityCode = row.GetString("QUALITY_CODE"),
                    PriceSeasonCode = row.GetString("PRICE_SEASON_CODE"),
                    LocationId = row.GetString("LOCATION_ID"),
                    Pieces = row.GetInteger("QUANTITY") ?? 0,
                    SkuInCarton = row.GetInteger("SKU_ID") == null ? null : new Sku
                        {
                            SkuId = row.GetInteger("SKU_ID").Value,
                            Style = row.GetString("STYLE_"),
                            Color = row.GetString("COLOR_"),
                            Dimension = row.GetString("DIMENSION_"),
                            SkuSize = row.GetString("SKU_SIZE_"),
                            UpcCode = row.GetString("UPC_CODE_")
                        },
                    CartonArea = new CartonArea
                        {
                            AreaId = row.GetString("CARTON_STORAGE_AREA"),
                            ShortName = row.GetString("SHORT_NAME")
                        },
                    BundleId = row.GetString("BUNDLE_ID"),
                    IsReserved = row.GetInteger("REQ_PROCESS_ID") != null,
                    RemarkWorkNeeded = row.GetInteger("REMARK_WORK_NEEDED").Value == 1
                }).Parameter("cartonId", cartonId)
                .Parameter("palletId", palletId);
            return _db.ExecuteReader(QUERY, binder);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="upcCode"></param>
        /// <returns></returns>
        public Sku GetSku(string upcCode)
        {
            const string QUERY = @"
         SELECT MSKU.SKU_ID    AS SKU_ID,
                MSKU.STYLE     AS STYLE,
                MSKU.COLOR     AS COLOR,
                MSKU.DIMENSION AS DIMENSION,
                MSKU.SKU_SIZE  AS SKU_SIZE
           FROM <proxy />MASTER_SKU MSKU
          WHERE MSKU.UPC_CODE = :upc_code
            ";

            var binder = SqlBinder.Create(row => new Sku()
                {
                    SkuId = row.GetInteger("SKU_ID").Value,
                    Style = row.GetString("STYLE"),
                    Color = row.GetString("COLOR"),
                    Dimension = row.GetString("DIMENSION"),
                    SkuSize = row.GetString("SKU_SIZE")
                }).Parameter("upc_code", upcCode);
            return _db.ExecuteSingle(QUERY, binder);
        }

        public DbTransaction BeginTransaction()
        {
            return _db.BeginTransaction();
        }

        /// <summary>
        /// Information of passed pallet.
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public Pallet GetPallet(string palletId)
        {
            const string QUERY = @"
                            SELECT COUNT(DISTINCT S.CARTON_ID) AS CARTON_COUNT,
                                   MIN(TIA.SHORT_NAME || NVL2(TIA.Warehouse_location_Id,(':' || TIA.Warehouse_location_Id), '')) AS MIN_SHORT_NAME,
                                   MAX(TIA.SHORT_NAME || NVL2(TIA.Warehouse_location_Id,(':' || TIA.Warehouse_location_Id), '')) AS MAX_SHORT_NAME,
                                   COUNT(DISTINCT S.CARTON_STORAGE_AREA) AS CARTON_AREA_COUNT
                              FROM <proxy />SRC_CARTON S
                              LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA ON
                              TIA.INVENTORY_STORAGE_AREA=S.CARTON_STORAGE_AREA
                             WHERE S.PALLET_ID = :PALLET_ID";

            var binder = SqlBinder.Create(row => new Pallet()
                {
                    CartonCount = row.GetInteger("CARTON_COUNT").Value,
                    MinShortName = row.GetString("MIN_SHORT_NAME"),
                    MaxShortName = row.GetString("MAX_SHORT_NAME"),
                    CartonAreaCount = row.GetInteger("CARTON_AREA_COUNT").Value
                }).Parameter("PALLET_ID", palletId);
            var result = _db.ExecuteSingle(QUERY, binder);
            result.PalletId = palletId;
            return result;
        }



        /// <summary>
        /// Remove irregular and sample pieces from carton 
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="bundleId"></param>
        /// <param name="destinationArea"></param>
        /// <param name="pieces"></param>
        /// <param name="reasonCode"></param>
        public void RemoveIrregularSamples(string cartonId, string bundleId, string destinationArea, int? pieces, string reasonCode)
        {
            string QUERY = @" 
            Declare
            LRelated_TRansaction_Id NUMBER(10);
            Begin        
            LRelated_TRansaction_Id:= <proxy />pkg_inv_3.cdscfixirregulars(acarton_id =&gt; :cartonId,
                                         abundle_id =&gt; :bundleId,
                                         apieces =&gt; :trgPieces,
                                         adestination_area =&gt; :destinationArea,
                                         areason_code =&gt; :reasonCode,
                                         arelated_transaction_id =&gt; NULL);
           
           
            End;
            ";
            var binder = SqlBinder.Create().Parameter("cartonId", cartonId)
                .Parameter("bundleId", bundleId)
                .Parameter("trgPieces", pieces)
                .Parameter("destinationArea", destinationArea)
                .Parameter("reasonCode", reasonCode);
            _db.ExecuteNonQuery(QUERY, binder);
        }


        /// <summary>
        /// Get areas to transfer irregular and sample pieces
        /// </summary>
        /// <param name="pieceFlag"></param>
        /// <returns></returns>
        public IEnumerable<SkuArea> GetTransferAreas(PiecesRemoveFlag pieceFlag)
        {
            const string QUERY =
                @"
                  SELECT TIA.INVENTORY_STORAGE_AREA  AS INVENTORY_STORAGE_AREA,
                         TIA.DESCRIPTION             AS DESCRIPTION,                      				        
                         TIA.SHORT_NAME              AS SHORT_NAME
		            FROM <proxy />TAB_INVENTORY_AREA TIA
	               WHERE TIA.STORES_WHAT = 'SKU'
                   <if c='$samples'> 
                         AND TIA.UNUSABLE_INVENTORY='Y'
                    </if>
                    <if c='$irregular'>
                        AND TIA.CONSOLIDATED_UPC_CODE IS NOT NULL  
                    </if>                                          
                ORDER BY TIA.INVENTORY_STORAGE_AREA
        ";
            var binder = SqlBinder.Create(row => new SkuArea()
                {
                    AreaId = row.GetString("INVENTORY_STORAGE_AREA"),
                    Description = row.GetString("DESCRIPTION"),
                    ShortName = row.GetString("SHORT_NAME")
                });
            binder.ParameterXPath("samples", pieceFlag == PiecesRemoveFlag.Samples);
            binder.ParameterXPath("irregular", pieceFlag == PiecesRemoveFlag.Irregular);

            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Get Reason code for removing samples.
        /// </summary>
        /// <returns></returns>
        public string GetSampleReasonCode()
        {
            const string QUERY =
                                @"
                                SELECT S.SPLH_VALUE AS SPLH_VALUE
                                   FROM  <proxy />SPLH S WHERE S.SPLH_ID='$SAMPLES'
                        ";
            var binder = SqlBinder.Create(row => row.GetString("SPLH_VALUE"));
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// Delete empty carton
        /// </summary>
        /// <param name="cartonId"></param>
        public void DeleteEmptyCarton(string cartonId)
        {
            const string QUERY = @"
                                DELETE FROM <proxy /> SRC_CARTON SC
                                WHERE SC.CARTON_ID=:cartonId       
                        ";
            var binder = SqlBinder.Create().Parameter("cartonId", cartonId);
            _db.ExecuteNonQuery(QUERY, binder);
        }
    }
}



//$Id$
