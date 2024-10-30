using System;

namespace XMMUI.Content
{
    public partial class ParameterDescription : UserControl
    {
        public string ParameterType { get; }

        public ParameterDescription(string type, string name)
        {
            InitializeComponent();

            paramNameLbl.Text = name;
            ParameterType = type;
            paramTypeLbl.Text = type;
        }
    }
}
