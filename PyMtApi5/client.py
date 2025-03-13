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
    del __
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
            "SymbolInfoSessionQuote": self.process_symbol_info_session_quote,
            "SymbolInfoSessionTrade": self.process_symbol_info_session_trade,
            "MarketBookAdd": self.process_market_book_add,
            "MarketBookRelease": self.process_market_book_release,
            "MarketBookGet": self.process_market_book_get,
            "CopyBuffer": self.process_copy_buffer,
            "CopyRates": self.process_copy_rates,
            "CopyTime": self.process_copy_time,
            "CopyOpen": self.process_copy_open,
            "CopyHigh": self.process_copy_high,
            "CopyLow": self.process_copy_low,
            "CopyClose": self.process_copy_close,
            "CopyTickVolume": self.process_copy_tick_volume,
            "CopyRealVolume": self.process_copy_real_volume,
            "CopySpread": self.process_copy_spread,
            "CopyTicks": self.process_copy_ticks,
            "ChartId": self.process_chart_id,
            "ChartRedraw": self.process_chart_redraw,
            "ChartApplyTemplate": self.process_chart_apply_template,
            "ChartSaveTemplate": self.process_chart_save_template,
            "ChartWindowFind": self.process_chart_window_find,
            "ChartTimePriceToXY": self.process_chart_time_price_to_xy,
            "ChartXYToTimePrice": self.process_chart_xy_to_time_price,
            "ChartOpen": self.process_chart_open,
            "ChartFirst": self.process_chart_first,
            "ChartNext": self.process_chart_next,
            "ChartClose": self.process_chart_close,
            "ChartSymbol": self.process_chart_symbol,
            "ChartPeriod": self.process_chart_period,
            "ChartSetDouble": self.process_chart_set_double,
            "ChartSetInteger": self.process_chart_set_integer,
            "ChartSetString": self.process_chart_set_string,
            "ChartGetDouble": self.process_chart_get_double,
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
        if len(pieces) == 0 or len(pieces) > 2:
            print(f"! Invalid command format: {command.rstrip()}")
            return
        command_name = pieces[0].rstrip()
        if command_name not in self.cmd_functions:
            print(f"! Unknown command: '{command_name}'")
            return
        if len(pieces) == 1:
            pieces.append("")
        params = pieces[1].rstrip()
        try:
            self.cmd_functions[command_name](mtapi, params)
        except Exception as e:
            print(f"Failed to process command {command.rstrip()}: {e}")

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

    def process_symbol_info_session_quote(self, mtapi, parameters):
        pieces = parameters.split(" ", 2)
        if len(pieces) != 3 or not pieces[0] or not pieces[1] or not pieces[2]:
            print(f"! Invalid parameters for command SymbolInfoSessionQuote: {parameters}")
            return
        symbol = pieces[0]
        day_of_week = mt5enums.ENUM_DAY_OF_WEEK(int(pieces[1]))
        session_index = int(pieces[2])
        result = mtapi.symbol_info_session_quote(symbol, day_of_week, session_index)
        print(f"> SymbolInfoSessionQuote: response = {result}")

    def process_symbol_info_session_trade(self, mtapi, parameters):
        pieces = parameters.split(" ", 2)
        if len(pieces) != 3 or not pieces[0] or not pieces[1] or not pieces[2]:
            print(f"! Invalid parameters for command SymbolInfoSessionTrade: {parameters}")
            return
        symbol = pieces[0]
        day_of_week = mt5enums.ENUM_DAY_OF_WEEK(int(pieces[1]))
        session_index = int(pieces[2])
        result = mtapi.symbol_info_session_trade(symbol, day_of_week, session_index)
        print(f"> SymbolInfoSessionTrade: response = {result}")

    def process_market_book_add(self, mtapi, parameters):
        if len(parameters) == 0:
            print(f"! Invalid parameters for command MarketBookAdd: {parameters} - {len(parameters)}")
            return
        symbol = parameters
        result = mtapi.market_book_add(symbol)
        print(f"> MarketBookAdd: response = {result}")

    def process_market_book_release(self, mtapi, parameters):
        if len(parameters) == 0:
            print(f"! Invalid parameters for command MarketBookRelease: {parameters} - {len(parameters)}")
            return
        symbol = parameters
        result = mtapi.market_book_release(symbol)
        print(f"> MarketBookRelease: response = {result}")

    def process_market_book_get(self, mtapi, parameters):
        if len(parameters) == 0:
            print(f"! Invalid parameters for command MarketBookGet: {parameters} - {len(parameters)}")
            return
        symbol = parameters
        result = mtapi.market_book_get(symbol)
        print(f"> MarketBookGet: response = {result}")

    def process_copy_buffer(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command CopyBuffer: {parameters}")
            return
        indicator_handle = int(pieces[0])
        buffer_num = int(pieces[1])
        start_pos = int(pieces[2])
        count = int(pieces[3])
        result = mtapi.copy_buffer(indicator_handle, buffer_num, start_pos, count)
        print(f"> CopyBuffer: response = {result}")

    def process_copy_rates(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command CopyRates: {parameters}")
            return
        symbol_name = pieces[0]
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        start_pos = int(pieces[2])
        count = int(pieces[3])
        result = mtapi.copy_rates(symbol_name, timeframe, start_pos, count)
        print(f"> CopyRates: response = {result}")

    def process_copy_time(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command CopyTime: {parameters}")
            return
        symbol_name = pieces[0]
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        start_pos = int(pieces[2])
        count = int(pieces[3])
        result = mtapi.copy_time(symbol_name, timeframe, start_pos, count)
        print(f"> CopyTime: response = {result}")

    def process_copy_open(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command CopyOpen: {parameters}")
            return
        symbol_name = pieces[0]
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        start_pos = int(pieces[2])
        count = int(pieces[3])
        result = mtapi.copy_open(symbol_name, timeframe, start_pos, count)
        print(f"> CopyOpen: response = {result}")

    def process_copy_high(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command CopyHigh: {parameters}")
            return
        symbol_name = pieces[0]
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        start_pos = int(pieces[2])
        count = int(pieces[3])
        result = mtapi.copy_high(symbol_name, timeframe, start_pos, count)
        print(f"> CopyHigh: response = {result}")

    def process_copy_low(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command CopyLow: {parameters}")
            return
        symbol_name = pieces[0]
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        start_pos = int(pieces[2])
        count = int(pieces[3])
        result = mtapi.copy_low(symbol_name, timeframe, start_pos, count)
        print(f"> CopyLow: response = {result}")

    def process_copy_close(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command CopyClose: {parameters}")
            return
        symbol_name = pieces[0]
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        start_pos = int(pieces[2])
        count = int(pieces[3])
        result = mtapi.copy_close(symbol_name, timeframe, start_pos, count)
        print(f"> CopyClose: response = {result}")

    def process_copy_tick_volume(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command CopyTickVolume: {parameters}")
            return
        symbol_name = pieces[0]
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        start_pos = int(pieces[2])
        count = int(pieces[3])
        result = mtapi.copy_tick_volume(symbol_name, timeframe, start_pos, count)
        print(f"> CopyTickVolume: response = {result}")

    def process_copy_real_volume(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command CopyRealVolume: {parameters}")
            return
        symbol_name = pieces[0]
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        start_pos = int(pieces[2])
        count = int(pieces[3])
        result = mtapi.copy_real_volume(symbol_name, timeframe, start_pos, count)
        print(f"> CopyRealVolume: response = {result}")

    def process_copy_spread(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command CopySpread: {parameters}")
            return
        symbol_name = pieces[0]
        timeframe = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        start_pos = int(pieces[2])
        count = int(pieces[3])
        result = mtapi.copy_spread(symbol_name, timeframe, start_pos, count)
        print(f"> CopySpread: response = {result}")

    def process_copy_ticks(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command CopyTicks: {parameters}")
            return
        symbol_name = pieces[0]
        timeframe = mt5enums.CopyTicksFlag(int(pieces[1]))
        from_date = int(pieces[2])
        count = int(pieces[3])
        result = mtapi.copy_ticks(symbol_name, timeframe, from_date, count)
        print(f"> CopyTicks: response = {result}")

    def process_chart_id(self, mtapi, parameters):
        result = mtapi.chart_id(int(parameters))
        print(f"> ChatId: response = {result}")

    def process_chart_redraw(self, mtapi, parameters):
        mtapi.chart_redraw(int(parameters))
        print(f"> ChartRedraw: success")

    def process_chart_apply_template(self, mtapi, parameters):
        pieces = parameters.split(" ", 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1]:
            print(f"! Invalid parameters for command ChartApplyTemplate: {parameters}")
            return
        result = mtapi.chart_apply_template(int(pieces[0]), pieces[1])
        print(f"> ChartApplyTemplate: response = {result}")

    def process_chart_save_template(self, mtapi, parameters):
        pieces = parameters.split(" ", 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1]:
            print(f"! Invalid parameters for command ChartSaveTemplate: {parameters}")
            return
        result = mtapi.chart_save_template(int(pieces[0]), pieces[1])
        print(f"> ChartSaveTemplate: response = {result}")

    def process_chart_window_find(self, mtapi, parameters):
        pieces = parameters.split(" ", 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1]:
            print(f"! Invalid parameters for command ChartWindowFind: {parameters}")
            return
        result = mtapi.chart_window_find(int(pieces[0]), pieces[1])
        print(f"> ChartWindowFind: response = {result}")

    def process_chart_time_price_to_xy(self, mtapi, parameters):
        pieces = parameters.split(" ", 3)
        if len(pieces) != 4 or not pieces[0] or not pieces[1] or not pieces[2] or not pieces[3]:
            print(f"! Invalid parameters for command ChartTimePriceToXY: {parameters}")
            return
        result = mtapi.chart_time_price_to_xy(int(pieces[0]), int(pieces[1]), int(pieces[2]), float(pieces[3]))
        print(f"> ChartTimePriceToXY: response = {result}")

    def process_chart_xy_to_time_price(self, mtapi, parameters):
        pieces = parameters.split(" ", 2)
        if len(pieces) != 3 or not pieces[0] or not pieces[1] or not pieces[2]:
            print(f"! Invalid parameters for command ChartXYToTimePrice: {parameters}")
            return
        result = mtapi.chart_xy_to_time_price(int(pieces[0]), int(pieces[1]), int(pieces[2]))
        print(f"> ChartXYToTimePrice: response = {result}")

    def process_chart_open(self, mtapi, parameters):
        pieces = parameters.split(" ", 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1]:
            print(f"! Invalid parameters for command ChartOpen: {parameters}")
            return
        period = mt5enums.ENUM_TIMEFRAMES(int(pieces[1]))
        result = mtapi.chart_open(pieces[0], period)
        print(f"> ChartOpen: response = {result}")

    def process_chart_first(self, mtapi, _):
        result = mtapi.chart_first()
        print(f"> ChartFirst: response = {result}")

    def process_chart_next(self, mtapi, parameters):
        result = mtapi.chart_next(int(parameters))
        print(f"> ChartNext: response = {result}")

    def process_chart_close(self, mtapi, parameters):
        result = mtapi.chart_close(int(parameters))
        print(f"> ChartClose: response = {result}")

    def process_chart_symbol(self, mtapi, parameters):
        result = mtapi.chart_symbol(int(parameters))
        print(f"> ChartSymbol: response = {result}")

    def process_chart_period(self, mtapi, parameters):
        result = mtapi.chart_period(int(parameters))
        print(f"> ChartPeriod: response = {result}")

    def process_chart_set_double(self, mtapi, parameters):
        pieces = parameters.split(" ", 2)
        if len(pieces) != 3 or not pieces[0] or not pieces[1] or not pieces[2]:
            print(f"! Invalid parameters for command ChartSetDouble: {parameters}")
            return
        prop_id = mt5enums.ENUM_CHART_PROPERTY_DOUBLE(int(pieces[1]))
        result = mtapi.chart_set_double(int(pieces[0]), prop_id, float(pieces[2]))
        print(f"> ChartSetDouble: response = {result}")

    def process_chart_set_integer(self, mtapi, parameters):
        pieces = parameters.split(" ", 2)
        if len(pieces) != 3 or not pieces[0] or not pieces[1] or not pieces[2]:
            print(f"! Invalid parameters for command ChartSetInteger: {parameters}")
            return
        prop_id = mt5enums.ENUM_CHART_PROPERTY_INTEGER(int(pieces[1]))
        result = mtapi.chart_set_integer(int(pieces[0]), prop_id, int(pieces[2]))
        print(f"> ChartSetInteger: response = {result}")

    def process_chart_set_string(self, mtapi, parameters):
        pieces = parameters.split(" ", 2)
        if len(pieces) != 3 or not pieces[0] or not pieces[1] or not pieces[2]:
            print(f"! Invalid parameters for command ChartSetString: {parameters}")
            return
        prop_id = mt5enums.ENUM_CHART_PROPERTY_STRING(int(pieces[1]))
        result = mtapi.chart_set_string(int(pieces[0]), prop_id, pieces[2])
        print(f"> ChartSetString: response = {result}")

    def process_chart_get_double(self, mtapi, parameters):
        pieces = parameters.split(" ", 2)
        if len(pieces) != 3 or not pieces[0] or not pieces[1] or not pieces[2]:
            print(f"! Invalid parameters for command ChartGetDouble: {parameters}")
            return
        prop_id = mt5enums.ENUM_CHART_PROPERTY_DOUBLE(int(pieces[1]))
        result = mtapi.chart_get_double(int(pieces[0]), prop_id, int(pieces[2]))
        print(f"> ChartGetDouble: response = {result}")

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
