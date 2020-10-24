function [obj, deleteLogger] = getLogger(name, varargin)
  persistent loggers;
  logger_found = false;
  if ~isempty(loggers)
    for logger = loggers
      if strcmp(logger.name, name)
        obj = logger;
        logger_found = true;
        break;
      end
    end
  end
  if ~logger_found
    obj = logging.logging(name, varargin{:});
    loggers = [loggers, obj];
  end
  
  deleteLogger = @() deleteLogInstance();
  
  function deleteLogInstance() 
      if ~logger_found
          error(['logger for file [ ' name ' ] not found'])
      end
      loggers = loggers(loggers ~= obj);
      delete(obj);
      clear('obj');
  end
end
