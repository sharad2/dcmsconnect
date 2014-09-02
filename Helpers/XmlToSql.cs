using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using EclipseLibrary.Oracle.Extensions;
using EclipseLibrary.Oracle.Helpers.XPath;
using Oracle.ManagedDataAccess.Client;

namespace EclipseLibrary.Oracle.Helpers
{

    /// <summary>
    /// Provides the functionality to prune an SQL query by interpreting the embedded XML tags
    /// </summary>
    /// <remarks>
    /// <para>
    /// By embedding XML within the SQL statement, you are able to prune the query based on parameter values. Additionally, Oracle
    /// does not tolerate parameters which are not used within a query. The function BuildCommand()
    /// provides both these functionalities. It parses the XML based on the value of parameters in the passed command and then removes all parameters
    /// from cmd which are not used within the resulting query.
    /// </para>
    /// <para>
    /// Sharad 28 Sep 2012: This class is public because it is used by EclipseLibrary.WebForms
    /// </para>
    /// </remarks>
    public static class XmlToSql
    {
        private static readonly Regex _regexParameters = new Regex(@":(?<paramName>\w+)",
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Converts the XML provided in <c>cmd.CommandText</c> to a proper SQL statement
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="paramUpdater">Populates the parameter properties corresponding to the passed parameter, or null if <paramref name="cmd"/> already contains all parameters</param>
        /// <param name="customTagResolver">If an unrecognized xml tag is encountered, this lambda will be called.</param>
        /// <remarks>
        /// <para>
        /// If you have already added the parameters to the passed command, then you do not need to pass <paramref name="paramUpdater"/>.
        /// Otherwise it is called for each parameter referenced in the SQL text or in the XML condition.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static void BuildCommand(OracleCommand cmd, Func<string, OracleParameter> paramUpdater = null, Func<XElement, bool> customTagResolver = null)
        {
            XElement elemSql;
            try
            {
                elemSql = XElement.Parse("<root>" + cmd.CommandText + "</root>", LoadOptions.PreserveWhitespace);
            }
            catch (XmlException ex)
            {
                var tokens = cmd.CommandText.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
                string msg;
                if (ex.LineNumber >= 0 && ex.LineNumber < tokens.Length)
                {
                    msg = tokens[ex.LineNumber - 1];
                }
                else
                {
                    msg = string.Format("Line Number: {0}; LinePosition: {1}", ex.LineNumber, ex.LinePosition);
                }
                throw new ArgumentException(msg, "cmd.CommandText", ex);
            }
            var nav = new XPathEvaluator(p =>
            {
                OracleParameter param;
                if (cmd.Parameters.Contains(p))
                {
                    param = cmd.Parameters[p];
                }
                else
                {
                    param = paramUpdater(p);
                    cmd.Parameters.Add(param);
                }
                return param.Value;
            });
            var toRemove = new HashSet<XElement>();
            foreach (var elem in elemSql.Descendants())
            {
                Debug.Assert(elem != null, "elem != null");
                if (toRemove.Contains(elem.Parent))
                {
                    // Optimization.
                    // Do nothing because the parent has been removed and so this element is irrelevant
                    continue;
                }

                bool bKeep;
                switch (elem.Name.LocalName)
                {
                    case "if":
                        bKeep = EvaluateCondition(nav, elem);
                        break;

                    case "a":
                        //<a pre="(", sep=" OR ", post=")">mri.ia_id = :ia_id</a>
                        var matches = _regexParameters.Matches(elem.Value);
                        // Exception here if multiple parameters are found
                        string varName = matches.Cast<Match>().Select(p => p.Groups["paramName"].Value)
                                .Distinct().Single();
                        string condition = string.Format("${0}", varName);
                        bKeep = nav.Matches(condition);
                        if (bKeep)
                        {
                            bKeep = HandleArrayParamterXml(elem, varName, cmd);
                        }
                        break;

                    case "else":
                        // Keep the else only if all the previous else and if have been removed
                        bKeep = elem.ElementsBeforeSelf().Reverse()
                            .TakeUntil(p => p.Name.LocalName == "if")
                            .All(toRemove.Contains);
                        if (bKeep)
                        {
                            XAttribute attr = elem.Attribute("c");
                            if (attr != null)
                            {
                                bKeep = EvaluateCondition(nav, elem);
                            }
                        }
                        break;

                    default:
                        if (customTagResolver == null)
                        {
                            throw new NotSupportedException(elem.Name.LocalName);
                        }
                        bKeep = customTagResolver(elem);
                        break;
                }
                if (!bKeep)
                {
                    toRemove.Add(elem);
                }
            }

            toRemove.Remove();
            cmd.CommandText = elemSql.Value;
            string[] paramsUsed = _regexParameters.Matches(cmd.CommandText).Cast<Match>()
                .Select(p => p.Groups["paramName"].Value)
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToArray();

            // Remove excess parameters. Note that we are performing case insensitive comparisons
            string[] paramsUnused = cmd.Parameters.Cast<OracleParameter>()
                .Select(p => p.ParameterName)
                .Except(paramsUsed, StringComparer.InvariantCultureIgnoreCase).ToArray();
            foreach (string name in paramsUnused)
            {
                cmd.Parameters.Remove(cmd.Parameters[name]);
            }

            // Create remaining used parameters
            string[] paramsUncreated = paramsUsed.Except(cmd.Parameters.Cast<OracleParameter>().Select(p => p.ParameterName), StringComparer.InvariantCultureIgnoreCase).ToArray();
            foreach (string name in paramsUncreated)
            {
                cmd.Parameters.Add(paramUpdater(name));
            }
            return;
        }

        private static bool EvaluateCondition(XPathEvaluator nav, XElement elem)
        {
            XAttribute attr = elem.Attribute("c");
            string condition;
            if (attr == null)
            {
                // Build the condition. This condition evaluates to true if all parameters found within the clause
                // are not null
                string[] paramsUsed = _regexParameters.Matches(elem.Value).Cast<Match>()
                    .Select(p => "$" + p.Groups["paramName"].Value)
                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                    .ToArray();
                if (paramsUsed.Length == 0)
                {
                    string msg = string.Format("Cannot infer condition. No parameter within {0}", elem.Value);
                    throw new Exception(msg);
                }
                condition = string.Join(" and ", paramsUsed);
            }
            else
            {
                condition = attr.Value;
            }
            bool bMatch = nav.Matches(condition);
            return bMatch;
        }

        private static bool HandleArrayParamterXml(XElement elem, string paramName, DbCommand cmd)
        {
            //<a pre="(", sep=" OR ", post=")">mri.ia_id = :ia_id</a>
            // Scanner returns \r so included \r also. \n is for new line
            string[] arrayValues = cmd.Parameters[paramName].Value.ToString().Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (arrayValues.Length == 0)
            {
                return false;
            }

            var arrayParams = new List<string>();
            for (int index = 0; index < arrayValues.Length; ++index)
            {
                DbParameter p = cmd.CreateParameter();
                p.ParameterName = string.Format("{0}{1}", paramName, index);
                arrayParams.Add(elem.Value.Replace(":" + paramName, ":" + p.ParameterName));
                p.Value = arrayValues[index].Trim();
                cmd.Parameters.Add(p);
            }
            XAttribute attr = elem.Attribute("pre");
            string replacement = attr == null ? string.Empty : attr.Value;
            attr = elem.Attribute("sep");
            replacement += string.Join(attr == null ? string.Empty : attr.Value, arrayParams);
            attr = elem.Attribute("post");
            if (attr != null)
            {
                replacement += attr.Value;
            }
            elem.Value = replacement;
            return true;
        }
    }
}
