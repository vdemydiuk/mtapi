// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;

namespace MtApi5
{
    // Economic calendar event value (native MqlCalendarValue).
    // Scaled integer fields (*_value) hold the real value multiplied by 1,000,000;
    // the sentinel long.MinValue (LONG_MIN) means "no value".
    // See CalendarValueById / CalendarValueHistory* / CalendarValueLast*.
    public class MqlCalendarValue
    {
        private const long Empty = long.MinValue; // LONG_MIN sentinel = no value

        public long id { get; set; }                  // Value id
        public long event_id { get; set; }            // Id of the event this value relates to
        public long mt_time { get; set; }             // Event date/time (original MT time)
        public long mt_period { get; set; }           // Event reporting period (original MT time)
        public int revision { get; set; }             // Revision of the published indicator value for the period
        public long actual_value { get; set; }        // Actual value (x1e6, LONG_MIN if absent)
        public long prev_value { get; set; }          // Previous value (x1e6, LONG_MIN if absent)
        public long revised_prev_value { get; set; }  // Revised previous value (x1e6, LONG_MIN if absent)
        public long forecast_value { get; set; }      // Forecast value (x1e6, LONG_MIN if absent)
        public ENUM_CALENDAR_EVENT_IMPACT impact_type { get; set; } // Potential impact on the currency rate

        // Convenience accessors (not serialized).
        public DateTime time => Mt5TimeConverter.ConvertFromMtTime(mt_time);
        public DateTime period => Mt5TimeConverter.ConvertFromMtTime(mt_period);
        public double? ActualValue      => actual_value       == Empty ? (double?)null : actual_value       / 1e6;
        public double? PrevValue        => prev_value         == Empty ? (double?)null : prev_value         / 1e6;
        public double? RevisedPrevValue => revised_prev_value == Empty ? (double?)null : revised_prev_value / 1e6;
        public double? ForecastValue    => forecast_value     == Empty ? (double?)null : forecast_value     / 1e6;
    }

    // Wraps the in/out change_id together with the values array returned by
    // CalendarValueLast / CalendarValueLastByEvent.
    internal class CalendarValueLastResult
    {
        public long ChangeId { get; set; }
        public List<MqlCalendarValue> Values { get; set; } = new();
    }
}
