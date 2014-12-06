using System;
using System.Collections.Generic;

namespace EclipseLibrary.Oracle.Extensions
{
    public static class EnumerableEx
    {
        /// <summary>
        /// Stops enumerating once the predicate is satisfied
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <example>
        /// <para>
        /// Example from OracleDataSourceView2.aspx.
        /// </para>
        /// <code lang="c#">
        /// <![CDATA[
        /// bKeep = elem.ElementsBeforeSelf().Reverse()
        ///                    .TakeUntil(p => p.Name.LocalName == "if")
        ///                    .All(p => toRemove.Contains(p));
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> elements, Func<T, bool> predicate)
        {
            foreach (T element in elements)
            {
                if (predicate(element))
                {
                    yield return element;
                    yield break;
                }
                yield return element;
            }
        }
    }
}
