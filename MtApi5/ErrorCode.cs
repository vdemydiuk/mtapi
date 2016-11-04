namespace MtApi5
{
    //Error codes from https://www.mql5.com/en/docs/constants/errorswarnings/errorcodes

    public enum ErrorCode
    {
        ErrCustom                           = -1,   // Error occurred in MtApi5.
        ErrSuccess                          = 0,    // The operation completed successfully
        ErrInternalError                    = 4001, // Unexpected internal error
        ErrWrongInternalParameter           = 4002, // Wrong parameter in the inner call of the client terminal function
        ErrInvalidParameter                 = 4003, // Wrong parameter when calling the system function
        ErrNotEnoughMemory                  = 4004, // Not enough memory to perform the system function
        ErrStructWithobjectsOrclass         = 4005, // The structure contains objects of strings and/or dynamic arrays and/or structure of such objects and/or classes
        ErrInvalidArray                     = 4006, // Array of a wrong type, wrong size, or a damaged object of a dynamic array
        ErrArrayResizeError                 = 4007, // Not enough memory for the relocation of an array, or an attempt to change the size of a static array
        ErrStringResizeError                = 4008, // Not enough memory for the relocation of string
        ErrNotinitializedString             = 4009, // Not initialized string
        ErrInvalidDatetime                  = 4010, // Invalid date and/or time
        ErrArrayBadSize                     = 4011, // Requested array size exceeds 2 GB
        ErrInvalidPointer                   = 4012, // Wrong pointer
        ErrInvalidPointerType               = 4013, // Wrong type of pointer
        ErrFunctionNotAllowed               = 4014, // Function is not allowed for call
        ErrResourceNameDuplicated           = 4015, // The names of the dynamic and the static resource match
        ErrResourceNotFound                 = 4016, // Resource with this name has not been found in EX5
        ErrResourceUnsuppotedType           = 4017, // Unsupported resource type or its size exceeds 16 Mb
        ErrResourceNameIsTooLong            = 4018, // The resource name exceeds 63 characters

        ErrChartWrongId                     = 4101, // Wrong chart ID
        ErrChartNoReply                     = 4102, // Chart does not respond
        ErrChartNotFound                    = 4103, // Chart not found
        ErrChartNoExpert                    = 4104, // No Expert Advisor in the chart that could handle the event
        ErrChartCannotOpen                  = 4105, // Chart opening error
        ErrChartCannotChange                = 4106, // Failed to change chart symbol and period
        ErrChartWrongParameter              = 4107, // Error value of the parameter for the function of working with charts
        ErrChartCannotCreateTimer           = 4108, // Failed to create timer
        ErrChartWrongProperty               = 4109, // Wrong chart property ID
        ErrChartScreenshotFailed            = 4110, // Error creating screenshots
        ErrChartNavigateFailed              = 4111, // Error navigating through chart
        ErrChartTemplateFailed              = 4112, // Error applying template
        ErrChartWindowNotFound              = 4113, // Subwindow containing the indicator was not found
        ErrChartIndicatorCannotAdd          = 4114, // Error adding an indicator to chart
        ErrChartIndicatorCannotDel          = 4115, // Error deleting an indicator from the chart
        ErrChartIndicatorNotFound           = 4116, // Indicator not found on the specified chart

        ErrObjectError                      = 4201, // Error working with a graphical object
        ErrObjectNotFound                   = 4202, // Graphical object was not found
        ErrObjectWrongProperty              = 4203, // Wrong ID of a graphical object property
        ErrObjectGetdateFailed              = 4204, // Unable to get date corresponding to the value
        ErrObjectGetvalueFailed             = 4205, // Unable to get value corresponding to the date

        ErrMarketUnknownSymbol              = 4301, // Unknown symbol
        ErrMarketNotSelected                = 4302, // Symbol is not selected in MarketWatch
        ErrMarketWrongProperty              = 4303, // Wrong identifier of a symbol property
        ErrMarketLasttimeUnknown            = 4304, // Time of the last tick is not known (no ticks)
        ErrMarketSelectError                = 4305, // Error adding or deleting a symbol in MarketWatch

        ErrHistoryNotFound                  = 4401, // Requested history not found
        ErrHistoryWrongProperty             = 4402, // Wrong ID of the history property

        ErrGlobalvariableNotFound           = 4501, // Global variable of the client terminal is not found
        ErrGlobalvariableExists             = 4502, // Global variable of the client terminal with the same name already exists
        ErrMailSendFailed                   = 4510, // Email sending failed
        ErrPlaySoundFailed                  = 4511, // Sound playing failed
        ErrMql5WrongProperty                = 4512, // Wrong identifier of the program property
        ErrTerminalWrongProperty            = 4513, // Wrong identifier of the terminal property
        ErrFtpSendFailed                    = 4514, // File sending via ftp failed
        ErrNotificationSendFailed           = 4515, // Failed to send a notification
        ErrNotificationWrongParameter       = 4516, // Invalid parameter for sending a notification — an empty string or NULL has been passed to the SendNotification() function
        ErrNotificationWrongSettings        = 4517, // Wrong settings of notifications in the terminal (ID is not specified or permission is not set)
        ErrNotificationTooFrequent          = 4518, // Too frequent sending of notifications
        ErrFtpNoserver                      = 4519, // FTP server is not specified
        ErrFtpNologin                       = 4520, // FTP login is not specified
        ErrFtpFileError                     = 4521, // File not found in the MQL5\Files directory to send on FTP server
        ErrFtpConnectFailed                 = 4522, // FTP connection failed
        ErrFtpChangedir                     = 4523, // FTP path not found on server
        ErrFtpClosed                        = 4524, // FTP connection closed

        ErrBuffersNoMemory                  = 4601, // Not enough memory for the distribution of indicator buffers
        ErrBuffersWrongIndex                = 4602, // Wrong indicator buffer index

        ErrCustomWrongProperty              = 4603, // Wrong ID of the custom indicator property

        ErrAccountWrongProperty             = 4701, // Wrong account property ID
        ErrTradeWrongProperty               = 4751, // Wrong trade property ID
        ErrTradeDisabled                    = 4752, // Trading by Expert Advisors prohibited
        ErrTradePositionNotFound            = 4753, // Position not found
        ErrTradeOrderNotFound               = 4754, // Order not found
        ErrTradeDealNotFound                = 4755, // Deal not found
        ErrTradeSendFailed                  = 4756, // Trade request sending failed

        ErrIndicatorUnknownSymbol           = 4801, // Unknown symbol
        ErrIndicatorCannotCreate            = 4802, // Indicator cannot be created
        ErrIndicatorNoMemory                = 4803, // Not enough memory to add the indicator
        ErrIndicatorCannotApply             = 4804, // The indicator cannot be applied to another indicator
        ErrIndicatorCannotAdd               = 4805, // Error applying an indicator to chart
        ErrIndicatorDataNotFound            = 4806, // Requested data not found
        ErrIndicatorWrongHandle             = 4807, // Wrong indicator handle
        ErrIndicatorWrongParameters         = 4808, // Wrong number of parameters when creating an indicator
        ErrIndicatorParametersMissing       = 4809, // No parameters when creating an indicator
        ErrIndicatorCustomName              = 4810, // The first parameter in the array must be the name of the custom indicator
        ErrIndicatorParameterType           = 4811, // Invalid parameter type in the array when creating an indicator
        ErrIndicatorWrongIndex              = 4812, // Wrong index of the requested indicator buffer

        ErrBooksCannotAdd                   = 4901, // Depth Of Market can not be added
        ErrBooksCannotDelete                = 4902, // Depth Of Market can not be removed
        ErrBooksCannotGet                   = 4903, // The data from Depth Of Market can not be obtained
        ErrBooksCannotSubscribe             = 4904, // Error in subscribing to receive new data from Depth Of Market

        ErrTooManyFiles                     = 5001, // More than 64 files cannot be opened at the same time
        ErrWrongFilename                    = 5002, // Invalid file name
        ErrTooLongFilename                  = 5003, // Too long file name
        ErrCannotOpenFile                   = 5004, // File opening error
        ErrFileCachebufferError             = 5005, // Not enough memory for cache to read
        ErrCannotDeleteFile                 = 5006, // File deleting error
        ErrInvalidFilehandle                = 5007, // A file with this handle was closed, or was not opening at all
        ErrWrongFilehandle                  = 5008, // Wrong file handle
        ErrFileNottowrite                   = 5009, // The file must be opened for writing
        ErrFileNottoread                    = 5010, // The file must be opened for reading
        ErrFileNotbin                       = 5011, // The file must be opened as a binary one
        ErrFileNottxt                       = 5012, // The file must be opened as a text
        ErrFileNottxtorcsv                  = 5013, // The file must be opened as a text or CSV
        ErrFileNotcsv                       = 5014, // The file must be opened as CSV
        ErrFileReaderror                    = 5015, // File reading error
        ErrFileBinstringsize                = 5016, // String size must be specified, because the file is opened as binary
        ErrIncompatibleFile                 = 5017, // A text file must be for string arrays, for other arrays - binary
        ErrFileIsDirectory                  = 5018, // This is not a file, this is a directory
        ErrFileNotExist                     = 5019, // File does not exist
        ErrFileCannotRewrite                = 5020, // File can not be rewritten
        ErrWrongDirectoryname               = 5021, // Wrong directory name
        ErrDirectoryNotExist                = 5022, // Directory does not exist
        ErrFileIsnotDirectory               = 5023, // This is a file, not a directory
        ErrCannotDeleteDirectory            = 5024, // The directory cannot be removed
        ErrCannotCleanDirectory             = 5025, // Failed to clear the directory (probably one or more files are blocked and removal operation failed)
        ErrFileWriteerror                   = 5026, // Failed to write a resource to a file
        ErrFileEndoffile                    = 5027, // Unable to read the next piece of data from a CSV file (FileReadString, FileReadNumber, FileReadDatetime, FileReadBool), since the end of file is reached

        ErrNoStringDate                     = 5030, // No date in the string
        ErrWrongStringDate                  = 5031, // Wrong date in the string
        ErrWrongStringTime                  = 5032, // Wrong time in the string
        ErrStringTimeError                  = 5033, // Error converting string to date
        ErrStringOutOfMemory                = 5034, // Not enough memory for the string
        ErrStringSmallLen                   = 5035, // The string length is less than expected
        ErrStringTooBignumber               = 5036, // Too large number, more than ULONG_MAX
        ErrWrongFormatstring                = 5037, // Invalid format string
        ErrTooManyFormatters                = 5038, // Amount of format specifiers more than the parameters
        ErrTooManyParameters                = 5039, // Amount of parameters more than the format specifiers
        ErrWrongStringParameter             = 5040, // Damaged parameter of string type
        ErrStringposOutofrange              = 5041, // Position outside the string
        ErrStringZeroadded                  = 5042, // 0 added to the string end, a useless operation
        ErrStringUnknowntype                = 5043, // Unknown data type when converting to a string
        ErrWrongStringObject                = 5044, // Damaged string object

        ErrIncompatibleArrays               = 5050, // Copying incompatible arrays. String array can be copied only to a string array, and a numeric array - in numeric array only
        ErrSmallAsseriesArray               = 5051, // The receiving array is declared as AS_SERIES, and it is of insufficient size
        ErrSmallArray                       = 5052, // Too small array, the starting position is outside the array
        ErrZerosizeArray                    = 5053, // An array of zero length
        ErrNumberArraysOnly                 = 5054, // Must be a numeric array
        ErrOnedimArraysOnly                 = 5055, // Must be a one-dimensional array
        ErrSeriesArray                      = 5056, // Timeseries cannot be used
        ErrDoubleArrayOnly                  = 5057, // Must be an array of type double
        ErrFloatArrayOnly                   = 5058, // Must be an array of type float
        ErrLongArrayOnly                    = 5059, // Must be an array of type long
        ErrIntArrayOnly                     = 5060, // Must be an array of type int
        ErrShortArrayOnly                   = 5061, // Must be an array of type short
        ErrCharArrayOnly                    = 5062, // Must be an array of type char

        ErrOpenclNotSupported               = 5100, // OpenCL functions are not supported on this computer
        ErrOpenclInternal                   = 5101, // Internal error occurred when running OpenCL
        ErrOpenclInvalidHandle              = 5102, // Invalid OpenCL handle
        ErrOpenclContextCreate              = 5103, // Error creating the OpenCL context
        ErrOpenclQueueCreate                = 5104, // Failed to create a run queue in OpenCL
        ErrOpenclProgramCreate              = 5105, // Error occurred when compiling an OpenCL program
        ErrOpenclTooLongKernelName          = 5106, // Too long kernel name (OpenCL kernel)
        ErrOpenclKernelCreate               = 5107, // Error creating an OpenCL kernel
        ErrOpenclSetKernelParameter         = 5108, // Error occurred when setting parameters for the OpenCL kernel
        ErrOpenclExecute                    = 5109, // OpenCL program runtime error
        ErrOpenclWrongBufferSize            = 5110, // Invalid size of the OpenCL buffer
        ErrOpenclWrongBufferOffset          = 5111, // Invalid offset in the OpenCL buffer
        ErrOpenclBufferCreate               = 5112, // Failed to create an OpenCL buffer

        ErrWebrequestInvalidAddress         = 5200, // Invalid URL
        ErrWebrequestConnectFailed          = 5201, // Failed to connect to specified URL
        ErrWebrequestTimeout                = 5202, // Timeout exceeded
        ErrWebrequestRequestFailed          = 5203, // HTTP request failed

        ErrUserErrorFirst                   = 65536 // User defined errors start with this code
    }
}