"""Unit tests for the single-bar timeseries access functions of Mt5ApiClient (i_close, i_open, i_bar_shift,
i_highest, ...); see test_trading.py for the approach.

The sample values and expected payloads mirror MtApi5.Tests/MtApi5ClientTimeseriesTests.cs.
"""

import unittest

from PyMtApi5 import ENUM_SERIESMODE, ENUM_TIMEFRAMES, Mt5ApiClient, Mt5CommandType

from .test_trading import TradingTestCase

TF = ENUM_TIMEFRAMES.PERIOD_H1
MT_TIME = 1700000000


def shift(value):
    return {"Symbol": "EURUSD", "Timeframe": int(TF), "Shift": value}


# (C# name, call, command, payload, reply, expected return value)
TIMESERIES = [
    ("iBars", lambda c: c.i_bars("EURUSD", TF), Mt5CommandType.iBars, {"Symbol": "EURUSD", "Timeframe": int(TF)}, 500, 500),
    ("iBarShift", lambda c: c.i_bar_shift("EURUSD", TF, MT_TIME, True), Mt5CommandType.iBarShift,
     {"Symbol": "EURUSD", "Timeframe": int(TF), "Time": MT_TIME, "Exact": True}, 7, 7),
    ("iClose", lambda c: c.i_close("EURUSD", TF, 1), Mt5CommandType.iClose, shift(1), 1.1, 1.1),
    ("iHigh", lambda c: c.i_high("EURUSD", TF, 2), Mt5CommandType.iHigh, shift(2), 1.2, 1.2),
    ("iHighest", lambda c: c.i_highest("EURUSD", TF, ENUM_SERIESMODE.MODE_HIGH, 10, 1), Mt5CommandType.iHighest,
     {"Symbol": "EURUSD", "Timeframe": int(TF), "Type": int(ENUM_SERIESMODE.MODE_HIGH), "Count": 10, "Start": 1}, 4, 4),
    ("iLow", lambda c: c.i_low("EURUSD", TF, 3), Mt5CommandType.iLow, shift(3), 1.05, 1.05),
    ("iLowest", lambda c: c.i_lowest("EURUSD", TF, ENUM_SERIESMODE.MODE_LOW, 20, 2), Mt5CommandType.iLowest,
     {"Symbol": "EURUSD", "Timeframe": int(TF), "Type": int(ENUM_SERIESMODE.MODE_LOW), "Count": 20, "Start": 2}, 9, 9),
    ("iOpen", lambda c: c.i_open("EURUSD", TF, 0), Mt5CommandType.iOpen, shift(0), 1.15, 1.15),
    ("iTime", lambda c: c.i_time("EURUSD", TF, 4), Mt5CommandType.iTime, shift(4), MT_TIME, MT_TIME),
    ("iTickVolume", lambda c: c.i_tick_volume("EURUSD", TF, 5), Mt5CommandType.iTickVolume, shift(5), 1234, 1234),
    ("iRealVolume", lambda c: c.i_real_volume("EURUSD", TF, 6), Mt5CommandType.iRealVolume, shift(6), 5000000000, 5000000000),
    ("iVolume", lambda c: c.i_volume("EURUSD", TF, 7), Mt5CommandType.iVolume, shift(7), 321, 321),
    ("iSpread", lambda c: c.i_spread("EURUSD", TF, 8), Mt5CommandType.iSpread, shift(8), 12, 12),
]

# Python method names of the timeseries functions; test_indicators.py excludes them from its coverage check.
TIMESERIES_METHODS = {
    "i_bars", "i_bar_shift", "i_close", "i_high", "i_highest", "i_low", "i_lowest",
    "i_open", "i_time", "i_tick_volume", "i_real_volume", "i_volume", "i_spread",
}


class TimeseriesTests(TradingTestCase):
    def test_functions_send_expected_payload_and_return_reply(self):
        for name, call, command, payload, reply, expected in TIMESERIES:
            with self.subTest(name):
                self.fake.reply = reply
                self.assertEqual(expected, call(self.client))
                self.assert_sent(command, payload)

    def test_table_covers_every_timeseries_function(self):
        covered = {name.lower() for name, *_ in TIMESERIES}
        self.assertEqual({m.replace("_", "") for m in TIMESERIES_METHODS}, covered)
        for m in TIMESERIES_METHODS:
            self.assertTrue(callable(getattr(Mt5ApiClient, m, None)), m)

    def test_defaults_match_mql5(self):
        self.fake.reply = 0
        self.client.i_bar_shift("EURUSD", TF, MT_TIME)
        self.assert_sent(Mt5CommandType.iBarShift, {"Symbol": "EURUSD", "Timeframe": int(TF), "Time": MT_TIME, "Exact": False})
        for method, command in (("i_highest", Mt5CommandType.iHighest), ("i_lowest", Mt5CommandType.iLowest)):
            with self.subTest(method):
                getattr(self.client, method)("EURUSD", TF, ENUM_SERIESMODE.MODE_CLOSE)
                self.assert_sent(command, {"Symbol": "EURUSD", "Timeframe": int(TF),
                                           "Type": int(ENUM_SERIESMODE.MODE_CLOSE), "Count": -1, "Start": 0})

    def test_none_symbol_sends_empty_string(self):
        self.fake.reply = 1.1
        self.client.i_close(None, TF, 0)
        self.assert_sent(Mt5CommandType.iClose, {"Symbol": "", "Timeframe": int(TF), "Shift": 0})

    def test_series_mode_matches_mql5(self):
        self.assertEqual(
            [0, 1, 2, 3, 4, 7, 6],
            [int(ENUM_SERIESMODE[n]) for n in ("MODE_OPEN", "MODE_LOW", "MODE_HIGH", "MODE_CLOSE",
                                                "MODE_VOLUME", "MODE_REAL_VOLUME", "MODE_SPREAD")],
        )


if __name__ == "__main__":
    unittest.main()
