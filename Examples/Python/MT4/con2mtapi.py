#!/usr/bin/env python3 
# con2mtapi.py - An example for connecting to the mtapi EA on MT4 and get some data
# -*- coding: utf-8 -*-
# ---------------------------------------------------------------------
#   Author:         EAML
#   Date:           2020-11-10
#   Version:        1.0.2
#   License:        MIT
#   URL:            https://github.com/eabase/mt4pycon/
# ---------------------------------------------------------------------
#
#   Description: 
#       
#       This is an example client using the MT4 API (mtapi) to connect 
#       to the MT4 EA, using either an TCP/IP or a pipe port, to get some
#       historical candle OHLCV data for a specified symbol and timeframe.
#
#   ToDo:
#
#       [ ] Make a native pip installer package (PyPi)
#       [ ] Automatic detection of MT5
#       [ ] Fix bug when requested chart does not (yet) have any candle data... double run!
#       [x] Allow TF specification as MT4/5 text ('4H') and not in minutes ('240')
#       [ ] Add CLI options:
#           ----------------------------------------------------------
#           - [ ] '-5'              : Use script for MT5 installs (this will use MT5 DLL's)
#           - [ ] '-c'              : Enable ANSI color markup of output
#           - [x] '-d'              : Enable extra debug info
#           - [ ] '-e'              : Show candle data as minimal CSV list (ignores -l) ???
#           - [x] '-h'              : Show THIS help list
#           - [x] '-n <n>'          : Show <n> number of candles back ['0' is last complete candle]
#           - [x] '-p'              : Use a named pipe connection (instead of a TCP/IP host:port) 
#           - [x] '-l'              : Show candle data as a list (one line per OHLC item)
#           - [x] '-s <symbol>'     : Set symbol name to <symbol>   ['EURUSD']
#           - [x] '-t <timeframe>'  : Set timeframe to <timeframe>
#           - [x] '-v'              : Show THIS program version
#           ----------------------------------------------------------
#   
#   Installation:
#
#       <TBA>
#   
#   Usage:
#   
#       <TBA>
#   
#   Requirements:
#   
#       pip install pythonnet
#       pip install 
#   
#   NOTES
#
#   1.  Spread for candles are meaningless, unless we talk about an "average", 
#       which is not available without also getting all the ticks inside.
#
#   2.  Some Default Settings: 
#       For MT5:
#       MTPATH = C:\Program Files\MtApi5    # Default Path to installed MtApi library files
#       MTSERV = 127.0.0.1                  # Default IP address of your localhost where MtApi terminal is running
#       MTPORT = 8228                       # Default Port setting of your MtApi EA
#
#   3.  Using TCP vs. a "Named Pipe" connection:
#       TCP:    client.BeginConnect(ip, port)
#       Pipe:   client.BeginConnect(port)
#       (Note that a Windows "named pipe" is using the "localhost" interface.)
# 
#   4.  What is the deal with: 0.0.0.0  &  127.0.0.1 ?
#       See [4]
#
#   5.  To measure performance, use:
#       (Measure-Command  { python.exe .\con2mtapi.py -n10000 -s "CADCHF" -t 15 -p }).TotalSeconds 
#   
#   6.  How to get Tick data streamed?
#
#       struct MqlTick {
#           datetime     time;          // Time of the last prices update
#           double       bid;           // Current Bid price
#           double       ask;           // Current Ask price
#           double       last;          // Price of the last deal (Last)
#           ulong        volume;        // Volume for the current Last price
#    
#   REFERENCES
# 
#   [1] https://www.mql5.com/en/docs/constants/structures/mqlrates
#   [2] https://docs.mql4.com/constants/structures/mqltick
#   [3] https://stackoverflow.com/questions/48542644/python-and-windows-named-pipes
#   [4] https://www.howtogeek.com/225487/what-is-the-difference-between-127.0.0.1-and-0.0.0.0/
#   [5] https://docs.mql4.com/constants/chartconstants/enum_timeframes
# 
# ---------------------------------------------------------------------
import os, sys
import clr
import time
import getopt

#--------------------------------------
# For MT5 installs
#--------------------------------------
# C:\Program Files\MtApi5
#sys.path.append(r"C:\Program Files\MtApi5")
#asm = clr.AddReference("MtApi5")
#import MtApi5 as mt

#--------------------------------------
# For MT4 installs
#--------------------------------------
# C:\Program Files (x86)\MtApi
sys.path.append(r"C:\Program Files\MtApi")
asm = clr.AddReference('MtApi')
import MtApi as mt

#--------------------------------------
# Import/Use colorama
#--------------------------------------
##from colorama import Fore, Style
#from colorama import init, AnsiToWin32
#init(wrap=False)
#stream = AnsiToWin32(sys.stderr).stream
#--------------------------------------

#----------------------------------------------------------
#  Global Variables
#----------------------------------------------------------
__author__    = 'EAML (EABASE)'
__copyright__ = 'MIT (2020)'
__version__   = '1.0.2'

debug   = 0      # Enable debug printing

#----------------------------------------------------------
#  Default Program Constants
#----------------------------------------------------------
#MTSERV  = '192.168.1.101'          # IP address of "your local machine" where MtApi terminal is running
MTSERV = '127.0.0.1'                # IP address of "your localhost"     where MtApi terminal is running
#MTSERV = '0.0.0.0'                 # IP address of "ALL on local net"   where MtApi terminal is running
MTPORT  = 8222                      # Port setting of your MT4 EA  [Default:8222]
#MTPORT = 8228                      # Port setting of your MT5 EA  [Default:8228]
MTSYMB  = 'EURUSD.e'                # <select your default symbol>
#MTTIME = 'M15'                     # <select your default timefrme>

#------------------------------------------------
# mt.ENUM_TIMEFRAMES.PERIOD_M15
#------------------------------------------------
t1 = '[M1,M5,M15,M30,H1,H4,D1,W1,MN1]'              # Standard Periods
t2 = '[M2,M3,M4,M6,M10,M12,M20,H2,H3,H6,H8,H12]'    # Extended Periods (MQL/API only)
p1 = [1,5,15,30,60,240,1440,10080,43200]            # [min]
p2 = [2,3,4,6,10,12,20,120,180,360,480,720]         # [min]

z1 = t1.translate(str.maketrans('','','[]')).split(',')     # Remove "[" and "]"
z2 = t2.translate(str.maketrans('','','[]')).split(',')     # 
d1 = dict(zip(z1,p1))                                       # convert to dict
d2 = dict(zip(z2,p2))                                       # convert to dict
d3 = {**d1,**d2}                                            # Add the 2 dicts (in Py3.9+ use: d3=d1|d2)

d4 = {k: v for k, v in sorted(d3.items(), key=lambda item: item[1])}    # sort dict

# "reverse" the dict key:value pairs, so we can use 
# get('value') to obtain the key of the original
#d4r = dict(zip(d4.values(),d4.keys()))

#TF = d4.get('M15')
# Get the key, given a value
#TFk = [k for k in d4 if (d4[k] == TF)][0]
#TFk = next((k for k in d4 if d4[k] == TF), None)

#------------------------------------------------
# Text Coloring
#------------------------------------------------
# Usage:  print(yellow("This is yellow"))
def color(text, color_code):
    #if self.nposix:
    #if not is_posix():
    #    return text
    # for brighter colors, use "1;" in front of "color_code"
    bright = ''  # '1;'
    return '\x1b[%s%sm%s\x1b[0m' % (bright, color_code, text)

def red(text):    return color(text, 31)            #                   # noqa
def green(text):  return color(text, 32)            # '1;49;32'         # noqa
def bgreen(text): return color(text, '1;49;32')     # bright green      # noqa
def orange(text): return color(text, '0;49;91')     # 31 - looks bad!   # noqa
def yellow(text): return color(text, 33)            #                   # noqa
def blue(text):   return color(text, '1;49;34')     # bright blue       # noqa
def purple(text): return color(text, 35)            # aka. magenta      # noqa
def cyan(text):   return color(text, '0;49;96')     # 36                # noqa
def white(text):  return color(text, '0;49;97')     # bright white      # noqa

#------------------------------------------------
# Print Usage
#------------------------------------------------
def usage():
    myName = os.path.basename(__file__)
    print('\n  Usage:  ./{}\n'.format(myName))
    print('  This connects to an MT4 EA via a TCP port (or pipe) to receive OHLCV data.\n')
    # [:cdhprvn:s:t:]
    print(' ','-'*80)
    print('  Command Line Options:')
#    print('   -5              : Use script for MT5 installs (this will use MT5 DLL\'s)')    
#    print('   -c              : Enable ANSI color markup of output')
    print('   -d              : Enable extra debug info')
    #print('   -e              : Show candle data as minimal CSV list (ignores -l)')     #  ???
    print('   -n <n>          : Show <n> number of candles back')
    print('   -p              : Use a "Named Pipe" connection (instead of a TCP IP host:port) ')
    print('   -l              : Show candle data as a list (one line per OHLC item)')
    print('   -s <symbol>     : Set symbol name to <symbol>   ["EURUSD"]')
    print('   -t <timeframe>  : Set timeframe to <timeframe>  [M1,M5,M15*,M30,H1,H4,D1,W1,MN1]**')
    print('   -h, --help      : Show THIS help list')
    print('   -v, --version   : Show THIS program version')
    print(' ','-'*80)
    print('   *  = a default setting')
    print('  **  = The standard MT4 TF\'s are:')
    print('        [M1,M5,M15,M30,H1,H4,D1,W1,MN1]')
    print('        [1,5,15,30,60,240,1440,10080,43200]\n')
    print('        The non-standard TF\'s are: (not yet available)')
    print('        [M2,M3,M4,M6,M10,M12,M20,H2,H3,H6,H8,H12]')
    print('        [2,3,4,6,10,12,20,120,180,360,480,720] ')
    print(' ','-'*80)
    print('\n  Example for Windows:')
    #print('  python.exe .\\{} -n4 -s "CADCHF.x" -t 240 -p -d\n'.format(myName))
    print('  python.exe .\\{} -n4 -s "CADCHF.x" -t H4 -p -d\n'.format(myName))
    print('  Please file any bug reports at:')
    print('  https://github.com/eabase/mt4pycon/\n')
    print('  For support of the MT4/5 API, see:')
    print('  https://github.com/vdemydiuk/mtapi/\n')
    print('  Version:  {}'.format(__version__))
    print('  License:  {}\n'.format(__copyright__))
    sys.exit(2)

#----------------------------------------------------------
#  Helper Functions
#----------------------------------------------------------
# Test if a given TF argument is a integer
def isArgInt(a) :
    try:
        int(a)
        return True
    except ValueError:
        return False

def printTFerr(a,b) :
    print("  ERROR: Bad TF argument: {} reverting to default: {} min.".format(a,b))

#----------------------------------------------------------
# Description: 
#   Get history data of MqlRates structure of a specified symbol-period 
#   in specified quantity into the ratesArray array. The elements ordering 
#   of the copied data is from present to the past, i.e., starting 
#   position of 0 means the current bar.
#
# NOTE:
#
#   The CopyRates() implementations are different between MT4 and MT5.
#
#   [1] https://docs.mql4.com/series/copyrates
#   [2] https://www.mql5.com/en/docs/series/copyrates
# 
#   MT5: int             CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out MqlRates[] ratesArray)
#   MT4: List<MqlRates>  CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count)
#----------------------------------------------------------
def getCopyRates(count,SYM,TF,useList,useCSV):

    startPos    = 1                                 # Starting position of candle (backward counting up!)
    #count      = 3                                 # Number of candles requested (to be received into Rate Array)
    rA          = []                                # The "Rate Array" for Receiving results
    ccnt        = 0                                 # Number of candles actually recived (into array)
    errn        = 0                                 # The GetLastError "code" (supposedly?)

    try:
        # Check how many candles are available to download:
        #cc      = c.iBars(SYM,TF)
        # if cc < count : print('  INFO: CopyRates: Not all candles are available!');

        #ccnt   = client.CopyRates(SYM, TF, startPos, count, rA)    # MT5:  int
        rA      = client.CopyRates(SYM, TF, startPos, count)        # MT4:  List<MqlRates>
        ccnt    = len(rA)                                           # Rate Array length

    except Exception as e:
        print('\n  ERROR: in CopyRates(): \n  {}\n'.format(e))
        if debug : 
            print('  DBG: CopyRates:ccnt = ', ccnt)
            errn = client.GetLastError()
            errd = client.ErrorDescription(errn) if errn else '<n/a>'; 
            print('  DBG: CopyRates: ErrorCode :  {:d}'.format(errn))
            print('  DBG: CopyRates: ErrorDescription :\n  {}'.format(errd))
        exit()

    errn = client.GetLastError();
    #TFk = next((k for k in d4 if d4[k] == TF), None)
    TFk  = [k for k in d1 if (d1[k] == TF)][0]

    #if debug :
    print()
    print(' ','-'*60)
    print('  INFO: CopyRates: Symbol    :  {}'.format(SYM))
    print('  INFO: CopyRates: TimeFrame :  {}'.format(TFk))
    print('  INFO: CopyRates: Requested :  {:d} candles'.format(count))
    print('  INFO: CopyRates: Received  :  {:d} candles'.format(ccnt))
    print('  INFO: CopyRates: ErrorCode :  {:d}'.format(errn))          # rA[0].ErrorCode))
    print(' ','-'*60)
    print('  INFO: CopyRates: Rate Array (rA):\n')

    #--------------------------------------------
    # Response = {"ErrorCode" : 0,  "Rates" : 
    #   [{  "Close"         : 1.17094,
    #       "Open"          : 1.17206,
    #       "MtTime"        : 1604432700,
    #       "Low"           : 1.17079,
    #       "High"          : 1.17207,
    #       "TickVolume"    : 1251,
    #       "RealVolume"    : 0,
    #       "Spread"        : 0},  ...
    #--------------------------------------------
    # We skip:  rA[i].Spread (see notes)
    #--------------------------------------------

    if useCSV : 
        csv_th = 'Time,Open,High,Low,Close,Volume'
        csv_tf = '{},{:.5f},{:.5f},{:.5f},{:.5f},{:d}'
        print(csv_th)
        for i in range(ccnt):
            print(csv_tf.format(rA[i].Time, rA[i].Open, rA[i].High, rA[i].Low, rA[i].Close, rA[i].TickVolume)) 
        sys.exit(2)

    if useList :
        myLformat   = '[{}]:  {}\nO: {}\nH: {}\nL: {}\nC: {}\nV: {:d}\n' # S: {:.2f}
        for i in range(ccnt):
            print(myLformat.format(i, rA[i].Time, rA[i].Open, rA[i].High, rA[i].Low, rA[i].Close, rA[i].TickVolume)) 
    else :
        tableHeader = '  {:<4}  {:<19}   {:<7}  {:<7}  {:<7}  {:<7}   {:<6}'
        header_str  = tableHeader.format('#','Time','Open', 'High', 'Low', 'Close', 'Volume')
        # 0:  2020-11-04 01:45:00   1.17531  1.17616  1.17500  1.17613   590
        hlen = len(header_str)
        print(header_str)
        print(' ','-'*hlen)

        myRformat       = '  {:<3}:  {}   {:.5f}  {:.5f}  {:.5f}  {:.5f}   {:d}'
        for i in range(ccnt):
            print(myRformat.format(i, rA[i].Time, rA[i].Open, rA[i].High, rA[i].Low, rA[i].Close, rA[i].TickVolume)) 

#----------------------------------------------------------
#  ToDo
#----------------------------------------------------------
#def getLiveTicks():
    # Here  be dragons
    # SymbolInfoTick
    # MqlTick tick;
    #   if (!SymbolInfoTick(symbol, tick)) {
    #      response = CreateErrorResponse(GetLastError(), "SymbolInfoDouble failed");
    #      return;
    #   }
    #SymbolInfoTick

#----------------------------------------------------------
#  MAIN
#----------------------------------------------------------
if __name__ == '__main__':
#def main(self):

    #----------------------------------------------------------
    # CLI arguments
    #----------------------------------------------------------
    dTF     = d1.get('M15')
    nmax    = 3             # Default number of candles
    mtVer   = 4             # Default to MT4 API version [4,5] 
    aSym    = str(MTSYMB)   # Default Symbol()
    aTF     = dTF           # Default TimeFrame [Default: M15]
    apipe   = 0             # Use a Named Pipe port for connection (instead of TCP host:port)
    alist   = 0             # 
    useCSV  = 0             # ... CSV

    narg = len(sys.argv) - 1
    try:
        opts, args = getopt.getopt(sys.argv[1:], ":cdehpvln:s:t:", ["help", "version"])
    except getopt.GetoptError : 
        usage(); sys.exit();

    if not opts : 
        if not args : 
            usage(); sys.exit();
    else :
        for opt, arg in opts:
            if (debug) :
                print ("opt: ", opt)
                print ("arg: ", arg)

            if opt in ("-h", "--help"):         usage(); sys.exit();
            elif opt in ("-v", "--version"):    print ("Version: ", __version__); sys.exit();
            #elif opt == "-c": useColors=1;                     # Use colored markup on: (Time, Volume, OHCL)
            elif opt == "-d": debug = 1;                        # 
            elif opt == "-e": useCSV = 1;                        # print OHLCV data as CSV
            elif opt == "-l": alist = 1;                        # print each OHLCV on new line
            elif opt == "-p": apipe = 1;                        # use named pipe connection
            elif opt == "-n": nmax  = int(arg);                 # Number of received candles
            elif opt == "-s": aSym  = str(arg);                 # Instrument Symbol
            elif opt == "-t":                                   # <timeframe>
                if (isArgInt(arg)) :                            # 
                    if int(arg) in d1.values() :                # check if '30' in d1
                        aTF  = int(arg)                         # convert '30' to 30 
                    else : 
                        printTFerr(arg,dTF)
                        continue
                else: 
                    if arg in d1.keys() :                       # check if 'M30' in d1 
                        aTF = d1.get(arg)                       # get 30 from 'M30'
                    else : 
                        printTFerr(arg,dTF)
                        continue
    #----------------------------------------------------------

    #client = mt.MtApi5Client()
    client = mt.MtApiClient()
    
    ip      = str(MTSERV)       # Host IP
    port    = MTPORT            # EA port
    sleeps  = 10                # Sleep this many times
    cnt     = 1                 # 

    print('\n  INFO: Attmpting to Connect to:  {}:{}'.format(ip,port))
    if (debug): print('  DBG : using {}'.format('a name pipe' if apipe else 'TCP IP'))

    try:
        while sleeps > 0:

            print('  [{}] Connecting '.format(cnt), end='')
            if (apipe == 0) :
                client.BeginConnect(ip, port)
            else : 
                client.BeginConnect(port)

            time.sleep(1)
            sleeps = sleeps - 1
            cnt += 1

            #------------------------------------
            # Check Connection state:
            #------------------------------------
            #   0 = Disconnected
            #   1 = Connecting
            #   2 = Connected
            #   3 = Failed
            #------------------------------------
            if client.ConnectionState == 0: print('Disconnected');  continue;
            if client.ConnectionState == 1: print('.', end='');     continue;
            if client.ConnectionState == 2: print('OK');            getCopyRates(nmax,aSym,aTF,alist,useCSV); break
            if client.ConnectionState == 3: print('FAILED');        break

            if (debug): print('  DBG: Unknown ConnectionState: ', client.ConnectionState)

    except Exception as e:
        print(e)

    client.BeginDisconnect()

    if (debug): print('\n  DBG: Disconnected!')
    print('\n  Done.')

#----------------------------------------------------------
#  END
#----------------------------------------------------------
#if __name__ == "__main__":
#    main()
#    sys.exit(0)
