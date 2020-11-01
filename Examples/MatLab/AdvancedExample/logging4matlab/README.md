# Logging for Matlab

This simple logging module is a modification of [`log4m`](http://goo.gl/qDUcvZ)
with the following improvements:

* multiple loggers can be created and retrieved by name (à la Python)
* different logging levels appear in different colors if using Matlab in the terminal

Each logger's output can be directed to the standard output and/or to a file.

Each logger is assigned a logging level that will control the amount of output.
The possible levels are, from high to low:

* ALL (highest)
* TRACE
* DEBUG
* INFO (default)
* WARNING
* ERROR
* CRITICAL
* OFF (lowest)

The default level in INFO.
If a logger outputs at a level lower than or equal to its assigned level, the output will be logged.
To silence a logger, set its level to OFF.

All loggers output a string according to the Matlab format `'%-s %-23s %-8s %s\n'`.
Note that a newline is always appended, so there is no need to terminate log lines
with a newline.
The format is as follows:
* `%-s` is used to display the caller name, i.e., the function or method in which
  logging occured
* `%-23s` is used for a time stamp of the form `2016-09-14 14:23:44,271`
* `%-8s` is used for the logging level
* `%s` is used for the message to be logged.

## API

An instance of the `logging` class is created using the `logging.getLogger` function.
The first argument for this function must be the name of the logger. Four additional
optional arguments are also available.  These can either be provided as name/value
pairs (such as `logger.getlogger(name, 'path', path)`) or as a struct where the
field names are the names of the argument (such as `logger.getlogger(name, struct('path', path)`).
The available arguments are:

* `path`: The path to the log file.  If this is not specified or is an empty string,
  then logging to a file is disabled.
  This must be a string.
* `logLevel`: set the file log level.
  Only log entries with a level greater than or equal to this level will be saved.
  This can either be a string or an integer.
  Note that this argument will be ignored if `path` is empty or not specified.
* `commandWindowLevel`: set the command window log level.
  Only log entries with a level greater than or equal to this level will be displayed.
  This can either be a string or an integer.
* `datefmt`: the date/time format string.
  This contains the date/time format string used by the logs.
  The format must be compatible with the built-in `datestr` function.
  This must be a string.

If `logger` is an instance of the `logging` class, the following methods can be used
to log output at different levels:

* `logger.trace(string)`: output `string` at level TRACE.
  This level is mostly used to trace a code path.
* `logger.debug(string)`: output `string` at level DEBUG.
  This level is mostly used to log debugging output that may help identify an issue
  or verify correctness by inspection.
* `logger.info(string)`: output `string` at level INFO.
  This level is intended for general user messages about the progress of the program.
* `logger.warn(string)`: output `string` at level WARNING (unrelated to Matlab's `warning()` function).
  This level is used to alert the user of a possible problem.
* `logger.error(string)`: output `string` at level ERROR (unrelated to Matlab's `error()` function).
  This level is used for non-critical errors that can endanger correctness.
* `logger.critical(string)`: output `string` at level CRITICAL
  This level is used for critical errors that definitely endanger correctness.
  
The following utility methods are also available:

* `logger.setFileName(string)`: set the log file to `string`.
  This can be used to specify or change the file logs are saved to.
* `logger.setCommandWindowLevel(level)`: set the command window log level to `level`.
  Only log entries with a level greater than or equal to `level` will be displayed.
  `level` can either be a string or an integer.
* `logger.setLogLevel(level)`: set the file log level to `level`.
  Only log entries with a level greater than or equal to `level` will be saved.
  `level` can either be a string or an integer.
  Note that even if the level is changed, nothing will be written if a valid
  filename has not been set for the log.

The following properties can be read or written:

* `logger.datefmt`: the date/time format string.
  This contains the date/time format string used by the logs.
  The format must be compatible with the built-in `datestr` function.
* `logger.commandWindowLevel`: the command window log level.
  Only log entries with a level greater than or equal to `level` will be displayed.
  It can be set with either a string or an integer, but will always return an integer.
* `logger.logLevel`: the file log level.
  Only log entries with a level greater than or equal to `level` will be saved.
  It can be set with either a string or an integer, but will always return an integer.

The following properties are read-only (note that these are called in a different way):

* `logging.logging.ALL`: The integer value for the `ALL` level (0).
* `logging.logging.TRACE`: The integer value for the `TRACE` level (1).
* `logging.logging.DEBUG`: The integer value for the `DEBUG` level (2).
* `logging.logging.INFO`: The integer value for the `INFO` level (3).
* `logging.logging.WARNING`: The integer value for the `WARNING` level (4).
* `logging.logging.ERROR`: The integer value for the `ERROR` level (5).
* `logging.logging.CRITICAL`: The integer value for the `CRITICAL` level (6).
* `logging.logging.OFF`: The integer value for the `OFF` level (6).
  Note that there is no corresponding write method for this level, 
  so if this level is set no logging will take place.

## Examples

A logger at default level INFO logs messages at levels INFO, WARNING, ERROR and CRITICAL, but not at levels TRACE or DEBUG:

```matlab
>> addpath('/path/to/logging4matlab')
>> logger = logging.getLogger('mylogger')  % new logger with default level INFO
>> logger.info('life is just peachy')
logging.info 2016-09-14 15:10:06,049 INFO     life is just peachy
>> logger.debug('Easy as pi! (Euclid)')    % produces no output
>> logger.critical('run away!')
logging.critical 2016-09-14 15:12:37,652 CRITICAL run away!
```

A logger's assigned level for the command window (or terminal) can be changed:

```matlab
>> logger.setCommandWindowLevel(logging.logging.WARNING)
```

A logger can also output to file:

```matlab
>> logger2 = logging.getLogger('myotherlogger', 'path', '/tmp/logger2.log')
>> logger.setLogLevel(logging.logging.WARNING)
```

Output to either the command window or a file can be suppressed with `logging.logging.OFF`.

# FAQ

1. *Why is there no colored logging in the Matlab command window?*
   I haven't gotten around to evaluating the performance of [`cprintf`](https://goo.gl/Nw5OOy),
   which seems to be the only viable option for colored output in the command window.
   Pull request welcome!
2. *Can I change the colors?*
   Currently, no, but feel free to submit a pull request!
3. *Can I change the format string used by loggers?*
   Currently, no, but feel free to submit a pull request!
