using System;

namespace XMMUI
{
    public partial class OneXMMUI
    {
        #region Menu Callbacks

        private void autoManageDecimalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoManageDecimalsToolStripMenuItem.Checked = !autoManageDecimalsToolStripMenuItem.Checked;
            _auto_manage_decimals = autoManageDecimalsToolStripMenuItem.Checked;
        }

        private void hardhatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckBlockchainMenuItem(sender as ToolStripMenuItem, selectBlockchainToolStripMenuItem.DropDownItems);
            _selectedBlockchain = OneXMM.Blockchains.HardHat;
        }

        private void ethereumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckBlockchainMenuItem(sender as ToolStripMenuItem, selectBlockchainToolStripMenuItem.DropDownItems);
            _selectedBlockchain = OneXMM.Blockchains.Ethereum;
        }

        #endregion

        private static void CheckBlockchainMenuItem(ToolStripMenuItem sender, ToolStripItemCollection blockchainItems)
        {
            foreach (ToolStripMenuItem item in blockchainItems)
                if (item.Name.Equals(sender.Name)) sender.Checked = true;
                else item.Checked = false;
        }
    }
}
