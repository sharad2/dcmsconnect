
namespace DcmsMobile.DcmsLite.Repository.Validation
{
    public class ValidationService : DcmsLiteServiceBase<ValidationRepository>
    {
        /// <summary>
        /// Mark scanned box as validated.
        /// </summary>
        /// <param name="uccId"></param>
        /// <returns></returns>
        internal void ValidateBox(string uccId)
        {
            _repos.ValidateBox(uccId);
        }
        
        /// <summary>
        /// Returns the area where validated boxes will be send after successful validation
        /// </summary>
        internal string GetPostVerificationArea()
        {
            return _repos.GetPostVerificationArea();
        }

        /// <summary>
        /// Returns the area where rejected boxes will be send after validation failed
        /// </summary>
        internal string GetBadVerificationArea()
        {
            return _repos.GetBadVerificationArea();
        }
    }
}