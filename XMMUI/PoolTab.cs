using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XMMUI
{
    using OneXMM;
    using OneXMM.Referential.Assets;

    public partial class PoolTab : TabPage
    {
        private readonly Pair _pair;

        public PoolTab(Pair pair)
        {
            _pair = pair;

            InitializeComponent();

            var symbol = pair.Symbol.Replace('/', '_');
            Name = symbol + "_tab";
            Text = pair.Symbol;

            SuspendLayout();

            SetDesign(symbol);

            PerformLayout();
        }

        private void SetDesign(string symbol)
        {
            var defaultFontFamily = new FontFamily("Segoe UI");

            Controls.Add(CreateFunctionList(symbol));
            Controls.Add(CreateFunctionDescriptionGroup(symbol, defaultFontFamily));
            Controls.Add(CreateParamDataGridView(symbol));
            Controls.Add(CreateResultDataGridView(symbol));
            Controls.Add(CreateSendButton(symbol));
        }

        #region Design Methods

        private ListBox CreateFunctionList(string symbol)
        {
            var functionList = new ListBox()
            {
                Name = $"{symbol}FunctionList",
                Location = new Point(6, 6),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                Size = new Size(175, 379),
            };

            foreach (var function in Enum.GetValues<PoolFunctions>()) functionList.Items.Add(function);

            functionList.SelectedIndexChanged += FunctionList_SelectedIndexChanged;

            return functionList;
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
                Font = new Font(defaultFontFamily, 9f, FontStyle.Regular)
            };

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
                Location = new Point(6, 81),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font(defaultFontFamily, 9f, FontStyle.Regular)
            };

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
                Location = new Point(419, 81),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font(defaultFontFamily, 9f, FontStyle.Regular)
            };

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
            throw new NotImplementedException();
        }

        private void Btn_Click(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
