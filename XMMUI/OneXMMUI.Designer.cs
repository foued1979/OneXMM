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
            contractsControl = new TabControl();
            tokenTab = new TabPage();
            tabPage2 = new TabPage();
            tokenFunctionList = new ListBox();
            mainMenu.SuspendLayout();
            contractsControl.SuspendLayout();
            tokenTab.SuspendLayout();
            SuspendLayout();
            // 
            // mainMenu
            // 
            mainMenu.Items.AddRange(new ToolStripItem[] { settingsToolStripMenuItem });
            mainMenu.Location = new Point(0, 0);
            mainMenu.Name = "mainMenu";
            mainMenu.Size = new Size(800, 24);
            mainMenu.TabIndex = 0;
            mainMenu.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { selectBlockchainToolStripMenuItem });
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(61, 20);
            settingsToolStripMenuItem.Text = "Settings";
            // 
            // selectBlockchainToolStripMenuItem
            // 
            selectBlockchainToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { ethereumToolStripMenuItem, hardhatToolStripMenuItem });
            selectBlockchainToolStripMenuItem.Name = "selectBlockchainToolStripMenuItem";
            selectBlockchainToolStripMenuItem.Size = new Size(180, 22);
            selectBlockchainToolStripMenuItem.Text = "Select Blockchain";
            // 
            // ethereumToolStripMenuItem
            // 
            ethereumToolStripMenuItem.Name = "ethereumToolStripMenuItem";
            ethereumToolStripMenuItem.Size = new Size(180, 22);
            ethereumToolStripMenuItem.Text = "Ethereum";
            ethereumToolStripMenuItem.Click += ethereumToolStripMenuItem_Click;
            // 
            // hardhatToolStripMenuItem
            // 
            hardhatToolStripMenuItem.Name = "hardhatToolStripMenuItem";
            hardhatToolStripMenuItem.Size = new Size(180, 22);
            hardhatToolStripMenuItem.Text = "Hardhat";
            hardhatToolStripMenuItem.Click += hardhatToolStripMenuItem_Click;
            // 
            // contractsControl
            // 
            contractsControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            contractsControl.Controls.Add(tokenTab);
            contractsControl.Controls.Add(tabPage2);
            contractsControl.Location = new Point(12, 27);
            contractsControl.Name = "contractsControl";
            contractsControl.SelectedIndex = 0;
            contractsControl.Size = new Size(776, 411);
            contractsControl.TabIndex = 1;
            // 
            // tokenTab
            // 
            tokenTab.Controls.Add(tokenFunctionList);
            tokenTab.Location = new Point(4, 24);
            tokenTab.Name = "tokenTab";
            tokenTab.Padding = new Padding(3);
            tokenTab.Size = new Size(768, 383);
            tokenTab.TabIndex = 0;
            tokenTab.Text = "Token";
            tokenTab.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(192, 72);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // tokenFunctionList
            // 
            tokenFunctionList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            tokenFunctionList.FormattingEnabled = true;
            tokenFunctionList.ItemHeight = 15;
            tokenFunctionList.Location = new Point(6, 6);
            tokenFunctionList.Name = "tokenFunctionList";
            tokenFunctionList.Size = new Size(125, 364);
            tokenFunctionList.TabIndex = 0;
            // 
            // OneXMMUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(contractsControl);
            Controls.Add(mainMenu);
            MainMenuStrip = mainMenu;
            Name = "OneXMMUI";
            Text = "1XMM UI";
            mainMenu.ResumeLayout(false);
            mainMenu.PerformLayout();
            contractsControl.ResumeLayout(false);
            tokenTab.ResumeLayout(false);
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
        private TabPage tabPage2;
        private ListBox tokenFunctionList;
    }
}
