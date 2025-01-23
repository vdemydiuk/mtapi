import asyncio
import json
import logging
from enum import IntEnum
from threading import Lock, Thread

from mt5commandtype import Mt5CommandType
from mt5enums import *
from mtrpcclient import MtRpcClient


class Mt5EventType(IntEnum):
    OnTradeTransaction = 1
    OnBookEvent = 2
    OnTick = 3
    OnLastTimeBar = 4
    OnLockTicks = 5


class Mt5Quote:
    def __init__(self, quote_json):
        self.instrument = quote_json["Instrument"]
        self.expert_handle = quote_json["ExpertHandle"]
        self.bid = quote_json["Tick"]["Bid"]
        self.ask = quote_json["Tick"]["Ask"]
        self.volume = quote_json["Tick"]["Volume"]

    def __repr__(self):
        return f"{self.expert_handle}-{self.instrument}: Bid = {self.bid}, Ask = {self.ask}, Volume = {self.volume}"


class Mql5Tick:
    def __init__(self, tick_json):
        self.bid = tick_json["Bid"]
        self.ask = tick_json["Ask"]
        self.last = tick_json["Last"]
        self.volume = tick_json["Volume"]

    def __repr__(self):
        return f"Bid = {self.bid}, Ask = {self.ask}, Last = {self.last}, Volume = {self.volume}"


class MqlRates:
    def __init__(self, mql_rates_json):
        self.time = mql_rates_json["mt_time"]
        self.open = mql_rates_json["open"]
        self.high = mql_rates_json["high"]
        self.low = mql_rates_json["low"]
        self.close = mql_rates_json["close"]
        self.tick_volume = mql_rates_json["tick_volume"]
        self.spread = mql_rates_json["spread"]
        self.real_volume = mql_rates_json["real_volume"]

    def __repr__(self):
        return f"time = {self.time}, open = {self.open}, high = {self.high}, low = {self.low}, close = {self.close}, tick_volume = {self.tick_volume}, spread = {self.spread}, real_volume = {self.real_volume}"


class MqlTradeTransaction:
    def __init__(self, mql_trade_transaction_json):
        self.deal = mql_trade_transaction_json["Deal"]
        self.order = mql_trade_transaction_json["Order"]
        self.symbol = mql_trade_transaction_json["Symbol"]
        self.transaction_type = mql_trade_transaction_json["Type"]
        self.order_type = mql_trade_transaction_json["OrderType"]
        self.order_state = mql_trade_transaction_json["OrderState"]
        self.deal_type = mql_trade_transaction_json["DealType"]
        self.time_type = mql_trade_transaction_json["TimeType"]
        self.price = mql_trade_transaction_json["Price"]
        self.price_trigger = mql_trade_transaction_json["PriceTrigger"]
        self.price_sl = mql_trade_transaction_json["PriceSl"]
        self.price_tp = mql_trade_transaction_json["PriceTp"]
        self.volume = mql_trade_transaction_json["Volume"]
        self.position = mql_trade_transaction_json["Position"]
        self.position_by = mql_trade_transaction_json["PositionBy"]
        self.time_expiration = mql_trade_transaction_json["MtTimeExpiration"]

    def __repr__(self):
        return (
            f"deal = {self.deal}, order = {self.order}, symbol = {self.symbol}, transaction_type = {self.transaction_type}, "
            f"order_type = {self.order_type}, order_state = {self.order_state}, deal_type = {self.deal_type}, time_type = {self.time_type}, "
            f"price = {self.price}, price_trigger = {self.price_trigger}, price_sl = {self.price_sl}, price_tp = {self.price_tp}, volume = {self.volume}, "
            f"position = {self.position}, position_by = {self.position_by}, time_expiration = {self.time_expiration}"
        )


class MqlTradeRequest:
    def __init__(self, mql_trade_request_json):
        self.action = mql_trade_request_json["Action"]
        self.magic = mql_trade_request_json["Magic"]
        self.order = mql_trade_request_json["Order"]
        self.symbol = mql_trade_request_json["Symbol"]
        self.volume = mql_trade_request_json["Volume"]
        self.price = mql_trade_request_json["Price"]
        self.stop_limit = mql_trade_request_json["Stoplimit"]
        self.sl = mql_trade_request_json["Sl"]
        self.tp = mql_trade_request_json["Tp"]
        self.deviation = mql_trade_request_json["Deviation"]
        self.order_type = mql_trade_request_json["Type"]
        self.type_filling = mql_trade_request_json["Type_filling"]
        self.type_time = mql_trade_request_json["Type_time"]
        self.expiration = mql_trade_request_json["MtExpiration"]
        self.comment = mql_trade_request_json["Comment"]
        # self.position = mql_trade_request_json["Position"]
        # self.position_by = mql_trade_request_json["PositionBy"]

    def __repr__(self):
        return (
            f"action = {self.action}, magic = {self.magic}, order = {self.order}, symbol = {self.symbol}, volume = {self.volume}, "
            f"price = {self.price}, stop_limit = {self.stop_limit}, sl = {self.sl}, tp = {self.tp}, deviation = {self.deviation}, "
            f"order_type = {self.order_type}, type_filling = {self.type_filling}, type_time = {self.type_time}, expiration = {self.expiration}, "
            f"comment = {self.comment}"
        )


class MqlTradeResult:
    def __init__(self, mql_trade_result_json):
        self.retcode = mql_trade_result_json["Retcode"]
        self.deal = mql_trade_result_json["Deal"]
        self.order = mql_trade_result_json["Order"]
        self.volume = mql_trade_result_json["Volume"]
        self.price = mql_trade_result_json["Price"]
        self.bid = mql_trade_result_json["Bid"]
        self.ask = mql_trade_result_json["Ask"]
        self.comment = mql_trade_result_json["Comment"]
        self.request_id = mql_trade_result_json["Request_id"]

    def __repr__(self):
        return (
            f"retcode = {self.retcode}, deal = {self.deal}, order = {self.order}, volume = {self.volume}, price = {self.price}, "
            f"bid = {self.bid}, ask = {self.ask}, comment = {self.comment}, request_id = {self.request_id}"
        )


class Mt5ApiClient:
    def __init__(self, address, port, callback=None):
        self.__address = address
        self.__port = port
        self.__callback = callback
        self.__logger = logging.getLogger(__name__)
        self.__rpcclient = MtRpcClient(self)
        self.__is_connected = False
        self.__quotes = dict()
        self.__experts = list()
        self.__lock = Lock()

    def __enter__(self):
        self.connect()
        return self

    def __exit__(self, *_):
        self.disconnect()

    def connect(self):
        self.__logger.info(f"Connecting to {self.__address}:{self.__port}")
        url = f"ws://{self.__address}:{self.__port}"
        self.__rpcclient.connect(url)
        experts = self.__rpcclient.request_expert_list()
        if experts is None:
            self.__rpcclient.disconnect()
            raise Exception("Failed to load expert list")
        self.__logger.info(f"loaded exerts {self.__experts}")
        for expert_handle in experts:
            quote = self.__get_quote(expert_handle)
            if quote is not None:
                self.__experts.append(expert_handle)
                self.__quotes[expert_handle] = quote
        self.__logger.info(f"loaded quotes {self.__quotes}")
        # TODO: send backtesting ready
        self.__event_loop = asyncio.new_event_loop()
        self.__event_thread = Thread(target=self.__event_thread_func)
        self.__event_thread.start()
        self.__is_connected = True

    def disconnect(self):
        self.__rpcclient.disconnect()
        self.__event_loop.call_soon_threadsafe(self.__event_loop.stop)
        self.__event_thread.join()
        self.__quotes.clear()
        self.__experts.clear()

    def is_connected(self):
        with self.__lock:
            return self.__is_connected

    def get_quotes(self):
        with self.__lock:
            return list(self.__quotes.values())

    def is_testing(self):
        return False

    # Account Information functions

    # AccountInfoDouble
    def account_info_double(self, property_id: ENUM_ACCOUNT_INFO_DOUBLE):
        cmd_params = {"PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.AccountInfoDouble, cmd_params)

    # AccountInfoInteger
    def account_info_integer(self, property_id: ENUM_ACCOUNT_INFO_INTEGER):
        cmd_params = {"PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.AccountInfoInteger, cmd_params)

    # AccountInfoString
    def account_info_string(self, property_id: ENUM_ACCOUNT_INFO_STRING):
        cmd_params = {"PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.AccountInfoString, cmd_params)

    # Timeseries and Indicators Access

    # SeriesInfoInteger
    def series_info_integer(self, symbol_name, timeframe: ENUM_TIMEFRAMES, prop_id: ENUM_SERIES_INFO_INTEGER):
        if symbol_name is None:
            symbol_name = ""
        cmd_params = {"Symbol": symbol_name, "Timeframe": timeframe, "PropId": prop_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.SeriesInfoInteger, cmd_params)

    # Bars
    def bars(self, symbol_name, timeframe: ENUM_TIMEFRAMES):
        if symbol_name is None:
            symbol_name = ""
        cmd_params = {"Symbol": symbol_name, "Timeframe": timeframe}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.Bars, cmd_params)

    # Bars (for a specified period)
    def bars_period(self, symbol_name, timeframe: ENUM_TIMEFRAMES, start_time: int, stop_time: int):
        if symbol_name is None:
            symbol_name = ""
        cmd_params = {"Symbol": symbol_name, "Timeframe": timeframe, "StartTime": start_time, "StopTime": stop_time}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.Bars2, cmd_params)

    # BarsCalculated
    def bars_calculated(self, indicator_handle: int):
        cmd_params = {"IndicatorHandle": indicator_handle}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.BarsCalculated, cmd_params)

    # CopyBuffer
    def copy_buffer(self):
        # TODO
        pass

    # CopyRates
    def copy_rates(self):
        # TODO
        pass

    # CopyTime
    def copy_time(self):
        # TODO
        pass

    # Copy Open
    def copy_open(self):
        # TODO
        pass

    # Copy High
    def copy_high(self):
        # TODO
        pass

    # CopyLow
    def copy_low(self):
        # TODO
        pass

    # CopyClose
    def copy_close(self):
        # TODO
        pass

    # CopyTickVolume
    def copy_tick_volume(self):
        # TODO
        pass

    # CopyRealVolume
    def copy_real_volume(self):
        # TODO
        pass

    # CopySpread
    def copy_spread(self):
        # TODO
        pass

    # CopyTicks
    def copy_ticks(self):
        # TODO
        pass

    # IndicatorCreate
    def indicator_create(
        self, symbol: str, period: ENUM_TIMEFRAMES, indicator_type: ENUM_INDICATOR, parameters: list = []
    ):
        cmd_params = {"Period": period, "IndicatorType": indicator_type}
        if symbol is not None:
            cmd_params["Symbol"] = symbol
        if len(parameters) != 0:
            cmd_params["Parameters"] = parameters
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.IndicatorCreate, cmd_params)

    # IndicatorRelease
    def indicator_release(self, indicator_handle: int):
        cmd_params = {"IndicatorHandle": indicator_handle}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.IndicatorRelease, cmd_params)

    # Market Info

    # SymbolsTotal
    def symbols_total(self, selected: bool):
        cmd_params = {"Selected": selected}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.SymbolsTotal, cmd_params)

    # SymbolName
    def symbol_name(self, pos: int, selected: bool):
        cmd_params = {"Pos": pos, "Selected": selected}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.SymbolName, cmd_params)

    # SymbolSelect
    def symbol_select(self, symbol_name: str, selected: bool):
        cmd_params = {"Symbol": symbol_name, "Selected": selected}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.SymbolSelect, cmd_params)

    # SymbolIsSynchronized
    def symbol_is_synchronized(self, symbol_name: str):
        cmd_params = {"Symbol": symbol_name}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.SymbolIsSynchronized, cmd_params)

    # SymbolInfoDouble
    def symbol_info_double(self, symbol_name: str, prop_id: ENUM_SYMBOL_INFO_DOUBLE):
        cmd_params = {"Symbol": symbol_name, "PropId": prop_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.SymbolInfoDouble, cmd_params)

    # SymbolInfoInteger
    def symbol_info_integer(self, symbol_name: str, prop_id: ENUM_SYMBOL_INFO_INTEGER):
        cmd_params = {"Symbol": symbol_name, "PropId": prop_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.SymbolInfoInteger, cmd_params)

    # SymbolInfoString
    def symbol_info_string(self, symbol_name: str, prop_id: ENUM_SYMBOL_INFO_STRING):
        cmd_params = {"Symbol": symbol_name, "PropId": prop_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.SymbolInfoString, cmd_params)

    # SymbolInoTick
    def symbol_info_tick(self, symbol_name: str):
        cmd_params = {"Symbol": symbol_name}
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.SymbolInfoTick, cmd_params)
        if res is not None and res["RetVal"] == True:
            return Mql5Tick(res["Result"])
        return None

    # SymbolInfoSessionQuote
    def symbol_info_session_quote(self, name: str, day_of_week: ENUM_DAY_OF_WEEK, session_index: int):
        cmd_params = {"Symbol": name, "DayOfWeek": day_of_week, "SessionIndex": session_index}
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.SymbolInfoSessionQuote, cmd_params)
        if res is not None and res["RetVal"] == True:
            return (res["Result"]["From"], res["Result"]["To"])
        return None

    # Private methods

    def __event_thread_func(self):
        self.__logger.debug(f"__event_thread started")
        asyncio.set_event_loop(self.__event_loop)
        self.__event_loop.run_forever()
        self.__logger.debug(f"__event_thread stopped")

    def __get_quote(self, expert_handle):
        response = self.__send_command(expert_handle, Mt5CommandType.GetQuote)
        quote = Mt5Quote(response) if response is not None else None
        return quote

    def __get_default_expert(self):
        with self.__lock:
            if len(self.__experts) > 0:
                return self.__experts[0]
        return 0

    def __send_command(self, expert_handle, command_type, payload=None):
        payload_json = None if payload is None else json.dumps(payload)
        response = self.__rpcclient.send_command(expert_handle, command_type, payload_json)
        if response is None:
            self.__logger.warning("Failed to send commad. Result is None")
            raise Exception("Failed to send commad. Result is None")
        print(f"response = {response}")
        response_json = json.loads(response)
        error_code = int(response_json["ErrorCode"])
        if error_code != 0:
            self.__logger.warning(f"send_command: ErrorCode = {response.ErrorCode}. {response.ErrorMessage}")
            raise Exception(f"Failed to send command: ErrorCode = {response.ErrorCode}. {response.ErrorMessage} ")
        return response_json["Value"]

    def __process_tick_event(self, payload):
        quote_json = json.loads(payload)
        if quote_json is not None:
            quote = Mt5Quote(quote_json)
            with self.__lock:
                self.__quotes[quote.expert_handle] = quote
            if self.__callback is not None:
                self.__callback.on_quote_update(quote)

    def __process_event_disconnect(self, error_msg=None):
        with self.__lock:
            self.__is_connected = False
        if self.__callback is not None:
            self.__callback.on_disconnect(error_msg)

    def __process_expert_added(self, expert_handle):
        quote = self.__get_quote(expert_handle)
        if quote is not None:
            with self.__lock:
                self.__quotes[expert_handle] = quote
                self.__experts.append(expert_handle)
            if self.__callback is not None:
                self.__callback.on_quote_added(quote)

    def __process_expert_removed(self, expert_handle):
        quote = None
        with self.__lock:
            self.__experts.remove(expert_handle)
            if expert_handle in self.__quotes:
                quote = self.__quotes.pop(expert_handle)
        if quote is not None and self.__callback is not None:
            self.__callback.on_quote_removed(quote)

    def __process_on_book_event(self, expert_handle, payload):
        book_event_json = json.loads(payload)
        if book_event_json is None:
            self.__logger.error("Failed to parse book event json")
            return
        symbol = book_event_json["Symbol"]
        if self.__callback is not None:
            self.__callback.on_book_event(expert_handle, symbol)

    def __process_on_last_time_bar(self, expert_handle, payload):
        last_time_bar_event_json = json.loads(payload)
        if last_time_bar_event_json is None:
            self.__logger.error("Failed to parse last time bar event json")
            return
        instrument = last_time_bar_event_json["Instrument"]
        rates = MqlRates(last_time_bar_event_json["Rates"])
        if self.__callback is not None:
            self.__callback.on_last_time_bar(expert_handle, instrument, rates)

    def __process_on_lock_tick(self, expert_handle, payload):
        # TODO: must be implemented
        self.__logger.warning(f"event type OnLockTicks is not supported. {expert_handle} - {payload}")

    def __process_on_trade_transaction(self, expert_handle, payload):
        trade_transaction_json = json.loads(payload)
        trade_transaction = MqlTradeTransaction(trade_transaction_json["Trans"])
        trade_request = MqlTradeRequest(trade_transaction_json["Request"])
        trade_result = MqlTradeResult(trade_transaction_json["Result"])
        if self.__callback is not None:
            self.__callback.on_trade_transaction(expert_handle, trade_transaction, trade_request, trade_result)

    # RPC event handlers

    def mt_rpc_on_event(self, expert_handle, event_type, payload):
        self.__logger.debug(f"received event from {expert_handle}: {event_type}, {payload}")
        mt_event_type = Mt5EventType(int(event_type))
        if mt_event_type == Mt5EventType.OnTick:
            self.__event_loop.call_soon_threadsafe(self.__process_tick_event, payload)
        elif mt_event_type == Mt5EventType.OnBookEvent:
            self.__event_loop.call_soon_threadsafe(self.__process_on_book_event, expert_handle, payload)
        elif mt_event_type == Mt5EventType.OnLastTimeBar:
            self.__event_loop.call_soon_threadsafe(self.__process_on_last_time_bar, expert_handle, payload)
        elif mt_event_type == Mt5EventType.OnLockTicks:
            self.__event_loop.call_soon_threadsafe(self.__process_on_lock_tick, expert_handle, payload)
        elif mt_event_type == Mt5EventType.OnTradeTransaction:
            self.__event_loop.call_soon_threadsafe(self.__process_on_trade_transaction, expert_handle, payload)
        else:
            self.__logger.warning(f"received unsupported event {event_type}")

    def mt_rcp_on_disconnect(self):
        self.__logger.info("normal disconnected")
        self.__event_loop.call_soon_threadsafe(self.__process_event_disconnect)

    def mt_rpc_on_connection_failed(self, error_msg=None):
        self.__logger.info(f"connection failed: {error_msg}")
        self.__event_loop.call_soon_threadsafe(self.__process_event_disconnect, error_msg)

    def mt_rpc_on_expert_added(self, expert_handle):
        self.__logger.info(f"expert added: {expert_handle}")
        self.__event_loop.call_soon_threadsafe(self.__process_expert_added, expert_handle)

    def mt_rpc_on_expert_removed(self, expert_handle):
        self.__logger.info(f"expert removed: {expert_handle}")
        self.__event_loop.call_soon_threadsafe(self.__process_expert_removed, expert_handle)
