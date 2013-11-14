using System;

namespace DcmsMobile.PickWaves.Helpers
{
    /// <summary>
    /// Provides optimized display for date ranges
    /// </summary>
    public class DateRange
    {
        public DateTimeOffset? From { get; set; }

        public DateTimeOffset? To { get; set; }

        public bool ShowTime { get; set; }

        public override string ToString()
        {
            string shortFmt;
            string longFmt;

            if (ShowTime)
            {
                shortFmt = "{0:t}";  // time only
                longFmt = "{0:g}";   // date and time
            }
            else
            {
                shortFmt = "{0:M/d}";  // day month
                longFmt = "{0:d}";   // day/month/year
            }

            // Both Null
            if (From == null && To == null)
            {
                return null;
            }

            // Exactly one null
            if (From == null || To == null)
            {
                return string.Format(longFmt, From ?? To);
            }

            // Nothing null
            if (!ShowTime && From.Value.Date == To.Value.Date)
            {
                return string.Format(longFmt, From);
            }

            // Both dates different
            var fromString = string.Format(longFmt, From);

            var toString = string.Format(From.Value.Year == To.Value.Year ? shortFmt : longFmt, To);
            return fromString + " to " + toString;
        }

    }
}