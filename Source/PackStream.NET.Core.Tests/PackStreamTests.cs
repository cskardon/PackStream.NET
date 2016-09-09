namespace PackStream.NET.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using global::PackStream.NET.Packers;
    using Xunit;

    public partial class PackStreamTests
    {
        public class ConvertSizeToBytesMethod
        {
            [Theory]
            [InlineData(0, 2, new byte[] {0x00, 0x00})]
            [InlineData(1, 2, new byte[] {0x00, 0x01})]
            public void PadsPositiveNumbersCorrectly(int value, int padding, byte[] expected)
            {
                var actual = PackStream.ConvertSizeToBytes(value, 2);
                actual.Should().Equal(expected);
            }
        }

        public class UnpackTMethod
        {
            [Fact]
            public void UnpacksSuccessMetaDataProperlyAsString()
            {
                var bytes = new byte[] {0xA1, 0x86, 0x66, 0x69, 0x65, 0x6C, 0x64, 0x73, 0x91, 0x81, 0x78};
                var expected = new Dictionary<string, IEnumerable<string>> {{"fields", new List<string> {"x"}}};

                var actual = PackStream.Unpack<Dictionary<string, IEnumerable<string>>>(bytes);

                foreach (var kvp in expected)
                {
                    actual.Keys.Should().Contain(kvp.Key);
                    foreach (var val in kvp.Value)
                        actual[kvp.Key].Should().Contain(val);
                }
            }
        }

        public class UnpackRecordMethod
        {
            [Fact]
            public void ShouldUnpackCorrectly_UsingDynamic()
            {
                var fields = new[] {"Name", "Year"};
                var input = new byte[] {0x92, 0x8B, 0x43, 0x6C, 0x6F, 0x75, 0x64, 0x20, 0x41, 0x74, 0x6C, 0x61, 0x73, 0xC9, 0x07, 0xDC}; //Cloud Atlas | 2012

                var actual = PackStream.UnpackRecord(input, fields);
                Assert.Equal(actual.Name, "Cloud Atlas");
                Assert.Equal(actual.Year, 2012);
            }
        }

        public class PackTMethod
        {
            [Theory]
            [InlineData(0, new byte[] {0x80})]
            [InlineData(1, new byte[] {0x81})]
            public void PacksSmallStringCorrectly(int size, byte[] expected)
            {
                var s = new string('a', size);

                var packed = PackStream.Pack(s);
                packed.Take(1).ToArray().Should().Equal(expected);
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

            [Fact]
            public void PacksNullCorrectly()
            {
                var packed = PackStream.Pack<object>(null);
                packed.Should().BeEquivalentTo(new[] {Markers.Null});
            }

            [Fact]
            public void PackEmptyDictionaryCorrectly()
            {
                var dic = new Dictionary<string, string>();
                var packed = PackStream.Pack(dic);

                packed.Should().NotBeNull();
                var expected = new byte[] {0xA0};

                packed.Should().BeEquivalentTo(expected);
            }
        }
    }
}