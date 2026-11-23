using System.Collections;
using MtApi5;
using Xunit;

namespace MtApi5.Tests
{
    public class MtConvertersTests
    {
        [Theory]
        [InlineData("1;12.5", true, 12.5)]
        [InlineData("0;3", false, 3.0)]
        public void ParseResult_Double_Valid(string input, bool expectedOk, double expectedValue)
        {
            var ok = input.ParseResult(';', out double value);
            Assert.Equal(expectedOk, ok);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData("")]
        [InlineData("1")]
        [InlineData("1;2;3")]
        [InlineData("x;1")]
        [InlineData("1;y")]
        public void ParseResult_Double_Invalid_ReturnsFalse(string input)
        {
            Assert.False(input.ParseResult(';', out _));
        }

        [Fact]
        public void ParseResult_DateRange_Valid()
        {
            var ok = "1;0;86400".ParseResult(';', out DateTime from, out DateTime to);
            Assert.True(ok);
            Assert.Equal(new DateTime(1970, 1, 1), from);
            Assert.Equal(new DateTime(1970, 1, 2), to);
        }

        [Theory]
        [InlineData("")]
        [InlineData("1;0")]
        [InlineData("1;a;b")]
        public void ParseResult_DateRange_Invalid_ReturnsFalse(string input)
        {
            Assert.False(input.ParseResult(';', out _, out _));
        }

        [Fact]
        public void ToArrayList_NullRequest_Throws()
        {
            MqlTradeRequest? request = null;
            Assert.Throws<ArgumentNullException>(() => request!.ToArrayList());
        }

        [Fact]
        public void ToArrayList_MapsFieldsInProtocolOrder()
        {
            var request = new MqlTradeRequest
            {
                Action = ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_PENDING,
                Magic = 7,
                Order = 8,
                Symbol = "EURUSD",
                Volume = 0.1,
                Price = 1.1,
                Stoplimit = 1.2,
                Sl = 1.0,
                Tp = 1.3,
                Deviation = 5,
                Comment = "c",
                Position = 9,
                PositionBy = 10,
            };

            ArrayList list = request.ToArrayList();

            Assert.Equal(17, list.Count);
            Assert.Equal((int)ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_PENDING, list[0]);
            Assert.Equal(7UL, list[1]);
            Assert.Equal(8UL, list[2]);
            Assert.Equal("EURUSD", list[3]);
            Assert.Equal(0.1, list[4]);
            Assert.Equal(1.1, list[5]);
            Assert.Equal(1.2, list[6]);
            Assert.Equal(1.0, list[7]);
            Assert.Equal(1.3, list[8]);
            Assert.Equal(5UL, list[9]);
            Assert.Equal("c", list[14]);
            Assert.Equal(9UL, list[15]);
            Assert.Equal(10UL, list[16]);
        }

        [Fact]
        public void ToArrayList_Expiration_IsEncodedAsLong()
        {
            var request = new MqlTradeRequest { Expiration = new DateTime(2100, 1, 1) };
            var list = request.ToArrayList();
            Assert.IsType<long>(list[13]);
            Assert.Equal(4_102_444_800L, list[13]);
        }
    }
}
