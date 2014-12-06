using System;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace EclipseLibrary.Mvc.Helpers
{
    public static class ReflectionHelpers
    {
        ///// <summary>
        ///// Useful when a strongly typed expression is passed to you e.g. 
        ///// <c>var fieldName = ReflectionHelpers.FieldNameFor((BoxViewModel m) => m.CurrentBox.Contents[0].Style);</c>
        ///// </summary>
        ///// <typeparam name="TModel"></typeparam>
        ///// <typeparam name="TValue"></typeparam>
        ///// <param name="propertyExpression"></param>
        ///// <returns></returns>
        ///// <remarks>
        ///// Simply returns a slightly modified string representation of the lambda you pass. Supports getting field name for indexed properties
        ///// as shown in the example given in summary.
        ///// </remarks>
        //[Obsolete("Use model.NameFor(m => m.PropertyName)")]
        //public static string FieldNameFor<TModel, TValue>(Expression<Func<TModel, TValue>> propertyExpression)
        //{
        //    return ExpressionHelper.GetExpressionText(propertyExpression);
        //}

        /// <summary>
        /// If you have an instance of the model available, then it is more convenient to use the extension function NameFor
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static string NameFor<TModel, TValue>(Expression<Func<TModel, TValue>> propertyExpression)
        {
            return ExpressionHelper.GetExpressionText(propertyExpression);
        }

        public static string NameFor<TModel, TValue>(this TModel value, Expression<Func<TModel, TValue>> propertyExpression)
        {
            return ExpressionHelper.GetExpressionText(propertyExpression);
        }
    }
}
