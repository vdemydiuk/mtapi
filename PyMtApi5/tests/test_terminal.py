"""Unit tests for the terminal info and time functions of Mt5ApiClient (see test_trading.py for the approach)."""

import unittest

from PyMtApi5 import (
    ENUM_TERMINAL_INFO_DOUBLE,
    ENUM_TERMINAL_INFO_INTEGER,
    ENUM_TERMINAL_INFO_STRING,
    Mt5CommandType,
)

from .test_trading import TradingTestCase

T = 1700000000
TI = ENUM_TERMINAL_INFO_INTEGER.TERMINAL_BUILD
TD = ENUM_TERMINAL_INFO_DOUBLE.TERMINAL_COMMUNITY_BALANCE
TS = ENUM_TERMINAL_INFO_STRING.TERMINAL_NAME


class TerminalAndTimeTests(TradingTestCase):
    """(name, method call, command, payload, reply, expected return value)."""

    CASES = [
        ("terminal_info_integer", lambda c: c.terminal_info_integer(TI), Mt5CommandType.TerminalInfoInteger, {"PropertyId": int(TI)}, 4000, 4000),
        ("terminal_info_double", lambda c: c.terminal_info_double(TD), Mt5CommandType.TerminalInfoDouble, {"PropertyId": int(TD)}, 1.5, 1.5),
        ("terminal_info_string", lambda c: c.terminal_info_string(TS), Mt5CommandType.TerminalInfoString, {"PropertyId": int(TS)}, "MT5", "MT5"),
        ("time_current", lambda c: c.time_current(), Mt5CommandType.TimeCurrent, None, T, T),
        ("time_trade_server", lambda c: c.time_trade_server(), Mt5CommandType.TimeTradeServer, None, T, T),
        ("time_local", lambda c: c.time_local(), Mt5CommandType.TimeLocal, None, T, T),
        ("time_gmt", lambda c: c.time_gmt(), Mt5CommandType.TimeGMT, None, T, T),
    ]

    def test_commands(self):
        for name, call, command, payload, reply, expected in self.CASES:
            with self.subTest(name):
                self.fake.reply = reply
                self.assertEqual(expected, call(self.client))
                self.assert_sent(command, payload)


if __name__ == "__main__":
    unittest.main()
