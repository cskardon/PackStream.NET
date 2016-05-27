namespace PackStream.NET.Packers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::PackStream.NET.Extensions;

    /*
     Integers
        --------

        Integer values occupy either 1, 2, 3, 5 or 9 bytes depending on magnitude and
        are stored as big-endian signed values. Several markers are designated
        specifically as TINY_INT values and can therefore be used to pass a small
        number in a single byte. These markers can be identified by a zero high-order
        bit or by a high-order nibble containing only ones.

        The available encodings are illustrated below and each shows a valid
        representation for the decimal value 42:

            2A                          -- TINY_INT

            C8 2A                       -- INT_8

            C9 00 2A                    -- INT_16

            CA 00 00 00 2A              -- INT_32

            CA 00 00 00 00 00 00 00 2A  -- INT_64

        Note that while encoding small numbers in wider formats is supported, it is
        generally recommended to use the most compact representation possible. The
        following table shows the optimal representation for every possible integer:

           Range Minimum             |  Range Maximum             | Representation
         ============================|============================|================
          -9 223 372 036 854 775 808 |             -2 147 483 649 | INT_64
                      -2 147 483 648 |                    -32 769 | INT_32
                             -32 768 |                       -129 | INT_16
                                -128 |                        -17 | INT_8
                                 -16 |                       +127 | TINY_INT
                                +128 |                    +32 767 | INT_16
                             +32 768 |             +2 147 483 647 | INT_32
                      +2 147 483 648 | +9 223 372 036 854 775 807 | INT_64
        */

    public static partial class Packers
    {
        public static class Int
        {
            public static bool Is(byte[] content)
            {
                var marker = content[0];
                if (((sbyte)marker <= 127 && (sbyte)marker >= -16) || (marker >= 0xC8 && marker <= 0xCB))
                    return true;

                return false;
            }

            public static int SizeOfMarkerInBytes(byte[] content)
            {
                if ((sbyte)content[0] <= 127 && (sbyte)content[0] >= -16)
                    return 0;

                return 1;
            }

            public static int GetExpectedSizeInBytes(byte[] content)
            {
                if ((sbyte)content[0] <= 127 && (sbyte)content[0] >= -16)
                    return 1;

                switch (content[0])
                {
                    case 0xC8:
                        return 1;
                    case 0xC9:
                        return 2;
                    case 0xCA:
                        return 4;
                    case 0xCB:
                        return 8;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(content), content[0], "Unknown marker");
                }

            }

            private static readonly byte[] Int64Marker = { Markers.Int64 };
            private static readonly byte[] Int32Marker = { Markers.Int32 };
            private static readonly byte[] Int16Marker = { Markers.Int16 };
            private static readonly byte[] Int8Marker = { Markers.Int8 };
            private static readonly byte[] TinyIntMarker = { };

            internal static byte[] GetMarker(long value)
            {
                if (value < int.MinValue)
                    return Int64Marker;
                if (value >= int.MinValue && value < short.MinValue)
                    return Int32Marker;
                if (value >= short.MinValue && value < sbyte.MinValue)
                    return Int16Marker;
                if (value >= sbyte.MinValue && value < -16)
                    return Int8Marker;
                if (value >= -16 && value <= sbyte.MaxValue)
                    return TinyIntMarker;
                if (value > sbyte.MaxValue && value <= short.MaxValue)
                    return Int16Marker;
                if (value > short.MaxValue && value <= int.MaxValue)
                    return Int32Marker;
                return Int64Marker;
            }

            private static byte[] ConvertPositiveLongs(IList<byte> output, long value)
            {
                var size = 1;
                if (value >= 128)
                    size = 2;

                if (value >= 32768)
                    size = 4;

                if (value >= 2147483648)
                    size = 8;

                var toPad = size - output.Count;
                return output.PadLeft(toPad, (byte)0).Skip(output.Count - size).ToArray();
            }

            private static byte[] ConvertNegativeLongs(ICollection<byte> output, long value)
            {
                if (value >= sbyte.MinValue)
                    return new[] { output.Last() };

                if (value >= -32768)
                    return output.Skip(output.Count - 2).ToArray();

                if (value >= -2147483648)
                    return output.Skip(output.Count - 4).ToArray();

                return output.ToArray();
            }


            internal static byte[] ConvertLongToBytes(long value)
            {
                var output = new List<byte>(PackStreamBitConverter.GetBytes(value));
                return value >= 0 ? ConvertPositiveLongs(output, value) : ConvertNegativeLongs(output, value);
            }

            public static byte[] Pack(long content)
            {
                var output = new List<byte>(GetMarker(content));

                output.AddRange(ConvertLongToBytes(content));

                return output.ToArray();
            }

            public static long Unpack(byte[] content)
            {
                if (content.Length == 1 && content[0] <= 0xFF)
                    return (sbyte) content[0];

                var toInterpret = content.Skip(1).ToArray();
                switch (content[0])
                {
                    case Markers.Int8:
                        return (sbyte) toInterpret[0];
                    case Markers.Int16:
                        return PackStreamBitConverter.ToInt16(toInterpret);
                    case Markers.Int32:
                        return PackStreamBitConverter.ToInt32(toInterpret);
                    case Markers.Int64:
                        return PackStreamBitConverter.ToInt64(toInterpret);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(content), content[0], "Unknown Marker");
                }
            }
        }
    }

}