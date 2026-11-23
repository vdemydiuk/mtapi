using MtApi5;
using Xunit;

namespace MtApi5.Tests
{
    public class Mt5TimeConverterTests
    {
        [Fact]
        public void ConvertFromMtTime_Zero_IsUnixEpoch()
        {
            Assert.Equal(new DateTime(1970, 1, 1), Mt5TimeConverter.ConvertFromMtTime(0L));
        }

        [Fact]
        public void ConvertFromMtTime_Int_And_Long_GiveSameResult()
        {
            Assert.Equal(Mt5TimeConverter.ConvertFromMtTime(1_700_000_000L),
                         Mt5TimeConverter.ConvertFromMtTime(1_700_000_000));
        }

        // Issue #340: values past Int32.MaxValue (2038-01-19 03:14:07) must not overflow.
        [Fact]
        public void ConvertFromMtTime_Long_BeyondYear2038()
        {
            var result = Mt5TimeConverter.ConvertFromMtTime((long)int.MaxValue + 1);
            Assert.Equal(new DateTime(2038, 1, 19, 3, 14, 8), result);
        }

        [Fact]
        public void ConvertToMtTime_BeyondYear2038_DoesNotOverflow()
        {
            var result = Mt5TimeConverter.ConvertToMtTime(new DateTime(2100, 1, 1));
            Assert.Equal(4_102_444_800L, result);
        }

        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        [InlineData(1_700_000_000L)]
        [InlineData(4_102_444_800L)]
        public void RoundTrip_LongTime(long seconds)
        {
            var dt = Mt5TimeConverter.ConvertFromMtTime(seconds);
            Assert.Equal(seconds, Mt5TimeConverter.ConvertToMtTime(dt));
        }

        [Fact]
        public void ConvertToMtTime_NullAndMinValue_ReturnZero()
        {
            Assert.Equal(0L, Mt5TimeConverter.ConvertToMtTime(null));
            Assert.Equal(0L, Mt5TimeConverter.ConvertToMtTime(DateTime.MinValue));
        }

        [Fact]
        public void ConvertToMtTime_DropsSubSecondPart()
        {
            var dt = new DateTime(2024, 1, 1, 0, 0, 0).AddMilliseconds(999);
            var whole = new DateTime(2024, 1, 1, 0, 0, 0);
            Assert.Equal(Mt5TimeConverter.ConvertToMtTime(whole), Mt5TimeConverter.ConvertToMtTime(dt));
        }
    }
}
