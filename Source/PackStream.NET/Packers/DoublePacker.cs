namespace PackStream.NET.Packers
{
    using System.Collections.Generic;
    using System.Linq;

    /*
    Floating Point Numbers
    ----------------------

    These are double-precision floating points for approximations of any number,
    notably for representing fractions and decimal numbers. Floats are encoded as a
    single 0xC1 marker byte followed by 8 bytes, formatted according to the IEEE
    754 floating-point "double format" bit layout.

    - Bit 63 (the bit that is selected by the mask `0x8000000000000000`) represents
      the sign of the number.
    - Bits 62-52 (the bits that are selected by the mask `0x7ff0000000000000`)
      represent the exponent.
    - Bits 51-0 (the bits that are selected by the mask `0x000fffffffffffff`)
      represent the significand (sometimes called the mantissa) of the number.

        C1 3F F1 99 99 99 99 99 9A  -- Float(+1.1)

        C1 BF F1 99 99 99 99 99 9A  -- Float(-1.1)
    */

    public static partial class Packers
    {
        public static class Double
        {
            public static byte[] Pack(double content)
            {
                var output = new List<byte> {Markers.Floating};
                output.AddRange(PackStreamBitConverter.GetBytes(content));
                return output.ToArray();
            }

            public static double Unpack(byte[] content)
            {
                var markerlessArray = content[0] == Markers.Floating ? content.Skip(1).ToArray() : content;
                return PackStreamBitConverter.ToDouble(markerlessArray);
            }

            public static int GetExpectedSizeInBytes(byte[] content)
            {
                return 8;
            }

            public static int SizeOfMarkerInBytes(byte[] content)
            {
                return 1;
            }

            public static bool Is(byte[] content)
            {
                return content[0] == Markers.Floating;
            }
        }
    }
}