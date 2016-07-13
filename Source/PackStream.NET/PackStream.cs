namespace PackStream.NET
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using global::PackStream.NET.Packers;

    /*
        PackStream
        ==========

        This module contains a full implementation of PackStream: the serialisation
        format built specifically for NDP. The PackStream design is based heavily on
        MessagePack but the implementation completely separate.

        Note that PackStream uses big-endian order exclusively and the byte values
        described below are all represented as hexadecimal.


        Data Types
        ----------

        PackStream allows serialisation of most common data types as well as a generic
        'structure' type, used for composite values. The full list of types are:

        - Null (absence of value)
        - Boolean (true or false)
        - Integer (signed 64-bit integer)
        - Float (64-bit floating point number)
        - Text (UTF-8 encoded text data)
        - List (ordered collection of values)
        - Map (keyed collection of values)
        - Structure (composite set of values with a type signature)

        Neither unsigned integers nor byte arrays are supported but may be added in a
        future version of the format. Note also that 32-bit floating point numbers
        are not supported. This is a deliberate decision and these are unlikely to be
        added in a future version.


        Markers
        -------

        Every serialised value begins with a marker byte. The marker contains
        information on data type as well as direct or indirect size information for
        those types that require it. How that size information is encoded varies by
        marker type.

        Some values, such as boolean true, can be encoded within a single marker byte.
        Many small integers (specifically between -16 and +127) are also encoded
        within a single byte.

        A number of marker bytes are reserved for future expansion of the format
        itself. These bytes should not be used, and encountering them in an incoming
        stream should treated as an error.


        Sized Values
        ------------

        Some value types require variable length representations and, as such, have
        their size explicitly encoded. These values generally begin with a single
        marker byte, followed by a size, followed by the data content itself. Here,
        the marker denotes both type and scale and therefore determines the number of
        bytes used to represent the size of the data. The size itself is either an
        8-bit, 16-bit or 32-bit unsigned integer.

        The diagram below illustrates the general layout for a sized value, here with a
        16-bit size:

          Marker Size          Content
            <>  <--->  <--------------------->
            XX  XX XX  XX XX XX XX .. .. .. XX

        Null
        ----

        Null is always encoded using the single marker byte 0xC0.

            C0  -- Null


        Boolean
        -------

        Boolean values are encoded within a single marker byte, using 0xC3 to denote
        true and 0xC2 to denote false.

            C3  -- True

            C2  -- False


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


        Text
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


        Lists
        -----

        Lists are heterogeneous sequences of values and therefore permit a mixture of
        types within the same list. The size of a list denotes the number of items
        within that list, not the total packed byte size. The markers used to denote
        a list are described in the table below:

          Marker | Size                                        | Maximum size
         ========|=============================================|=====================
          90..9F | contained within low-order nibble of marker | 15 bytes
          D4     | 8-bit big-endian unsigned integer           | 255 items
          D5     | 16-bit big-endian unsigned integer          | 65 535 items
          D6     | 32-bit big-endian unsigned integer          | 4 294 967 295 items

        For lists containing fewer than 16 items, including empty lists, the marker
        byte should contain the high-order nibble `1001` followed by a low-order
        nibble containing the size. The items within the list are then serialised in
        order immediately after the marker.

        For lists containing 16 items or more, the marker 0xD4, 0xD5 or 0xD6 should be
        used, depending on scale. This marker is followed by the size and list items,
        serialized in order. Examples follow below:

            90  -- []

            93 01 02 03 -- [1,2,3]

            D4 14 01 02  03 04 05 06  07 08 09 00  01 02 03 04
            05 06 07 08  09 00  -- [1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0]


        Maps
        ----

        Maps are sized sequences of pairs of values and permit a mixture of types
        within the same map. The size of a map denotes the number of pairs within
        that map, not the total packed byte size. The markers used to denote a map
        are described in the table below:

          Marker | Size                                        | Maximum size
         ========|=============================================|=======================
          A0..AF | contained within low-order nibble of marker | 15 entries
          D8     | 8-bit big-endian unsigned integer           | 255 entries
          D9     | 16-bit big-endian unsigned integer          | 65 535 entries
          DA     | 32-bit big-endian unsigned integer          | 4 294 967 295 entries

        For maps containing fewer than 16 key-value pairs, including empty maps,
        the marker byte should contain the high-order nibble `1010` followed by a
        low-order nibble containing the size. The items within the map are then
        serialised in key-value-key-value order immediately after the marker. Keys
        are typically text values.

        For maps containing 16 pairs or more, the marker 0xD8, 0xD9 or 0xDA should be
        used, depending on scale. This marker is followed by the size and map
        entries, serialised in key-value-key-value order. Examples follow below:

            A0  -- {}

            A1 81 61 01  -- {a:1}

            D8 10 81 61  01 81 62 01  81 63 03 81  64 04 81 65
            05 81 66 06  81 67 07 81  68 08 81 69  09 81 6A 00
            81 6B 01 81  6C 02 81 6D  03 81 6E 04  81 6F 05 81
            70 06  -- {a:1,b:1,c:3,d:4,e:5,f:6,g:7,h:8,i:9,j:0,k:1,l:2,m:3,n:4,o:5,p:6}


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


    public enum PackType
    {
        Unknown,
        Map,
        Text,
        Integer,
        Float,
        List,
        Boolean,
        Structure,
        Null
    }

    public static class PackStream
    {
        public static void AddPacker(IPacker packer)
        {
            ActualCustomPackers.Add(packer);
        }

        private static readonly List<IPacker> ActualCustomPackers = new List<IPacker>();
        public static IReadOnlyCollection<IPacker> CustomPackers => ActualCustomPackers;

        public static T Unpack<T>(byte[] content) where T : new()
        {
            var underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            var customPacker = CustomPackers.FirstOrDefault(p => p.CanPack(underlyingType));
            if (customPacker != null)
                return (T) customPacker.Unpack(content);

            var packedEntities = GetPackedEntities(content);
            if (packedEntities.Length != 1)
                throw new ArgumentException(packedEntities.Length > 1 ? "Too many entities" : "No entities supplied", nameof(content));

            var entity = packedEntities.First();
            switch (entity.PackType)
            {
                case PackType.Map:
                    return Packers.Packers.Map.Unpack<T>(entity.Original);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static IList<dynamic> Unpack(byte[] content)
        {
            var packedEntities = GetPackedEntities(content);
            return packedEntities.Select(packedEntity => Unpack(packedEntity)).ToList();
        }

        public static dynamic Unpack(Packed content)
        {
            switch (content.PackType)
            {
                case PackType.Map:
                    return Packers.Packers.Map.Unpack<dynamic>(content.Original);
                case PackType.Text:
                    return Packers.Packers.Text.Unpack(content.Original);
                case PackType.Integer:
                    return Packers.Packers.Int.Unpack(content.Original);
                case PackType.Float:
                    return Packers.Packers.Double.Unpack(content.Original);
                case PackType.List:
                    return Packers.Packers.List.Unpack<dynamic>(content.Original);
                case PackType.Boolean:
                    return Packers.Packers.Bool.Unpack(content.Original);
                case PackType.Null:
                    return null;
                case PackType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Packed[] GetPackedEntities(byte[] content)
        {
            var editable = content;
            int size, numberOfItems;
            var entityType = GetTypeAndSize(content, out size, out numberOfItems);
            var output = new List<Packed>();
            while (entityType != PackType.Unknown)
            {
                output.Add(new Packed(editable.Take(size).ToArray()) {PackType = entityType, NumberOfItems = numberOfItems});
                editable = editable.Skip(size).ToArray();

                entityType = GetTypeAndSize(editable, out size, out numberOfItems);
            }
            return output.ToArray();
        }

        public static PackType GetTypeAndSize(byte[] content, out int size, out int numberOfItems)
        {
            var type = PackType.Unknown;
            size = -1;
            numberOfItems = -1;
            if (content == null || content.Length == 0)
                return type;

            if (Packers.Packers.Double.Is(content))
            {
                type = PackType.Float;
                size = Packers.Packers.Double.GetExpectedSizeInBytes(content);
            }
            if (Packers.Packers.Null.Is(content))
            {
                type = PackType.Null;
                size = 1;
            }
            if (Packers.Packers.Text.Is(content))
            {
                type = PackType.Text;
                size = Packers.Packers.Text.GetExpectedSize(content) + Packers.Packers.Text.SizeOfMarkerInBytes(content);
            }
            if (Packers.Packers.Bool.Is(content))
            {
                type = PackType.Boolean;
                size = 1;
            }
            if (Packers.Packers.Int.Is(content))
            {
                type = PackType.Integer;
                size = Packers.Packers.Int.GetExpectedSizeInBytes(content) + Packers.Packers.Int.SizeOfMarkerInBytes(content);
            }

            if (Packers.Packers.Map.Is(content))
            {
                type = PackType.Map;
                //Not the size, but number of fields.
                //in this case (and others??) size = content up to 0x00 0x00??
                size = content.Length;
            }
            if (Packers.Packers.List.IsUnpackable(content))
            {
                type = PackType.List;
                size = content.Length;
            }
            if (Packers.Packers.Struct.IsStruct(content))
            {
                type = PackType.Structure;
                size = content.Length;
                numberOfItems = Packers.Packers.Struct.GetNumberOfItems(content);
            }
            return type;
        }

        public static byte[] GetLength(long length)
        {
            if (length <= 0xFF)
                return new[] {(byte) length};

            if (length > 0xFFFFFF)
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length given is too big.");

            return ConvertSizeToBytes(length);
        }

        public static byte[] Pack<T>(T toPack, bool addZeroEnding = false)
        {
            if (toPack == null)
                return new[] {Markers.Null};

            var underlyingType = Nullable.GetUnderlyingType(toPack.GetType()) ?? toPack.GetType();

            var customPacker = CustomPackers.FirstOrDefault(p => p.CanPack(underlyingType));
            if (customPacker != null)
                return customPacker.Pack(toPack);

            byte[] output;
            if (underlyingType == typeof (bool))
                output = Packers.Packers.Bool.Pack(Convert.ToBoolean(toPack));

            else if (underlyingType == typeof (string))
                output = Packers.Packers.Text.Pack(toPack as string);

            else if (underlyingType == typeof (long) || underlyingType == typeof (int) || underlyingType == typeof (short) || underlyingType == typeof (sbyte))
                output = Packers.Packers.Int.Pack(Convert.ToInt64(toPack));

            else if (underlyingType == typeof (float) || underlyingType == typeof (double) || underlyingType == typeof (decimal))
                output = Packers.Packers.Double.Pack(Convert.ToDouble(toPack));

            else if (Packers.Packers.List.IsPackable(underlyingType))
            {
                Type genericParameter;
                Packers.Packers.List.IsEnumerable(underlyingType, out genericParameter);
                output = Packers.Packers.List.PackwithType(toPack, genericParameter);
            }
            else output = Packers.Packers.Map.Pack(toPack);

            if (addZeroEnding)
            {
                var newOutput = new List<byte>(output);
                newOutput.AddRange(new byte[] {0x00, 0x00});
                output = newOutput.ToArray();
            }
            return output;
        }


        public static byte[] ConvertSizeToBytes(long length, int? size = null)
        {
            var hexValue = length.ToString("X2");
            if (hexValue.Length % 2 != 0)
                hexValue = hexValue.PadLeft(hexValue.Length + 1, '0');

            var output = new List<byte>();

            for (var i = 0; i < hexValue.Length; i += 2)
                output.Add((byte) int.Parse(string.Concat(hexValue[i], hexValue[i + 1]), NumberStyles.HexNumber));

            if (size != null && size > 0)
            {
                var toAdd = output.Count % (size * 2);
                if (toAdd > 0)
                    for (var i = 0; i < toAdd; i++)
                        output.Insert(0, 0x0);
            }

            return output.ToArray();
        }

        public static dynamic UnpackRecord(byte[] value, IEnumerable<string> fields)
        {
            return Packers.Packers.Map.UnpackRecord(value, fields);
        }

        public static int GetLengthOfFirstItem(byte[] content, bool includeMarker = true)
        {
            var packed = GetPackedEntities(content);
            var first = packed.FirstOrDefault();
            if (first == null)
                return 0;

            var markerSizeInBytes = 0;
            var contentSizeInBytes = 0;
            switch (first.PackType)
            {
                case PackType.Map:
                    markerSizeInBytes = Packers.Packers.Map.SizeOfMarkerInBytes(content);
                    contentSizeInBytes = Packers.Packers.Map.GetExpectedSizeInBytes(content);
                    break;
                case PackType.Text:
                    markerSizeInBytes = Packers.Packers.Text.SizeOfMarkerInBytes(content);
                    contentSizeInBytes = Packers.Packers.Text.GetExpectedSize(content);
                    break;
                case PackType.Integer:
                    markerSizeInBytes = Packers.Packers.Int.SizeOfMarkerInBytes(content);
                    contentSizeInBytes = Packers.Packers.Int.GetExpectedSizeInBytes(content);
                    break;
                case PackType.Float:
                    markerSizeInBytes = Packers.Packers.Double.SizeOfMarkerInBytes(content);
                    contentSizeInBytes = Packers.Packers.Double.GetExpectedSizeInBytes(content);
                    break;
                case PackType.List:
                    contentSizeInBytes = Packers.Packers.List.GetLengthInBytes(content, true);
                    break;
                case PackType.Boolean:
                case PackType.Null:
                    contentSizeInBytes = 1;
                    break;
                case PackType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return markerSizeInBytes + contentSizeInBytes;
        }
    }
}