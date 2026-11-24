"""PyMtApi5 - Python client for MetaTrader 5 via MtApi5 WebSocket bridge."""

from .mt5apiclient import (
    Mt5ApiClient,
    Mt5EventType,
    Mt5Quote,
    MqlTick,
    MqlRates,
    MqlTradeTransaction,
    MqlTradeRequest,
    MqlTradeResult,
    MqlTradeCheckResult,
    MqlBookInfo,
)
from .mt5commandtype import Mt5CommandType
from .mtrpcclient import MtRpcClient, MtRpcError
from .mtprotocol_sbe import ResponseStatus
from .mt5enums import *  # noqa: F401,F403  (ENUM_* types, CopyTicksFlag)
from . import mt5enums

__all__ = [
    "Mt5ApiClient",
    "Mt5EventType",
    "Mt5Quote",
    "MqlTick",
    "MqlRates",
    "MqlTradeTransaction",
    "MqlTradeRequest",
    "MqlTradeResult",
    "MqlTradeCheckResult",
    "MqlBookInfo",
    "Mt5CommandType",
    "MtRpcClient",
    "MtRpcError",
    "ResponseStatus",
    "mt5enums",
] + [name for name in dir(mt5enums) if name.startswith("ENUM_")] + ["CopyTicksFlag", "ParametersType"]
