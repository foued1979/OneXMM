using OneXMM;
using OneXMM.Factories;
using OneXMM.Utils;
using XMMUI.Configuration;
using XMMUI.Content;

namespace XMMUI
{
    public partial class OneXMMUI : Form
    {
        private static Settings _settings;
        private static Blockchains _selectedBlockchain;

        public OneXMMUI()
        {
            var pathToSettings = Path.Combine(AppContext.BaseDirectory, "Configuration\\Settings.json");
            _settings = OneXMM.Utils.Utils.ConvertFileToObject<Settings>(pathToSettings)!;

            InitializeComponent();

            ContractFactory.SetContractFactory(_settings.IsTestMode);
            Set();
        }

        private void tokenFunctionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            tokenFunctionDescrLbl.Text = "-";
            tokenFlowLayoutPanel.Controls.Clear();
            tokenParameterGrid.Rows.Clear();

            if (tokenFunctionList.SelectedIndex == -1) {  return; }

            var selectedFunction = tokenFunctionList.SelectedItem!.ToString()!;
            tokenFunctionDescrLbl.Text = FunctionDescriptions.Descriptions[selectedFunction];

            var parameters = _tokenFunctions[selectedFunction];

            foreach (var parameter in parameters)
            {
                tokenFlowLayoutPanel.Controls.Add(new ParameterDescription(parameter.Type, parameter.Name));
                tokenParameterGrid.Rows.Add([parameter.Name]);
            }
        }

        private void tokenSendBtn_Click(object sender, EventArgs e)
        {
            if (tokenFunctionList.SelectedIndex == -1) { return; }

            var selectedFunction = tokenFunctionList.SelectedItem!.ToString()!;
            var parameters = new object[tokenParameterGrid.Rows.Count];

            for (int i = 0; i < parameters.Length; ++i)
            {

                parameters[i] = tokenParameterGrid.Rows[i].Cells[1].ToBlockchainObject((tokenFlowLayoutPanel.Controls[i] as ParameterDescription)!.ParameterType);
            }

            _tokenRequestManager.ExecuteTask(selectedFunction.ToEnum<TokenFunctions>(), parameters);
        }
    }
}
