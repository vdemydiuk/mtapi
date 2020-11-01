function [str,header] = http_paramsToString(params,encodeOption)
%http_paramsToString Creates string for a POST or GET requests
%
%   [queryString,header] = http_paramsToString(params, *encodeOption)
%
%   INPUTS
%   =======================================================================
%   params: cell array of property/value pairs
%           NOTE: If the input is in a 2 column matrix, then first column
%           entries are properties and the second column entries are
%           values, however this is NOT necessary (generally linear)
%   encodeOption: (default 1)
%           1 - the typical URL encoding scheme (Java call)
%               
%   OUTPUTS
%   =======================================================================
%   queryString: querystring to add onto URL (LACKS "?", see example)
%   header     : the header that should be attached for post requests when
%                using urlread2
%
%   EXAMPLE:
%   ==============================================================
%   params = {'cmd' 'search' 'db' 'pubmed' 'term' 'wtf batman'};
%   queryString = http_paramsToString(params);
%   queryString => cmd=search&db=pubmed&term=wtf+batman
%
%   IMPORTANT: This function does not filter parameters, sort them,
%   or remove empty inputs (if necessary), this must be done before hand

if ~exist('encodeOption','var')
    encodeOption = 1;
end

if size(params,2) == 2 && size(params,1) > 1
    params = params';
    params = params(:);
end

str = '';
for i=1:2:length(params)
    if (i == 1), separator = ''; else separator = '&'; end
    switch encodeOption
        case 1
            param  = urlencode(params{i});
            value  = urlencode(params{i+1});
%         case 2
%             param    = oauth.percentEncodeString(params{i});
%             value    = oauth.percentEncodeString(params{i+1});
%               header = http_getContentTypeHeader(1);
        otherwise
            error('Case not used')
    end
    str = [str separator param '=' value]; %#ok<AGROW>
end

switch encodeOption
    case 1
        header = http_createHeader('Content-Type','application/x-www-form-urlencoded');
end


end