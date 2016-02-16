namespace PackStream.NET.Tests
{
    using FluentAssertions;
    using Xunit;
    using Packers = global::PackStream.NET.Packers.Packers;

    public class DoublePackerTests
    {
        public class PackMethod
        {
            [Theory]
            [InlineData(0.0, new byte[] {0xC1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})]
            [InlineData(1.1, new byte[] {0xC1, 0x3F, 0xF1, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9A})]
            public void PositiveNumbers(double value, byte[] expected)
            {
                var actual = Packers.Double.Pack(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(-1.1, new byte[] {0xC1, 0xBF, 0xF1, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9A})]
            public void NegativeNumbers(double value, byte[] expected)
            {
                var actual = Packers.Double.Pack(value);
                actual.Should().Equal(expected);
            }
        }

        public class UnpackMethod
        {
            [Theory]
            [InlineData(new byte[] {0xC1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, 0.0)]
            [InlineData(new byte[] { 0xC1, 0x3F, 0xF1, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9A }, 1.1)]
            public void PositiveNumbers(byte[] value, double expected)
            {
                var actual = Packers.Double.Unpack(value);
                actual.Should().Be(expected);
            }

            [Theory]
            [InlineData(new byte[] { 0xC1, 0xBF, 0xF1, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9A }, -1.1)]
            public void NegativeNumbers(byte[] value, double expected)
            {
                var actual = Packers.Double.Unpack(value);
                actual.Should().Be(expected);
            }
        }
    }
}