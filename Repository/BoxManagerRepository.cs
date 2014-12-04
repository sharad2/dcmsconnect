using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Web;
using EclipseLibrary.Oracle;


//Reviewed By: MBisht 11 June 2012
//Reviewed By: Deepak and Shiva 29 June 2012
namespace DcmsMobile.BoxManager.Repository
{
    public class BoxManagerRepository : IDisposable
    {
        #region Initialization
        //const string MODULE_NAME = "BoxManager";

        private OracleDatastore _db;

        private int _queryCount;

        /// <summary>
        /// For injecting the value through unit tests
        /// </summary>
        /// <param name="db"></param>
        public BoxManagerRepository(OracleDatastore db)
        {
            _db = db;
        }

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

        public BoxManagerRepository(TraceContext ctx, string connectString, string userName, string clientInfo, string moduleName)
        {
            var db = new OracleDatastore(ctx);
            db.CreateConnection(connectString, userName);

            db.ModuleName = moduleName;
            db.ClientInfo = clientInfo;
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

        public DbTransaction BeginTransaction()
        {
            return _db.BeginTransaction();
        }

        /// <summary>
        /// Retrieves area whihc is for bad pitch boxes. We are retreiving it from the iaconfig $BADPITCH.
        /// </summary>
        /// <returns>Returns the area where under pitched boxes are sent</returns>
        public string GetBadPitchArea()
        {
            const string QUERY = @"
           SELECT IA_ID AS IA_ID
           FROM <proxy />IACONFIG IC 
            WHERE IC.IACONFIG_ID = '$BADPITCH'
            ";
            Contract.Assert(_db != null);

            var binder = SqlBinder.Create(row => row.GetString("IA_ID"));
            ++_queryCount;
            var strBadPitchArea = _db.ExecuteSingle(QUERY, binder);
            return strBadPitchArea;
        }

        /// <summary>
        /// Returns box detail of the passed pallet or ucc.
        /// TODO: Check this query again.
        /// </summary>
        /// <param name="palletId">Returns details all boxes on this pallet</param>
        /// <param name="ucc128Id">Returns details of the passed box.</param>
        /// <param name="isVasUI">If UI is VAS UI we have to get additional info about box.</param>
        /// <returns>This function returns a list of box</returns>
        /// <remarks>
        /// If case volume is not specified, it is assumed to be 1. Boxes which have completed processing are not considered.
        /// Only one of the arguments can be non null.
        /// </remarks>
        public IEnumerable<Box> GetBoxes(string palletId, string ucc128Id, bool isVasUI = false)
        {
            var paramCount = 0;
            if (!string.IsNullOrEmpty(palletId))
            {
                ++paramCount;
            }

            if (!string.IsNullOrEmpty(ucc128Id))
            {
                ++paramCount;
            }

            if (paramCount != 1)
            {
                throw new ApplicationException("Exactly one parameter should be non null");
            }

            const string QUERY = @"
             SELECT BOX.UCC128_ID                    AS UCC128_ID,
                    BOX.LOCATION_ID                 AS LOCATION_ID,
                    BOX.CASE_ID                     AS CASE_ID,
                    NVL(SCASE.OUTER_CUBE_VOLUME, 1) AS OUTER_CUBE_VOLUME,
                    BOX.PALLET_ID                   AS PALLET_ID,
                    BOX.IA_ID                       AS IA_ID,
                    BOX.STOP_PROCESS_DATE           AS STOP_PROCESS_DATE,
                    BOX.VERIFY_DATE                 AS VERIFY_DATE,
                    BOX.REJECTION_CODE              AS REJECTION_CODE,
                    BOX.SUSPENSE_DATE               AS SUSPENSE_DATE,
                    BOX.SCAN_TO_PALLET_DATE         AS SCAN_TO_PALLET_DATE,
                    P.CUSTOMER_ID                   AS CUSTOMER_ID,
                    P.BUCKET_ID                     AS BUCKET_ID,
                    P.PO_ID                         AS PO_ID,
                    P.CUSTOMER_DC_ID                AS CUSTOMER_DC_ID,
                    P.TRANSFER_DATE                 AS TRANSFER_DATE,
                    MC.SMALL_SHIPMENT_FLAG          AS SMALL_SHIPMENT_FLAG,
<if c='$IS_VAS'>
                    (CASE
                     WHEN PSVAS.VAS_ID = '$CATALOG' THEN
                       'TRUE'
                     ELSE
                      'FALSE'
                   END)                             AS IS_VAS_REQUIRED,
                   (CASE
                     WHEN BP.BOX_PROCESS_CODE = '$CATALOG' AND BP.PROCESS_STATUS ='COMPLETED' THEN
                      'TRUE'
                     ELSE
                      'FALSE'
                   END)                             AS IS_VAS_COMPLETED,
</if>  
<else>
'FALSE' AS  IS_VAS_REQUIRED,
'FALSE' AS  IS_VAS_COMPLETED,
</else>    
                    APP.APPOINTMENT_NUMBER          AS APPOINTMENT_NUMBER,
                    APP.APPOINTMENT_DATE            AS APPOINTMENT_DATE,
                    APP.PICKUP_DOOR                 AS PICKUP_DOOR
               FROM <proxy />BOX BOX
               INNER JOIN <proxy />PS P
                  ON P.PICKSLIP_ID = BOX.PICKSLIP_ID
<if c='$IS_VAS'>
                LEFT OUTER JOIN <proxy />PS_VAS PSVAS
                  ON PSVAS.PICKSLIP_ID = P.PICKSLIP_ID
		         AND PSVAS.VAS_ID ='$CATALOG'
</if>
                LEFT OUTER JOIN <proxy />SKUCASE SCASE
                  ON BOX.CASE_ID = SCASE.CASE_ID
                LEFT OUTER JOIN <proxy />MASTER_CARRIER MC
                  ON P.CARRIER_ID = MC.CARRIER_ID
                LEFT OUTER JOIN <proxy />SHIP SHIP
                  ON SHIP.SHIPPING_ID = P.SHIPPING_ID
                LEFT OUTER JOIN <proxy />APPOINTMENT APP
                  ON APP.APPOINTMENT_ID = SHIP.APPOINTMENT_ID
<if c='$IS_VAS'>
                LEFT OUTER JOIN <proxy />BOX_VAS BP
                  ON BP.UCC128_ID = BOX.UCC128_ID
</if>
               WHERE BOX.STOP_PROCESS_DATE IS NULL
                 AND P.TRANSFER_DATE IS NULL
            <if>
                AND BOX.PALLET_ID = :PALLET_ID
            </if>
            <if>
                AND BOX.UCC128_ID = :UCC128_ID
            </if>
                ORDER BY BOX.DATE_MODIFIED DESC
            ";
            Contract.Assert(_db != null);

            var binder = SqlBinder.Create(row => new Box
            {
                Ucc128Id = row.GetString("UCC128_ID"),
                LocationId = row.GetString("LOCATION_ID"),
                IaId = row.GetString("IA_ID"),
                Case = row.GetString("CASE_ID"),
                PalletId = row.GetString("PALLET_ID"),
                Volume = row.GetDecimal("OUTER_CUBE_VOLUME").Value,
                CustomerId = row.GetString("CUSTOMER_ID"),
                BucketId = row.GetInteger("BUCKET_ID"),
                PoId = row.GetString("PO_ID"),
                CustomerDcId = row.GetString("CUSTOMER_DC_ID"),
                VerifyDate = row.GetDate("VERIFY_DATE"),
                TransferDate = row.GetDate("TRANSFER_DATE"),
                StopProcessDate = row.GetDate("STOP_PROCESS_DATE"),
                ScanToPalletDate = row.GetDate("SCAN_TO_PALLET_DATE"),
                SmallShipmentFlag = row.GetString("SMALL_SHIPMENT_FLAG"),
                RejectionCode = row.GetString("REJECTION_CODE"),
                SuspenseDate = row.GetDate("SUSPENSE_DATE"),
                AppointmentDate = row.GetDate("APPOINTMENT_DATE"),
                AppointmentNo = row.GetInteger("APPOINTMENT_NUMBER"),
                DoorId = row.GetString("PICKUP_DOOR"),
                IsVasRequired = row.GetString("IS_VAS_REQUIRED") == "TRUE",
                IsVasCompleted = row.GetString("IS_VAS_COMPLETED") == "TRUE"
            }).Parameter("PALLET_ID", palletId)
                .Parameter("UCC128_ID", ucc128Id);
            binder.ParameterXPath("IS_VAS", isVasUI);
            var result = _db.ExecuteReader(QUERY, binder);
            ++_queryCount;
            return result;
        }

        public int QueryCount
        {
            get { return _queryCount; }
        }

        /// <summary>
        /// Retrieves pallet limit which is globally set in the SPLH against splh_id $PCAP.
        /// </summary>
        /// <returns>Returns set pallet volume limit </returns>
        public decimal? GetPalletVolumeLimit()
        {
            const string QUERY = @"
           SELECT S.SPLH_VALUE AS SPLH_VALUE 
           FROM <proxy />SPLH S 
            WHERE S.SPLH_ID = '$PCAP'
            ";
            Contract.Assert(_db != null);

            var binder = SqlBinder.Create(row => row.GetString("SPLH_VALUE"));
            ++_queryCount;
            var strPalletLimit = _db.ExecuteSingle(QUERY, binder);
            if (string.IsNullOrEmpty(strPalletLimit))
            {
                // Not specified in the database
                return null;
            }
            try
            {
                return Convert.ToDecimal(strPalletLimit);
            }
            catch (FormatException)
            {
                // If the SPLH value in the database is non numeric, ignore it.
                return null;
            }
        }

        /// <summary>
        /// This function places pallet.
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="ucc128Id"></param>
        /// <param name="bTemporaryPallet">
        /// If bTemporaryPallet = true: We put box in suspense
        /// otherwise we remove the box from suspense
        /// </param>
        /// <param name="isVasUi"> </param>
        /// <returns>This is a void function</returns>
        /// <remarks>
        /// This function places passed box on passed pallet. 
        /// The box is put to suspense if the pallet is temporary pallet, otherwise it is removed from suspense. 
        /// If pallet is on location, updates the area and location of the box to that of the pallet.
        /// </remarks>
        public void PutBoxOnPallet(string ucc128Id, string palletId, bool bTemporaryPallet, bool isVasUi)
        {
            const string QUERY = @"        
 DECLARE
    LLOCATION_ID <proxy />BOX.LOCATION_ID%TYPE;
    LIA_ID       <proxy />BOX.IA_ID%TYPE;
      BEGIN
        BEGIN
            SELECT MAX(B.LOCATION_ID) AS LOCATION_ID, MAX(B.IA_ID) AS IA_ID
                INTO LLOCATION_ID, LIA_ID
                FROM <proxy />BOX B
                INNER JOIN <proxy />PS PS
                    ON B.PICKSLIP_ID = PS.PICKSLIP_ID
                WHERE B.PALLET_ID = :palletid
                AND PS.TRANSFER_DATE IS NULL
                AND B.STOP_PROCESS_DATE IS NULL
                GROUP BY B.PALLET_ID;
            EXCEPTION WHEN NO_DATA_FOUND THEN
			    NULL;
        END;
 -- We have found pallet location. Set the box location to be same.
        IF LLOCATION_ID IS NOT NULL THEN
             UPDATE <proxy />BOX B
             SET B.PALLET_ID            = :palletid,                 
                 B.LOCATION_ID          = LLOCATION_ID,
                 B.IA_ID                = NVL(LIA_ID,B.IA_ID),
                 B.IA_CHANGE_DATE       = SYSDATE,
<if c='not($isVasUi)'>
                 B.SCAN_TO_PALLET_DATE  = SYSDATE,
                 B.SCAN_TO_PALLET_BY    = USER,
</if>
                 B.SUSPENSE_DATE        = <if c='$temporarypallet'>SYSDATE</if><else>NULL</else>
           WHERE B.UCC128_ID = :ucc128Id;  
        ELSE              
 -- Update the location as NULL
             UPDATE <proxy />BOX B
             SET B.PALLET_ID            = :palletid,
                 B.LOCATION_ID          = NULL,                 
<if c='not($isVasUi)'>
                 B.SCAN_TO_PALLET_DATE  = SYSDATE,
                 B.SCAN_TO_PALLET_BY    = USER,
</if>           
                 B.SUSPENSE_DATE        = <if c='$temporarypallet'>SYSDATE</if><else>NULL</else>
            WHERE B.UCC128_ID = :ucc128Id;
        END IF;
       END;
            ";
            Contract.Assert(_db != null);

            var binder = SqlBinder.Create().Parameter("ucc128id", ucc128Id)
                .Parameter("palletid", palletId);
            binder.ParameterXPath("temporarypallet", bTemporaryPallet);
            binder.ParameterXPath("isVasUi", isVasUi);
            _db.ExecuteNonQuery(QUERY, binder);
            ++_queryCount;
        }

        /// <summary>
        /// Updates the location of the pallet. On the basis of location
        /// area of the pallet will automatically get updated by this function.
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="palletLoc"></param>
        /// <returns>This is a void function</returns>
        public void UpdatePalletLocation(string palletId, string palletLoc)
        {
            const string QUERY = @"
            DECLARE
              LIA_ID <proxy />BOX.IA_ID%TYPE;

            BEGIN
                SELECT I.IA_ID
                  INTO LIA_ID
                  FROM <proxy />IALOC I
                 WHERE I.LOCATION_ID = :LOCATION_ID;
             
              UPDATE <proxy />BOX B
                 SET B.LOCATION_ID = :LOCATION_ID, B.IA_ID = LIA_ID, B.SUSPENSE_DATE = NULL
               WHERE B.PALLET_ID = :pallet_id;
            END;
            ";
            Contract.Assert(_db != null);


            var binder = SqlBinder.Create().Parameter("location_id", palletLoc)
                .Parameter("pallet_id", palletId);
            _db.ExecuteNonQuery(QUERY, binder);
            ++_queryCount;
        }

        /// <summary>
        /// This function is for validating the location.
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns>True if the location exists else will return false</returns>
        public bool IsLocationValid(string locationId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
        SELECT COUNT(I.LOCATION_ID) AS Get_LOCATION_ID
          FROM <proxy />IALOC I
         WHERE I.LOCATION_ID = :locationid            
            ";


            var binder = SqlBinder.Create(row => row.GetInteger("Get_LOCATION_ID").Value)
                .Parameter("locationid", locationId);
            ++_queryCount;
            return _db.ExecuteSingle(QUERY, binder) > 0;
        }

        /// <summary>
        /// This function will calculate customer's pallet creation criteria
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns>Pallet creation criteria of the customer</returns>
        /// <remarks>This function is for retrieving the sorting criteria by following the SPLH philosophy of DCMS
        /// According to the SPLH philosophy this function will first look in table CUSTSPLH whether any sorting
        /// criteria is defined at customer level or not. If not then this function will look the master table SPLH
        /// for the set sorting criteria.
        /// DB: Don't agree with repos having business logic.
        /// </remarks>
        public SortCriteria GetSortCriteria(string customerId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
WITH Q1 AS
 (SELECT 'CUST' || SPLH_ID AS SPLH_ID, SPLH_VALUE
    FROM <proxy />CUSTSPLH
   WHERE CUSTOMER_ID = :customerid
  UNION
  SELECT 'GLOBEL' || SPLH_ID AS SPLH_ID, SPLH_VALUE FROM <proxy />SPLH)

SELECT *
  FROM Q1 PIVOT(MAX(SPLH_VALUE) FOR SPLH_ID IN('GLOBEL$AMP' AS GLOBEL_PO_MIXING,
                                               'GLOBEL$AMD' AS GLOBEL_DC_MIXING,
                                               'GLOBEL$AMB' AS GLOBEL_BUCKET_MIXING,
                                               'CUST$AMP' AS CUST_PO_MIXING,
                                               'CUST$AMD' AS CUST_DC_MIXING,
                                               'CUST$AMB' AS CUST_BUCKET_MIXING))
";
            var binder = SqlBinder.Create(row =>
            {
                var result = SortCriteria.NoMixing;
                // First check customer level settings
                if (row.GetString("CUST_PO_MIXING") == "Y")
                {
                    result |= SortCriteria.AllowPoMixing;
                }
                if (row.GetString("CUST_DC_MIXING") == "Y")
                {
                    result |= SortCriteria.AllowCustomerDcMixing;
                }
                if (row.GetString("CUST_BUCKET_MIXING") == "Y")
                {
                    result |= SortCriteria.AllowBucketMixing;
                }
                // No customer level setting defined. Check global settings.
                if (row.GetString("GLOBEL_PO_MIXING") == "Y")
                {
                    if (result == SortCriteria.NoMixing)
                    {
                        result |= SortCriteria.AllowPoMixing;
                    }
                }
                if (row.GetString("GLOBEL_DC_MIXING") == "Y")
                {
                    if (result == SortCriteria.NoMixing)
                    {
                        result |= SortCriteria.AllowCustomerDcMixing;
                    }
                }
                if (row.GetString("GLOBEL_BUCKET_MIXING") == "Y")
                {
                    if (result == SortCriteria.NoMixing)
                    {
                        result |= SortCriteria.AllowBucketMixing;
                    }
                }

                return result;
            }).Parameter("customerid", customerId);
            ++_queryCount;
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// This function is for retrieving the count of the boxes which are related to the past criteria waiting to be 
        /// placed on a pallet.
        /// If you need verified boxes only then pass needVerifiedBoxes = true
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="poId"></param>
        /// <param name="customerDcId"></param>
        /// <param name="bucketId"></param>
        /// <param name="needVerifiedBoxes"></param>
        /// <param name="isVasUi"> </param>
        /// <returns>This function will return the remaining box count of the set criteria boxes</returns>
        public int GetQualifyingBoxCount(string customerId, string poId, string customerDcId, int? bucketId, bool needVerifiedBoxes, bool isVasUi)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                throw new ArgumentNullException("customerId");
            }
            const string QUERY = @"
           SELECT COUNT(DISTINCT BOX.UCC128_ID) as QUALIFYING_BOXCOUNT
             FROM <proxy />BOX BOX 
            INNER JOIN <proxy />PS PS
               ON BOX.PICKSLIP_ID = PS.PICKSLIP_ID
            <if c='$isVasUi'>
            INNER JOIN <proxy />PS_VAS PSVAS
               ON PSVAS.PICKSLIP_ID = PS.PICKSLIP_ID
            </if>
            WHERE PS.CUSTOMER_ID = :customer_id
            <if c='$isVasUi'>
              AND BOX.PALLET_ID IS NULL
              AND BOX.VERIFY_DATE IS NULL
            </if>
            <else>
              AND BOX.SCAN_TO_PALLET_DATE IS NULL
            </else>
              AND BOX.STOP_PROCESS_DATE IS NULL
              AND PS.TRANSFER_DATE IS NULL
               <if>  
               AND PS.PO_ID = :po_id
               </if>
               <if>
               AND PS.CUSTOMER_DC_ID = :customer_dc_id
               </if>
               <if>  
               AND PS.BUCKET_ID = :bucket_id
               </if> 
               <if c='$needVerifiedBoxes'>  
               AND BOX.VERIFY_DATE IS NOT NULL
               </if>
            ";

            var binder = SqlBinder.Create(row => row.GetInteger("QUALIFYING_BOXCOUNT").Value)
                .Parameter("customer_id", customerId)
                .Parameter("po_id", poId)
                .Parameter("customer_dc_id", customerDcId)
                .Parameter("bucket_id", bucketId);
            binder.ParameterXPath("needVerifiedBoxes", needVerifiedBoxes);
            binder.ParameterXPath("isVasUi", isVasUi);
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// Returns a new unique Pallet ID which can be used as a temporary pallet. This will be prefixed with  TP followed by a sequence.
        /// </summary>
        /// <returns>Returns temporary pallet id</returns>
        /// <remarks>For calculating temporary pallet following oracle sequence is being used:- TEMPORARY_PALLET_SEQUENCE </remarks>
        public string GetTemporaryPalletId()
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
           SELECT 'TP'||<proxy />TEMPORARY_PALLET_SEQUENCE.Nextval AS TEMPORARY_PALLET_SEQUENCE FROM dual";

            var binder = SqlBinder.Create(row => row.GetString("TEMPORARY_PALLET_SEQUENCE"));
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// This function is for retrieving pallets for suggestion. 
        /// </summary>
        /// <param name="customerId">Criteria customer</param>
        /// <param name="bucketId">Criteria bucket</param>
        /// <param name="poId">Criteria PO</param>
        /// <param name="customerDcId">Criteria DC</param>
        /// <param name="effectivePalletVolumeLimit">Pallet volume limit. If passed then pallets should be suggested within in this limit</param>
        /// <param name="orderbyTouchDate">If the most recent pallets are required then pass this parameter as TRUE.</param>
        /// <param name="excludeAreaId">Area from which no need of pallet suggestion. </param>
        /// <param name="isVasPalletSuggestion">if true, returns only those pallets which having boxes for VAS</param>
        /// <returns>
        /// This function will return list of top 5 pallets that are most suitable for suggestions.
        /// Pallets within the set volume limit are candidate pallets for suggestions
        /// </returns>
        /// <remarks>
        /// On the basis of orderbyTouchDate flag this function will serve two purposes. If orderbyTouchDate passed as true then this function will
        /// suggest pallets which are most recently touched. In case if orderbyTouchDate is passed as false then this function will suggest pallets which are 
        /// in order of area then location. In case if no suggestions are needed from a particular area then that area should be passed through parameter excludeAreaId   
        /// </remarks>
        public IEnumerable<Pallet> SuggestPalletOrLocation(string customerId, int? bucketId, string poId, string customerDcId, decimal? effectivePalletVolumeLimit, bool orderbyTouchDate, string excludeAreaId = null, bool isVasPalletSuggestion = false)
        {
            const string QUERY = @"
    SELECT B.PALLET_ID AS PALLET_ID,
       B.LOCATION_ID AS LOCATION_ID,
       B.IA_ID AS IA_ID,
       MAX(B.IA_CHANGE_DATE) AS IA_CHANGE_DATE,
       NVL(SUM(SCASE.OUTER_CUBE_VOLUME),0) AS PALLET_VOLUME,
       COUNT(DISTINCT B.UCC128_ID) AS TOTAL_PALLET_BOXES
  FROM <proxy />BOX B
 INNER JOIN <proxy />PS PS
    ON B.PICKSLIP_ID = PS.PICKSLIP_ID
<if c='$isVasPalletSuggestion'>
 INNER JOIN <proxy />PS_VAS PSVAS
    ON PSVAS.PICKSLIP_ID = PS.PICKSLIP_ID   
</if>
  LEFT OUTER JOIN <proxy />SKUCASE SCASE
    ON B.CASE_ID = SCASE.CASE_ID
 WHERE PS.CUSTOMER_ID = :customer_id
   AND B.PALLET_ID IS NOT NULL
   AND PS.TRANSFER_DATE IS NULL
   AND B.STOP_PROCESS_DATE IS NULL
<if c='$isVasPalletSuggestion'>
   AND PSVAS.VAS_ID = '$CATALOG'
  AND B.VERIFY_DATE IS NULL  -- Don't suggest pallet if any box of pallet is verified. 
</if>
<else>
   AND B.LOCATION_ID IS NOT NULL
</else>
<if>   
  AND B.IA_ID != :excludeareaid
</if>
<if>
   AND PS.PO_ID = :po_id 
</if> 
<if>
   AND PS.CUSTOMER_DC_ID = :customer_dc_id 
</if> 
<if>
   AND PS.BUCKET_ID = :bucket_id 
</if>
 GROUP BY B.PALLET_ID, B.IA_ID, B.LOCATION_ID 
<if>
HAVING SUM(SCASE.OUTER_CUBE_VOLUME) &lt; :palletvolumelimit 
</if>
 ORDER BY
<if c='not($orderbytouchdate)'> 
 B.IA_ID, B.LOCATION_ID 
</if> 
<else>
MAX(B.IA_CHANGE_DATE) DESC,
          SUM(SCASE.OUTER_CUBE_VOLUME) 
</else>
";
            var binder = SqlBinder.Create(row => new Pallet
            {
                PalletId = row.GetString("PALLET_ID"),
                LocationId = row.GetString("LOCATION_ID"),
                IaId = row.GetString("IA_ID"),
                PalletVolume = row.GetDecimal("PALLET_VOLUME").Value,
                IaChangeDate = row.GetDate("IA_CHANGE_DATE"),
                TotalBoxesOnPallet = row.GetInteger("TOTAL_PALLET_BOXES").Value
            }).Parameter("customer_id", customerId)
                .Parameter("po_id", poId)
                .Parameter("customer_dc_id", customerDcId)
                .Parameter("bucket_id", bucketId)
                .Parameter("palletvolumelimit", effectivePalletVolumeLimit)
                .Parameter("excludeareaid", excludeAreaId)
                .Parameter("isVasPalletSuggestion", isVasPalletSuggestion ? "Y" : "");
            binder.ParameterXPath("orderbytouchdate", orderbyTouchDate);

            ++_queryCount;
            return _db.ExecuteReader(QUERY, binder, 5);
        }

        /// <summary>
        /// Puts all boxes of the pallet in suspense.
        /// </summary>
        /// <param namparam name="palletId">
        /// </param>
        public void PutBoxOfPalletInSuspence(string palletId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"       
                                UPDATE <proxy />BOX B
                                    SET B.SUSPENSE_DATE = SYSDATE
                                    WHERE B.PALLET_ID = :PALLET_ID
            ";

            var binder = SqlBinder.Create().Parameter("PALLET_ID", palletId);
            _db.ExecuteNonQuery(QUERY, binder);
            ++_queryCount;
        }

        /// <summary>
        /// Removes suspense date of passed box if it exists on the passed pallet.
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="ucc128Id"></param>
        /// <returns>
        /// bool: If passed box is on passed pallet we remove it from suspense and return true otherwise we return false.
        /// </returns>
        public bool RemoveBoxFromSuspense(string palletId, string ucc128Id)
        {
            Contract.Assert(_db != null);
            var rowCount = 0;
            const string QUERY = @"
                DECLARE
                LROWUPDATED NUMBER(1);
                    BEGIN
                        UPDATE <proxy />BOX B
                            SET B.SUSPENSE_DATE = NULL
                            WHERE B.PALLET_ID = :PALLET_ID
                            AND B.UCC128_ID = :UCC128_ID;                       
                            :LROWUPDATED := SQL%ROWCOUNT;
                        END;
            ";



            var binder = SqlBinder.Create().Parameter("PALLET_ID", palletId)
                .Parameter("UCC128_ID", ucc128Id)
                .OutParameter("LROWUPDATED", val => rowCount = val ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            ++_queryCount;
            return rowCount > 0;
        }

        /// <summary>
        /// Remove pallet from box.
        /// </summary>
        /// <param name="ucc128Id"></param>
        public void RemovePalletFromBox(string ucc128Id)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"       
                                UPDATE <proxy />BOX B
                                    SET B.PALLET_ID = NULL, B.LOCATION_ID = NULL
                                    WHERE B.UCC128_ID = :UCC128_ID
            ";

            var binder = SqlBinder.Create().Parameter("UCC128_ID", ucc128Id);
            _db.ExecuteNonQuery(QUERY, binder);
            ++_queryCount;
        }

        /// <summary>
        /// Return count of non-suspense boxes on the passed pallet.
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public int GetValidBoxesCount(string palletId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                                SELECT COUNT(UCC128_ID) AS UCC128_ID
                                FROM <proxy />BOX B
                                WHERE PALLET_ID =:PALLET_ID
                                AND B.SUSPENSE_DATE IS NULL
                                AND B.STOP_PROCESS_DATE IS NULL
                                ";

            var binder = SqlBinder.Create(row => row.GetInteger("UCC128_ID").Value).Parameter("PALLET_ID", palletId);
            ++_queryCount;
            return _db.ExecuteSingle(QUERY, binder);
        }

        public bool MarkVasComplete(string ucc128Id)
        {
            const string QUERY = @"
                               INSERT INTO <proxy />BOX_VAS BP
                                (BOX_PROCESS_CODE, UCC128_ID, PROCESS_STATUS)
                               VALUES
                                ('$CATALOG', :UCC128_ID, 'COMPLETED')
                            ";

            var binder = SqlBinder.Create()
                .Parameter("UCC128_ID", ucc128Id);
            var rowCount = _db.ExecuteDml(QUERY, binder);
            ++_queryCount;
            return rowCount > 0;
        }
    }
}

