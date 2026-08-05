// ReSharper disable InconsistentNaming
namespace MtApi5
{
    // Economic calendar event description (native MqlCalendarEvent).
    // See CalendarEventById / CalendarEventByCountry / CalendarEventByCurrency.
    public class MqlCalendarEvent
    {
        public long id { get; set; }                              // Event id
        public ENUM_CALENDAR_EVENT_TYPE type { get; set; }        // Event type
        public ENUM_CALENDAR_EVENT_SECTOR sector { get; set; }    // Sector the event relates to
        public ENUM_CALENDAR_EVENT_FREQUENCY frequency { get; set; } // Event frequency
        public ENUM_CALENDAR_EVENT_TIMEMODE time_mode { get; set; }  // Event time mode
        public long country_id { get; set; }                      // Country id the event relates to
        public ENUM_CALENDAR_EVENT_UNIT unit { get; set; }        // Measurement unit of the event value
        public ENUM_CALENDAR_EVENT_IMPORTANCE importance { get; set; } // Event importance
        public ENUM_CALENDAR_EVENT_MULTIPLIER multiplier { get; set; } // Event value multiplier
        public int digits { get; set; }                           // Number of decimal places for the value
        public string source_url { get; set; } = string.Empty;    // URL of the event value source
        public string event_code { get; set; } = string.Empty;    // Event code
        public string name { get; set; } = string.Empty;          // Event text name (in the current terminal encoding)
    }
}
