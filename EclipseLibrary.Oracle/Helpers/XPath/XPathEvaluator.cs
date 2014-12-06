using System;
using System.ComponentModel;
using System.Xml.XPath;

namespace EclipseLibrary.Oracle.Helpers.XPath
{
    /// <summary>
    /// Provides the ability to evaluate XPath expressions containing variables.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides a simple way to evaluate XPath expressions. The only useful public functions are <see cref="Evaluate"/>
    /// and  <see cref="Matches"/>. You can pass variable values in the constructor, or any time before evaluation using the
    /// <see cref="Callback"/> property.
    /// </para>
    /// <para>
    /// Each variable should be prefixed with a $ unless <see cref="VariablePrefix"/> has been specified.
    /// </para>
    /// <para>
    /// Refer to MSDN for full syntax of XPath expressions.
    /// </para>
    /// <para>
    /// Common boolean and comparison operators:
    /// <![CDATA[
    /// and or not() = != &lt; &lt;= &gt; &gt;=
    /// ]]>
    /// </para>
    /// <para>
    /// Common string functions
    /// <![CDATA[
    /// concat(string1, string2, ...) contains(str1, str2) starts-with(str1, str2) string-length(str)
    /// substring(str, startPos, [length]) substring-after(str1, str2) substring-before(str1, str2)
    /// ]]>
    /// </para>
    /// <para>
    /// This class is public because it is used by EclipseLibrary.WebForms
    /// </para>
    /// <seealso cref="ConditionalFormatter"/>
    /// </remarks>
    /// <example>
    /// <code lang="C#">
    /// <![CDATA[
    /// Dictionary<string, object> dict = new Dictionary<string, object>();
    /// dict.Add("Col1", 5);
    /// dict.Add("Col2", 6);
    /// dict.Add("Col3", 7);
    /// dict.Add("Col4", 8);
    /// ParseNavigator nav = new ParseNavigator(allValues);
    /// object obj = nav.Evaluate("$Col1 + $Col2");   // Result: 11
    /// obj = nav.Evaluate("$Col1 + $Col2 - ($Col3 + $Col4)");   // Result: -4
    /// object obj = nav.Evaluate("$Col6 + $Col8");   // Exception: No such variable
    /// ]]>
    /// </code>
    /// </example>
    public class XPathEvaluator : XPathNavigator
    {
        private readonly ParseDataContext _ctx;

        public XPathEvaluator()
        {
            _ctx = new ParseDataContext();
        }

        /// <summary>
        /// This constructor raises error if any variable referenced in an xpath expression is not found in the
        /// dictionary.
        /// </summary>
        /// <param name="valueCallback">Dictionary containing the name of the variable and its value</param>
        public XPathEvaluator(Func<string, object> valueCallback)
        {
            _ctx = new ParseDataContext {ValueCallBack = valueCallback};
        }


        /// <summary>
        /// A dictionary of variable values to be used during expression evaluation
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is often convenient to construct a single instance of <c>XPathEvaluator</c> and then evaluate
        /// expressions with different variable values.
        /// </para>
        /// </remarks>
        public Func<string, object> Callback
        {
            get
            {
                return _ctx.ValueCallBack;
            }
            set
            {
                _ctx.ValueCallBack = value;
            }
        }

        /// <summary>
        /// Normally each variable should be prefixed with $. If this is not the case, specify your prefix here.
        /// </summary>
        /// <remarks>
        /// As an example, OracleDataSource uses : as the prefix for each variable
        /// </remarks>
        [DefaultValue("$")]
        public string VariablePrefix { get; set; }

        public override object Evaluate(string xpath)
        {
            if (!string.IsNullOrEmpty(this.VariablePrefix))
            {
                xpath = xpath.Replace(":", "$");
            }
            XPathExpression expr = Compile(xpath);
            expr.SetContext(_ctx);
            return Evaluate(expr);
        }

        public override XPathNavigator Clone()
        {
            return this;
        }

        /// <summary>
        /// Same as <see cref="Evaluate"/> but the result returned is boolean
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public override bool Matches(string xpath)
        {
            if (!string.IsNullOrEmpty(this.VariablePrefix))
            {
                xpath = xpath.Replace(":", "$");
            }

            xpath = string.Format("boolean({0})", xpath);
            XPathExpression expr = Compile(xpath);
            expr.SetContext(_ctx);
            try
            {
                return (bool)Evaluate(expr);
            }
            catch (Exception ex)
            {
                throw new Exception(xpath, ex);
            }
        }

        #region Unused Functionality
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string BaseURI
        {
            get { throw new NotImplementedException(); }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool IsEmptyElement
        {
            get { throw new NotImplementedException(); }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool IsSamePosition(XPathNavigator other)
        {
            throw new NotImplementedException();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string LocalName
        {
            get { throw new NotImplementedException(); }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool MoveTo(XPathNavigator other)
        {
            throw new NotImplementedException();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool MoveToFirstAttribute()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <returns></returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool MoveToFirstChild()
        {
            throw new NotImplementedException("Did you forget to prefix your variable name?");
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            throw new NotImplementedException();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool MoveToId(string id)
        {
            throw new NotImplementedException();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool MoveToNext()
        {
            throw new NotImplementedException();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool MoveToNextAttribute()
        {
            throw new NotImplementedException();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            throw new NotImplementedException();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool MoveToParent()
        {
            throw new NotImplementedException();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool MoveToPrevious()
        {
            throw new NotImplementedException();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Xml.XmlNameTable NameTable
        {
            get { throw new NotImplementedException(); }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string NamespaceURI
        {
            get { throw new NotImplementedException(); }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override XPathNodeType NodeType
        {
            get { throw new NotImplementedException(); }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Prefix
        {
            get { throw new NotImplementedException(); }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Value
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
