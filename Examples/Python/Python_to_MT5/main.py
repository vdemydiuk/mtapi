import sys
import clr
import time


sys.path.append(r"C:\Program Files\MtApi5")
asm = clr.AddReference("MtApi5")
import MtApi5 as mt5


def testCopyRates():
    symbol = str('EURUSD')
    startPos = 1
    count = 100
    rates = []
    tf = mt5.ENUM_TIMEFRAMES.PERIOD_M5
    out_dummy = []
    try:
        result_count, rates_out = client.CopyRates(symbol, tf, startPos, count, out_dummy)
    except Exception as e:
        print(e)
        print('Error in CopyRates = %s\n ', e)
        exit()

    print("CopyRates: result_count = ", result_count)




if __name__ == '__main__':

    client = mt5.MtApi5Client()
    ip = str('192.168.178.15')
    port = 8300
    sleeps = 10
    try:

        while sleeps > 0:

            print("Connecting...")
            client.BeginConnect(ip, port)
            time.sleep(1)

            sleeps = sleeps - 1

            if client.ConnectionState == 1:
                print("Connected")
                testCopyRates()
                break
            if client.ConnectionState == 0:
                print("Connecting...")
                continue
            if client.ConnectionState == 4:
                print("Connection failed")
                break


    except Exception as e:
        print(e)

    client.BeginDisconnect()
