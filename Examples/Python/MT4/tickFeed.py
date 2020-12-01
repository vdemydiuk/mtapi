#!/usr/bin/env python3
# tickFeed.py - A PoC listener class to obtain QuoteUpdate the .NET proxy for OnTick().
#----------------------------------------------------------------------
# [0] https://github.com/eabase/mt4pycon
# [1] https://github.com/vdemydiuk/mtapi
#----------------------------------------------------------------------
import os, sys, clr
import time

#sys.path.append(r"C:\Program Files\MtApi")         # MT5
#sys.path.append(r"C:\Program Files (x86)\MtApi")   # MT4
sys.path.append(r".\libs")                          # Adjust this to location of MtApi.DLL
# Load the .NET assembly
#clr.AddReference('MtApi')
asm = clr.AddReference('MtApi')
import MtApi as mt

#  Constants
res = 0

#  MT API Constants
MODE_DIGITS     = 12    # MarketInfo(sym ,MODE_DIGITS)              # MT4:  --> 5.0
SYMBOL_DIGITS   = 17    # SymbolInfoInteger(sym, SYMBOL_DIGITS)     # MT4:  --> 5
SYMBOL_POINT    = 16    # SymbolInfoDouble(sym, SYMBOL_POINT)       # MT4:  --> 1e-05

#----------------------------------------------------------------------
#  Helpers...
#----------------------------------------------------------------------
# _apiClient_QuoteUpdate(object sender, MtQuoteEventArgs e)
def printTick1(source, sym, bid, ask) :
    global symPt        # 0.00001
    spread = round((ask - bid)/symPt)
    qstr = '{}  {:.5f} {:.5f}  {:d}'.format(sym,bid,ask,spread)
    print(qstr)

#----------------------------------------------------------------------
#  MAIN
#----------------------------------------------------------------------
# Setup .NET API bridge connection
mtc = mt.MtApiClient()

print('Connecting...', end='')
res = mtc.BeginConnect('127.0.0.1', 8222);

time.sleep(1.0)
if (mtc.IsConnected()) : 
    mtc.PlaySound("connect")
    print('ok')
else : 
    print('failed')
    sys.exit(1)

# Get symbol Point() value:
sym = str(mtc.ChartSymbol(0))                       # 0 is for current chart!
symPt = mtc.SymbolInfoDouble(sym, SYMBOL_POINT)     # MT4: --> 1e-05 = 0.00001 (EURUSD)

#--------------------------------------
# Register and use the listener
#--------------------------------------
# Available Events:
#   ConnectionStateChanged
#   OnChartEvent
#   OnLastTimeBar
#   QuoteAdded
#   QuoteRemoved
#   QuoteUpdate
#   QuoteUpdated
#--------------------------------------
print('Registering listener...',end='')

mtc.QuoteUpdated += printTick1      # 3-args

print('ok\n')

while 1:
    pass
    try: 
        time.sleep(5)
    except KeyboardInterrupt:
        print('\n  Break!')
        break

if (mtc.IsConnected()) :
    mtc.PlaySound("tick")
    mtc.BeginDisconnect()
print('\n  Done!')

sys.exit(2)

#----------------------------------------------------------------------
#  END
#----------------------------------------------------------------------
