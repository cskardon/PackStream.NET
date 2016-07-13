namespace PackStream.NET.Packers
{
    using System;

    public static partial class Packers
    {
        public static class Bool
        {
            public static bool Is(byte[] content)
            {
                return content[0] == Markers.True || content[0] == Markers.False;
            }

            public static byte[] Pack(bool content)
            {
                return content ? new[] {Markers.True} : new[] {Markers.False};
            }

            public static bool Unpack(byte[] content)
            {
                if (content[0] == Markers.True)
                    return true;
                if (content[0] == Markers.False)
                    return false;

                throw new ArgumentOutOfRangeException(nameof(content), content[0], "Marker Unknown");
            }
        }
    }
}