using System;
using System.Collections.Generic;
using System.Xml.Xsl;

namespace EclipseLibrary.Oracle.Helpers.XPath
{
    /// <summary>
    /// Implements the ResolveVariable() function which simply returns an IXsltContextVariable corresponding
    /// to the passed variable name. We also represent an empty variable since that is such a common case.
    /// If the value in null, we return our own reference.
    /// </summary>
    internal class ParseDataContext:XsltContext
    {
        /// <summary>
        /// A dictionary of variable values
        /// </summary>
        public Func<string, object> ValueCallBack { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix">Not used</param>
        /// <param name="name">The name of the variable to lookup</param>
        /// <returns>An interface representing the variable</returns>
        /// <exception cref="KeyNotFoundException">The variable was not found in the dictionary
        /// and missing values are not allowed.
        /// </exception>
        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            //object obj = this.ValueCallBack(name);
            //if (obj == null || obj == DBNull.Value)
            //{
            //    // Common case. Save the trouble of instantiating a new class
            //    return this;
            //}
            ParseContextVariable var;
            var.Value = this.ValueCallBack(name);
            return var;
        }

        #region Unused functionality
        public override int CompareDocument(string baseUri, string nextbaseUri)
        {
            throw new NotImplementedException();
        }

        public override bool PreserveWhitespace(System.Xml.XPath.XPathNavigator node)
        {
            throw new NotImplementedException();
        }

        public override IXsltContextFunction ResolveFunction(string prefix, string name, System.Xml.XPath.XPathResultType[] argTypes)
        {
            throw new NotImplementedException();
        }

        public override bool Whitespace
        {
            get { throw new NotImplementedException(); }
        }
        #endregion


        //#region IXsltContextVariable Members

        //public object Evaluate(XsltContext xsltContext)
        //{
        //    return null;
        //}

        //public bool IsLocal
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public bool IsParam
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public System.Xml.XPath.XPathResultType VariableType
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //#endregion
    }
}
