using DcmsMobile.CartonManager.Models;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web;

namespace DcmsMobile.CartonManager.Repository.Locating
{
    public class LocatingRepository : IDisposable
    {
        private readonly OracleDatastore _db;

        public LocatingRepository(string userName, string moduleName, string clientInfo, TraceContext trace)
        {
            Contract.Assert(ConfigurationManager.ConnectionStrings["dcms4"] != null);
            var store = new OracleDatastore(trace);
            store.CreateConnection(ConfigurationManager.ConnectionStrings["dcms4"].ConnectionString,
                                   userName);
            store.ModuleName = moduleName;
            store.ClientInfo = clientInfo;
            _db = store;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        /// <summary>
        /// Returns all information of the passed pallet or cartons.
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="cartonList"></param>
        /// <returns></returns>
        internal IEnumerable<Carton> GetCartonsOfPallet(string palletId, IList<string> cartonList)
        {
            if(string.IsNullOrWhiteSpace(palletId) && cartonList == null)
            {
                throw new ArgumentNullException("palletId / cartonList");
            }
            const string QUERY = @"
                            SELECT S.CARTON_ID          AS CARTON_ID,
                                   S.WORK_NEEDED_XML    AS WORK_NEEDED_XML
                              FROM <proxy />SRC_CARTON S
                             WHERE 1= 1
                             <if> AND S.PALLET_ID = :PALLET_ID </if>
                            <else><a pre='AND S.CARTON_ID IN (' sep=',' post=')'>:CARTON_ID_LIST</a></else>
                ";
            var binder = SqlBinder.Create(row => new Carton()
            {
                CartonId = row.GetString("CARTON_ID"),
                RemarkWorkNeeded = string.IsNullOrWhiteSpace(row.GetString("WORK_NEEDED_XML")) ? false : true
            }).Parameter("PALLET_ID", palletId);
            binder.ParameterXmlArray("CARTON_ID_LIST", cartonList);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Retrive info for passed location.
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public Location GetLocation(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new ArgumentNullException("locationId");
            }
            const string QUERY = @"
                    Select msl.storage_area AS storage_area,
                           tia.short_name   AS SHORT_NAME,
                           msl.travel_sequence AS travel_sequence,
                        msl.assigned_max_cartons AS assigned_max_cartons,
                           NVL(TIA.WAREHOUSE_LOCATION_ID, MSL.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
                           TIA.STORES_WHAT AS STORES_WHAT,
                           MSL.unavailable_flag AS unavailable_flag,
                           (select count(*)
                              from  <proxy />src_carton ctn
                             where ctn.location_id = :location_id) AS count_cartons_at_location
                      from  <proxy />master_storage_location msl
                     INNER JOIN  <proxy />TAB_INVENTORY_AREA TIA
                        ON TIA.INVENTORY_STORAGE_AREA = MSL.STORAGE_AREA
                     where msl.location_id = :location_id
";
            var binder = SqlBinder.Create(row => new Location
            {
                AreaId = row.GetString("storage_area"),
                AreaShortName = row.GetString("SHORT_NAME"),
                TravelSequence = row.GetInteger("travel_sequence"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                StoresWhat = row.GetString("STORES_WHAT"),
                LocationId = locationId,
                UnavailableFlag = row.GetString("unavailable_flag") == "Y",
                CountCartons = row.GetInteger("count_cartons_at_location"),
                MaxCartons = row.GetInteger("assigned_max_cartons")
            }).Parameter("location_id", locationId);
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// Raises exception if carton is not located. 
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="destAreaId"> </param>
        /// <param name="destLocationId"></param>
        /// <param name="locationTravelSequence"> </param>
        /// <param name="destPalletId"></param>
        /// <param name="destBuildingId"> </param>
        /// <returns>Item 1: Previous location of the carton. Item 2: true if the carton is invalid</returns>
        /// <remarks>
        /// Exception raised for Forinvalid carton 20002.
        /// Location is considered invalid if it does not exist in database or unavailable flag is set. 
        /// </remarks>
        public Tuple<string, bool> LocateCarton(string cartonId, string destBuildingId, string destAreaId, string destLocationId, int? locationTravelSequence, string destPalletId)
        {
            const string QUERY = @"
              DECLARE
              LCARTON_STORAGE_AREA SRC_CARTON.CARTON_STORAGE_AREA%TYPE;
              LVWH_ID SRC_CARTON.VWH_ID%TYPE;
              LUPC_CODE MASTER_SKU.UPC_CODE%TYPE;
              LQUANTITY SRC_CARTON_DETAIL.QUANTITY%TYPE;
              LRow_id ROWID;
              BEGIN
        BEGIN
            SELECT SC.CARTON_STORAGE_AREA,
                   SC.VWH_ID,
                   SC.LOCATION_ID,
                   M.UPC_CODE,
                   SCD.QUANTITY,
        sc.rowid
              INTO LCARTON_STORAGE_AREA, LVWH_ID, :LCARTON_LOCATION, LUPC_CODE, LQUANTITY, LRow_id
              FROM <proxy />SRC_CARTON SC
              INNER JOIN <proxy />SRC_CARTON_DETAIL SCD
                ON SCD.CARTON_ID = SC.CARTON_ID
              INNER JOIN <proxy />MASTER_SKU M
                ON M.SKU_ID = SCD.SKU_ID
             WHERE SC.CARTON_ID = :CARTON_ID AND ROWNUM &lt; 2
               FOR UPDATE OF sc.SUSPENSE_DATE NOWAIT;
         EXCEPTION
           WHEN NO_DATA_FOUND THEN
              --RAISE_APPLICATION_ERROR(-20200, 'Invalid carton');
              :INVALID_CARTON := 'Y';
              RETURN;
        END;
                             UPDATE <proxy />SRC_CARTON SC 
                     SET SC.SUSPENSE_DATE = NULL, 
                     SC.CARTON_STORAGE_AREA = :CARTON_STORAGE_AREA, 
                     sc.pallet_id =null,
                     SC.location_id = :LOCATION_ID 
                     WHERE SC.rowid = LRow_id;
                    IF SQL%ROWCOUNT != 1 THEN
                      RAISE_APPLICATION_ERROR(-20100, 'Internal error. Carton just selected could not be updated.');
                    END IF;

                    INSERT INTO <proxy />CARTON_PRODUCTIVITY
                    (
                        MODULE_CODE,
                        ACTION_CODE,
                        PROCESS_START_DATE,
                        PROCESS_END_DATE,
                        CARTON_ID,
                        PALLET_ID,
                        UPC_CODE,
                        CARTON_QUANTITY,
                        CARTON_SOURCE_AREA,
                        CARTON_DESTINATION_AREA,
                        AISLE,
                        WAREHOUSE_LOCATION_ID,
                        VWH_ID)   
                    values(
                        'LOC',
                        'LOC',
                        SYSDATE,
                        SYSDATE,
                        :CARTON_ID,
                        :PALLET_ID,
                        LUPC_CODE,
                        LQUANTITY,
                        LCARTON_STORAGE_AREA,
                        :CARTON_STORAGE_AREA,
                        :travel_sequence,
                        :WAREHOUSE_LOCATION_ID,
                        LVWH_ID );

END;

";

            string cartonLocation = string.Empty;
            bool bInvalidCarton = true;
            var binder = SqlBinder.Create().Parameter("CARTON_ID", cartonId)
                .Parameter("LOCATION_ID", destLocationId)
                .Parameter("PALLET_ID", destPalletId)
                .Parameter("CARTON_STORAGE_AREA", destAreaId)
                .Parameter("travel_sequence", locationTravelSequence)
                .Parameter("WAREHOUSE_LOCATION_ID", destBuildingId)
                .OutParameter("LCARTON_LOCATION", p => cartonLocation = p)
                .OutParameter("INVALID_CARTON", p => bInvalidCarton = p == "Y")
                ;
            _db.ExecuteNonQuery(QUERY, binder);
            return Tuple.Create(cartonLocation, bInvalidCarton);

        }

        /// <summary>
        /// Carton put in suspence
        /// </summary>
        /// <param name="palletId"></param>
        public void MarkCartonInSuspense(string palletId)
        {
            const string QUERY = @"
                          UPDATE <proxy />SRC_CARTON SC
                                SET SC.SUSPENSE_DATE = SYSDATE
                            WHERE SC.PALLET_ID = :PALLET_ID";
            var binder = SqlBinder.Create().Parameter("PALLET_ID", palletId);
            _db.ExecuteDml(QUERY, binder);
        }
    }
}