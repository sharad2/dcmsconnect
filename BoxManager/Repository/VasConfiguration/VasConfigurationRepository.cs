using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Web;

namespace DcmsMobile.BoxManager.Repository.VasConfiguration
{
    public class VasConfigurationRepository
    {
        #region Initialization

        private OracleDatastore _db;

        public VasConfigurationRepository(OracleDatastore db)
        {
            _db = db;
        }

        public OracleDatastore Db
        {
            get
            {
                return _db;
            }
        }

        public VasConfigurationRepository(TraceContext ctx, string connectString, string userName, string clientInfo, string moduleName)
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

        /// <summary>
        /// Returns the list of VAS configuration with their description.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescription> GetVasList()
        {
            const string QUERY = @"
                SELECT TV.VAS_CODE      AS VAS_CODE, 
                       TV.DESCRIPTION   AS DESCRIPTION
                  FROM <proxy />TAB_VAS TV
            ";

            var binder = SqlBinder.Create(row => new CodeDescription
            {
                Code = row.GetString("VAS_CODE"),
                Description = row.GetString("DESCRIPTION")
            });
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Returns the list of VAS Configuration with their customer for whom they are being applied.
        /// </summary>
        /// <param name="customerId">
        /// If customerId passed, returns the respective VAS configuration list.
        /// If customerId is null, returns all the list of VAS configuration.
        /// </param>
        /// <returns></returns>
        public IEnumerable<CustomerVasSetting> GetCustomerVasSettings(string customerId)
        {
            const string QUERY = @"
                SELECT MCV.CUSTOMER_ID                AS CUSTOMER_ID,
                       CUST.NAME                      AS NAME,
                       MCV.VAS_ID                     AS VAS_ID,
                       TV.DESCRIPTION                 AS DESCRIPTION,
                       MCV.VAS_REGEXP                 AS VAS_REGEXP,                     
                       MCV.VAS_REGEXP_DESCRIPTION     AS VAS_REGEXP_DESCRIPTION,
                       MCV.USER_REMARK                AS USER_REMARK,
                       MCV.INACTIVE_FLAG              AS INACTIVE_FLAG
                  FROM <proxy />MASTER_CUSTOMER_VAS MCV
                 INNER JOIN <proxy />TAB_VAS TV
                    ON TV.VAS_CODE = MCV.VAS_ID
                 INNER JOIN <proxy />CUST CUST
                    ON CUST.CUSTOMER_ID = MCV.CUSTOMER_ID
                 WHERE 1 = 1
                <if>AND MCV.CUSTOMER_ID = :CUSTOMER_ID</if>                
                  ORDER BY CUST.NAME
            ";

            var binder = SqlBinder.Create(row => new CustomerVasSetting
            {
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),
                VasId = row.GetString("VAS_ID"),
                PatternRegEx = row.GetString("VAS_REGEXP"),
                VasDescription = row.GetString("DESCRIPTION"),
                VasPatternDescription = row.GetString("VAS_REGEXP_DESCRIPTION"),
                Remark = row.GetString("USER_REMARK"),
                InactiveFlag = row.GetString("INACTIVE_FLAG") == "Y"
            })
            .Parameter("CUSTOMER_ID", customerId);
            return _db.ExecuteReader(QUERY, binder);
        }

        public enum PoQualificationType
        {
            OldOnly,
            NewOnly,
            BothOldAndNew,
            NeitherOldNorNew
        }

        static PoQualificationType MapEnum(string text)
        {
            switch (text)
            {
                case "O":
                    return PoQualificationType.OldOnly;
                case "N":
                    return PoQualificationType.NewOnly;
                case "B":
                    return PoQualificationType.BothOldAndNew;
                case "X":
                    return PoQualificationType.NeitherOldNorNew;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the qualifying POs list on the basis of PO patterns passed.
        /// Also returns the compared result of old qualified with newly qualified POs.
        /// </summary>
        /// <param name="vasId"> </param>
        /// <param name="regExpOld"></param>
        /// <param name="regExpNew"></param>
        /// <param name="customerId"> </param>
        /// <returns></returns>
        public IEnumerable<Tuple<PoQualificationType, string>> GetComprehensivePoList(string customerId, string vasId, string regExpOld, string regExpNew)
        {
            const string QUERY = @"
            WITH OLD_PO AS
                 (SELECT PO_ID FROM <proxy />PS P
                   INNER JOIN <proxy />MASTER_CUSTOMER_VAS MCV
                      ON P.CUSTOMER_ID = MCV.CUSTOMER_ID 
                   WHERE P.CUSTOMER_ID = :CUSTOMER_ID
                     AND MCV.VAS_ID = :VAS_ID
                     AND P.TRANSFER_DATE IS NULL
                     AND P.PICKSLIP_CANCEL_DATE IS NULL
                     AND REGEXP_LIKE(P.PO_ID || '@' || P.LABEL_ID, NVL(:REGEXP_OLD,'.'))
                 ),
                NEW_PO AS
                 (SELECT PO_ID FROM <proxy />PS P 
                   INNER JOIN <proxy />MASTER_CUSTOMER_VAS MCV
                      ON P.CUSTOMER_ID = MCV.CUSTOMER_ID 
                   WHERE P.CUSTOMER_ID = :CUSTOMER_ID
                     AND MCV.VAS_ID = :VAS_ID
                     AND P.TRANSFER_DATE IS NULL
                     AND P.PICKSLIP_CANCEL_DATE IS NULL
                     AND REGEXP_LIKE(P.PO_ID || '@' || P.LABEL_ID, NVL(:REGEXP_NEW,'.'))
                  ),
                OLD_PO_ONLY AS
                 (SELECT PO_ID FROM OLD_PO
                  MINUS
                  SELECT PO_ID FROM NEW_PO),
                NEW_PO_ONLY AS
                 (SELECT PO_ID FROM NEW_PO
                  MINUS
                  SELECT PO_ID FROM OLD_PO),
                BOTH_NEW_AND_OLD AS
                 (SELECT PO_ID FROM NEW_PO
                  INTERSECT
                  SELECT PO_ID FROM OLD_PO),
                NEITHER_NEW_NOR_OLD AS
                 (SELECT PO_ID
                    FROM <proxy />PS P
                   INNER JOIN <proxy />MASTER_CUSTOMER_VAS MCV
                      ON P.CUSTOMER_ID = MCV.CUSTOMER_ID
                   WHERE NOT REGEXP_LIKE(P.PO_ID || '@' || P.LABEL_ID, NVL(:REGEXP_OLD,'.'))
                     AND NOT REGEXP_LIKE(P.PO_ID || '@' || P.LABEL_ID, NVL(:REGEXP_NEW,'.'))
                     AND P.CUSTOMER_ID = :CUSTOMER_ID 
                     AND P.TRANSFER_DATE IS NULL
                     AND P.PICKSLIP_CANCEL_DATE IS NULL
                     AND MCV.VAS_ID = :VAS_ID                     
                )
                SELECT UNIQUE 'O' AS PATTERN_TYPE, O.PO_ID AS PO_ID
                  FROM OLD_PO_ONLY O
                 WHERE ROWNUM &lt; 21
                UNION ALL
                SELECT UNIQUE 'N' AS PATTERN_TYPE, O.PO_ID AS PO_ID                
                  FROM NEW_PO_ONLY O
                 WHERE ROWNUM &lt; 21
                UNION ALL
                SELECT UNIQUE 'B' AS PATTERN_TYPE, O.PO_ID AS PO_ID
                  FROM BOTH_NEW_AND_OLD O
                 WHERE ROWNUM &lt; 21
                UNION ALL
                SELECT UNIQUE 'X' AS PATTERN_TYPE, O.PO_ID AS PO_ID
                  FROM NEITHER_NEW_NOR_OLD O
                 WHERE ROWNUM &lt; 21
                 ORDER BY 2
            ";

            var binder = SqlBinder.Create(row =>
                                Tuple.Create(MapEnum(row.GetString("PATTERN_TYPE")), row.GetString("PO_ID")))
                                    .Parameter("CUSTOMER_ID", customerId)
                                    .Parameter("VAS_ID", vasId)
                                    .Parameter("REGEXP_OLD", regExpOld)
                                    .Parameter("REGEXP_NEW", regExpNew);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Returns the list of actual POs which qualifies for the passed customer, VAS Id on the basis of regular expression for Labels and PO.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <param name="regExp"> </param>
        /// <returns></returns>
        public IEnumerable<string> GetQualifyingCustomerPos(string customerId, string vasId, string regExp)
        {
            const string QUERY = @"
            SELECT DISTINCT(P.PO_ID) AS PO_ID
              FROM <proxy />PS P
             INNER JOIN <proxy />MASTER_CUSTOMER_VAS MCV
                ON P.CUSTOMER_ID = MCV.CUSTOMER_ID
             WHERE MCV.CUSTOMER_ID = :CUSTOMER_ID
               AND MCV.VAS_ID = :VAS_ID
               AND P.TRANSFER_DATE IS NULL
               AND P.PICKSLIP_CANCEL_DATE IS NULL
               AND REGEXP_LIKE(P.PO_ID || '@' || P.LABEL_ID, NVL(:REGEX_PATTERN,'.'))               
             ORDER BY P.PO_ID
            ";

            var binder = SqlBinder.Create(row => row.GetString("PO_ID"))
                                    .Parameter("CUSTOMER_ID", customerId)
                                    .Parameter("VAS_ID", vasId)
                                    .Parameter("REGEX_PATTERN", regExp);
            return _db.ExecuteReader(QUERY, binder, 20);
        }

        /// <summary>
        ///  Insert or update the passed pattern of specific customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <param name="vasPattern"></param>
        /// <param name="patternDescription"></param>
        /// <param name="userRemarks"></param>
        /// <param name="inactiveFlag"> </param>
        /// <param name="updateVas">If true then only update the VAS settings regEx and its descriptions</param>
        /// <returns></returns>
        public void UpdateVasConfiguration(string customerId, string vasId, string vasPattern, string patternDescription, string userRemarks, bool inactiveFlag, bool updateVas)
        {
            const string QUERY = @"
                MERGE INTO <proxy />MASTER_CUSTOMER_VAS MCV
                USING DUAL
                ON (MCV.CUSTOMER_ID = :CUSTOMER_ID AND MCV.vas_id=:VAS_ID)
                WHEN MATCHED THEN
                  UPDATE SET
              <if c='$UPDATE_VAS_FLAG'> 
                    MCV.VAS_REGEXP = :VAS_REGEXP, 
                    MCV.VAS_REGEXP_DESCRIPTION = :VAS_REGEXP_DESCRIPTION, 
              </if>
              <if>MCV.USER_REMARK =:USER_REMARK,</if>
              <if c='$INACTIVE_FLAG'>
                    MCV.INACTIVE_FLAG = 'Y'
              </if>
              <else>
                    MCV.INACTIVE_FLAG = NULL
              </else>
                WHEN NOT MATCHED THEN
                INSERT 
                  (MCV.CUSTOMER_ID,
                   MCV.VAS_ID,
                   MCV.VAS_REGEXP,
                   MCV.VAS_REGEXP_DESCRIPTION,
                   MCV.USER_REMARK
                   <if c='$INACTIVE_FLAG'>,MCV.INACTIVE_FLAG</if>)
                VALUES
                  (:CUSTOMER_ID,
                   :VAS_ID,
                   :VAS_REGEXP,
                   :VAS_REGEXP_DESCRIPTION,
                   :USER_REMARK
                   <if c='$INACTIVE_FLAG'>,'Y'</if>)
             ";

            var binder = SqlBinder.Create()
                .Parameter("CUSTOMER_ID", customerId)
                .Parameter("VAS_ID", vasId)
                .Parameter("VAS_REGEXP", vasPattern)
                .Parameter("VAS_REGEXP_DESCRIPTION", patternDescription)
                .Parameter("USER_REMARK", userRemarks);
            binder.ParameterXPath("INACTIVE_FLAG", inactiveFlag);
            binder.ParameterXPath("UPDATE_VAS_FLAG", updateVas);
            _db.ExecuteDml(QUERY, binder);
        }

        /// <summary>
        /// Removes the VAS configuration for the passed customer and VAS Id
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <returns></returns>
        public bool RemoveVasConfiguration(string customerId, string vasId)
        {
            const string QUERY = @"
                            DELETE FROM <proxy />MASTER_CUSTOMER_VAS MCV
                             WHERE MCV.CUSTOMER_ID = :CUSTOMER_ID
                               AND MCV.VAS_ID = :VAS_ID
                            ";

            var binder = SqlBinder.Create()
                .Parameter("CUSTOMER_ID", customerId)
                .Parameter("VAS_ID", vasId);
            var rowCount = _db.ExecuteDml(QUERY, binder);
            return rowCount > 0;
        }


        /// <summary>
        /// This method disables the VAS configuration for passed customer and VAS, on basis of all/selective non-validated orders .
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <param name="regExp"> </param>
        /// <param name="currentOrdersOnly">
        /// True: method will disable configuration from Current orders only.
        /// False: method will disable configuration from All orders excluding Current orders.
        /// Null: method will disable configuration from All orders.
        /// </param>
        internal void DisableVasConfiguration(string customerId, string vasId, string regExp, bool? currentOrdersOnly)
        {
            const string QUERY = @"
BEGIN
           <if c= 'not($currentOrdersOnly) or $allOrders'>
            UPDATE <proxy />MASTER_CUSTOMER_VAS MCV
               SET MCV.INACTIVE_FLAG = 'Y'
             WHERE MCV.CUSTOMER_ID = :CUSTOMER_ID
               AND MCV.VAS_ID = :VAS_ID;
            </if>
            <if c= '$currentOrdersOnly or $allOrders'>
            DELETE <proxy />PS_VAS T
             WHERE T.PICKSLIP_ID IN
                   (SELECT DISTINCT(P.PICKSLIP_ID) AS PICKSLIP_ID
                      FROM <proxy />PS P
                     INNER JOIN <proxy />MASTER_CUSTOMER_VAS MCV
                        ON P.CUSTOMER_ID = MCV.CUSTOMER_ID                       
                     WHERE MCV.CUSTOMER_ID = :CUSTOMER_ID
                       AND MCV.VAS_ID = :VAS_ID
                       AND P.TRANSFER_DATE IS NULL
                       AND P.PICKSLIP_CANCEL_DATE IS NULL
                       AND REGEXP_LIKE(P.PO_ID || '@' || P.LABEL_ID, NVL(:REGEX_PATTERN,'.')));
            </if>
END;
            ";

            var binder = SqlBinder.Create()
                                .Parameter("CUSTOMER_ID", customerId)
                                .Parameter("VAS_ID", vasId)
                                .Parameter("REGEX_PATTERN", regExp);
            binder.ParameterXPath("currentOrdersOnly", currentOrdersOnly != null && currentOrdersOnly.Value);
            binder.ParameterXPath("allOrders", currentOrdersOnly == null);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// This method enables the VAS configuration for passed customer and VAS, on the basis of all/selective non-validated orders.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <param name="regExp"> </param>
        /// <param name="allOrders">
        /// True: method will enable configuration from All orders.
        /// False: method will enable configuration from All orders excluding Current orders.
        /// </param>
        internal void EnableVasConfiguration(string customerId, string vasId, string regExp, bool allOrders)
        {
            const string QUERY = @"
BEGIN           
            UPDATE <proxy />MASTER_CUSTOMER_VAS MCV
               SET MCV.INACTIVE_FLAG = NULL
             WHERE MCV.CUSTOMER_ID = :CUSTOMER_ID
               AND MCV.VAS_ID = :VAS_ID;            
           <if c='$allOrders'>
           INSERT INTO <proxy />PS_VAS
            (VAS_ID, PICKSLIP_ID)
            SELECT MCV.VAS_ID, P.PICKSLIP_ID
            FROM <proxy />PS P
            INNER JOIN <proxy />MASTER_CUSTOMER_VAS MCV
                ON P.CUSTOMER_ID = MCV.CUSTOMER_ID
            WHERE MCV.CUSTOMER_ID = :CUSTOMER_ID
                AND MCV.VAS_ID = :VAS_ID
                AND P.TRANSFER_DATE IS NULL
                AND P.PICKSLIP_CANCEL_DATE IS NULL
                AND REGEXP_LIKE(P.PO_ID || '@' || P.LABEL_ID, NVL(:REGEX_PATTERN, '.'))
                AND NOT EXISTS (SELECT 1
                    FROM <proxy />PS_VAS PV
                    WHERE PV.VAS_ID = :VAS_ID
                      AND PV.PICKSLIP_ID = P.PICKSLIP_ID);
            </if>
END;
            ";

            var binder = SqlBinder.Create()
                                .Parameter("CUSTOMER_ID", customerId)
                                .Parameter("VAS_ID", vasId)
                                .Parameter("REGEX_PATTERN", regExp);
            binder.ParameterXPath("allOrders", allOrders);
            _db.ExecuteNonQuery(QUERY, binder);
        }
    }
}