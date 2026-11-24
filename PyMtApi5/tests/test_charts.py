"""Unit tests for the remaining chart functions of Mt5ApiClient (see test_trading.py for the approach)."""

import unittest

from PyMtApi5 import (
    ENUM_ALIGN_MODE,
    ENUM_CHART_POSITION,
    ENUM_CHART_PROPERTY_STRING,
    ENUM_TIMEFRAMES,
    Mt5CommandType,
)

from .test_trading import TradingTestCase

T = 1700000000
PS = ENUM_CHART_PROPERTY_STRING.CHART_COMMENT


class ChartTests(TradingTestCase):
    """(name, method call, command, payload, reply, expected return value)."""

    CASES = [
        ("get_string", lambda c: c.chart_get_string(5, PS), Mt5CommandType.ChartGetString, {"ChartId": 5, "PropId": int(PS)}, "c", "c"),
        (
            "set_symbol_period",
            lambda c: c.chart_set_symbol_period(5, "GBPUSD", ENUM_TIMEFRAMES.PERIOD_H4),
            Mt5CommandType.ChartSetSymbolPeriod,
            {"ChartId": 5, "Symbol": "GBPUSD", "Period": int(ENUM_TIMEFRAMES.PERIOD_H4)},
            True,
            True,
        ),
        (
            "screen_shot_default_align",
            lambda c: c.chart_screen_shot(5, "a.gif", 800, 600),
            Mt5CommandType.ChartScreenShot,
            {"ChartId": 5, "Filename": "a.gif", "Width": 800, "Height": 600, "AlignMode": int(ENUM_ALIGN_MODE.ALIGN_RIGHT)},
            True,
            True,
        ),
        (
            "screen_shot_align",
            lambda c: c.chart_screen_shot(5, "a.gif", 800, 600, ENUM_ALIGN_MODE.ALIGN_LEFT),
            Mt5CommandType.ChartScreenShot,
            {"ChartId": 5, "Filename": "a.gif", "Width": 800, "Height": 600, "AlignMode": int(ENUM_ALIGN_MODE.ALIGN_LEFT)},
            True,
            True,
        ),
        (
            "navigate",
            lambda c: c.chart_navigate(5, ENUM_CHART_POSITION.CHART_END, -2),
            Mt5CommandType.ChartNavigate,
            {"ChartId": 5, "Position": int(ENUM_CHART_POSITION.CHART_END), "Shift": -2},
            True,
            True,
        ),
        (
            "navigate_default_shift",
            lambda c: c.chart_navigate(5, ENUM_CHART_POSITION.CHART_BEGIN),
            Mt5CommandType.ChartNavigate,
            {"ChartId": 5, "Position": int(ENUM_CHART_POSITION.CHART_BEGIN), "Shift": 0},
            True,
            True,
        ),
        ("indicator_add", lambda c: c.chart_indicator_add(5, 1, 7), Mt5CommandType.ChartIndicatorAdd, {"ChartId": 5, "SubWindow": 1, "IndicatorHandle": 7}, True, True),
        ("indicator_delete", lambda c: c.chart_indicator_delete(5, 1, "RSI"), Mt5CommandType.ChartIndicatorDelete, {"ChartId": 5, "SubWindow": 1, "IndicatorShortname": "RSI"}, True, True),
        ("indicator_get", lambda c: c.chart_indicator_get(5, 1, "RSI"), Mt5CommandType.ChartIndicatorGet, {"ChartId": 5, "SubWindow": 1, "IndicatorShortname": "RSI"}, 13, 13),
        ("indicator_name", lambda c: c.chart_indicator_name(5, 1, 0), Mt5CommandType.ChartIndicatorName, {"ChartId": 5, "SubWindow": 1, "Index": 0}, "RSI(14)", "RSI(14)"),
        ("indicators_total", lambda c: c.chart_indicators_total(5, 1), Mt5CommandType.ChartIndicatorsTotal, {"ChartId": 5, "SubWindow": 1}, 2, 2),
        ("window_on_dropped", lambda c: c.chart_window_on_dropped(), Mt5CommandType.ChartWindowOnDropped, None, 1, 1),
        ("price_on_dropped", lambda c: c.chart_price_on_dropped(), Mt5CommandType.ChartPriceOnDropped, None, 1.2345, 1.2345),
        ("time_on_dropped", lambda c: c.chart_time_on_dropped(), Mt5CommandType.ChartTimeOnDropped, None, T, T),
        ("x_on_dropped", lambda c: c.chart_x_on_dropped(), Mt5CommandType.ChartXOnDropped, None, 10, 10),
        ("y_on_dropped", lambda c: c.chart_y_on_dropped(), Mt5CommandType.ChartYOnDropped, None, 20, 20),
    ]

    def test_commands(self):
        for name, call, command, payload, reply, expected in self.CASES:
            with self.subTest(name):
                self.fake.reply = reply
                self.assertEqual(expected, call(self.client))
                self.assert_sent(command, payload)


if __name__ == "__main__":
    unittest.main()
