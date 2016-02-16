namespace PackStream.NET.Packers
{
    using System;

    public interface IPacker
    {
        bool CanPack(Type type);
        byte[] Pack(object content);
        object Unpack(byte[] content);
    }
}