"""Unit tests for the history functions of Mt5ApiClient (see test_trading.py for the approach)."""

import unittest

from PyMtApi5 import (
    ENUM_DEAL_PROPERTY_DOUBLE,
    ENUM_DEAL_PROPERTY_INTEGER,
    ENUM_DEAL_PROPERTY_STRING,
    ENUM_ORDER_PROPERTY_DOUBLE,
    ENUM_ORDER_PROPERTY_INTEGER,
    ENUM_ORDER_PROPERTY_STRING,
    Mt5CommandType,
)

from .test_trading import U64_MAX, TradingTestCase


class HistoryTests(TradingTestCase):
    """(name, method call, command, payload, reply, expected return value)."""

    CASES = [
        ("history_select", lambda c: c.history_select(1700000000, 1700086400), Mt5CommandType.HistorySelect, {"FromDate": 1700000000, "ToDate": 1700086400}, True, True),
        ("history_select_by_position", lambda c: c.history_select_by_position(123), Mt5CommandType.HistorySelectByPosition, {"PositionId": 123}, True, True),
        ("history_order_select", lambda c: c.history_order_select(8), Mt5CommandType.HistoryOrderSelect, {"Ticket": 8}, True, True),
        ("history_orders_total", lambda c: c.history_orders_total(), Mt5CommandType.HistoryOrdersTotal, None, 12, 12),
        ("history_order_get_ticket", lambda c: c.history_order_get_ticket(3), Mt5CommandType.HistoryOrderGetTicket, {"Index": 3}, 44, 44),
        (
            "history_order_get_double",
            lambda c: c.history_order_get_double(44, ENUM_ORDER_PROPERTY_DOUBLE.ORDER_SL),
            Mt5CommandType.HistoryOrderGetDouble,
            {"TicketNumber": 44, "PropertyId": int(ENUM_ORDER_PROPERTY_DOUBLE.ORDER_SL)},
            1.1,
            1.1,
        ),
        (
            "history_order_get_integer",
            lambda c: c.history_order_get_integer(44, ENUM_ORDER_PROPERTY_INTEGER.ORDER_STATE),
            Mt5CommandType.HistoryOrderGetInteger,
            {"TicketNumber": 44, "PropertyId": int(ENUM_ORDER_PROPERTY_INTEGER.ORDER_STATE)},
            4,
            4,
        ),
        (
            "history_order_get_string",
            lambda c: c.history_order_get_string(44, ENUM_ORDER_PROPERTY_STRING.ORDER_SYMBOL),
            Mt5CommandType.HistoryOrderGetString,
            {"TicketNumber": 44, "PropertyId": int(ENUM_ORDER_PROPERTY_STRING.ORDER_SYMBOL)},
            "EURUSD",
            "EURUSD",
        ),
        ("history_deal_select", lambda c: c.history_deal_select(9), Mt5CommandType.HistoryDealSelect, {"Ticket": 9}, True, True),
        ("history_deals_total", lambda c: c.history_deals_total(), Mt5CommandType.HistoryDealsTotal, None, 5, 5),
        ("history_deal_get_ticket", lambda c: c.history_deal_get_ticket(1), Mt5CommandType.HistoryDealGetTicket, {"Index": 1}, U64_MAX, U64_MAX),
        (
            "history_deal_get_double",
            lambda c: c.history_deal_get_double(9, ENUM_DEAL_PROPERTY_DOUBLE.DEAL_PROFIT),
            Mt5CommandType.HistoryDealGetDouble,
            {"TicketNumber": 9, "PropertyId": int(ENUM_DEAL_PROPERTY_DOUBLE.DEAL_PROFIT)},
            12.5,
            12.5,
        ),
        (
            "history_deal_get_integer",
            lambda c: c.history_deal_get_integer(9, ENUM_DEAL_PROPERTY_INTEGER.DEAL_TICKET),
            Mt5CommandType.HistoryDealGetInteger,
            {"TicketNumber": 9, "PropertyId": int(ENUM_DEAL_PROPERTY_INTEGER.DEAL_TICKET)},
            9,
            9,
        ),
        (
            "history_deal_get_string",
            lambda c: c.history_deal_get_string(9, ENUM_DEAL_PROPERTY_STRING.DEAL_SYMBOL),
            Mt5CommandType.HistoryDealGetString,
            {"TicketNumber": 9, "PropertyId": int(ENUM_DEAL_PROPERTY_STRING.DEAL_SYMBOL)},
            "EURUSD",
            "EURUSD",
        ),
    ]

    def test_commands(self):
        for name, call, command, payload, reply, expected in self.CASES:
            with self.subTest(name):
                self.fake.reply = reply
                self.assertEqual(expected, call(self.client))
                self.assert_sent(command, payload)


if __name__ == "__main__":
    unittest.main()
