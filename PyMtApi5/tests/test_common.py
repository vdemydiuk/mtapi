"""Unit tests for the common, strategy tester and IsTesting functions of Mt5ApiClient (see test_trading.py)."""

import unittest

from PyMtApi5 import Mt5CommandType

from .test_trading import TradingTestCase


class CommonFunctionTests(TradingTestCase):
    """(name, method call, command, payload, reply, expected return value)."""

    CASES = [
        ("alert", lambda c: c.alert("warn"), Mt5CommandType.Alert, {"Message": "warn"}, None, None),
        ("print_message", lambda c: c.print_message("hi"), Mt5CommandType.Print, {"PrintMsg": "hi"}, True, True),
        ("get_last_error", lambda c: c.get_last_error(), Mt5CommandType.GetLastError, None, 4301, 4301),
        ("reset_last_error", lambda c: c.reset_last_error(), Mt5CommandType.ResetLastError, None, None, None),
        ("get_symbols", lambda c: c.get_symbols(True), Mt5CommandType.GetSymbols, {"Selected": True}, ["EURUSD", "GBPUSD"], ["EURUSD", "GBPUSD"]),
        ("tester_stop", lambda c: c.tester_stop(), Mt5CommandType.TesterStop, None, None, None),
        ("unlock_ticks", lambda c: c.unlock_ticks(), Mt5CommandType.UnlockTicks, None, None, None),
        ("is_testing_true", lambda c: c.is_testing(), Mt5CommandType.IsTesting, None, True, True),
        ("is_testing_false", lambda c: c.is_testing(), Mt5CommandType.IsTesting, None, False, False),
    ]

    def test_commands(self):
        for name, call, command, payload, reply, expected in self.CASES:
            with self.subTest(name):
                self.fake.reply = reply
                self.assertEqual(expected, call(self.client))
                self.assert_sent(command, payload)


if __name__ == "__main__":
    unittest.main()
