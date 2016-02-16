namespace PackStream.NET
{
    using System;

    /// <summary>Instructs the NDP Client not to serialize the public field or public read/write property value.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class PackStreamIgnoreAttribute : Attribute { }
}