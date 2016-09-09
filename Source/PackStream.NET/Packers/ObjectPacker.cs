namespace PackStream.NET.Packers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using NET;

    /*
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
        */


    public static partial class Packers
    {
        public class Map
        {
            public static byte[] Pack<T>(T content)
            {
                if (content is IDictionary)
                {
                    return PackDictionary(content as IDictionary);
                }
                return PackObject(content);
            }

            private static byte[] PackObject<T>(T content)
            {
                var typeInfo = content.GetType().GetTypeInfo();
                var fields = typeInfo.DeclaredFields
                    .Where(f => f.IsPublic && !f.IsDefined( typeof (PackStreamIgnoreAttribute)))
                    .ToList();

                //TODO this will serialize it all!!!! -- only want public instance properties but doesn't get base properties
                var properties = content.GetType().GetRuntimeProperties()
                    .Where(prop => !prop.IsDefined(typeof (PackStreamIgnoreAttribute))).ToList();

                var dictionary = fields.ToDictionary(field => field.Name, field => field.GetValue(content));
                foreach (var prop in properties)
                    dictionary.Add(prop.Name, prop.GetValue(content));

                return PackDictionary(dictionary);
            }

            private static IEnumerable<byte> GetMarker(long fields)
            {
                if (fields <= 15)
                    return new[] {(byte) (0xA0 + fields)};

                var output = new List<byte>();

                if (fields <= byte.MaxValue)
                    output.Add(0xD8);

                if (fields <= ushort.MaxValue)
                    output.Add(0xD9);

                if (fields <= uint.MaxValue)
                    output.Add(0xDA);

                if (fields > uint.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(fields), "Too many fields defined!");

                output.AddRange(PackStream.GetLength(fields));
                return output.ToArray();
            }

            private static byte[] PackDictionary(IDictionary content)
            {
                var bytes = new List<byte>();
                bytes.AddRange(GetMarker(content.Count));

                if (content.Count == 0)
                    return bytes.ToArray();

                var arguments = content.GetType().GetTypeInfo().GenericTypeArguments;
                var keyType = arguments[0];
                var valueType = arguments[1];

                

                foreach (var item in content.Keys)
                {
                    var keyBytes = PackStream.Pack(Convert.ChangeType(item, keyType));
                    bytes.AddRange(keyBytes);
                    
                    try
                    {
                        var x = (valueType == typeof(object)) ? (object) content[item] : Convert.ChangeType(content[item], valueType);
                        bytes.AddRange(PackStream.Pack(x));
                    }
                    catch (Exception ex)
                    {
                        int i = 0;
                    }
                }

                return bytes.ToArray();
            }


            private static IDictionary UnpackDictionary<TKey, TValue>(byte[] content)
            {
                int numberOfPairs;
                var markerLess = RemoveMarker(content, out numberOfPairs);

                var output = new Dictionary<TKey, TValue>();
                if (numberOfPairs == 0)
                    return output;

                var packed = PackStream.GetPackedEntities(markerLess);

                for (var i = 0; i < packed.Length; i += 2)
                {
                    var key = PackStream.Unpack(packed[i]);
                    var value = PackStream.Unpack(packed[i + 1]);
                    Type genericType;
                    if (List.IsEnumerable(typeof (TValue), out genericType))
                    {
                        
                        var method = typeof(List).GetTypeInfo().GetDeclaredMethod("Unpack");
                        var genericMethod = method.MakeGenericMethod(genericType);
                        var list = genericMethod.Invoke(null, new object[] {packed[i + 1].Original}) as IEnumerable;
                        value = (TValue) list;
                    }

                    output.Add(key, value);
                }

                return output;
            }

            private static byte[] RemoveMarker(byte[] content, out int numberOfFields)
            {
                if (content[0] >= 0xA0 && content[0] <= 0xAF)
                {
                    numberOfFields = content[0] - 0xA0;
                    return content.Skip(1).ToArray();
                }
                if (content[0] == 0xD8)
                {
                    numberOfFields = content[1];
                    return content.Skip(2).ToArray();
                }
                if (content[0] == 0xD9)
                {
                    var markerSize = content.Skip(1).Take(2).ToArray();
                    numberOfFields = int.Parse(BitConverter.ToString(markerSize).Replace("-", ""), NumberStyles.HexNumber);
                    return content.Skip(3).ToArray();
                }
                if (content[0] == 0xDA)
                {
                    var markerSize = content.Skip(1).Take(4).ToArray();
                    numberOfFields = int.Parse(BitConverter.ToString(markerSize).Replace("-", ""), NumberStyles.HexNumber);
                    return content.Skip(5).ToArray();
                }
                throw new ArgumentOutOfRangeException(nameof(content), content[0], "Unknown marker.");
            }

            public static dynamic UnpackRecord(byte[] content, IEnumerable<string> fields)
            {
                //string to val
                var elements = List.Unpack<dynamic>(content).ToList();
                var fieldsList = fields.ToList();
                if(fieldsList.Count != elements.Count)
                    throw new ArgumentException($"Mismatched field count ({fieldsList.Count}) to content count ({elements.Count})", nameof(fields));

                var expando = new ExpandoObject();
                var dictionary = (IDictionary<string, object>) expando;

                for(var i = 0; i<elements.Count; i++)
                {
                    dictionary.Add(fieldsList[i], elements[i]);
                }

                return expando;
            }

            public static T Unpack<T>(byte[] content)
                where T : new()
            {
                var tType = typeof (T);
                var isDict = tType.GetTypeInfo().IsGenericType && tType.GetGenericTypeDefinition() == typeof (Dictionary<,>);
                if (isDict)
                {
                    var key = tType.GenericTypeArguments[0];
                    var value = tType.GenericTypeArguments[1];

                    var method = typeof (Map).GetTypeInfo().GetDeclaredMethod("UnpackDictionary");
                    var generic = method.MakeGenericMethod(key, value);
                    var dict = generic.Invoke(null, new object[] {content}) as IDictionary;

                    return (T) dict;
                }

                var defaultDictionary = (IDictionary<string, dynamic>) UnpackDictionary<string, dynamic>(content);
                return Change<T>(defaultDictionary);
            }

            public static bool Is(byte[] content)
            {
                return (content[0] >= 0xA0 && content[0] <= 0xAF) || (content[0] >= 0xD8 && content[0] <= 0xDA);
            }

            public static int GetExpectedNumberOfFields(byte[] content)
            {
                if (content[0] >= 0xA0 && content[0] <= 0xAF)
                {
                    return content[0] - 0xA0;
                }
                if (content[0] == 0xD8)
                {
                    return content[1];
                }
                if (content[0] == 0xD9)
                {
                    var markerSize = content.Skip(1).Take(2).ToArray();
                    return int.Parse(BitConverter.ToString(markerSize).Replace("-", ""), NumberStyles.HexNumber);
                }
                if (content[0] == 0xDA)
                {
                    var markerSize = content.Skip(1).Take(4).ToArray();
                    return int.Parse(BitConverter.ToString(markerSize).Replace("-", ""), NumberStyles.HexNumber);
                }
                throw new ArgumentOutOfRangeException(nameof(content), content[0], "Unknown Marker");
            }

            public static int SizeOfMarkerInBytes(byte[] content)
            {
                if (content[0] >= 0xA0 && content[0] <= 0xAF)
                    return 1;
                if (content[0] == 0xD8)
                    return 2;
                if (content[0] == 0xD9)
                    return 3;
                if (content[0] == 0xDA)
                    return 5;

                throw new ArgumentOutOfRangeException(nameof(content), content[0], "Unknown Marker");
            }

            public static T Change<T>(IDictionary<string, dynamic> toConvert)
                where T : new()
            {
                var obj = new T();
                var type = obj.GetType();

                //TODO: Getting static and private properties!!!
                var properties = type.GetRuntimeProperties()
                    .Where(prop => !prop.IsDefined(typeof (PackStreamIgnoreAttribute)));
                foreach (var property in properties)
                    if (toConvert.ContainsKey(property.Name))
                        property.SetValue(obj, (object) toConvert[property.Name]);

                var fields = type.GetTypeInfo().DeclaredFields.Where(field => field.IsPublic && !field.IsDefined(typeof (PackStreamIgnoreAttribute)));
                foreach (var field in fields)
                    if (toConvert.ContainsKey(field.Name))
                        field.SetValue(obj, (object) toConvert[field.Name]);

                return obj;
            }

            public static int GetExpectedSizeInBytes(byte[] content, bool includeMarkerSize = true)
            {
                var numberOfElements = GetExpectedNumberOfFields(content);
                var markerSize = SizeOfMarkerInBytes(content);

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