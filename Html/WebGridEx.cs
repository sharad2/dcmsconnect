using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EclipseLibrary.Mvc.Html
{
    /// <summary>
    /// This static class contains static methods to instantiate the grid
    /// </summary>
    [Obsolete("Use Manual Loops")]
    public static class WebGridEx
    {
        /// <summary>
        /// This is the recommended way to instatiate a grid. The template argument can be deduced from the passed source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        ///<param name="helper"></param>
        ///<param name="source"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <c>WebGridEx</c> is designed to be used within the view code. First you create the grid using <see cref="GridFor{T}"/> and then
        /// add the columns you wish to display using <see cref="WebGridEx{T}.AddColumn"/>. To create the grid, pass any strongly typed enumerable as the data source.
        /// </para>
        /// <para>
        /// To render the grid you must call the <see cref="WebGridEx{T}.GetHtml"/> function at the place where you wish to render it.
        /// All parameters of <see cref="WebGridEx{T}.GetHtml"/> are optional. In most cases you should specify the <c>columns</c> parameter to indicate
        /// which columns must be displayed. Columns are added using the <see cref="WebGridEx{T}.AddColumn"/> helper.
        /// </para>
        /// <code>
        /// <![CDATA[
        ///@{
        ///    var grid = WebGridEx.Create(Model.RecentProcesses);
        ///    grid.AddColumn(m => m.StartDate).Presorted(true);
        ///    grid.AddColumn(m => m.ProcessId).CellHtml(item => Html.ActionLink(item.Value.ToString(), MVC_Receiving.Receiving.Home.Receiving(item.Model.ProcessId)));
        ///    grid.AddColumn(m => m.OperatorName);
        ///    grid.AddColumn(m => m.ProNumber);
        ///    grid.AddColumn(m => m.ProDate);
        ///    grid.AddColumn(m => m.CarrierDisplayName);
        ///    grid.AddColumn(m => m.PalletCount).CellStyle("right-align");
        ///    grid.AddColumn(m => m.CartonCount).CellStyle("right-align");    
        ///}
        /// ...
        ///@grid.GetHtml(
        ///    selectedRowStyle: "ui-state-highlight",
        ///    rowStyle: "rowA",
        ///    alternatingRowStyle: "rowB",
        ///    caption: "Recent Process List"
        ///)
        /// ]]>
        /// </code>
        /// <para>
        /// If the data source is empty, the grid does not render anything.
        /// </para>
        /// <para>
        /// Sort icons: Declare a column as <see cref="WebGridColumnEx{T}.Presorted"/> to cause a sort icon to show up in the header.
        /// </para>
        /// <para>
        /// Conditional Cell Styles: <see cref="WebGridColumnEx{T}.CellStyle(System.Func{WebGridCellEx{T},bool},string)"/>.
        /// </para>
        /// <para>
        /// Spanning headers: <see cref="WebGridColumnEx{T}.SpanHeader"/>.
        /// </para>
        /// <para>
        /// Highlighting Selected Row: Set <see cref="WebGridEx{T}.SelectedIndex"/> and supply the <c>tableStyle</c> parameter to <see cref="WebGridEx{T}.GetHtml"/>
        /// </para>
        /// <para>
        /// Auto generation of columns: Not supported.
        /// </para>
        /// <para>
        /// Footer: <see cref="WebGridColumnEx{T}.FooterHtml"/> and <see cref="WebGridColumnEx{T}.FooterStyle"/>.
        /// </para>
        /// <para>
        /// Adding a column which displays row number. <c>grid.AddColumn().HeaderHtml("#").CellHtml(c => c.RowIndex + 1)</c>. Also see <see cref="WebGridCellEx{T}.RowIndex"/>
        /// </para>
        /// <para>
        /// Sharad 21 Sep 2011: Caption defaults to DisplayName of data source property. Summary attribute of table generated from Description property of data source.
        /// emptyDataHtml defaults to NullDisplayText of data source.
        /// </para>
        /// </remarks>
        [Obsolete("Use the other overload")]
        public static WebGridEx<T> GridFor<T>(this HtmlHelper helper, IEnumerable<T> source)
        {
            return new WebGridEx<T>(helper, source, null);
        }

        public static WebGridEx<T> GridFor<TModel, T>(this HtmlHelper<TModel> helper, Expression<Func<TModel, IEnumerable<T>>> sourceExpr)
        {
            var source = sourceExpr.Compile()(helper.ViewData.Model);
            var meta = ModelMetadata.FromLambdaExpression(sourceExpr, helper.ViewData);
            return new WebGridEx<T>(helper, source, meta);
        }

    }

    /// <summary>
    /// Implements a strongly typed WebGrid. Makes full use of the MetaData associated with the properties being displayed.
    /// </summary>
    /// <typeparam name="T">The type of each item</typeparam>
    /// <remarks>
    /// <para>
    /// Inspired by http://msdn.microsoft.com/en-us/magazine/hh288075.aspx
    /// </para>
    /// <para>
    /// Columns can be added to the grid using strongly typed lambda expressions. The column header defaults to the ShortName specified in the model metadata.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// This is how you would use this WebGrid within a view
    /// </para>
    /// <code>
    /// <![CDATA[
    ///@{
    ///    var grid = WebGridEx.Create(Model.RecentProcesses);
    ///}
    ///@grid.GetHtml(
    ///    selectedRowStyle: "ui-state-highlight",
    ///    rowStyle: "rowA",
    ///    alternatingRowStyle: "rowB",
    ///        caption: "Recent Process List",
    ///    columns: new[] {
    ///            grid.Column(m => m.StartDate).Presorted(true),
    ///            grid.Column(m => m.ProcessId).Cell(item => Html.ActionLink(item.Value.ToString(), MVC_Receiving.Receiving.Home.Receiving(item.Model.ProcessId))),
    ///            grid.Column(m => m.OperatorName),
    ///            grid.Column(m => m.ProNumber),
    ///            grid.Column(m => m.ProDate),
    ///            grid.Column(m => m.CarrierDisplayName),
    ///            grid.Column(m => m.PalletCount).SetStyle("right-align"),
    ///            grid.Column(m => m.CartonCount).SetStyle("right-align")
    ///    })
    /// ]]>
    /// </code>
    /// </example>
    [Obsolete]
    public class WebGridEx<T>
    {
        private readonly IEnumerable<T> _source;
        private readonly HtmlHelper _helper;

        // Meta data associated with the source property
        private readonly ModelMetadata _metadata;

        /// <summary>
        /// Constructor is internal since the grid should be created using <see cref="WebGridEx.GridFor{T}"/>.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="source"></param>
        /// <param name="metadata"></param>
        internal WebGridEx(HtmlHelper helper, IEnumerable<T> source, ModelMetadata metadata)
        {
            _source = source;
            this.SelectedIndex = -1;
            _columns = new List<WebGridColumnEx<T>>(8);
            _helper = helper;
            _metadata = metadata;
        }

        /// <summary>
        /// The row at this index will get the <c>selectedRowStyle</c> passed to <see cref="GetHtml"/>
        /// </summary>
        // ReSharper disable MemberCanBePrivate.Global
        public int SelectedIndex { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

        private readonly IList<WebGridColumnEx<T>> _columns;

        public IList<WebGridColumnEx<T>> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Instantiates a column which is not associated with a specific property of the model
        /// </summary>
        public WebGridColumnEx<T> AddColumn()
        {
            var col = new WebGridColumnEx<T>();
            _columns.Add(col);
            return col;
        }

        /// <summary>
        /// Instantiates a column which is associated with a specific property of the model
        /// </summary>
        /// <typeparam name="TValue">You should not need to specify this. It will be inferred.</typeparam>
        /// <param name="columnExpr"></param>
        /// <returns></returns>
        public WebGridColumnEx<T> AddColumn<TValue>(Expression<Func<T, TValue>> columnExpr)
        {
            var col = new WebGridColumnEx<T>();
            col.SetColumnExpression(columnExpr);
            _columns.Add(col);
            return col;
        }

        /// <summary>
        /// Returns the HTML for the grid.
        /// </summary>
        /// <param name="tableStyle">The style to apply to the table tag.</param>
        /// <param name="headerStyle">The style to apply to the header row.</param>
        /// <param name="footerStyle">The style to apply to the footer row.</param>
        /// <param name="rowStyle">The style to apply to each data row.</param>
        /// <param name="alternatingRowStyle">The style to apply to alternating rows in place of <paramref name="rowStyle"/>.</param>
        /// <param name="selectedRowStyle">The style to apply to the row at <see cref="SelectedIndex"/>.</param>
        /// <param name="caption">Caption of the table. Defaults to the Display Name of the data source. Pass empty string as caption to suppress caption altogether</param>
        /// <param name="displayHeader">Whether header row should be displayed.</param>
        /// <param name="emptyDataHtml">The Markup to render when data source is empty. Defaults to NullDisplayText of datasource</param>
        /// <param name="id">The id attribute of the table tag.</param>
        /// <returns></returns>
        public IHtmlString GetHtml(
                    string tableStyle = null,
                    string headerStyle = null,
                    string footerStyle = null,
                    string rowStyle = null,
                    string alternatingRowStyle = null,
                    string selectedRowStyle = null,
                    string caption = null,
                    bool displayHeader = true,
                    string emptyDataHtml = null,
                    string id = null)
        {
            if (_columns.Count == 0)
            {
                // quick exit
                return MvcHtmlString.Empty;
            }
            var table = new TagBuilder("table");
            if (!String.IsNullOrEmpty(tableStyle))
            {
                table.MergeAttribute("class", tableStyle);
            }

            if (id != null)
            {
                table.MergeAttribute("id", id, true);
            }

            if (_metadata != null && !string.IsNullOrEmpty(_metadata.Description))
            {
                table.MergeAttribute("summary", _metadata.Description, true);
            }

            if (caption == null && _metadata != null)
            {
                caption = _metadata.DisplayName;
            }

            if (!String.IsNullOrEmpty(caption))
            {
                var captionTag = new TagBuilder("caption") { InnerHtml = caption };
                table.InnerHtml += captionTag;
            }

            if (displayHeader)
            {
                var thead = new TagBuilder("thead") { InnerHtml = GetTableHeaderHtml(headerStyle) };
                table.InnerHtml += thead.ToString();
            }

            // XHTML 1.1 requires that tfoot come before tbody 
            if (_columns.Any(p => !string.IsNullOrEmpty(p.GetFooterHtml())))
            {
                var tfoot = new TagBuilder("tfoot");
                var tr = new TagBuilder("tr");
                if (!String.IsNullOrEmpty(footerStyle))
                {
                    tr.MergeAttribute("class", footerStyle);
                }
                foreach (var col in _columns)
                {
                    var td = new TagBuilder("td");
                    var style = col.GetFooterStyle();
                    if (!string.IsNullOrEmpty(style))
                    {
                        td.MergeAttribute("class", style);
                    }
                    var footerAttributes = col.GetFooterAttributes();
                    if (footerAttributes != null)
                    {
                        td.MergeAttributes(footerAttributes);
                    }
                    td.InnerHtml = col.GetFooterHtml();
                    tr.InnerHtml += td;
                }
                tfoot.InnerHtml = tr.ToString();
                table.InnerHtml += tfoot.ToString();
            }

            var tbody = new TagBuilder("tbody");
            var str = GetTableBodyHtml(rowStyle, alternatingRowStyle, selectedRowStyle);
            if (string.IsNullOrWhiteSpace(str))
            {
                // Data source returned no rows. Render emptyDataHtml.
                if (emptyDataHtml == null && _metadata != null)
                {
                    emptyDataHtml = _metadata.NullDisplayText;
                }
                return emptyDataHtml == null ? MvcHtmlString.Empty : MvcHtmlString.Create(emptyDataHtml);
            }
            tbody.InnerHtml += str;

            table.InnerHtml += tbody;
            return new HtmlString(table.ToString());
        }

        private string GetTableBodyHtml(string rowStyle, string alternatingRowStyle, string selectedRowStyle)
        {
            if (_source == null)
            {
                // Quick exit
                return string.Empty;
            }
            var sb = new StringBuilder();
            int r = 0;
            var cellItem = new WebGridCellEx<T>(_helper);
            var styles = new List<string>(4);
            foreach (var row in _source)
            {
                styles.Clear();
                if (r % 2 == 0)
                {
                    if (!string.IsNullOrEmpty(rowStyle))
                    {
                        styles.Add(rowStyle);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(alternatingRowStyle))
                    {
                        styles.Add(alternatingRowStyle);
                    }
                    else if (!string.IsNullOrEmpty(rowStyle))
                    {
                        styles.Add(rowStyle);
                    }
                }
                if (r == this.SelectedIndex && !string.IsNullOrEmpty(selectedRowStyle))
                {
                    styles.Add(selectedRowStyle);
                }
                var tr = new TagBuilder("tr");
                if (styles.Count > 0)
                {
                    tr.MergeAttribute("class", string.Join(" ", styles));
                }
                cellItem.SetModel(row, r);
                foreach (var column in _columns)
                {
                    //cellItem.Column = column;
                    cellItem.Value = column.GetColumnValue(row);
                    var td = new TagBuilder("td");
                    var style = column.GetCellStyle(cellItem);
                    if (!string.IsNullOrEmpty(style))
                    {
                        td.MergeAttribute("class", style);
                    }
                    var dictAttributes = column.GetCellAttributes(cellItem);
                    if (dictAttributes != null && dictAttributes.Count > 0)
                    {
                        td.MergeAttributes(dictAttributes);
                    }
                    td.InnerHtml += column.GetCellHtml(cellItem).ToString();
                    tr.InnerHtml += td;
                }
                sb.Append(tr.ToString());
                r++;
            }
            return sb.ToString();
        }

        private string GetTableHeaderHtml(string headerStyle)
        {
            var tr = new TagBuilder("tr");
            if (!String.IsNullOrEmpty(headerStyle))
            {
                tr.MergeAttribute("class", headerStyle);
            }
            foreach (var column in _columns.Where(p => !p.IsSpanned()))
            {
                var th = new TagBuilder("th");
                var colspan = _columns.SkipWhile(p => !ReferenceEquals(p, column))
                    .Skip(1).TakeWhile(p => p.IsSpanned()).Count();
                if (colspan > 0)
                {
                    th.MergeAttribute("colspan", (colspan + 1).ToString());
                }
                th.MergeAttribute("scope", "col");
                var style = column.GetHeaderStyle();
                if (!string.IsNullOrWhiteSpace(style))
                {
                    th.MergeAttribute("class", style);
                }
                var dictAttributes = column.GetHeaderAttributes();
                if (dictAttributes != null && dictAttributes.Count > 0)
                {
                    th.MergeAttributes(dictAttributes);
                }
                var headerString = column.GetHeaderText();
                if (headerString != null)
                {
                    th.InnerHtml = headerString;
                }
                tr.InnerHtml += th.ToString();
            }
            return tr.ToString();
        }
    }
}