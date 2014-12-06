using System;
using System.Web.Mvc;

namespace EclipseLibrary.Mvc.Html
{
    /// <summary>
    /// This class is passed to you as a parameter to the lambda specified in <see cref="WebGridColumnEx{T}.CellHtml(System.Func{WebGridCellEx{T},object})"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the model on which the row is based</typeparam>
    /// <remarks>
    /// <para>
    /// The <see cref="ViewPage{TModel}.Html"/> property can be used to generate custom HTML using any of the standard helpers.
    /// The <see cref="Value"/> property is a convenience which lets you know the value that will be displayed in the column.
    /// </para>
    /// </remarks>
    [Obsolete]
    public class WebGridCellEx<TModel>: ViewPage<TModel>
    {
        public WebGridCellEx(HtmlHelper helper)
        {
            Html = new HtmlHelper<TModel>(helper.ViewContext, this, helper.RouteCollection);                
        }
        /// <summary>
        /// Creates the HTML helper for the row.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="rowIndex"></param>
        internal void SetModel(TModel row, int rowIndex)
        {
            this.ViewData = new ViewDataDictionary<TModel>(row);
            this.Html.ViewData.Model = row;
            RowIndex = rowIndex;
        }


        /// <summary>
        /// Index of the current row
        /// </summary>
        /// <example>
        /// <para>
        /// Add a column which displays the row number.
        /// </para>
        /// <code>
        /// <![CDATA[
        /// grid.AddColumn().HeaderHtml("#").CellHtml(c => c.RowIndex + 1);
        /// ]]>
        /// </code>
        /// </example>
        public int RowIndex { get; private set; }

        /// <summary>
        /// If the column does not represent a specific property, returns null. Otherwise, returns the raw value of the property which is being displayed in the column
        /// </summary>
        public object Value { get; internal set; }

    }
}