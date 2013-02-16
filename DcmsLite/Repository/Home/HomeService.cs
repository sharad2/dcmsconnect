
namespace DcmsMobile.DcmsLite.Repository.Home
{
    public class HomeService : DcmsLiteServiceBase<HomeRepository>
    {
        internal ScanInfo ParseScan(string searchText, string buildingId)
        {
            return _repos.ParseScan(searchText, buildingId);
        }
    }
}