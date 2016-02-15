namespace PackStream.NET.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using FluentAssertions;
    using global::PackStream;
    using Xunit;
    using Packers = global::PackStream.NET.Packers.Packers;

    public class StringPackerTests
    {
        public class PackMethod
        {
            [Theory]
            [InlineData(0, new byte[] {0x80})]
            [InlineData(1, new byte[] {0x81, 0x61})]
            public void PacksSmallStringCorrectly(int size, byte[] expected)
            {
                var s = new string('a', size);

                var packed = PackStream.Pack(s);
                packed.Should().Equal(expected);
            }

            [Theory]
            [InlineData(17, new byte[] {0xD0, 0x11})]
            public void PackD0StringCorrectly(int size, byte[] expected)
            {
                var s = new string('a', size);

                var packed = PackStream.Pack(s);
                packed.Take(2).ToArray().Should().Equal(expected);
            }

            [Theory]
            [InlineData(0x0100, new byte[] {0xD1, 0x01, 0x00})]
            [InlineData(0xFFFF, new byte[] {0xD1, 0xFF, 0xFF})]
            public void PackD1StringCorrectly(int size, byte[] expected)
            {
                var s = new string('a', size);

                var packed = PackStream.Pack(s);
                packed.Take(3).ToArray().Should().Equal(expected, size.ToString("X"));
            }
        }

        public class UnpackMethod
        {
            [Theory]
            [InlineData(new byte[] {0x80}, "")]
            [InlineData(new byte[] {0x81, 0x61}, "a")]
            public void UnpacksSmallStringCorrectly(byte[] input, string expected)
            {
                var actual = Packers.Text.Unpack(input);
                actual.Should().Be(expected);
            }

            [Theory]
            [InlineData(new byte[] {0xD0, 0x11, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61, 0x61}, "aaaaaaaaaaaaaaaaa")]
            public void UnpacksD0StringsCorrectly(byte[] input, string expected)
            {
                var actual = Packers.Text.Unpack(input);
                actual.Should().Be(expected);
            }

            [Theory]
            //[InlineData(new byte[] {0x01, 0x00}, 256)]
            [InlineData(new byte[] {0xFF, 0xFF}, 65535)]
            public void UnpacksD1StringCorrrectly(byte[] marker, int size)
            {
                var expected = new string('a', size);
                var content = new List<byte> {0xD1};
                content.AddRange(marker);
                content.AddRange(Encoding.UTF8.GetBytes(expected));

                var actual = Packers.Text.Unpack(content.ToArray());
                actual.Should().Be(expected);
            }

            [Theory]
            [InlineData(new byte[] {0x00, 0x01, 0x00, 0x00}, 65536)]
            [InlineData(new byte[] {0x00, 0x0F, 0x42, 0x40}, 1000000)]
            public void UnpacksD2StringCorrrectly(byte[] marker, int size)
            {
                var expected = new string('a', size);
                var content = new List<byte> {0xD2};
                content.AddRange(marker);
                content.AddRange(Encoding.UTF8.GetBytes(expected));

                var actual = Packers.Text.Unpack(content.ToArray());
                actual.Should().Be(expected);
            }
        }

        public class GetSizeInBytesMethod
        {
        }
    }
}