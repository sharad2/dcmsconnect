using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace EclipseLibrary.Mvc.Html
{
    /// <summary>
    /// Enables you to add error and status messages to Controller.TempData so that they can survive a redirect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Errors should be added to model state as always. <see cref="Controllers.EclipseController"/> has code to copy errors to TempData.
    /// Status messages should be added using <see cref="Controllers.EclipseController.AddStatusMessage"/>
    /// </para>
    /// <code>
    /// <![CDATA[
    /// TempData.AddErrorMessage("Your session has expired. Please start over.");
    /// ]]>
    /// </code>
    /// </remarks>
    public static class StatusSummaryExtensions
    {
        #region Public Functions

        /// <summary>
        /// Returns a status summary list.
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Status messages are added using <see cref="Controllers.EclipseController.AddStatusMessage"/>
        /// </para>
        /// </remarks>
        [Obsolete("Use StatusMessages instead")]
        public static MvcHtmlString StatusSummary(this HtmlHelper helper)
        {
            var sb = new StringBuilder();
            StatusSummaryImpl(helper.ViewContext.TempData, sb);
            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// We do not want to generate markup in compiled code. So this function returns the list of messages which you can format any way you want.
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static IList<string> StatusMessages(HtmlHelper helper)
        {
            return GetStatusList(helper.ViewContext.TempData);
        }

        public static IList<string> ErrorMessages(HtmlHelper helper)
        {
            var list = GetErrorList(helper.ViewContext.TempData);
            list.AddRange(helper.ViewContext.ViewData.ModelState.SelectMany(p => p.Value.Errors).Select(p => p.Exception == null ? p.ErrorMessage : p.Exception.Message));
            return list;
        }
        #endregion

        #region Internal TempData Storage
        private const string TEMPDATA_ERROR_MESSAGES = "ErrorMessages";
        private const string TEMPDATA_STATUS_MESSAGES = "StatusMessages";

        internal static List<string> GetErrorList(TempDataDictionary td)
        {
            return GetList(td, TEMPDATA_ERROR_MESSAGES);
        }

        internal static List<string> GetStatusList(TempDataDictionary td)
        {
            return GetList(td, TEMPDATA_STATUS_MESSAGES);
        }

        private static List<string> GetList(TempDataDictionary td, string key)
        {
            object obj;
            var b = td.TryGetValue(key, out obj);
            List<string> messages = null;
            if (b)
            {
                messages = obj as List<string>;
            }
            if (messages == null)
            {
                messages = new List<string>();
                td[key] = messages;
            }
            return messages;
        }
        #endregion


        /// <summary>
        /// Sharad 2 Dec 2011: Now displaying error messages in tempdata as well
        /// </summary>
        /// <param name="tempData"></param>
        /// <param name="sb"></param>
        private static void StatusSummaryImpl(TempDataDictionary tempData, StringBuilder sb)
        {
            var messages = GetList(tempData, TEMPDATA_STATUS_MESSAGES);
            if (messages.Count > 0)
            {
                sb.Append("<ul class=\"ui-state-highlight\">");
                foreach (var msg in messages)
                {
                    sb.AppendFormat("<li>{0}</li>", msg);
                }
                sb.Append("</ul>");
            }
            messages = GetList(tempData, TEMPDATA_ERROR_MESSAGES);
            if (messages.Count > 0)
            {
                sb.Append("<ul class=\"ui-state-error\">");
                foreach (var msg in messages)
                {
                    sb.AppendFormat("<li>{0}</li>", msg);
                }
                sb.Append("</ul>");
            }
            return;
        }
    }
}
