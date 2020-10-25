function clearLogger(name)
    [~, destructor] = logging.getLogger(name);
    destructor();
end

