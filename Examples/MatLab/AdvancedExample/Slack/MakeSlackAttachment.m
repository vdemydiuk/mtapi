function sAttachment = MakeSlackAttachment(strFallback, strText, strPreText, strColor, varargin)

% MakeSlackAttachment - FUNCTION Construct an attachment to add to a Slack notification
%
% Usage: sAttachment = MakeSlackAttachment(strFallback, <strText>, <strPreText>, strColor, ...)
% Usage: sAttachment = MakeSlackAttachment(..., {strField1Title, strField1Value, <bField1Short>}, {strField2Title, strField2Value, <bField2Short>}, ...)
% 
% Creates an attachment structure, to send along with a Slack notification
% using 'SendSlackNotification'. See <https://api.slack.com/docs/attachments>
% and <https://api.slack.com/docs/formatting> for information.
%
% 'strFallback' is a text string, which is displayed when the notification
% cannot be displayed in full.
%
% 'strText' (optional) is a text string containing the text of the notification.
%
% 'strPreText' (optional) is a text string that will be displayed before
% the notification text.
%
% 'strColor' (optional) is a hex color string (e.g. '#ff3300'), or one of
% 'good', 'warning', 'danger'.
%
% These parameters can be followed by a list of cell arrays, each array
% containing a "field" to be added to the attachment. Each field array must
% be in the format {strTitle, strValue <, bShort>}. 'strTitle' is a text
% string containing the title of the field. 'strValue' is a text string
% containing the value to be shown for that field.
%
% If present, these fields will be shown below the attachment in a
% notificaition.

% Author: Dylan Muir <dylan.muir@unibas.ch>
% Created: 19th September, 2014

% -- Check arguments

if (nargin < 1)
   help MakeSlackAttachment;
   error('*** MakeSlackAttachment: Incorrect usage');
end

% - Include fallback text
sAttachment.fallback = strFallback;

% - Include extended text
if (exist('strText', 'var') && ~isempty(strText))
   sAttachment.text = strText;
end

% - Include pre-text
if (exist('strPreText', 'var') && ~isempty(strPreText))
   sAttachment.pretext = strPreText;
end

% - Include extended text
if (exist('strColor', 'var') && ~isempty(strColor))
   sAttachment.color = strColor;
end

% - Include list of fields
for (nFieldIndex = numel(varargin):-1:1)
   sThisField = [];
   sThisField.title = varargin{nFieldIndex}{1};
   sThisField.value = varargin{nFieldIndex}{2};
   
   % - Add "short"
   if ((numel(varargin{nFieldIndex}) > 2) && varargin{nFieldIndex}{3})
      sThisField.short = 'true';
   else
      sThisField.short = 'false';
   end
   
   sAttachment.fields{nFieldIndex} = sThisField;
end

% - Add a dummy field if necessary (to ensure proper JSON formatting)
if (numel(varargin) == 1)
   sThisField = [];
   sThisField.title = '';
   sAttachment.fields{end+1} = sThisField;
end


% --- END of MakeSlackAttachment.m ---
