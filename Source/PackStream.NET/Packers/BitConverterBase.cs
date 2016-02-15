namespace PackStream.NET.Packers
{
    using System;
    using System.Linq;
    using System.Text;

    //This code from https://github.com/IngvarKofoed/neo4j-ndp-csharpdriver he wrote it better than I did :)

    /// <summary>
    ///     Converts .NET types to an array bytes and an array of bytes to .NET types.
    /// </summary>
    public interface IBitConverter
    {
        /// <summary>
        ///     Converts a byte to bytes.
        /// </summary>
        /// <param name="value">The byte value to convert.</param>
        /// <returns>The specified byte value as an array of bytes.</returns>
        byte[] GetBytes(byte value);

        /// <summary>
        ///     Converts a shot (Int16) to bytes.
        /// </summary>
        /// <param name="value">The short (Int16) value to convert.</param>
        /// <returns>The specified short (Int16) value as an array of bytes.</returns>
        byte[] GetBytes(short value);

        /// <summary>
        ///     Converts a shot (UInt16) to bytes.
        /// </summary>
        /// <param name="value">The short (UInt16) value to convert.</param>
        /// <returns>The specified short (UInt16) value as an array of bytes.</returns>
        byte[] GetBytes(ushort value);

        /// <summary>
        ///     Converts an int (Int32) to bytes.
        /// </summary>
        /// <param name="value">The int (Int32) value to convert.</param>
        /// <returns>The specified int (Int32) value as an array of bytes.</returns>
        byte[] GetBytes(int value);

        /// <summary>
        ///     Converts an uint (UInt32) to bytes.
        /// </summary>
        /// <param name="value">The uint (UInt32) value to convert.</param>
        /// <returns>The specified uint (UInt32) value as an array of bytes.</returns>
        byte[] GetBytes(uint value);

        /// <summary>
        ///     Converts an int (Int64) to bytes.
        /// </summary>
        /// <param name="value">The int (Int64) value to convert.</param>
        /// <returns>The specified int (Int64) value as an array of bytes.</returns>
        byte[] GetBytes(long value);

        /// <summary>
        ///     Converts an int (double) to bytes.
        /// </summary>
        /// <param name="value">The int (double) value to convert.</param>
        /// <returns>The specified int (double) value as an array of bytes.</returns>
        byte[] GetBytes(double value);

        /// <summary>
        ///     Converts an string to bytes.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The specified string value as an array of bytes.</returns>
        byte[] GetBytes(string value);

        /// <summary>
        ///     Converts an byte array to a short.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A short converted from the byte array.</returns>
        short ToInt16(byte[] bytes);

        /// <summary>
        ///     Converts an byte array to a int (Int32).
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A int (Int32) converted from the byte array.</returns>
        int ToInt32(byte[] bytes);

        /// <summary>
        ///     Converts an byte array to a int (Int64).
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A int (Int64) converted from the byte array.</returns>
        long ToInt64(byte[] bytes);

        /// <summary>
        ///     Converts an byte array to a int (double).
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A int (double) converted from the byte array.</returns>
        double ToDouble(byte[] bytes);

        /// <summary>
        ///     Converts an byte array of a UTF8 encoded string to a string
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A string converted from the byte array</returns>
        string ToString(byte[] bytes);
    }

    /// <summary>
    ///     Converts from/to big endian (target) to platform endian.
    /// </summary>
    public class BigEndianTargetBitConverter : BitConverterBase
    {
        /// <summary>
        ///     Converts the bytes to big endian.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>The bytes converted to big endian.</returns>
        protected override byte[] ToTargetEndian(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                return bytes.Reverse().ToArray();
            }
            return bytes;
        }

        /// <summary>
        ///     Converts the bytes to the platform endian type.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>The bytes converted to the platform endian type.</returns>
        protected override byte[] ToPlatformEndian(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                return bytes.Reverse().ToArray();
            }
            return bytes;
        }
    }

    /// <summary>
    ///     Base class for <see cref="IBitConverter" /> that handles
    ///     little vs big endian.
    /// </summary>
    public abstract class BitConverterBase : IBitConverter
    {
        /// <summary>
        ///     Converts a byte to bytes.
        /// </summary>
        /// <param name="value">The byte value to convert.</param>
        /// <returns>The specified byte value as an array of bytes.</returns>
        public byte[] GetBytes(byte value)
        {
            byte[] bytes = {value};
            return bytes;
        }

        /// <summary>
        ///     Converts a shot (Int16) to bytes.
        /// </summary>
        /// <param name="value">The short (Int16) value to convert.</param>
        /// <returns>The specified short (Int16) value as an array of bytes.</returns>
        public byte[] GetBytes(short value)
        {
            var bytes = BitConverter.GetBytes(value);

            return ToTargetEndian(bytes);
        }

        /// <summary>
        ///     Converts a shot (UInt16) to bytes.
        /// </summary>
        /// <param name="value">The short (UInt16) value to convert.</param>
        /// <returns>The specified short (UInt16) value as an array of bytes.</returns>
        public byte[] GetBytes(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);

            return ToTargetEndian(bytes);
        }

        /// <summary>
        ///     Converts an int (Int32) to bytes.
        /// </summary>
        /// <param name="value">The int (Int32) value to convert.</param>
        /// <returns>The specified int (Int32) value as an array of bytes.</returns>
        public byte[] GetBytes(int value)
        {
            var bytes = BitConverter.GetBytes(value);

            return ToTargetEndian(bytes);
        }

        /// <summary>
        ///     Converts an uint (UInt32) to bytes.
        /// </summary>
        /// <param name="value">The uint (UInt32) value to convert.</param>
        /// <returns>The specified uint (UInt32) value as an array of bytes.</returns>
        public byte[] GetBytes(uint value)
        {
            var bytes = BitConverter.GetBytes(value);

            return ToTargetEndian(bytes);
        }

        /// <summary>
        ///     Converts an int (Int64) to bytes.
        /// </summary>
        /// <param name="value">The int (Int64) value to convert.</param>
        /// <returns>The specified int (Int64) value as an array of bytes.</returns>
        public byte[] GetBytes(long value)
        {
            var bytes = BitConverter.GetBytes(value);

            return ToTargetEndian(bytes);
        }

        /// <summary>
        ///     Converts an int (double) to bytes.
        /// </summary>
        /// <param name="value">The int (double) value to convert.</param>
        /// <returns>The specified int (double) value as an array of bytes.</returns>
        public byte[] GetBytes(double value)
        {
            var bytes = BitConverter.GetBytes(value);

            return ToTargetEndian(bytes);
        }

        /// <summary>
        ///     Converts an string to bytes.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The specified string value as an array of bytes.</returns>
        public byte[] GetBytes(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        /// <summary>
        ///     Converts an byte array to a short.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A short converted from the byte array.</returns>
        public short ToInt16(byte[] bytes)
        {
            bytes = ToPlatformEndian(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        ///     Converts an byte array to a int (Int32).
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A int (Int32) converted from the byte array.</returns>
        public int ToInt32(byte[] bytes)
        {
            bytes = ToPlatformEndian(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        ///     Converts an byte array to a int (Int64).
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A int (Int64) converted from the byte array.</returns>
        public long ToInt64(byte[] bytes)
        {
            bytes = ToPlatformEndian(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        ///     Converts an byte array to a int (double).
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A int (double) converted from the byte array.</returns>
        public double ToDouble(byte[] bytes)
        {
            bytes = ToPlatformEndian(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        ///     Converts an byte array of a UTF8 encoded string to a string
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A string converted from the byte array</returns>
        public string ToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes, 0 , bytes.Length);
        }

        /// <summary>
        ///     Converts the bytes to the target endian type.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>The bytes converted to the targert endian type.</returns>
        protected abstract byte[] ToTargetEndian(byte[] bytes);

        /// <summary>
        ///     Converts the bytes to the platform endian type.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>The bytes converted to the platform endian type.</returns>
        protected abstract byte[] ToPlatformEndian(byte[] bytes);
    }
}