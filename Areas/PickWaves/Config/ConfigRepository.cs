using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Web;

namespace DcmsMobile.PickWaves.Repository.Config
{
    internal class ConfigRepository : PickWaveRepositoryBase
    {
        public class Splh
        {
            [Key]
            public string SplhId { get; set; }

            /// <summary>
            /// Description of SPLH.
            /// </summary>
            public string SplhDescription { get; set; }

            /// <summary>
            /// Value of SPLH.
            /// </summary>
            public string SplhValue { get; set; }
        }

        public class CustomerSplh : Splh
        {
            /// <summary>
            /// Id of Customer who has overwritten SPLH
            /// </summary>
            public string CustomerId { get; set; }

            /// <summary>
            /// Customer Name who have overwritten SPLH
            /// </summary>
            public string CustomerName { get; set; }
        }

        #region Intialization

        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="moduleName"></param>
        /// <param name="clientInfo"></param>
        /// <param name="trace"></param>
        public ConfigRepository(TraceContext trace, string userName, string clientInfo)
            : base(trace, userName, clientInfo)
        {
        }

        #endregion

        /// <summary>
        /// This query returns list of SPLH needed for Box Creation
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Splh> GetDefaultSplhValues()
        {
            const string QUERY = @"
           SELECT SH.SPLH_ID     AS SPLH_ID,
                  SH.DESCRIPTION AS DESCRIPTION,
                  SH.SPLH_VALUE  AS SPLH_VALUE
             FROM <proxy />SPLH SH
            WHERE SH.SPLH_ID IN ('$SSB', '$MAXSKUPB', '$BOXMAXWT', '$PSMINMAX')
            ";

            var binder = SqlBinder.Create(row => new Splh
            {
                SplhId = row.GetString("SPLH_ID"),
                SplhValue = row.GetString("SPLH_VALUE"),
                SplhDescription = row.GetString("DESCRIPTION")
            });

            return _db.ExecuteReader(QUERY, binder);

        }

        /// <summary>
        /// This query returns list of SPLH with their default values and implying customers overridden value.
        /// If there is no SPLH against passed customer, method will return at least customer name and Id.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CustomerSplh> GetCustomerSplhValues(string customerId)
        {
            const string QUERY = @"
            SELECT CUST.CUSTOMER_ID AS CUSTOMER_ID,
                   CUST.NAME        AS CUSTOMER_NAME,
                   CSH.SPLH_ID      AS SPLH_ID,
                   CSH.SPLH_VALUE   AS OVERWRITTEN_VALUE
              FROM <proxy />CUST CUST
              LEFT OUTER JOIN <proxy />CUSTSPLH CSH
                ON CUST.CUSTOMER_ID = CSH.CUSTOMER_ID
               AND CSH.SPLH_ID IN ('$SSB', '$MAXSKUPB', '$BOXMAXWT')             
           <if>WHERE CUST.CUSTOMER_ID = :CUSTOMER_ID</if>
         <else>WHERE CSH.SPLH_VALUE IS NOT NULL</else>

    UNION ALL

            SELECT CUST.CUSTOMER_ID AS CUSTOMER_ID,
                   CUST.NAME AS CUSTOMER_NAME,
                   '_$MINSKUPIECES' AS SPLH_ID,
                   CAST(CUST.MIN_PIECES_PER_BOX AS VARCHAR(5)) AS OVERWRITTEN_VALUE
              FROM <proxy />CUST CUST
             WHERE CUST.MIN_PIECES_PER_BOX IS NOT NULL
           <if>AND CUST.CUSTOMER_ID = :CUSTOMER_ID</if>

    UNION ALL

            SELECT CUST.CUSTOMER_ID AS CUSTOMER_ID,
                   CUST.NAME AS CUSTOMER_NAME,
                   '_$MAXSKUPIECES' AS SPLH_ID,
                   CAST(CUST.MAX_PIECES_PER_BOX AS VARCHAR(5)) AS OVERWRITTEN_VALUE
              FROM <proxy />CUST CUST
             WHERE CUST.MAX_PIECES_PER_BOX IS NOT NULL
           <if>AND CUST.CUSTOMER_ID = :CUSTOMER_ID</if>
            ";

            var binder = SqlBinder.Create(row => new CustomerSplh
            {
                SplhId = row.GetString("SPLH_ID"),
                SplhValue = row.GetString("OVERWRITTEN_VALUE"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME")
            }).Parameter("CUSTOMER_ID", customerId);

            return _db.ExecuteReader(QUERY, binder);

        }

        /// <summary>
        /// Add or update the SPLH Values and Min Max Pieces of SKU of passed customer.
        /// If SPLH value was not passed against any SPLH Id, we delete that SPLH. 
        /// In case of Min/Max pieces we don't delete the row, we assign Null
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="splhId"></param>
        /// <param name="splhValue"></param>
        public void UpdateCustomerSplhValue(string customerId, string splhId, string splhValue)
        {
            const string QUERY_UPDATE = @"
                MERGE INTO <proxy />CUSTSPLH CSH
                USING DUAL
                ON (CSH.CUSTOMER_ID = :CUSTOMER_ID AND CSH.SPLH_ID = :SPLH_ID)
                WHEN MATCHED THEN
                    UPDATE SET CSH.SPLH_VALUE = :SPLH_VALUE
                    DELETE WHERE CSH.SPLH_VALUE IS NULL
                WHEN NOT MATCHED THEN
                    INSERT 
                    (CSH.CUSTOMER_ID, CSH.SPLH_ID, CSH.SPLH_VALUE)
                    VALUES
                    (:CUSTOMER_ID, :SPLH_ID, :SPLH_VALUE)
            ";
            const string QUERY_DELETE = @"
                DELETE FROM <proxy />CUSTSPLH CSH where CSH.CUSTOMER_ID = :CUSTOMER_ID AND CSH.SPLH_ID = :SPLH_ID
            ";

            const string QUERY_MINSKUPIECES = @"
                BEGIN
                    UPDATE <proxy />CUST CST
                        SET CST.MIN_PIECES_PER_BOX = :SPLH_VALUE                        
                    WHERE CST.CUSTOMER_ID  = :CUSTOMER_ID;
                    IF SQL%ROWCOUNT = 0 THEN
                        NULL;   -- Raise invalid customer error
                    END IF;
                 END;
              ";

            const string QUERY_MAXSKUPIECES = @"
                BEGIN
                    UPDATE <proxy />CUST CST
                        SET CST.MAX_PIECES_PER_BOX = :SPLH_VALUE
                        WHERE CST.CUSTOMER_ID  = :CUSTOMER_ID;
                    IF SQL%ROWCOUNT = 0 THEN
                        NULL;   -- Raise invalid customer error
                    END IF;
                END;
            ";

            string query;
            switch (splhId)
            {
                case "_$MINSKUPIECES":
                    query = QUERY_MINSKUPIECES;
                    break;
                case "_$MAXSKUPIECES":
                    query = QUERY_MAXSKUPIECES;
                    break;
                default:
                    query = string.IsNullOrWhiteSpace(splhValue) ? QUERY_DELETE : QUERY_UPDATE;
                    break;
            }

            var binder = SqlBinder.Create()
                .Parameter("CUSTOMER_ID", customerId)
                .Parameter("SPLH_VALUE", splhValue)
                .Parameter("SPLH_ID", splhId);
            _db.ExecuteDml(query, binder);
        }


        #region SKU Case rules
        /// <summary>
        /// This function returns a list of customer preferred SKU cases.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CustomerSkuCase> GetCustomerSkuCaseList()
        {
            const string QUERY = @"         
                 SELECT SCC.CUSTOMER_ID          AS CUSTOMER_ID,
                        CUST.NAME                AS NAME,
                        SCC.CASE_ID              AS CASE_ID,
                        SCASE.SHORT_DESCRIPTION  AS SHORT_DESCRIPTION,
                        SCASE.MAX_CONTENT_VOLUME AS MAX_CONTENT_VOLUME,
                        SCASE.OUTER_CUBE_VOLUME  AS OUTER_CUBE_VOLUME,
                        SCASE.EMPTY_WT           AS EMPTY_WT,
                        SCC.COMMENTS             AS COMMENTS
                   FROM <proxy />CUSTSKUCASE_CONSTRAINTS SCC
                  INNER JOIN <proxy />SKUCASE SCASE
                     ON SCASE.CASE_ID = SCC.CASE_ID
                   LEFT OUTER JOIN <proxy />CUST CUST
                     ON SCC.CUSTOMER_ID = CUST.CUSTOMER_ID
                  WHERE SCASE.MAX_CONTENT_VOLUME IS NOT NULL
                    AND SCASE.MAX_CONTENT_VOLUME &gt; 0.0 
                   ORDER BY SCC.CUSTOMER_ID,SCASE.MAX_CONTENT_VOLUME DESC             
            ";
            var binder = SqlBinder.Create(row => new CustomerSkuCase
            {
                CaseId = row.GetString("CASE_ID"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),
                EmptyWeight = row.GetDecimal("EMPTY_WT"),
                MaxContentVolume = row.GetDecimal("MAX_CONTENT_VOLUME"),
                OuterCubeVolume = row.GetDecimal("OUTER_CUBE_VOLUME"),
                Comment = row.GetString("COMMENTS"),
                CaseDescription = row.GetString("SHORT_DESCRIPTION")

            });
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// This function returns a list of SKU cases.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SkuCase> GetSkuCaseList(string skuCaseId)
        {
            const string QUERY = @"
            SELECT S.CASE_ID                AS CASE_ID,
                   S.SHORT_DESCRIPTION      AS SHORT_DESCRIPTION,
                   S.EMPTY_WT               AS EMPTY_WT,
                   S.MAX_CONTENT_VOLUME     AS MAX_CONTENT_VOLUME,
                   S.UNAVAILABILITY_FLAG    AS UNAVAILABILITY_FLAG,
                   S.OUTER_CUBE_VOLUME      AS OUTER_CUBE_VOLUME
              FROM <proxy />SKUCASE S
                WHERE 1 = 1
                <if>AND UPPER(S.CASE_ID) = UPPER(:CASE_ID)</if>
               ORDER BY S.UNAVAILABILITY_FLAG NULLS FIRST, S.MAX_CONTENT_VOLUME DESC
            ";
            var binder = SqlBinder.Create(row => new SkuCase
            {
                CaseId = row.GetString("CASE_ID"),
                Description = row.GetString("SHORT_DESCRIPTION"),
                EmptyWeight = row.GetDecimal("EMPTY_WT"),
                IsAvailable = string.IsNullOrEmpty(row.GetString("UNAVAILABILITY_FLAG")),
                MaxContentVolume = row.GetDecimal("MAX_CONTENT_VOLUME"),
                OuterCubeVolume = row.GetDecimal("OUTER_CUBE_VOLUME")
            }).Parameter("CASE_ID", skuCaseId);

            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// This function update property of passed SKU case.
        /// </summary>
        public void AddorUpdateSkuCase(SkuCase skuCase)
        {
            const string QUERY = @"
                MERGE INTO <proxy />SKUCASE S
                USING DUAL
                ON (S.CASE_ID = :CASE_ID)
                WHEN MATCHED THEN
                  UPDATE
                     SET S.SHORT_DESCRIPTION   = :SHORT_DESCRIPTION,
                         S.EMPTY_WT            = :EMPTY_WT,
                         S.MAX_CONTENT_VOLUME  = :MAX_CONTENT_VOLUME,
                         S.UNAVAILABILITY_FLAG = :UNAVAILABILITY_FLAG,
                         S.OUTER_CUBE_VOLUME   = :OUTER_CUBE_VOLUME,
                         S.MODIFIED_BY         = :MODIFIED_BY,
                         S.DATE_MODIFIED       = SYSDATE
                   WHERE S.CASE_ID = :CASE_ID
                WHEN NOT MATCHED THEN
                  INSERT
                    (CASE_ID,
                     SHORT_DESCRIPTION,
                     EMPTY_WT,
                     MAX_CONTENT_VOLUME,
                     UNAVAILABILITY_FLAG,
                     DATE_CREATED,
                     CREATED_BY,
                     OUTER_CUBE_VOLUME)
                  VALUES
                    (:CASE_ID,
                     :SHORT_DESCRIPTION,
                     :EMPTY_WT,
                     :MAX_CONTENT_VOLUME,
                     :UNAVAILABILITY_FLAG,
                      SYSDATE,
                     :CREATED_BY,
                     :OUTER_CUBE_VOLUME)
            ";
            var binder = SqlBinder.Create().Parameter("SHORT_DESCRIPTION", skuCase.Description)
                .Parameter("EMPTY_WT", skuCase.EmptyWeight)
                .Parameter("MAX_CONTENT_VOLUME", skuCase.MaxContentVolume)
                .Parameter("UNAVAILABILITY_FLAG", skuCase.IsAvailable ? "" : "Y")
                .Parameter("CASE_ID", skuCase.CaseId)
                .Parameter("OUTER_CUBE_VOLUME", skuCase.OuterCubeVolume)
                .Parameter("MODIFIED_BY", HttpContext.Current.User.Identity.Name)
                .Parameter("CREATED_BY", HttpContext.Current.User.Identity.Name);
            _db.ExecuteNonQuery(QUERY, binder);
        }


        /// <summary>
        /// This function returns a list of Packing rules for SKU cases
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PackingRules> GetPackingRules()
        {
            const string QUERY = @"
                  SELECT PR.STYLE       AS STYLE,
                         PR.CASE_ID     AS CASE_ID,
                         MAX(PR.IGNORE_FLAG) AS IGNORE_FLAG
                      FROM <proxy />PACKING_RULE PR
                      WHERE PR.STYLE IS NOT NULL                    
                      GROUP BY PR.STYLE, PR.CASE_ID
                      ORDER BY PR.STYLE

            ";
            var binder = SqlBinder.Create(row => new PackingRules
            {
                Style = row.GetString("STYLE"),
                CaseId = row.GetString("CASE_ID"),
                IgnoreFlag = !string.IsNullOrEmpty(row.GetString("IGNORE_FLAG"))
            });
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// This function deletes customer's sku case preference.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="caseId"></param>
        public void DelCustSkuCasePrefereence(string customerId, string caseId)
        {
            const string QUERY = @"
            DELETE FROM <proxy />CUSTSKUCASE_CONSTRAINTS S
             WHERE S.CUSTOMER_ID = :CUSTOMER_ID
               AND S.CASE_ID = :CASE_ID
            ";
            var binder = SqlBinder.Create().Parameter("CUSTOMER_ID", customerId)
                .Parameter("CASE_ID", caseId);

            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// This function delete's sku case ignorance against style.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="caseId"></param>
        public void DelCaseIgnorance(string style, string caseId)
        {
            const string QUERY = @"
                            DELETE FROM <proxy />PACKING_RULE P
                            WHERE P.STYLE = :STYLE
                            AND P.CASE_ID = :CASE_ID
            ";
            var binder = SqlBinder.Create().Parameter("STYLE", style)
                                            .Parameter("CASE_ID", caseId);
            _db.ExecuteNonQuery(QUERY, binder);

        }

        /// <summary>
        /// This function adds customer's sku case preference.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="skuCaseId"></param>
        /// <param name="comments"></param>
        public void AddCustSKuCasePreference(string customerId, string skuCaseId, string comments)
        {
            const string QUERY = @"
            INSERT INTO <proxy />CUSTSKUCASE_CONSTRAINTS
              (CUSTOMER_ID, CASE_ID, DATE_CREATED, CREATED_BY,COMMENTS)
            VALUES
              (:CUSTOMER_ID, :CASE_ID, SYSDATE, :CREATED_BY, :COMMENTS)
            ";
            var binder = SqlBinder.Create().Parameter("CUSTOMER_ID", customerId)
                                            .Parameter("CASE_ID", skuCaseId)
                                            .Parameter("CREATED_BY", HttpContext.Current.User.Identity.Name)
                                            .Parameter("COMMENTS", comments);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// This function insert a new packing rule.
        /// </summary>
        /// <param name="model"></param>
        public void InsertPackingRule(PackingRules model)
        {
            const string QUERY = @"

 MERGE INTO <proxy />PACKING_RULE S
                USING DUAL
                ON (S.STYLE = :STYLE AND S.CASE_ID = :CASE_ID)
                WHEN MATCHED THEN
                  UPDATE
                     SET S.IGNORE_FLAG         = :IGNORE_FLAG,
                         S.MODIFIED_BY         = :CREATED_BY,
                         S.MODIFIED_DATE       = SYSDATE
                   WHERE S.CASE_ID = :CASE_ID
                        AND S.STYLE   =  :STYLE
                WHEN NOT MATCHED THEN
                  INSERT
                    (PACKING_RULE_ID,STYLE, 
                      CASE_ID, 
                      IGNORE_FLAG,
                      CREATED_BY,
                      DATE_CREATED)
                   VALUES
                     (SEQ_PACKING_RULE.NEXTVAL,
                       :STYLE,
                       :CASE_ID,
                       :IGNORE_FLAG,
                       :CREATED_BY,
                        SYSDATE)
            ";

            var binder = SqlBinder.Create().Parameter("STYLE", model.Style)
                        .Parameter("CASE_ID", model.CaseId)
                        .Parameter("IGNORE_FLAG", model.IgnoreFlag ? "Y" : "")
                        .Parameter("CREATED_BY", HttpContext.Current.User.Identity.Name);
            _db.ExecuteNonQuery(QUERY, binder);
        }
        #endregion




        #region autocomplete
        /// <summary>
        /// To get customer list for Carrier Auto Complete text box
        /// </summary>
        /// <param name="searchText">
        /// Search term is passed to populate the list
        /// </param>
        /// <returns></returns>        
        public IList<Tuple<string, string>> CustomerAutoComplete(string searchId, string searchDescription)
        {
            const string QUERY = @"
                        SELECT CUST.CUSTOMER_ID AS CUSTOMER_ID, 
                           CUST.NAME AS CUSTOMER_NAME
                           FROM <proxy />CUST CUST
                           WHERE 1 = 1                     
                        and (UPPER(CUST.CUSTOMER_ID) LIKE '%' || UPPER(:CUSTOMER_ID) || '%' 
                            OR UPPER(CUST.NAME) LIKE '%' || UPPER(:CUSTOMER_NAME) ||'%')                       
                         AND ROWNUM &lt; 40 and SUBSTR(UPPER(CUST.CUSTOMER_ID), 1, 1) != '$'
                        ORDER BY CUST.CUSTOMER_ID
                        ";
            var binder = SqlBinder.Create(row => Tuple.Create(row.GetString("CUSTOMER_ID"), row.GetString("CUSTOMER_NAME")))
                .Parameter("CUSTOMER_ID", searchId)
                .Parameter("CUSTOMER_NAME", searchDescription);
            return _db.ExecuteReader(QUERY, binder);

        }

        /// <summary>
        /// To get Style list for Style Auto Complete text box
        /// </summary>
        /// <param name="searchText">
        /// Search term is passed to populate the list
        /// </param>
        /// <param name="style"> </param>
        /// <returns></returns>
        public IList<Tuple<string, string>> StyleAutoComplete(string searchId, string searchDescription)
        {
            const string QUERY = @"
                SELECT MS.STYLE AS STYLE,
                       MS.DESCRIPTION AS DESCRIPTION
                  FROM <proxy />MASTER_STYLE MS
                 WHERE 1=1
                        and (UPPER(MS.STYLE) LIKE '%' || UPPER(:STYLE) || '%' 
                            OR UPPER(MS.DESCRIPTION) LIKE '%' || UPPER(:DESCRIPTION) ||'%')                       
                         AND ROWNUM &lt; 40
                        ORDER BY CUST.CUSTOMER_ID";
            var binder = SqlBinder.Create(row => Tuple.Create(row.GetString("STYLE"), row.GetString("DESCRIPTION")))
                .Parameter("STYLE", searchId)
                .Parameter("DESCRIPTION", searchDescription);
            return _db.ExecuteReader(QUERY, binder);
        }

        #endregion
    }
}