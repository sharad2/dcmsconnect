
using System;
using System.Data.Common;
//using System.Diagnostics;
using System.Web;

namespace EclipseLibrary.Oracle.Helpers
{
    /// <summary>
    /// Provides useful functions for logging queries. I wanted to make this class internal, but it is being used by EclipseLibrary.WebForms
    /// </summary>
    public static class QueryLogging
    {
        [Obsolete("Use the overload which accepts actionName")]
        public static void TraceOracleCommand(TraceContext ctx, DbCommand cmd)
        {
            TraceOracleCommand(ctx, cmd, "Unspecified Action");
        }
        /// <summary>
        /// Designed for MVC applications. They pass the controller trace context
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="cmd"></param>
        /// <param name="actionName"></param>
        public static void TraceOracleCommand(TraceContext ctx, DbCommand cmd, string actionName)
        {
            if (ctx == null)
            {
                // This can happen during unit tests. Do nothing.
                return;
            }
            ctx.Write("QueryBegin", string.Format("{0:F} for Action {1}", DateTime.Now, actionName));
            // Make sure that PersistSecurityInfo has not been set to true in the connection string. Otherwise, the password will be traced as well.
            // OracleDatastore takes the responsibility of explicitly setting PersistSecurityInfo=false.
            //ctx.Write("Connection String", cmd.Connection.ConnectionString);
            string str = string.Format("[{0}] {1}", cmd.CommandType, cmd.CommandText.Trim());
            ctx.Write("QueryText", str);
            foreach (DbParameter param in cmd.Parameters)
            {
                if (param.Value == null)
                {
                    str = string.Format("Parameter {0} -> null (null {1}) {2}",
                        param.ParameterName, param.DbType, param.Direction);
                }
                else
                {
                    str = string.Format("Parameter {0} -> {1} ({2} {3}) {4}",
                        param.ParameterName, param.Value, param.Value.GetType(), param.DbType, param.Direction);
                }
                ctx.Write("QueryParameter", str);
            }
            return;
        }

        /// <summary>
        /// Call this after query has finished executing to trace the query end time.
        /// Writes to Diagnostics if ctx is null
        /// </summary>
        /// <param name="ctx"></param>
        public static void TraceQueryEnd(TraceContext ctx)
        {
            if (ctx == null)
            {
                System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("F"), "QueryEnd");
            }
            else
            {
                ctx.Write("QueryEnd", DateTime.Now.ToString("F"));
            }
        }

    }
}
