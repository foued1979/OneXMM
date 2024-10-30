using OneXMM;
using OneXMM.Factories;
using XMMUI.Configuration;

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

        
    }
}
