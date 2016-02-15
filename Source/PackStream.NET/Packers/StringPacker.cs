namespace PackStream.NET.Packers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using global::PackStream;

    /*  Text
        ----

        Text data is represented as UTF-8 encoded binary data. Note that sizes used
        for text are the byte counts of the UTF-8 encoded data, not the character count
        of the original text.

          Marker | Size                                        | Maximum size
         ========|=============================================|=====================
          80..8F | contained within low-order nibble of marker | 15 bytes
          D0     | 8-bit big-endian unsigned integer           | 255 bytes
          D1     | 16-bit big-endian unsigned integer          | 65 535 bytes
          D2     | 32-bit big-endian unsigned integer          | 4 294 967 295 bytes

        For encoded text containing fewer than 16 bytes, including empty strings,
        the marker byte should contain the high-order nibble `1000` followed by a
        low-order nibble containing the size. The encoded data then immediately
        follows the marker.

        For encoded text containing 16 bytes or more, the marker 0xD0, 0xD1 or 0xD2
        should be used, depending on scale. This marker is followed by the size and
        the UTF-8 encoded data. Examples follow below:

            80  -- ""

            81 61  -- "a"

            D0 1A 61 62  63 64 65 66  67 68 69 6A  6B 6C 6D 6E
            6F 70 71 72  73 74 75 76  77 78 79 7A  -- "abcdefghijklmnopqrstuvwxyz"

            D0 18 45 6E  20 C3 A5 20  66 6C C3 B6  74 20 C3 B6
            76 65 72 20  C3 A4 6E 67  65 6E  -- "En å flöt över ängen"
    */

    public static partial class Packers
    {
        public static class Text
        {
            public static byte[] Pack(string toPack)
            {
                if (toPack == null)
                    return null;

                var encoded = Encoding.UTF8.GetBytes(toPack);

                var output = new List<byte>(Marker(encoded.Length));
                output.AddRange(encoded);

                return output.ToArray();
            }

            public static string Unpack(byte[] content)
            {
                var markerLess = RemoveMarker(content);
                var decoded = Encoding.UTF8.GetString(markerLess, 0, markerLess.Length);
                return decoded;
            }

            private static IEnumerable<byte> Marker(long size)
            {
                if (size <= 15)
                    return new[] {(byte) (0x80 + size)};

                var output = new List<byte>();
                if (size <= 255)
                    output.Add(0xD0);

                else if (size <= 65535)
                    output.Add(0xD1);

                else if (size <= 4294967295)
                    output.Add(0xD2);

                if (size > uint.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(size), size, "Size given is too big.");

                output.AddRange(PackStream.GetLength(size));
                return output.ToArray();
            }

            private static byte[] RemoveMarker(byte[] content)
            {
                int toSkip;
                var expectedSize = GetExpectedSizeFromMarker(content, out toSkip);

                var output = content.Skip(toSkip).ToArray();
                if (output.Length != expectedSize)
                    throw new ArgumentException($"Data length ({output.Length}) doesn't match marker given ({expectedSize}).", nameof(content));

                return output;
            }

            public static bool Is(byte[] content)
            {
                return (content[0] >= 0x80 && content[0] <= 0x8F) || (content[0] >= 0xD0 && content[0] <= 0xD2);
            }

            public static int SizeOfMarkerInBytes(byte[] content)
            {
                if (content[0] >= 0x80 && content[0] <= 0x8F)
                    return 1;
                if (content[0] == 0xD0)
                    return 2;
                if (content[0] == 0xD1)
                    return 3;
                if (content[0] == 0xD2)
                    return 5;

                throw new ArgumentOutOfRangeException(nameof(content), content[0], "Unknown Marker");
            }

            public static int GetExpectedSize(byte[] content)
            {
                if (content[0] >= 0x80 && content[0] <= 0x8F)
                {
                    return content[0] - 0x80;
                }
                if (content[0] == 0xD0)
                {
                    return content[1];
                }
                if (content[0] == 0xD1)
                {
                    var markerSize = content.Skip(1).Take(2).ToArray();
                    return (ushort) global::PackStream.NET.Packers.Packers.BitConverter.ToInt16(markerSize);
                }
                if (content[0] == 0xD2)
                {
                    var markerSize = content.Skip(1).Take(4).ToArray();
                    return  global::PackStream.NET.Packers.Packers.BitConverter.ToInt32(markerSize);
                }
                throw new ArgumentOutOfRangeException(nameof(content), content[0], "Unknown Marker");
            }

            private static long GetExpectedSizeFromMarker(byte[] content, out int toSkip)
            {
                //IF NO MARKER RETURN CONTENT AS IS

                toSkip = -1;
                if (content[0] >= 0x80 && content[0] <= 0x8F)
                    toSkip = 1;
                if (content[0] == 0xD0)
                    toSkip = 2;
                if (content[0] == 0xD1)
                    toSkip = 3;
                if (content[0] == 0xD2)
                    toSkip = 5;
                if (toSkip == -1)
                    throw new ArgumentOutOfRangeException(nameof(content), content[0], "Unknown Marker.");

                return GetExpectedSize(content);
            }
        }

        public static class Null
        {
            public static bool Is(byte[] content)
            {
                return content[0] == 0xC0;
            }
        }
    }
}