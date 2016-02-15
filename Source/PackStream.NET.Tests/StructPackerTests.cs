namespace PackStream.NET.Tests
{
    using FluentAssertions;
    using global::PackStream;
    using Xunit;
    using Packers = global::PackStream.NET.Packers.Packers;

    public class StructPackerTests
    {
        public class ToNodeTMethod
        {
            [Fact]
            public void UnpacksNodeCorrectly()
            {
                //Node, 105, Label = Movie, tagline = Everything is connected, released = 2012, Title = Cloud Atlas 
                //removed 0x91
                var bytes = new byte[] {0xB3, 0x4E, 0x88, 0x6E, 0x6F, 0x64, 0x65, 0x2F, 0x31, 0x30, 0x35, 0x91, 0x85, 0x4D, 0x6F, 0x76, 0x69, 0x65, 0xA3, 0x87, 0x74, 0x61, 0x67, 0x6C, 0x69, 0x6E, 0x65, 0xD0, 0x17, 0x45, 0x76, 0x65, 0x72, 0x79, 0x74, 0x68, 0x69, 0x6E, 0x67, 0x20, 0x69, 0x73, 0x20, 0x63, 0x6F, 0x6E, 0x6E, 0x65, 0x63, 0x74, 0x65, 0x64, 0x85, 0x74, 0x69, 0x74, 0x6C, 0x65, 0x8B, 0x43, 0x6C, 0x6F, 0x75, 0x64, 0x20, 0x41, 0x74, 0x6C, 0x61, 0x73, 0x88, 0x72, 0x65, 0x6C, 0x65, 0x61, 0x73, 0x65, 0x64, 0xC9, 0x07, 0xDC};
                var strct = Packers.Struct.Unpack(bytes);

                var node = strct.GetNode<Movie>();
                node.Id.Should().Be("node/105");
                node.Labels.Should().HaveCount(1);
                node.Labels.Should().Contain("Movie");

                node.Data.released.Should().Be(2012);
                node.Data.tagline.Should().Be("Everything is connected");
                node.Data.title.Should().Be("Cloud Atlas");
            }

            private class Movie
            {
                // ReSharper disable UnusedAutoPropertyAccessor.Local
                // ReSharper disable InconsistentNaming
                public long released { get; set; }
                public string title { get; set; }
                public string tagline { get; set; }
                // ReSharper restore UnusedAutoPropertyAccessor.Local
                // ReSharper restore InconsistentNaming
            }
        }
    }

    public partial class PackStreamTests
    {
        public class GetPackedEntitiesMethod
        {
            [Fact]
            public void GetsNumberOfItemsOfStruct()
            {
                var bytes = new byte[] {0xB3, 0x4E, 0x88, 0x6E, 0x6F, 0x64, 0x65, 0x2F, 0x31, 0x30, 0x35, 0x91, 0x85, 0x4D, 0x6F, 0x76, 0x69, 0x65, 0xA3, 0x87, 0x74, 0x61, 0x67, 0x6C, 0x69, 0x6E, 0x65, 0xD0, 0x17, 0x45, 0x76, 0x65, 0x72, 0x79, 0x74, 0x68, 0x69, 0x6E, 0x67, 0x20, 0x69, 0x73, 0x20, 0x63, 0x6F, 0x6E, 0x6E, 0x65, 0x63, 0x74, 0x65, 0x64, 0x85, 0x74, 0x69, 0x74, 0x6C, 0x65, 0x8B, 0x43, 0x6C, 0x6F, 0x75, 0x64, 0x20, 0x41, 0x74, 0x6C, 0x61, 0x73, 0x88, 0x72, 0x65, 0x6C, 0x65, 0x61, 0x73, 0x65, 0x64, 0xC9, 0x07, 0xDC};
                var packedEntities = PackStream.GetPackedEntities(bytes);
                packedEntities.Length.Should().Be(1);
                packedEntities[0].PackType.Should().Be(PackType.Structure);
                packedEntities[0].NumberOfItems.Should().Be(3);
            }
        }
    }
}