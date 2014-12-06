using System;
using System.Web.Mvc;

namespace DcmsMobile.Shipping.Helpers
{
    /// <summary>
    /// Expects a UNIX timestamp and returns a DateTimeOffset object
    /// </summary>
    /// <remarks>
    /// Reference: http://stackoverflow.com/questions/249760/how-to-convert-unix-timestamp-to-datetime-and-vice-versa
    /// </remarks>
    public class UnixTimestampBinder:IModelBinder
    {
        private static readonly DateTime __baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var val = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (val == null || string.IsNullOrWhiteSpace(val.AttemptedValue)) {
                return null;
            }
            var seconds = long.Parse(val.AttemptedValue);

            //var baseDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var date = GetDateFromUnixTimeStamp(seconds);        //__baseDate.AddSeconds(seconds).ToLocalTime();
            if ((Nullable.GetUnderlyingType(bindingContext.ModelType) ?? bindingContext.ModelType) == typeof(DateTimeOffset))
            {
                return new DateTimeOffset(date);
            }
            else
            {
                throw new NotImplementedException();
            }
            
        }

        /// <summary>
        /// Utility function to convert passed date to a UNIX timestamp
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long GetUnixTimeStamp(DateTime date)
        {
            //return (int)Math.Round((date - __baseDate).TotalSeconds);
            return (long)(date - __baseDate.ToLocalTime()).TotalSeconds;
        }

        public static DateTime GetDateFromUnixTimeStamp(long timestamp)
        {
            //return (int)Math.Round((date - __baseDate).TotalSeconds);
            var x = __baseDate.AddSeconds(timestamp).ToLocalTime();
            return x;
        }
    }
}