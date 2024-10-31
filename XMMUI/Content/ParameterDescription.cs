using System;

namespace XMMUI.Content
{
    using OneXMM.Attributes;

    public partial class ParameterDescription : UserControl
    {
        public string ParameterType { get; }
        public uint? Precision { get; } = null;

        public ParameterDescription(string type, string name, ContractFunctionAttribute? fieldAttribute)
        {
            InitializeComponent();
            var fieldAttrIsNull = fieldAttribute is null;

            paramNameLbl.Text = name;
            ParameterType = fieldAttrIsNull ? type : "double";
            paramTypeLbl.Text = fieldAttrIsNull ? type : "double";
            if (!fieldAttrIsNull) Precision = fieldAttribute!.Precision;
        }
    }
}
