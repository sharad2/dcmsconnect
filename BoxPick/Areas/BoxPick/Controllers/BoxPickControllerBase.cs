using System;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.BoxPick.Helpers;
using DcmsMobile.BoxPick.Repositories;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.BoxPick.Areas.BoxPick.Controllers
{
    /// <summary>
    /// Manages repository lifetime. 
    /// </summary>
    [SessionState(System.Web.SessionState.SessionStateBehavior.Required)]
    [AuthorizeEx("Box picking requires {0} role", Roles = "DCMS8_BOXPICK")]
    [BoxPickContextCheck]
    public abstract class BoxPickControllerBase : EclipseController
    {
        #region Initialize

        protected BoxPickRepository _repos;

        public BoxPickRepository Repository
        {
            get { return _repos; }
            set { _repos = value; }
        }

        /// <summary>
        /// Create the database connection
        /// </summary>
        /// <param name="requestContext"></param>
        /// <remarks>
        /// Creates a connection using the <c>dcms8</c> connection string in the default web.config file.
        /// </remarks>
        protected override void Initialize(RequestContext requestContext)
        {
            if (_repos == null)
            {
                _repos = new BoxPickRepository(requestContext);
            }
            base.Initialize(requestContext);
        }

        /// <summary>
        /// Get rid of the database connection
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            var dis = _repos as IDisposable;
            if (dis != null)
            {
                dis.Dispose();
            }
            _repos = null;
            base.Dispose(disposing);
        }

        #endregion
    }
}




//$Id$