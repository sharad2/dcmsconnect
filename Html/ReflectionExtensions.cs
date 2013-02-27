using System;
using System.Linq.Expressions;
using System.Web.Mvc;


namespace EclipseLibrary.Mvc.Html.ModelBinding
{
    /// <summary>
    /// This class will go away once all apps have moved up to MVC 4. The namespace is very wrong because we had to choose a namespace which already existed.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Returns the name of the property referenced in the specified lambda expression
        /// e.g. Html.FieldNameFor(m => m.ScanModel.ScanText)"
        /// </summary>
        /// <typeparam name="TModel">Type of the model. Automatically deduced from <paramref name="helper"/>.</typeparam>
        /// <typeparam name="TValue">Type of the value returned by the property. Automatically deduced.</typeparam>
        /// <param name="propertyExpression">Lambda expression e.g. m => m.ScanModel.ScanText</param>
        /// <param name="helper">Not used. Exists only to make type deduction possible.</param>
        /// <returns>The field name</returns>
        /// <remarks>
        /// Simply returns a slightly modified string representation of the lambda you pass. Supports getting field name for indexed properties.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <input type="text" id="scan" name="@Html.NameFor(m => m.ScanModel.ScanText)" />
        /// ]]>
        /// </code>
        /// </example>
        [Obsolete("Not needed in MVC 4")]
        public static string NameFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> propertyExpression)
        {
            return ExpressionHelper.GetExpressionText(propertyExpression);
        }

        /// <summary>
        /// Returns partial name of a nested property of the model
        /// </summary>
        /// <typeparam name="TModel">Type of the model</typeparam>
        /// <typeparam name="TValue">Type of the value returned by the base portion of the name</typeparam>
        /// <typeparam name="TValue2">Type of the value for which name is being requested</typeparam>
        /// <param name="helper">Not used. Exists only to make type deduction possible.</param>
        /// <param name="baseExp">Lambda specifying the child property of the model. Not used. Exists only to make type deduction possible.</param>
        /// <param name="relativeTo">Property of the child property whose name is needed.</param>
        /// <returns>Name of the child property</returns>
        [Obsolete("Not needed in MVC 4")]
        public static string NameFor<TModel, TValue, TValue2>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> baseExp, 
            Expression<Func<TValue, TValue2>> relativeTo)
        {
            return ExpressionHelper.GetExpressionText(relativeTo);
        }
    }
}

