using Org.SbeTool.Sbe.Dll;
using Xunit;

namespace MtApi5.Tests
{
    public class PrimitiveValueTests
    {
        [Fact]
        public void ULong_KeepsValue()
        {
            var value = new PrimitiveValue(ulong.MaxValue, 8);

            Assert.Equal(ulong.MaxValue, value.ULongValue());
            Assert.Equal("18446744073709551615", value.ToString());
        }

        [Fact]
        public void ULong_EqualityUsesValue()
        {
            Assert.Equal(new PrimitiveValue(5UL, 8), new PrimitiveValue(5UL, 8));
            Assert.NotEqual(new PrimitiveValue(5UL, 8), new PrimitiveValue(6UL, 8));
        }
    }
}
