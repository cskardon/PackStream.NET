namespace PackStream.NET.Packers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::PackStream;

    public class Struct
    {
        public byte[] OriginalBytes { get; }
        private int _numberOfFields;
        public Struct() { }
        public Struct(byte[] originalBytes)
        {
            OriginalBytes = originalBytes;
            SignatureByte = (SignatureBytes) originalBytes[1];
        }

        public int NumberOfFields
        {
            get { return _numberOfFields; }
            set
            {
                if (OriginalBytes == null)
                    throw new InvalidOperationException("No fields can be set without original bytes!");
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Number of fields must be a positive number.");

                _numberOfFields = value;
                if (NumberOfFields <= 15)
                    ContentWithoutStructAndSignature = OriginalBytes.Skip(2).ToArray();
                else if (NumberOfFields >= 16 && NumberOfFields <= 255)
                    ContentWithoutStructAndSignature = OriginalBytes.Skip(3).ToArray();
                else if (NumberOfFields >= 256 && NumberOfFields <= 65535)
                    ContentWithoutStructAndSignature = OriginalBytes.Skip(4).ToArray();
            }
        }

        public byte[] ContentWithoutStructAndSignature { get; private set; }
        public SignatureBytes SignatureByte { get; set; }

        public Node<T> GetNode<T>() where T : new()
        {
            if(SignatureByte != SignatureBytes.Node)
                throw new InvalidOperationException("The data is not a node.");

            return new Node<T>(ContentWithoutStructAndSignature);
        }


        private static IDictionary<string, IEnumerable<string>> GetMetaData(Struct s) 
        {
            if (s.NumberOfFields == 0)
                return null;

            //Get actual data
            var response = PackStream.Unpack<Dictionary<string, IEnumerable<string>>>(s.ContentWithoutStructAndSignature);
            return response;
        }

        public override string ToString()
        {
            if (OriginalBytes != null && OriginalBytes.Length >= 0)
                return BitConverter.ToString(OriginalBytes);
            return "No original bytes to convert";
        }
    }
}