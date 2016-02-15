namespace PackStream.NET.Packers
{
    public enum Markers
    {
        Null = 0xC0,
        True = 0xC3,
        False = 0xC2,
        FloatingPoint = 0xC1,
    }
}