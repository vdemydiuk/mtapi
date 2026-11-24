using Xunit;

namespace MtApi5.Tests
{
    // Enum values travel as integers in JSON payloads and must equal the MQL5 constants
    // (https://www.mql5.com/en/docs/constants). Values below are taken from the MQL5 docs.
    public class MtEnumsTests
    {
        [Theory]
        [InlineData(ENUM_TIMEFRAMES.PERIOD_CURRENT, 0)]
        [InlineData(ENUM_TIMEFRAMES.PERIOD_M1, 1)]
        [InlineData(ENUM_TIMEFRAMES.PERIOD_M5, 5)]
        [InlineData(ENUM_TIMEFRAMES.PERIOD_M15, 15)]
        [InlineData(ENUM_TIMEFRAMES.PERIOD_M30, 30)]
        [InlineData(ENUM_TIMEFRAMES.PERIOD_H1, 0x4001)]
        [InlineData(ENUM_TIMEFRAMES.PERIOD_H4, 0x4004)]
        [InlineData(ENUM_TIMEFRAMES.PERIOD_H12, 0x400C)]
        [InlineData(ENUM_TIMEFRAMES.PERIOD_D1, 0x4018)]
        [InlineData(ENUM_TIMEFRAMES.PERIOD_W1, 0x8001)]
        [InlineData(ENUM_TIMEFRAMES.PERIOD_MN1, 0xC001)]
        public void Timeframes_MatchMql5(ENUM_TIMEFRAMES value, int expected) => Assert.Equal(expected, (int)value);

        [Theory]
        [InlineData(ENUM_ORDER_TYPE.ORDER_TYPE_BUY, 0)]
        [InlineData(ENUM_ORDER_TYPE.ORDER_TYPE_SELL, 1)]
        [InlineData(ENUM_ORDER_TYPE.ORDER_TYPE_BUY_LIMIT, 2)]
        [InlineData(ENUM_ORDER_TYPE.ORDER_TYPE_SELL_LIMIT, 3)]
        [InlineData(ENUM_ORDER_TYPE.ORDER_TYPE_BUY_STOP, 4)]
        [InlineData(ENUM_ORDER_TYPE.ORDER_TYPE_SELL_STOP, 5)]
        [InlineData(ENUM_ORDER_TYPE.ORDER_TYPE_BUY_STOP_LIMIT, 6)]
        [InlineData(ENUM_ORDER_TYPE.ORDER_TYPE_SELL_STOP_LIMIT, 7)]
        [InlineData(ENUM_ORDER_TYPE.ORDER_TYPE_CLOSE_BY, 8)]
        public void OrderType_MatchMql5(ENUM_ORDER_TYPE value, int expected) => Assert.Equal(expected, (int)value);

        [Theory]
        [InlineData(ENUM_ORDER_STATE.ORDER_STATE_STARTED, 0)]
        [InlineData(ENUM_ORDER_STATE.ORDER_STATE_PLACED, 1)]
        [InlineData(ENUM_ORDER_STATE.ORDER_STATE_CANCELED, 2)]
        [InlineData(ENUM_ORDER_STATE.ORDER_STATE_PARTIAL, 3)]
        [InlineData(ENUM_ORDER_STATE.ORDER_STATE_FILLED, 4)]
        [InlineData(ENUM_ORDER_STATE.ORDER_STATE_REJECTED, 5)]
        [InlineData(ENUM_ORDER_STATE.ORDER_STATE_EXPIRED, 6)]
        [InlineData(ENUM_ORDER_STATE.ORDER_STATE_REQUEST_ADD, 7)]
        [InlineData(ENUM_ORDER_STATE.ORDER_STATE_REQUEST_MODIFY, 8)]
        [InlineData(ENUM_ORDER_STATE.ORDER_STATE_REQUEST_CANCEL, 9)]
        public void OrderState_MatchMql5(ENUM_ORDER_STATE value, int expected) => Assert.Equal(expected, (int)value);

        [Fact]
        public void OrderFillingAndTime_MatchMql5()
        {
            Assert.Equal(0, (int)ENUM_ORDER_TYPE_FILLING.ORDER_FILLING_FOK);
            Assert.Equal(1, (int)ENUM_ORDER_TYPE_FILLING.ORDER_FILLING_IOC);
            Assert.Equal(2, (int)ENUM_ORDER_TYPE_FILLING.ORDER_FILLING_RETURN);
            Assert.Equal(0, (int)ENUM_ORDER_TYPE_TIME.ORDER_TIME_GTC);
            Assert.Equal(1, (int)ENUM_ORDER_TYPE_TIME.ORDER_TIME_DAY);
            Assert.Equal(2, (int)ENUM_ORDER_TYPE_TIME.ORDER_TIME_SPECIFIED);
            Assert.Equal(3, (int)ENUM_ORDER_TYPE_TIME.ORDER_TIME_SPECIFIED_DAY);
        }

        [Fact]
        public void PositionType_MatchMql5()
        {
            Assert.Equal(0, (int)ENUM_POSITION_TYPE.POSITION_TYPE_BUY);
            Assert.Equal(1, (int)ENUM_POSITION_TYPE.POSITION_TYPE_SELL);
        }

        [Theory]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_BUY, 0)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_SELL, 1)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_BALANCE, 2)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_CREDIT, 3)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_CHARGE, 4)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_CORRECTION, 5)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_BONUS, 6)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_COMMISSION, 7)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_COMMISSION_DAILY, 8)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_COMMISSION_MONTHLY, 9)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_COMMISSION_AGENT_DAILY, 10)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_COMMISSION_AGENT_MONTHLY, 11)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_INTEREST, 12)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_BUY_CANCELED, 13)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TYPE_SELL_CANCELED, 14)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_DIVIDEND, 15)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_DIVIDEND_FRANKED, 16)]
        [InlineData(ENUM_DEAL_TYPE.DEAL_TAX, 17)]
        public void DealType_MatchMql5(ENUM_DEAL_TYPE value, int expected) => Assert.Equal(expected, (int)value);

        [Fact]
        public void DealEntry_MatchMql5()
        {
            Assert.Equal(0, (int)ENUM_DEAL_ENTRY.DEAL_ENTRY_IN);
            Assert.Equal(1, (int)ENUM_DEAL_ENTRY.DEAL_ENTRY_OUT);
            Assert.Equal(2, (int)ENUM_DEAL_ENTRY.DEAL_ENTRY_INOUT);
            Assert.Equal(255, (int)ENUM_DEAL_ENTRY.DEAL_ENTRY_STATE);
        }

        [Theory]
        [InlineData(ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_DEAL, 1)]
        [InlineData(ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_PENDING, 5)]
        [InlineData(ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_SLTP, 6)]
        [InlineData(ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_MODIFY, 7)]
        [InlineData(ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_REMOVE, 8)]
        [InlineData(ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_CLOSE_BY, 10)]
        public void TradeRequestActions_MatchMql5(ENUM_TRADE_REQUEST_ACTIONS value, int expected) => Assert.Equal(expected, (int)value);

        [Theory]
        [InlineData(ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_ORDER_ADD, 0)]
        [InlineData(ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_ORDER_UPDATE, 1)]
        [InlineData(ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_ORDER_DELETE, 2)]
        [InlineData(ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_HISTORY_ADD, 3)]
        [InlineData(ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_HISTORY_UPDATE, 4)]
        [InlineData(ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_HISTORY_DELETE, 5)]
        [InlineData(ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_DEAL_ADD, 6)]
        [InlineData(ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_DEAL_UPDATE, 7)]
        [InlineData(ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_DEAL_DELETE, 8)]
        [InlineData(ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_POSITION, 9)]
        [InlineData(ENUM_TRADE_TRANSACTION_TYPE.TRADE_TRANSACTION_REQUEST, 10)]
        public void TradeTransactionType_MatchMql5(ENUM_TRADE_TRANSACTION_TYPE value, int expected) => Assert.Equal(expected, (int)value);

        [Fact]
        public void BookType_MatchMql5()
        {
            Assert.Equal(1, (int)ENUM_BOOK_TYPE.BOOK_TYPE_SELL);
            Assert.Equal(2, (int)ENUM_BOOK_TYPE.BOOK_TYPE_BUY);
            Assert.Equal(3, (int)ENUM_BOOK_TYPE.BOOK_TYPE_SELL_MARKET);
            Assert.Equal(4, (int)ENUM_BOOK_TYPE.BOOK_TYPE_BUY_MARKET);
        }

        [Fact]
        public void AppliedPriceAndMaMethod_MatchMql5()
        {
            Assert.Equal(1, (int)ENUM_APPLIED_PRICE.PRICE_CLOSE);
            Assert.Equal(2, (int)ENUM_APPLIED_PRICE.PRICE_OPEN);
            Assert.Equal(3, (int)ENUM_APPLIED_PRICE.PRICE_HIGH);
            Assert.Equal(4, (int)ENUM_APPLIED_PRICE.PRICE_LOW);
            Assert.Equal(5, (int)ENUM_APPLIED_PRICE.PRICE_MEDIAN);
            Assert.Equal(6, (int)ENUM_APPLIED_PRICE.PRICE_TYPICAL);
            Assert.Equal(7, (int)ENUM_APPLIED_PRICE.PRICE_WEIGHTED);
            Assert.Equal(0, (int)ENUM_MA_METHOD.MODE_SMA);
            Assert.Equal(1, (int)ENUM_MA_METHOD.MODE_EMA);
            Assert.Equal(2, (int)ENUM_MA_METHOD.MODE_SMMA);
            Assert.Equal(3, (int)ENUM_MA_METHOD.MODE_LWMA);
        }

        [Fact]
        public void TickFlags_MatchMql5_AndCombine()
        {
            Assert.Equal(2, (int)ENUM_TICK_FLAGS.TICK_FLAG_BID);
            Assert.Equal(4, (int)ENUM_TICK_FLAGS.TICK_FLAG_ASK);
            Assert.Equal(8, (int)ENUM_TICK_FLAGS.TICK_FLAG_LAST);
            Assert.Equal(16, (int)ENUM_TICK_FLAGS.TICK_FLAG_VOLUME);
            Assert.Equal(32, (int)ENUM_TICK_FLAGS.TICK_FLAG_BUY);
            Assert.Equal(64, (int)ENUM_TICK_FLAGS.TICK_FLAG_SELL);
            var flags = (ENUM_TICK_FLAGS)6;
            Assert.True(flags.HasFlag(ENUM_TICK_FLAGS.TICK_FLAG_BID));
            Assert.True(flags.HasFlag(ENUM_TICK_FLAGS.TICK_FLAG_ASK));
            Assert.False(flags.HasFlag(ENUM_TICK_FLAGS.TICK_FLAG_LAST));
        }

        [Fact]
        public void AllEnums_HaveNoDuplicateValues_ExceptKnown()
        {
            // Duplicated values usually mean a copy/paste mistake in a hand-written enum.
            var enums = typeof(ENUM_TIMEFRAMES).Assembly.GetTypes()
                .Where(t => t.IsEnum && t.Namespace == "MtApi5" && t.Name.StartsWith("ENUM_"));
            var dups = new List<string>();
            foreach (var e in enums)
            {
                var groups = Enum.GetNames(e).GroupBy(n => Convert.ToInt64(Enum.Parse(e, n)))
                    .Where(g => g.Count() > 1);
                foreach (var g in groups)
                    dups.Add($"{e.Name}={g.Key}: {string.Join(",", g)}");
            }
            Assert.True(dups.Count == 0, "Duplicate enum values: " + string.Join("; ", dups));
        }

        [Theory]
        [InlineData(ErrorCode.ErrCustom, -1)]
        [InlineData(ErrorCode.ErrSuccess, 0)]
        [InlineData(ErrorCode.ErrInternalError, 4001)]
        [InlineData(ErrorCode.ErrChartWrongId, 4101)]
        [InlineData(ErrorCode.ErrObjectError, 4201)]
        [InlineData(ErrorCode.ErrMarketUnknownSymbol, 4301)]
        [InlineData(ErrorCode.ErrHistoryNotFound, 4401)]
        [InlineData(ErrorCode.ErrGlobalvariableNotFound, 4501)]
        [InlineData(ErrorCode.ErrBuffersNoMemory, 4601)]
        [InlineData(ErrorCode.ErrAccountWrongProperty, 4701)]
        [InlineData(ErrorCode.ErrTradeWrongProperty, 4751)]
        [InlineData(ErrorCode.ErrTradeDisabled, 4752)]
        [InlineData(ErrorCode.ErrTradeSendFailed, 4756)]
        [InlineData(ErrorCode.ErrIndicatorUnknownSymbol, 4801)]
        [InlineData(ErrorCode.ErrBooksCannotAdd, 4901)]
        [InlineData(ErrorCode.ErrTooManyFiles, 5001)]
        [InlineData(ErrorCode.ErrFileEndoffile, 5027)]
        [InlineData(ErrorCode.ErrNoStringDate, 5030)]
        [InlineData(ErrorCode.ErrIncompatibleArrays, 5050)]
        [InlineData(ErrorCode.ErrOpenclNotSupported, 5100)]
        [InlineData(ErrorCode.ErrWebrequestInvalidAddress, 5200)]
        [InlineData(ErrorCode.ErrWebrequestRequestFailed, 5203)]
        [InlineData(ErrorCode.ErrUserErrorFirst, 65536)]
        public void ErrorCode_MatchMql5(ErrorCode value, int expected) => Assert.Equal(expected, (int)value);

        [Fact]
        public void ErrorCode_HasNoDuplicateValues()
        {
            var dups = Enum.GetValues<ErrorCode>().GroupBy(v => (int)v).Where(g => g.Count() > 1)
                .Select(g => $"{g.Key}: {string.Join(",", g)}").ToList();
            Assert.True(dups.Count == 0, "Duplicate error codes: " + string.Join("; ", dups));
        }

        [Fact]
        public void ErrorCode_UnknownValue_CastsAndIsNotDefined()
        {
            // ExecutionException casts raw MQL error numbers to ErrorCode; unknown ones must not throw.
            var e = (ErrorCode)9999;
            Assert.False(Enum.IsDefined(e));
            Assert.Equal("9999", e.ToString());
        }
    }
}
