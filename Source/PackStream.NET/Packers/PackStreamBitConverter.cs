namespace PackStream.NET.Packers
{
    using System;
    using System.Linq;
    using System.Text;

    public static class PackStreamBitConverter
    {
        private static byte[] ToBigEndian(byte[] bytes)
        {
            return BitConverter.IsLittleEndian ? bytes.Reverse().ToArray() : bytes;
        }

        private static byte[] ToLittleEndian(byte[] bytes)
        {
            return BitConverter.IsLittleEndian ? bytes.Reverse().ToArray() : bytes;
        }

        /// <summary>Converts a <see cref="byte"/> into a <see cref="byte"/> array.</summary>
        /// <param name="value">The <see cref="byte"/> to convert.</param>
        /// <returns>The specified <see cref="byte"/> value as an array of <see cref="byte"/>.</returns>
        public static byte[] GetBytes(byte value)
        {
            return new [] { value };
        }

        /// <summary>Converts a <see cref="short"/> to a <see cref="byte"/> array.</summary>
        /// <param name="value">The <see cref="short"/> to convert.</param>
        /// <returns>The specified <see cref="short"/> value as an array of <see cref="byte"/>.</returns>
        public static byte[] GetBytes(short value)
        {
            var bytes = BitConverter.GetBytes(value);
            return ToBigEndian(bytes);
        }

        //// <summary>Converts a <see cref="ushort"/> to a <see cref="byte"/> array.</summary>
        /// <param name="value">The <see cref="ushort"/> to convert.</param>
        /// <returns>The specified <see cref="ushort"/> value as an array of <see cref="byte"/>.</returns>
        public static byte[] GetBytes(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            return ToBigEndian(bytes);
        }

        /// <summary>Converts a <see cref="int"/> to a <see cref="byte"/> array.</summary>
        /// <param name="value">The <see cref="int"/> to convert.</param>
        /// <returns>The specified <see cref="int"/> value as an array of <see cref="byte"/>.</returns>
        public static byte[] GetBytes(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            return ToBigEndian(bytes);
        }

        /// <summary>Converts a <see cref="uint"/> to a <see cref="byte"/> array.</summary>
        /// <param name="value">The <see cref="uint"/> to convert.</param>
        /// <returns>The specified <see cref="uint"/> value as an array of <see cref="byte"/>.</returns>
        public static byte[] GetBytes(uint value)
        {
            var bytes = BitConverter.GetBytes(value);

            return ToBigEndian(bytes);
        }

        /// <summary>Converts a <see cref="long"/> to a <see cref="byte"/> array.</summary>
        /// <param name="value">The <see cref="long"/> to convert.</param>
        /// <returns>The specified <see cref="long"/> value as an array of <see cref="byte"/>.</returns>
        public static byte[] GetBytes(long value)
        {
            var bytes = BitConverter.GetBytes(value);

            return ToBigEndian(bytes);
        }

        /// <summary>Converts a <see cref="double"/> to a <see cref="byte"/> array.</summary>
        /// <param name="value">The <see cref="double"/> to convert.</param>
        /// <returns>The specified <see cref="double"/> value as an array of <see cref="byte"/>.</returns>
        public static byte[] GetBytes(double value)
        {
            var bytes = BitConverter.GetBytes(value);

            return ToBigEndian(bytes);
        }

        /// <summary>Converts a <see cref="string"/> to a <see cref="byte"/> array.</summary>
        /// <param name="value">The <see cref="string"/> to convert.</param>
        /// <returns>The specified <see cref="string"/> value as an array of <see cref="byte"/>.</returns>
        public static byte[] GetBytes(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        /// <summary>Converts an array of <see cref="byte"/> to a <see cref="short"/>.</summary>
        /// <param name="bytes">The array of <see cref="byte"/> to convert.</param>
        /// <returns>A <see cref="short"/> converted from the array of <see cref="byte"/>.</returns>
        public static short ToInt16(byte[] bytes)
        {
            bytes = ToLittleEndian(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>Converts an array of <see cref="byte"/> to an <see cref="int"/>.</summary>
        /// <param name="bytes">The array of <see cref="byte"/> to convert.</param>
        /// <returns>An <see cref="int"/> converted from the array of <see cref="byte"/>.</returns>
        public static int ToInt32(byte[] bytes)
        {
            bytes = ToLittleEndian(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>Converts an array of <see cref="byte"/> to a <see cref="long"/>.</summary>
        /// <param name="bytes">The array of <see cref="byte"/> to convert.</param>
        /// <returns>A <see cref="long"/> converted from the array of <see cref="byte"/>.</returns>
        public static long ToInt64(byte[] bytes)
        {
            bytes = ToLittleEndian(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>Converts an array of <see cref="byte"/> to a <see cref="double"/>.</summary>
        /// <param name="bytes">The array of <see cref="byte"/> to convert.</param>
        /// <returns>A <see cref="double"/> converted from the array of <see cref="byte"/>.</returns>
        public static double ToDouble(byte[] bytes)
        {
            bytes = ToLittleEndian(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>Converts an array of <see cref="byte"/> to a <see cref="string"/>.</summary>
        /// <param name="bytes">The array of <see cref="byte"/> to convert.</param>
        /// <returns>A <see cref="string"/> converted from the array of <see cref="byte"/>.</returns>
        public static string ToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
    }
}