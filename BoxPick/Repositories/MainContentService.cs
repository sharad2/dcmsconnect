using System;
using System.Collections.Generic;
using System.Web.Routing;
using DcmsMobile.BoxPick.Models.MainContent;
using DcmsMobile.BoxPick.Models;

namespace DcmsMobile.BoxPick.Repositories
{
    public class MainContentService : IDisposable
    {
        #region Intialization
        private readonly MainContentRepository _repos;

        public MainContentService(RequestContext ctx)
        {
            var module = ctx.HttpContext.Request.Url == null ? "BOXPICK" : ctx.HttpContext.Request.Url.AbsoluteUri;
            var clientInfo = string.IsNullOrEmpty(ctx.HttpContext.Request.UserHostName) ?
                             ctx.HttpContext.Request.UserHostAddress : ctx.HttpContext.Request.UserHostName;

            _repos = new MainContentRepository(ctx.HttpContext.User.Identity.Name, module, clientInfo, ctx.HttpContext.Trace);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion

        /// <summary>
        /// Returns all possible pallets which can be picked
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Activity> GetPendingActivity()
        {
            return _repos.GetPendingActivity();
        }

        public IEnumerable<Box> GetBoxesOnPallet(string palletId)
        {
            return _repos.GetBoxesOnPallet(palletId);
        }
    }
}

/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/