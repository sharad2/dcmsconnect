using System;
using System.Collections.Generic;
using System.Web;
using EclipseLibrary.Oracle;

namespace DcmsMobile.Shipping.Repository.ScanToTruck
{
    //Reviewed By: Ravneet, Binod and Deepak 15 Dec 2012
    public class ScanToTruckRepository : IDisposable
    {
        #region Intialization

        const string MODULE_NAME = "ScanToTruck";

        private readonly OracleDatastore _db;

        public OracleDatastore Db
        {
            get
            {
                return _db;
            }
        }

        public ScanToTruckRepository(OracleDatastore db)
        {
            _db = db;
        }
        public ScanToTruckRepository(TraceContext ctx, string connectString, string userName, string clientInfo)
        {
            var db = new OracleDatastore(ctx);
            db.CreateConnection(connectString, userName);
            db.ModuleName = MODULE_NAME;
            db.ClientInfo = clientInfo;
            db.DefaultMaxRows = 10000;      // Allow retrieving up to 10000 rows. Number of cartons can be huge
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
        /// Retrieves a pallet suggestion for passed appointment.
        /// </summary>
        /// <param name="appointmentNo"></param>
        /// <returns></returns>
        public ICollection<Pallet> GetSuggestedPallet(int appointmentNo)
        {
            //Assuming the appointment will be unique
            const string QUERY = @"               
                DECLARE
                LPALLET <proxy />BOX.PALLET_ID%TYPE;
                Lappointment_id <proxy />Appointment.appointment_id%type;
          
                            BEGIN
                              SELECT A.APPOINTMENT_ID
                                INTO LAPPOINTMENT_ID
                                FROM <proxy />APPOINTMENT A
                                 WHERE A.APPOINTMENT_NUMBER = :APPOINTMENT_NUMBER
                                    AND EXISTS (SELECT 1
                                        FROM <proxy />SHIP T
                                WHERE T.APPOINTMENT_ID = A.APPOINTMENT_ID
                                AND T.TRANSFER_DATE IS NULL);
                              LPALLET  := <proxy />PKG_APPOINTMENT.SUGGEST_PALLET_TO_LOAD_2(AAPPOINTMENT_ID => LAPPOINTMENT_ID);
IF LPALLET IS NOT NULL THEN                           
OPEN :REF_CURSOR FOR
                                    SELECT BOX.PALLET_ID                    AS PALLET_ID,
                                           MAX(BOX.IA_ID)                   AS IA_ID,
                                           MAX(BOX.LOCATION_ID)             AS LOCATION_ID,
                                           COUNT(DISTINCT BOX.UCC128_ID)    AS BOX_COUNT                                           
                                      FROM <proxy />BOX BOX
                                      LEFT OUTER JOIN <proxy />PS PS
                                        ON PS.PICKSLIP_ID = BOX.PICKSLIP_ID
                                      LEFT OUTER JOIN <proxy />SHIP SHIP
                                        ON SHIP.SHIPPING_ID = PS.SHIPPING_ID
                                      LEFT OUTER JOIN <proxy />APPOINTMENT MS
                                        ON MS.APPOINTMENT_ID = SHIP.APPOINTMENT_ID
                                     WHERE MS.APPOINTMENT_NUMBER = :APPOINTMENT_NUMBER
                                           AND BOX.PALLET_ID = LPALLET 
                                       AND PS.TRANSFER_DATE IS NULL
                                     GROUP BY BOX.PALLET_ID
                                     ORDER BY MAX(SHIP.SHIPPING_ID);
END IF;
END;
";
            var binder = SqlBinder.Create(row => new Pallet
            {
                PalletId = row.GetString("PALLET_ID"),
                IaId = row.GetString("IA_ID"),
                LocationId = row.GetString("LOCATION_ID"),
                BoxesCount = row.GetInteger("BOX_COUNT")
            })
                .Parameter("APPOINTMENT_NUMBER", appointmentNo)
                .OutRefCursorParameter("REF_CURSOR");
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Gets boxes of passed pallet.
        /// </summary>
        /// <param name="palletId"> </param>
        public ICollection<Box> GetBoxesOfPallet(string palletId)
        {
            const string QUERY = @"
                                    SELECT BOX.PALLET_ID         AS PALLET_ID,
                                           BOX.UCC128_ID         AS UCC128_ID,
                                           BOX.VERIFY_DATE       AS VERIFY_DATE,
                                           BOX.STOP_PROCESS_DATE AS STOP_PROCESS_DATE,
                                           BOX.TRUCK_LOAD_DATE   AS TRUCK_LOAD_DATE,
                                           MS.APPOINTMENT_NUMBER AS APPOINTMENT_NUMBER
                                      FROM <proxy />BOX BOX
                                      LEFT OUTER JOIN <proxy />PS PS
                                        ON PS.PICKSLIP_ID = BOX.PICKSLIP_ID
                                      LEFT OUTER JOIN <proxy />SHIP SHIP
                                        ON PS.SHIPPING_ID = SHIP.SHIPPING_ID
                                      LEFT OUTER JOIN <proxy />APPOINTMENT MS
                                        ON MS.APPOINTMENT_ID = SHIP.APPOINTMENT_ID
                                     WHERE BOX.PALLET_ID = :PALLET_ID
                                       AND PS.TRANSFER_DATE IS NULL";
            var binder = SqlBinder.Create(row => new Box
                                            {
                                                Ucc128Id = row.GetString("UCC128_ID"),
                                                PalletId = row.GetString("PALLET_ID"),
                                                VerifyDate = row.GetDate("VERIFY_DATE"),
                                                StopProcessDate = row.GetDate("STOP_PROCESS_DATE"),
                                                TruckLoadDate = row.GetDate("TRUCK_LOAD_DATE"),
                                                AppointmentNumber = row.GetInteger("APPOINTMENT_NUMBER")
                                            })
                                            .Parameter("PALLET_ID", palletId);

            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Load the pallet and capture productivity.
        /// </summary>
        /// <param name="palletId"></param>
        public void LoadPallet(string palletId)
        {
            const string QUERY = @"
                begin
                <proxy />pkg_appointment.load_pallet(apallet_id => :PALLET_ID);
                END;";
            var binder = SqlBinder.Create().Parameter("PALLET_ID", palletId);

            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Unload Pallet from truck.
        /// </summary>
        /// <param name="palletId"></param>
        public void UnLoadPallet(string palletId)
        {
            const string QUERY = @"
                           begin
                           <proxy />pkg_appointment.unload_pallet(apallet_id => :PALLET_ID);
                           end;
                            ";
            var binder = SqlBinder.Create().Parameter("PALLET_ID", palletId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        ///  Gets the 
        /// </summary>
        /// <param name="appointmentNo"></param>
        /// <returns></returns>
        public Appointment GetAppointmentDetails(int appointmentNo)
        {
            const string QUERY = @"
                                    SELECT COUNT(DISTINCT(CASE
                                                    WHEN BOX.TRUCK_LOAD_DATE IS NOT NULL THEN
                                                     BOX.PALLET_ID END))                        AS LOADED_PALLET_COUNT,
                                   COUNT(DISTINCT BOX.PALLET_ID)                                AS TOTAL_PALLET_COUNT,
                                   COUNT(DISTINCT(CASE
                                                    WHEN BOX.SUSPENSE_DATE IS NOT NULL THEN
                                                     BOX.PALLET_ID END))                        AS SUSPENCE_PALLET_COUNT,
                                   COUNT(DISTINCT(CASE
                                                    WHEN BOX.PALLET_ID IS NULL THEN
                                                     BOX.UCC128_ID END))                        AS UNPALLETIZE_BOX_COUNT,
                                   COUNT(DISTINCT(CASE
                                                    WHEN BOX.TRUCK_LOAD_DATE IS NOT NULL THEN
                                                     BOX.UCC128_ID END))                        AS LOADED_BOX_COUNT,
                                   COUNT(DISTINCT BOX.UCC128_ID)                                AS TOTAL_BOX_COUNT,
                                    MAX(MS.BUILDING_ID)                                         AS BUILDING_ID,
                                    MAX(MS.PICKUP_DOOR)                                         AS PICKUP_DOOR,
                                    MAX(MS.CARRIER_ID)                                          AS CARRIER_ID
                              FROM <proxy />BOX BOX
                              LEFT OUTER JOIN <proxy />PS PS
                                ON PS.PICKSLIP_ID = BOX.PICKSLIP_ID
                              LEFT OUTER JOIN <proxy />SHIP SHIP
                                ON SHIP.SHIPPING_ID = PS.SHIPPING_ID
                              LEFT OUTER JOIN <proxy />APPOINTMENT MS
                                ON MS.APPOINTMENT_ID = SHIP.APPOINTMENT_ID
                             WHERE MS.APPOINTMENT_NUMBER = :APPOINTMENT_NUMBER
                               AND PS.TRANSFER_DATE IS NULL
                               AND BOX.STOP_PROCESS_DATE IS NULL
                               GROUP BY MS.APPOINTMENT_ID";
            var binder = SqlBinder.Create(row => new Appointment
            {
                LoadedBoxCount = row.GetInteger("LOADED_BOX_COUNT") ?? 0,
                LoadedPalletCount = row.GetInteger("LOADED_PALLET_COUNT") ?? 0,
                PalletsInSuspenseCount = row.GetInteger("SUSPENCE_PALLET_COUNT") ?? 0,
                TotalPalletCount = row.GetInteger("TOTAL_PALLET_COUNT") ?? 0,
                TotalBoxCount = row.GetInteger("TOTAL_BOX_COUNT") ?? 0,
                UnPalletizeBoxCount = row.GetInteger("UNPALLETIZE_BOX_COUNT") ?? 0,
                BuildingId = row.GetString("BUILDING_ID"),
                DoorId = row.GetString("PICKUP_DOOR"),
                CarrierId = row.GetString("CARRIER_ID")
            })
               .Parameter("APPOINTMENT_NUMBER", appointmentNo);
            return _db.ExecuteSingle(QUERY, binder);

        }

    }
}