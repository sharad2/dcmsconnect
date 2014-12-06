using EclipseLibrary.Oracle;

namespace DcmsMobile.DcmsLite.Repository.Home
{
    public class HomeRepository : DcmsLiteRepositoryBase
    {
        internal ScanInfo ParseScan(string searchText, string buildingId)
        {
            const string QUERY = @"
<if c='$is_integer'>
                SELECT :type_pickwave AS SCAN_TYPE, CAST(BKT.BUCKET_ID AS VARCHAR2(10)) as scan_key
                  FROM <proxy />BUCKET BKT
                 INNER JOIN <proxy />PS P
                    ON P.Bucket_Id = BKT.BUCKET_ID
                 WHERE BKT.BUCKET_ID = :SEARCH_TEXT
                   AND P.TRANSFER_DATE IS NULL
                   AND BKT.STATUS IS NOT NULL
                   AND P.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID
</if>
<else>
                SELECT :type_printbatch AS SCAN_TYPE, T.PALLET_ID AS scan_key
                  FROM <proxy />BOX T
                 INNER JOIN <proxy />PS P
                    ON T.PICKSLIP_ID = P.PICKSLIP_ID
                 WHERE T.PALLET_ID = :SEARCH_TEXT
                   AND P.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID and rownum &lt; 2

                UNION ALL

                SELECT :type_ucc AS SCAN_TYPE, t.pallet_id
                  FROM <proxy />BOX T
                 INNER JOIN <proxy />PS P
                    ON T.PICKSLIP_ID = P.PICKSLIP_ID
                 WHERE T.UCC128_ID = :SEARCH_TEXT
                   AND P.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID and rownum &lt; 2
</else>
                ";
            var binder = SqlBinder.Create(row => new ScanInfo
            {
                ScanType = row.GetEnum<SearchTextType>("SCAN_TYPE"),
                Key = row.GetString("SCAN_KEY")
            })
            .Parameter("SEARCH_TEXT", searchText)
            .Parameter("WAREHOUSE_LOCATION_ID", buildingId)
            .Parameter("type_pickwave", SearchTextType.PickWave.ToString())
            .Parameter("type_printbatch", SearchTextType.PrintBatch.ToString())
            .Parameter("type_ucc", SearchTextType.Ucc.ToString())
            ;
            int x;
            binder.ParameterXPath("is_integer", int.TryParse(searchText, out x));
            return _db.ExecuteSingle(QUERY, binder);
        }
    }
}