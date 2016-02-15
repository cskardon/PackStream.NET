namespace PackStream.NET.Packers
{
    public interface IPacker<T>
    {
        byte[] Pack(T content);
        T Unpack(byte[] content);
    }
}