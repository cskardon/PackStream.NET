namespace PackStream.NET.Packers
{
    using System;

    public static partial class Packers
    {
        public static class Bool
        {
            public static bool Is(byte[] content)
            {
                return content[0] == (byte) Markers.True || content[0] == (byte) Markers.False;
            }

            public static byte[] Pack(bool content)
            {
                return content ? new[] {(byte) Markers.True} : new[] {(byte) Markers.False};
            }

            public static bool Unpack(byte[] content)
            {
                if (content[0] == (byte) Markers.True)
                    return true;
                if (content[0] == (byte) Markers.False)
                    return false;

                throw new ArgumentOutOfRangeException(nameof(content), content[0], "Marker Unknown");
            }
        }
    }
}