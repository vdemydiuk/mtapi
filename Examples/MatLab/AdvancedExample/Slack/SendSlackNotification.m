function [strHTTPOutput, sHTTPExtra] = SendSlackNotification(strHookURL, strText, strTarget, strUsername, strIconURL, strIconEmoji, csAttachments)

% SendSlackNotification - FUNCTION Send a customisable notification via a Slack webhook integration
%
% Usage: [strHTTPOutput, sHTTPExtra] = SendSlackNotification(strHookURL, strText, <strTarget, strUsername, strIconURL, strIconEmoji>, ...)
%                                      SendSlackNotification(..., sAttachment)
%                                      SendSlackNotification(..., {sAttachment1 sAttachment2})
% 
% Use the Slack webhooks API to send a notification to a Slack channel or
% user. See See <https://slack.com/services/new/incoming-webhook>,
% <https://api.slack.com/docs/attachments> and
% <https://api.slack.com/docs/formatting> for further information.
%
% 'strHookURL' is the full URL configured for webhook integration from
% Slack.
%
% 'strText' is a string containing (possibly marked-up) text, which will be
% sent as the notification.
%
% Optional arguments:
%    strTarget: A channel name ('#channel') or a user name ('@username') to
%               send the notification to. By default, the channel
%               configured within Slack for the provided webhook URL will
%               be used.
%    strUsername: A text string defining the name under which the
%                 notification will be posted. By default, Slack uses
%                 'incoming-webhook'.
%    strIconURL: A URL referencing an image file to use as the icon for the
%                notification. By default, Slack uses a webhook icon.
%    strIconEmoji: A text string containing an Emoji reference (e.g.
%                  ':bear:'), that Slack will use as the icon for the
%                  notification. Note that strIconURL and strIconEmoji
%                  should not both be provided.
%
%    sAttachment: A Slack attachment structure, created by
%                 'MakeSlackAttachment'. Multiple attachments can be
%                 provided in a cell array.
%
% Uses components of:
%    URLREAD2: http://www.mathworks.com/matlabcentral/fileexchange/35693-urlread2
%    JSONLAB: http://www.mathworks.com/matlabcentral/fileexchange/33381-jsonlab--a-toolbox-to-encode-decode-json-files-in-matlab-octave
%
% With thanks to Jim Hokanson and Qianqian Fang.

% Author: Dylan Muir <dylan.muir@unibas.ch>
% Created: 19th November, 2014

% -- Check arguments

if (nargin < 2)
   help SendSlackNotification;
   error('*** SendSlackNotification: Incorrect usage.');
end


% -- Create JSON payload structure

% - Set up  payload
sPayload.text = strText;

% - Add target channel or user
if (exist('strTarget', 'var') && ~isempty(strTarget))
   sPayload.channel = strTarget;
end

% - Define custom source user name
if (exist('strUsername', 'var') && ~isempty(strUsername))
   sPayload.username = strUsername;
end

% - Define custom icon (URL)
if (exist('strIconURL', 'var') && ~isempty(strIconURL))
   sPayload.icon_url = strIconURL;
end

% - Define custom icon (emoji)
if (exist('strIconEmoji', 'var') && ~isempty(strIconEmoji))
   sPayload.icon_emoji = strIconEmoji;
end

% - Add attachments
if (exist('csAttachments', 'var') && ~isempty(csAttachments))
   % - Accept a single attachment as a simple structure
   if (~iscell(csAttachments))
      csAttachments = {csAttachments};
   end
   
   % - Add a dummy attachment, if necessary (to ensure proper JSON
   %   formatting)
   if (numel(csAttachments) == 1)
      csAttachments = [csAttachments MakeSlackAttachment('')];
   end
   
   % - Include the attachments
   sPayload.attachments = csAttachments;
end


% -- Translate to JSON

opt.NoRowBracket = 1;
strJSON = savejson('', sPayload, opt);


% -- Send to Slack using POST to the hook URL

[strHTTPOutput, sHTTPExtra] = urlread2(strHookURL, 'POST', strJSON);


% --- END of SendSlackNotification.m ---
