using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Web.Routing;
using DcmsMobile.Receiving.Models.Rad;
using EclipseLibrary.Oracle;

namespace DcmsMobile.Receiving.Repository
{
    //interface IRadRepository
    //{
    //    IList<SpotCheckConfiguration> GetSpotCheckList();
    //    void SetSpotCheckPercentage(SpotCheckConfiguration spotCheck);
    //    IEnumerable<SewingPlant> GetSewingPlants();
    //    int QueryCount { get; }
    //}

    public class RadRepository : IDisposable
    {
        private int _queryCount;

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
        /// <param name="requestContext"></param>
        public RadRepository(RequestContext requestContext)
        {
            Contract.Assert(ConfigurationManager.ConnectionStrings["dcms4"] != null);
            var store = new OracleDatastore(requestContext.HttpContext.Trace);
            store.CreateConnection(ConfigurationManager.ConnectionStrings["dcms4"].ConnectionString,
                requestContext.HttpContext.SkipAuthorization ? string.Empty : requestContext.HttpContext.User.Identity.Name);
            Debug.Assert(requestContext.HttpContext.Request.Url != null, "requestContext.HttpContext.Request.Url != null");
            store.ModuleName = requestContext.HttpContext.Request.Url.AbsoluteUri;
            store.ClientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                requestContext.HttpContext.Request.UserHostName;
            _db = store;
        }

        public RadRepository(OracleDatastore db)
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
        /// returns Sewing Plant list for Sewing plant drop down list box
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SewingPlant> GetSewingPlants()
        {
            const string QUERY = @"
               SELECT SP.SEWING_PLANT_CODE      AS SEWING_PLANT_CODE,
                   SP.SEWING_PLANT_NAME         AS SEWING_PLANT_NAME,
                   SPPARENT.SEWING_PLANT_NAME   AS PARENT_SEWING_PLANT_NAME,
                   TC.NAME                      AS COUNTRY_NAME
              FROM <proxy />TAB_SEWINGPLANT SP
              LEFT OUTER JOIN <proxy />TAB_SEWINGPLANT SPPARENT
                ON SPPARENT.SEWING_PLANT_CODE = SP.PARENT_SEWING_PLANT
             LEFT OUTER JOIN <proxy />TAB_COUNTRY TC ON
                TC.COUNTRY_ID= SP.CO_OF_ORIGIN
             ORDER BY SPPARENT.SEWING_PLANT_NAME,
                      SP.SEWING_PLANT_CODE,
                      SP.SEWING_PLANT_NAME
            ";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new SewingPlant()
                {
                    SewingPlantCode = row.GetString("SEWING_PLANT_CODE"),
                    PlantName = row.GetString("SEWING_PLANT_NAME"),
                    GroupingColumn = row.GetString("PARENT_SEWING_PLANT_NAME"),
                    CountryName=row.GetString("COUNTRY_NAME")
                });
            ++_queryCount;
           return _db.ExecuteReader(QUERY,binder);
        }

        #region SpotCheck

        public IList<SpotCheckConfiguration> GetSpotCheckList()
        {
            const string QUERY = @"
        SELECT MS.SEWING_PLANT_CODE AS SEWING_PLANT_CODE,
               TS.SEWING_PLANT_NAME AS SEWING_PLANT_NAME,
               MS.STYLE             AS STYLE,
               MS.SPOTCHECK_PERCENT AS SPOTCHECK_PERCENT,
               MS.COLOR AS COLOR,
               MS.SPOTCHECK_FLAG AS SPOTCHECK_FLAG,
               MS.INSERT_DATE AS INSERT_DATE,
               MS.INSERTED_BY AS INSERTED_BY,
               MS.MODIFIED_DATE AS MODIFIED_DATE,
               MS.MODIFIED_BY AS MODIFIED_BY
          FROM <proxy />MASTER_SEWINGPLANT_STYLE MS
          LEFT OUTER JOIN <proxy />TAB_SEWINGPLANT TS
            ON TS.SEWING_PLANT_CODE = MS.SEWING_PLANT_CODE
         ORDER BY MS.MODIFIED_DATE,MS.INSERT_DATE
        ";

            var binder = SqlBinder.Create(row => new SpotCheckConfiguration()
                {
                    Style = row.GetString("STYLE"),
                    SewingPlantId = row.GetString("SEWING_PLANT_CODE"),
                    PlantName = row.GetString("SEWING_PLANT_NAME"),
                    SpotCheckPercent = row.GetInteger("SPOTCHECK_PERCENT"),
                    Color=row.GetString("COLOR"),
                    IsSpotCheckEnable = row.GetString("SPOTCHECK_FLAG").ToUpper() == "Y",
                    CreatedDate=row.GetDateTimeOffset("INSERT_DATE"),
                    CreatedBy=row.GetString("INSERTED_BY"),
                    ModifiedDate = row.GetDateTimeOffset("MODIFIED_DATE"),
                    ModifiedBy = row.GetString("MODIFIED_BY")

                });
            ++_queryCount;
            return _db.ExecuteReader(QUERY,binder);
        }


        /// <summary>
        ///Updates the spot check percent and Insert if necessary
        /// </summary>
        /// <param name="spotCheck"></param>
        public void SetSpotCheckPercentage(SpotCheckConfiguration spotCheck)
        {
            if (spotCheck.SpotCheckPercent == null)
            {
                DeleteSpotCheckPercentage(spotCheck);
                return;
            }
            const string QUERY = @"
        BEGIN
            UPDATE <proxy />MASTER_SEWINGPLANT_STYLE MS
               SET MS.SPOTCHECK_PERCENT = :SPOTCHECK_PERCENT,
                   MS.SPOTCHECK_FLAG=:SPOTCHECK_FLAG
                WHERE MS.STYLE = :STYLE            
               AND MS.SEWING_PLANT_CODE = :SEWING_PLANT_CODE
               AND MS.COLOR= :COLOR;
        IF SQL%ROWCOUNT = 0 THEN
                INSERT INTO <proxy />MASTER_SEWINGPLANT_STYLE MS
                            (MS.STYLE,
                             MS.COLOR,
                             MS.SEWING_PLANT_CODE,
                             MS.SPOTCHECK_PERCENT,
                             MS.SPOTCHECK_FLAG
                            )
                     VALUES (:STYLE,
                             :COLOR,
                             :SEWING_PLANT_CODE,
                             :SPOTCHECK_PERCENT,
                             :SPOTCHECK_FLAG);
        END IF;
        END;
           ";
            var binder = SqlBinder.Create()
                .Parameter("STYLE", spotCheck.Style)
                .Parameter("COLOR", spotCheck.Color)
                .Parameter("SEWING_PLANT_CODE", spotCheck.SewingPlantId)
                .Parameter("SPOTCHECK_PERCENT", spotCheck.SpotCheckPercent)
                .Parameter("SPOTCHECK_FLAG", spotCheck.IsSpotCheckEnable.HasValue && spotCheck.IsSpotCheckEnable.Value ? "Y" : "");
            ++_queryCount;
            _db.ExecuteNonQuery(QUERY, binder);
        }

        private void DeleteSpotCheckPercentage(SpotCheckConfiguration spotCheck)
        {
            const string QUERY = @"
            DELETE FROM <proxy />MASTER_SEWINGPLANT_STYLE MS
             WHERE MS.STYLE = :STYLE
               AND MS.SEWING_PLANT_CODE = :SEWING_PLANT_CODE
           ";
            var binder = SqlBinder.Create()
                .Parameter("STYLE", spotCheck.Style)
                .Parameter("SEWING_PLANT_CODE", spotCheck.SewingPlantId);
            ++_queryCount;
            _db.ExecuteNonQuery(QUERY, binder);
        }
        #endregion


        public int QueryCount
        {
            get { return _queryCount; }
        }

        /// <summary>
        /// List of available spot check areas
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SpotCheckArea> GetSpotCheckAreas()
        {
            const string QUERY = @"
                                select tia.short_name as area,
                                NVL(tia.warehouse_location_id,'All') as buildingid from <proxy />tab_inventory_area tia
                                where tia.is_spotcheck_area='Y'
                                order by tia.warehouse_location_id
                                ";

            var binder = SqlBinder.Create(row => new SpotCheckArea
                {
                    AreaId = row.GetString("area"),
                    BuildingId = row.GetString("buildingid")
                });

            return _db.ExecuteReader(QUERY, binder);
        }
    }
}



//$Id$