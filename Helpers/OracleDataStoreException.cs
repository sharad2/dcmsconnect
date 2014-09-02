using System.Data.Common;
using System.Linq;
using System.Text;
using System.Web.Management;
using Oracle.ManagedDataAccess.Client;

namespace EclipseLibrary.Oracle.Helpers
{
    /// <summary>
    /// Dispays more useful diagnostics when oracle error occurs, but only in DEBUG mode
    /// </summary>
    /// <remarks>
    /// The contents of OracleErrorCollection are displayed. Raises <see cref="OracleDataStoreErrorEvent"/> so that it can be logged even if this exception
    /// is caught and handled
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class OracleDataStoreException : DbException
    {
        private readonly string _message;
        public OracleDataStoreException(OracleException ex, OracleCommand cmd)
            : base("", ex)
        {
            StringBuilder sb = new StringBuilder();
            if (ex.Number == 24381)
            {
                // error(s) in array DML
                // See http://docs.oracle.com/html/B14164_01/featOraCommand.htm
                foreach (var error in ex.Errors.OfType<OracleError>())
                {
                    sb.AppendFormat("Array Bind Error {0} occured at Row Number {1}", error.Message, error.ArrayBindIndex);
                    sb.AppendLine();
                }
            }
            else
            {
                foreach (var error in ex.Errors.OfType<OracleError>())
                {
                    sb.AppendLine(error.Message);
                }
            }
#if DEBUG
            if (cmd != null)
            {
                sb.AppendLine("Additional Debug Mode Information");
                sb.AppendLine(cmd.CommandText);
                foreach (OracleParameter param in cmd.Parameters)
                {
                    sb.AppendFormat("{0} ({1} {2}: *{3}*)", param.ParameterName, param.Direction, param.OracleDbType, param.Value);
                    sb.AppendLine();
                }
            }
#endif

            _message = sb.ToString();

            var ev = new OracleDataStoreErrorEvent("OracleDataStoreException is being raised", cmd, this);
            ev.Raise();
        }

        public override string Message
        {
            get
            {
                return _message;
            }
        }

        public int OracleErrorNumber
        {
            get
            {
                return ((OracleException)this.InnerException).Number;
            }
        }
    }

    internal class OracleDataStoreErrorEvent: WebErrorEvent
    {
        private StringBuilder _sb;
        public OracleDataStoreErrorEvent(string msg, OracleCommand cmd, OracleDataStoreException ex): base(msg, ex, WebEventCodes.WebExtendedBase + 1, ex)
        {
            _sb = new StringBuilder();
            if (cmd != null)
            {
                if (cmd.Connection != null)
                {
                    _sb.AppendFormat("Connection DataSource {0}; Host Name: {1}; State: {2}", cmd.Connection.DataSource, cmd.Connection.HostName, cmd.Connection.State);
                }
                _sb.AppendLine(cmd.CommandText);
                foreach (OracleParameter param in cmd.Parameters)
                {
                    _sb.AppendFormat("{0} ({1} {2}: *{3}*)", param.ParameterName, param.Direction, param.OracleDbType, param.Value);
                    _sb.AppendLine();
                }
            }
        }

        public override void Raise()
        {
            _sb.AppendFormat("Event Raised at {0}", EventTime);
            base.Raise();
        }

        public override void FormatCustomEventDetails(WebEventFormatter formatter)
        {
            formatter.AppendLine(_sb.ToString());
            base.FormatCustomEventDetails(formatter);
        }
    }
}
