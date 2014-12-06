using System;

namespace EclipseLibrary.Oracle.Helpers.XPath
{
    /// <summary>
    /// Provides the ability to conditionally decide which format string to use
    /// </summary>
    /// <remarks>
    /// <para>
    /// You supply a pair of condition and format strings. Each condition is evaluated and the format string
    /// corresponding to the first condition which evaluates to <c>true</c> is used. The general syntax is
    /// <c>{0::cond1:fmt1:cond2:fmt2:fmtdefault</c>. If <c>cond1</c> evaluates to true, format string <c>fmt1</c>
    /// is used. If none of the conditions evaluate to true, then the last format string is used, which in this case is
    /// <c>fmtdefault</c>.
    /// </para>
    /// <para>
    /// For formattable values, such as int, decimal, DateTime, etc., the selected format string is passed to the
    /// <c>ToString()</c> function. For all other values, the format string is used as is. If the format string contains
    /// the <c>~</c> character, it is replaced with the string version of the argument.
    /// </para>
    /// <para>
    /// Consider the example <c>{0::$amount &gt; 1000:<strong>#</strong>:$amount &lt;= 1000:#}</c>.
    /// If <c>amount</c> is greater than 1000, then this format string is equivalent to
    /// <c>{0:<strong>#</strong>}</c>. If <c>amount</c> is 1000 or less, the format string is equivalent to
    /// <c>{0:#}</c>. The conditions are evaluated in order so the first condition which evaluates
    /// to true wins. The net result is that amounts greater than 1000 are displayed as bold.
    /// </para>
    /// <para>
    /// If none of the conditions match, then the last format string is used. The above example can be more
    /// succinctly and eqivalently written as <c>{0::$amount &gt; 1000:<b>#</b>:#}</c>
    /// </para>
    /// <para>
    /// String arguments permit the special character ~ (tilde) which is replaced with the value of the string.
    /// </para>
    /// <para>
    /// Sharad 28 Sep 2012: This class is public because it is used by EclipseLibrary.WebForms
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// Create a conditional formatter by supplying it with a lambda expression which is responsible for returning values
    /// of variables used in the XPath expressions.
    /// </para>
    /// <code>
    /// <![CDATA[
    ///var dict = new Dictionary<string,object> {
    ///    {"IsDeduction", true},
    ///    {"picking_status", "Good"}
    ///};
    ///var formatter = new ConditionalFormatter(p => dict[p]);
    /// ]]>
    /// </code>
    /// <para>
    /// String examples
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Format String</term>
    /// <description>Result</description>
    /// </listheader>
    /// <item>
    /// <term>string.Format(formatter, "{0::not($picking_status):Unprocessed:~}", "Sharad")</term>
    /// <description>If <c>picking_status</c> is null, then result is <c>Unprocessed</c>,
    /// otherwise the result is <c>Sharad</c>
    /// </description>
    /// </item>
    /// <item>
    /// <term>string.Format(formatter, "{0::$picking_status = 'DONE':(~):~}", "Sharad")</term>
    /// <description>If <c>picking_status</c> is DONE, then result is <c>(Sharad)</c>,
    /// otherwise the result is <c>Sharad</c>
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// <c>Bool</c> examples.
    /// <list type="table">
    /// <listheader>
    /// <term>Format String</term>
    /// <description>Result</description>
    /// </listheader>
    /// <item>
    /// <term>string.Format(formatter, "{0::$IsDeduction:Deduction:Allowance}", true)</term>
    /// <description>If <c>IsDeduction</c> is true, then result is <c>Deduction</c>,
    /// otherwise the result is <c>Allowance</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term>string.Format(formatter, "{0::$IsDeduction:Deduction:Allowance}", false)</term>
    /// <description>If <c>IsDeduction</c> is true, then result is <c>Deduction</c>,
    /// otherwise the result is <c>Allowance</c>.
    /// </description>
    /// </item> 
    /// </list>
    /// </para>
    /// </example>
    public class ConditionalFormatter : XPathEvaluator, IFormatProvider, ICustomFormatter
    {
        public ConditionalFormatter(Func<string, object> valueCallBack)
            : base(valueCallBack)
        {
        }

        #region IFormatProvider Members

        /// <summary>
        /// Boilerplate implementation. Returns self if <paramref name="formatType"/> is <see cref="ICustomFormatter"/>
        /// </summary>
        /// <param name="formatType">The type of format needed</param>
        /// <returns>null or self</returns>
        /// <remarks>
        /// <para>
        /// </para>
        /// </remarks>
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
            {
                return this;
            }
            return null;
        }

        #endregion

        #region ICustomFormatter Members

        /// <summary>
        /// Actually performs the formatting by evaluating XPath conditions
        /// </summary>
        /// <param name="format">A format string containing formatting specifications.</param>
        /// <param name="arg">An object to format.</param>
        /// <param name="formatProvider">An object that supplies format information about the current instance.</param>
        /// <returns>The formatted string</returns>
        /// <remarks>
        /// <para>
        /// The syntax of a valid format string is <c>{n::condition-1:format-string1:condition-2:format-string2:...:format-string-default}</c>.
        /// The <c>::</c> after n triggers the condition evaluation process. The format string corresponding to the first condition which evaluates
        /// to true is chosen and then used for formatting the string. If none of the conditions evaluate to true, then <c>format-string-default</c>
        /// is used.
        /// </para>
        /// <para>
        /// If <paramref name="format"/> starts with a <c>:</c>, then it goes through the process of evaluating the embedded XPath
        /// expressions and choosing the actual format string to use. Once the actual format string is discovered, it provides the same behavior
        /// that string.Format() would have provided using that format string.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// Example format strings:
        /// </para>
        /// <list type="table">
        /// <listheader>
        /// <term>Sample Format String</term>
        /// <description>Result</description>
        /// </listheader>
        /// <item>
        /// <term>"{0:$IsGood:Nice Guy:Bad Fellow}"</term>
        /// <description>If <c>IsGood</c> is true then <c>Nice Guy</c> else <c>Bad Fellow</c></description>
        /// </item>
        /// <item>
        /// <term></term>
        /// <description></description>
        /// </item>
        /// <item>
        /// <term></term>
        /// <description></description>
        /// </item>
        /// <item>
        /// <term></term>
        /// <description></description>
        /// </item>
        /// </list>
        /// </example>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == null)
            {
                return String.Format("{0}", arg);
            }

            string ret;
            if (format.StartsWith(":"))
            {
                // Our specialized format string. Treat ~ as {0}
                string formatToUse = string.Empty;
                string[] tokens = format.Split(':');
                // Odd numbered tokens contain formula, even numbered tokens contain the format string to use
                int countTokenPairs = tokens.Length - ((tokens.Length - 1) % 2);
                for (int i = 1; i < countTokenPairs; i += 2)
                {
                    bool b = this.Matches(tokens[i]);
                    if (b)
                    {
                        formatToUse = tokens[i + 1];
                        break;
                    }
                }
                if (string.IsNullOrEmpty(formatToUse))
                {
                    // Use the last specified format
                    formatToUse = tokens[tokens.Length - 1];
                }

                IFormattable formattable = arg as IFormattable;
                if (formattable == null)
                {
                    // Special case for string argument.
                    string str = formatToUse.Replace("~", "{0}");
                    ret = string.Format(formatProvider, str, arg);
                    return ret;
                }
                return formattable.ToString(formatToUse, formatProvider);
            }

            // Normal format string. Provide default behavior
            if (arg == null)
            {
                ret = string.Empty;
            }
            else
            {
                IFormattable formattable = arg as IFormattable;
                ret = formattable == null ? arg.ToString() : formattable.ToString(format, formatProvider);
            }

            return ret;
        }

        #endregion
    }
}
