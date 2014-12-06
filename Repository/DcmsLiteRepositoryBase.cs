using EclipseLibrary.Oracle;
using System;
using System.Data.Common;
using System.Web;

namespace DcmsMobile.DcmsLite.Repository
{
    public class DcmsLiteRepositoryBase : IDisposable
    {
        protected OracleDatastore _db;

        internal void Initialize(TraceContext trace, string connectionString, string userName, string clientInfo)
        {
            var store = new OracleDatastore(trace);
            store.CreateConnection(connectionString, userName);
            store.ModuleName = "DcmsLite";
            store.ClientInfo = clientInfo;
            _db = store;
        }

        public DbTransaction BeginTransaction()
        {
            return _db.BeginTransaction();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        internal string GetBuildingDescription(string buildingId)
        {
            const string QUERY = @"
                SELECT TWL.DESCRIPTION           AS DESCRIPTION
                  FROM <proxy />TAB_WAREHOUSE_LOCATION TWL
                 WHERE TWL.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID
                ";
            var binder = SqlBinder.Create(row => row.GetString("DESCRIPTION"))
                .Parameter("WAREHOUSE_LOCATION_ID", buildingId);
            return _db.ExecuteSingle(QUERY, binder);
        }

    }
}