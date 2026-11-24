"""Unit tests for the global variable functions of Mt5ApiClient (see test_trading.py for the approach)."""

import unittest

from PyMtApi5 import Mt5CommandType

from .test_trading import TradingTestCase


class GlobalVariableTests(TradingTestCase):
    """(name, method call, command, payload, reply, expected return value)."""

    CASES = [
        ("check", lambda c: c.global_variable_check("g"), Mt5CommandType.GlobalVariableCheck, {"Name": "g"}, True, True),
        ("time", lambda c: c.global_variable_time("g"), Mt5CommandType.GlobalVariableTime, {"Name": "g"}, 1700000000, 1700000000),
        ("del", lambda c: c.global_variable_del("g"), Mt5CommandType.GlobalVariableDel, {"Name": "g"}, True, True),
        ("get", lambda c: c.global_variable_get("g"), Mt5CommandType.GlobalVariableGet, {"Name": "g"}, 2.5, 2.5),
        ("name", lambda c: c.global_variable_name(0), Mt5CommandType.GlobalVariableName, {"Index": 0}, "g", "g"),
        ("set", lambda c: c.global_variable_set("g", 2.5), Mt5CommandType.GlobalVariableSet, {"Name": "g", "Value": 2.5}, 1700000000, 1700000000),
        ("temp", lambda c: c.global_variable_temp("g"), Mt5CommandType.GlobalVariableTemp, {"Name": "g"}, True, True),
        (
            "set_on_condition",
            lambda c: c.global_variable_set_on_condition("g", 2, 1),
            Mt5CommandType.GlobalVariableSetOnCondition,
            {"Name": "g", "Value": 2, "CheckValue": 1},
            True,
            True,
        ),
        (
            "delete_all_defaults",
            lambda c: c.global_variables_delete_all(),
            Mt5CommandType.GlobalVariablesDeleteAll,
            {"PrefixName": "", "LimitData": 0},
            2,
            2,
        ),
        (
            "delete_all_limit",
            lambda c: c.global_variables_delete_all("p_", 1700000000),
            Mt5CommandType.GlobalVariablesDeleteAll,
            {"PrefixName": "p_", "LimitData": 1700000000},
            2,
            2,
        ),
        ("total", lambda c: c.global_variables_total(), Mt5CommandType.GlobalVariablesTotal, None, 7, 7),
        ("flush", lambda c: c.global_variables_flush(), Mt5CommandType.GlobalVariablesFlush, None, None, None),
    ]

    def test_commands(self):
        for name, call, command, payload, reply, expected in self.CASES:
            with self.subTest(name):
                self.fake.reply = reply
                self.assertEqual(expected, call(self.client))
                self.assert_sent(command, payload)


if __name__ == "__main__":
    unittest.main()
