//+----------------------------------------------------------------------------+
//|                                                              mql4-auth.mqh |
//+----------------------------------------------------------------------------+
//|                                                      Built by Sergey Lukin |
//|                                                    contact@sergeylukin.com |
//+----------------------------------------------------------------------------+

#include <WinUser32.mqh>

#import "user32.dll"
int GetAncestor(int,int);
int GetLastActivePopup(int);
int GetDlgItem(int,int);
int GetParent(int hWnd);
#import
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
bool loginDialogIsOpen()
  {
   int hwnd=WindowHandle(Symbol(),Period());
   int hMetaTrader,hLoginDialog=0;

// Retrieve Terminal Window Handler
   while(!IsStopped())
     {
      hwnd=GetParent(hwnd);
      if(hwnd==0) break;
      hMetaTrader=hwnd;
     }

   hLoginDialog=GetLastActivePopup(hMetaTrader);
   if(hLoginDialog!=0)
     {
      return(true);
        } else {
      return(false);
     }
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void closeLoginDialog()
  {
   int hwnd=WindowHandle(Symbol(),Period());
   int hMetaTrader,hLoginDialog=0,hCancelButton=0;

// Retrieve Terminal Window Handler
   while(!IsStopped())
     {
      hwnd=GetParent(hwnd);
      if(hwnd==0) break;
      hMetaTrader=hwnd;
     }

   Sleep(60);

   hLoginDialog=GetLastActivePopup(hMetaTrader);
   if(hLoginDialog!=0)
     {
      hCancelButton=GetDlgItem(hLoginDialog,0x2);
      if(hCancelButton!=0)
        {
         // Click "Cancel" button in Login Dialog
         PostMessageA(hCancelButton,0x00F5,0,0);
        }
     }
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
bool auth(string login,string passwd,string server)
  {
   datetime s=TimeLocal();
   int h = 0, e = 0, a = GetAncestor(WindowHandle(Symbol(), NULL), 2);
   int i = 0;

   PostMessageA(a,WM_COMMAND,35429,0);

   while(e==0)
     {
      // Give us up to a minute to find the Login dialog
      if(TimeLocal()-s>60)
        {
         return(false);
        }

      h=GetLastActivePopup(a);

      // Select login field
      e=GetDlgItem(h,0x49d);

      Sleep(1);
     }

// Press DELETE key many times in login field to remove current value
   for(i=1; i<=100; i++) 
     {
      PostMessageA(e,WM_KEYDOWN,0x2E,1);
     }
// Iterate over characters in "login" string and pass them to login field one by one
   char login_chars[];
   string2CharsArray(login,login_chars);
   for(i=0; i<ArraySize(login_chars); i++) 
     {
      PostMessageA(e,WM_CHAR,login_chars[i],0);
     }

// Select password field
   e=GetDlgItem(h,0x4c4);
// Press DELETE key many times in password field to remove current value
   for(i=1; i<=100; i++) 
     {
      PostMessageA(e,WM_KEYDOWN,0x2E,1);
     }
// Iterate over characters in "passwd" string and pass them to password field one by one
   char password_chars[];
   string2CharsArray(passwd,password_chars);
   for(i=0; i<ArraySize(password_chars); i++) 
     {
      PostMessageA(e,WM_CHAR,password_chars[i],0);
     }

// Select server field
   e=GetDlgItem(h,0x50d);
// Press DELETE key many times in server field to remove current value
   for(i=1; i<=100; i++) 
     {
      PostMessageA(e,WM_KEYDOWN,0x2E,1);
     }
// Iterate over characters in "server" string and pass them to server field one by one
   char server_chars[];
   string2CharsArray(server,server_chars);

   for(i=0; i<ArraySize(server_chars); i++) 
     {
      PostMessageA(e,WM_CHAR,server_chars[i],0);
     }
   Sleep(2*1000);
// Press submit button
   e=GetDlgItem(h,0x1);
   SendMessageA(e,0x00F5,0,0);

   return(true);
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
/*
 * Iterates over string and puts each character code in array
 */
void string2CharsArray(string myString,char  &chars[])
  {
   int cnt=StringLen(myString);
   ArrayResize(chars,cnt);

   for(int i=0; i<cnt; i++) 
     {
      chars[i]=StringGetChar(myString,i);
     }
  }
//+------------------------------------------------------------------+

