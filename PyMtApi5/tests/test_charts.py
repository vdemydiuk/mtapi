"""Unit tests for the chart functions of Mt5ApiClient (see test_trading.py for the approach)."""

import unittest

from PyMtApi5 import (
    ENUM_ALIGN_MODE,
    ENUM_CHART_POSITION,
    ENUM_CHART_PROPERTY_DOUBLE,
    ENUM_CHART_PROPERTY_INTEGER,
    ENUM_CHART_PROPERTY_STRING,
    ENUM_TIMEFRAMES,
    Mt5CommandType,
)

from .test_trading import EXPERT, TradingTestCase

T = 1700000000
PS = ENUM_CHART_PROPERTY_STRING.CHART_COMMENT
PD = ENUM_CHART_PROPERTY_DOUBLE.CHART_SHIFT_SIZE
PI = ENUM_CHART_PROPERTY_INTEGER.CHART_SCALE


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


class ChartBasicTests(TradingTestCase):
    """The chart functions that existed before the ones in ChartTests."""

    CASES = [
        ("redraw", lambda c: c.chart_redraw(5), Mt5CommandType.ChartRedraw, {"ChartId": 5}, None, None),
        ("redraw_default", lambda c: c.chart_redraw(), Mt5CommandType.ChartRedraw, {"ChartId": 0}, None, None),
        ("apply_template", lambda c: c.chart_apply_template(5, "t.tpl"), Mt5CommandType.ChartApplyTemplate, {"ChartId": 5, "TemplateFileName": "t.tpl"}, True, True),
        ("save_template", lambda c: c.chart_save_template(5, "t.tpl"), Mt5CommandType.ChartSaveTemplate, {"ChartId": 5, "TemplateFileName": "t.tpl"}, True, True),
        ("window_find", lambda c: c.chart_window_find(5, "RSI"), Mt5CommandType.ChartWindowFind, {"ChartId": 5, "IndicatorShortname": "RSI"}, 1, 1),
        ("open", lambda c: c.chart_open("EURUSD", ENUM_TIMEFRAMES.PERIOD_M5), Mt5CommandType.ChartOpen, {"Symbol": "EURUSD", "Timeframe": int(ENUM_TIMEFRAMES.PERIOD_M5)}, 123456, 123456),
        ("first", lambda c: c.chart_first(), Mt5CommandType.ChartFirst, None, 111, 111),
        ("next", lambda c: c.chart_next(111), Mt5CommandType.ChartNext, {"ChartId": 111}, 222, 222),
        ("close", lambda c: c.chart_close(5), Mt5CommandType.ChartClose, {"ChartId": 5}, True, True),
        ("symbol", lambda c: c.chart_symbol(5), Mt5CommandType.ChartSymbol, {"ChartId": 5}, "EURUSD", "EURUSD"),
        ("period", lambda c: c.chart_period(5), Mt5CommandType.ChartPeriod, {"ChartId": 5}, 16385, 16385),
        ("set_double", lambda c: c.chart_set_double(5, PD, 20.5), Mt5CommandType.ChartSetDouble, {"ChartId": 5, "PropId": int(PD), "Value": 20.5}, True, True),
        ("set_integer", lambda c: c.chart_set_integer(5, PI, 3), Mt5CommandType.ChartSetInteger, {"ChartId": 5, "PropId": int(PI), "Value": 3}, True, True),
        ("set_string", lambda c: c.chart_set_string(5, PS, "x"), Mt5CommandType.ChartSetString, {"ChartId": 5, "PropId": int(PS), "Value": "x"}, True, True),
        (
            "get_double",
            lambda c: c.chart_get_double(5, ENUM_CHART_PROPERTY_DOUBLE.CHART_PRICE_MAX, 1),
            Mt5CommandType.ChartGetDouble,
            {"ChartId": 5, "PropId": int(ENUM_CHART_PROPERTY_DOUBLE.CHART_PRICE_MAX), "SubWindow": 1},
            1.9,
            1.9,
        ),
        ("get_integer_default_sub_window", lambda c: c.chart_get_integer(5, PI), Mt5CommandType.ChartGetInteger, {"ChartId": 5, "PropId": int(PI), "SubWindow": 0}, 4, 4),
    ]

    def test_commands(self):
        for name, call, command, payload, reply, expected in self.CASES:
            with self.subTest(name):
                self.fake.reply = reply
                self.assertEqual(expected, call(self.client))
                self.assert_sent(command, payload)

    def test_chart_id_of_default_expert(self):
        self.fake.reply = 123
        self.assertEqual(123, self.client.chart_id())
        self.assert_sent(Mt5CommandType.ChartId, None)  # sent to EXPERT (the default expert)

    def test_chart_id_of_specified_expert(self):
        self.fake.reply = 456
        self.assertEqual(456, self.client.chart_id(EXPERT + 2))
        expert, command, payload = self.fake.sent[-1]
        self.assertEqual(EXPERT + 2, expert)
        self.assertEqual(int(Mt5CommandType.ChartId), int(command))
        self.assertIsNone(payload)

    def test_chart_time_price_to_xy(self):
        self.fake.reply = {"RetVal": True, "Result": {"X": 10, "Y": 20}}
        self.assertEqual((10, 20), self.client.chart_time_price_to_xy(77, 0, T, 1.25))
        self.assert_sent(Mt5CommandType.ChartTimePriceToXY, {"ChartId": 77, "SubWindow": 0, "Time": T, "Price": 1.25})

    def test_chart_time_price_to_xy_failure_returns_none(self):
        self.fake.reply = {"RetVal": False, "Result": {"X": 0, "Y": 0}}
        self.assertIsNone(self.client.chart_time_price_to_xy(77, 0, T, 1.25))
        self.fake.reply = None
        self.assertIsNone(self.client.chart_time_price_to_xy(77, 0, T, 1.25))

    def test_chart_xy_to_time_price(self):
        self.fake.reply = {"RetVal": True, "Result": {"SubWindow": 1, "Time": T, "Price": 1.5}}
        self.assertEqual((1, T, 1.5), self.client.chart_xy_to_time_price(77, 10, 20))
        self.assert_sent(Mt5CommandType.ChartXYToTimePrice, {"ChartId": 77, "X": 10, "Y": 20})

    def test_chart_xy_to_time_price_failure_returns_none(self):
        self.fake.reply = {"RetVal": False}
        self.assertIsNone(self.client.chart_xy_to_time_price(77, 10, 20))
        self.fake.reply = None
        self.assertIsNone(self.client.chart_xy_to_time_price(77, 10, 20))


if __name__ == "__main__":
    unittest.main()
