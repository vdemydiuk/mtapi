"""Unit tests for trade, position and order functions of Mt5ApiClient.

No MetaTrader terminal is needed: the RPC client is replaced with a fake that records the
command id and JSON payload sent and returns a canned reply (same idea as FakeRpcClient in
MtApi5.Tests). Run from the repository root: python -m unittest discover -s PyMtApi5/tests -t .
"""

import json
import unittest

from PyMtApi5 import (
    ENUM_ORDER_PROPERTY_DOUBLE,
    ENUM_ORDER_PROPERTY_INTEGER,
    ENUM_ORDER_PROPERTY_STRING,
    ENUM_ORDER_TYPE,
    ENUM_POSITION_PROPERTY_DOUBLE,
    ENUM_POSITION_PROPERTY_INTEGER,
    ENUM_POSITION_PROPERTY_STRING,
    ENUM_TRADE_REQUEST_ACTIONS,
    Mt5ApiClient,
    Mt5CommandType,
    MqlTradeCheckResult,
    MqlTradeRequest,
    MqlTradeResult,
)

U64_MAX = 0xFFFFFFFFFFFFFFFF
EXPERT = 7

TRADE_RESULT = {
    "Retcode": 10009,
    "Deal": 1,
    "Order": 2,
    "Volume": 0.5,
    "Price": 1.1,
    "Bid": 1.09,
    "Ask": 1.11,
    "Comment": "done",
    "Request_id": 3,
}
CHECK_RESULT = {
    "Retcode": 0,
    "Balance": 1000.0,
    "Equity": 990.0,
    "Profit": -10.0,
    "Margin": 5.0,
    "Margin_free": 985.0,
    "Margin_level": 19800.0,
    "Comment": "Done",
}


class FakeRpcClient:
    def __init__(self):
        self.sent = []
        self.reply = None  # value placed into the "Value" field; None -> no value
        self.raw_reply = None

    def send_command(self, expert_handle, command_type, payload=None, timeout=10):
        self.sent.append((expert_handle, command_type, None if payload is None else json.loads(payload)))
        if self.raw_reply is not None:
            return self.raw_reply
        response = {"ErrorCode": 0}
        if self.reply is not None:
            response["Value"] = self.reply
        return json.dumps(response)

    def disconnect(self):
        pass


def make_client():
    client = Mt5ApiClient("localhost", 8222)
    fake = FakeRpcClient()
    client._Mt5ApiClient__rpcclient = fake
    client._Mt5ApiClient__experts.append(EXPERT)
    return client, fake


class TradingTestCase(unittest.TestCase):
    def setUp(self):
        self.client, self.fake = make_client()

    def last(self):
        return self.fake.sent[-1]

    def assert_sent(self, command, payload):
        expert, command_type, sent_payload = self.last()
        self.assertEqual(EXPERT, expert)
        self.assertEqual(int(command), int(command_type))
        self.assertEqual(payload, sent_payload)


class SimpleCommandTests(TradingTestCase):
    """(method call, command, payload, reply, expected return value)."""

    CASES = [
        ("positions_total", lambda c: c.positions_total(), Mt5CommandType.PositionsTotal, None, 2, 2),
        ("position_get_symbol", lambda c: c.position_get_symbol(1), Mt5CommandType.PositionGetSymbol, {"Index": 1}, "EURUSD", "EURUSD"),
        ("position_get_ticket", lambda c: c.position_get_ticket(0), Mt5CommandType.PositionGetTicket, {"Index": 0}, U64_MAX, U64_MAX),
        ("position_select", lambda c: c.position_select("EURUSD"), Mt5CommandType.PositionSelect, {"Symbol": "EURUSD"}, True, True),
        ("position_select_by_ticket", lambda c: c.position_select_by_ticket(55), Mt5CommandType.PositionSelectByTicket, {"Ticket": 55}, True, True),
        (
            "position_get_double",
            lambda c: c.position_get_double(ENUM_POSITION_PROPERTY_DOUBLE.POSITION_PROFIT),
            Mt5CommandType.PositionGetDouble,
            {"PropertyId": int(ENUM_POSITION_PROPERTY_DOUBLE.POSITION_PROFIT)},
            12.5,
            12.5,
        ),
        (
            "position_get_integer",
            lambda c: c.position_get_integer(ENUM_POSITION_PROPERTY_INTEGER.POSITION_TICKET),
            Mt5CommandType.PositionGetInteger,
            {"PropertyId": int(ENUM_POSITION_PROPERTY_INTEGER.POSITION_TICKET)},
            77,
            77,
        ),
        (
            "position_get_string",
            lambda c: c.position_get_string(ENUM_POSITION_PROPERTY_STRING.POSITION_SYMBOL),
            Mt5CommandType.PositionGetString,
            {"PropertyId": int(ENUM_POSITION_PROPERTY_STRING.POSITION_SYMBOL)},
            "EURUSD",
            "EURUSD",
        ),
        ("position_modify", lambda c: c.position_modify(5, 1.1, 1.3), Mt5CommandType.PositionModify, {"Ticket": 5, "Sl": 1.1, "Tp": 1.3}, True, True),
        (
            "position_close_partial_by_symbol",
            lambda c: c.position_close_partial_by_symbol("EURUSD", 0.5, 10),
            Mt5CommandType.PositionClosePartial_bySymbol,
            {"Symbol": "EURUSD", "Volume": 0.5, "Deviation": 10},
            True,
            True,
        ),
        (
            "position_close_partial_by_ticket_default_deviation",
            lambda c: c.position_close_partial_by_ticket(9, 0.5),
            Mt5CommandType.PositionClosePartial_byTicket,
            {"Ticket": 9, "Volume": 0.5, "Deviation": U64_MAX},
            True,
            True,
        ),
        ("position_close_all", lambda c: c.position_close_all(), Mt5CommandType.PositionCloseAll, None, 3, 3),
        ("orders_total", lambda c: c.orders_total(), Mt5CommandType.OrdersTotal, None, 4, 4),
        ("order_get_ticket", lambda c: c.order_get_ticket(2), Mt5CommandType.OrderGetTicket, {"Index": 2}, 99, 99),
        ("order_select", lambda c: c.order_select(99), Mt5CommandType.OrderSelect, {"Ticket": 99}, True, True),
        (
            "order_get_double",
            lambda c: c.order_get_double(ENUM_ORDER_PROPERTY_DOUBLE.ORDER_PRICE_OPEN),
            Mt5CommandType.OrderGetDouble,
            {"PropertyId": int(ENUM_ORDER_PROPERTY_DOUBLE.ORDER_PRICE_OPEN)},
            1.25,
            1.25,
        ),
        (
            "order_get_integer",
            lambda c: c.order_get_integer(ENUM_ORDER_PROPERTY_INTEGER.ORDER_TICKET),
            Mt5CommandType.OrderGetInteger,
            {"PropertyId": int(ENUM_ORDER_PROPERTY_INTEGER.ORDER_TICKET)},
            7,
            7,
        ),
        (
            "order_get_string",
            lambda c: c.order_get_string(ENUM_ORDER_PROPERTY_STRING.ORDER_COMMENT),
            Mt5CommandType.OrderGetString,
            {"PropertyId": int(ENUM_ORDER_PROPERTY_STRING.ORDER_COMMENT)},
            "c",
            "c",
        ),
    ]

    def test_commands(self):
        for name, call, command, payload, reply, expected in self.CASES:
            with self.subTest(name):
                self.fake.reply = reply
                self.assertEqual(expected, call(self.client))
                self.assert_sent(command, payload)


class TradeResultTests(TradingTestCase):
    def test_buy_sell_send_optional_fields_only_when_set(self):
        for method, command in ((self.client.buy, Mt5CommandType.Buy), (self.client.sell, Mt5CommandType.Sell)):
            with self.subTest(command.name):
                self.fake.reply = {"RetVal": True, "Result": TRADE_RESULT}
                ok, result = method(0.5)
                self.assertTrue(ok)
                self.assertIsInstance(result, MqlTradeResult)
                self.assertEqual(10009, result.retcode)
                self.assert_sent(command, {"Volume": 0.5, "Price": 0.0, "Sl": 0.0, "Tp": 0.0})

                method(1, "GBPUSD", 1.3, 1.2, 1.4, "c")
                self.assert_sent(
                    command,
                    {"Volume": 1, "Price": 1.3, "Sl": 1.2, "Tp": 1.4, "Symbol": "GBPUSD", "Comment": "c"},
                )

    def test_position_open_uses_result_command(self):
        self.fake.reply = {"RetVal": True, "Result": TRADE_RESULT}
        ok, result = self.client.position_open("EURUSD", ENUM_ORDER_TYPE.ORDER_TYPE_SELL, 0.1, 1.1, 1.2, 1.0)
        self.assertTrue(ok)
        self.assertEqual(2, result.order)
        self.assert_sent(
            Mt5CommandType.PositionOpen2,
            {"Symbol": "EURUSD", "OrderType": 1, "Volume": 0.1, "Price": 1.1, "Sl": 1.2, "Tp": 1.0},
        )
        self.client.position_open("EURUSD", ENUM_ORDER_TYPE.ORDER_TYPE_BUY, 0.1, 1.1, 1.0, 1.2, "cm")
        self.assertEqual("cm", self.last()[2]["Comment"])

    def test_position_close_default_and_explicit_deviation(self):
        self.fake.reply = {"RetVal": True}
        ok, result = self.client.position_close(9)
        self.assertTrue(ok)
        self.assertIsNone(result)  # reply has no Result
        self.assert_sent(Mt5CommandType.PositionClose, {"Ticket": 9, "Deviation": U64_MAX})

        self.fake.reply = {"RetVal": True, "Result": TRADE_RESULT}
        ok, result = self.client.position_close(9, 25)
        self.assertEqual(10009, result.retcode)
        self.assert_sent(Mt5CommandType.PositionClose, {"Ticket": 9, "Deviation": 25})

    def test_false_ret_val_is_returned(self):
        self.fake.reply = {"RetVal": False, "Result": TRADE_RESULT}
        ok, _ = self.client.buy(1)
        self.assertFalse(ok)

    def test_null_reply_returns_false_and_none(self):
        self.fake.reply = None
        self.assertEqual((False, None), self.client.buy(1))
        self.assertEqual((False, None), self.client.position_close(1))
        self.assertEqual((False, None), self.client.order_send(MqlTradeRequest()))


class OrderRequestTests(TradingTestCase):
    def make_request(self):
        return MqlTradeRequest(
            action=ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_DEAL,
            symbol="EURUSD",
            volume=0.1,
            price=1.1,
            sl=1.0,
            tp=1.2,
            deviation=5,
            order_type=ENUM_ORDER_TYPE.ORDER_TYPE_SELL,
            magic=42,
            comment="x",
            position=11,
            position_by=12,
            expiration=1700000000,
        )

    EXPECTED_REQUEST = {
        "Action": 1,
        "Magic": 42,
        "Order": 0,
        "Symbol": "EURUSD",
        "Volume": 0.1,
        "Price": 1.1,
        "Stoplimit": 0.0,
        "Sl": 1.0,
        "Tp": 1.2,
        "Deviation": 5,
        "Type": 1,
        "Type_filling": 0,
        "Type_time": 0,
        "MtExpiration": 1700000000,
        "Comment": "x",
        "Position": 11,
        "PositionBy": 12,
    }

    def test_request_to_dict(self):
        self.assertEqual(self.EXPECTED_REQUEST, self.make_request().to_dict())

    def test_request_roundtrip_from_json(self):
        parsed = MqlTradeRequest(self.EXPECTED_REQUEST)
        self.assertEqual(self.EXPECTED_REQUEST, parsed.to_dict())

    def test_request_from_event_json_without_position_fields(self):
        j = {k: v for k, v in self.EXPECTED_REQUEST.items() if k not in ("Position", "PositionBy")}
        parsed = MqlTradeRequest(j)
        self.assertEqual(0, parsed.position)
        self.assertEqual(0, parsed.position_by)

    def test_order_send_and_async(self):
        for method, command in (
            (self.client.order_send, Mt5CommandType.OrderSend),
            (self.client.order_send_async, Mt5CommandType.OrderSendAsync),
        ):
            with self.subTest(command.name):
                self.fake.reply = {"RetVal": True, "Result": TRADE_RESULT}
                ok, result = method(self.make_request())
                self.assertTrue(ok)
                self.assertEqual(10009, result.retcode)
                self.assert_sent(command, {"TradeRequest": self.EXPECTED_REQUEST})

    def test_order_check(self):
        self.fake.reply = {"RetVal": True, "Result": CHECK_RESULT}
        ok, result = self.client.order_check(self.make_request())
        self.assertTrue(ok)
        self.assertIsInstance(result, MqlTradeCheckResult)
        self.assertEqual(985.0, result.margin_free)
        self.assertEqual("Done", result.comment)
        self.assert_sent(Mt5CommandType.OrderCheck, {"TradeRequest": self.EXPECTED_REQUEST})


class CalcTests(TradingTestCase):
    def test_order_calc_margin(self):
        self.fake.reply = {"RetVal": True, "Result": 55.5}
        self.assertEqual(55.5, self.client.order_calc_margin(ENUM_ORDER_TYPE.ORDER_TYPE_SELL, "EURUSD", 0.5, 1.1))
        self.assert_sent(Mt5CommandType.OrderCalcMargin, {"Action": 1, "Symbol": "EURUSD", "Volume": 0.5, "Price": 1.1})

    def test_order_calc_profit_sends_all_prices(self):
        self.fake.reply = {"RetVal": True, "Result": 10.0}
        self.assertEqual(10.0, self.client.order_calc_profit(ENUM_ORDER_TYPE.ORDER_TYPE_BUY, "EURUSD", 1, 1.1, 1.0))
        self.assert_sent(
            Mt5CommandType.OrderCalcProfit,
            {"Action": 0, "Symbol": "EURUSD", "Volume": 1, "PriceOpen": 1.1, "PriceClose": 1.0},
        )

    def test_calc_failure_returns_none(self):
        self.fake.reply = {"RetVal": False, "Result": 0.0}
        self.assertIsNone(self.client.order_calc_margin(ENUM_ORDER_TYPE.ORDER_TYPE_BUY, "EURUSD", 1, 1))
        self.assertIsNone(self.client.order_calc_profit(ENUM_ORDER_TYPE.ORDER_TYPE_BUY, "EURUSD", 1, 1, 1))
        self.fake.reply = None
        self.assertIsNone(self.client.order_calc_margin(ENUM_ORDER_TYPE.ORDER_TYPE_BUY, "EURUSD", 1, 1))


class ErrorTests(TradingTestCase):
    def test_error_code_raises(self):
        self.fake.raw_reply = json.dumps({"ErrorCode": 4, "ErrorMessage": "boom"})
        with self.assertRaises(Exception) as ctx:
            self.client.positions_total()
        self.assertIn("boom", str(ctx.exception))


if __name__ == "__main__":
    unittest.main()
