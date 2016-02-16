namespace PackStream.NET.Packers
{
    public static class Markers
    {
        public const byte Null = 0xC0;
        public const byte Floating = 0xC1;
        public const byte False = 0xC2;
        public const byte True = 0xC3;

        public const byte Int8 = 0xC8;
        public const byte Int16 = 0xC9;
        public const byte Int32 = 0xCA;
        public const byte Int64 = 0xCB;

        public const byte Text8 = 0xD0;
        public const byte Text16 = 0xD1;
        public const byte Text32 = 0xD2;

        public const byte List8 = 0xD4;
        public const byte List16 = 0xD5;
        public const byte List32 = 0xD6;
        /// <summary>
        /// Runs until 0xDF marker is hit.
        /// </summary>
        public const byte ListUnlimited = 0xD7;

        public const byte Map8 = 0xD8;
        public const byte Map16 = 0xD9;
        public const byte Map32 = 0xDA;
        /// <summary>
        /// Runs until 0xDF marker is hit.
        /// </summary>
        public const byte MapUnlimited = 0xDB;

        public const byte Struct8 = 0xDC;
        public const byte Struct16 = 0xDD;
    }
}