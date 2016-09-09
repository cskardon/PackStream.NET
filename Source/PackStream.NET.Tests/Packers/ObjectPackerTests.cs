namespace PackStream.NET.Tests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::PackStream.NET;
    using global::PackStream.NET.Packers;
    using Xunit;
    using Packers = global::PackStream.NET.Packers.Packers;

    public class ObjectPackerTests
    {
        private class SimpleClassWithField
        {
#pragma warning disable 414
            public bool _a;
#pragma warning restore 414
        }

        private class SimpleClass
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public bool A { get; set; }
        }

        private class MultiPropertyClass : SimpleClass
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public bool B { get; set; }
        }

        private class SimpleClassWithPrivateProperty : SimpleClass
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            private bool B { get; set; }
            public void SetB(bool value)
            {
                B = value;
            }
        }

        private class SimpleClassWithBoltIgnoredProperty : SimpleClass
        {
            [PackStreamIgnore]
            public bool B { get; set; }
        }

        public class PackMethod
        {
            [Fact]
            public void PacksPrivatePropertyCorrectly()
            {
                var toPack = new SimpleClassWithPrivateProperty {A = true};
                toPack.SetB(true);
                var expected = new List<byte> { 0xA2, 0x81, 0x42, Markers.True, 0x81, 0x41, Markers.True };


                var actual = Packers.Map.Pack(toPack);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void PacksAnonymousCorrectly()
            {
                var toPack = new {A = true};
                var expected = new List<byte> {0xA1, 0x81, 0x41, Markers.True}; //1 fields


                var actual = Packers.Map.Pack(toPack);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void PacksDefinedClassCorrectly()
            {
                var toPack = new SimpleClass {A = true};
                var expected = new List<byte> {0xA1, 0x81, 0x41, Markers.True}; //1 fields


                var actual = Packers.Map.Pack(toPack);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void PacksDefinedClassWithMultiplePropertiesCorrectly()
            {
                var toPack = new MultiPropertyClass {A = true, B = false};
                var expected = new List<byte> {0xA2, 0x81, 0x42, Markers.False, 0x81, 0x41, Markers.True};


                var actual = Packers.Map.Pack(toPack);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void PacksDefinedClassCorrectlyIgnoringBoltIgnoredProperties()
            {
                var toPack = new SimpleClassWithBoltIgnoredProperty {A = true};
                var expected = new List<byte> {0xA1, 0x81, 0x41, Markers.True}; //1 fields


                var actual = Packers.Map.Pack(toPack);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(0, new byte[] {0xA0})]
            [InlineData(1, new byte[] {0xA1, 0x82, 0x61, 0x30, 0x82, 0x61, 0x30})]
            public void PacksDictionary_StringString_Correctly(int size, byte[] expected)
            {
                var dictionary = new Dictionary<string, string>();
                for (var i = 0; i < size; i++)
                    dictionary.Add("a" + i, "a" + i);


                var actual = Packers.Map.Pack(dictionary);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void PackEmptyDictionaryCorrectly()
            {
                var dic = new Dictionary<string, string>();
                var packed = Packers.Map.Pack(dic);

                packed.Should().NotBeNull();
                var expected = new byte[] { 0xA0 };

                packed.Should().BeEquivalentTo(expected);
            }
        }

        public class UnpackMethod
        {
            [Theory]
            [InlineData(0, new byte[] {0xA0})]
            [InlineData(1, new byte[] {0xA1, 0x82, 0x61, 0x30, 0x82, 0x61, 0x30})]
            public void UnpacksDictionary_StringString_Correctly(int size, byte[] input)
            {
                var expected = new Dictionary<string, string>();
                for (var i = 0; i < size; i++)
                    expected.Add("a" + i, "a" + i);

                var actual = Packers.Map.Unpack<Dictionary<string, string>>(input);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void UnpacksDefinedClass_WithProperty()
            {
                var expected = new SimpleClass {A = true};
                var toUnpack = new List<byte> {0xA1, 0x81, 0x41, Markers.True}; //1 fields


                var actual = Packers.Map.Unpack<SimpleClass>(toUnpack.ToArray());
                actual.ShouldBeEquivalentTo(expected);
            }

            [Fact]
            public void PacksPrivatePropertyCorrectly()
            {
                var expected = new SimpleClassWithPrivateProperty { A = true };
                expected.SetB(true);
                var toUnpack = new List<byte> { 0xA2, 0x81, 0x42, Markers.True, 0x81, 0x41, Markers.True };


                var actual = Packers.Map.Unpack<SimpleClassWithPrivateProperty>(toUnpack.ToArray());
                actual.ShouldBeEquivalentTo(expected);
            }

            [Fact]
            public void UnpacksDefinedClass_WithField()
            {
                var expected = new SimpleClassWithField {_a = true};
                var toUnpack = new List<byte> {0xA1, 0x82, 0x5F, 0x61, Markers.True}; //1 fields


                var actual = Packers.Map.Unpack<SimpleClassWithField>(toUnpack.ToArray());
                actual.ShouldBeEquivalentTo(expected);
            }

            [Fact]
            public void UnpPacksDefinedClassCorrectlyIgnoringBoltIgnoredProperties()
            {
                var expected = new SimpleClassWithBoltIgnoredProperty {A = true};
                var toUnpack = new List<byte> {0xA1, 0x81, 0x41, Markers.True}.ToArray(); //1 fields


                var actual = Packers.Map.Unpack<SimpleClassWithBoltIgnoredProperty>(toUnpack);
                actual.ShouldBeEquivalentTo(expected);
            }

            [Fact]
            public void UnpacksSuccessMetaDataProperly()
            {
                var bytes = new byte[] {0xA1, 0x86, 0x66, 0x69, 0x65, 0x6C, 0x64, 0x73, 0x91, 0x81, 0x78};
                var expected = new Dictionary<string, dynamic> {{"fields", new List<dynamic> {"x"}}};

                var actual = Packers.Map.Unpack<Dictionary<string, dynamic>>(bytes);
                actual.Keys.Should().BeEquivalentTo(expected.Keys);
                Assert.Equal(actual.Values, expected.Values);
            }
            [Fact]
            public void UnpacksSuccessMetaDataProperlyAsString()
            {
                var bytes = new byte[] { 0xA1, 0x86, 0x66, 0x69, 0x65, 0x6C, 0x64, 0x73, 0x91, 0x81, 0x78 };
                var expected = new Dictionary<string, IEnumerable<string>> { { "fields", new List<string> { "x" } } };

                var actual = Packers.Map.Unpack<Dictionary<string, IEnumerable<string>>>(bytes);

                foreach (var kvp in expected)
                {
                    actual.Keys.Should().Contain(kvp.Key);
                    foreach (var val in kvp.Value)
                        actual[kvp.Key].Should().Contain(val);
                }
            }

            [Fact(Skip = "To come back to")]
            public void UnpacksAnonymousCorrectly()
            {
                throw new NotImplementedException();
            }
        }
    }
}