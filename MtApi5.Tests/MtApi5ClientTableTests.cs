using MtApi5.MtProtocol;
using Newtonsoft.Json.Linq;
using Xunit;
using static MtApi5.Tests.MtApi5ClientTestHelper;

namespace MtApi5.Tests
{
    // Table-driven tests for the simple request/response methods. Each row states the command id and
    // the exact JSON parameters that must be sent to the expert (null = no payload), plus how a reply
    // is converted. Together with the Copy* tests this pins the whole command surface of MtApi5Client.
    public class MtApi5ClientTableTests
    {
        private static object?[] R(string name, Func<MtApi5Client, object?> call, Mt5CommandType cmd,
            object? payload, string reply, object? result) => [name, call, (int)cmd, payload, reply, result];

        public static IEnumerable<object?[]> Rows()
        {
            // --- terminal / account ---
            yield return R("IsTesting", c => c.IsTesting(), Mt5CommandType.IsTesting, null, "true", true);
            yield return R("GetSymbols", c => c.GetSymbols(true), Mt5CommandType.GetSymbols, new { Selected = true }, "[\"EURUSD\",\"GBPUSD\"]", new List<string> { "EURUSD", "GBPUSD" });
            yield return R("AccountInfoInteger", c => c.AccountInfoInteger(ENUM_ACCOUNT_INFO_INTEGER.ACCOUNT_LOGIN), Mt5CommandType.AccountInfoInteger, new { PropertyId = (int)ENUM_ACCOUNT_INFO_INTEGER.ACCOUNT_LOGIN }, "123456789012", 123456789012L);
            yield return R("AccountInfoString", c => c.AccountInfoString(ENUM_ACCOUNT_INFO_STRING.ACCOUNT_NAME), Mt5CommandType.AccountInfoString, new { PropertyId = (int)ENUM_ACCOUNT_INFO_STRING.ACCOUNT_NAME }, "\"John\"", "John");
            yield return R("TerminalInfoInteger", c => c.TerminalInfoInteger(ENUM_TERMINAL_INFO_INTEGER.TERMINAL_BUILD), Mt5CommandType.TerminalInfoInteger, new { PropertyId = (int)ENUM_TERMINAL_INFO_INTEGER.TERMINAL_BUILD }, "4000", 4000);
            yield return R("TerminalInfoDouble", c => c.TerminalInfoDouble(ENUM_TERMINAL_INFO_DOUBLE.TERMINAL_COMMUNITY_BALANCE), Mt5CommandType.TerminalInfoDouble, new { PropertyId = (int)ENUM_TERMINAL_INFO_DOUBLE.TERMINAL_COMMUNITY_BALANCE }, "1.5", 1.5);
            yield return R("TerminalInfoString", c => c.TerminalInfoString(ENUM_TERMINAL_INFO_STRING.TERMINAL_NAME), Mt5CommandType.TerminalInfoString, new { PropertyId = (int)ENUM_TERMINAL_INFO_STRING.TERMINAL_NAME }, "\"MT5\"", "MT5");
            yield return R("Print", c => c.Print("hi"), Mt5CommandType.Print, new { PrintMsg = "hi" }, "true", true);
            yield return R("Alert", c => { c.Alert("warn"); return null; }, Mt5CommandType.Alert, new { Message = "warn" }, "null", null);
            yield return R("GetLastError", c => c.GetLastError(), Mt5CommandType.GetLastError, null, "4301", 4301);
            yield return R("ResetLastError", c => { c.ResetLastError(); return null; }, Mt5CommandType.ResetLastError, null, "null", null);
            yield return R("UnlockTicks", c => { c.UnlockTicks(); return null; }, Mt5CommandType.UnlockTicks, null, "null", null);
            yield return R("TesterStop", c => { c.TesterStop(); return null; }, Mt5CommandType.TesterStop, null, "null", null);

            // --- times ---
            yield return R("TimeCurrent", c => c.TimeCurrent(), Mt5CommandType.TimeCurrent, null, "1700000000", Time);
            yield return R("TimeTradeServer", c => c.TimeTradeServer(), Mt5CommandType.TimeTradeServer, null, "1700000000", Time);
            yield return R("TimeLocal", c => c.TimeLocal(), Mt5CommandType.TimeLocal, null, "1700000000", Time);
            yield return R("TimeGMT", c => c.TimeGMT(), Mt5CommandType.TimeGMT, null, "1700000000", Time);

            // --- positions ---
            yield return R("PositionsTotal", c => c.PositionsTotal(), Mt5CommandType.PositionsTotal, null, "2", 2);
            yield return R("PositionGetSymbol", c => c.PositionGetSymbol(1), Mt5CommandType.PositionGetSymbol, new { Index = 1 }, "\"EURUSD\"", "EURUSD");
            yield return R("PositionSelect", c => c.PositionSelect("EURUSD"), Mt5CommandType.PositionSelect, new { Symbol = "EURUSD" }, "true", true);
            yield return R("PositionSelectByTicket", c => c.PositionSelectByTicket(55), Mt5CommandType.PositionSelectByTicket, new { Ticket = 55UL }, "true", true);
            yield return R("PositionGetDouble", c => c.PositionGetDouble(ENUM_POSITION_PROPERTY_DOUBLE.POSITION_PROFIT), Mt5CommandType.PositionGetDouble, new { PropertyId = (int)ENUM_POSITION_PROPERTY_DOUBLE.POSITION_PROFIT }, "12.5", 12.5);
            yield return R("PositionGetInteger", c => c.PositionGetInteger(ENUM_POSITION_PROPERTY_INTEGER.POSITION_TICKET), Mt5CommandType.PositionGetInteger, new { PropertyId = (int)ENUM_POSITION_PROPERTY_INTEGER.POSITION_TICKET }, "77", 77L);
            yield return R("PositionGetString", c => c.PositionGetString(ENUM_POSITION_PROPERTY_STRING.POSITION_SYMBOL), Mt5CommandType.PositionGetString, new { PropertyId = (int)ENUM_POSITION_PROPERTY_STRING.POSITION_SYMBOL }, "\"EURUSD\"", "EURUSD");
            yield return R("PositionGetTicket", c => c.PositionGetTicket(0), Mt5CommandType.PositionGetTicket, new { Index = 0 }, "18446744073709551615", ulong.MaxValue);
            yield return R("PositionCloseAll", c => c.PositionCloseAll(), Mt5CommandType.PositionCloseAll, null, "3", 3);
            yield return R("PositionModify", c => c.PositionModify(5, 1.1, 1.3), Mt5CommandType.PositionModify, new { Ticket = 5UL, Sl = 1.1, Tp = 1.3 }, "true", true);
            yield return R("PositionClosePartial_symbol", c => c.PositionClosePartial("EURUSD", 0.5, 10), Mt5CommandType.PositionClosePartial_bySymbol, new { Symbol = "EURUSD", Volume = 0.5, Deviation = 10UL }, "true", true);
            yield return R("PositionClosePartial_ticket", c => c.PositionClosePartial(9UL, 0.5), Mt5CommandType.PositionClosePartial_byTicket, new { Ticket = 9UL, Volume = 0.5, Deviation = ulong.MaxValue }, "true", true);
            yield return R("PositionClose_default_deviation", c => c.PositionClose(9), Mt5CommandType.PositionClose, new { Ticket = 9UL, Deviation = ulong.MaxValue }, "{\"RetVal\":true}", true);
            yield return R("PositionOpen", c => c.PositionOpen("EURUSD", ENUM_ORDER_TYPE.ORDER_TYPE_SELL, 0.1, 1.1, 1.2, 1.0, "cm"), Mt5CommandType.PositionOpen, new { Symbol = "EURUSD", OrderType = 1, Volume = 0.1, Price = 1.1, Sl = 1.2, Tp = 1.0, Comment = "cm" }, "true", true);
            yield return R("PositionOpen2_no_comment", c => c.PositionOpen("EURUSD", ENUM_ORDER_TYPE.ORDER_TYPE_BUY, 0.1, 1.1, 1.0, 1.2, null, out _), Mt5CommandType.PositionOpen2, new { Symbol = "EURUSD", OrderType = 0, Volume = 0.1, Price = 1.1, Sl = 1.0, Tp = 1.2 }, "{\"RetVal\":true}", true);
            yield return R("PositionOpen2_default_comment", c => c.PositionOpen("EURUSD", ENUM_ORDER_TYPE.ORDER_TYPE_BUY, 0.1, 1.1, 1.0, 1.2, out _), Mt5CommandType.PositionOpen2, new { Symbol = "EURUSD", OrderType = 0, Volume = 0.1, Price = 1.1, Sl = 1.0, Tp = 1.2, Comment = "" }, "{\"RetVal\":false}", false);

            // --- orders ---
            yield return R("OrdersTotal", c => c.OrdersTotal(), Mt5CommandType.OrdersTotal, null, "4", 4);
            yield return R("OrderGetTicket", c => c.OrderGetTicket(2), Mt5CommandType.OrderGetTicket, new { Index = 2 }, "99", 99UL);
            yield return R("OrderSelect", c => c.OrderSelect(99), Mt5CommandType.OrderSelect, new { Ticket = 99UL }, "true", true);
            yield return R("OrderGetDouble", c => c.OrderGetDouble(ENUM_ORDER_PROPERTY_DOUBLE.ORDER_PRICE_OPEN), Mt5CommandType.OrderGetDouble, new { PropertyId = (int)ENUM_ORDER_PROPERTY_DOUBLE.ORDER_PRICE_OPEN }, "1.25", 1.25);
            yield return R("OrderGetInteger", c => c.OrderGetInteger(ENUM_ORDER_PROPERTY_INTEGER.ORDER_TICKET), Mt5CommandType.OrderGetInteger, new { PropertyId = (int)ENUM_ORDER_PROPERTY_INTEGER.ORDER_TICKET }, "7", 7L);
            yield return R("OrderGetString", c => c.OrderGetString(ENUM_ORDER_PROPERTY_STRING.ORDER_COMMENT), Mt5CommandType.OrderGetString, new { PropertyId = (int)ENUM_ORDER_PROPERTY_STRING.ORDER_COMMENT }, "\"c\"", "c");

            // --- history ---
            yield return R("HistorySelect", c => c.HistorySelect(Time, Time.AddDays(1)), Mt5CommandType.HistorySelect, new { FromDate = MtTime, ToDate = MtTime + 86400 }, "true", true);
            yield return R("HistorySelectByPosition", c => c.HistorySelectByPosition(31), Mt5CommandType.HistorySelectByPosition, new { PositionId = 31L }, "true", true);
            yield return R("HistoryOrderSelect", c => c.HistoryOrderSelect(8), Mt5CommandType.HistoryOrderSelect, new { Ticket = 8UL }, "true", true);
            yield return R("HistoryOrdersTotal", c => c.HistoryOrdersTotal(), Mt5CommandType.HistoryOrdersTotal, null, "12", 12);
            yield return R("HistoryOrderGetTicket", c => c.HistoryOrderGetTicket(3), Mt5CommandType.HistoryOrderGetTicket, new { Index = 3 }, "44", 44UL);
            yield return R("HistoryOrderGetDouble", c => c.HistoryOrderGetDouble(44, ENUM_ORDER_PROPERTY_DOUBLE.ORDER_SL), Mt5CommandType.HistoryOrderGetDouble, new { TicketNumber = 44UL, PropertyId = (int)ENUM_ORDER_PROPERTY_DOUBLE.ORDER_SL }, "1.1", 1.1);
            yield return R("HistoryOrderGetInteger", c => c.HistoryOrderGetInteger(44, ENUM_ORDER_PROPERTY_INTEGER.ORDER_STATE), Mt5CommandType.HistoryOrderGetInteger, new { TicketNumber = 44UL, PropertyId = (int)ENUM_ORDER_PROPERTY_INTEGER.ORDER_STATE }, "4", 4L);
            yield return R("HistoryOrderGetString", c => c.HistoryOrderGetString(44, ENUM_ORDER_PROPERTY_STRING.ORDER_SYMBOL), Mt5CommandType.HistoryOrderGetString, new { TicketNumber = 44UL, PropertyId = (int)ENUM_ORDER_PROPERTY_STRING.ORDER_SYMBOL }, "\"EURUSD\"", "EURUSD");
            yield return R("HistoryDealSelect", c => c.HistoryDealSelect(6), Mt5CommandType.HistoryDealSelect, new { Ticket = 6UL }, "true", true);
            yield return R("HistoryDealsTotal", c => c.HistoryDealsTotal(), Mt5CommandType.HistoryDealsTotal, null, "5", 5);
            yield return R("HistoryDealGetTicket", c => c.HistoryDealGetTicket(1), Mt5CommandType.HistoryDealGetTicket, new { Index = 1 }, "66", 66UL);
            yield return R("HistoryDealGetDouble", c => c.HistoryDealGetDouble(66, ENUM_DEAL_PROPERTY_DOUBLE.DEAL_PROFIT), Mt5CommandType.HistoryDealGetDouble, new { TicketNumber = 66UL, PropertyId = (int)ENUM_DEAL_PROPERTY_DOUBLE.DEAL_PROFIT }, "-3.5", -3.5);
            yield return R("HistoryDealGetInteger", c => c.HistoryDealGetInteger(66, ENUM_DEAL_PROPERTY_INTEGER.DEAL_TIME), Mt5CommandType.HistoryDealGetInteger, new { TicketNumber = 66UL, PropertyId = (int)ENUM_DEAL_PROPERTY_INTEGER.DEAL_TIME }, "1700000000", 1700000000L);
            yield return R("HistoryDealGetString", c => c.HistoryDealGetString(66, ENUM_DEAL_PROPERTY_STRING.DEAL_COMMENT), Mt5CommandType.HistoryDealGetString, new { TicketNumber = 66UL, PropertyId = (int)ENUM_DEAL_PROPERTY_STRING.DEAL_COMMENT }, "\"x\"", "x");

            // --- symbols / series ---
            yield return R("SymbolsTotal", c => c.SymbolsTotal(false), Mt5CommandType.SymbolsTotal, new { Selected = false }, "80", 80);
            yield return R("SymbolName", c => c.SymbolName(3, true), Mt5CommandType.SymbolName, new { Pos = 3, Selected = true }, "\"EURUSD\"", "EURUSD");
            yield return R("SymbolSelect", c => c.SymbolSelect("EURUSD", true), Mt5CommandType.SymbolSelect, new { Symbol = "EURUSD", Selected = true }, "true", true);
            yield return R("SymbolIsSynchronized", c => c.SymbolIsSynchronized("EURUSD"), Mt5CommandType.SymbolIsSynchronized, new { Symbol = "EURUSD" }, "true", true);
            yield return R("SymbolInfoDouble", c => c.SymbolInfoDouble("EURUSD", ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_BID), Mt5CommandType.SymbolInfoDouble, new { Symbol = "EURUSD", PropId = (int)ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_BID }, "1.1", 1.1);
            yield return R("SymbolInfoInteger", c => c.SymbolInfoInteger("EURUSD", ENUM_SYMBOL_INFO_INTEGER.SYMBOL_DIGITS), Mt5CommandType.SymbolInfoInteger, new { Symbol = "EURUSD", PropId = (int)ENUM_SYMBOL_INFO_INTEGER.SYMBOL_DIGITS }, "5", 5L);
            yield return R("SymbolInfoString", c => c.SymbolInfoString("EURUSD", ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION), Mt5CommandType.SymbolInfoString, new { Symbol = "EURUSD", PropId = (int)ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION }, "\"Euro\"", "Euro");
            yield return R("SeriesInfoInteger", c => c.SeriesInfoInteger("EURUSD", ENUM_TIMEFRAMES.PERIOD_D1, ENUM_SERIES_INFO_INTEGER.SERIES_BARS_COUNT), Mt5CommandType.SeriesInfoInteger, new { Symbol = "EURUSD", Timeframe = (int)ENUM_TIMEFRAMES.PERIOD_D1, PropId = (int)ENUM_SERIES_INFO_INTEGER.SERIES_BARS_COUNT }, "1000", 1000L);
            yield return R("BarsCalculated", c => c.BarsCalculated(7), Mt5CommandType.BarsCalculated, new { IndicatorHandle = 7 }, "250", 250);
            yield return R("IndicatorRelease", c => c.IndicatorRelease(7), Mt5CommandType.IndicatorRelease, new { IndicatorHandle = 7 }, "true", true);
            yield return R("MarketBookAdd", c => c.MarketBookAdd("EURUSD"), Mt5CommandType.MarketBookAdd, new { Symbol = "EURUSD" }, "true", true);
            yield return R("MarketBookRelease", c => c.MarketBookRelease("EURUSD"), Mt5CommandType.MarketBookRelease, new { Symbol = "EURUSD" }, "true", true);

            // --- charts ---
            yield return R("ChartRedraw", c => { c.ChartRedraw(5); return null; }, Mt5CommandType.ChartRedraw, new { ChartId = 5L }, "null", null);
            yield return R("ChartApplyTemplate", c => c.ChartApplyTemplate(5, "t.tpl"), Mt5CommandType.ChartApplyTemplate, new { ChartId = 5L, TemplateFileName = "t.tpl" }, "true", true);
            yield return R("ChartSaveTemplate", c => c.ChartSaveTemplate(5, "t.tpl"), Mt5CommandType.ChartSaveTemplate, new { ChartId = 5L, TemplateFileName = "t.tpl" }, "true", true);
            yield return R("ChartWindowFind", c => c.ChartWindowFind(5, "RSI"), Mt5CommandType.ChartWindowFind, new { ChartId = 5L, IndicatorShortname = "RSI" }, "1", 1);
            yield return R("ChartOpen", c => c.ChartOpen("EURUSD", ENUM_TIMEFRAMES.PERIOD_M5), Mt5CommandType.ChartOpen, new { Symbol = "EURUSD", Timeframe = 5 }, "123456", 123456L);
            yield return R("ChartFirst", c => c.ChartFirst(), Mt5CommandType.ChartFirst, null, "111", 111L);
            yield return R("ChartNext", c => c.ChartNext(111), Mt5CommandType.ChartNext, new { ChartId = 111L }, "222", 222L);
            yield return R("ChartClose_default", c => c.ChartClose(), Mt5CommandType.ChartClose, new { ChartId = 0L }, "true", true);
            yield return R("ChartSymbol", c => c.ChartSymbol(5), Mt5CommandType.ChartSymbol, new { ChartId = 5L }, "\"EURUSD\"", "EURUSD");
            yield return R("ChartPeriod", c => c.ChartPeriod(5), Mt5CommandType.ChartPeriod, new { ChartId = 5L }, "16385", ENUM_TIMEFRAMES.PERIOD_H1);
            yield return R("ChartSetDouble", c => c.ChartSetDouble(5, ENUM_CHART_PROPERTY_DOUBLE.CHART_SHIFT_SIZE, 20.5), Mt5CommandType.ChartSetDouble, new { ChartId = 5L, PropId = (int)ENUM_CHART_PROPERTY_DOUBLE.CHART_SHIFT_SIZE, Value = 20.5 }, "true", true);
            yield return R("ChartSetInteger", c => c.ChartSetInteger(5, ENUM_CHART_PROPERTY_INTEGER.CHART_SCALE, 3), Mt5CommandType.ChartSetInteger, new { ChartId = 5L, PropId = (int)ENUM_CHART_PROPERTY_INTEGER.CHART_SCALE, Value = 3L }, "true", true);
            yield return R("ChartSetString", c => c.ChartSetString(5, ENUM_CHART_PROPERTY_STRING.CHART_COMMENT, "x"), Mt5CommandType.ChartSetString, new { ChartId = 5L, PropId = (int)ENUM_CHART_PROPERTY_STRING.CHART_COMMENT, Value = "x" }, "true", true);
            yield return R("ChartGetDouble", c => c.ChartGetDouble(5, ENUM_CHART_PROPERTY_DOUBLE.CHART_PRICE_MAX, 1), Mt5CommandType.ChartGetDouble, new { ChartId = 5L, PropId = (int)ENUM_CHART_PROPERTY_DOUBLE.CHART_PRICE_MAX, SubWindow = 1 }, "1.9", 1.9);
            yield return R("ChartGetInteger_default_subwindow", c => c.ChartGetInteger(5, ENUM_CHART_PROPERTY_INTEGER.CHART_SCALE), Mt5CommandType.ChartGetInteger, new { ChartId = 5L, PropId = (int)ENUM_CHART_PROPERTY_INTEGER.CHART_SCALE, SubWindow = 0 }, "4", 4L);
            yield return R("ChartGetString", c => c.ChartGetString(5, ENUM_CHART_PROPERTY_STRING.CHART_COMMENT), Mt5CommandType.ChartGetString, new { ChartId = 5L, PropId = (int)ENUM_CHART_PROPERTY_STRING.CHART_COMMENT }, "\"c\"", "c");
            yield return R("ChartNavigate", c => c.ChartNavigate(5, ENUM_CHART_POSITION.CHART_END, -2), Mt5CommandType.ChartNavigate, new { ChartId = 5L, Position = (int)ENUM_CHART_POSITION.CHART_END, Shift = -2 }, "true", true);
            yield return R("ChartIndicatorAdd", c => c.ChartIndicatorAdd(5, 1, 7), Mt5CommandType.ChartIndicatorAdd, new { ChartId = 5L, SubWindow = 1, IndicatorHandle = 7 }, "true", true);
            yield return R("ChartIndicatorDelete", c => c.ChartIndicatorDelete(5, 1, "RSI"), Mt5CommandType.ChartIndicatorDelete, new { ChartId = 5L, SubWindow = 1, IndicatorShortname = "RSI" }, "true", true);
            yield return R("ChartIndicatorGet", c => c.ChartIndicatorGet(5, 1, "RSI"), Mt5CommandType.ChartIndicatorGet, new { ChartId = 5L, SubWindow = 1, IndicatorShortname = "RSI" }, "13", 13);
            yield return R("ChartIndicatorName", c => c.ChartIndicatorName(5, 1, 0), Mt5CommandType.ChartIndicatorName, new { ChartId = 5L, SubWindow = 1, Index = 0 }, "\"RSI(14)\"", "RSI(14)");
            yield return R("ChartIndicatorsTotal", c => c.ChartIndicatorsTotal(5, 1), Mt5CommandType.ChartIndicatorsTotal, new { ChartId = 5L, SubWindow = 1 }, "2", 2);
            yield return R("ChartWindowOnDropped", c => c.ChartWindowOnDropped(), Mt5CommandType.ChartWindowOnDropped, null, "1", 1);
            yield return R("ChartPriceOnDropped", c => c.ChartPriceOnDropped(), Mt5CommandType.ChartPriceOnDropped, null, "1.2345", 1.2345);
            yield return R("ChartTimeOnDropped", c => c.ChartTimeOnDropped(), Mt5CommandType.ChartTimeOnDropped, null, "1700000000", Time);
            yield return R("ChartXOnDropped", c => c.ChartXOnDropped(), Mt5CommandType.ChartXOnDropped, null, "10", 10);
            yield return R("ChartYOnDropped", c => c.ChartYOnDropped(), Mt5CommandType.ChartYOnDropped, null, "20", 20);
            yield return R("ChartSetSymbolPeriod", c => c.ChartSetSymbolPeriod(5, "GBPUSD", ENUM_TIMEFRAMES.PERIOD_H4), Mt5CommandType.ChartSetSymbolPeriod, new { ChartId = 5L, Symbol = "GBPUSD", Period = (int)ENUM_TIMEFRAMES.PERIOD_H4 }, "true", true);
            yield return R("ChartScreenShot_default_align", c => c.ChartScreenShot(5, "a.gif", 800, 600), Mt5CommandType.ChartScreenShot, new { ChartId = 5L, Filename = "a.gif", Width = 800, Height = 600, AlignMode = (int)ENUM_ALIGN_MODE.ALIGN_RIGHT }, "true", true);

            // --- objects ---
            yield return R("ObjectName_defaults", c => c.ObjectName(5, 2), Mt5CommandType.ObjectName, new { ChartId = 5L, Pos = 2, SubWindow = -1, Type = -1 }, "\"o1\"", "o1");
            yield return R("ObjectDelete", c => c.ObjectDelete(5, "o1"), Mt5CommandType.ObjectDelete, new { ChartId = 5L, Name = "o1" }, "true", true);
            yield return R("ObjectsDeleteAll_defaults", c => c.ObjectsDeleteAll(5), Mt5CommandType.ObjectsDeleteAll, new { ChartId = 5L, SubWindow = -1, Type = -1 }, "3", 3);
            yield return R("ObjectFind", c => c.ObjectFind(5, "o1"), Mt5CommandType.ObjectFind, new { ChartId = 5L, Name = "o1" }, "0", 0);
            yield return R("ObjectGetTimeByValue", c => c.ObjectGetTimeByValue(5, "o1", 1.5, 0), Mt5CommandType.ObjectGetTimeByValue, new { ChartId = 5L, Name = "o1", Value = 1.5, LineId = 0 }, "1700000000", Time);
            yield return R("ObjectGetValueByTime", c => c.ObjectGetValueByTime(5, "o1", Time, 0), Mt5CommandType.ObjectGetValueByTime, new { ChartId = 5L, Name = "o1", Time = MtTime, LineId = 0 }, "1.75", 1.75);
            yield return R("ObjectMove", c => c.ObjectMove(5, "o1", 1, Time, 1.5), Mt5CommandType.ObjectMove, new { ChartId = 5L, Name = "o1", PointIndex = 1, Time = MtTime, Price = 1.5 }, "true", true);
            yield return R("ObjectsTotal_defaults", c => c.ObjectsTotal(5), Mt5CommandType.ObjectsTotal, new { ChartId = 5L, SubWindow = -1, Type = -1 }, "9", 9);
            yield return R("ObjectSetDouble", c => c.ObjectSetDouble(5, "o1", ENUM_OBJECT_PROPERTY_DOUBLE.OBJPROP_PRICE, 1.5), Mt5CommandType.ObjectSetDouble, new { ChartId = 5L, Name = "o1", PropId = (int)ENUM_OBJECT_PROPERTY_DOUBLE.OBJPROP_PRICE, PropValue = 1.5 }, "true", true);
            yield return R("ObjectSetInteger", c => c.ObjectSetInteger(5, "o1", ENUM_OBJECT_PROPERTY_INTEGER.OBJPROP_COLOR, 255), Mt5CommandType.ObjectSetInteger, new { ChartId = 5L, Name = "o1", PropId = (int)ENUM_OBJECT_PROPERTY_INTEGER.OBJPROP_COLOR, PropValue = 255L }, "true", true);
            yield return R("ObjectSetString", c => c.ObjectSetString(5, "o1", ENUM_OBJECT_PROPERTY_STRING.OBJPROP_TEXT, "t"), Mt5CommandType.ObjectSetString, new { ChartId = 5L, Name = "o1", PropId = (int)ENUM_OBJECT_PROPERTY_STRING.OBJPROP_TEXT, PropValue = "t" }, "true", true);
            yield return R("ObjectGetDouble", c => c.ObjectGetDouble(5, "o1", ENUM_OBJECT_PROPERTY_DOUBLE.OBJPROP_PRICE), Mt5CommandType.ObjectGetDouble, new { ChartId = 5L, Name = "o1", PropId = (int)ENUM_OBJECT_PROPERTY_DOUBLE.OBJPROP_PRICE }, "1.5", 1.5);
            yield return R("ObjectGetInteger", c => c.ObjectGetInteger(5, "o1", ENUM_OBJECT_PROPERTY_INTEGER.OBJPROP_COLOR), Mt5CommandType.ObjectGetInteger, new { ChartId = 5L, Name = "o1", PropId = (int)ENUM_OBJECT_PROPERTY_INTEGER.OBJPROP_COLOR }, "255", 255L);
            yield return R("ObjectGetString", c => c.ObjectGetString(5, "o1", ENUM_OBJECT_PROPERTY_STRING.OBJPROP_TEXT), Mt5CommandType.ObjectGetString, new { ChartId = 5L, Name = "o1", PropId = (int)ENUM_OBJECT_PROPERTY_STRING.OBJPROP_TEXT }, "\"t\"", "t");

            // --- global variables ---
            yield return R("GlobalVariableCheck", c => c.GlobalVariableCheck("g"), Mt5CommandType.GlobalVariableCheck, new { Name = "g" }, "true", true);
            yield return R("GlobalVariableTime", c => c.GlobalVariableTime("g"), Mt5CommandType.GlobalVariableTime, new { Name = "g" }, "1700000000", Time);
            yield return R("GlobalVariableDel", c => c.GlobalVariableDel("g"), Mt5CommandType.GlobalVariableDel, new { Name = "g" }, "true", true);
            yield return R("GlobalVariableGet", c => c.GlobalVariableGet("g"), Mt5CommandType.GlobalVariableGet, new { Name = "g" }, "2.5", 2.5);
            yield return R("GlobalVariableName", c => c.GlobalVariableName(0), Mt5CommandType.GlobalVariableName, new { Index = 0 }, "\"g\"", "g");
            yield return R("GlobalVariableSet", c => c.GlobalVariableSet("g", 2.5), Mt5CommandType.GlobalVariableSet, new { Name = "g", Value = 2.5 }, "1700000000", Time);
            yield return R("GlobalVariablesFlush", c => { c.GlobalVariablesFlush(); return null; }, Mt5CommandType.GlobalVariablesFlush, null, "null", null);
            yield return R("GlobalVariableTemp", c => c.GlobalVariableTemp("g"), Mt5CommandType.GlobalVariableTemp, new { Name = "g" }, "true", true);
            yield return R("GlobalVariableSetOnCondition", c => c.GlobalVariableSetOnCondition("g", 2, 1), Mt5CommandType.GlobalVariableSetOnCondition, new { Name = "g", Value = 2.0, CheckValue = 1.0 }, "true", true);
            yield return R("GlobalVariablesDeleteAll_defaults", c => c.GlobalVariablesDeleteAll(), Mt5CommandType.GlobalVariablesDeleteAll, new { PrefixName = "", LimitData = 0L }, "2", 2);
            yield return R("GlobalVariablesDeleteAll_limit", c => c.GlobalVariablesDeleteAll("p_", Time), Mt5CommandType.GlobalVariablesDeleteAll, new { PrefixName = "p_", LimitData = MtTime }, "2", 2);
            yield return R("GlobalVariablesTotal", c => c.GlobalVariablesTotal(), Mt5CommandType.GlobalVariablesTotal, null, "7", 7);
        }

        [Theory]
        [MemberData(nameof(Rows))]
        public async Task Command_SendsExpectedPayload_AndConvertsReply(
            string name, Func<MtApi5Client, object?> call, int command, object? payload, string reply, object? expected)
        {
            var (client, fake) = await Connect();
            fake.Reply(reply);

            var actual = call(client);

            var sent = fake.Last();
            Assert.True(command == sent.CommandType, $"{name}: command {sent.CommandType} != expected {command}");
            if (payload == null)
                Assert.True(sent.Payload == "", $"{name}: expected no payload but got {sent.Payload}");
            else
                Assert.True(JToken.DeepEquals(JObject.FromObject(payload), JObject.Parse(sent.Payload)),
                    $"{name}: payload {sent.Payload} != expected {JObject.FromObject(payload)}");
            Assert.True(Equals(expected, actual) || (expected is List<string> e && actual is List<string> a && e.SequenceEqual(a)),
                $"{name}: result '{actual}' ({actual?.GetType().Name}) != expected '{expected}' ({expected?.GetType().Name})");
        }

        [Fact]
        public async Task SymbolInfoString_OutOverload_UsesSecondCommand()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":\"Euro\"}");

            Assert.True(client.SymbolInfoString("EURUSD", ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION, out var value));

            Assert.Equal("Euro", value);
            Assert.Equal((int)Mt5CommandType.SymbolInfoString2, fake.Last().CommandType);
        }

        // ---- out-parameter and structured results ----

        [Fact]
        public async Task OrderCalcMargin_ReturnsMargin()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":110.5}");

            Assert.True(client.OrderCalcMargin(ENUM_ORDER_TYPE.ORDER_TYPE_SELL, "EURUSD", 0.5, 1.1, out var margin));

            Assert.Equal(110.5, margin);
            Assert.True(JToken.DeepEquals(JObject.FromObject(new { Action = 1, Symbol = "EURUSD", Volume = 0.5, Price = 1.1 }), fake.LastPayload()));
        }

        [Fact]
        public async Task OrderCalcMargin_NullReply_ReturnsNaNAndFalse()
        {
            var (client, fake) = await Connect();
            fake.Reply("null");

            Assert.False(client.OrderCalcMargin(ENUM_ORDER_TYPE.ORDER_TYPE_BUY, "EURUSD", 1, 1, out var margin));
            Assert.True(double.IsNaN(margin));
        }

        [Fact]
        public async Task OrderCalcProfit_ReturnsProfit_AndSendsAllPrices()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":-7.25}");

            Assert.True(client.OrderCalcProfit(ENUM_ORDER_TYPE.ORDER_TYPE_BUY, "EURUSD", 1, 1.1, 1.0, out var profit));

            Assert.Equal(-7.25, profit);
            Assert.True(JToken.DeepEquals(JObject.FromObject(new { Action = 0, Symbol = "EURUSD", Volume = 1.0, PriceOpen = 1.1, PriceClose = 1.0 }), fake.LastPayload()));
        }

        [Fact]
        public async Task OrderCheck_ReturnsCheckResult()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":{\"Retcode\":0,\"Balance\":1000.5,\"Comment\":\"Done\"}}");

            Assert.True(client.OrderCheck(new MqlTradeRequest { Symbol = "EURUSD" }, out var result));

            Assert.Equal(1000.5, result!.Balance);
            Assert.Equal("Done", result.Comment);
            Assert.Equal("EURUSD", (string?)fake.LastPayload()["TradeRequest"]!["Symbol"]);
        }

        [Fact]
        public async Task PositionClose_WithResult_ReturnsTradeResult()
        {
            var (client, fake) = await Connect();
            fake.Reply("{\"RetVal\":true,\"Result\":{\"Retcode\":10009}}");

            Assert.True(client.PositionClose(9, 25, out var result));

            Assert.Equal(10009u, result!.Retcode);
            Assert.Equal(25, (int)fake.LastPayload()["Deviation"]!);
        }

        [Fact]
        public async Task MarketBookGet_ReturnsBook_OrNullWhenNoReply()
        {
            var (client, fake) = await Connect();
            fake.Reply("[{\"type\":1,\"price\":1.1,\"volume\":10,\"volume_real\":10.5},{\"type\":2,\"price\":1.09,\"volume\":5}]");

            Assert.True(client.MarketBookGet("EURUSD", out var book));
            Assert.Equal(2, book!.Length);
            Assert.Equal(ENUM_BOOK_TYPE.BOOK_TYPE_SELL, book[0].type);
            Assert.Equal(1.1, book[0].price);
            Assert.Equal(10, book[0].volume);

            fake.Reply("null");
            Assert.False(client.MarketBookGet("EURUSD", out book));
            Assert.Null(book);
        }
    }
}
