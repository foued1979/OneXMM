using System;
using System.Text;

namespace OneXMM.Resources.RawHardhatArtifact
{
    internal sealed class HardhatArtifact
    {
        public string contractName { get; set; }
        public string sourceName { get; set; }
        public AbiDesc[] abi { get; set; } = new AbiDesc[0];
        public string contractAddress { get; set; }
        public string deployedBytecode { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("[");

            foreach (var abidesc in abi)
            {
                stringBuilder.Append(abidesc.ToString() + ",");
            }

            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }
    }

    internal sealed class AbiDesc
    {
        public IOType[] inputs { get; set; } = new IOType[0];
        public IOType[] outputs { get; set; } = new IOType[0];
        public string name { get; set; }
        public bool anonymous { get; set; }
        public string stateMutability { get; set; }
        public string type { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(name)) stringBuilder.Append($"{{\"stateMutability\":\"{stateMutability}\",\"type\":\"{type}\",\"anonymous\":{(anonymous ? "true" : "false")},");
            else stringBuilder.Append($"{{\"name\":\"{name}\",\"stateMutability\":\"{stateMutability}\",\"type\":\"{type}\",\"anonymous\":{(anonymous ? "true" : "false")},");
            stringBuilder.Append($"\"inputs\":[");

            foreach (var input in inputs)
            {
                stringBuilder.Append(input.ToString());
            }

            stringBuilder.Append($"],\"outputs\":[");

            foreach (var output in outputs)
            {
                stringBuilder.Append(output.ToString());
            }

            stringBuilder.Append($"],}}");
            return stringBuilder.ToString();
        }
    }

    internal sealed class IOType
    {
        public bool? indexed { get; set; }
        public Components[]? components { get; set; }
        public string internalType { get; set; }
        public string name { get; set; }
        public string type { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            if (indexed.HasValue) stringBuilder.Append($"{{\"indexed\":{(indexed.Value ? "true" : "false")},");
            else stringBuilder.Append("{");

            if (components is not null)
            {
                stringBuilder.Append($"\"components\":[");
                foreach (var cmp in components) stringBuilder.Append(cmp.ToString());
                stringBuilder.Append($"],");
            }
            stringBuilder.Append($"\"name\":\"{name}\",\"internalType\":\"{internalType}\",\"type\":\"{type}\"}},");
            return stringBuilder.ToString();
        }
    }

    internal sealed class Components
    {
        public string internalType { get; set; }
        public string name { get; set; }
        public string type { get; set; }

        public override string ToString()
        {
            return $"{{\"internalType\":\"{internalType}\",\"name\":\"{name}\",\"type\":\"{type}\"}},";
        }
    }
}
