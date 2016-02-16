namespace PackStream.NET.Packers
{
    public static partial class Packers
    {
        private static IBitConverter BitConverter => new BigEndianTargetBitConverter();
    }
}