// ReSharper disable InconsistentNaming
namespace MtApi5
{
    // Country description as returned by the MT5 economic calendar
    // (native MqlCalendarCountry). See CalendarCountries / CalendarCountryById.
    public class MqlCalendarCountry
    {
        public long id { get; set; }               // Country id (ISO 3166-1)
        public string name { get; set; } = string.Empty;            // Country text name (in the current terminal encoding)
        public string code { get; set; } = string.Empty;            // Country code name (ISO 3166-1 alpha-2)
        public string currency { get; set; } = string.Empty;        // Country currency code
        public string currency_symbol { get; set; } = string.Empty; // Country currency symbol
        public string url_name { get; set; } = string.Empty;        // Country name used in the mql5.com website URL
    }
}
