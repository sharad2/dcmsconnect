using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ShipmentEntity
{
    internal class ShipmentEntityRepository : IDisposable
    {
        private readonly OracleDatastore _db;
        public ShipmentEntityRepository(string userName, string clientInfo)
        {
            _db = new OracleDatastore(HttpContext.Current.Trace);
            _db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString, userName);
            _db.ModuleName = "Inquiry_ShipmentEntity";
            _db.ClientInfo = clientInfo;
        }

        public void Dispose()
        {
            _db.Dispose();
        }



        /// <summary>
        /// This function will provide information about intransit cartons. 
        ///  <param name="shippingId"></param>
        /// </summary>
        public ParentShipment GetOutboundShipment(string shippingId, string mBolId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
            SELECT MAX(S.SHIP_DATE)                                AS SHIP_DATE,    
               MAX(MC.CARRIER_ID)                                   AS CARRIER_ID,
               MAX(MC.DESCRIPTION)                                   AS CARRIER_NAME,
               MAX(S.SHIPPING_TYPE)                            AS SHIPPING_TYPE,
               MAX(S.ARRIVAL_DATE)                             AS ARRIVAL_DATE,
               MAX(C.CUSTOMER_ID)                              AS CUSTOMER_ID,
               MAX(C.NAME)                                     AS CUSTOMER_NAME,
               MAX(S.CUSTOMER_DC_ID)                           AS CUSTOMER_DC_ID,
               MAX(S.MBOL_ID)                                  AS MBOL_ID,
               MAX(S.ONHOLD_FLAG)                              AS ONHOLD_FLAG, 
               S.PARENT_SHIPPING_ID                            AS PARENT_SHIPPING_ID,
               MAX(S.SHIPPING_ADDRESS.ADDRESS_LINE_1)          AS ADDRESS_LINE_1,
               MAX(S.SHIPPING_ADDRESS.ADDRESS_LINE_2)          AS ADDRESS_LINE_2,
               MAX(S.SHIPPING_ADDRESS.ADDRESS_LINE_3)          AS ADDRESS_LINE_3,
               MAX(S.SHIPPING_ADDRESS.ADDRESS_LINE_4)          AS ADDRESS_LINE_4,
               MAX(S.SHIPPING_ADDRESS.CITY)                    AS CITY,
               MAX(S.SHIPPING_ADDRESS.STATE)                   AS STATE,
               MAX(S.SHIPPING_ADDRESS.COUNTRY_CODE)            AS COUNTRY_CODE,
               MAX(S.PREPAID_CODE)                             AS PREPAID_CODE,
               MAX(S.SHIPPING_ADDRESS.ZIP_CODE)                AS ZIP_CODE,
               MAX(WHL.ADDRESS_1)                              AS SHIP_FROM_ADDRESS_LINE_1,
               MAX(WHL.ADDRESS_2)                              AS SHIP_FROM_ADDRESS_LINE_2,
               MAX(WHL.ADDRESS_3)                              AS SHIP_FROM_ADDRESS_LINE_3,
               MAX(WHL.ADDRESS_4)                              AS SHIP_FROM_ADDRESS_LINE_4,
               MAX(WHL.CITY)                                   AS SHIP_FROM_CITY,
               MAX(WHL.STATE)                                  AS SHIP_FROM_STATE,
               MAX(WHL.ZIP_CODE)                               AS SHIP_FROM_ZIP_CODE,
               MAX(WHL.COUNTRY_CODE)                           AS SHIP_FROM_COUNTRY_CODE,
               MAX(WHL.COMPANY_NAME)                           AS SHIP_FROM_COMPANY_NAME,
              MAX(A.APPOINTMENT_NUMBER)                        AS APPOINTMENT_NUMBER,
              MAX(S.UPLOAD_DATE)                               AS UPLOAD_DATE
          FROM <proxy />SHIP S
          LEFT OUTER JOIN <proxy />CUST C
            ON S.CUSTOMER_ID = C.CUSTOMER_ID
          LEFT OUTER JOIN <proxy />MASTER_CARRIER MC
            ON S.CARRIER_ID = MC.CARRIER_ID
         LEFT OUTER JOIN <proxy />IA I
            ON S.DOOR_IA_ID = I.IA_ID
          LEFT OUTER JOIN <proxy />Tab_Warehouse_location WHL
            ON I.WAREHOUSE_LOCATION_ID = WHL.WAREHOUSE_LOCATION_ID
          LEFT OUTER JOIN <proxy />APPOINTMENT A
            ON S.APPOINTMENT_ID = A.APPOINTMENT_ID
         WHERE 1 = 1
    <if>
         AND S.SHIPPING_ID = :SHIPPING_ID
         OR S.PARENT_SHIPPING_ID = :SHIPPING_ID
    </if>
    <if>
         AND S.MBOL_ID = :MBOL_ID
    </if>
         GROUP BY S.PARENT_SHIPPING_ID            
            ";

            var binder = SqlBinder.Create(row => new ParentShipment
            {
                ToAddress = new[] {
                         row.GetString("ADDRESS_LINE_1"),
                         row.GetString("ADDRESS_LINE_2"),
                         row.GetString("ADDRESS_LINE_3"),
                         row.GetString("ADDRESS_LINE_4")
                     },
                FromAddress = new[] {
                         row.GetString("SHIP_FROM_ADDRESS_LINE_1"),
                         row.GetString("SHIP_FROM_ADDRESS_LINE_2"),
                         row.GetString("SHIP_FROM_ADDRESS_LINE_3"),
                         row.GetString("SHIP_FROM_ADDRESS_LINE_4")
                     },

                ZipCode = row.GetString("ZIP_CODE"),
                City = row.GetString("CITY"),
                State = row.GetString("STATE"),
                Country = row.GetString("COUNTRY_CODE"),
                ShippingDate = row.GetDate("SHIP_DATE"),
                //Carrier = row.GetString("CARRIER"),
                CarrierId = row.GetString("CARRIER_ID"),
                CarrierName = row.GetString("CARRIER_NAME"),
                ShippingType = row.GetString("SHIPPING_TYPE"),
                ArrivalDate = row.GetDate("ARRIVAL_DATE"),
                CustomerID = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME"),
                CustomerDcId = row.GetString("CUSTOMER_DC_ID"),
                MBolID = row.GetString("MBOL_ID"),
                ParentShippingId = row.GetString("PARENT_SHIPPING_ID"),
                FreightChargeTerm = row.GetString("PREPAID_CODE"),
                FromZipCode = row.GetString("SHIP_FROM_ZIP_CODE"),
                FromCity = row.GetString("SHIP_FROM_CITY"),
                FromState = row.GetString("SHIP_FROM_STATE"),
                FromCountry = row.GetString("SHIP_FROM_COUNTRY_CODE"),
                FromCompany = row.GetString("SHIP_FROM_COMPANY_NAME"),
                OnHoldFlag = !string.IsNullOrEmpty(row.GetString("ONHOLD_FLAG")),
                IsTransferred = !string.IsNullOrEmpty(row.GetDate("UPLOAD_DATE").ToString()),
                AppointmentNumber = row.GetInteger("APPOINTMENT_NUMBER")
            }).Parameter("SHIPPING_ID", shippingId).Parameter("MBOL_ID", mBolId);


            return _db.ExecuteSingle(QUERY, binder);


        }


        /// <summary>
        /// Contains SKU related functions
        /// </summary>

        public IList<Tuple<string, string>> GetBolPrinters()
        {
            Contract.Assert(_db != null);
            const string QUERY = @"    
    SELECT NAME AS NAME, DESCRIPTION AS DESCRIPTION
      FROM <proxy />TAB_PRINTER
     WHERE UPPER(PRINTER_TYPE) IN
           (SELECT UPPER(PRINTER_TYPE)
              FROM <proxy />DOC
             WHERE DOCUMENT_ID IN ('$BOL', '$MBOL'))
     ORDER BY NAME ASC
              ";

            var binder = SqlBinder.Create(row => Tuple.Create(row.GetString("NAME"), row.GetString("DESCRIPTION")));
            return _db.ExecuteReader(QUERY, binder);
        }


        public IList<MasterBolShipment> GetMasterBolShipments(string mBolId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                SELECT S.SHIPPING_ID AS SHIPPING_ID, 
                       S.ONHOLD_FLAG AS ONHOLD_FLAG,
                       S.SHIP_DATE   AS SHIP_DATE,
                       S.ARRIVAL_DATE AS ARRIVAL_DATE
                  FROM <proxy />SHIP S
                 WHERE S.MBOL_ID = :MBOL_ID
            ";

            var binder = SqlBinder.Create(row => new MasterBolShipment
                {
                    ShippingId = row.GetString("SHIPPING_ID"),
                    OnHold = !string.IsNullOrWhiteSpace(row.GetString("ONHOLD_FLAG")),
                    ShippingDate = row.GetDate("SHIP_DATE"),
                    ArrivalDate = row.GetDate("ARRIVAL_DATE")
                }).Parameter("MBOL_ID", mBolId);

            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// This function will provides detail information about outbound shipment. 
        ///  <param name="parentShippingId"></param>
        /// </summary>
        public IList<ParentShipmentContent> GetDetailsOfOutboundShipment(string parentShippingId, string mBolId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"WITH WEIGHT AS
                                     (SELECT ST.SHIPPING_ID AS SHIPPING_ID, SUM(SC.EMPTY_WT) AS WEIGHT
                                        FROM SHIP ST
                                       INNER JOIN PS P
                                          ON ST.SHIPPING_ID = P.SHIPPING_ID
                                        LEFT OUTER JOIN BOX B
                                          ON P.PICKSLIP_ID = B.PICKSLIP_ID
                                        LEFT OUTER JOIN SKUCASE SC
                                          ON B.CASE_ID = SC.CASE_ID
                                       WHERE 1 = 1 
                                        <if>
                                        AND ST.PARENT_SHIPPING_ID = :PARENT_SHIPPING_ID
                                        </if>
                                        <if>
                                        AND ST.MBOL_ID = :MBOL_ID
                                        </if>
                                       GROUP BY ST.SHIPPING_ID)

                                    SELECT S.SHIPPING_ID AS SHIPPING_ID,
                                           COUNT(DISTINCT BU.BUCKET_ID) AS TOTAL_BUCKET,
                                           COUNT(DISTINCT CASE
                                                   WHEN BU.FREEZE IS NOT NULL THEN
                                                    BU.BUCKET_ID
                                                 END) AS FROZEN_BUCKETS,
                                           MIN(CASE
                                                 WHEN BU.FREEZE IS NOT NULL THEN
                                                  BU.BUCKET_ID
                                               END) AS MIN_FROZEN_BUCKET,
                                           MAX(CASE
                                                 WHEN BU.FREEZE IS NOT NULL THEN
                                                  BU.BUCKET_ID
                                               END) AS MAX_FROZEN_BUCKET,
                                           COUNT(DISTINCT P.PICKSLIP_ID) AS TOTAL_PICKSLIPS,
                                           COUNT(DISTINCT CASE
                                                   WHEN P.PICKING_STATUS NOT IN ('COMPLETED', 'TRANSFERED', 'SHIPPED') THEN
                                                    P.PICKSLIP_ID
                                                 END) AS INCOMPLETE_PICKSLIPS,
                                           MIN(CASE
                                                 WHEN P.PICKING_STATUS NOT IN ('COMPLETED', 'TRANSFERED', 'SHIPPED') THEN
                                                  P.PICKSLIP_ID
                                               END) AS MIN_PROBLEMATIC_PICKSLIP,
                                           MAX(CASE
                                                 WHEN P.PICKING_STATUS NOT IN ('COMPLETED', 'TRANSFERED', 'SHIPPED') THEN
                                                  P.PICKSLIP_ID
                                               END) AS MAX_PROBLEMATIC_PICKSLIP,
                                           COUNT(DISTINCT CASE
                                                   WHEN B.STOP_PROCESS_REASON != '$BOXCANCEL' AND B.SUSPENSE_DATE IS NULL AND
                                                      B.REJECTION_CODE IS NULL THEN
                                                    B.UCC128_ID
                                                 END) AS TOTAL_BOXES,
                                         --  COUNT(DISTINCT CASE
                                         --          WHEN B.STOP_PROCESS_REASON = '$XREF' THEN
                                         --           B.UCC128_ID
                                         --        END) AS TRANSFERED_TOTAL_BOXES,
                                           COUNT(DISTINCT CASE
                                                   WHEN B.VERIFY_DATE IS NOT NULL AND
                                                        B.IA_ID IN
                                                        (SELECT IA_ID FROM IACONFIG WHERE IACONFIG_ID = '$DOORAREA') AND
                                                        B.SUSPENSE_DATE IS NULL AND B.REJECTION_CODE IS NULL AND
                                                        B.STOP_PROCESS_DATE IS NULL THEN
                                                    B.UCC128_ID
                                                 END) AS SHIPAABLE_BOXES,
                                           MIN(CASE
                                                 WHEN B.VERIFY_DATE IS NULL OR
                                                      B.IA_ID NOT IN
                                                      (SELECT IA_ID FROM IACONFIG WHERE IACONFIG_ID = '$DOORAREA') OR
                                                      B.SUSPENSE_DATE IS NOT NULL OR B.REJECTION_CODE IS NOT NULL AND
                                                      B.STOP_PROCESS_DATE IS NOT NULL THEN
                                                  B.UCC128_ID
                                               END) AS MIN_NON_SHIPABLE_BOX,
                                           MAX(CASE
                                                 WHEN B.VERIFY_DATE IS NULL OR
                                                      B.IA_ID NOT IN
                                                      (SELECT IA_ID FROM IACONFIG WHERE IACONFIG_ID = '$DOORAREA') OR
                                                      B.SUSPENSE_DATE IS NOT NULL OR B.REJECTION_CODE IS NOT NULL AND
                                                      B.STOP_PROCESS_DATE IS NOT NULL THEN
                                                  B.UCC128_ID
                                               END) AS MAX_NON_SHIPABLE_BOX,
                                           SUM(CASE
                                                 WHEN B.VERIFY_DATE IS NOT NULL AND B.SUSPENSE_DATE IS NULL AND
                                                      B.REJECTION_CODE IS NULL AND B.STOP_PROCESS_REASON != '$BOXCANCEL' AND
                                                      B.IA_ID IN
                                                      (SELECT IA_ID FROM IA WHERE SHIPPING_AREA_FLAG = 'Y') THEN
                                                  BD.CURRENT_PIECES
                                               END) AS CURRENT_PIECES,
                                       --     SUM(CASE
                                       --     WHEN B.STOP_PROCESS_REASON = '$XREF' THEN
                                       --        BD.CURRENT_PIECES
                                       --        END) AS TRANSFERED_CURRENT_PIECES,
                                           SUM(BD.EXPECTED_PIECES) AS EXPECTED_PIECES,
                                           ROUND(SUM(BD.CURRENT_PIECES * (MSKU.WEIGHT_PER_DOZEN / 12)) +
                                                 MAX(WEIGHT.WEIGHT),
                                                 2) AS WEIGHT
                                      FROM <proxy />SHIP S
                                     INNER JOIN <proxy />PS P
                                        ON S.SHIPPING_ID = P.SHIPPING_ID
                                     INNER JOIN WEIGHT WEIGHT
                                        ON S.SHIPPING_ID = WEIGHT.SHIPPING_ID
                                      LEFT OUTER JOIN <proxy />BOX B
                                        ON P.PICKSLIP_ID = B.PICKSLIP_ID
                                      LEFT OUTER JOIN <proxy />BUCKET BU
                                        ON P.BUCKET_ID = BU.BUCKET_ID
                                      LEFT OUTER JOIN <proxy />BOXDET BD
                                        ON B.UCC128_ID = BD.UCC128_ID
                                       AND B.PICKSLIP_ID = BD.PICKSLIP_ID
                                      LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
                                        ON MSKU.SKU_ID = BD.SKU_ID
                                     WHERE 1 = 1 
                                    <if>
                                    AND S.PARENT_SHIPPING_ID = :PARENT_SHIPPING_ID
                                    </if>
                                    <if>
                                    AND S.MBOL_ID = :MBOL_ID
                                    </if>
                                     GROUP BY S.PARENT_SHIPPING_ID, S.SHIPPING_ID

                     ";
            var binder = SqlBinder.Create(row => new ParentShipmentContent
            {
                ShippingId = row.GetString("SHIPPING_ID"),
                TotalBoxes = row.GetInteger("TOTAL_BOXES") ?? 0,
                TotalPickslips = row.GetInteger("TOTAL_PICKSLIPS") ?? 0,
                TotalBuckets = row.GetInteger("TOTAL_BUCKET"),
                CountFrozenBuckets = row.GetInteger("FROZEN_BUCKETS") ?? 0,
                IncompletePickslips = row.GetInteger("INCOMPLETE_PICKSLIPS"),
                CurrentPieces = row.GetInteger("CURRENT_PIECES") ?? 0,
                ExpectedPieces = row.GetInteger("EXPECTED_PIECES") ?? 0,
                MinBucketSuggestion = row.GetInteger("MIN_FROZEN_BUCKET"),
                MaxBucketSuggestion = row.GetInteger("MAX_FROZEN_BUCKET"),
                MaxBoxSuggestion = row.GetString("MAX_NON_SHIPABLE_BOX"),
                MinBoxSuggestion = row.GetString("MIN_NON_SHIPABLE_BOX"),
                MaxPickslipSuggestion = row.GetInteger("MAX_PROBLEMATIC_PICKSLIP"),
                MinPickslipSuggestion = row.GetInteger("MIN_PROBLEMATIC_PICKSLIP"),
                ShippableBoxes = row.GetInteger("SHIPAABLE_BOXES"),
                // TotalTransferedBoxes = row.GetInteger("TRANSFERED_TOTAL_BOXES"),
                //  TransferedCurrentPieces = row.GetInteger("TRANSFERED_CURRENT_PIECES") ?? 0,
                Weight = row.GetDecimal("WEIGHT")
            }).Parameter("PARENT_SHIPPING_ID", parentShippingId).Parameter("MBOL_ID", mBolId);

            return _db.ExecuteReader(QUERY, binder);


        }


        public void PrintBol(string parentShippingId, string printerid, int numberOfCopies)
        {
            const string QUERY = @"
                DECLARE
                  Lresult number; 
                BEGIN
                  Lresult := <proxy />pkg_print_bol.write_bol_to_file(aparent_shipping_id => :aparent_shipping_id,
                                                             aprinter_name => :aprinter_name,
                                                             ano_of_copies => :ano_of_copies);

                END;
            ";

            var binder = SqlBinder.Create().Parameter("aparent_shipping_id", parentShippingId)
                .Parameter("aprinter_name", printerid)
                .Parameter("ano_of_copies", numberOfCopies);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        public void PrintMasterBol(string[] address, string city, string state, string zipcode, string country, string mBolId, string printerId)
        {
            const string QUERY = @"
                    declare
                      -- Non-scalar parameters require additional processing 
                      aship_address dcms8sys.address_t;
                      result number;  
                    begin
                      aship_address := dcms8sys.address_t(ADDRESS_LINE_1 => :ADDRESS_LINE_1,
                                                          ADDRESS_LINE_2 => :ADDRESS_LINE_2,
                                                          ADDRESS_LINE_3 => :ADDRESS_LINE_3,
                                                          ADDRESS_LINE_4 => :ADDRESS_LINE_4,
                                                          CITY           => :CITY,
                                                          STATE          => :STATE,
                                                          ZIP_CODE       => :ZIP_CODE,
                                                          COUNTRY_CODE   => :COUNTRY_CODE,
                                                          ADDRESS_TYPE => null);
                      -- Call the function
                      result := pkg_print_bol.write_mbol_to_file(ambol_id      => :ambol_id,
                                                                  aship_address => aship_address,
                                                                  aprinter_name => :aprinter_name);
                    end;
            ";
            var binder = SqlBinder.Create().Parameter("ADDRESS_LINE_1", address[0])
                .Parameter("ADDRESS_LINE_2", address[1])
                .Parameter("ADDRESS_LINE_3", address[2])
                .Parameter("ADDRESS_LINE_4", address[3])
                .Parameter("CITY", city)
                .Parameter("STATE", state)
                .Parameter("ZIP_CODE", zipcode)
                .Parameter("COUNTRY_CODE", country)
                .Parameter("ambol_id", mBolId)
                .Parameter("aprinter_name", printerId);
            _db.ExecuteNonQuery(QUERY, binder);
        }


        /// <summary>
        /// This function returns Appointment information of passed appointmentNumber and appointment Id.
        /// </summary>
        /// <param name="appointmentNumber"></param>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        public Appointment GetAppointmentDetails(int appointmentId)
        {

            const string QUERY = @"            
               SELECT AP.APPOINTMENT_ID                                 AS APPOINTMENT_ID,
                      AP.APPOINTMENT_NUMBER                             AS APPOINTMENT_NUMBER,
                      MAX(MC.CARRIER_ID)                                AS CARRIER_ID,
                      MAX(MC.DESCRIPTION)                               AS CARRIER_DESCRIPTION_,
                      MAX(AP.APPOINTMENT_DATE)                          AS APPOINTMENT_DATE,
                      MAX(AP.TRUCK_ARRIVAL_DELAY)                       AS ARRIVAL_TIME,
                      SYS.STRAGG(DISTINCT CASE
                                   WHEN CUST.CUSTOMER_ID IS NOT NULL THEN
                                    CUST.CUSTOMER_ID || ' [ ' || CUST.NAME || ' ],'
                                 END)                                   AS CUSTOMER_LIST,
                      SYS.STRAGG(DISTINCT CASE
                                   WHEN B.IA_ID IS NOT NULL THEN
                                    B.IA_ID || ','
                                 END)                                   AS AREA_LIST,
                      COUNT(DISTINCT B.PALLET_ID)                       AS TOTAL_PALLETS,
                      COUNT(DISTINCT CASE
                              WHEN B.TRUCK_LOAD_DATE IS NOT NULL THEN
                               B.PALLET_ID
                            END)                                        AS LOADED_PALLETS,
                      COUNT(DISTINCT B.UCC128_ID)                       AS TOTAL_BOXES,
                      COUNT(DISTINCT CASE
                              WHEN B.TRUCK_LOAD_DATE IS NOT NULL THEN
                               B.UCC128_ID
                            END)                                        AS LOADED_BOXES,
                      COUNT(DISTINCT SHIP.SHIPPING_ID)                  AS BOL_COUNT,
                      COUNT(DISTINCT CASE
                              WHEN B.SUSPENSE_DATE IS NOT NULL THEN
                               B.PALLET_ID
                            END)                                        AS SUSPENSE_PALLETS
                 FROM <proxy />APPOINTMENT AP
                 LEFT OUTER JOIN <proxy />SHIP SHIP
                   ON AP.APPOINTMENT_ID = SHIP.APPOINTMENT_ID
                 LEFT OUTER JOIN <proxy />PS PS
                   ON SHIP.SHIPPING_ID = PS.SHIPPING_ID
                 LEFT OUTER JOIN <proxy />BOX B
                   ON PS.PICKSLIP_ID = B.PICKSLIP_ID
                 LEFT OUTER JOIN <proxy />BOXDET BD
                   ON B.UCC128_ID = BD.UCC128_ID
                  AND B.PICKSLIP_ID = BD.PICKSLIP_ID
                 LEFT OUTER JOIN <proxy />CUST CUST
                   ON PS.CUSTOMER_ID = CUST.CUSTOMER_ID
                 LEFT OUTER JOIN <proxy />MASTER_CARRIER MC
                   ON AP.CARRIER_ID = MC.CARRIER_ID
                WHERE AP.APPOINTMENT_ID = :APPOINTMENT_ID
                GROUP BY AP.APPOINTMENT_ID, AP.APPOINTMENT_NUMBER
            ";
            var binder = SqlBinder.Create(row => new Appointment
            {
                AppointmentId = row.GetInteger("APPOINTMENT_ID").Value,
                AppointmentNumber = row.GetInteger("APPOINTMENT_NUMBER").Value,
                Carrier = !string.IsNullOrEmpty(row.GetString("CARRIER_ID")) ? row.GetString("CARRIER_ID") + ": [ " + row.GetString("CARRIER_DESCRIPTION_") + " ]" : null,
                AppointmentArrivalDate = row.GetInterval("ARRIVAL_TIME"),
                AppointmentDate = row.GetDate("APPOINTMENT_DATE").Value,
                CustomerList = row.GetString("CUSTOMER_LIST").Split(',').Where(p => !string.IsNullOrEmpty(p)),
                BoxesInAreas = row.GetString("AREA_LIST").Split(',').Where(p => !string.IsNullOrEmpty(p)),
                TotalPallets = row.GetInteger("TOTAL_PALLETS"),
                LoadedPallets = row.GetInteger("LOADED_PALLETS"),
                TotalBoxes = row.GetInteger("TOTAL_BOXES"),
                LoadedBoxes = row.GetInteger("LOADED_BOXES"),
                ShipmentCount = row.GetInteger("BOL_COUNT"),
                SuspensePallets = row.GetInteger("SUSPENSE_PALLETS") ?? 0
            }).Parameter("APPOINTMENT_ID", appointmentId);

            return _db.ExecuteSingle(QUERY, binder);


        }


        public IList<ParentShipmentHeadline> GetParentShipmentList()
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                              WITH BOX_COUNT AS
                             (SELECT PS.SHIPPING_ID,  
                                COUNT(CASE
                                            WHEN (B.STOP_PROCESS_REASON IS NULL OR B.STOP_PROCESS_REASON != '$BOXCANCEL') AND
                                        B.SUSPENSE_DATE IS NULL AND B.REJECTION_CODE IS NULL THEN
                                             B.UCC128_ID
                                       END) AS NO_OF_BOXES
                                FROM <proxy /> PS
                                LEFT OUTER JOIN <proxy />BOX B
                                  ON B.PICKSLIP_ID = PS.PICKSLIP_ID
                               WHERE PS.SHIPPING_ID IS NOT NULL
                               GROUP BY PS.SHIPPING_ID)
                            SELECT S.PARENT_SHIPPING_ID AS PARENT_SHIPPING_ID,
                                   MAX(S.STATUS_SHIPPED_DATE) AS STATUS_SHIPPED_DATE,
                                   MAX(S.SHIP_DATE) AS SHIP_DATE,
                                -- MAX(S.ARRIVAL_DATE) AS ARRIVAL_DATE,
                                  MAX(S.MBOL_ID) AS MBOL_ID,
                                   MAX(S.CARRIER_ID) AS CARRIER_ID,
                                   --MAX(MC.SCAC_CODE) AS CARRIER_ID,
                                  MAX(MC.DESCRIPTION) AS CARRIER_DESCRIPTION,
                                  MAX(S.CUSTOMER_ID) AS CUSTOMER_ID,
                                  MAX(M.NAME) AS CUSTOMER_NAME,
                                  SUM(BC.NO_OF_BOXES) AS NO_OF_BOXES
                              FROM <proxy />SHIP S
                              LEFT OUTER JOIN <proxy />MASTER_CUSTOMER M
                                ON M.CUSTOMER_ID = S.CUSTOMER_ID
                              LEFT OUTER JOIN<proxy /> MASTER_CARRIER MC
                                ON MC.CARRIER_ID = S.CARRIER_ID
                              LEFT OUTER JOIN BOX_COUNT BC
                                ON S.SHIPPING_ID = BC.SHIPPING_ID
                             GROUP BY S.PARENT_SHIPPING_ID
                             ORDER BY MAX(S.STATUS_SHIPPED_DATE) DESC,S.PARENT_SHIPPING_ID DESC";

            var binder = SqlBinder.Create(row => new ParentShipmentHeadline
            {
                ParentShippingId = row.GetString("PARENT_SHIPPING_ID"),
                MBolID = row.GetString("MBOL_ID"),
                //ArrivalDate = row.GetDate("ARRIVAL_DATE"),
                ShippingDate = row.GetDate("SHIP_DATE"),
                StatusShippedDate = row.GetDate("STATUS_SHIPPED_DATE"),
                BoxCount = row.GetInteger("NO_OF_BOXES"),
                CarrierId = row.GetString("CARRIER_ID"),
                CarrierName = row.GetString("CARRIER_DESCRIPTION"),
                CustomerID = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME")

            });
            return _db.ExecuteReader(QUERY, binder, 200);
        }

    }
}