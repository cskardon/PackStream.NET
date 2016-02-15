namespace PackStream.NET.Packers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using global::PackStream.NET.Extensions;

    /*
    
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
        */

    public static partial class Packers
    {
        public static class List
        {
            private static readonly Type[] AllowedTypes = {typeof (List<>),  typeof (IList)};

            public static bool IsUnpackable(byte[] content)
            {
                return (content[0] >= 0x90 && content[0] <= 0x9F) || (content[0] >= 0xD4 && content[0] <= 0xD6);
            }

            public static bool IsPackable(Type type)
            {
                return AllowedTypes.Any(t => t.Name == type.Name);
            }

            public static byte[] PackwithType(dynamic content, Type genericType)
            {
                var method = typeof (List).GetTypeInfo().GetDeclaredMethod("Pack");
                var genericMethod = method.MakeGenericMethod(genericType);
                var list = genericMethod.Invoke(null, new object[] {content});
                return (byte[]) list;
            }

            public static bool IsEnumerable(Type type, out Type genericParameter)
            {
//                throw new NotImplementedException();
                genericParameter = null;
                if (type == typeof (string))
                    return false;

                var typeInfo = type.GetTypeInfo();
                var isEnumerable = typeInfo.ImplementedInterfaces.Contains(typeof(IEnumerable));
                if (!isEnumerable)
                    return false;

                var genericArgs = typeInfo.GenericTypeArguments;
                if (!genericArgs.IsNullOrEmpty())
                    genericParameter = genericArgs[0];

                return true;
            }

            

            public static byte[] Pack<T>(IEnumerable<T> content)
            {
                var output = new List<byte>();
                var contentAsList = content.ToList();
                output.AddRange(GetMarker(contentAsList.Count));

                for (var i = 0; i < contentAsList.Count; i++)
                    output.AddRange(PackStream.Pack(contentAsList[i]));

                return output.ToArray();
            }

            private static int GetNumberOfElements(byte[] content)
            {
                if (content[0] >= 0x90 && content[0] <= 0x9F)
                    return content[0] - 0x90;

                if (content[0] == 0xD4)
                    return content[1];

                if (content[0] == 0xD5)
                    return int.Parse(global::PackStream.NET.Packers.Packers.BitConverter.ToString(content.Skip(1).Take(2).ToArray()).Replace("-", ""), NumberStyles.HexNumber);

                if (content[0] == 0xD6)
                    return int.Parse(global::PackStream.NET.Packers.Packers.BitConverter.ToString(content.Skip(1).Take(4).ToArray()).Replace("-", ""), NumberStyles.HexNumber);

                throw new ArgumentException("Unknown marker", nameof(content));
            }
            internal static byte[] GetMarker(long itemsInList)
            {
                if (itemsInList <= 15)
                    return new[] { (byte)(0x90 + itemsInList) };

                var output = new List<byte>();
                if (itemsInList <= 0xFF)
                    output.Add(0xD4);

                else if (itemsInList <= 0xFFFF)
                    output.Add(0xD5);

                else if (itemsInList <= uint.MaxValue)
                    output.Add(0xD6);

                if (itemsInList > uint.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(itemsInList), itemsInList, "Too many items in the list!");

                output.AddRange(PackStream.ConvertSizeToBytes(itemsInList));
                return output.ToArray();
            }
            private static int GetSizeOfMarkerInBytes(long numberOfElements)
            {
                if (numberOfElements <= 15)
                    return 1;
                if (numberOfElements <= 255)
                    return 2;
                if (numberOfElements <= 65535)
                    return 3;
                if (numberOfElements <= 4294967295)
                    return 5;
                throw new ArgumentOutOfRangeException(nameof(numberOfElements), numberOfElements, "Too many elements!");
            }

            public static IEnumerable<T> Unpack<T>(byte[] content)
            {
                if (!IsUnpackable(content))
                    throw new ArgumentException("Not a list.", nameof(content));

                var numberOfElements = GetNumberOfElements(content);
                var bytesToSkip = GetSizeOfMarkerInBytes(numberOfElements);

                var unpacked = PackStream.Unpack(content.Skip(bytesToSkip).ToArray());
                return unpacked.Cast<T>();
            }

            public static int GetLengthInBytes(byte[] bytes, bool includeMarkerSize)
            {
                var numberOfElements = GetNumberOfElements(bytes);
                var markerSize = GetSizeOfMarkerInBytes(numberOfElements);

                var length = 0;
                if (includeMarkerSize) length += markerSize;

                var bytesWithoutMarker = bytes.Skip(markerSize).ToArray();

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