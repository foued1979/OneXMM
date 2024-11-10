namespace XMMUI
{
    partial class OneXMMUI
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mainMenu = new MenuStrip();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            selectBlockchainToolStripMenuItem = new ToolStripMenuItem();
            ethereumToolStripMenuItem = new ToolStripMenuItem();
            hardhatToolStripMenuItem = new ToolStripMenuItem();
            autoManageDecimalsToolStripMenuItem = new ToolStripMenuItem();
            contractsControl = new TabControl();
            tokenTab = new TabPage();
            tokenResultsGrid = new DataGridView();
            outputNameCol = new DataGridViewTextBoxColumn();
            outputValueCol = new DataGridViewTextBoxColumn();
            tokenSendBtn = new Button();
            tokenFunctionDescrGroup = new GroupBox();
            label1 = new Label();
            tokenOutputFlowLayoutPanel = new FlowLayoutPanel();
            tokenInputFlowLayoutPanel = new FlowLayoutPanel();
            label2 = new Label();
            tokenFunctionDescrLbl = new Label();
            tokenParameterGrid = new DataGridView();
            tokenParamNameCol = new DataGridViewTextBoxColumn();
            tokenParamValueCol = new DataGridViewTextBoxColumn();
            tokenFunctionList = new ListBox();
            mainMenu.SuspendLayout();
            contractsControl.SuspendLayout();
            tokenTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tokenResultsGrid).BeginInit();
            tokenFunctionDescrGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tokenParameterGrid).BeginInit();
            SuspendLayout();
            // 
            // mainMenu
            // 
            mainMenu.Items.AddRange(new ToolStripItem[] { settingsToolStripMenuItem });
            mainMenu.Location = new Point(0, 0);
            mainMenu.Name = "mainMenu";
            mainMenu.Size = new Size(1044, 24);
            mainMenu.TabIndex = 0;
            mainMenu.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { selectBlockchainToolStripMenuItem, autoManageDecimalsToolStripMenuItem });
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(61, 20);
            settingsToolStripMenuItem.Text = "Settings";
            // 
            // selectBlockchainToolStripMenuItem
            // 
            selectBlockchainToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { ethereumToolStripMenuItem, hardhatToolStripMenuItem });
            selectBlockchainToolStripMenuItem.Name = "selectBlockchainToolStripMenuItem";
            selectBlockchainToolStripMenuItem.Size = new Size(199, 22);
            selectBlockchainToolStripMenuItem.Text = "Select Blockchain";
            // 
            // ethereumToolStripMenuItem
            // 
            ethereumToolStripMenuItem.Name = "ethereumToolStripMenuItem";
            ethereumToolStripMenuItem.Size = new Size(125, 22);
            ethereumToolStripMenuItem.Text = "Ethereum";
            ethereumToolStripMenuItem.Click += ethereumToolStripMenuItem_Click;
            // 
            // hardhatToolStripMenuItem
            // 
            hardhatToolStripMenuItem.Name = "hardhatToolStripMenuItem";
            hardhatToolStripMenuItem.Size = new Size(125, 22);
            hardhatToolStripMenuItem.Text = "Hardhat";
            hardhatToolStripMenuItem.Click += hardhatToolStripMenuItem_Click;
            // 
            // autoManageDecimalsToolStripMenuItem
            // 
            autoManageDecimalsToolStripMenuItem.Checked = true;
            autoManageDecimalsToolStripMenuItem.CheckState = CheckState.Checked;
            autoManageDecimalsToolStripMenuItem.Name = "autoManageDecimalsToolStripMenuItem";
            autoManageDecimalsToolStripMenuItem.Size = new Size(199, 22);
            autoManageDecimalsToolStripMenuItem.Text = "Auto-Manage Decimals";
            autoManageDecimalsToolStripMenuItem.Click += autoManageDecimalsToolStripMenuItem_Click;
            // 
            // contractsControl
            // 
            contractsControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            contractsControl.Controls.Add(tokenTab);
            contractsControl.Location = new Point(12, 27);
            contractsControl.Name = "contractsControl";
            contractsControl.SelectedIndex = 0;
            contractsControl.Size = new Size(1020, 437);
            contractsControl.TabIndex = 1;
            // 
            // tokenTab
            // 
            tokenTab.Controls.Add(tokenResultsGrid);
            tokenTab.Controls.Add(tokenSendBtn);
            tokenTab.Controls.Add(tokenFunctionDescrGroup);
            tokenTab.Controls.Add(tokenParameterGrid);
            tokenTab.Controls.Add(tokenFunctionList);
            tokenTab.Location = new Point(4, 24);
            tokenTab.Name = "tokenTab";
            tokenTab.Padding = new Padding(3);
            tokenTab.Size = new Size(1012, 409);
            tokenTab.TabIndex = 0;
            tokenTab.Text = "Token";
            tokenTab.UseVisualStyleBackColor = true;
            // 
            // tokenResultsGrid
            // 
            tokenResultsGrid.AllowUserToAddRows = false;
            tokenResultsGrid.AllowUserToDeleteRows = false;
            tokenResultsGrid.AllowUserToResizeRows = false;
            tokenResultsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            tokenResultsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            tokenResultsGrid.Columns.AddRange(new DataGridViewColumn[] { outputNameCol, outputValueCol });
            tokenResultsGrid.Location = new Point(581, 157);
            tokenResultsGrid.Name = "tokenResultsGrid";
            tokenResultsGrid.Size = new Size(425, 193);
            tokenResultsGrid.TabIndex = 4;
            // 
            // outputNameCol
            // 
            outputNameCol.HeaderText = "Name";
            outputNameCol.MaxInputLength = 40;
            outputNameCol.Name = "outputNameCol";
            outputNameCol.ReadOnly = true;
            outputNameCol.Width = 125;
            // 
            // outputValueCol
            // 
            outputValueCol.HeaderText = "Value";
            outputValueCol.MaxInputLength = 6000;
            outputValueCol.Name = "outputValueCol";
            outputValueCol.ReadOnly = true;
            outputValueCol.Width = 225;
            // 
            // tokenSendBtn
            // 
            tokenSendBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            tokenSendBtn.BackColor = Color.YellowGreen;
            tokenSendBtn.Location = new Point(162, 356);
            tokenSendBtn.Name = "tokenSendBtn";
            tokenSendBtn.Size = new Size(115, 29);
            tokenSendBtn.TabIndex = 3;
            tokenSendBtn.Text = "Send";
            tokenSendBtn.UseVisualStyleBackColor = false;
            tokenSendBtn.Click += tokenSendBtn_Click;
            // 
            // tokenFunctionDescrGroup
            // 
            tokenFunctionDescrGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tokenFunctionDescrGroup.BackColor = Color.Linen;
            tokenFunctionDescrGroup.Controls.Add(label1);
            tokenFunctionDescrGroup.Controls.Add(tokenOutputFlowLayoutPanel);
            tokenFunctionDescrGroup.Controls.Add(tokenInputFlowLayoutPanel);
            tokenFunctionDescrGroup.Controls.Add(label2);
            tokenFunctionDescrGroup.Controls.Add(tokenFunctionDescrLbl);
            tokenFunctionDescrGroup.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tokenFunctionDescrGroup.Location = new Point(162, 6);
            tokenFunctionDescrGroup.Name = "tokenFunctionDescrGroup";
            tokenFunctionDescrGroup.Size = new Size(844, 145);
            tokenFunctionDescrGroup.TabIndex = 2;
            tokenFunctionDescrGroup.TabStop = false;
            tokenFunctionDescrGroup.Text = "Function Description";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(419, 63);
            label1.Name = "label1";
            label1.Size = new Size(114, 15);
            label1.TabIndex = 4;
            label1.Text = "Output Parameters";
            // 
            // tokenOutputFlowLayoutPanel
            // 
            tokenOutputFlowLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tokenOutputFlowLayoutPanel.Location = new Point(419, 81);
            tokenOutputFlowLayoutPanel.Name = "tokenOutputFlowLayoutPanel";
            tokenOutputFlowLayoutPanel.Size = new Size(419, 58);
            tokenOutputFlowLayoutPanel.TabIndex = 3;
            // 
            // tokenInputFlowLayoutPanel
            // 
            tokenInputFlowLayoutPanel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tokenInputFlowLayoutPanel.Location = new Point(6, 81);
            tokenInputFlowLayoutPanel.Name = "tokenInputFlowLayoutPanel";
            tokenInputFlowLayoutPanel.Size = new Size(407, 58);
            tokenInputFlowLayoutPanel.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(6, 63);
            label2.Name = "label2";
            label2.Size = new Size(104, 15);
            label2.TabIndex = 1;
            label2.Text = "Input Parameters";
            // 
            // tokenFunctionDescrLbl
            // 
            tokenFunctionDescrLbl.AutoSize = true;
            tokenFunctionDescrLbl.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tokenFunctionDescrLbl.Location = new Point(6, 19);
            tokenFunctionDescrLbl.Name = "tokenFunctionDescrLbl";
            tokenFunctionDescrLbl.Size = new Size(12, 15);
            tokenFunctionDescrLbl.TabIndex = 0;
            tokenFunctionDescrLbl.Text = "-";
            // 
            // tokenParameterGrid
            // 
            tokenParameterGrid.AllowUserToAddRows = false;
            tokenParameterGrid.AllowUserToDeleteRows = false;
            tokenParameterGrid.AllowUserToResizeRows = false;
            tokenParameterGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            tokenParameterGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            tokenParameterGrid.Columns.AddRange(new DataGridViewColumn[] { tokenParamNameCol, tokenParamValueCol });
            tokenParameterGrid.Location = new Point(162, 157);
            tokenParameterGrid.Name = "tokenParameterGrid";
            tokenParameterGrid.Size = new Size(410, 193);
            tokenParameterGrid.TabIndex = 1;
            // 
            // tokenParamNameCol
            // 
            tokenParamNameCol.HeaderText = "Name";
            tokenParamNameCol.MaxInputLength = 40;
            tokenParamNameCol.Name = "tokenParamNameCol";
            tokenParamNameCol.ReadOnly = true;
            tokenParamNameCol.Width = 125;
            // 
            // tokenParamValueCol
            // 
            tokenParamValueCol.HeaderText = "Value";
            tokenParamValueCol.MaxInputLength = 6000;
            tokenParamValueCol.Name = "tokenParamValueCol";
            tokenParamValueCol.Width = 225;
            // 
            // tokenFunctionList
            // 
            tokenFunctionList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            tokenFunctionList.FormattingEnabled = true;
            tokenFunctionList.ItemHeight = 15;
            tokenFunctionList.Location = new Point(6, 6);
            tokenFunctionList.Name = "tokenFunctionList";
            tokenFunctionList.Size = new Size(150, 379);
            tokenFunctionList.TabIndex = 0;
            tokenFunctionList.SelectedIndexChanged += tokenFunctionList_SelectedIndexChanged;
            // 
            // OneXMMUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1044, 476);
            Controls.Add(contractsControl);
            Controls.Add(mainMenu);
            MainMenuStrip = mainMenu;
            MinimumSize = new Size(1060, 515);
            Name = "OneXMMUI";
            Text = "1XMM UI";
            mainMenu.ResumeLayout(false);
            mainMenu.PerformLayout();
            contractsControl.ResumeLayout(false);
            tokenTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)tokenResultsGrid).EndInit();
            tokenFunctionDescrGroup.ResumeLayout(false);
            tokenFunctionDescrGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tokenParameterGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip mainMenu;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem selectBlockchainToolStripMenuItem;
        private ToolStripMenuItem ethereumToolStripMenuItem;
        private ToolStripMenuItem hardhatToolStripMenuItem;
        private TabControl contractsControl;
        private TabPage tokenTab;
        private ListBox tokenFunctionList;
        private GroupBox tokenFunctionDescrGroup;
        private DataGridView tokenParameterGrid;
        private FlowLayoutPanel tokenInputFlowLayoutPanel;
        private Label label2;
        private Label tokenFunctionDescrLbl;
        private Button tokenSendBtn;
        private DataGridViewTextBoxColumn tokenParamNameCol;
        private DataGridViewTextBoxColumn tokenParamValueCol;
        private ToolStripMenuItem autoManageDecimalsToolStripMenuItem;
        private FlowLayoutPanel tokenOutputFlowLayoutPanel;
        private Label label1;
        private DataGridView tokenResultsGrid;
        private DataGridViewTextBoxColumn outputNameCol;
        private DataGridViewTextBoxColumn outputValueCol;
    }
}
