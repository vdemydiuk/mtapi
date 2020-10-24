function [] = NT_TextToOutputWindow(ListH,Text)

if isempty(Text)
    error(' Text for Outputwindow is empty ');
    Text = 'error';
end

% FigH = figure('Name',windowname, ...
%                    'menubar', 'none', ...
%                    'NumberTitle','off');
%                
% ListH = uicontrol('Style', 'listbox', ...
%                        'Units',    'normalized', ...
%                        'Position', [0,0,1,1], ...
%                        'String',   {}, ...
%                        'Min', 0, 'Max', 2, ...
%                        'Value', []);



  newString = cat(1, get(ListH, 'String'), {Text});
  set(ListH, 'String', newString);
  drawnow;
  
  
  
  
end




