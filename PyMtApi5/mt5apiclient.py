import asyncio
import json
import logging
from enum import IntEnum
from threading import Lock, Thread

from .mt5commandtype import Mt5CommandType
from .mt5enums import *
from .mtrpcclient import MtRpcClient, MtRpcError


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


class MqlTick:
    def __init__(self, tick_json):
        self.bid = tick_json["Bid"]
        self.ask = tick_json["Ask"]
        self.last = tick_json["Last"]
        self.volume = tick_json["Volume"]
        self.time = tick_json["Time"]

    def __repr__(self):
        return f"Bid = {self.bid}, Ask = {self.ask}, Last = {self.last}, Volume = {self.volume}, Time = {self.time}"


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
    """Trade request. Build it with keyword arguments to pass to order_send/order_send_async/order_check,
    or parse it from a JSON dict received in a trade transaction event."""

    def __init__(
        self,
        mql_trade_request_json=None,
        *,
        action: ENUM_TRADE_REQUEST_ACTIONS = 0,
        magic: int = 0,
        order: int = 0,
        symbol: str = "",
        volume: float = 0.0,
        price: float = 0.0,
        stop_limit: float = 0.0,
        sl: float = 0.0,
        tp: float = 0.0,
        deviation: int = 0,
        order_type: ENUM_ORDER_TYPE = 0,
        type_filling: ENUM_ORDER_TYPE_FILLING = 0,
        type_time: ENUM_ORDER_TYPE_TIME = 0,
        expiration: int = 0,
        comment: str = "",
        position: int = 0,
        position_by: int = 0,
    ):
        if mql_trade_request_json is not None:
            j = mql_trade_request_json
            action = j["Action"]
            magic = j["Magic"]
            order = j["Order"]
            symbol = j["Symbol"]
            volume = j["Volume"]
            price = j["Price"]
            stop_limit = j["Stoplimit"]
            sl = j["Sl"]
            tp = j["Tp"]
            deviation = j["Deviation"]
            order_type = j["Type"]
            type_filling = j["Type_filling"]
            type_time = j["Type_time"]
            expiration = j["MtExpiration"]
            comment = j["Comment"]
            position = j.get("Position", 0)
            position_by = j.get("PositionBy", 0)
        self.action = action
        self.magic = magic
        self.order = order
        self.symbol = symbol
        self.volume = volume
        self.price = price
        self.stop_limit = stop_limit
        self.sl = sl
        self.tp = tp
        self.deviation = deviation
        self.order_type = order_type
        self.type_filling = type_filling
        self.type_time = type_time
        self.expiration = expiration
        self.comment = comment
        self.position = position
        self.position_by = position_by

    def to_dict(self):
        return {
            "Action": int(self.action),
            "Magic": self.magic,
            "Order": self.order,
            "Symbol": self.symbol,
            "Volume": self.volume,
            "Price": self.price,
            "Stoplimit": self.stop_limit,
            "Sl": self.sl,
            "Tp": self.tp,
            "Deviation": self.deviation,
            "Type": int(self.order_type),
            "Type_filling": int(self.type_filling),
            "Type_time": int(self.type_time),
            "MtExpiration": self.expiration,
            "Comment": self.comment,
            "Position": self.position,
            "PositionBy": self.position_by,
        }

    def __repr__(self):
        return (
            f"action = {self.action}, magic = {self.magic}, order = {self.order}, symbol = {self.symbol}, volume = {self.volume}, "
            f"price = {self.price}, stop_limit = {self.stop_limit}, sl = {self.sl}, tp = {self.tp}, deviation = {self.deviation}, "
            f"order_type = {self.order_type}, type_filling = {self.type_filling}, type_time = {self.type_time}, expiration = {self.expiration}, "
            f"comment = {self.comment}, position = {self.position}, position_by = {self.position_by}"
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


class MqlTradeCheckResult:
    def __init__(self, mql_trade_check_result_json):
        j = mql_trade_check_result_json
        self.retcode = j["Retcode"]
        self.balance = j["Balance"]
        self.equity = j["Equity"]
        self.profit = j["Profit"]
        self.margin = j["Margin"]
        self.margin_free = j["Margin_free"]
        self.margin_level = j["Margin_level"]
        self.comment = j["Comment"]

    def __repr__(self):
        return (
            f"retcode = {self.retcode}, balance = {self.balance}, equity = {self.equity}, profit = {self.profit}, "
            f"margin = {self.margin}, margin_free = {self.margin_free}, margin_level = {self.margin_level}, "
            f"comment = {self.comment}"
        )


class MqlBookInfo:
    def __init__(self, mql_book_info):
        self.book_type = ENUM_BOOK_TYPE(mql_book_info["type"])
        self.price = mql_book_info["price"]
        self.volume = mql_book_info["volume"]
        self.volume_real = mql_book_info["volume_real"]

    def __repr__(self):
        return f"book_type = {self.book_type}, price = {self.price}, volume = {self.volume}, volume_real = {self.volume_real}"


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
        # The event loop must be running before the RPC client connects: the server starts
        # pushing events (ticks, etc.) right after the connection is opened, and they are
        # dispatched to this loop from the RPC receive thread.
        self.__event_loop = asyncio.new_event_loop()
        self.__event_thread = Thread(target=self.__event_thread_func)
        self.__event_thread.start()
        try:
            url = f"ws://{self.__address}:{self.__port}"
            self.__rpcclient.connect(url)
            experts = self.__rpcclient.request_expert_list()
            if experts is None:
                self.__rpcclient.disconnect()
                raise Exception("Failed to load expert list")
        except Exception:
            self.__stop_event_thread()
            raise
        self.__logger.info(f"loaded experts {experts}")
        for expert_handle in experts:
            quote = self.__get_quote(expert_handle)
            if quote is not None:
                with self.__lock:
                    self.__experts.append(expert_handle)
                    self.__quotes[expert_handle] = quote
        self.__logger.info(f"loaded quotes {self.__quotes}")
        # TODO: send backtesting ready
        with self.__lock:
            self.__is_connected = True

    def disconnect(self):
        self.__rpcclient.disconnect()
        self.__stop_event_thread()
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

    # Trade functions

    @staticmethod
    def __func_result(res, result_type):
        """Converts a FuncResult reply to (ret_val, result_object_or_None)."""
        if res is None:
            return False, None
        result = res.get("Result")
        return bool(res["RetVal"]), (result_type(result) if result is not None else None)

    # OrderSend
    def order_send(self, request: MqlTradeRequest):
        """Returns (ok, MqlTradeResult | None)."""
        cmd_params = {"TradeRequest": request.to_dict()}
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.OrderSend, cmd_params)
        return self.__func_result(res, MqlTradeResult)

    # OrderSendAsync
    def order_send_async(self, request: MqlTradeRequest):
        """Returns (ok, MqlTradeResult | None)."""
        cmd_params = {"TradeRequest": request.to_dict()}
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.OrderSendAsync, cmd_params)
        return self.__func_result(res, MqlTradeResult)

    # OrderCheck
    def order_check(self, request: MqlTradeRequest):
        """Returns (ok, MqlTradeCheckResult | None)."""
        cmd_params = {"TradeRequest": request.to_dict()}
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.OrderCheck, cmd_params)
        return self.__func_result(res, MqlTradeCheckResult)

    # OrderCalcMargin
    def order_calc_margin(self, action: ENUM_ORDER_TYPE, symbol: str, volume: float, price: float):
        """Returns the required margin, or None on failure."""
        cmd_params = {"Action": int(action), "Symbol": symbol, "Volume": volume, "Price": price}
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.OrderCalcMargin, cmd_params)
        if res is not None and res["RetVal"]:
            return res["Result"]
        return None

    # OrderCalcProfit
    def order_calc_profit(
        self, action: ENUM_ORDER_TYPE, symbol: str, volume: float, price_open: float, price_close: float
    ):
        """Returns the calculated profit, or None on failure."""
        cmd_params = {
            "Action": int(action),
            "Symbol": symbol,
            "Volume": volume,
            "PriceOpen": price_open,
            "PriceClose": price_close,
        }
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.OrderCalcProfit, cmd_params)
        if res is not None and res["RetVal"]:
            return res["Result"]
        return None

    # PositionsTotal
    def positions_total(self):
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionsTotal)

    # PositionGetSymbol
    def position_get_symbol(self, index: int):
        cmd_params = {"Index": index}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionGetSymbol, cmd_params)

    # PositionGetTicket
    def position_get_ticket(self, index: int):
        cmd_params = {"Index": index}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionGetTicket, cmd_params)

    # PositionSelect
    def position_select(self, symbol: str):
        cmd_params = {"Symbol": symbol}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionSelect, cmd_params)

    # PositionSelectByTicket
    def position_select_by_ticket(self, ticket: int):
        cmd_params = {"Ticket": ticket}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionSelectByTicket, cmd_params)

    # PositionGetDouble
    def position_get_double(self, property_id: ENUM_POSITION_PROPERTY_DOUBLE):
        cmd_params = {"PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionGetDouble, cmd_params)

    # PositionGetInteger
    def position_get_integer(self, property_id: ENUM_POSITION_PROPERTY_INTEGER):
        cmd_params = {"PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionGetInteger, cmd_params)

    # PositionGetString
    def position_get_string(self, property_id: ENUM_POSITION_PROPERTY_STRING):
        cmd_params = {"PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionGetString, cmd_params)

    # PositionOpen
    def position_open(
        self,
        symbol: str,
        order_type: ENUM_ORDER_TYPE,
        volume: float,
        price: float,
        sl: float,
        tp: float,
        comment: str = None,
    ):
        """Returns (ok, MqlTradeResult | None)."""
        cmd_params = {"Symbol": symbol, "OrderType": int(order_type), "Volume": volume, "Price": price, "Sl": sl, "Tp": tp}
        if comment is not None:
            cmd_params["Comment"] = comment
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionOpen2, cmd_params)
        return self.__func_result(res, MqlTradeResult)

    # PositionModify
    def position_modify(self, ticket: int, sl: float, tp: float):
        cmd_params = {"Ticket": ticket, "Sl": sl, "Tp": tp}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionModify, cmd_params)

    # PositionClose
    def position_close(self, ticket: int, deviation: int = 0xFFFFFFFFFFFFFFFF):
        """Returns (ok, MqlTradeResult | None)."""
        cmd_params = {"Ticket": ticket, "Deviation": deviation}
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionClose, cmd_params)
        return self.__func_result(res, MqlTradeResult)

    # PositionClosePartial (by ticket)
    def position_close_partial_by_ticket(self, ticket: int, volume: float, deviation: int = 0xFFFFFFFFFFFFFFFF):
        cmd_params = {"Ticket": ticket, "Volume": volume, "Deviation": deviation}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionClosePartial_byTicket, cmd_params)

    # PositionClosePartial (by symbol, hedging accounts)
    def position_close_partial_by_symbol(self, symbol: str, volume: float, deviation: int = 0xFFFFFFFFFFFFFFFF):
        cmd_params = {"Symbol": symbol, "Volume": volume, "Deviation": deviation}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionClosePartial_bySymbol, cmd_params)

    # PositionCloseAll, returns count of closed positions
    def position_close_all(self):
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.PositionCloseAll)

    # Buy
    def buy(
        self,
        volume: float,
        symbol: str = None,
        price: float = 0.0,
        sl: float = 0.0,
        tp: float = 0.0,
        comment: str = None,
    ):
        """Returns (ok, MqlTradeResult | None)."""
        return self.__market_order(Mt5CommandType.Buy, volume, symbol, price, sl, tp, comment)

    # Sell
    def sell(
        self,
        volume: float,
        symbol: str = None,
        price: float = 0.0,
        sl: float = 0.0,
        tp: float = 0.0,
        comment: str = None,
    ):
        """Returns (ok, MqlTradeResult | None)."""
        return self.__market_order(Mt5CommandType.Sell, volume, symbol, price, sl, tp, comment)

    def __market_order(self, command_type, volume, symbol, price, sl, tp, comment):
        cmd_params = {"Volume": volume, "Price": price, "Sl": sl, "Tp": tp}
        if symbol is not None:
            cmd_params["Symbol"] = symbol
        if comment is not None:
            cmd_params["Comment"] = comment
        res = self.__send_command(self.__get_default_expert(), command_type, cmd_params)
        return self.__func_result(res, MqlTradeResult)

    # Order functions (pending orders)

    # OrdersTotal
    def orders_total(self):
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.OrdersTotal)

    # OrderGetTicket
    def order_get_ticket(self, index: int):
        cmd_params = {"Index": index}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.OrderGetTicket, cmd_params)

    # OrderSelect
    def order_select(self, ticket: int):
        cmd_params = {"Ticket": ticket}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.OrderSelect, cmd_params)

    # OrderGetDouble
    def order_get_double(self, property_id: ENUM_ORDER_PROPERTY_DOUBLE):
        cmd_params = {"PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.OrderGetDouble, cmd_params)

    # OrderGetInteger
    def order_get_integer(self, property_id: ENUM_ORDER_PROPERTY_INTEGER):
        cmd_params = {"PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.OrderGetInteger, cmd_params)

    # OrderGetString
    def order_get_string(self, property_id: ENUM_ORDER_PROPERTY_STRING):
        cmd_params = {"PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.OrderGetString, cmd_params)

    # History functions

    # HistorySelect (times are MetaTrader server times, seconds since 1970)
    def history_select(self, from_date: int, to_date: int):
        cmd_params = {"FromDate": from_date, "ToDate": to_date}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistorySelect, cmd_params)

    # HistorySelectByPosition
    def history_select_by_position(self, position_id: int):
        cmd_params = {"PositionId": position_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistorySelectByPosition, cmd_params)

    # HistoryOrderSelect
    def history_order_select(self, ticket: int):
        cmd_params = {"Ticket": ticket}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryOrderSelect, cmd_params)

    # HistoryOrdersTotal
    def history_orders_total(self):
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryOrdersTotal)

    # HistoryOrderGetTicket
    def history_order_get_ticket(self, index: int):
        cmd_params = {"Index": index}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryOrderGetTicket, cmd_params)

    # HistoryOrderGetDouble
    def history_order_get_double(self, ticket_number: int, property_id: ENUM_ORDER_PROPERTY_DOUBLE):
        cmd_params = {"TicketNumber": ticket_number, "PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryOrderGetDouble, cmd_params)

    # HistoryOrderGetInteger
    def history_order_get_integer(self, ticket_number: int, property_id: ENUM_ORDER_PROPERTY_INTEGER):
        cmd_params = {"TicketNumber": ticket_number, "PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryOrderGetInteger, cmd_params)

    # HistoryOrderGetString
    def history_order_get_string(self, ticket_number: int, property_id: ENUM_ORDER_PROPERTY_STRING):
        cmd_params = {"TicketNumber": ticket_number, "PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryOrderGetString, cmd_params)

    # HistoryDealSelect
    def history_deal_select(self, ticket: int):
        cmd_params = {"Ticket": ticket}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryDealSelect, cmd_params)

    # HistoryDealsTotal
    def history_deals_total(self):
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryDealsTotal)

    # HistoryDealGetTicket
    def history_deal_get_ticket(self, index: int):
        cmd_params = {"Index": index}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryDealGetTicket, cmd_params)

    # HistoryDealGetDouble
    def history_deal_get_double(self, ticket_number: int, property_id: ENUM_DEAL_PROPERTY_DOUBLE):
        cmd_params = {"TicketNumber": ticket_number, "PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryDealGetDouble, cmd_params)

    # HistoryDealGetInteger
    def history_deal_get_integer(self, ticket_number: int, property_id: ENUM_DEAL_PROPERTY_INTEGER):
        cmd_params = {"TicketNumber": ticket_number, "PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryDealGetInteger, cmd_params)

    # HistoryDealGetString
    def history_deal_get_string(self, ticket_number: int, property_id: ENUM_DEAL_PROPERTY_STRING):
        cmd_params = {"TicketNumber": ticket_number, "PropertyId": property_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.HistoryDealGetString, cmd_params)

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
    def copy_buffer(self, indicator_handle: int, buffer_num: int, start_pos: int, count: int):
        cmd_params = {
            "IndicatorHandle": indicator_handle,
            "BufferNum": buffer_num,
            "StartPos": start_pos,
            "Count": count,
        }
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.CopyBuffer, cmd_params)

    # CopyRates
    def copy_rates(self, symbol_name: str, timeframe: ENUM_TIMEFRAMES, start_pos: int, count: int):
        cmd_params = {
            "Symbol": symbol_name,
            "Timeframe": timeframe,
            "StartPos": start_pos,
            "Count": count,
        }
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.CopyRates, cmd_params)
        rates = [MqlRates(obj) for obj in res]
        return rates

    # CopyTime
    def copy_time(self, symbol_name: str, timeframe: ENUM_TIMEFRAMES, start_pos: int, count: int):
        cmd_params = {
            "Symbol": symbol_name,
            "Timeframe": timeframe,
            "StartPos": start_pos,
            "Count": count,
        }
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.CopyTime, cmd_params)

    # CopyOpen
    def copy_open(self, symbol_name: str, timeframe: ENUM_TIMEFRAMES, start_pos: int, count: int):
        cmd_params = {
            "Symbol": symbol_name,
            "Timeframe": timeframe,
            "StartPos": start_pos,
            "Count": count,
        }
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.CopyOpen, cmd_params)

    # CopyHigh
    def copy_high(self, symbol_name: str, timeframe: ENUM_TIMEFRAMES, start_pos: int, count: int):
        cmd_params = {
            "Symbol": symbol_name,
            "Timeframe": timeframe,
            "StartPos": start_pos,
            "Count": count,
        }
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.CopyHigh, cmd_params)

    # CopyLow
    def copy_low(self, symbol_name: str, timeframe: ENUM_TIMEFRAMES, start_pos: int, count: int):
        cmd_params = {
            "Symbol": symbol_name,
            "Timeframe": timeframe,
            "StartPos": start_pos,
            "Count": count,
        }
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.CopyLow, cmd_params)

    # CopyClose
    def copy_close(self, symbol_name: str, timeframe: ENUM_TIMEFRAMES, start_pos: int, count: int):
        cmd_params = {
            "Symbol": symbol_name,
            "Timeframe": timeframe,
            "StartPos": start_pos,
            "Count": count,
        }
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.CopyClose, cmd_params)

    # CopyTickVolume
    def copy_tick_volume(self, symbol_name: str, timeframe: ENUM_TIMEFRAMES, start_pos: int, count: int):
        cmd_params = {
            "Symbol": symbol_name,
            "Timeframe": timeframe,
            "StartPos": start_pos,
            "Count": count,
        }
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.CopyTickVolume, cmd_params)

    # CopyRealVolume
    def copy_real_volume(self, symbol_name: str, timeframe: ENUM_TIMEFRAMES, start_pos: int, count: int):
        cmd_params = {
            "Symbol": symbol_name,
            "Timeframe": timeframe,
            "StartPos": start_pos,
            "Count": count,
        }
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.CopyRealVolume, cmd_params)

    # CopySpread
    def copy_spread(self, symbol_name: str, timeframe: ENUM_TIMEFRAMES, start_pos: int, count: int):
        cmd_params = {
            "Symbol": symbol_name,
            "Timeframe": timeframe,
            "StartPos": start_pos,
            "Count": count,
        }
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.CopySpread, cmd_params)

    # CopyTicks
    def copy_ticks(self, symbol_name: str, flags: CopyTicksFlag, from_date: int, count: int):
        cmd_params = {
            "Symbol": symbol_name,
            "Flags": flags,
            "From": from_date,
            "Count": count,
        }
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.CopyTicks, cmd_params)
        if res is None:
            return None
        ticks = [MqlTick(obj) for obj in res]
        return ticks

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
            return MqlTick(res["Result"])
        return None

    # SymbolInfoSessionQuote
    def symbol_info_session_quote(self, name: str, day_of_week: ENUM_DAY_OF_WEEK, session_index: int):
        cmd_params = {"Symbol": name, "DayOfWeek": day_of_week, "SessionIndex": session_index}
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.SymbolInfoSessionQuote, cmd_params)
        if res is not None and res["RetVal"] == True:
            return (res["Result"]["From"], res["Result"]["To"])
        return None

    # SymbolInfoSessionTrade
    def symbol_info_session_trade(self, name: str, day_of_week: ENUM_DAY_OF_WEEK, session_index: int):
        cmd_params = {"Symbol": name, "DayOfWeek": day_of_week, "SessionIndex": session_index}
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.SymbolInfoSessionTrade, cmd_params)
        if res is not None and res["RetVal"] == True:
            return (res["Result"]["From"], res["Result"]["To"])
        return None

    # MarketBookAdd
    def market_book_add(self, symbol: str):
        cmd_params = {"Symbol": symbol}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.MarketBookAdd, cmd_params)

    # MarketBookRelease
    def market_book_release(self, symbol: str):
        cmd_params = {"Symbol": symbol}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.MarketBookRelease, cmd_params)

    # MarketBookGet
    def market_book_get(self, symbol: str):
        cmd_params = {"Symbol": symbol}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.MarketBookGet, cmd_params)

    # ChartId
    def chart_id(self, expert_handle=0):
        if expert_handle == 0:
            return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartId)
        else:
            return self.__send_command(expert_handle, Mt5CommandType.ChartId)

    # ChartRedraw
    def chart_redraw(self, chart_id=0):
        cmd_params = {"ChartId": chart_id}
        self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartRedraw, cmd_params)

    # ChartApplyTemplate
    def chart_apply_template(self, chart_id, filename: str):
        cmd_params = {"ChartId": chart_id, "TemplateFileName": filename}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartApplyTemplate, cmd_params)

    # ChartSaveTemplate
    def chart_save_template(self, chart_id, filename: str):
        cmd_params = {"ChartId": chart_id, "TemplateFileName": filename}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartSaveTemplate, cmd_params)

    # ChartWindowFind
    def chart_window_find(self, chart_id, indicator_short_name: str):
        cmd_params = {"ChartId": chart_id, "IndicatorShortname": indicator_short_name}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartWindowFind, cmd_params)

    # ChartTimePriceToXY
    def chart_time_price_to_xy(self, chart_id, sub_window, time, price):
        cmd_params = {"ChartId": chart_id, "SubWindow": sub_window, "Time": time, "Price": price}
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartTimePriceToXY, cmd_params)
        if res is not None and res["RetVal"] == True:
            return (res["Result"]["X"], res["Result"]["Y"])
        return None

    # ChartXYToTimePrice
    def chart_xy_to_time_price(self, chart_id, x, y):
        cmd_params = {"ChartId": chart_id, "X": x, "Y": y}
        res = self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartXYToTimePrice, cmd_params)
        if res is not None and res["RetVal"] == True:
            return (res["Result"]["SubWindow"], res["Result"]["Time"], res["Result"]["Price"])
        return None

    # ChartOpen
    def chart_open(self, symbol: str, period: ENUM_TIMEFRAMES):
        cmd_params = {"Symbol": symbol, "Timeframe": period}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartOpen, cmd_params)

    # ChartFirst
    def chart_first(self):
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartFirst)

    # ChartNext
    def chart_next(self, chart_id):
        cmd_params = {"ChartId": chart_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartNext, cmd_params)

    # ChartClose
    def chart_close(self, chart_id):
        cmd_params = {"ChartId": chart_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartClose, cmd_params)

    # ChartSymbol
    def chart_symbol(self, chart_id):
        cmd_params = {"ChartId": chart_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartSymbol, cmd_params)

    # ChartPeriod
    def chart_period(self, chart_id):
        cmd_params = {"ChartId": chart_id}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartPeriod, cmd_params)

    # ChartSetDouble
    def chart_set_double(self, chart_id, prop_id: ENUM_CHART_PROPERTY_DOUBLE, value):
        cmd_params = {"ChartId": chart_id, "PropId": prop_id, "Value": value}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartSetDouble, cmd_params)

    # ChartSetInteger
    def chart_set_integer(self, chart_id, prop_id: ENUM_CHART_PROPERTY_INTEGER, value):
        cmd_params = {"ChartId": chart_id, "PropId": prop_id, "Value": value}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartSetInteger, cmd_params)

    # ChartSetString
    def chart_set_string(self, chart_id, prop_id: ENUM_CHART_PROPERTY_STRING, value):
        cmd_params = {"ChartId": chart_id, "PropId": prop_id, "Value": value}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartSetString, cmd_params)

    # ChartGetDouble
    def chart_get_double(self, chart_id, prop_id: ENUM_CHART_PROPERTY_DOUBLE, sub_window=0):
        cmd_params = {"ChartId": chart_id, "PropId": prop_id, "SubWindow": sub_window}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartGetDouble, cmd_params)

    # ChartGetInteger
    def chart_get_integer(self, chart_id, prop_id: ENUM_CHART_PROPERTY_INTEGER, sub_window=0):
        cmd_params = {"ChartId": chart_id, "PropId": prop_id, "SubWindow": sub_window}
        return self.__send_command(self.__get_default_expert(), Mt5CommandType.ChartGetInteger, cmd_params)

    # Private methods

    def __event_thread_func(self):
        self.__logger.debug(f"__event_thread started")
        asyncio.set_event_loop(self.__event_loop)
        self.__event_loop.run_forever()
        self.__logger.debug(f"__event_thread stopped")

    def __stop_event_thread(self):
        self.__event_loop.call_soon_threadsafe(self.__event_loop.stop)
        self.__event_thread.join()
        self.__event_loop.close()

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
        try:
            response = self.__rpcclient.send_command(expert_handle, command_type, payload_json)
        except MtRpcError as e:
            # MtService could not deliver the command to MetaTrader (no expert, expert not found or removed)
            self.__logger.warning(f"send_command: command was not executed. Status = {e.status!r}. {e}")
            raise
        if response is None:
            self.__logger.warning("Failed to send commad. Result is None")
            raise Exception("Failed to send commad. Result is None")
        response_json = json.loads(response)
        error_code = int(response_json["ErrorCode"])
        if error_code != 0:
            error_message = response_json["ErrorMessage"]
            self.__logger.warning(f"send_command: ErrorCode = {error_code}. {error_message}")
            raise Exception(f"Failed to send command: ErrorCode = {error_code}. {error_message} ")
        if "Value" in response_json:
            return response_json["Value"]
        return None

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
