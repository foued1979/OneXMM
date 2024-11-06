using System;
using Nethereum.ABI.Model;

namespace XMMUI
{
    using OneXMM;
    using OneXMM.Attributes;
    using OneXMM.Factories;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;
    using System.Runtime.CompilerServices;
    using XMMUI.Content;

    public partial class PoolTab : TabPage
    {
        private readonly Pair _pair;
        private readonly string _symbol;
        private readonly Dictionary<string, Control> _controls;

        private PoolMaturities _selectedMaturity;

        private Dictionary<PoolMaturities, IRequestManager<PoolFunctions>> _requestManagers = new Dictionary<PoolMaturities, IRequestManager<PoolFunctions>>();
        private SortedDictionary<string, Parameter[]> _poolFunctionParameters;
        private SortedDictionary<string, Parameter[]> _poolFunctionOutputs;

        public PoolTab(Pair pair)
        {
            _pair = pair;
            _controls = new Dictionary<string, Control>(10);

            InitializeComponent();

            _symbol = pair.Symbol.Replace('/', '_');
            Name = _symbol + "_tab";
            Text = pair.Symbol;

            SuspendLayout();

            SetDesign(_symbol);

            PerformLayout();

            Set();
        }

        private void SetDesign(string symbol)
        {
            var defaultFontFamily = new FontFamily("Segoe UI");

            Controls.Add(CreateFunctionList(symbol));
            Controls.Add(CreateKYCCheckBox(symbol));
            Controls.Add(CreateMaturityCombo(symbol, defaultFontFamily));
            Controls.Add(CreateFunctionDescriptionGroup(symbol, defaultFontFamily));
            Controls.Add(CreateParamDataGridView(symbol));
            Controls.Add(CreateResultDataGridView(symbol));
            Controls.Add(CreateSendButton(symbol));
        }

        private void Set()
        {
            _poolFunctionParameters = new SortedDictionary<string, Parameter[]>();
            _poolFunctionOutputs = new SortedDictionary<string, Parameter[]>();
            FillInMaturityCombo((_controls[$"{_symbol}MaturityBox"] as ComboBox)!, new FontFamily("Segoe UI"));

            var kycStatus = (_controls[$"{_symbol}KYCCheckBox"] as CheckBox)!.Checked ? KycStatus.KYC : KycStatus.NonKYC;
            var functions = ContractFactory.GetPoolContractFunctions(OneXMMUI._selectedBlockchain, kycStatus);

            if (functions == null || functions.Length == 0)
            {
                _controls[$"{_symbol}FunctionList"].Enabled = false;
                return;
            }

            _controls[$"{_symbol}FunctionList"].Enabled = true;

            foreach (var function in functions)
            {
                var cleanName = CleanSignatureName(function.Signature);
                _poolFunctionParameters.Add(cleanName, function.InputParameters);
                if (function.Constant) _poolFunctionOutputs.Add(cleanName, function.OutputParameters);
            }
        }

        #region Design Methods

        private ListBox CreateFunctionList(string symbol)
        {
            var functionList = new ListBox()
            {
                Name = $"{symbol}FunctionList",
                Location = new Point(6, 54),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                Size = new Size(175, 331),
            };

            foreach (var function in Enum.GetValues<PoolFunctions>()) functionList.Items.Add(function);

            functionList.SelectedIndexChanged += FunctionList_SelectedIndexChanged;

            _controls.Add($"{symbol}FunctionList", functionList);

            return functionList;
        }

        private CheckBox CreateKYCCheckBox(string symbol)
        {
            var checkBox = new CheckBox()
            {
                Name = $"{symbol}KYCCheckBox",
                Text = "Use KYC Pools",
                Location = new Point(6, 6),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
            };

            checkBox.CheckedChanged += CheckBox_CheckedChanged;

            _controls.Add($"{symbol}KYCCheckBox", checkBox);

            return checkBox;
        }

        private ComboBox CreateMaturityCombo(string symbol, FontFamily defaultFontFamily)
        {
            var comboBox = new ComboBox()
            {
                Name = $"{symbol}MaturityBox",
                Location = new Point(6, 30),
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;

            _controls.Add($"{symbol}MaturityBox", comboBox);

            return comboBox;
        }

        private void FillInMaturityCombo(ComboBox cb, FontFamily defaultFontFamily)
        {
            cb.Items.Clear();

            cb.Font = new Font(defaultFontFamily, 9f, FontStyle.Italic);
            cb.Text = "Select Tenor";
            cb.ForeColor = Color.Gray;

            var maturities = Config.GetPoolMaturities(_pair, (_controls[$"{_symbol}KYCCheckBox"] as CheckBox)!.Checked ? KycStatus.KYC : KycStatus.NonKYC);

            foreach (var matu in maturities)
            {
                cb.Items.Add(matu);
            }
        }

        private GroupBox CreateFunctionDescriptionGroup(string symbol, FontFamily defaultFontFamily)
        {
            var functionDescrGroup = new GroupBox()
            {
                Name = $"{symbol}FunctionDescrGroup",
                Text = "Function Description",
                Location = new Point(187, 6),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(819, 145),
                Font = new Font(defaultFontFamily, 9f, FontStyle.Bold),
                BackColor = Color.Linen
            };

            functionDescrGroup.SuspendLayout();

            var funcDescLbl = new Label()
            {
                Name = $"{symbol}FuncDescrLbl",
                Location = new Point(6, 19),
                Text = "-",
                AutoSize = true,
                Font = new Font(defaultFontFamily, 9f, FontStyle.Regular)
            };

            _controls.Add($"{symbol}FuncDescrLbl", funcDescLbl);

            var inputParamLbl = new Label()
            {
                Name = $"{symbol}Label2",
                Location = new Point(6, 63),
                Size = new Size(104, 15),
                Text = "Input Parameters"
            };

            var inputFlowLayout = new FlowLayoutPanel()
            {
                Name = $"{symbol}InputFlowLayout",
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Location = new Point(6, 81),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font(defaultFontFamily, 9f, FontStyle.Regular),
                AutoSize = true,
            };

            _controls.Add($"{symbol}InputFlowLayout", inputFlowLayout);

            var outputParamLbl = new Label()
            {
                Name = $"{symbol}Label3",
                Location = new Point(419, 63),
                Text = "Output Parameters",
                Size = new Size(114, 15),
            };

            var outputFlowLayout = new FlowLayoutPanel()
            {
                Name = $"{symbol}OutputFlowLayout",
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Location = new Point(419, 81),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font(defaultFontFamily, 9f, FontStyle.Regular),
                AutoSize= true,
            };

            _controls.Add($"{symbol}OutputFlowLayout", outputFlowLayout);

            functionDescrGroup.Controls.Add(funcDescLbl);
            functionDescrGroup.Controls.Add(inputParamLbl);
            functionDescrGroup.Controls.Add(inputFlowLayout);
            functionDescrGroup.Controls.Add(outputParamLbl);
            functionDescrGroup.Controls.Add(outputFlowLayout);

            functionDescrGroup.ResumeLayout();

            return functionDescrGroup;
        }

        private DataGridView CreateParamDataGridView(string symbol)
        {
            var grid = new DataGridView()
            {
                Name = $"{symbol}ParamGrid",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToOrderColumns = false,
                Location = new Point(187, 157),
                Size = new Size(410, 193)
            };

            _controls.Add($"{symbol}ParamGrid", grid);

            var nameCol = new DataGridViewColumn()
            {
                Name = $"{symbol}ParamNameCol",
                HeaderText = "Name",
                ReadOnly = true,
                CellTemplate = new DataGridViewTextBoxCell() { MaxInputLength = 45 },
                Width = 125
            };

            var inputCol = new DataGridViewColumn()
            {
                Name = $"{symbol}ParamInputCol",
                HeaderText = "Value",
                ReadOnly = false,
                CellTemplate = new DataGridViewTextBoxCell() { MaxInputLength = 6000 },
                Width = 225
            };

            grid.Columns.Add(nameCol);
            grid.Columns.Add(inputCol);

            return grid;
        }

        private DataGridView CreateResultDataGridView(string symbol)
        {
            var grid = new DataGridView()
            {
                Name = $"{symbol}ResultGrid",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToOrderColumns = false,
                Location = new Point(606, 157),
                Size = new Size(400, 193)
            };

            _controls.Add($"{symbol}ResultGrid", grid);

            var nameCol = new DataGridViewColumn()
            {
                Name = $"{symbol}ResultNameCol",
                HeaderText = "Name",
                ReadOnly = true,
                CellTemplate = new DataGridViewTextBoxCell() { MaxInputLength = 45 },
                Width = 125
            };

            var inputCol = new DataGridViewColumn()
            {
                Name = $"{symbol}ResultValueCol",
                HeaderText = "Value",
                ReadOnly = false,
                CellTemplate = new DataGridViewTextBoxCell() { MaxInputLength = 6000 },
                Width = 230
            };

            grid.Columns.Add(nameCol);
            grid.Columns.Add(inputCol);

            return grid;
        }

        public Button CreateSendButton(string symbol)
        {
            var btn = new Button()
            {
                Name = $"{symbol}SendBtn",
                Text = "Send",
                Location = new Point(187, 356),
                Size = new Size(115, 29),
                BackColor = Color.YellowGreen
            };

            btn.Click += Btn_Click;

            return btn;
        }

        #endregion

        #region Callbacks

        private void FunctionList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var tokenFunctionList = (sender as ListBox)!;

            _controls[$"{_symbol}FuncDescrLbl"].Text = "-";
            _controls[$"{_symbol}InputFlowLayout"].Controls.Clear();
            _controls[$"{_symbol}OutputFlowLayout"].Controls.Clear();
            (_controls[$"{_symbol}ParamGrid"] as DataGridView)!.Rows.Clear();
            (_controls[$"{_symbol}ResultGrid"] as DataGridView)!.Rows.Clear();

            if (tokenFunctionList.SelectedIndex == -1) { return; }

            var selectedFunction = tokenFunctionList.SelectedItem!.ToString()!;
            // We check if the function involves a rate to be adjusted
            var function = selectedFunction.ToEnum<TokenFunctions>();
            var attr = function.GetAttributesForEnum<TokenFunctions, ContractFunctionAttribute>();

            _controls[$"{_symbol}FuncDescrLbl"].Text = FunctionDescriptions.PoolDescriptions[selectedFunction];

            var inputParameters = _poolFunctionParameters[selectedFunction];

            foreach (var parameter in inputParameters)
            {
                if (attr is not null && attr.Any(a => a.ParametersName.Equals(parameter.Name))) _controls[$"{_symbol}InputFlowLayout"].Controls.Add(new ParameterDescription(parameter.Type, parameter.Name, attr.First(a => a.ParametersName.Equals(parameter.Name))));
                else _controls[$"{_symbol}InputFlowLayout"].Controls.Add(new ParameterDescription(parameter.Type, parameter.Name, null));
                (_controls[$"{_symbol}ParamGrid"] as DataGridView)!.Rows.Add([parameter.Name]);
            }

            if (_poolFunctionOutputs.TryGetValue(selectedFunction, out var outputParameters))
            {
                var count = 1;

                foreach (var parameter in outputParameters)
                {
                    _controls[$"{_symbol}OutputFlowLayout"].Controls.Add(new ParameterDescription(parameter.Type, parameter.Name, null));
                    (_controls[$"{_symbol}ResultGrid"] as DataGridView)!.Rows.Add([(string.IsNullOrEmpty(parameter.Name) ? $"output{count++}" : parameter.Name)]);
                }
            }
            else
            {
                (_controls[$"{_symbol}ResultGrid"] as DataGridView)!.Rows.Add(["Tx Hash"]);
            }
        }

        private void CheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            Set();
        }

        private void Btn_Click(object? sender, EventArgs e)
        {
            var res = MessageBox.Show($"Send transaction to {_symbol} on {OneXMMUI._selectedBlockchain}?", "Tx Confirmation", MessageBoxButtons.OKCancel);

            if (res == DialogResult.OK) { throw new NotImplementedException(); }
        }

        private void ComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var cb = (sender as ComboBox)!;
            if (cb.SelectedIndex == -1) return;

            if (cb.ForeColor != Color.Black)
            {
                cb.Font = new Font(new FontFamily("Segoe UI"), 9f, FontStyle.Regular);
                cb.ForeColor = Color.Black;
            }

            _selectedMaturity = (PoolMaturities)cb.SelectedItem!;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string CleanSignatureName(string signature)
        {
            return signature.Substring(0, signature.IndexOf('('));
        }
    }
}
