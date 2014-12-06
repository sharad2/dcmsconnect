using DcmsMobile.Inquiry.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.Home
{
    public partial class HomeController : InquiryControllerBase
    {
        public HomeController()
        {
        }

        /// <summary>
        /// Default Action
        /// </summary>
        /// <returns></returns>
        [Route]
        public virtual ActionResult Index()
        {
            return View(this.Views.Index);
        }
        /// <summary>
        /// Default HTTPGet Action method of Inquiry. Displays the Inquiry home page, else redirects to the page which displays details of passed id        
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The parameter must be named id. The routing defined in AreaRegistration.cs relies on this.
        /// </remarks>
        //[Route(HomeController.ActionNameConstants.Search + "/{" + HomeController.SearchParams.id + "?}")]
        [Route("search", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Search1)]
        public virtual ActionResult Search(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Index();
            }

            id = id.Trim().ToUpper();
            IList<SearchQueryResult> results;
            using (var repos = new HomeRepository(this.HttpContext.Trace))
            {
                results = repos.ParseScan(id);
            }


            if (results == null || results.Count == 0)
            {
                this.AddStatusMessage(string.Format("Scan is not recognized {0}", id));
                return View(this.Views.Index);
            }

            // Within each group, only top rated items are retained
            // Here we associate each query result with the the SearchAttribute it corresponds to
            var joinedResults = from result in results
                                join routing in HomeRepository.SearchRoutingList on result.RoutingId equals routing.SearchAttr.RoutingId
                                select new
                                {
                                    Result = result,
                                    Routing = routing
                                };

            // Then we group the query results by Group so that we can retain best rated items in each group
            var query = (from item in joinedResults
                         group item by item.Routing.SearchAttr.Group into g
                         let bestRatingInGroup = g.Max(p => p.Routing.SearchAttr.Rating)
                         // Retain only best rated items in group
                         let finalResults = g.Where(p => p.Routing.SearchAttr.Rating == bestRatingInGroup)
                         from item2 in finalResults
                         select new ChoiceItem
                         {
                             Description = item2.Result.ScanDescription,
                             Url = Url.Action(item2.Routing.ActionName, item2.Routing.ControllerName, new
                             {
                                 id = item2.Result.Id,
                                 pk1 = item2.Result.PrimaryKey1,
                                 pk2 = item2.Result.PrimaryKey2
                             })
                         }).ToList();

            //var query = (from item in groupedResults
            //             from item2 in item.Group
            //             select new
            //             {
            //                 ActionName = item2.Routing.ActionName,
            //                 Id = item2.Result.Id,
            //                 PrimaryKey1 = item2.Result.PrimaryKey1,
            //                 PrimaryKey2 = item2.Result.PrimaryKey2,
            //                 ScanDescription = item2.Result.ScanDescription
            //             }).ToArray();


            switch (query.Count)
            {
                case 0:
                    throw new InvalidOperationException("Internal error. Since we have results, the join cannot return 0 rows");

                case 1:
                    return Redirect(query[0].Url);

                default:
                    // Multiple types
                    var model = new DisambiguateViewModel
                    {
                        Scan = id,
                        Choices = query
                    };
                    return View(Views.Disambiguate, model);
            }

        }

    }
}




//$Id$