namespace PackStream.NET.Packers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using global::PackStream;

    /*
      Structures
        ----------

        Structures represent composite values and consist, beyond the marker, of a
        single byte signature followed by a sequence of fields, each an individual
        value. The size of a structure is measured as the number of fields, not the
        total packed byte size. The markers used to denote a structure are described
        in the table below:

          Marker | Size                                        | Maximum size
         ========|=============================================|=======================
          B0..BF | contained within low-order nibble of marker | 15 fields
          DC     | 8-bit big-endian unsigned integer           | 255 fields
          DD     | 16-bit big-endian unsigned integer          | 65 535 fields

        The signature byte is used to identify the type or class of the structure.
        Signature bytes may hold any value between 0 and +127. Bytes with the high bit
        set are reserved for future expansion.

        For structures containing fewer than 16 fields, the marker byte should
        contain the high-order nibble `1011` followed by a low-order nibble
        containing the size. The marker is immediately followed by the signature byte
        and the field values.

        For structures containing 16 fields or more, the marker 0xDC or 0xDD should
        be used, depending on scale. This marker is followed by the size, the signature
        byte and the actual fields, serialised in order. Examples follow below:

            B3 01 01 02 03  -- Struct(sig=0x01, fields=[1,2,3])

            DC 10 7F 01  02 03 04 05  06 07 08 09  00 01 02 03
            04 05 06  -- Struct(sig=0x7F, fields=[1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6]
     */


    public interface IPacker
    {
        byte[] Pack<T>(T content);
    }

    public interface IUnpacker
    {
        T Unpack<T>(byte[] content) where T : new();
        int GetSizeInBytes(byte[] content, bool includeMarker = true);
        bool IsUnpackable(byte[] content);
    }


    public static partial class Packers
    {
        public static class Struct
        {
            private static int GetNumberOfFields(byte[] bytes)
            {
                if (bytes[0] >= 0xB0 && bytes[0] <= 0xBF)
                    return bytes[0] - 0xB0;

                if (bytes[0] == 0xDC)
                    return bytes[1];

                if (bytes[0] == 0xDD)
                {
                    var markerSize = bytes.Skip(1).Take(2).ToArray();
                    return int.Parse(global::PackStream.NET.Packers.Packers.BitConverter.ToString(markerSize).Replace("-", ""), NumberStyles.HexNumber);
                }

                throw new ArgumentOutOfRangeException(nameof(bytes), bytes[0], "Unknown Marker");
            }

            public static bool IsStruct(byte[] content)
            {
                return (content[0] >= 0xB0 && content[0] <= 0xBF) || content[0] == 0xDC || content[0] == 0xDD;
            }

            public static global::PackStream.NET.Packers.Struct Unpack(byte[] content)
            {
                if (!IsStruct(content))
                    throw new ArgumentException("Content doesn't represent a Struct.", nameof(content));

                var output = new global::PackStream.NET.Packers.Struct(content);
                output.NumberOfFields = GetNumberOfFields(content);
                output.SignatureByte = GetSignatureByte(content, output.NumberOfFields);

                return output;
            }

            private static SignatureBytes GetSignatureByte(IEnumerable<byte> content, int numberOfFields)
            {
                var skip = 1;
                if (numberOfFields >= 16 && numberOfFields <= 255)
                    skip = 2;
                if (numberOfFields >= 256 && numberOfFields <= 65535)
                    skip = 3;

                var signatureBytes = content.Skip(skip).Take(1).Single();
                return (SignatureBytes) signatureBytes;
            }

            private static int GetSizeOfMarkerInBytes(int numberOfFields)
            {
                if (numberOfFields >= 16 && numberOfFields <= 255)
                    return 2;
                if (numberOfFields >= 256 && numberOfFields <= 65535)
                    return 3;
                return 1;
            }

            public static int GetNumberOfItems(byte[] content)
            {
                return GetNumberOfFields(content);
            }

            public static int GetExpectedSizeInBytes(byte[] content, bool includeMarkerSize = true)
            {
                var numberOfElements = GetNumberOfItems(content);
                var markerSize = GetSizeOfMarkerInBytes(numberOfElements) + 1;
                

                var length = 0;
                if (includeMarkerSize) length += markerSize;

                var bytesWithoutMarker = content.Skip(markerSize).ToArray();

                for (var i = 0; i < numberOfElements; i++)
                {
                    var itemLength = PackStream.GetLengthOfFirstItem(bytesWithoutMarker);
                    bytesWithoutMarker = bytesWithoutMarker.Skip(itemLength).ToArray();
                    length += itemLength;
                }

                return length;
            }
        }
    }
}