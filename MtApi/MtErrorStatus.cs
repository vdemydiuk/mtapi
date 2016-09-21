using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtApi
{
    namespace MtNews.Model
    {
        public enum ErrorStatus
        {
            /// No error returned
            ERR_NO_ERROR = 0,

            /// No error returned, but the result is unknown
            ERR_NO_RESULT = 1,

            /// Common error
            ERR_COMMON_ERROR = 2,

            /// Invalid trade parameters
            ERR_INVALID_TRADE_PARAMETERS = 3,

            /// Trade server is busy
            ERR_SERVER_BUSY = 4,

            /// Old version of the client terminal
            ERR_OLD_VERSION = 5,

            /// No connection with trade server
            ERR_NO_CONNECTION = 6,

            /// Not enough rights
            ERR_NOT_ENOUGH_RIGHTS = 7,

            /// Too frequent requests
            ERR_TOO_FREQUENT_REQUESTS = 8,

            /// Malfunctional trade operation
            ERR_MALFUNCTIONAL_TRADE = 9,

            /// Account disabled
            ERR_ACCOUNT_DISABLED = 64,

            /// Invalid account
            ERR_INVALID_ACCOUNT = 65,

            /// Trade timeout
            ERR_TRADE_TIMEOUT = 128,

            /// Invalid price
            ERR_INVALID_PRICE = 129,

            /// Invalid stops
            ERR_INVALID_STOPS = 130,

            /// Invalid trade volume
            ERR_INVALID_TRADE_VOLUME = 131,

            /// Market is closed
            ERR_MARKET_CLOSED = 132,

            /// Trade is disabled
            ERR_TRADE_DISABLED = 133,

            /// Not enough money
            ERR_NOT_ENOUGH_MONEY = 134,

            /// Price changed
            ERR_PRICE_CHANGED = 135,

            /// Off quotes
            ERR_OFF_QUOTES = 136,

            /// Broker is busy
            ERR_BROKER_BUSY = 137,

            /// Requote
            ERR_REQUOTE = 138,

            /// Order is locked
            ERR_ORDER_LOCKED = 139,

            /// Buy orders only allowed
            ERR_LONG_POSITIONS_ONLY_ALLOWED = 140,

            /// Too many requests
            ERR_TOO_MANY_REQUESTS = 141,

            /// Modification denied because order is too close to market
            ERR_TRADE_MODIFY_DENIED = 145,

            /// Trade context is busy
            ERR_TRADE_CONTEXT_BUSY = 146,

            /// Expirations are denied by broker
            ERR_TRADE_EXPIRATION_DENIED = 147,

            /// The amount of open and pending orders has reached the limit set by the broker
            ERR_TRADE_TOO_MANY_ORDERS = 148,

            /// An attempt to open an order opposite to the existing one when hedging is disabled
            ERR_TRADE_HEDGE_PROHIBITED = 149,

            /// An attempt to close an order contravening the FIFO rule
            ERR_TRADE_PROHIBITED_BY_FIFO = 150,

            /// No error returned
            ERR_NO_MQLERROR = 4000,

            /// Wrong function pointer
            ERR_WRONG_FUNCTION_POINTER = 4001,

            /// Array index is out of range
            ERR_ARRAY_INDEX_OUT_OF_RANGE = 4002,

            /// No memory for function call stack
            ERR_NO_MEMORY_FOR_CALL_STACK = 4003,

            /// Recursive stack overflow
            ERR_RECURSIVE_STACK_OVERFLOW = 4004,

            /// Not enough stack for parameter
            ERR_NOT_ENOUGH_STACK_FOR_PARAM = 4005,

            /// No memory for parameter string
            ERR_NO_MEMORY_FOR_PARAM_STRING = 4006,

            /// No memory for temp string
            ERR_NO_MEMORY_FOR_TEMP_STRING = 4007,

            /// Not initialized string
            ERR_NOT_INITIALIZED_STRING = 4008,

            /// Not initialized string in array
            ERR_NOT_INITIALIZED_ARRAYSTRING = 4009,

            /// No memory for array string
            ERR_NO_MEMORY_FOR_ARRAYSTRING = 4010,

            /// Too long string
            ERR_TOO_LONG_STRING = 4011,

            /// Remainder from zero divide
            ERR_REMAINDER_FROM_ZERO_DIVIDE = 4012,

            /// Zero divide
            ERR_ZERO_DIVIDE = 4013,

            /// Unknown command
            ERR_UNKNOWN_COMMAND = 4014,

            /// Wrong jump (never generated error)
            ERR_WRONG_JUMP = 4015,

            /// Not initialized array
            ERR_NOT_INITIALIZED_ARRAY = 4016,

            /// DLL calls are not allowed
            ERR_DLL_CALLS_NOT_ALLOWED = 4017,

            /// Cannot load library
            ERR_CANNOT_LOAD_LIBRARY = 4018,

            /// Cannot call function
            ERR_CANNOT_CALL_FUNCTION = 4019,

            /// Expert function calls are not allowed
            ERR_EXTERNAL_CALLS_NOT_ALLOWED = 4020,

            /// Not enough memory for temp string returned from function
            ERR_NO_MEMORY_FOR_RETURNED_STR = 4021,

            /// System is busy (never generated error)
            ERR_SYSTEM_BUSY = 4022,

            /// DLL-function call critical error
            ERR_DLLFUNC_CRITICALERROR = 4023,

            /// Internal error
            ERR_INTERNAL_ERROR = 4024,

            /// Out of memory
            ERR_OUT_OF_MEMORY = 4025,

            /// Invalid pointer
            ERR_INVALID_POINTER = 4026,

            /// Too many formatters in the format function
            ERR_FORMAT_TOO_MANY_FORMATTERS = 4027,

            /// Parameters count exceeds formatters count
            ERR_FORMAT_TOO_MANY_PARAMETERS = 4028,

            /// Invalid array
            ERR_ARRAY_INVALID = 4029,

            /// No reply from chart
            ERR_CHART_NOREPLY = 4030,

            /// Invalid function parameters count
            ERR_INVALID_FUNCTION_PARAMSCNT = 4050,

            /// Invalid function parameter value
            ERR_INVALID_FUNCTION_PARAMVALUE = 4051,

            /// String function internal error
            ERR_STRING_FUNCTION_INTERNAL = 4052,

            /// Some array error
            ERR_SOME_ARRAY_ERROR = 4053,

            /// Incorrect series array using
            ERR_INCORRECT_SERIESARRAY_USING = 4054,

            /// Custom indicator error
            ERR_CUSTOM_INDICATOR_ERROR = 4055,

            /// Arrays are incompatible
            ERR_INCOMPATIBLE_ARRAYS = 4056,

            /// Global variables processing error
            ERR_GLOBAL_VARIABLES_PROCESSING = 4057,

            /// Global variable not found
            ERR_GLOBAL_VARIABLE_NOT_FOUND = 4058,

            /// Function is not allowed in testing mode
            ERR_FUNC_NOT_ALLOWED_IN_TESTING = 4059,

            /// Function is not allowed for call
            ERR_FUNCTION_NOT_CONFIRMED = 4060,

            /// Send mail error
            ERR_SEND_MAIL_ERROR = 4061,

            /// String parameter expected
            ERR_STRING_PARAMETER_EXPECTED = 4062,

            /// Integer parameter expected
            ERR_INTEGER_PARAMETER_EXPECTED = 4063,

            /// Double parameter expected
            ERR_DOUBLE_PARAMETER_EXPECTED = 4064,

            /// Array as parameter expected
            ERR_ARRAY_AS_PARAMETER_EXPECTED = 4065,

            /// Requested history data is in updating state
            ERR_HISTORY_WILL_UPDATED = 4066,

            /// Internal trade error
            ERR_TRADE_ERROR = 4067,

            /// Resource not found
            ERR_RESOURCE_NOT_FOUND = 4068,

            /// Resource not supported
            ERR_RESOURCE_NOT_SUPPORTED = 4069,

            /// Duplicate resource
            ERR_RESOURCE_DUPLICATED = 4070,

            /// Custom indicator cannot initialize
            ERR_INDICATOR_CANNOT_INIT = 4071,

            /// Cannot load custom indicator
            ERR_INDICATOR_CANNOT_LOAD = 4072,

            /// No history data
            ERR_NO_HISTORY_DATA = 4073,

            /// No memory for history data
            ERR_NO_MEMORY_FOR_HISTORY = 4074,

            /// Not enough memory for indicator calculation
            ERR_NO_MEMORY_FOR_INDICATOR = 4075,

            /// End of file
            ERR_END_OF_FILE = 4099,

            /// Some file error
            ERR_SOME_FILE_ERROR = 4100,

            /// Wrong file name
            ERR_WRONG_FILE_NAME = 4101,

            /// Too many opened files
            ERR_TOO_MANY_OPENED_FILES = 4102,

            /// Cannot open file
            ERR_CANNOT_OPEN_FILE = 4103,

            /// Incompatible access to a file
            ERR_INCOMPATIBLE_FILEACCESS = 4104,

            /// No order selected
            ERR_NO_ORDER_SELECTED = 4105,

            /// Unknown symbol
            ERR_UNKNOWN_SYMBOL = 4106,

            /// Invalid price
            ERR_INVALID_PRICE_PARAM = 4107,

            /// Invalid ticket
            ERR_INVALID_TICKET = 4108,

            /// Trade is not allowed. Enable checkbox "Allow live trading" in the Expert Advisor properties
            ERR_TRADE_NOT_ALLOWED = 4109,

            /// Longs are not allowed. Check the Expert Advisor properties
            ERR_LONGS_NOT_ALLOWED = 4110,

            /// Shorts are not allowed. Check the Expert Advisor properties
            ERR_SHORTS_NOT_ALLOWED = 4111,

            /// Automated trading by Expert Advisors/Scripts disabled by trade server
            ERR_TRADE_EXPERT_DISABLED_BY_SERVER = 4112,

            /// Object already exists
            ERR_OBJECT_ALREADY_EXISTS = 4200,

            /// Unknown object property
            ERR_UNKNOWN_OBJECT_PROPERTY = 4201,

            /// Object does not exist
            ERR_OBJECT_DOES_NOT_EXIST = 4202,

            /// Unknown object type
            ERR_UNKNOWN_OBJECT_TYPE = 4203,

            /// No object name
            ERR_NO_OBJECT_NAME = 4204,

            /// Object coordinates error
            ERR_OBJECT_COORDINATES_ERROR = 4205,

            /// No specified subwindow
            ERR_NO_SPECIFIED_SUBWINDOW = 4206,

            /// Graphical object error
            ERR_SOME_OBJECT_ERROR = 4207,

            /// Unknown chart property
            ERR_CHART_PROP_INVALID = 4210,

            /// Chart not found
            ERR_CHART_NOT_FOUND = 4211,

            /// Chart subwindow not found
            ERR_CHARTWINDOW_NOT_FOUND = 4212,

            /// Chart indicator not found
            ERR_CHARTINDICATOR_NOT_FOUND = 4213,

            /// Symbol select error
            ERR_SYMBOL_SELECT = 4220,

            /// Notification error
            ERR_NOTIFICATION_ERROR = 4250,

            /// Notification parameter error
            ERR_NOTIFICATION_PARAMETER = 4251,

            /// Notifications disabled
            ERR_NOTIFICATION_SETTINGS = 4252,

            /// Notification send too frequent
            ERR_NOTIFICATION_TOO_FREQUENT = 4253,

            /// FTP server is not specified
            ERR_FTP_NOSERVER = 4260,

            /// FTP login is not specified
            ERR_FTP_NOLOGIN = 4261,

            /// FTP connection failed
            ERR_FTP_CONNECT_FAILED = 4262,

            /// FTP connection closed
            ERR_FTP_CLOSED = 4263,

            /// FTP path not found on server
            ERR_FTP_CHANGEDIR = 4264,

            /// File not found in the MQL4\Files directory to send on FTP server
            ERR_FTP_FILE_ERROR = 4265,

            /// Common error during FTP data transmission
            ERR_FTP_ERROR = 4266,

            /// Too many opened files
            ERR_FILE_TOO_MANY_OPENED = 5001,

            /// Wrong file name
            ERR_FILE_WRONG_FILENAME = 5002,

            /// Too long file name
            ERR_FILE_TOO_LONG_FILENAME = 5003,

            /// Cannot open file
            ERR_FILE_CANNOT_OPEN = 5004,

            /// Text file buffer allocation error
            ERR_FILE_BUFFER_ALLOCATION_ERROR = 5005,

            /// Cannot delete file
            ERR_FILE_CANNOT_DELETE = 5006,

            /// Invalid file handle (file closed or was not opened)
            ERR_FILE_INVALID_HANDLE = 5007,

            /// Wrong file handle (handle index is out of handle table)
            ERR_FILE_WRONG_HANDLE = 5008,

            /// File must be opened with FILE_WRITE flag
            ERR_FILE_NOT_TOWRITE = 5009,

            /// File must be opened with FILE_READ flag
            ERR_FILE_NOT_TOREAD = 5010,

            /// File must be opened with FILE_BIN flag
            ERR_FILE_NOT_BIN = 5011,

            /// File must be opened with FILE_TXT flag
            ERR_FILE_NOT_TXT = 5012,

            /// File must be opened with FILE_TXT or FILE_CSV flag
            ERR_FILE_NOT_TXTORCSV = 5013,

            /// File must be opened with FILE_CSV flag
            ERR_FILE_NOT_CSV = 5014,

            /// File read error
            ERR_FILE_READ_ERROR = 5015,

            /// File write error
            ERR_FILE_WRITE_ERROR = 5016,

            /// String size must be specified for binary file
            ERR_FILE_BIN_STRINGSIZE = 5017,

            /// Incompatible file (for string arrays-TXT, for others-BIN)
            ERR_FILE_INCOMPATIBLE = 5018,

            /// File is directory not file
            ERR_FILE_IS_DIRECTORY = 5019,

            /// File does not exist
            ERR_FILE_NOT_EXIST = 5020,

            /// File cannot be rewritten
            ERR_FILE_CANNOT_REWRITE = 5021,

            /// Wrong directory name
            ERR_FILE_WRONG_DIRECTORYNAME = 5022,

            /// Directory does not exist
            ERR_FILE_DIRECTORY_NOT_EXIST = 5023,

            /// Specified file is not directory
            ERR_FILE_NOT_DIRECTORY = 5024,

            /// Cannot delete directory
            ERR_FILE_CANNOT_DELETE_DIRECTORY = 5025,

            /// Cannot clean directory
            ERR_FILE_CANNOT_CLEAN_DIRECTORY = 5026,

            /// Array resize error
            ERR_FILE_ARRAYRESIZE_ERROR = 5027,

            /// String resize error
            ERR_FILE_STRINGRESIZE_ERROR = 5028,

            /// Structure contains strings or dynamic arrays
            ERR_FILE_STRUCT_WITH_OBJECTS = 5029,

            /// Invalid URL
            ERR_WEBREQUEST_INVALID_ADDRESS = 5200,

            /// Failed to connect to specified URL
            ERR_WEBREQUEST_CONNECT_FAILED = 5201,

            /// Timeout exceeded
            ERR_WEBREQUEST_TIMEOUT = 5202,

            /// HTTP request failed
            ERR_WEBREQUEST_REQUEST_FAILED = 5203,

            /// User defined errors start with this code
            ERR_USER_ERROR_FIRST = 65536
        }
    }
}
