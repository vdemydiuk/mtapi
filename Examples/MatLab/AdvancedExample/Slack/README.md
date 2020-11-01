# README #

This repository contains ```Matlab``` functions to send notifications to a Slack channel or user, via the Slack [Incoming Webhooks](https://slack.com/services/new/incoming-webhook) API.

## Usage ##

```SendSlackNotification``` is used to send a notification to a URL provided by Slack, for a configured [Incoming Webhooks](https://slack.com/services/new/incoming-webhook) integration. See the documentation for this function for information on options.

```MakeSlackAttachments``` can be used to generate Slack [message attachments](https://api.slack.com/docs/attachments), which can then be sent as notifications using ```SendSlackNotification```. See the documentation for this function for more information.

## Set up ##

1. Configure an [Incoming Webhooks](https://slack.com/services/new/incoming-webhook) integration for your team, from your team's [services](https://slack.com/services) page.
2. Copy the Webhook URL once the service is configured, and store it in a ```Matlab``` string.
3. Clone the [SlackMatlab repository](https://github.com/DylanMuir/SlackMatlab), and add the root directory to the ```Matlab``` path using ```pathtool```.
4. Call ```SendSlackNotification```, passing the Webhook URL as an argument.

### Example ###

```matlab
% - Create a message attachment to send with a notification
%   (optional; several message attachments can be sent with a single notification)
sA = MakeSlackAttachment('New open task [urgent]: <link.to.website>', 'Text of the notification message', ...
      'Text that will be displayed before the message', '#0000ff', ...
      {'Field 1 title', 'This is a field that will be shown in a table'}, ...
      {'Field 2 title', 'This is another field that will be shown in a table'});

% - Send the notification, with the attached message
SendSlackNotification('https://hooks.slack.com/services/this/is/your/webhook/url', ...
   'I sent this notification from matlab, on behalf of @username.', '#target-channel', ...
   'Name to post under', 'http://www.icon.com/url/to/icon/image.png', [], sA);
```

## Emojis ##

A list of Emojis supported by Slack is available from http://www.emoji-cheat-sheet.com.


## Acknowledgements ##

Contains code from [URLREAD2](http://www.mathworks.com/matlabcentral/fileexchange/35693-urlread2) and [JSONLAB](http://www.mathworks.com/matlabcentral/fileexchange/33381-jsonlab--a-toolbox-to-encode-decode-json-files-in-matlab-octave).
