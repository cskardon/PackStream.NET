namespace PackStream.NET.Packers
{
    using System.Collections.Generic;
    using System.Linq;
    using global::PackStream;

    public class Node<T> where T : new()
    {
        public Node() { } 
        public Node(byte[] bytes)
        {
            byte[] editedBytes;
            Id = GetId(bytes, out editedBytes);
            Labels = GetLabels(editedBytes, out editedBytes);
            Data = PackStream.Unpack<T>(editedBytes);
        }

        private static IEnumerable<string> GetLabels(byte[] bytes, out byte[] editedBytes)
        {
            //List of strings.
            var length = global::PackStream.NET.Packers.Packers.List.GetLengthInBytes(bytes, true);

            var labels = global::PackStream.NET.Packers.Packers.List.Unpack<string>(bytes.Take(length).ToArray());
            editedBytes = bytes.Skip(length).ToArray();


            return labels;
        }

        private static string GetId(byte[] bytes, out byte[] editedBytes)
        {
            var length = global::PackStream.NET.Packers.Packers.Text.GetExpectedSize(bytes);
            var markerSize = global::PackStream.NET.Packers.Packers.Text.SizeOfMarkerInBytes(bytes);

            var bytesToUnpack = bytes.Take(length + markerSize);
            editedBytes = bytes.Skip(length + markerSize).ToArray();
            var id = global::PackStream.NET.Packers.Packers.Text.Unpack(bytesToUnpack.ToArray());
            return id;
        }


        public string Id { get; set; }
        public IEnumerable<string> Labels { get; set; }
        public T Data { get; set; }
    }

    public class Relationship<T> where T : new()
    {
        public Relationship() { }

        public Relationship(byte[] bytes)
        {
            
        }

        public T Data { get; set; }
    }
}