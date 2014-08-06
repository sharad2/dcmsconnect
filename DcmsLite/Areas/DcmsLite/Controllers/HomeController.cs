
using DcmsMobile.DcmsLite.Repository.Home;
using DcmsMobile.DcmsLite.ViewModels;
using System;
using System.Net;
using System.Web.Mvc;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers
{
    
    public partial class HomeController : DcmsLiteControllerBase<HomeService>
    {
        [Route(Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_DcmsLite)]
        public virtual ActionResult Index()
        {
            return View(Views.Index, new ViewModelBase());
        }

        /// <summary>
        /// Interprets the searchtext and redirects to the appropriate url
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        /// <remarks>
        /// Status 200: Content contains the URL to redirect to
        /// Status 203: Content is error message
        /// </remarks>
        public virtual ActionResult Search(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                this.Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation; // 203
                return Content("Specify something to search");
            }
            searchText = searchText.ToUpper();
            var scanInfo = _service.ParseScan(searchText, _buildingId);
            if (scanInfo == null || scanInfo.ScanType == SearchTextType.Unrecognized)
            {
                this.Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation; // 203
                return Content("Unrecognized scan. Only UCC, Wave and Print Batch are recognized.");
            }

            // This is a workaround to StatusMessages bug. They get lost if we do not call Keep()
            TempData.Keep();

            switch (scanInfo.ScanType)
            {
                case SearchTextType.Ucc:
                    if (string.IsNullOrWhiteSpace(scanInfo.Key))
                    {
                        this.Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation; // 203
                        return Content("Unprinted UCC cannot be searched for");
                    }
                    AddStatusMessage("This Print Batch contains the UCC you scanned");
                    return Content(Url.Action(MVC_DcmsLite.DcmsLite.Pick.Batch(scanInfo.Key)));

                case SearchTextType.PrintBatch:
                    AddStatusMessage("Here is the information about the Print Batch you scanned");
                    return Content(Url.Action(MVC_DcmsLite.DcmsLite.Pick.Batch(scanInfo.Key)));

                case SearchTextType.PickWave:
                    AddStatusMessage("Here is the information about the Pick Wave you scanned");
                    return Content(Url.Action(MVC_DcmsLite.DcmsLite.Pick.Wave(int.Parse(scanInfo.Key))));

                default:
                    throw new NotImplementedException();
            }

        }
    }
}
