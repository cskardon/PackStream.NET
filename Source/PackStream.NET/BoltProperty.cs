namespace PackStream.NET
{
    using System;

    /// <summary>Instructs the Bolt Client on serialization specifics</summary>
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
    public sealed class BoltPropertyAttribute : Attribute
    {
        /// <summary>The name to use for serialization / deserialization.</summary>
        public string Name { get; }
        
        public BoltPropertyAttribute(string name = null)
        {
            Name = name;
        }
    }
}