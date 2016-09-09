namespace PackStream.NET
{
    public enum SignatureBytes 
    {
        /// <summary>INIT &lt;user_agent&gt;</summary>
        Init = 0x01,
        AckFailure = 0x0F,

        /// <summary>RUN &lt;statement&gt; &lt;parameters&gt;</summary>
        Run = 0x10,
        Discard = 0x2F,
        PullAll = 0x3F,

        /// <summary>SUCCESS &lt;metadata&gt;</summary>
        Success = 0x70,

        /// <summary>RECORD &lt;value&gt;</summary>
        Record = 0x71,

        /// <summary>IGNORED &lt;metadata&gt;</summary>
        Ignored = 0x7E,

        /// <summary>FAILURE &lt;metadata&gt;</summary>
        Failure = 0x7F,

        /// <summary>Represents 'N'</summary>
        Node = 0x4E,

        /// <summary>Represents 'R'</summary>
        Relationship = 0x52,

        /// <summary>Represents 'P'</summary>
        Path = 0x50
    }
}