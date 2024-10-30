using System;

namespace XMMUI
{
    public partial class OneXMMUI
    {
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

        private static void CheckBlockchainMenuItem(ToolStripMenuItem sender, ToolStripItemCollection blockchainItems)
        {
            foreach (ToolStripMenuItem item in blockchainItems)
                if (item.Name.Equals(sender.Name)) sender.Checked = true;
                else item.Checked = false;
        }
    }
}
