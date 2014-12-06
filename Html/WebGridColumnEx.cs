using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace EclipseLibrary.Mvc.Html
{
    /// <summary>
    /// This class encapsulates all properties of a single grid column
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// It provides enough properties to allow full control on each <c>th</c> and <c>td</c> tag generated.
    /// <see cref="HeaderHtml"/> customizes what is rendered inide the <c>th</c> tag whereas <see cref="HeaderAttributes"/> provide full control
    /// on the attributes of <c>th</c>. The <c>class</c> attribute can be more conveniently specified using <see cref="HeaderStyle"/>.
    /// For the <c>td</c> tags used within the table body, corresponding methods are <see cref="CellAttributes"/>, <see cref="CellStyle(string)"/>
    /// and <see cref="CellHtml(System.Func{WebGridCellEx{T},bool},System.Func{WebGridCellEx{T},object})"/>.
    /// Similarly, for the footer you can use <see cref="FooterStyle"/> and <see cref="FooterHtml"/>.
    /// </remarks>
    [Obsolete]
    public class WebGridColumnEx<T>
    {
        internal WebGridColumnEx()
        {
            _conditionalStyles = new List<KeyValuePair<Func<WebGridCellEx<T>, bool>, string>>(4);
        }

        #region Header Cell

        private Func<WebGridColumnEx<T>, IDictionary<string, object>> _headerAttributesAccessor;

        private IDictionary<string, object> _headerAttributes;
        /// <summary>
        /// Adds a single attribute unconditionally. Can be called multiple times to add multiple attributes.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the attribute has already been added, it will be overwritten.
        /// </remarks>
        public WebGridColumnEx<T> HeaderAttribute(string name, object value)
        {
            if (_headerAttributes == null)
            {
                _headerAttributes = new Dictionary<string, object>(4);
            }
            _headerAttributes[name] = value;
            return this;
        }

        /// <summary>
        /// Attributes to be rendered with the <c>th</c> tag.
        /// </summary>
        /// <param name="headerAttributesAccessor"></param>
        /// <returns></returns>
        /// <example>
        /// <para>
        /// The attributes are supplied as a dictionary of name value pairs. The lambda is passed an instance of this class so that you can access the meta data.
        /// </para>
        /// <code>
        /// <![CDATA[
        ///     grid.AddColumn(m => m.Building).Presorted().HeaderAttributes(col => new Dictionary<string, object>
        ///      {
        ///        {"data-name", col.MetaData.PropertyName},
        ///        {"data-display-name", col.MetaData.ShortDisplayName}                                          
        ///      });
        /// ]]>
        /// </code>
        /// </example>
        public WebGridColumnEx<T> HeaderAttributes(Func<WebGridColumnEx<T>, IDictionary<string, object>> headerAttributesAccessor)
        {
            _headerAttributesAccessor = headerAttributesAccessor;
            return this;
        }

        internal IDictionary<string, object> GetHeaderAttributes()
        {
            var dict = _headerAttributesAccessor == null ? null : _headerAttributesAccessor(this);
            if (_headerAttributes != null)
            {
                if (dict == null)
                {
                    dict = new Dictionary<string, object>(_headerAttributes);
                }
                else
                {
                    foreach (var item in _headerAttributes)
                    {
                        dict[item.Key] = item.Value;
                    }
                }
            }
            return dict;
        }

        private string _headerHtml;

        /// <summary>
        /// The text you supply is not automatically encoded. Thus is is OK to embed HTML within the text.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// If <c>columnExpr</c> was passed to <see cref="WebGridEx{T}.AddColumn"/>, then the header text is automatically set to the Display Name of the property
        /// referenced by the expression. You can still choose to override it by specifying custom text.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        ///  grid.AddColumn().HeaderHtml("#").CellHtml(c => c.RowIndex + 1);
        /// ]]>
        /// </code>
        /// </example>
        public WebGridColumnEx<T> HeaderHtml(string header)
        {
            _headerHtml = header;
            return this;
        }

        internal string GetHeaderText()
        {
            string ret;
            if (!String.IsNullOrEmpty(_headerHtml))
            {
                ret = _headerHtml;
            }
            else if (MetaData != null)
            {
                ret = MetaData.ShortDisplayName ?? MetaData.DisplayName;
            }
            else
            {
                ret = string.Empty;
            }
            if (_sortDescending.HasValue)
            {
                ret += string.Format("<span class='ui-icon ui-icon-triangle-1-{0}' style='display:inline-block'></span>", _sortDescending.Value ? "s" : "n");
            }
            return ret;
        }

        private string _headerStyle;

        /// <summary>
        /// Specifies style for the header column.
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Note that there is no default value for this property. <c>CellStyle</c> is never used as header style.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// grid.AddColumn(m => m.Sku.Style).HeaderHtml("SKU").HeaderStyle("centered");
        /// ]]>
        /// </code>
        /// </example>
        public WebGridColumnEx<T> HeaderStyle(string style)
        {
            _headerStyle = style;
            return this;
        }

        internal string GetHeaderStyle()
        {
            return _headerStyle;
        }

        private bool _isSpanned;

        /// <summary>
        /// Spans the header of the previous column to cover the header of this column
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// In the example below, the heading <c>Sewing Plant</c> will span two columns
        /// </para>
        /// <code>
        /// <![CDATA[
        ///columns: new[] {
        ///    grid.AddColumn(m => m.SewingPlantId).Header("Sewing Plant").CellHtml(cellFormat),
        ///    grid.AddColumn(m => m.PlantName).Span();
        ///    grid.AddColumn(m => m.Style).CellHtml(cellFormat);
        /// ]]>
        /// </code>
        /// </remarks>
        public WebGridColumnEx<T> SpanHeader()
        {
            _isSpanned = true;
            return this;
        }

        public bool IsSpanned()
        {
            return _isSpanned;
        }

        private bool? _sortDescending;

        /// <summary>
        /// Arrow icon is displayed against presorted columns. The arrow changes direction depending on the sort direction.
        /// </summary>
        /// <param name="descending"></param>
        /// <returns></returns>
        public WebGridColumnEx<T> Presorted(bool descending = false)
        {
            _sortDescending = descending;
            return this;
        }
        #endregion

        #region Row Cell

        private Func<WebGridCellEx<T>, IDictionary<string, object>> _attributesAccessor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributesAccessor">Passed cell infomation enables you to apply attributes conditionally</param>
        /// <returns></returns>
        public WebGridColumnEx<T> CellAttributes(Func<WebGridCellEx<T>, IDictionary<string, object>> attributesAccessor)
        {
            _attributesAccessor = attributesAccessor;
            return this;
        }

        private Dictionary<string, object> _cellAttributes;

        /// <summary>
        /// Adds a single attribute unconditionally. Can be called multiple times to add multiple attributes.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the attribute has already been added, it will be overwritten.
        /// </remarks>
        public WebGridColumnEx<T> CellAttribute(string name, object value)
        {
            if (_cellAttributes == null)
            {
                _cellAttributes = new Dictionary<string, object>(4);
            }
            _cellAttributes[name] = value;
            return this;
        }

        internal IDictionary<string, object> GetCellAttributes(WebGridCellEx<T> cellItem)
        {
            var dict = _attributesAccessor == null ? null : _attributesAccessor(cellItem);
            if (_cellAttributes != null)
            {
                if (dict == null)
                {
                    dict = new Dictionary<string, object>(_cellAttributes);
                }
                else
                {
                    foreach (var item in _cellAttributes)
                    {
                        dict[item.Key] = item.Value;
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// A null predicate implies that it is always true
        /// </summary>
        private IList<KeyValuePair<Func<WebGridCellEx<T>, bool>, Func<WebGridCellEx<T>, object>>> _conditionalCellHtml;


        /// <summary>
        /// Provides the HTML to be displayed in the cell
        /// </summary>
        /// <param name="cellHtml"></param>
        /// <returns></returns>
        /// <example>
        /// <para>
        /// This example uses the helper syntax to provide the HTML. You can access cell info using the <c>item</c> variable.
        /// This variable is of type <see cref="WebGridCellEx{TModel}"/>
        /// </para>
        /// <code>
        /// <![CDATA[
        /// grid.AddColumn().HeaderHtml("Pieces")
        ///     .CellHtml(@<text>@item.Html.DisplayFor(m => m.Pieces) of @item.Html.DisplayFor(m => m.ExpectedPieces)</text>);
        ///     
        ///    grid.AddColumn(m => m.AllEpc)
        ///        .CellAttribute("style", "min-width: 14em")
        ///        .CellHtml(c => c.Html.ViewData.Model.AllEpc.Count() >= 2,
        ///@<div class="widget-container">
        ///    <div class="accordion">
        ///        <h3>
        ///            <a href="#">@string.Format("{0:N0} EPC", item.Html.ViewData.Model.AllEpc.Count())</a></h3>
        ///        <div>
        ///            @OrderedList(item.Html.ViewData.Model.AllEpc)
        ///        </div>
        ///    </div>
        ///</div>
        ///        ).CellHtml(c => OrderedList(c.Html.ViewData.Model.AllEpc, "noaccordion"));
        /// ]]>
        /// </code>
        /// <para>
        /// Alternatively, you can provide the HTML using the lambda syntax. This is useful when you are displaying simple values.
        /// </para>
        /// <code>
        /// <![CDATA[
        ///  grid.AddColumn(m => m.Header.ResvId).CellHtml(item => item.Html.ActionLink(item.Model.Header.ResvId, MVC_REQ2.REQ2.Home.ManageSku(item.Model.Header.ResvId)))
        ///      .CellAttribute("title", "Click to add SKUs");
        /// ]]>
        /// </code>
        /// </example>
        public WebGridColumnEx<T> CellHtml(Func<WebGridCellEx<T>, object> cellHtml)
        {
            //_defaultCellHtml = cellHtml;
            if (cellHtml == null) throw new ArgumentNullException("cellHtml");
            if (_conditionalCellHtml == null)
            {
                _conditionalCellHtml =
                    new List<KeyValuePair<Func<WebGridCellEx<T>, bool>, Func<WebGridCellEx<T>, object>>>(2);
            }
            _conditionalCellHtml.Add(new KeyValuePair<Func<WebGridCellEx<T>, bool>, Func<WebGridCellEx<T>, object>>(null, cellHtml));
            return this;
        }

        /// <summary>
        /// You can supply HTML based on some condition
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cellHtml"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public WebGridColumnEx<T> CellHtml(Func<WebGridCellEx<T>, bool> predicate, Func<WebGridCellEx<T>, object> cellHtml)
        {
            if (predicate == null) throw new ArgumentNullException("predicate");
            if (cellHtml == null) throw new ArgumentNullException("cellHtml");
            if (_conditionalCellHtml == null)
            {
                _conditionalCellHtml =
                    new List<KeyValuePair<Func<WebGridCellEx<T>, bool>, Func<WebGridCellEx<T>, object>>>(2);
            }
            _conditionalCellHtml.Add(new KeyValuePair<Func<WebGridCellEx<T>, bool>, Func<WebGridCellEx<T>, object>>(predicate, cellHtml));
            return this;
        }

        internal HelperResult GetCellHtml(WebGridCellEx<T> cellItem)
        {
            object result = null;
            if (_conditionalCellHtml != null)
            {
                // First check for an explicitly true predicate, and then check for the null predicate
                result = _conditionalCellHtml.Where(p => p.Key != null && p.Key(cellItem)).Select(p => p.Value(cellItem)).FirstOrDefault() ??
                    _conditionalCellHtml.Where(p => p.Key == null).Select(p => p.Value(cellItem)).FirstOrDefault();
            }
            if (result != null)
            {
                // We are done. One of the conditions was specified and result is the HTML
            }
            else if (MetaData == null)
            {
                // This column is not bound to a property. Empty HTML.
            }
            else if (cellItem.Value == null || cellItem.Value.ToString() == string.Empty)
            {
                // Value is null so NullDisplayText
                result = MetaData.NullDisplayText;
            }
            else if (string.IsNullOrEmpty(MetaData.DisplayFormatString))
            {
                // DisplayFormatString not specified. Render the value as is.
                result = cellItem.Value;
            }
            else
            {
                // Apply the DisplayFormatString
                result = string.Format(MetaData.DisplayFormatString, cellItem.Value);
            }
            return new HelperResult(tw =>
            {
                var helper = result as HelperResult;
                if (helper != null)
                {
                    helper.WriteTo(tw);
                    return;
                }
                var htmlString = result as IHtmlString;
                if (htmlString != null)
                {
                    tw.Write(htmlString);
                    return;
                }
                if (result != null)
                {
                    tw.Write(HttpUtility.HtmlEncode(result));
                }
            });

        }

        /// <summary>
        /// A null predicate implies that it is always true
        /// </summary>
        private readonly IList<KeyValuePair<Func<WebGridCellEx<T>, bool>, string>> _conditionalStyles;

        /// <summary>
        /// If <paramref name="cellItem"/> is null, cell specific styles are not included.
        /// </summary>
        /// <param name="cellItem"></param>
        /// <returns></returns>
        internal string GetCellStyle(WebGridCellEx<T> cellItem)
        {
            if (cellItem == null)
            {
                throw new ArgumentNullException("cellItem");
            }

            var style = string.Join("", _conditionalStyles.Where(p => p.Key == null || p.Key(cellItem)).Select(p => p.Value));
            return style;
        }

        /// <summary>
        /// Conditionally apply style to a cell
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// The class <c>ui-state-error</c> is applied to the cell only if pallet count is 0. The class <c>right-align</c> is appled
        /// to all cells in the column.
        /// </para>
        /// <code>
        /// <![CDATA[
        /// grid.AddColumn(m => m.PalletCount).CellStyle(m => m.Model.PalletCount == 0, "ui-state-error");
        /// ]]>
        /// </code>
        /// </remarks>
        public WebGridColumnEx<T> CellStyle(Func<WebGridCellEx<T>, bool> predicate, string style)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (string.IsNullOrWhiteSpace(style))
            {
                throw new ArgumentNullException("style");
            }
            _conditionalStyles.Add(new KeyValuePair<Func<WebGridCellEx<T>, bool>, string>(predicate, style));
            return this;
        }

        /// <summary>
        /// Apply the style to all columns
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public WebGridColumnEx<T> CellStyle(string style)
        {
            _conditionalStyles.Add(new KeyValuePair<Func<WebGridCellEx<T>, bool>, string>(null, style));
            return this;
        }
        #endregion

        #region Footer Cell

        private IDictionary<string, object> _footerAttributes;

        public WebGridColumnEx<T> FooterAttribute(string name, object value)
        {
            if (_footerAttributes == null)
            {
                _footerAttributes = new Dictionary<string, object>(4);
            }
            _footerAttributes[name] = value;
            return this;
        }

        internal IDictionary<string, object> GetFooterAttributes()
        {
            if (_footerAttributes == null)
            {
                return null;
            }

            return new Dictionary<string, object>(_footerAttributes);
        }

        private string _footerHtml;
        /// <summary>
        /// Supplies the HTML for the footer of the column
        /// </summary>
        /// <param name="footerHtml"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// In the example, the model of the view has a property called <c>EarliestDate</c> which we would like to display as footer text of the
        /// column for <c>StartDate</c>.
        /// </para>
        /// <code>
        /// <![CDATA[
        /// grid.Column(m => m.StartDate).Footer(Html.DisplayFor(m => m.EarliestDate))
        /// ]]>
        /// </code>
        /// </remarks>
        public WebGridColumnEx<T> FooterHtml(string footerHtml)
        {
            _footerHtml = footerHtml;
            return this;
        }

        internal string GetFooterHtml()
        {
            return _footerHtml;
        }

        private string _footerStyle;
        /// <summary>
        /// Specifies style for the header column.
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public WebGridColumnEx<T> FooterStyle(string style)
        {
            _footerStyle = style;
            return this;
        }

        internal string GetFooterStyle()
        {
            return _footerStyle;
        }
        #endregion

        #region Metadata

        private Func<T, object> _columnExpression;

        internal object GetColumnValue(T model)
        {
            if (_columnExpression == null)
            {
                return null;
            }
            object val;
            try
            {
                val = _columnExpression(model);
            }
            catch (NullReferenceException)
            {
                // This will happen if one of the intermediate properties in the expression is null
                // e.g. for m => m.Sku.Style, we get here if m is null or Sku is null
                val = null;
            }
            return val;
        }

        internal void SetColumnExpression<TValue>(Expression<Func<T, TValue>> columnExpr)
        {
            _columnExpression = o => columnExpr.Compile()(o);
            var columnName = ExpressionHelper.GetExpressionText(columnExpr);
            //var x = ModelMetadata.FromLambdaExpression()
            MetaData = GetMetaDataForProperty(null, typeof(T), columnName);
            return;
        }

        /// <summary>
        /// Provides access to the meta data associated with the property referenced in the lambda expression passed to
        /// <see cref="SetColumnExpression{TValue}"/>. If coumn expression has not been sset, then null is returned.
        /// </summary>
        public ModelMetadata MetaData { get; private set; }


        private static ModelMetadata GetMetaDataForProperty(Func<object> modelAccessor, Type modelType, string name)
        {
            var tokens = name.Split('.');
            var type = modelType;
            ModelMetadata meta = null;        // To keep the compiler happy
            foreach (var token in tokens)
            {
                meta = ModelMetadataProviders.Current.GetMetadataForProperty(modelAccessor, type, token);
                type = meta.ModelType;
            }
            return meta;
        }
        #endregion
    }
}
