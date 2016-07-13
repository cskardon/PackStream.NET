namespace PackStream.NET.Tests
{
    using FluentAssertions;
    using Xunit;
    using Packers = global::PackStream.NET.Packers.Packers;

    public class IntPackerTests
    {
        public class GetMarkerMethod
        {
            [Theory]
            [InlineData(long.MinValue)]
            [InlineData(-2147483649)]
            [InlineData(2147483648)]
            [InlineData(long.MaxValue)]
            public void GetsCorrect_Int64Marker(long value)
            {
                byte[] expected = {0xCB};

                var actual = Packers.Int.GetMarker(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(int.MinValue)]
            [InlineData(-32769)]
            [InlineData(32768)]
            [InlineData(int.MaxValue)]
            public void GetsCorrect_Int32Marker(long value)
            {
                byte[] expected = {0xCA};

                var actual = Packers.Int.GetMarker(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(short.MinValue)]
            [InlineData(-129)]
            [InlineData(128)]
            [InlineData(short.MaxValue)]
            public void GetsCorrect_Int16Marker(long value)
            {
                byte[] expected = {0xC9};

                var actual = Packers.Int.GetMarker(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(sbyte.MinValue)]
            [InlineData(-17)]
            public void GetsCorrect_Int8Marker(long value)
            {
                byte[] expected = {0xC8};

                var actual = Packers.Int.GetMarker(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(-16)]
            [InlineData(sbyte.MaxValue)]
            public void GetsCorrect_TinyIntMarker(long value)
            {
                byte[] expected = {};

                var actual = Packers.Int.GetMarker(value);
                actual.Should().Equal(expected);
            }
        }

        public class PackMethod
        {
            [Theory]
            [InlineData(-16, new byte[] {0xF0})]
            [InlineData(42, new byte[] {0x2A})]
            [InlineData(127, new byte[] {0x7F})]
            public void PacksAsTinyInt(long value, byte[] expected)
            {
                var actual = Packers.Int.Pack(value);
                actual.Should().Equal(expected, $"{value}");
            }

            [Theory]
            [InlineData(-128, new byte[] {0xC8, 0x80})]
            [InlineData(-17, new byte[] {0xC8, 0xEF})]
            public void PackAsInt8(long value, byte[] expected)
            {
                var actual = Packers.Int.Pack(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(128, new byte[] {0xC9, 0x00, 0x80})]
            public void PackAsInt16(long value, byte[] expected)
            {
                var actual = Packers.Int.Pack(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(32768, new byte[] {0xCA, 0x00, 0x00, 0x80, 0x00})]
            public void PackAsInt32(long value, byte[] expected)
            {
                var actual = Packers.Int.Pack(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(2147483648, new byte[] {0xCB, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00})]
            [InlineData(9223372036854775807, new byte[] {0xCB, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF})]
            public void PackAsInt64(long value, byte[] expected)
            {
                var actual = Packers.Int.Pack(value);
                actual.Should().Equal(expected);
            }
        }

        public class ConvertLongToBytesMethod
        {
            [Theory]
            [InlineData(0, new byte[] {0x00})]
            [InlineData(1, new byte[] {0x01})]
            [InlineData(127, new byte[] {0x7F})]
            public void HandlePositiveTinyIntNumbersCorrectly(int value, byte[] expected)
            {
                var actual = Packers.Int.ConvertLongToBytes(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(128, new byte[] {0x00, 0x80})]
            [InlineData(32767, new byte[] {0x7F, 0xFF})]
            public void HandlePositiveInt16NumbersCorrectly(int value, byte[] expected)
            {
                var actual = Packers.Int.ConvertLongToBytes(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(32768, new byte[] {0x00, 0x00, 0x80, 0x00})]
            [InlineData(2147483647, new byte[] {0x7F, 0xFF, 0xFF, 0xFF})]
            public void HandlePositiveInt32NumbersCorrectly(int value, byte[] expected)
            {
                var actual = Packers.Int.ConvertLongToBytes(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(2147483648, new byte[] {0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00})]
            [InlineData(9223372036854775807, new byte[] {0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF})]
            public void HandlePositiveInt64NumbersCorrectly(long value, byte[] expected)
            {
                var actual = Packers.Int.ConvertLongToBytes(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(-16, new byte[] {0xF0})]
            [InlineData(-1, new byte[] {0xFF})]
            public void HandleNegativeTinyIntNumbersCorrectly(int value, byte[] expected)
            {
                var actual = Packers.Int.ConvertLongToBytes(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(-128, new byte[] {0x80})]
            public void HandleNegativeInt8NumbersCorrectly(int value, byte[] expected)
            {
                var actual = Packers.Int.ConvertLongToBytes(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(-129, new byte[] {0xFF, 0x7F})]
            [InlineData(-32768, new byte[] {0x80, 0x00})]
            public void HandleNegativeInt16NumbersCorrectly(int value, byte[] expected)
            {
                var actual = Packers.Int.ConvertLongToBytes(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(-32769, new byte[] {0xFF, 0xFF, 0x7F, 0xFF})]
            [InlineData(-2147483648, new byte[] {0x80, 0x00, 0x00, 0x00})]
            public void HandleNegativeInt32NumbersCorrectly(int value, byte[] expected)
            {
                var actual = Packers.Int.ConvertLongToBytes(value);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(-2147483649, new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0xFF, 0xFF, 0xFF})]
            [InlineData(-9223372036854775808, new byte[] {0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})]
            public void HandleNegativeInt64NumbersCorrectly(long value, byte[] expected)
            {
                var actual = Packers.Int.ConvertLongToBytes(value);
                actual.Should().Equal(expected);
            }
        }

        public class UnpackMethod
        {
            [Theory]
            [InlineData(-16, new byte[] {0xF0})]
            [InlineData(42, new byte[] {0x2A})]
            [InlineData(127, new byte[] {0x7F})]
            public void UnpPacksAsTinyInt(long expected, byte[] value)
            {
                var actual = Packers.Int.Unpack(value);
                actual.Should().Be(expected);
            }

            [Theory]
            [InlineData(-128, new byte[] {0xC8, 0x80})]
            [InlineData(-17, new byte[] {0xC8, 0xEF})]
            public void UnpPacksAsInt8(long expected, byte[] value)
            {
                var actual = Packers.Int.Unpack(value);
                actual.Should().Be(expected);
            }

            [Theory]
            [InlineData(128, new byte[] {0xC9, 0x00, 0x80})]
            [InlineData(32767, new byte[] {0xC9, 0x7F, 0xFF})]
            [InlineData(-129, new byte[] {0xC9, 0xFF, 0x7F})]
            [InlineData(-32768, new byte[] {0xC9, 0x80, 0x00})]
            public void UnpackAsInt16(long expected, byte[] value)
            {
                var actual = Packers.Int.Unpack(value);
                actual.Should().Be(expected);
            }

            [Theory]
            [InlineData(32768, new byte[] {0xCA, 0x00, 0x00, 0x80, 0x00})]
            [InlineData(2147483647, new byte[] {0xCA, 0x7F, 0xFF, 0xFF, 0xFF})]
            [InlineData(-32769, new byte[] {0xCA, 0xFF, 0xFF, 0x7F, 0xFF})]
            [InlineData(-2147483648, new byte[] {0xCA, 0x80, 0x00, 0x00, 0x00})]
            public void UnpackAsInt32(long expected, byte[] value)
            {
                var actual = Packers.Int.Unpack(value);
                actual.Should().Be(expected);
            }

            [Theory]
            [InlineData(2147483648, new byte[] {0xCB, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00})]
            [InlineData(9223372036854775807, new byte[] {0xCB, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF})]
            [InlineData(-2147483649, new byte[] {0xCB, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0xFF, 0xFF, 0xFF})]
            [InlineData(-9223372036854775808, new byte[] {0xCB, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})]
            public void UnpackAsInt64(long expected, byte[] value)
            {
                var actual = Packers.Int.Unpack(value);
                actual.Should().Be(expected);
            }
        }
    }
}