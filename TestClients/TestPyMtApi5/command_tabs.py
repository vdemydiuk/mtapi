"""Command tabs of the TestPyMtApi5 UI.

Exposes the Mt5ApiClient functions (the same set as 'cmd_functions' in client_cmd.py)
grouped into tabs like in MtApi5TestClient: each tab has shared parameter fields and
a button per function.
"""

from collections.abc import Callable
from datetime import datetime, timedelta, timezone
from enum import IntEnum
from typing import Any

import flet as ft

from PyMtApi5 import Mt5ApiClient, mt5enums

# A command reads its parameters from the UI controls and calls the client.
# It may raise ValueError for invalid parameters.
Command = Callable[[Mt5ApiClient], Any]
# Runs a command for a button: (command name, command).
Execute = Callable[[str, Command], None]

MT_TIME_FORMATS = ("%Y.%m.%d %H:%M:%S", "%Y.%m.%d %H:%M", "%Y.%m.%d")


# --- controls -------------------------------------------------------------------------


def text_field(label: str, value: str = "", width: int = 140) -> ft.TextField:
    return ft.TextField(label=label, value=value, width=width, dense=True)


def enum_dropdown(label: str, enum_cls: type[IntEnum], value: IntEnum | None = None, width: int = 320) -> ft.Dropdown:
    members = list(enum_cls)
    selected = value if value is not None else members[0]
    return ft.Dropdown(
        label=label,
        width=width,
        dense=True,
        options=[ft.dropdown.Option(key=str(m.value), text=m.name) for m in members],
        value=str(selected.value),
    )


def section(title: str, *rows: ft.Control) -> ft.Control:
    return ft.Column(
        [ft.Text(title, weight=ft.FontWeight.BOLD, color=ft.Colors.PRIMARY), *rows],
        spacing=8,
    )


def row(*controls: ft.Control) -> ft.Row:
    return ft.Row(list(controls), wrap=True, spacing=8, vertical_alignment=ft.CrossAxisAlignment.CENTER)


def tab(text: str, *sections: ft.Control) -> ft.Tab:
    controls: list[ft.Control] = []
    for s in sections:
        if controls:
            controls.append(ft.Divider())
        controls.append(s)
    content = ft.Column(controls, scroll=ft.ScrollMode.AUTO, spacing=10)
    return ft.Tab(text=text, content=ft.Container(content=content, padding=10))


# --- parameter readers (called on a worker thread when a command runs) ----------------


def get_str(field: ft.TextField) -> str:
    value = (field.value or "").strip()
    if not value:
        raise ValueError(f"'{field.label}' is empty")
    return value


def get_int(field: ft.TextField) -> int:
    value = get_str(field)
    try:
        return int(value)
    except ValueError:
        raise ValueError(f"'{field.label}': '{value}' is not an integer") from None


def get_float(field: ft.TextField) -> float:
    value = get_str(field)
    try:
        return float(value)
    except ValueError:
        raise ValueError(f"'{field.label}': '{value}' is not a number") from None


def get_mt_time(field: ft.TextField) -> int:
    """MetaTrader time (seconds since 1970, trade server time) from 'YYYY.MM.DD HH:MM[:SS]' or seconds."""
    value = get_str(field)
    if value.lstrip("-").isdigit():
        return int(value)
    for fmt in MT_TIME_FORMATS:
        try:
            return int(datetime.strptime(value, fmt).replace(tzinfo=timezone.utc).timestamp())
        except ValueError:
            pass
    raise ValueError(f"'{field.label}': '{value}' is not a time (YYYY.MM.DD HH:MM:SS or seconds)")


def format_mt_time(mt_time: datetime) -> str:
    return mt_time.strftime(MT_TIME_FORMATS[1])


def get_enum(dropdown: ft.Dropdown, enum_cls: type[IntEnum]) -> Any:
    if dropdown.value is None:
        raise ValueError(f"'{dropdown.label}' is not selected")
    return enum_cls(int(dropdown.value))


def set_value(field: ft.TextField, value: Any):
    field.value = str(value)
    field.update()


# --- tabs -----------------------------------------------------------------------------


class CommandTabs:
    """Builds the command tabs. Commands are run through 'execute'; ChartId uses the
    expert of the quote selected in the quotes list ('get_selected_expert')."""

    def __init__(self, execute: Execute, get_selected_expert: Callable[[], int | None]):
        self.__execute = execute
        self.__get_selected_expert = get_selected_expert
        self.control = ft.Tabs(
            tabs=[
                self.__account_tab(),
                self.__timeseries_tab(),
                self.__market_info_tab(),
                self.__chart_tab(),
            ],
            selected_index=0,
            animation_duration=150,
            expand=True,
        )

    def __button(self, name: str, command: Command) -> ft.OutlinedButton:
        return ft.OutlinedButton(name, on_click=lambda _: self.__execute(name, command))

    # Account Information

    def __account_tab(self) -> ft.Tab:
        prop_double = enum_dropdown("Property", mt5enums.ENUM_ACCOUNT_INFO_DOUBLE)
        prop_integer = enum_dropdown("Property", mt5enums.ENUM_ACCOUNT_INFO_INTEGER)
        prop_string = enum_dropdown("Property", mt5enums.ENUM_ACCOUNT_INFO_STRING)
        return tab(
            "Account Information",
            section(
                "Account properties",
                row(prop_double, self.__button(
                    "AccountInfoDouble",
                    lambda c: c.account_info_double(get_enum(prop_double, mt5enums.ENUM_ACCOUNT_INFO_DOUBLE)),
                )),
                row(prop_integer, self.__button(
                    "AccountInfoInteger",
                    lambda c: c.account_info_integer(get_enum(prop_integer, mt5enums.ENUM_ACCOUNT_INFO_INTEGER)),
                )),
                row(prop_string, self.__button(
                    "AccountInfoString",
                    lambda c: c.account_info_string(get_enum(prop_string, mt5enums.ENUM_ACCOUNT_INFO_STRING)),
                )),
            ),
        )

    # Timeseries and Indicators Access

    def __timeseries_tab(self) -> ft.Tab:
        now = datetime.now()
        symbol = text_field("Symbol", "EURUSD")
        timeframe = enum_dropdown("Timeframe", mt5enums.ENUM_TIMEFRAMES, mt5enums.ENUM_TIMEFRAMES.PERIOD_H1, width=200)
        start_pos = text_field("Start pos", "0", width=100)
        count = text_field("Count", "10", width=100)
        start_time = text_field("Start time", format_mt_time(now - timedelta(days=1)), width=180)
        stop_time = text_field("Stop time", format_mt_time(now), width=180)
        series_prop = enum_dropdown("Series property", mt5enums.ENUM_SERIES_INFO_INTEGER)
        ticks_flag = enum_dropdown("Ticks flag", mt5enums.CopyTicksFlag, mt5enums.CopyTicksFlag.All, width=160)
        ticks_from = text_field("From (msc)", "0", width=180)

        indicator_type = enum_dropdown("Indicator type", mt5enums.ENUM_INDICATOR, width=240)
        indicator_handle = text_field("Indicator handle", "", width=150)
        buffer_num = text_field("Buffer num", "0", width=100)

        def tf():
            return get_enum(timeframe, mt5enums.ENUM_TIMEFRAMES)

        def copy_command(func: Callable[..., Any]) -> Command:
            return lambda c: func(c, get_str(symbol), tf(), get_int(start_pos), get_int(count))

        copy_functions = [
            ("CopyRates", Mt5ApiClient.copy_rates),
            ("CopyTime", Mt5ApiClient.copy_time),
            ("CopyOpen", Mt5ApiClient.copy_open),
            ("CopyHigh", Mt5ApiClient.copy_high),
            ("CopyLow", Mt5ApiClient.copy_low),
            ("CopyClose", Mt5ApiClient.copy_close),
            ("CopyTickVolume", Mt5ApiClient.copy_tick_volume),
            ("CopyRealVolume", Mt5ApiClient.copy_real_volume),
            ("CopySpread", Mt5ApiClient.copy_spread),
        ]

        def indicator_create(c: Mt5ApiClient):
            handle = c.indicator_create(get_str(symbol), tf(), get_enum(indicator_type, mt5enums.ENUM_INDICATOR))
            if isinstance(handle, int) and handle >= 0:
                set_value(indicator_handle, handle)
            return handle

        return tab(
            "Timeseries and Indicators",
            section(
                "Timeseries",
                row(symbol, timeframe, start_pos, count),
                row(*[self.__button(name, copy_command(func)) for name, func in copy_functions]),
                row(series_prop, self.__button(
                    "SeriesInfoInteger",
                    lambda c: c.series_info_integer(
                        get_str(symbol), tf(), get_enum(series_prop, mt5enums.ENUM_SERIES_INFO_INTEGER)
                    ),
                )),
                row(
                    self.__button("Bars", lambda c: c.bars(get_str(symbol), tf())),
                    start_time,
                    stop_time,
                    self.__button(
                        "BarsPeriod",
                        lambda c: c.bars_period(get_str(symbol), tf(), get_mt_time(start_time), get_mt_time(stop_time)),
                    ),
                ),
                row(ticks_flag, ticks_from, self.__button(
                    "CopyTicks",
                    lambda c: c.copy_ticks(
                        get_str(symbol), get_enum(ticks_flag, mt5enums.CopyTicksFlag), get_int(ticks_from), get_int(count)
                    ),
                )),
            ),
            section(
                "Indicators (symbol and timeframe from above)",
                row(indicator_type, self.__button("IndicatorCreate", indicator_create)),
                row(
                    indicator_handle,
                    self.__button("IndicatorRelease", lambda c: c.indicator_release(get_int(indicator_handle))),
                    self.__button("BarsCalculated", lambda c: c.bars_calculated(get_int(indicator_handle))),
                ),
                row(buffer_num, self.__button(
                    "CopyBuffer",
                    lambda c: c.copy_buffer(get_int(indicator_handle), get_int(buffer_num), get_int(start_pos), get_int(count)),
                )),
            ),
        )

    # Market Info

    def __market_info_tab(self) -> ft.Tab:
        symbol = text_field("Symbol", "EURUSD")
        selected = ft.Checkbox(label="Selected", value=True)
        position = text_field("Position", "0", width=100)
        prop_double = enum_dropdown("Property", mt5enums.ENUM_SYMBOL_INFO_DOUBLE)
        prop_integer = enum_dropdown("Property", mt5enums.ENUM_SYMBOL_INFO_INTEGER)
        prop_string = enum_dropdown("Property", mt5enums.ENUM_SYMBOL_INFO_STRING)
        day_of_week = enum_dropdown("Day of week", mt5enums.ENUM_DAY_OF_WEEK, mt5enums.ENUM_DAY_OF_WEEK.MONDAY, width=180)
        session_index = text_field("Session index", "0", width=120)

        def is_selected() -> bool:
            return bool(selected.value)

        def session_args():
            return get_str(symbol), get_enum(day_of_week, mt5enums.ENUM_DAY_OF_WEEK), get_int(session_index)

        return tab(
            "Market Info",
            section(
                "Symbols",
                row(selected, position),
                row(
                    self.__button("SymbolsTotal", lambda c: c.symbols_total(is_selected())),
                    self.__button("SymbolName", lambda c: c.symbol_name(get_int(position), is_selected())),
                ),
            ),
            section(
                "Symbol",
                row(
                    symbol,
                    self.__button("SymbolSelect", lambda c: c.symbol_select(get_str(symbol), is_selected())),
                    self.__button("SymbolIsSynchronized", lambda c: c.symbol_is_synchronized(get_str(symbol))),
                    self.__button("SymbolInfoTick", lambda c: c.symbol_info_tick(get_str(symbol))),
                ),
                row(prop_double, self.__button(
                    "SymbolInfoDouble",
                    lambda c: c.symbol_info_double(get_str(symbol), get_enum(prop_double, mt5enums.ENUM_SYMBOL_INFO_DOUBLE)),
                )),
                row(prop_integer, self.__button(
                    "SymbolInfoInteger",
                    lambda c: c.symbol_info_integer(get_str(symbol), get_enum(prop_integer, mt5enums.ENUM_SYMBOL_INFO_INTEGER)),
                )),
                row(prop_string, self.__button(
                    "SymbolInfoString",
                    lambda c: c.symbol_info_string(get_str(symbol), get_enum(prop_string, mt5enums.ENUM_SYMBOL_INFO_STRING)),
                )),
                row(
                    day_of_week,
                    session_index,
                    self.__button("SymbolInfoSessionQuote", lambda c: c.symbol_info_session_quote(*session_args())),
                    self.__button("SymbolInfoSessionTrade", lambda c: c.symbol_info_session_trade(*session_args())),
                ),
            ),
            section(
                "Market depth (symbol from above)",
                row(
                    self.__button("MarketBookAdd", lambda c: c.market_book_add(get_str(symbol))),
                    self.__button("MarketBookRelease", lambda c: c.market_book_release(get_str(symbol))),
                    self.__button("MarketBookGet", lambda c: c.market_book_get(get_str(symbol))),
                ),
            ),
        )

    # Chart Functions

    def __chart_tab(self) -> ft.Tab:
        chart_id = text_field("Chart ID (0 = current)", "0", width=220)
        symbol = text_field("Symbol", "EURUSD")
        timeframe = enum_dropdown("Timeframe", mt5enums.ENUM_TIMEFRAMES, mt5enums.ENUM_TIMEFRAMES.PERIOD_H1, width=200)
        template = text_field("Template file", "test.tpl", width=200)
        indicator_name = text_field("Indicator short name", "", width=220)
        sub_window = text_field("Sub-window", "0", width=110)
        time = text_field("Time", format_mt_time(datetime.now()), width=180)
        price = text_field("Price", "1.1", width=120)
        x = text_field("X", "100", width=90)
        y = text_field("Y", "100", width=90)
        prop_double = enum_dropdown("Property", mt5enums.ENUM_CHART_PROPERTY_DOUBLE)
        prop_integer = enum_dropdown("Property", mt5enums.ENUM_CHART_PROPERTY_INTEGER)
        prop_string = enum_dropdown("Property", mt5enums.ENUM_CHART_PROPERTY_STRING)
        value = text_field("Value", "", width=200)

        def cid() -> int:
            return get_int(chart_id)

        def updates_chart_id(command: Command) -> Command:
            """Put the chart id returned by the command into the 'Chart ID' field."""

            def wrapper(c: Mt5ApiClient):
                result = command(c)
                if isinstance(result, int) and result > 0:
                    set_value(chart_id, result)
                return result

            return wrapper

        def chart_id_of_selected_expert(c: Mt5ApiClient):
            # ChartId of the expert selected in the quotes list, or of the default expert.
            return c.chart_id(self.__get_selected_expert() or 0)

        return tab(
            "Chart Functions",
            section(
                "Charts (ChartId uses the expert selected in the quotes list)",
                row(
                    chart_id,
                    self.__button("ChartId", updates_chart_id(chart_id_of_selected_expert)),
                    self.__button("ChartFirst", updates_chart_id(lambda c: c.chart_first())),
                    self.__button("ChartNext", updates_chart_id(lambda c: c.chart_next(cid()))),
                ),
                row(
                    self.__button("ChartSymbol", lambda c: c.chart_symbol(cid())),
                    self.__button("ChartPeriod", lambda c: c.chart_period(cid())),
                    self.__button("ChartRedraw", lambda c: c.chart_redraw(cid())),
                    self.__button("ChartClose", lambda c: c.chart_close(cid())),
                ),
                row(symbol, timeframe, self.__button(
                    "ChartOpen",
                    updates_chart_id(lambda c: c.chart_open(get_str(symbol), get_enum(timeframe, mt5enums.ENUM_TIMEFRAMES))),
                )),
            ),
            section(
                "Templates and windows (Chart ID from above)",
                row(
                    template,
                    self.__button("ChartApplyTemplate", lambda c: c.chart_apply_template(cid(), get_str(template))),
                    self.__button("ChartSaveTemplate", lambda c: c.chart_save_template(cid(), get_str(template))),
                ),
                row(indicator_name, self.__button(
                    "ChartWindowFind", lambda c: c.chart_window_find(cid(), get_str(indicator_name))
                )),
            ),
            section(
                "Coordinates (Chart ID from above)",
                row(sub_window, time, price, self.__button(
                    "ChartTimePriceToXY",
                    lambda c: c.chart_time_price_to_xy(cid(), get_int(sub_window), get_mt_time(time), get_float(price)),
                )),
                row(x, y, self.__button(
                    "ChartXYToTimePrice", lambda c: c.chart_xy_to_time_price(cid(), get_int(x), get_int(y))
                )),
            ),
            section(
                "Properties (Chart ID and Sub-window from above)",
                row(value),
                row(
                    prop_double,
                    self.__button(
                        "ChartSetDouble",
                        lambda c: c.chart_set_double(
                            cid(), get_enum(prop_double, mt5enums.ENUM_CHART_PROPERTY_DOUBLE), get_float(value)
                        ),
                    ),
                    self.__button(
                        "ChartGetDouble",
                        lambda c: c.chart_get_double(
                            cid(), get_enum(prop_double, mt5enums.ENUM_CHART_PROPERTY_DOUBLE), get_int(sub_window)
                        ),
                    ),
                ),
                row(
                    prop_integer,
                    self.__button(
                        "ChartSetInteger",
                        lambda c: c.chart_set_integer(
                            cid(), get_enum(prop_integer, mt5enums.ENUM_CHART_PROPERTY_INTEGER), get_int(value)
                        ),
                    ),
                    self.__button(
                        "ChartGetInteger",
                        lambda c: c.chart_get_integer(
                            cid(), get_enum(prop_integer, mt5enums.ENUM_CHART_PROPERTY_INTEGER), get_int(sub_window)
                        ),
                    ),
                ),
                row(prop_string, self.__button(
                    "ChartSetString",
                    lambda c: c.chart_set_string(
                        cid(), get_enum(prop_string, mt5enums.ENUM_CHART_PROPERTY_STRING), value.value or ""
                    ),
                )),
            ),
        )
