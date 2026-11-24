"""Unit tests for the technical indicator functions (iXxx) of Mt5ApiClient (see test_trading.py for the approach).

The sample values and expected payloads mirror MtApi5.Tests/MtApi5ClientIndicatorTests.cs.
"""

import unittest

from PyMtApi5 import (
    ENUM_APPLIED_PRICE,
    ENUM_APPLIED_VOLUME,
    ENUM_MA_METHOD,
    ENUM_STO_PRICE,
    ENUM_TIMEFRAMES,
    Mt5ApiClient,
    Mt5CommandType,
    ParametersType,
)

from .test_trading import TradingTestCase

TF = ENUM_TIMEFRAMES.PERIOD_H1
AP = ENUM_APPLIED_PRICE.PRICE_HIGH
AV = ENUM_APPLIED_VOLUME.VOLUME_REAL
MM = ENUM_MA_METHOD.MODE_EMA
STO = ENUM_STO_PRICE.STO_CLOSECLOSE

# (C# name, call, command, payload)
INDICATORS = [
        ("iAC", lambda c: c.i_ac("EURUSD", TF), Mt5CommandType.iAC, {"Symbol": "EURUSD", "Period": int(TF)}),
        ("iAD", lambda c: c.i_ad("EURUSD", TF, AV), Mt5CommandType.iAD, {"Symbol": "EURUSD", "Period": int(TF), "AppliedVolume": int(AV)}),
        ("iADX", lambda c: c.i_adx("EURUSD", TF, 14), Mt5CommandType.iADX, {"Symbol": "EURUSD", "Period": int(TF), "AdxPeriod": 14}),
        ("iADXWilder", lambda c: c.i_adx_wilder("EURUSD", TF, 14), Mt5CommandType.iADXWilder, {"Symbol": "EURUSD", "Period": int(TF), "AdxPeriod": 14}),
        ("iAlligator", lambda c: c.i_alligator("EURUSD", TF, 13, 8, 8, 5, 5, 3, MM, AP), Mt5CommandType.iAlligator, {"Symbol": "EURUSD", "Period": int(TF), "JawPeriod": 13, "JawShift": 8, "TeethPeriod": 8, "TeethShift": 5, "LipsPeriod": 5, "LipsShift": 3, "MaMethod": int(MM), "AppliedPrice": int(AP)}),
        ("iAMA", lambda c: c.i_ama("EURUSD", TF, 9, 2, 30, 1, AP), Mt5CommandType.iAMA, {"Symbol": "EURUSD", "Period": int(TF), "AmaPeriod": 9, "FastMaPeriod": 2, "SlowMaPeriod": 30, "AmaShift": 1, "AppliedPrice": int(AP)}),
        ("iAO", lambda c: c.i_ao("EURUSD", TF), Mt5CommandType.iAO, {"Symbol": "EURUSD", "Period": int(TF)}),
        ("iATR", lambda c: c.i_atr("EURUSD", TF, 14), Mt5CommandType.iATR, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 14}),
        ("iBearsPower", lambda c: c.i_bears_power("EURUSD", TF, 13), Mt5CommandType.iBearsPower, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 13}),
        ("iBands", lambda c: c.i_bands("EURUSD", TF, 20, 1, 2.0, AP), Mt5CommandType.iBands, {"Symbol": "EURUSD", "Period": int(TF), "BandsPeriod": 20, "BandsShift": 1, "Deviation": 2.0, "AppliedPrice": int(AP)}),
        ("iBullsPower", lambda c: c.i_bulls_power("EURUSD", TF, 13), Mt5CommandType.iBullsPower, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 13}),
        ("iCCI", lambda c: c.i_cci("EURUSD", TF, 14, AP), Mt5CommandType.iCCI, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 14, "AppliedPrice": int(AP)}),
        ("iChaikin", lambda c: c.i_chaikin("EURUSD", TF, 3, 10, MM, AV), Mt5CommandType.iChaikin, {"Symbol": "EURUSD", "Period": int(TF), "FastMaPeriod": 3, "SlowMaPeriod": 10, "MaMethod": int(MM), "AppliedVolume": int(AV)}),
        ("iDEMA", lambda c: c.i_dema("EURUSD", TF, 14, 2, AP), Mt5CommandType.iDEMA, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 14, "MaShift": 2, "AppliedPrice": int(AP)}),
        ("iDeMarker", lambda c: c.i_de_marker("EURUSD", TF, 14), Mt5CommandType.iDeMarker, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 14}),
        ("iEnvelopes", lambda c: c.i_envelopes("EURUSD", TF, 14, 2, MM, AP, 0.1), Mt5CommandType.iEnvelopes, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 14, "MaShift": 2, "MaMethod": int(MM), "AppliedPrice": int(AP), "Deviation": 0.1}),
        ("iForce", lambda c: c.i_force("EURUSD", TF, 13, MM, AV), Mt5CommandType.iForce, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 13, "MaMethod": int(MM), "AppliedVolume": int(AV)}),
        ("iFractals", lambda c: c.i_fractals("EURUSD", TF), Mt5CommandType.iFractals, {"Symbol": "EURUSD", "Period": int(TF)}),
        ("iFrAMA", lambda c: c.i_frama("EURUSD", TF, 14, 2, AP), Mt5CommandType.iFrAMA, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 14, "MaShift": 2, "AppliedPrice": int(AP)}),
        ("iGator", lambda c: c.i_gator("EURUSD", TF, 13, 8, 8, 5, 5, 3, MM, AP), Mt5CommandType.iGator, {"Symbol": "EURUSD", "Period": int(TF), "JawPeriod": 13, "JawShift": 8, "TeethPeriod": 8, "TeethShift": 5, "LipsPeriod": 5, "LipsShift": 3, "MaMethod": int(MM), "AppliedPrice": int(AP)}),
        ("iIchimoku", lambda c: c.i_ichimoku("EURUSD", TF, 9, 26, 52), Mt5CommandType.iIchimoku, {"Symbol": "EURUSD", "Period": int(TF), "TenkanSen": 9, "KijunSen": 26, "SenkouSpanB": 52}),
        ("iBWMFI", lambda c: c.i_bwmfi("EURUSD", TF, AV), Mt5CommandType.iBWMFI, {"Symbol": "EURUSD", "Period": int(TF), "AppliedVolume": int(AV)}),
        ("iMomentum", lambda c: c.i_momentum("EURUSD", TF, 14, AP), Mt5CommandType.iMomentum, {"Symbol": "EURUSD", "Period": int(TF), "MomPeriod": 14, "AppliedPrice": int(AP)}),
        ("iMFI", lambda c: c.i_mfi("EURUSD", TF, 14, AV), Mt5CommandType.iMFI, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 14, "AppliedVolume": int(AV)}),
        ("iMA", lambda c: c.i_ma("EURUSD", TF, 14, 2, MM, AP), Mt5CommandType.iMA, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 14, "MaShift": 2, "MaMethod": int(MM), "AppliedPrice": int(AP)}),
        ("iOsMA", lambda c: c.i_osma("EURUSD", TF, 12, 26, 9, AP), Mt5CommandType.iOsMA, {"Symbol": "EURUSD", "Period": int(TF), "FastEmaPeriod": 12, "SlowEmaPeriod": 26, "SignalPeriod": 9, "AppliedPrice": int(AP)}),
        ("iMACD", lambda c: c.i_macd("EURUSD", TF, 12, 26, 9, AP), Mt5CommandType.iMACD, {"Symbol": "EURUSD", "Period": int(TF), "FastEmaPeriod": 12, "SlowEmaPeriod": 26, "SignalPeriod": 9, "AppliedPrice": int(AP)}),
        ("iOBV", lambda c: c.i_obv("EURUSD", TF, AV), Mt5CommandType.iOBV, {"Symbol": "EURUSD", "Period": int(TF), "AppliedVolume": int(AV)}),
        ("iSAR", lambda c: c.i_sar("EURUSD", TF, 0.02, 0.2), Mt5CommandType.iSAR, {"Symbol": "EURUSD", "Period": int(TF), "Step": 0.02, "Maximum": 0.2}),
        ("iRSI", lambda c: c.i_rsi("EURUSD", TF, 14, AP), Mt5CommandType.iRSI, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 14, "AppliedPrice": int(AP)}),
        ("iRVI", lambda c: c.i_rvi("EURUSD", TF, 10), Mt5CommandType.iRVI, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 10}),
        ("iStdDev", lambda c: c.i_std_dev("EURUSD", TF, 20, 1, MM, AP), Mt5CommandType.iStdDev, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 20, "MaShift": 1, "MaMethod": int(MM), "AppliedPrice": int(AP)}),
        ("iStochastic", lambda c: c.i_stochastic("EURUSD", TF, 5, 3, 3, MM, STO), Mt5CommandType.iStochastic, {"Symbol": "EURUSD", "Period": int(TF), "Kperiod": 5, "Dperiod": 3, "Slowing": 3, "MaMethod": int(MM), "PriceField": int(STO)}),
        ("iTEMA", lambda c: c.i_tema("EURUSD", TF, 14, 2, AP), Mt5CommandType.iTEMA, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 14, "MaShift": 2, "AppliedPrice": int(AP)}),
        ("iTriX", lambda c: c.i_trix("EURUSD", TF, 14, AP), Mt5CommandType.iTriX, {"Symbol": "EURUSD", "Period": int(TF), "MaPeriod": 14, "AppliedPrice": int(AP)}),
        ("iWPR", lambda c: c.i_wpr("EURUSD", TF, 14), Mt5CommandType.iWPR, {"Symbol": "EURUSD", "Period": int(TF), "CalcPeriod": 14}),
        ("iVIDyA", lambda c: c.i_vidya("EURUSD", TF, 9, 12, 2, AP), Mt5CommandType.iVIDyA, {"Symbol": "EURUSD", "Period": int(TF), "CmoPeriod": 9, "EmaPeriod": 12, "MaShift": 2, "AppliedPrice": int(AP)}),
        ("iVolumes", lambda c: c.i_volumes("EURUSD", TF, AV), Mt5CommandType.iVolumes, {"Symbol": "EURUSD", "Period": int(TF), "AppliedVolume": int(AV)}),
]


class IndicatorTests(TradingTestCase):
    def test_indicators_send_expected_payload_and_return_handle(self):
        for name, call, command, payload in INDICATORS:
            with self.subTest(name):
                self.fake.reply = 321
                self.assertEqual(321, call(self.client))
                self.assert_sent(command, payload)

    def test_table_covers_every_indicator_function(self):
        # Guards against adding a new i_xxx function without a test row.
        covered = {name.lower() for name, *_ in INDICATORS}
        all_names = {
            m.replace("_", "")
            for m in dir(Mt5ApiClient)
            if m.startswith("i_") and m != "i_custom" and callable(getattr(Mt5ApiClient, m))
        }
        self.assertEqual(all_names, covered)

    def test_error_reply_raises(self):
        self.fake.raw_reply = '{"ErrorCode":4802,"ErrorMessage":"Indicator cannot be created"}'
        with self.assertRaises(Exception) as ctx:
            self.client.i_rsi("EURUSD", TF, 14, AP)
        self.assertIn("Indicator cannot be created", str(ctx.exception))


class CustomIndicatorTests(TradingTestCase):
    def assert_custom(self, params, params_type):
        self.fake.reply = 7
        self.assertEqual(7, self.client.i_custom("EURUSD", ENUM_TIMEFRAMES.PERIOD_M1, "Ind", params))
        self.assert_sent(
            Mt5CommandType.iCustom,
            {
                "Symbol": "EURUSD",
                "Timeframe": int(ENUM_TIMEFRAMES.PERIOD_M1),
                "Name": "Ind",
                "Params": params,
                "ParamsType": int(params_type),
            },
        )

    def test_double_parameters(self):
        self.assert_custom([1.5, 2.5], ParametersType.Double)

    def test_int_parameters(self):
        self.assert_custom([1, 2], ParametersType.Int)

    def test_string_parameters(self):
        self.assert_custom(["a", "b"], ParametersType.String)

    def test_bool_parameters_are_not_ints(self):
        self.assert_custom([True, False], ParametersType.Boolean)

    def test_ints_mixed_with_floats_are_doubles(self):
        self.assert_custom([1, 2.5], ParametersType.Double)

    def test_no_parameters(self):
        self.assert_custom([], ParametersType.Int)

    def test_mixed_kinds_raise_without_sending(self):
        with self.assertRaises(ValueError):
            self.client.i_custom("EURUSD", ENUM_TIMEFRAMES.PERIOD_M1, "Ind", [1, "a"])
        with self.assertRaises(ValueError):
            self.client.i_custom("EURUSD", ENUM_TIMEFRAMES.PERIOD_M1, "Ind", [True, 1])
        self.assertEqual([], self.fake.sent)


if __name__ == "__main__":
    unittest.main()
