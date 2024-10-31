using System;

namespace OneXMM.Attributes
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ContractFunctionAttribute : Attribute
    {
        public string ParametersName { get; } = "";
        public uint Precision { get; } = 0;

        public ContractFunctionAttribute(string parametersName, uint precision)
        {
            ParametersName = parametersName;
            Precision = precision;
        }
    }
}
