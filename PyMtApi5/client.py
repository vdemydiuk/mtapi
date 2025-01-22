import logging
import os
import os.path
import signal
import sys
import time
from functools import partial
from threading import Thread

import mt5enums
from mt5apiclient import Mt5ApiClient

logger = logging.getLogger(__name__)


def signal_handler(mtapi, _, __):
    if mtapi.is_connected():
        mtapi.disconnect()


class Mt5ApiApp:
    def __init__(self, address, port):
        self.__address = address
        self.__port = port
        self.cmd_functions = {
            "AccountInfoDouble": self.process_account_info_double,
            "AccountInfoInteger": self.process_account_info_integer,
            "AccountInfoString": self.process_account_info_string,
            "SeriesInfoInteger": self.process_series_info_integer,
            "Bars": self.process_bars,
            "BarsPeriod": self.process_bars_period,
            "BarsCalculated": self.process_bars_calculated,
            "IndicatorCreate": self.process_indicator_create,
            "IndicatorRelease": self.process_indicator_release,
            "SymbolsTotal": self.process_symbols_total,
            "SymbolName": self.process_symbol_name,
            "SymbolSelect": self.process_symbol_select,
            "SymbolIsSynchronized": self.process_symbol_is_synchronized,
            "SymbolInfoDouble": self.process_symbol_info_double,
            "SymbolInfoInteger": self.process_symbol_info_integer,
            "SymbolInfoString": self.process_symbol_info_string,
            "SymbolInfoTick": self.process_symbol_info_tick,
        }

    def on_disconnect(self, error_msg=None):
        if error_msg is not None:
            print(f"> Disconnected with error: {error_msg}")
        else:
            print("> Normal disconnected")
        os.kill(os.getpid(), signal.SIGINT)

    def on_quote_update(self, quote):
        print(f"> update quote: {quote}")

    def on_quote_added(self, quote):
        print(f"> added quote: {quote}")

    def on_quote_removed(self, quote):
        print(f"> removed quote: {quote}")

    def on_book_event(self, expert_handle, symbol):
        print(f"> received book event: {expert_handle} - {symbol}")

    def on_last_time_bar(self, expert_handle, instrument, rates):
        print(f"> received last time bar event: {expert_handle} - {instrument}, {rates}")

    def on_trade_transaction(self, expert_handle, trade_transaction, trade_request, trade_result):
        print(
            f"> received trade transaction event: {expert_handle} - {trade_transaction}, {trade_request}, {trade_result}"
        )

    def process_command(self, mtapi, command):
        pieces = command.split(" ", 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1]:
            print(f"! Invalid command format: {command}")
            return
        if pieces[0] not in self.cmd_functions:
            print(f"! Unknown command: {pieces[0]}")
            return
        params = pieces[1].rstrip()
        self.cmd_functions[pieces[0]](mtapi, params)

    def process_account_info_double(self, mtapi, parameters):
        property_id = mt5enums.ENUM_ACCOUNT_INFO_DOUBLE(int(parameters))
        result = mtapi.account_info_double(property_id)
        print(f"> AccountInfoDouble {property_id}: result = {result}")

    def process_account_info_integer(self, mtapi, parameters):
        property_id = mt5enums.ENUM_ACCOUNT_INFO_INTEGER(int(parameters))
        value = mtapi.account_info_integer(property_id)
        print(f"> AccountInfoInteger {property_id}: response = {value}")

    def process_account_info_string(self, mtapi, parameters):
        property_id = mt5enums.ENUM_ACCOUNT_INFO_STRING(int(parameters))
        result = mtapi.account_info_string(property_id)
        print(f"> AccountInfoString {property_id}: result = {result}")

    def process_series_info_integer(self, mtpapi, parameters):
        pieces = parameters.split(" ", 2)
        if len(pieces) != 3 or not pieces[0] or not pieces[1] or not pieces[2]:
            print(f"! Invalid parameters for command SeriesInfoInteger: {parameters}")
            return
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        prop_id = mt5enums.ENUM_SERIES_INFO_INTEGER(int(pieces[2]))
        result = mtpapi.series_info_integer(pieces[0], timeframe, prop_id)
        print(f"> SeriesInfoInteger: result = {result}")

    def process_bars(self, mtpapi, parameters):
        pieces = parameters.split(" ", 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1]:
            print(f"! Invalid parameters for command Bars: {parameters}")
            return
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        result = mtpapi.bars(pieces[0], timeframe)
        print(f"> Bars: result = {result}")

    def process_bars_period(self, mtpapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command BarsPeriod: {parameters}")
            return
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        start_time = int(pieces[2])
        stop_time = int(pieces[3])
        result = mtpapi.bars_period(pieces[0], timeframe, start_time, stop_time)
        print(f"> Bars: result = {result}")

    def process_bars_calculated(self, mtpapi, parameters):
        if not parameters:
            print(f"! Invalid parameters for command BarsCalculated: {parameters}")
            return
        indicator_handle = int(parameters)
        result = mtpapi.bars_calculated(indicator_handle)
        print(f"> BarsCalculated: result = {result}")

    def process_indicator_create(self, mtpapi, parameters):
        pieces = parameters.split(" ")
        if len(pieces) != 3 or not pieces[0] or not pieces[1] or not pieces[2]:
            print(f"! Invalid parameters for command IndicatorCreate: {parameters}")
            return
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        indicator_type = mt5enums.ENUM_INDICATOR(int(pieces[2]))
        result = mtpapi.indicator_create(pieces[0], timeframe, indicator_type)
        print(f"> IndicatorCreate: result = {result}")

    def process_indicator_release(self, mtpapi, parameters):
        if not parameters:
            print(f"! Invalid parameters for command IndicatorRelease: {parameters}")
            return
        indicator_handle = int(parameters)
        result = mtpapi.indicator_release(indicator_handle)
        print(f"> IndicatorRelease: response = {result}")

    def process_symbols_total(self, mtpapi, parameters):
        if not parameters or len(parameters) == 0:
            print(f"! Invalid parameters for command SymbolsTotal: {parameters}")
            return
        selected = parameters == "True"
        result = mtpapi.symbols_total(selected)
        print(f"> SymbolsTotal: response = {result}")

    def process_symbol_name(self, mtpapi, parameters):
        pieces = parameters.split(" ", 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1] or len(pieces[1]) == 0:
            print(f"! Invalid parameters for command SymbolName: {parameters}")
            return
        pos = int(pieces[0])
        selected = pieces[1] == "True"
        result = mtpapi.symbol_name(pos, selected)
        print(f"> SymbolName: response = {result}")

    def process_symbol_select(self, mtpapi, parameters):
        pieces = parameters.split(" ", 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1] or len(pieces[1]) == 0:
            print(f"! Invalid parameters for command SymbolSelect: {parameters}")
            return
        selected = pieces[1] == "True"
        result = mtpapi.symbol_select(pieces[0], selected)
        print(f"> SymbolSelect: response = {result}")

    def process_symbol_is_synchronized(self, mtpapi, parameters):
        if not parameters or len(parameters) == 0:
            print(f"! Invalid parameters for command SymbolIsSynchronized: {parameters}")
            return
        symbol = parameters
        result = mtpapi.symbol_is_synchronized(symbol)
        print(f"> SymbolIsSynchronized: response = {result}")

    def process_symbol_info_double(self, mtapi, parameters):
        pieces = parameters.split(" ", 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1]:
            print(f"! Invalid parameters for command SymbolInfoDouble: {parameters}")
            return
        symbol = pieces[0]
        prop_id = mt5enums.ENUM_SYMBOL_INFO_DOUBLE(int(pieces[1]))
        result = mtapi.symbol_info_double(symbol, prop_id)
        print(f"> SymbolInfoDouble: response = {result}")

    def process_symbol_info_integer(self, mtapi, parameters):
        pieces = parameters.split(" ", 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1]:
            print(f"! Invalid parameters for command SymbolInfoInteger: {parameters}")
            return
        symbol = pieces[0]
        prop_id = mt5enums.ENUM_SYMBOL_INFO_INTEGER(int(pieces[1]))
        result = mtapi.symbol_info_integer(symbol, prop_id)
        print(f"> SymbolInfoInteger: response = {result}")

    def process_symbol_info_string(self, mtapi, parameters):
        pieces = parameters.split(" ", 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1]:
            print(f"! Invalid parameters for command SymbolInfoString: {parameters}")
            return
        symbol = pieces[0]
        prop_id = mt5enums.ENUM_SYMBOL_INFO_STRING(int(pieces[1]))
        result = mtapi.symbol_info_string(symbol, prop_id)
        print(f"> SymbolInfoString: response = {result}")

    def process_symbol_info_tick(self, mtapi, parameters):
        if len(parameters) == 0:
            print(f"! Invalid parameters for command SymbolInfoTick: {parameters} - {len(parameters)}")
            return
        symbol = parameters
        result = mtapi.symbol_info_tick(symbol)
        print(f"> SymbolInfoTick: response = {result}")

    def mtapi_command_thread(self, mtapi):
        while mtapi.is_connected():
            filename = "client.cmd"
            if os.path.isfile(filename):
                f = open("client.cmd", "r")
                command = f.read()
                f.close()
                os.remove(filename)
                self.process_command(mtapi, command)
            time.sleep(0.5)

    def run(self):
        with Mt5ApiClient(self.__address, self.__port, self) as mtapi:
            print(f"> Connected to {self.__address}:{self.__port}")
            signal.signal(signal.SIGINT, partial(signal_handler, mtapi))
            quotes = mtapi.get_quotes()
            print(f"> quotes: {quotes}")
            command_thread = Thread(target=self.mtapi_command_thread, args=(mtapi,))
            command_thread.start()
            while mtapi.is_connected():
                signal.pause()
            command_thread.join()


def main():
    logging.basicConfig(filename="client.log", filemode="w", level=logging.DEBUG)
    logger.info("Started")

    args_num = len(sys.argv)
    if args_num != 3:
        print("Incorrect arguments. For using input:\n\tclient <address> <port>")
        exit(1)

    address = sys.argv[1]
    port = int(sys.argv[2])

    app = Mt5ApiApp(address, port)
    app.run()


if __name__ == "__main__":
    main()
