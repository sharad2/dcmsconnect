using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EclipseLibrary.Mvc.Html
{
    public class GroupSelectListItem : SelectListItem
    {
        public string GroupText { get; set; }
    }

    public static class GroupDropListExtensions
    {
        /// <summary>
        /// Render a grouped drop down list for the specified property of the model and the spefied grouped data
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="helper">Not used. Helps in deducing model type.</param>
        /// <param name="expression"></param>
        /// <param name="data"></param>
        /// <returns>HTML to render</returns>
        /// <remarks>
        /// <para>
        /// Items with empty GroupText are supported. They are appended directly to the <c>select</c> element and appear in the beginning, outside of all groups.
        /// </para>
        /// <para>
        /// The item with <c>Selected</c> set to true is initially selected. If selected item is not specified, the first item is selected.
        /// </para>
        /// <para>
        /// The sorting of the passed <paramref name="data"/> is preserved. The first group in the sort order is the first group encountered in the passed data,
        /// and so on. This is as per the behavior of the LINQ GroupBy() method.
        /// </para>
        /// <para>
        /// In order to use this helper, the <c>web.config</c> in Views folder must reference the <c>EclipseLibrary.Mvc.Html</c> namespace.
        /// </para>
        /// <code>
        /// <![CDATA[
        ///<system.web.webPages.razor>
        ///<pages pageBaseType="System.Web.Mvc.WebViewPage">
        ///    <namespaces>
        ///    <add namespace="System.Web.Mvc" />
        ///    <add namespace="System.Web.Mvc.Html" />
        ///    <add namespace="System.Web.Routing" />
        ///    <add namespace="EclipseLibrary.Mvc.Html" />
        ///    </namespaces>
        ///</pages>
        ///</system.web.webPages.razor>
        /// ]]>
        /// </code>
        /// </remarks>
        public static MvcHtmlString GroupDropListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression,
            IEnumerable<GroupSelectListItem> data)
        {
            var select = GroupDropListForImpl(helper, expression, data);
            return new MvcHtmlString(select.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString GroupDropListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression,
            IEnumerable<GroupSelectListItem> data, object htmlAttributes)
        {
            var select = GroupDropListForImpl(helper, expression, data);
            if (htmlAttributes != null)
            {
                select.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            }
            return new MvcHtmlString(select.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString GroupDropListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression,
            IEnumerable<GroupSelectListItem> data, IDictionary<string, object> htmlAttributes)
        {
            var select = GroupDropListForImpl(helper, expression, data);
            if (htmlAttributes != null)
            {
                select.MergeAttributes(htmlAttributes);
            }
            return new MvcHtmlString(select.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString GroupDropListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression,
            IEnumerable<GroupSelectListItem> data, string optionLabel)
        {
            return helper.GroupDropListFor(expression, data, optionLabel, null);
        }

        /// <summary>
        /// Sharad 12 Jul 2012: GroupDropListFor() Can specify html attributes along with option label. 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <param name="data"></param>
        /// <param name="optionLabel"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString GroupDropListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression,
            IEnumerable<GroupSelectListItem> data, string optionLabel, object htmlAttributes)
        {
            IEnumerable<GroupSelectListItem> finaldata;
            if (string.IsNullOrWhiteSpace(optionLabel))
            {
                finaldata = data;
            }
            else
            {
                finaldata = Enumerable.Repeat(new GroupSelectListItem { Value = string.Empty, Text = optionLabel}, 1).Concat(data);
            }
            var select = GroupDropListForImpl(helper, expression, finaldata);
            if (htmlAttributes != null)
            {
                select.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            }
            return new MvcHtmlString(select.ToString(TagRenderMode.Normal));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sharad 24 April 2012: Generating Validation attributes for client side validation
        /// </remarks>
        private static TagBuilder GroupDropListForImpl<TModel, TProperty>(HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, IEnumerable<GroupSelectListItem> data)
        {
            var select = new TagBuilder("select");

            var name = ExpressionHelper.GetExpressionText(expression);
            select.Attributes.Add("name", name);
            var valAttr = helper.GetUnobtrusiveValidationAttributes(name);
            select.MergeAttributes(valAttr);

            if (data == null)
            {
                return select;
            }

            // If an item has not been explicitly selected, use the expression to determine the selected item
            if (helper.ViewData.Model != null && !data.Any(p => p.Selected))
            {
                //TODO: This might cause a null reference if the expression refers to a nested child property and one the properties in the chain evaluates to null.
                // e.g. m => m.Prop1.Prop2
                var selectedValue = expression.Compile()(helper.ViewData.Model);
                if (selectedValue != null)
                {
                    var str = selectedValue.ToString();
                    var selectedItem = data.FirstOrDefault(p => p.Value == str);
                    if (selectedItem != null)
                    {
                        selectedItem.Selected = true;
                    }
                }
            }

            var selectHtml = new StringBuilder();

            // Take care of empty group name. These are added directly to select element without the optgroup
            selectHtml.AppendLine(GetOptionsHtml(data.Where(p => string.IsNullOrWhiteSpace(p.GroupText))));

            foreach (var group in data.Where(p => !string.IsNullOrWhiteSpace(p.GroupText)).GroupBy(p => p.GroupText))
            {
                selectHtml.AppendFormat("<optgroup label=\"{0}\">{1}</optgroup>", HttpUtility.HtmlEncode(group.Key),
                    GetOptionsHtml(group));
                selectHtml.AppendLine();
            }
            select.InnerHtml = selectHtml.ToString();
            return select;
        }

        private static string GetOptionsHtml(IEnumerable<SelectListItem> items)
        {
            var optHtml = new StringBuilder();
            foreach (var item in items)
            {
                var option = new TagBuilder("option");
                // Do not encode item.Value; Otherwise ids like L&F will not post properly
                option.Attributes.Add("value", item.Value);
                if (item.Selected)
                {
                    option.Attributes.Add("selected", "selected");
                }
                option.InnerHtml = HttpUtility.HtmlEncode(item.Text);
                optHtml.AppendLine(option.ToString(TagRenderMode.Normal));
            }
            return optHtml.ToString();
        }
    }


}
