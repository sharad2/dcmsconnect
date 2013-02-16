using EclipseLibrary.Oracle;

namespace DcmsMobile.DcmsLite.Repository.Validation
{
    public class ValidationRepository : DcmsLiteRepositoryBase
    {
        internal void ValidateBox(string uccId)
        {
            const string QUERY = @"
                BEGIN                  
                  <proxy />VALIDATE_BOX_LITE(AUCC128_ID => :AUCC128_ID);
                END;
            ";
            var binder = SqlBinder.Create()
                .Parameter("AUCC128_ID", uccId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Returns the area where validated boxes will be send after successful validation
        /// </summary>
        internal string GetPostVerificationArea()
        {
            const string QUERY = @"
                SELECT IACONFIG.IA_ID AS IA_ID
                  FROM <proxy />IACONFIG IACONFIG
                 WHERE IACONFIG.IACONFIG_ID = '$POSTVERIFY'
              ";

            var binder = SqlBinder.Create(row => row.GetString("IA_ID"));
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// Returns the area where rejected boxes will be send after validation failed
        /// </summary>
        internal string GetBadVerificationArea()
        {
            const string QUERY = @"
                  SELECT IACONFIG.IA_ID AS IA_ID
                    FROM <proxy />IACONFIG IACONFIG
                   WHERE IACONFIG.IACONFIG_ID = '$BADVERIFY'
                ";

            var binder = SqlBinder.Create(row => row.GetString("IA_ID"));
            return _db.ExecuteSingle(QUERY, binder);
        }
    }
}