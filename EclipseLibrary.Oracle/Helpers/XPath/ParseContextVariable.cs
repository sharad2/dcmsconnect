using System;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace EclipseLibrary.Oracle.Helpers.XPath
{
    internal struct ParseContextVariable:IXsltContextVariable
    {
        public object Value;

        #region IXsltContextVariable Members

        public object Evaluate(XsltContext xsltContext)
        {
            if (this.Value is DateTime)
            {
                return string.Format("{0:u}", this.Value);
            }
            return this.Value;
        }

        public bool IsLocal
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsParam
        {
            get { throw new NotImplementedException(); }
        }

        public XPathResultType VariableType
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
