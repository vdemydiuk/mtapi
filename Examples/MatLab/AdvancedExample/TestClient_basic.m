function [ClientResult] = TestClient_basic()
% profile('-memory','on');
% profile('on');
 
ClientResult = 0;

Instance = 'default';

dbstop if error
tic 
%% slack
WebHook = 'https://hooks.slack.com/services/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';   % Use your KEY HOOK
message = strcat(Instance,': Start TestClient_basic');

SendSlackNotification(WebHook,message);


% load api to mtclient
MtApi_Dlls(1).path =  'C:\Program Files\MtApi5\MtApi5.dll';
MtApi_Dlls(1).name = 'Default';
MtApi_Dlls(1).version = '';

MtApi_Dlls(2).path =  'dll\MtApi5.dll ' ;  % 1.021.1
MtApi_Dlls(2).name = 'dev_64';
MtApi_Dlls(2).version = '21.1';
 
 
client = Mt5Api(MtApi_Dlls(2).path);

client.init_Loggers('DEBUG','DEBUG',WebHook,Instance);



eTF    = MtApi5.ENUM_TIMEFRAMES.PERIOD_M1;
chSymbol = 'EURUSD';

[ApiConnected, ~] = client.runOn(Instance);     

if ~ApiConnected
    exitCode = 5;   
    disp(['ExitCode :',num2str(exitCode)])
    return;
end   


   
         
[ok,EurUsd_Bars] = client.cTimeSeries.mCreateFilledBars(chSymbol,eTF,0,10000,int32(400000));
                            

filename = sprintf('bars_%s_bytestream.mat',chSymbol);
% filestring = strcat(filepath,filename);

           
EurUsd_Bars.saveDataAsByteStream(filename); 
res = dir(filename);
client.cLogger.debug('Bars Saved ');
client.cLogger.debug(sprintf('File name: %s',res.name));
client.cLogger.debug(sprintf('File size %s = %.2f kb ', chSymbol,res.bytes/1024)); 

ClientResult = 1;
toc
end
