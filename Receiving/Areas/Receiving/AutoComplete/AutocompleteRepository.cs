using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Routing;
using DcmsMobile.Receiving.Models;
using EclipseLibrary.Oracle;
using DcmsMobile.Receiving.Models.Rad;
using DcmsMobile.Receiving.Areas.Receiving.Home.Repository;

namespace DcmsMobile.Receiving.Repository
{
    public class AutocompleteRepository : IDisposable
    {
        private readonly OracleDatastore _db;

        public AutocompleteRepository(RequestContext requestContext)
        {
            var db = new OracleDatastore(requestContext.HttpContext.Trace);
            db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms4"].ConnectionString,
                requestContext.HttpContext.SkipAuthorization ? string.Empty : requestContext.HttpContext.User.Identity.Name);

            // This is a well known module code so that receving reports can reliably access receiving records from src_carton_process table.
            db.ModuleName = "Receiving";
            db.ClientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                requestContext.HttpContext.Request.UserHostName;
            _db = db;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        /// <summary>
        /// To get Carrier list for Carrier Auto Complete text box
        /// </summary>
        /// <param name="searchText">
        /// Search term is passed to populate the list
        /// </param>
        /// <returns></returns>        
        public IList<Tuple<string, string>> GetCarriers(string searchId, string searchDescription)
        {
            const string QUERY = @"
                        SELECT mc.carrier_id as carrier_id, 
                                mc.description as description
                            FROM <proxy />v_carrier mc
                        where 1 =1                         
                        and (UPPER(mc.carrier_id) LIKE '%' || UPPER(:id) || '%' 
                            OR UPPER(mc.description) LIKE '%' || UPPER(:description) ||'%')                       
                        AND ROWNUM &lt; 40
                        ORDER BY mc.carrier_id
                        ";
            var binder = SqlBinder.Create(row => Tuple.Create(row.GetString("carrier_id"), row.GetString("description")))
                .Parameter("id", searchId)
                .Parameter("description", searchDescription);
            return _db.ExecuteReader(QUERY,binder);

        }

        /// <summary>
        /// To get Style list for Style Auto Complete text box
        /// </summary>
        /// <param name="searchText">
        /// Search term is passed to populate the list
        /// </param>
        /// <returns></returns>
        public IEnumerable<Style> GetStyles(string searchText, string style)
        {
            const string QUERY = @"
                SELECT MS.STYLE AS STYLE, MS.DESCRIPTION AS DESCRIPTION
                  FROM <proxy />MASTER_STYLE MS
                 WHERE 1=1
                        <if c='$style'> 
                            and MS.STYLE =:style
                        </if>
                    <else>
                   AND (UPPER(MS.STYLE) LIKE '%' || UPPER(:SEARCH) || '%')
                    </else>
                   AND ROWNUM &lt; 40
";
            var binder = SqlBinder.Create(row => new Style()
                {
                    StyleId = row.GetString("STYLE"),
                    Description = row.GetString("DESCRIPTION")
                }).Parameter("SEARCH", searchText)
                .Parameter("style",style);
            return _db.ExecuteReader(QUERY,binder);
        }

        public IEnumerable<Color> GetColors(string searchText, string color)
        {
            const string QUERY = @"
                SELECT MC.COLOR_ID AS COLOR, MC.COLOR_DESCRIPTION AS DESCRIPTION
                  FROM <proxy />MASTER_COLOR MC
                 WHERE 1=1
                        <if c='$color'> 
                            and MC.COLOR_ID =:color
                        </if>
                   <else> 
                        AND (UPPER(MC.COLOR_ID) LIKE '%' || UPPER(:SEARCH) || '%')
                    </else>
                   AND ROWNUM &lt; 40
";
            var binder = SqlBinder.Create(row => new Color()
            {
                ColorId = row.GetString("COLOR"),
                Description = row.GetString("DESCRIPTION")
            }).Parameter("SEARCH", searchText).Parameter("color", color);
            return _db.ExecuteReader(QUERY, binder);
        }

    }
}




//$Id$