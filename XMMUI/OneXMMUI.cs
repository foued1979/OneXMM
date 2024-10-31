using Nethereum.ABI.FunctionEncoding;
using Nethereum.RPC.Eth.DTOs;
using OneXMM;
using OneXMM.Attributes;
using OneXMM.Factories;
using OneXMM.Utils;
using XMMUI.Configuration;
using XMMUI.Content;

namespace XMMUI
{
    public partial class OneXMMUI : Form
    {
        #region Settings

        private static Settings _settings;
        private static Blockchains _selectedBlockchain;

        private static bool _auto_manage_decimals = true;

        #endregion


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
            tokenInputFlowLayoutPanel.Controls.Clear();
            tokenOutputFlowLayoutPanel.Controls.Clear();
            tokenParameterGrid.Rows.Clear();
            tokenResultsGrid.Rows.Clear();

            if (tokenFunctionList.SelectedIndex == -1) { return; }

            var selectedFunction = tokenFunctionList.SelectedItem!.ToString()!;
            // We check if the function involves a rate to be adjusted
            var function = selectedFunction.ToEnum<TokenFunctions>();
            var attr = function.GetAttributesForEnum<TokenFunctions, ContractFunctionAttribute>();

            tokenFunctionDescrLbl.Text = FunctionDescriptions.Descriptions[selectedFunction];

            var inputParameters = _tokenFunctionParameters[selectedFunction];

            foreach (var parameter in inputParameters)
            {
                if (attr is not null && attr.Any(a => a.ParametersName.Equals(parameter.Name))) tokenInputFlowLayoutPanel.Controls.Add(new ParameterDescription(parameter.Type, parameter.Name, attr.First(a => a.ParametersName.Equals(parameter.Name))));
                else tokenInputFlowLayoutPanel.Controls.Add(new ParameterDescription(parameter.Type, parameter.Name, null));
                tokenParameterGrid.Rows.Add([parameter.Name]);
            }

            if (_tokenFunctionOutputs.TryGetValue(selectedFunction, out var outputParameters))
            {
                var count = 1;

                foreach (var parameter in outputParameters)
                {
                    tokenOutputFlowLayoutPanel.Controls.Add(new ParameterDescription(parameter.Type, parameter.Name, null));
                    tokenResultsGrid.Rows.Add([(string.IsNullOrEmpty(parameter.Name) ? $"output{count++}" : parameter.Name)]);
                }
            }
            else
            {
                tokenResultsGrid.Rows.Add(["Tx Hash"]);
            }
        }

        private void tokenSendBtn_Click(object sender, EventArgs e)
        {
            if (tokenFunctionList.SelectedIndex == -1) { return; }

            var selectedFunction = tokenFunctionList.SelectedItem!.ToString()!;
            
            // We check if the function involves a rate to be adjusted
            var function = selectedFunction.ToEnum<TokenFunctions>();

            var parameters = new object[tokenParameterGrid.Rows.Count];

            for (int i = 0; i < parameters.Length; ++i)
            {
                parameters[i] = tokenParameterGrid.Rows[i].Cells[1].ToBlockchainObject((tokenInputFlowLayoutPanel.Controls[i] as ParameterDescription)!, _auto_manage_decimals);
            }

            var res = _tokenRequestManager.ExecuteTask(selectedFunction.ToEnum<TokenFunctions>(), parameters);

            if (_tokenFunctionOutputs.TryGetValue(selectedFunction, out var outputs))
            {
                for (int i = 0; i < outputs.Length; ++i)
                {
                    tokenResultsGrid.Rows[i].Cells[1].Value = (res[i] as ParameterOutput).Result;
                }
            }
            else
            {
                var txResult = res as TransactionReceipt;
                tokenResultsGrid.Rows[0].Cells[1].Value = txResult.TransactionHash;
            }
        }
    }
}
