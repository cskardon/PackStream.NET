namespace PackStream.NET.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using global::PackStream.NET.Packers;
    using Xunit;
    using Packers = global::PackStream.NET.Packers.Packers;

    public class ListPackerTests
    {
        public class GetMarkerMethod
        {
            [Theory]
            [InlineData(0, new byte[] {0x90})]
            [InlineData(1, new byte[] {0x91})]
            [InlineData(2, new byte[] {0x92})]
            [InlineData(3, new byte[] {0x93})]
            [InlineData(4, new byte[] {0x94})]
            [InlineData(5, new byte[] {0x95})]
            [InlineData(6, new byte[] {0x96})]
            [InlineData(7, new byte[] {0x97})]
            [InlineData(8, new byte[] {0x98})]
            [InlineData(9, new byte[] {0x99})]
            [InlineData(10, new byte[] {0x9A})]
            [InlineData(11, new byte[] {0x9B})]
            [InlineData(12, new byte[] {0x9C})]
            [InlineData(13, new byte[] {0x9D})]
            [InlineData(14, new byte[] {0x9E})]
            [InlineData(15, new byte[] {0x9F})]
            public void GetsCorrectMarker_ForListsUpTo15Items(int items, byte[] expected)
            {
                var actual = Packers.List.GetMarker(items);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(16, new byte[] {0xD4, 0x10})]
            [InlineData(255, new byte[] {0xD4, 0xFF})]
            public void GetsCorrectMarker_For8BitLists(int items, byte[] expected)
            {
                var actual = Packers.List.GetMarker(items);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(256, new byte[] {0xD5, 0x01, 0x00})]
            [InlineData(0xFFFF, new byte[] {0xD5, 0xFF, 0xFF})]
            public void GetsCorrectMarker_For16BitLists(int items, byte[] expected)
            {
                var actual = Packers.List.GetMarker(items);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(0x010000, new byte[] {0xD6, 0x01, 0x00, 0x00})]
            [InlineData(0xFFFFFF, new byte[] {0xD6, 0xFF, 0xFF, 0xFF})]
            [InlineData(uint.MaxValue, new byte[] {0xD6, 0xFF, 0xFF, 0xFF, 0xFF})]
            public void GetsCorrectMarker_For32BitLists(long items, byte[] expected)
            {
                var actual = Packers.List.GetMarker(items);
                actual.Should().Equal(expected);
            }
        }

        public class UnpackMethod
        {
            [Theory]
            [InlineData("a", new byte[] {0x91, 0x81, 0x61})]
            [InlineData("aa", new byte[] {0x91, 0x82, 0x61, 0x61})]
            public void UnpacksStringsCorrectly(string expected, byte[] input)
            {
                var expectedList = new List<string> {expected};

                var response = Packers.List.Unpack<string>(input);
                response.Should().Equal(expectedList);
            }

            [Theory]
            [InlineData(2, new byte[] {0x92, 0x81, 0x61, 0x81, 0x61})]
            [InlineData(3, new byte[] {0x93, 0x81, 0x61, 0x81, 0x61, 0x81, 0x61})]
            public void UnpacksMultipleStringsCorrectly(int count, byte[] input)
            {
                var expectedList = new List<string>();
                for (var i = 0; i < count; i++)
                    expectedList.Add("a");


                var response = Packers.List.Unpack<string>(input);
                response.Should().Equal(expectedList);
            }

            [Fact]
            public void UnpacksListWithMultipleTypes()
            {
                var input = new byte[] {0x92, 0x8B, 0x43, 0x6C, 0x6F, 0x75, 0x64, 0x20, 0x41, 0x74, 0x6C, 0x61, 0x73, 0xC9, 0x07, 0xDC}; //Cloud Atlas | 2012
                var expectedList = new List<dynamic> {"Cloud Atlas", 2012};

                var response = Packers.List.Unpack<dynamic>(input).ToList();
                ((bool) (response.First() == expectedList.First())).Should().BeTrue();
                ((bool) (response.Skip(1).First() == expectedList.Skip(1).First())).Should().BeTrue();
            }
        }

        public class PackMethod
        {
            [Theory]
            [InlineData("a", new byte[] {0x91, 0x81, 0x61})]
            [InlineData("aa", new byte[] {0x91, 0x82, 0x61, 0x61})]
            public void PacksStringsCorrectly(string inputString, byte[] expected)
            {
                var input = new List<string> {inputString};

                var response = Packers.List.Pack(input);

                response.Should().Equal(expected);
            }

            [Fact]
            public void PacksNullCorrectly()
            {
                var input = new List<bool?> {null};

                var response = Packers.List.Pack(input);

                var expected = new byte[] {0x91, 0xC0};
                response.Should().Equal(expected);
            }

            [Theory]
            [InlineData(true, new byte[] {0x91, 0xC3})]
            [InlineData(false, new byte[] {0x91, 0xC2})]
            public void PacksBoolCorrectly(bool inputBool, byte[] expected)
            {
                var input = new List<bool> {inputBool};

                var response = Packers.List.Pack(input);

                response.Should().Equal(expected);
            }

            [Theory]
            [InlineData(true, new byte[] {0x91, 0xC3})]
            [InlineData(false, new byte[] {0x91, 0xC2})]
            public void PacksNullableBoolCorrectly(bool? inputBool, byte[] expected)
            {
                var input = new List<bool?> {inputBool};

                var response = Packers.List.Pack(input);

                response.Should().Equal(expected);
            }

            [Theory]
            [InlineData(1.1, new byte[] {0x91, 0xC1, 0x3F, 0xF1, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9A})]
            [InlineData(-1.1, new byte[] {0x91, 0xC1, 0xBF, 0xF1, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9A})]
            public void PacksFloatsCorrectly(double d, byte[] expected)
            {
                var input = new List<double> {d};

                var response = Packers.List.Pack(input);

                response.Should().Equal(expected);
            }

            [Fact]
            public void PacksListsCorrectly()
            {
                var input = new List<IEnumerable<int>> {new List<int> {1}};
                var expected = new byte[] {0x91, 0x91, 0x01};
                var actual = Packers.List.Pack(input);

                actual.Should().Equal(expected);
            }

            [Fact]
            public void PacksDictionariesCorrectly()
            {
                var input = new List<Dictionary<string, string>> {new Dictionary<string, string> {{"a0", "a0"}}};

                var expected = new byte[] {0x91, 0xA1, 0x82, 0x61, 0x30, 0x82, 0x61, 0x30};


                var actual = Packers.List.Pack(input);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(1, new byte[] {0x91, 0x01})]
            [InlineData(42, new byte[] {0x91, 0x2A})]
            [InlineData(128, new byte[] {0x91, 0xC9, 0x00, 0x80})]
            public void PacksPositiveIntsCorrectly(int i, byte[] expected)
            {
                var input = new List<int> {i};

                var response = Packers.List.Pack(input);

                response.Should().Equal(expected);
            }

            [Theory]
            [InlineData(-16, new byte[] {0x91, 0xF0})]
            [InlineData(-128, new byte[] {0x91, 0xC8, 0x80})]
            [InlineData(-129, new byte[] {0x91, 0xC9, 0xFF, 0x7F})]
            public void PacksNegativeIntsCorrectly(int i, byte[] expected)
            {
                var input = new List<int> {i};

                var response = Packers.List.Pack(input);

                response.Should().Equal(expected);
            }

            [Fact]
            public void PacksObjectsCorrectly()
            {
                var toPack = new List<SimpleClass> {new SimpleClass {A = true}};
                var expected = new List<byte> {0x91, 0xA1, 0x81, 0x41, Markers.True };

                var actual = Packers.List.Pack(toPack);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(1.1, new byte[] {0x91, 0xC1, 0x3F, 0xF1, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9A})]
            [InlineData(-16, new byte[] {0x91, 0xF0})]
            public void PacksDynamicCorrectly(dynamic element, byte[] expected)
            {
                var input = new List<dynamic> {element};

                var response = Packers.List.Pack(input);

                response.Should().Equal(expected);
            }

            private class SimpleClass
            {
                // ReSharper disable once UnusedAutoPropertyAccessor.Local
                public bool A { get; set; }
            }
        }

        public class GetSizeInBytesMethod
        {
            [Fact]
            public void ReturnsRightSizeForNoElements()
            {
                var bytes = new byte[] {0x90};
                var withMarker = Packers.List.GetLengthInBytes(bytes, true);
                withMarker.Should().Be(1);

                var withoutMarker = Packers.List.GetLengthInBytes(bytes, false);
                withoutMarker.Should().Be(0);
            }

            [Fact]
            public void ReturnsRightSizeForSingleElement()
            {
                var bytes = new byte[] {0x91, 0x81, 0x61};
                var withMarker = Packers.List.GetLengthInBytes(bytes, true);
                withMarker.Should().Be(3);

                var withoutMarker = Packers.List.GetLengthInBytes(bytes, false);
                withoutMarker.Should().Be(2);
            }

            [Fact]
            public void ReturnsRightSizeForTwoElements()
            {
                var bytes = new byte[] {0x92, 0x81, 0x61, 0x81, 0x62}; //a,b
                var withMarker = Packers.List.GetLengthInBytes(bytes, true);
                withMarker.Should().Be(5);

                var withoutMarker = Packers.List.GetLengthInBytes(bytes, false);
                withoutMarker.Should().Be(4);
            }

            [Fact]
            public void ReturnsRightSizeForNestedLists()
            {
                var bytes = new byte[] {0x91, 0x91, 0x81, 0x61};
                var withMarker = Packers.List.GetLengthInBytes(bytes, true);
                withMarker.Should().Be(4);

                var withoutMarker = Packers.List.GetLengthInBytes(bytes, false);
                withoutMarker.Should().Be(3);
            }

            [Fact]
            public void ReturnsRightSizeForNestedMaps()
            {
                var bytes = new byte[] {0x91, 0xA1, 0x61, 0x91}; //{a:1}
                var withMarker = Packers.List.GetLengthInBytes(bytes, true);
                withMarker.Should().Be(4);

                var withoutMarker = Packers.List.GetLengthInBytes(bytes, false);
                withoutMarker.Should().Be(3);
            }

            [Fact]
            public void ReturnsRightSizeForNestedIntegers()
            {
                //CDS: try with bigger ints.
                var bytes = new byte[] {0x91, 0x01};
                var withMarker = Packers.List.GetLengthInBytes(bytes, true);
                withMarker.Should().Be(2);

                var withoutMarker = Packers.List.GetLengthInBytes(bytes, false);
                withoutMarker.Should().Be(1);
            }

            [Fact]
            public void ReturnsRightSizeForNestedFloat()
            {
                //1.1
                var bytes = new byte[] {0x91, 0xC1, 0x3F, 0xF1, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9A};
                var withMarker = Packers.List.GetLengthInBytes(bytes, true);
                withMarker.Should().Be(10);

                var withoutMarker = Packers.List.GetLengthInBytes(bytes, false);
                withoutMarker.Should().Be(9);
            }

            [Fact]
            public void ReturnsRightSizeForNestedBoolean()
            {
                //CDS: try with bigger ints.
                var bytes = new byte[] {0x91, 0xC3};
                var withMarker = Packers.List.GetLengthInBytes(bytes, true);
                withMarker.Should().Be(2);

                var withoutMarker = Packers.List.GetLengthInBytes(bytes, false);
                withoutMarker.Should().Be(1);
            }

            [Fact]
            public void ReturnsRightSizeForNestedNull()
            {
                //CDS: try with bigger ints.
                var bytes = new byte[] {0x91, 0xC0};
                var withMarker = Packers.List.GetLengthInBytes(bytes, true);
                withMarker.Should().Be(2);

                var withoutMarker = Packers.List.GetLengthInBytes(bytes, false);
                withoutMarker.Should().Be(1);
            }
        }
    }
}