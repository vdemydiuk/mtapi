namespace MtApi
{
    // https://docs.mql4.com/constants/environment_state/terminalstatus#enum_terminal_info_integer
    public enum EnumTerminalInfoInteger
    {
        TERMINAL_BUILD                  = 5, // The client terminal build number
        TERMINAL_COMMUNITY_ACCOUNT      = 23, // The flag indicates the presence of MQL5.community authorization data in the terminal
        TERMINAL_COMMUNITY_CONNECTION   = 24, // Connection to MQL5.community
        TERMINAL_CONNECTED              = 6, // Connection to a trade server
        TERMINAL_DLLS_ALLOWED           = 7, // Permission to use DLL
        TERMINAL_TRADE_ALLOWED          = 8, // Permission to trade
        TERMINAL_EMAIL_ENABLED          = 9, // Permission to send e-mails using SMTP-server and login, specified in the terminal settings
        TERMINAL_FTP_ENABLED            = 10, // Permission to send reports using FTP-server and login, specified in the terminal settings
        TERMINAL_NOTIFICATIONS_ENABLED  = 26, // Permission to send notifications to smartphone
        TERMINAL_MAXBARS                = 11, // The maximal bars count on the chart
        TERMINAL_MQID                   = 22, // The flag indicates the presence of MetaQuotes ID data to send Push notifications
        TERMINAL_CODEPAGE               = 12, // Number of the code page of the language installed in the client terminal
        TERMINAL_CPU_CORES              = 21, // The number of CPU cores in the system
        TERMINAL_DISK_SPACE             = 20, // Free disk space for the MQL4\Files folder of the terminal, Mb
        TERMINAL_MEMORY_PHYSICAL        = 14, // Physical memory in the system, Mb
        TERMINAL_MEMORY_TOTAL           = 15, // Memory available to the process of the terminal , Mb
        TERMINAL_MEMORY_AVAILABLE       = 16, // Free memory of the terminal process, Mb
        TERMINAL_MEMORY_USED            = 17, // Memory used by the terminal , Mb
        TERMINAL_SCREEN_DPI             = 27, // The resolution of information display on the screen is measured as number of Dots in a line per Inch (DPI). Knowing the parameter value, you can set the size of graphical objects so that they look the same on monitors with different resolution characteristics.
        TERMINAL_PING_LAST              = 28  // The last known value of a ping to a trade server in microseconds. One second comprises of one million microseconds
    }
}