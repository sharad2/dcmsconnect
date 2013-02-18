using EclipseLibrary.Oracle;
using System;

namespace DcmsMobile.DcmsLite.Repository.Validation
{
    public enum ValidationStatus
    {
        Valid,
        Invalid,
        Failed
    }

    public class ValidationRepository : DcmsLiteRepositoryBase
    {
        internal Tuple<ValidationStatus, string> ValidateBox(string uccId, string postVerificationAreaId, string badVerificationAreaId)
        {
            var message = string.Empty;
            var status = -1;
            const string QUERY = @"
            DECLARE 
                LRESULT <proxy />PKG_VALIDATE_BOX.INFO_REC;
            BEGIN 
                LRESULT := <proxy />PKG_VALIDATE_BOX_LITE.VALIDATE_BOX(AUCC128_ID       => :AUCC128_ID,
                                                                        ABADVERIFY_AREA  => :ABADVERIFY_AREA,
                                                                        APOSTVERIFY_AREA => :APOSTVERIFY_AREA);
                        :MESSAGE  := LRESULT.MESSAGE;
                        :STATUS    := LRESULT.STATUS;
            END;
            ";
            var binder = SqlBinder.Create()
                .Parameter("AUCC128_ID", uccId)
                .Parameter("APOSTVERIFY_AREA", postVerificationAreaId)
                .Parameter("ABADVERIFY_AREA", badVerificationAreaId);
            binder.OutParameter("STATUS", val => status = val)
                  .OutParameter("MESSAGE", val => message = val);
            _db.ExecuteNonQuery(QUERY, binder);

            ValidationStatus result;
            switch (status)
            {
                case 0:
                    result = ValidationStatus.Invalid;
                    break;
                case 1:
                    result = ValidationStatus.Valid;
                    break;
                case 2:
                    result = ValidationStatus.Failed;
                    break;
                default:
                    throw new NotImplementedException("Unexpected Status code : " + status);
            }
            return Tuple.Create(result, message);
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