"""Unit tests for the chart object functions of Mt5ApiClient (see test_trading.py for the approach)."""

import unittest

from PyMtApi5 import (
    ENUM_OBJECT,
    ENUM_OBJECT_PROPERTY_DOUBLE,
    ENUM_OBJECT_PROPERTY_INTEGER,
    ENUM_OBJECT_PROPERTY_STRING,
    Mt5CommandType,
)

from .test_trading import TradingTestCase

T = 1700000000
PD = ENUM_OBJECT_PROPERTY_DOUBLE.OBJPROP_PRICE
PI = ENUM_OBJECT_PROPERTY_INTEGER.OBJPROP_COLOR
PS = ENUM_OBJECT_PROPERTY_STRING.OBJPROP_TEXT


class ObjectTests(TradingTestCase):
    """(name, method call, command, payload, reply, expected return value)."""

    CASES = [
        ("name_defaults", lambda c: c.object_name(5, 2), Mt5CommandType.ObjectName, {"ChartId": 5, "Pos": 2, "SubWindow": -1, "Type": -1}, "o1", "o1"),
        ("delete", lambda c: c.object_delete(5, "o1"), Mt5CommandType.ObjectDelete, {"ChartId": 5, "Name": "o1"}, True, True),
        ("delete_all_defaults", lambda c: c.objects_delete_all(5), Mt5CommandType.ObjectsDeleteAll, {"ChartId": 5, "SubWindow": -1, "Type": -1}, 3, 3),
        ("find", lambda c: c.object_find(5, "o1"), Mt5CommandType.ObjectFind, {"ChartId": 5, "Name": "o1"}, 0, 0),
        ("get_time_by_value", lambda c: c.object_get_time_by_value(5, "o1", 1.5, 0), Mt5CommandType.ObjectGetTimeByValue, {"ChartId": 5, "Name": "o1", "Value": 1.5, "LineId": 0}, T, T),
        ("get_value_by_time", lambda c: c.object_get_value_by_time(5, "o1", T, 0), Mt5CommandType.ObjectGetValueByTime, {"ChartId": 5, "Name": "o1", "Time": T, "LineId": 0}, 1.75, 1.75),
        ("move", lambda c: c.object_move(5, "o1", 1, T, 1.5), Mt5CommandType.ObjectMove, {"ChartId": 5, "Name": "o1", "PointIndex": 1, "Time": T, "Price": 1.5}, True, True),
        ("total_defaults", lambda c: c.objects_total(5), Mt5CommandType.ObjectsTotal, {"ChartId": 5, "SubWindow": -1, "Type": -1}, 9, 9),
        ("total_filtered", lambda c: c.objects_total(5, 1, 2), Mt5CommandType.ObjectsTotal, {"ChartId": 5, "SubWindow": 1, "Type": 2}, 9, 9),
        ("set_double", lambda c: c.object_set_double(5, "o1", PD, 1.5), Mt5CommandType.ObjectSetDouble, {"ChartId": 5, "Name": "o1", "PropId": int(PD), "PropValue": 1.5}, True, True),
        ("set_integer", lambda c: c.object_set_integer(5, "o1", PI, 255), Mt5CommandType.ObjectSetInteger, {"ChartId": 5, "Name": "o1", "PropId": int(PI), "PropValue": 255}, True, True),
        ("set_string", lambda c: c.object_set_string(5, "o1", PS, "t"), Mt5CommandType.ObjectSetString, {"ChartId": 5, "Name": "o1", "PropId": int(PS), "PropValue": "t"}, True, True),
        ("get_double", lambda c: c.object_get_double(5, "o1", PD), Mt5CommandType.ObjectGetDouble, {"ChartId": 5, "Name": "o1", "PropId": int(PD)}, 1.5, 1.5),
        ("get_integer", lambda c: c.object_get_integer(5, "o1", PI), Mt5CommandType.ObjectGetInteger, {"ChartId": 5, "Name": "o1", "PropId": int(PI)}, 255, 255),
        ("get_string", lambda c: c.object_get_string(5, "o1", PS), Mt5CommandType.ObjectGetString, {"ChartId": 5, "Name": "o1", "PropId": int(PS)}, "t", "t"),
    ]

    def test_commands(self):
        for name, call, command, payload, reply, expected in self.CASES:
            with self.subTest(name):
                self.fake.reply = reply
                self.assertEqual(expected, call(self.client))
                self.assert_sent(command, payload)

    def test_object_create_first_point_only(self):
        self.fake.reply = True
        self.assertTrue(self.client.object_create(0, "o", ENUM_OBJECT.OBJ_VLINE, 0, T, 1.0))
        self.assert_sent(
            Mt5CommandType.ObjectCreate,
            {"ChartId": 0, "Name": "o", "Type": int(ENUM_OBJECT.OBJ_VLINE), "Nwin": 0, "Times": [T], "Prices": [1.0]},
        )

    def test_object_create_additional_coordinates_follow_first_point(self):
        self.fake.reply = True
        self.client.object_create(0, "line", ENUM_OBJECT.OBJ_TREND, 0, T, 1.0, [(T + 3600, 2.0), (T + 7200, 3.0)])
        payload = self.fake.sent[-1][2]
        self.assertEqual([T, T + 3600, T + 7200], payload["Times"])
        self.assertEqual([1.0, 2.0, 3.0], payload["Prices"])

    def test_object_create_limits_additional_coordinates_to_29(self):
        self.fake.reply = True
        self.assertTrue(self.client.object_create(0, "o", ENUM_OBJECT.OBJ_TREND, 0, T, 1.0, [(T, 1.0)] * 29))
        self.assertEqual(30, len(self.fake.sent[-1][2]["Times"]))
        sent = len(self.fake.sent)
        with self.assertRaises(ValueError):
            self.client.object_create(0, "o", ENUM_OBJECT.OBJ_TREND, 0, T, 1.0, [(T, 1.0)] * 30)
        self.assertEqual(sent, len(self.fake.sent))  # nothing was sent


if __name__ == "__main__":
    unittest.main()
